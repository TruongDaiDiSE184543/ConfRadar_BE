using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.Conference;
using Microsoft.EntityFrameworkCore;

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
        Task<Dictionary<string, Conference>> GetConferencesByIdsAsync(List<string> conferenceIds);
        Task<List<Conference>> GetConferencesByUserIdAndStatusAsync(string userId, string statusId);
        Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId, DateOnly dateNow, string conferenceStatusReadyId);
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

        public async Task<int> DeleteConferenceAsync(Conference conference)
        {
            _context.Conferences.Remove(conference);
            return await _context.SaveChangesAsync();
        }

        public async Task<Conference?> GetConferenceByIdAsync(string conferenceId)
        {
            return await _context.Conferences
                .Include(c => c.ResearchConferencePhases)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
        }

        public async Task<List<Conference>> GetAllConferencesAsync()
        {
            return await _context.Conferences.ToListAsync();
        }
        public IQueryable<Conference> GetAllConferences()
        {
            return _context.Conferences.AsNoTracking(); ;
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
            var query = _context.Conferences.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.CreatedBy == userId);
            }

            if (!string.IsNullOrEmpty(statusId))
            {
                query = query.Where(c => c.ConferenceStatusId == statusId);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ConferenceDetailForScheduleResponse>> GetListConferencesForScheduleByUserId(string userId, DateOnly dateNow, string conferenceStatusReadyId)
        {
            var conferenceList = await _context.Tickets
                                 .AsNoTracking()
                                 .Where(t => t.UserId == userId
                                 && t.PricePhase != null
                                 && t.PricePhase.ConferencePrice != null
                                 && t.PricePhase.ConferencePrice.Conference != null
                                 && t.PricePhase.ConferencePrice.Conference.StartDate > dateNow
                                 && t.PricePhase.ConferencePrice.Conference.ConferenceStatusId == conferenceStatusReadyId
                                 )
                                 .Select(t => new ConferenceDetailForScheduleResponse()
                                 {
                                     //ConferenceId = t.PricePhase.ConferencePrice.Conference.ConferenceId,
                                     //ConferenceName = t.ConferencePrice.Conference.ConferenceName,
                                     //Description = t.ConferencePrice.Conference.Description,
                                     //StartDate = t.ConferencePrice.Conference.StartDate,
                                     //EndDate = t.ConferencePrice.Conference.EndDate,
                                     //TotalSlot = t.ConferencePrice.Conference.TotalSlot,
                                     //AvailableSlot = t.ConferencePrice.Conference.AvailableSlot,
                                     //Address = t.ConferencePrice.Conference.Address,
                                     //BannerImageUrl = t.ConferencePrice.Conference.BannerImageUrl,
                                     //CreatedAt = t.ConferencePrice.Conference.CreatedAt,
                                     //TicketSaleStart = t.ConferencePrice.Conference.TicketSaleStart,
                                     //TicketSaleEnd = t.ConferencePrice.Conference.TicketSaleEnd,
                                     //IsInternalHosted = t.ConferencePrice.Conference.IsInternalHosted,
                                     //IsResearchConference = t.ConferencePrice.Conference.IsResearchConference,
                                     //CityId = t.ConferencePrice.Conference.CityId,
                                     //CityName = t.ConferencePrice.Conference.City != null ? t.ConferencePrice.Conference.City.CityName : null,
                                     //ConferenceCategoryId = t.ConferencePrice.Conference.ConferenceCategoryId,
                                     //ConferenceCategoryName = t.ConferencePrice.Conference.ConferenceCategory != null ? t.ConferencePrice.Conference.ConferenceCategory.ConferenceCategoryName : null,
                                     //ConferenceStatusId = t.ConferencePrice.Conference.ConferenceStatusId,
                                     //ConferenceStatusName = t.ConferencePrice.Conference.ConferenceStatus != null ? t.ConferencePrice.Conference.ConferenceStatus.ConferenceStatusName : null,
                                     Sessions = t.PricePhase.ConferencePrice.Conference.ConferenceSessions.Any() ? t.PricePhase.ConferencePrice.Conference.ConferenceSessions.Select(cs => new SessionDetailForScheduleResponse()
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
                                     }).ToList() : new List<SessionDetailForScheduleResponse>()
                                 })
                                 .AsSplitQuery()
                                 .ToListAsync();
            return conferenceList;
        }
    }
}