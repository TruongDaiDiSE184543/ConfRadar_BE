using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.General;
using ConfRadar.Services.DTOs.Ticket;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;

namespace ConfRadar.Services.Services
{
    public interface ITicketService
    {
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10,  DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
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
            if (tickets == null || tickets.Count <= 0)
            {
                return new List<PaidTicketResponse>();
            }
            return tickets.Select(x => new PaidTicketResponse()
            {
                TicketId = x.TicketId,
                UserId = x.UserId!,
                UserName = x.User?.FullName ?? "",
                Email = x.User?.Email ?? "",
                AvatarUrl = x.User?.AvatarUrl ?? "",
                //RegisteredDate = x.RegisteredDate ?? DateTime.Now,
                ConferenceId = x.ConferencePrice?.ConferencePriceId ?? "",
                ConferenceName = x.ConferencePrice.Conference.ConferenceName,
            }).ToList();
        }

        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId,string? keyword,int? pageNumber=1,int? pageSize=10,  DateTime? sessionStartTime = null,  DateTime? sessionEndTime = null)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;
            return await _unitOfWork.TicketRepository.GetTicketsByUserId(userId,keyword, page, size,sessionStartTime,sessionEndTime);
        }

    }
}
