using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.WaitList;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperWaitListRepository
    {
        Task<int> CreatePaperWaitListAsync(PaperWaitList paperWaitList);
        Task<int> UpdatePaperWaitListAsync(PaperWaitList paperWaitList);
        Task<bool> DeletePaperWaitListAsync(PaperWaitList paperWaitList);
        Task<PaperWaitList?> GetPaperWaitListByIdAsync(string paperWaitListId);
        Task<PaperWaitList?> GetPaperWaitListByUserIdAndConferenceIdAsync(string userId, string conferenceId);
        Task<List<PaperWaitList>> GetAllPaperWaitListsAsync();
        Task<int> CreateMultiplePaperWaitListsAsync(List<PaperWaitList> paperWaitList);
        Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId);
        Task<string> NotifyNextInWaitListInAConferenceAsync(string conferenceId, string pendingWaitListStatusId, string notifiedAtWaitListStatusId, DateTime notifiedAt);
    }
    public class PaperWaitListRepository : GenericRepository<PaperWaitList>, IPaperWaitListRepository
    {
        public PaperWaitListRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreatePaperWaitListAsync(PaperWaitList entry)
        {
            return await CreateAsync(entry);
        }

        public async Task<int> UpdatePaperWaitListAsync(PaperWaitList entry)
        {
            return await UpdateAsync(entry);
        }

        public async Task<bool> DeletePaperWaitListAsync(PaperWaitList entry)
        {
            return await RemoveAsync(entry);
        }

        public async Task<PaperWaitList?> GetPaperWaitListByIdAsync(string paperWaitListId)
        {
            return await _context.PaperWaitLists.FirstOrDefaultAsync(x => x.PaperWaitListId == paperWaitListId);
        }
        public async Task<List<PaperWaitList>> GetAllPaperWaitListsAsync()
        {
            return await GetAllAsync();
        }
        public async Task<int> CreateMultiplePaperWaitListsAsync(List<PaperWaitList> entries)
        {
            await _context.PaperWaitLists.AddRangeAsync(entries);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<CustomerWaitListResponse>> GetCustomerWaitList(string userId)
        {
            var result = await _context.PaperWaitLists.AsNoTracking().Where(pwl => pwl.UserId == userId).OrderByDescending(pwl => pwl.NotifiedAt).ThenByDescending(pwl => pwl.CreatedAt)
                            .Select(pwl => new CustomerWaitListResponse()
                            {
                                PaperWaitListId = pwl.PaperWaitListId,
                                CreatedAt = pwl.CreatedAt,
                                NotifiedAt = pwl.NotifiedAt,
                                WaitListStatusId = pwl.WaitListStatusId,
                                WaitListStatusName = pwl.WaitListStatus != null ? pwl.WaitListStatus.Name : null,
                                ConferenceId = pwl.ConferenceId,
                                ConferenceName = pwl.Conference != null ? pwl.Conference.ConferenceName : null,
                                ConferenceDescription = pwl.Conference != null ? pwl.Conference.Description : null,
                                ConferenceStartDate = pwl.Conference != null ? pwl.Conference.StartDate : null,
                                ConferenceEndDate = pwl.Conference != null ? pwl.Conference.EndDate : null,
                                ConferenceAvailableSlot = pwl.Conference != null ? pwl.Conference.AvailableSlot : null,
                                ConferenceAddress = pwl.Conference != null ? pwl.Conference.Address : null,
                                ConferenceBannerImageUrl = pwl.Conference != null ? pwl.Conference.BannerImageUrl : null,
                                IsInternalHosted = pwl.Conference != null ? pwl.Conference.IsInternalHosted : null,
                                IsResearchConference = pwl.Conference != null ? pwl.Conference.IsResearchConference : null,
                                ConferenceCategoryId = pwl.Conference != null ? pwl.Conference.ConferenceCategoryId : null,
                                ConferenceCategoryName = pwl.Conference != null && pwl.Conference.ConferenceCategory != null ? pwl.Conference.ConferenceCategory.ConferenceCategoryName : null,
                                ConferenceStatusId = pwl.Conference != null ? pwl.Conference.ConferenceStatusId : null,
                                ConferenceStatusName = pwl.Conference != null && pwl.Conference.ConferenceStatus != null ? pwl.Conference.ConferenceStatus.ConferenceStatusName : null,
                            }).ToListAsync();
            return result;
        }

        public async Task<string> NotifyNextInWaitListInAConferenceAsync(string conferenceId,string pendingWaitListStatusId,string notifiedAtWaitListStatusId,DateTime notifiedAt)
        {
            var conference = await _context.Conferences.Include(c=>c.ResearchConferencePhases).FirstOrDefaultAsync(c=>c.ConferenceId == conferenceId);
            if (conference == null || conference.AvailableSlot <= 0)
            {
                return string.Empty;
            }
            var activeSecondWaitListPhase = conference.ResearchConferencePhases.FirstOrDefault(rcp=>rcp.IsWaitlist==true && rcp.IsActive==true);
            if (activeSecondWaitListPhase == null)
            {
                return string.Empty;
            }
            var pendingWaitListStatus = await _context.WaitListStatuses.FirstOrDefaultAsync(wls => wls.WaitListStatusId == pendingWaitListStatusId);
            var notifiedWaitListStatus = await _context.WaitListStatuses.FirstOrDefaultAsync(wls => wls.WaitListStatusId == notifiedAtWaitListStatusId);
            if (pendingWaitListStatus == null || notifiedWaitListStatus == null) 
            {
                return string.Empty;
            }
            var nextInLine = await _context.PaperWaitLists
                .Include(pwl=>pwl.User)
                .Where(pwl => pwl.ConferenceId == conferenceId && pwl.WaitListStatusId == pendingWaitListStatus.WaitListStatusId && pwl.NotifiedAt==null)
                .OrderBy(pwl => pwl.CreatedAt)
                .FirstOrDefaultAsync();
            var finalResult = 0;
            var email = string.Empty;
            if (nextInLine != null)
            {
                nextInLine.NotifiedAt = notifiedAt;
                nextInLine.WaitListStatusId = notifiedWaitListStatus.WaitListStatusId;
                //thêm gửi mail và in web/app noti sau
                finalResult =await _context.SaveChangesAsync();
                email = nextInLine.User?.Email ?? string.Empty;
            }
            if (finalResult > 0)
            {
                return email;
            }

            return email;
        }

        public async Task<PaperWaitList?> GetPaperWaitListByUserIdAndConferenceIdAsync(string userId, string conferenceId)
        {
            return await _context.PaperWaitLists.FirstOrDefaultAsync(pwl => pwl.UserId == userId && pwl.ConferenceId == conferenceId);
        }
    }

}
