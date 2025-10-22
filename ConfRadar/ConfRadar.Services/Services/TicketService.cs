using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface ITicketService
    {
        Task<List<Ticket>> GetTicketsByUserId(string userId);
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
            if (tickets==null || tickets.Count <= 0)
            {
                return new List<PaidTicketResponse>();
            }
           return tickets.Select(x=> new PaidTicketResponse()
           {
               TicketId = x.TicketId,
               UserId = x.UserId!,
               UserName = x.User?.FullName?? "",
               Email = x.User?.Email ?? "",
               AvatarUrl = x.User?.AvatarUrl ?? "",
               RegisteredDate = x.RegisteredDate ?? DateTime.Now,
               ConferenceId = x.ConferencePrice?.ConferencePriceId ?? "",
               ConferenceName = x.ConferencePrice.Conference.ConferenceName,
           }).ToList();
        }

        public Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            return _unitOfWork.TicketRepository.GetTicketsByUserId(userId);
        }

    }
}
