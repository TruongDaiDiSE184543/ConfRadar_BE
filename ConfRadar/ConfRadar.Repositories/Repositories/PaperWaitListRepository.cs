using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.User;
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
        Task<List<NotifyUserWaitListDetailResponse>> NotifyWaitListAsync(string readyConfereceStatusId, string pendingWaitListStatusId, string notifiedAtWaitListStatusId, DateTime notifiedAt);
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

        public async Task<List<NotifyUserWaitListDetailResponse>> NotifyWaitListAsync(string readyConfereceStatusId, string pendingWaitListStatusId, string notifiedAtWaitListStatusId, DateTime notifiedAt)
        {

            var notifyUserList = new List<NotifyUserWaitListDetailResponse>();
            var activeConferenceIds = await _context.Conferences
                 //.Include(c => c.ResearchConferencePhases)
                 //.Include(c => c.ConferencePrices)
                 .Where(c => c.ResearchConferencePhases.Any(rcp => rcp.IsWaitlist == true && rcp.IsActive == true)
                        && c.ConferencePrices.Any(cp => cp.AvailableSlot > 0 && cp.IsAuthor == true)
                        && c.ConferenceStatusId == readyConfereceStatusId).Select(c => c.ConferenceId).ToListAsync();
            var finalResult = 0;
            if (activeConferenceIds.Any())
            {
                var paperWaitListUser = await _context.PaperWaitLists
                    .Include(pwl => pwl.User)
                    .Where
                    (pwl => pwl.ConferenceId!=null 
                    && activeConferenceIds.Contains(pwl.ConferenceId) 
                    && pwl.WaitListStatusId == pendingWaitListStatusId 
                    && pwl.NotifiedAt == null 
                    && pwl.UserId!=null 
                    && pwl.User!=null 
                    && pwl.User.IsActive==true).ToListAsync();
                if (paperWaitListUser.Any())
                {
                    var listPaperWaitList = new List<PaperWaitList>();
                   
                    foreach (var user in paperWaitListUser)
                    {
                        user.WaitListStatusId = notifiedAtWaitListStatusId;
                        user.NotifiedAt = notifiedAt;

                        listPaperWaitList.Add(user);
                        notifyUserList.Add(new NotifyUserWaitListDetailResponse()
                        {
                            Email = user.User?.Email,
                            UserId = user.UserId
                        });
                    }
                    if (listPaperWaitList.Any())
                    {
                        _context.PaperWaitLists.UpdateRange(listPaperWaitList);
                        finalResult = await _context.SaveChangesAsync();
                    }
                }


            }
            if (finalResult > 0 && notifyUserList.Count>0)
            {
                return notifyUserList;
            }
            notifyUserList.Clear();
            return notifyUserList;
        }

        public async Task<PaperWaitList?> GetPaperWaitListByUserIdAndConferenceIdAsync(string userId, string conferenceId)
        {
            return await _context.PaperWaitLists.FirstOrDefaultAsync(pwl => pwl.UserId == userId && pwl.ConferenceId == conferenceId);
        }
    }

}
