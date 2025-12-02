using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IConferenceRepository
    {
        Task<int> CreateConferenceAsync(Conference conference);
        Task<int> UpdateConferenceAsync(Conference conference);
        Task<int> DeleteConferenceAsync(Conference conference);
        Task<Conference?> GetConferenceByIdAsync(string conferenceId);
        Task<List<Conference>> GetAllConferencesAsync();
        IQueryable<Conference> GetAllConferences();
        Task<Conference?> GetConferenceWithDetailsAsync(string conferenceId);
        IQueryable<Conference> GetAllTechnicalIncludedConference();
        Task<Conference> GetTechnicalIncludedById(string technicalId);
        Task<Conference> GetResearchIncludedById(string researchId);
        IQueryable<Conference> GetAllResearchIncludedConference();

        Task<Dictionary<string, Conference>> GetConferencesByIdsAsync(List<string> conferenceIds);
        Task<List<Conference>> GetConferencesByUserIdAndStatusAsync(string userId, string? statusId);
        Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId, DateOnly dateNow, string conferenceStatusReadyId);
        //Task<List<Conference>> GetConferencesByUserId(string userId);
        Task<List<string>> GetTechnicalConferenceOrResearchConferenceIdsByUserId(string userId, bool isResearchConference);
        Task<List<Conference>> GetConferenceByStatus(ConferenceStatus conferenceStaus);
        Task<int> UpdateMutipleConferenceAsync(List<Conference> conferences);
    }

    public class ConferenceRepository : GenericRepository<Conference>, IConferenceRepository
    {
        public ConferenceRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateConferenceAsync(Conference conference)
        {
            return await CreateAsync(conference);
        }

        public async Task<int> UpdateConferenceAsync(Conference conference)
        {
            return await UpdateAsync(conference);
        }
        public async Task<int> UpdateMutipleConferenceAsync(List<Conference> conferences)
        {
            _context.Conferences.UpdateRange(conferences);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> DeleteConferenceAsync(Conference conference)
        {
            _context.Conferences.Remove(conference);
            return await _context.SaveChangesAsync();
        }

        public async Task<Conference?> GetConferenceByIdAsync(string conferenceId)
        {
            return await _context.Conferences
                .Include(c => c.ConferenceStatus)
                .Include(c => c.CreatedByNavigation)
                .Include(c => c.ResearchConferencePhases)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
        }

        public async Task<List<Conference>> GetAllConferencesAsync()
        {
            return await _context.Conferences.ToListAsync();
        }
        public IQueryable<Conference> GetAllConferences()
        {
            return _context.Conferences.Include(c => c.City).Include(c => c.ConferenceStatus).AsNoTracking(); ;
        }

        public async Task<Conference?> GetConferenceWithDetailsAsync(string conferenceId)
        {
            return await _context.Conferences
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room)
                        .ThenInclude(r => r.Destination)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Speakers)
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                //.Include(c => c.FavouriteConferences)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
        }

        public async Task<Dictionary<string, Conference>> GetConferencesByIdsAsync(List<string> conferenceIds)
        {
            var conferences = await _context.Conferences
                .Where(c => conferenceIds.Contains(c.ConferenceId))
                .ToListAsync();

            return conferences.ToDictionary(c => c.ConferenceId);
        }

        public async Task<List<Conference>> GetConferencesByUserIdAndStatusAsync(string userId, string? statusId)
        {
            var query = _context.Conferences
        .Include(c => c.CollaboratorContract)
        .Include(c => c.ConferenceStatus)    // Lấy tên trạng thái
        .Include(c => c.ConferenceCategory)  // Lấy tên loại (Type)
        .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.CreatedBy == userId);
            }

            if (!string.IsNullOrEmpty(statusId))
            {
                query = query.Where(c => c.ConferenceStatusId == statusId);
            }

            // Sắp xếp giảm dần theo ngày tạo (tùy chọn)
            query = query.OrderByDescending(c => c.CreatedAt);

            return await query.ToListAsync();
        }

        public async Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId, DateOnly dateNow, string conferenceStatusReadyId)
        {
            var conferenceList = await _context.Tickets
                                 .AsNoTracking()
                                 .Where(t => t.UserId == userId
                                 && t.IsRefunded == false
                                 && t.PricePhase != null
                                 && t.PricePhase.ConferencePrice != null
                                 && t.PricePhase.ConferencePrice.Conference != null
                                 && t.PricePhase.ConferencePrice.Conference.StartDate > dateNow
                                 && t.PricePhase.ConferencePrice.Conference.ConferenceStatusId == conferenceStatusReadyId
                                 )
                                 .Select(t => new ConferenceDetailForScheduleResponse()
                                 {
                                     ConferenceId = t.PricePhase.ConferencePrice.Conference.ConferenceId,
                                     ConferenceName = t.PricePhase.ConferencePrice.Conference.ConferenceName,
                                     Description = t.PricePhase.ConferencePrice.Conference.Description,
                                     StartDate = t.PricePhase.ConferencePrice.Conference.StartDate,
                                     EndDate = t.PricePhase.ConferencePrice.Conference.EndDate,
                                     TotalSlot = t.PricePhase.ConferencePrice.Conference.TotalSlot,
                                     AvailableSlot = t.PricePhase.ConferencePrice.Conference.AvailableSlot,
                                     Address = t.PricePhase.ConferencePrice.Conference.Address,
                                     BannerImageUrl = t.PricePhase.ConferencePrice.Conference.BannerImageUrl,
                                     CreatedAt = t.PricePhase.ConferencePrice.Conference.CreatedAt,
                                     TicketSaleStart = t.PricePhase.ConferencePrice.Conference.TicketSaleStart,
                                     TicketSaleEnd = t.PricePhase.ConferencePrice.Conference.TicketSaleEnd,
                                     IsInternalHosted = t.PricePhase.ConferencePrice.Conference.IsInternalHosted,
                                     IsResearchConference = t.PricePhase.ConferencePrice.Conference.IsResearchConference,
                                     CityId = t.PricePhase.ConferencePrice.Conference.CityId,
                                     CityName = t.PricePhase.ConferencePrice.Conference.City != null ? t.PricePhase.ConferencePrice.Conference.City.CityName : null,
                                     ConferenceCategoryId = t.PricePhase.ConferencePrice.Conference.ConferenceCategoryId,
                                     ConferenceCategoryName = t.PricePhase.ConferencePrice.Conference.ConferenceCategory != null ? t.PricePhase.ConferencePrice.Conference.ConferenceCategory.ConferenceCategoryName : null,
                                     ConferenceStatusId = t.PricePhase.ConferencePrice.Conference.ConferenceStatusId,
                                     ConferenceStatusName = t.PricePhase.ConferencePrice.Conference.ConferenceStatus != null ? t.PricePhase.ConferencePrice.Conference.ConferenceStatus.ConferenceStatusName : null,
                                     Sessions = t.PricePhase.ConferencePrice.Conference.ConferenceSessions.Select(cs => new SessionDetailForScheduleResponse()
                                     {
                                         ConferenceSessionId = cs.ConferenceSessionId,
                                         Title = cs.Title,
                                         Description = cs.Description,
                                         StartTime = cs.StartTime,
                                         EndTime = cs.EndTime,
                                         SessionDate = cs.SessionDate,
                                         ConferenceId = cs.ConferenceId,
                                         RoomId = cs.RoomId,
                                         RoomNumber = cs.Room != null ? cs.Room.Number : null,
                                         RoomDisplayName = cs.Room != null ? cs.Room.DisplayName : null,
                                         DestinationId = cs.Room != null ? cs.Room.DestinationId : null,
                                         DestinationName = cs.Room != null && cs.Room.Destination != null ? cs.Room.Destination.Name : null,
                                         DestinationDistrict = cs.Room != null && cs.Room.Destination != null ? cs.Room.Destination.District : null,
                                         DestinationStreet = cs.Room != null && cs.Room.Destination != null ? cs.Room.Destination.Street : null,
                                         CityId = cs.Room != null && cs.Room.Destination != null ? cs.Room.Destination.CityId : null,
                                         CityName = cs.Room != null && cs.Room.Destination != null && cs.Room.Destination.City != null ? cs.Room.Destination.City.CityName : null,
                                         PresenterAuthor = cs.PresentAuthors.Select(pa => new PresenterAuthorDetailForScheduleResponse()
                                         {
                                             ConferenceSessionId = cs.ConferenceSessionId,
                                             PaperId = pa.PaperId,
                                             AssignedAt = pa.AssignedAt,

                                             ConferenceId = t.PricePhase.ConferencePrice.ConferenceId,
                                             PaperPhaseId = pa.Paper.PaperPhaseId,
                                             PaperPhaseName = pa.Paper.PaperPhase != null ? pa.Paper.PaperPhase.PhaseName : null,
                                             CreatedAt = pa.Paper.CreatedAt,
                                             PaperTitle = pa.Paper.Title,
                                             PaperDescription = pa.Paper.Description,
                                             ResearchConferencePhaseId = pa.Paper.ResearchConferencePhaseId,

                                             PaperAuthor = pa.Paper.PaperAuthors
                                             .Where(pau =>
                                             _context.Tickets.Any(t => t.UserId == pau.UserId
                                             && t.IsRefunded == false
                                             && t.PricePhase != null && t.PricePhase.ConferencePrice != null
                                             && t.PricePhase.ConferencePrice.IsAuthor == true
                                             && t.PricePhase.ConferencePrice.ConferenceId == pa.Paper.ConferenceId))
                                             .Select(pau => new PaperAuthorDetailForScheduleResponse()
                                             {
                                                 UserId = pau.UserId,
                                                 FullName = pau.User.FullName,
                                                 AvatarUrl = pau.User.AvatarUrl,
                                                 PaperId = pau.PaperId,
                                                 IsPresenter = pau.IsPresenter,
                                                 IsRootAuthor = pau.IsRootAuthor,
                                             }).ToList(),
                                         }).ToList(),
                                     }).ToList(),
                                 })
                                 .AsSplitQuery()
                                 .ToListAsync();
            return conferenceList;
        }
        public async Task<List<string>> GetTechnicalConferenceOrResearchConferenceIdsByUserId(string userId, bool isResearchConference)
        {
            var conferences = await _context.Conferences
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId && c.IsResearchConference == isResearchConference)
                .Select(c => c.ConferenceId)
                .ToListAsync();
            return conferences;
        }


        public IQueryable<Conference> GetAllTechnicalIncludedConference()
        {
            return _context.Conferences
                    .Include(c => c.CollaboratorContract)
                    .Include(c => c.CreatedByNavigation)
                        .ThenInclude(u => u.Organization)
                    .Include(c => c.ConferenceCategory)
                    .Include(c => c.ConferenceMedia)
                    .Include(c => c.Policies)
                    .Include(c => c.ConferencePrices)
                        .ThenInclude(cp => cp.PricePhases)
                            .ThenInclude(pp => pp.RefundPolicies)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.Speakers)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.ConferenceSessionMedia)
                    .Include(c => c.ConferenceSessions)
                        .ThenInclude(cs => cs.Room) // Include room information
                            .ThenInclude(r => r.Destination)
                                .ThenInclude(d => d.City)
                    .Include(c => c.Sponsors)
                    .Include(c => c.TechnicalConferenceDetail)
                .AsNoTracking()
                .AsSplitQuery();
        }

        public async Task<Conference> GetTechnicalIncludedById(string technicalId)
        {
            return await _context.Conferences
                .Include(c => c.CreatedByNavigation)
                    .ThenInclude(u => u.Organization)
                .Include(c => c.CollaboratorContract)
                .Include(c => c.ConferenceCategory)
                .Include(c => c.ConferenceMedia)
                .Include(c => c.Policies)
                .Include(c => c.ConferencePrices)
                    .ThenInclude(cp => cp.PricePhases)
                        .ThenInclude(pp => pp.RefundPolicies)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Speakers)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.ConferenceSessionMedia)
                .Include(c => c.ConferenceSessions)
                    .ThenInclude(cs => cs.Room) // Include room information
                         .ThenInclude(r => r.Destination)
                            .ThenInclude(d => d.City)
                .Include(c => c.Sponsors)
                .Include(c => c.TechnicalConferenceDetail)
                .Include(c => c.ConferenceTimelines) // Include timeline
                    .ThenInclude(ct => ct.PreviousStatus)
                .Include(c => c.ConferenceTimelines)
                    .ThenInclude(ct => ct.AfterwardStatus)
                .Include(c => c.RefundPolicies)
                .AsNoTracking()
                .AsSplitQuery()
               .FirstOrDefaultAsync(c => c.ConferenceId == technicalId);
        }

        public Task<Conference> GetResearchIncludedById(string researchId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Conference> GetAllResearchIncludedConference()
        {
            throw new NotImplementedException();
        }

        //public Task<List<Conference>> GetConferencesByUserId(string userId)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<List<Conference>> GetConferenceByStatus(ConferenceStatus conferenceStaus)
        {
            return await _context.Conferences
                .Where(c => c.ConferenceStatusId == conferenceStaus.ConferenceStatusId)
                .ToListAsync();
        }
    }
}