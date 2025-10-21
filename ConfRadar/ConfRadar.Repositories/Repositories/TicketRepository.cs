using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetTicketsByUserId(string userId);
        Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId);
    }
    public class TicketRepository :GenericRepository<Ticket>,ITicketRepository
    {
        public TicketRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            return await _context.Tickets.Where(x => x.UserId == userId).ToListAsync();
        }
        public async Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId,string conferencePriceId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(x => x.UserId == userId&& x.ConferencePriceId == conferencePriceId);
        }
    }
}
