using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.FavouriteConference;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IFavouriteConferenceRepository
    {
        Task<List<FavouriteConferenceDetailResponse>> GetByUserIdAsync(string userId);
        Task<FavouriteConference?> GetByUserAndConferenceIdAsync(string userId, string conferenceId);
        Task<int> AddFavouriteAsync(FavouriteConference favouriteConference);
        Task<bool> DeleteFavouriteAsync(FavouriteConference favouriteConference);
        Task<bool> ExistsFavouriteAsync(string userId, string conferenceId);
    }
    public class FavouriteConferenceRepository : GenericRepository<FavouriteConference>, IFavouriteConferenceRepository
    {
        public FavouriteConferenceRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<List<FavouriteConferenceDetailResponse>> GetByUserIdAsync(string userId)
        {
            var listFavourite = await _context.FavouriteConferences.AsNoTracking().Where(fc => fc.UserId == userId).OrderByDescending(fc => fc.CreatedAt)
                               .Select(fc => new FavouriteConferenceDetailResponse()
                               {
                                   ConferenceId = fc.ConferenceId,
                                   FavouriteCreatedAt = fc.CreatedAt,
                                   ConferenceName = fc.Conference != null ? fc.Conference.ConferenceName : null,
                                   ConferenceDescription = fc.Conference != null ? fc.Conference.Description : null,
                                   BannerImageUrl = fc.Conference != null ? fc.Conference.BannerImageUrl : null,
                                   ConferenceStartDate = fc.Conference != null ? fc.Conference.StartDate : null,
                                   ConferenceEndDate = fc.Conference != null ? fc.Conference.EndDate : null,
                                   TicketSaleStart = fc.Conference != null ? fc.Conference.TicketSaleStart : null,
                                   TicketSaleEnd = fc.Conference != null ? fc.Conference.TicketSaleEnd : null,
                                   IsInternalHosted = fc.Conference != null ? fc.Conference.IsInternalHosted : null,
                                   IsResearchConference = fc.Conference != null ? fc.Conference.IsResearchConference : null,
                                   AvailableSlot = fc.Conference != null ? fc.Conference.AvailableSlot : null,
                               }).ToListAsync();
            return listFavourite;
        }

        public async Task<FavouriteConference?> GetByUserAndConferenceIdAsync(string userId, string conferenceId)
        {
            return await _context.FavouriteConferences.FirstOrDefaultAsync(fc => fc.UserId == userId && fc.ConferenceId == conferenceId);
        }

        public async Task<int> AddFavouriteAsync(FavouriteConference favouriteConference)
        {
            return await CreateAsync(favouriteConference);
        }

        public async Task<bool> DeleteFavouriteAsync(FavouriteConference favouriteConference)
        {
            return await RemoveAsync(favouriteConference);
        }

        public async Task<bool> ExistsFavouriteAsync(string userId, string conferenceId)
        {
            return await _context.FavouriteConferences.AnyAsync(fc => fc.UserId == userId && fc.ConferenceId == conferenceId);
        }
    }
}
