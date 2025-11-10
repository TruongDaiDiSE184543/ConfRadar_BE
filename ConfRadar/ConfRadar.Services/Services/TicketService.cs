using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.Ticket;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;

namespace ConfRadar.Services.Services
{
    public interface ITicketService
    {
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId);
    }
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId)
        {
            var tickets = await _unitOfWork.TicketRepository.GetTicketListByConferenceId(conferenceId);
            return tickets.Select(x => new PaidTicketResponse()
            {
                TicketId = x.TicketId,
                UserId = x.UserId,
                IsRefunded = x.IsRefunded,
                UserName = x.User?.FullName ?? null,
                Email = x.User?.Email ?? null,
                AvatarUrl = x.User?.AvatarUrl ?? null,
                RegisteredDate = x.RegisteredDate ?? null,
                ConferenceId = x.PricePhase?.ConferencePrice?.ConferenceId ?? null,
                ConferenceName = x.PricePhase?.ConferencePrice?.Conference?.ConferenceName ?? null,
            }).ToList();
        }

        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;
            return await _unitOfWork.TicketRepository.GetTicketsByUserId(userId, keyword, page, size, sessionStartTime, sessionEndTime);
        }

    }
}
