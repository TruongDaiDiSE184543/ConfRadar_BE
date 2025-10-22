using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Destination;
using ConfRadar.Services.DTOs.Room;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IDestinationService
    {
        Task<string> CreateDestinationAsync(CreateDestinationRequest request);
        Task<int> UpdateDestinationAsync(UpdateDestinationRequest request, string destinationId);
        Task<int> DeleteDestinationAsync(string destinationId);
        Task<DestinationResponse> GetDestinationByIdAsync(string destinationId);
        Task<List<DestinationResponse>> GetAllDestinationsAsync();
        Task<DestinationWithRoomsResponse> GetDestinationWithRoomsAsync(string destinationId);
    }

    public class DestinationService : IDestinationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DestinationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateDestinationAsync(CreateDestinationRequest request)
        {
            var destination = new Destination
            {
                DestinationId = Guid.NewGuid().ToString(),
                Name = request.Name,
                City = request.City,
                District = request.District,
                Street = request.Street
            };

            var result = await _unitOfWork.DestinationRepository.CreateDestinationAsync(destination);
            if (result <= 0)
            {
                throw new BadRequestException("Failed to create destination");
            }

            return destination.DestinationId;
        }

        public async Task<int> UpdateDestinationAsync(UpdateDestinationRequest request, string destinationId)
        {
            var existingDestination = await _unitOfWork.DestinationRepository.GetDestinationByIdAsync(destinationId);
            if (existingDestination == null)
            {
                throw new NotFoundException($"Destination with ID {destinationId} not found");
            }

            existingDestination.Name = request.Name ?? existingDestination.Name;
            existingDestination.City = request.City ?? existingDestination.City;
            existingDestination.District = request.District ?? existingDestination.District;
            existingDestination.Street = request.Street ?? existingDestination.Street;

            var result = await _unitOfWork.DestinationRepository.UpdateDestinationAsync(existingDestination);
            return result;
        }

        public async Task<int> DeleteDestinationAsync(string destinationId)
        {
            var destination = await _unitOfWork.DestinationRepository.GetDestinationByIdAsync(destinationId);
            if (destination == null)
            {
                throw new NotFoundException($"Destination with ID {destinationId} not found");
            }

            var result = await _unitOfWork.DestinationRepository.DeleteDestinationAsync(destination);
            return result;
        }

        public async Task<DestinationResponse> GetDestinationByIdAsync(string destinationId)
        {
            var destination = await _unitOfWork.DestinationRepository.GetDestinationByIdAsync(destinationId);
            if (destination == null)
            {
                throw new NotFoundException($"Destination with ID {destinationId} not found");
            }

            var response = new DestinationResponse
            {
                DestinationId = destination.DestinationId,
                Name = destination.Name,
                City = destination.City,
                District = destination.District,
                Street = destination.Street
            };

            return response;
        }

        public async Task<List<DestinationResponse>> GetAllDestinationsAsync()
        {
            var destinations = await _unitOfWork.DestinationRepository.GetAllDestinationsAsync();
            var responses = destinations.Select(destination => new DestinationResponse
            {
                DestinationId = destination.DestinationId,
                Name = destination.Name,
                City = destination.City,
                District = destination.District,
                Street = destination.Street
            }).ToList();

            return responses;
        }

        public async Task<DestinationWithRoomsResponse> GetDestinationWithRoomsAsync(string destinationId)
        {
            var destination = await _unitOfWork.DestinationRepository.GetDestinationByIdAsync(destinationId);
            if (destination == null)
            {
                throw new NotFoundException($"Destination with ID {destinationId} not found");
            }

            var rooms = await _unitOfWork.RoomRepository.GetRoomsByDestinationIdAsync(destinationId);
            var roomResponses = rooms.Select(room => new RoomResponse
            {
                RoomId = room.RoomId,
                Number = room.Number,
                DisplayName = room.DisplayName,
                DestinationId = room.DestinationId
            }).ToList();

            var response = new DestinationWithRoomsResponse
            {
                DestinationId = destination.DestinationId,
                Name = destination.Name,
                City = destination.City,
                District = destination.District,
                Street = destination.Street,
                Rooms = roomResponses
            };

            return response;
        }
    }
}