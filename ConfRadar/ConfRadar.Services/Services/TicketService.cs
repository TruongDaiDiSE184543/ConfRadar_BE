using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
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
    }
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            return _unitOfWork.TicketRepository.GetTicketsByUserId(userId);
        }

    }
}
