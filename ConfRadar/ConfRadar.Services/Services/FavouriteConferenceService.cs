using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.FavouriteConference;

namespace ConfRadar.Services.Services
{
    public interface IFavouriteConferenceService
    {
        Task<List<FavouriteConferenceDetailResponse>> GetFavouritesByUserIdAsync(string userId);
        Task<AddedFavouriteConfereceResponse> AddFavouriteAsync(string userId, string conferenceId);
        Task<DeletedFavouriteConfereceResponse> DeleteFavouriteAsync(string userId, string conferenceId);
    }

    public class FavouriteConferenceService : IFavouriteConferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        public FavouriteConferenceService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
        }

        public async Task<AddedFavouriteConfereceResponse> AddFavouriteAsync(string userId, string conferenceId)
        {
            var existingConference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (existingConference == null)
            {
                throw new BadRequestException($"Không tìm thấy sự kiện với mã {conferenceId}");
            }
            var existingFavouriteConference = await _unitOfWork.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(userId, conferenceId);
            if (existingFavouriteConference != null)
            {
                throw new BadRequestException("Bạn đã thích conference này rồi");
            }
            var favouriteConferenceObj = new FavouriteConference()
            {
                UserId = userId,
                ConferenceId = conferenceId,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
            };
            await _unitOfWork.FavoriteConferenceRepository.AddFavouriteAsync(favouriteConferenceObj);
            return new AddedFavouriteConfereceResponse()
            {
                ConferenceId = conferenceId,
                IsAdded = true
            };
        }

        public async Task<DeletedFavouriteConfereceResponse> DeleteFavouriteAsync(string userId, string conferenceId)
        {
            var existingConference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(conferenceId);
            if (existingConference == null)
            {
                throw new BadRequestException($"Không tìm thấy sự kiện với mã {conferenceId}");
            }
            var exisitingFavouriteConferece = await _unitOfWork.FavoriteConferenceRepository.GetByUserAndConferenceIdAsync(userId, conferenceId);
            if (exisitingFavouriteConferece == null)
            {
                throw new NotFoundException("Không tìm thấy conference để xóa");
            }
            var result = await _unitOfWork.FavoriteConferenceRepository.DeleteFavouriteAsync(exisitingFavouriteConferece);
            return new DeletedFavouriteConfereceResponse()
            {
                ConferenceId = conferenceId,
                IsDeleted = result,
            };
        }

        public Task<List<FavouriteConferenceDetailResponse>> GetFavouritesByUserIdAsync(string userId)
        {
            return _unitOfWork.FavoriteConferenceRepository.GetByUserIdAsync(userId);
        }
    }
}
