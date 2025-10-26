using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetTicketsByUserId(string userId);
        Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId);
        Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId);
    }
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            return await _context.Tickets.Where(x => x.UserId == userId).ToListAsync();
        }
        public async Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(x => x.UserId == userId && x.ConferencePriceId == conferencePriceId);
        }
        public async Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId)
        {
            var query = from t in _context.Tickets
                        join cp in _context.ConferencePrices on t.ConferencePriceId equals cp.ConferencePriceId
                        join c in _context.Conferences on cp.ConferenceId equals c.ConferenceId
                        join u in _context.Users on t.UserId equals u.UserId
                        where t.IsRefunded == false && c.ConferenceId == conferenceId
                        select new Ticket()
                        {
                            TicketId = t.TicketId,
                            UserId = u.UserId,
                            User = new User()
                            {
                                UserId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                AvatarUrl = u.AvatarUrl,
                            },
                            ConferencePrice = new ConferencePrice
                            {
                                ConferencePriceId = cp.ConferencePriceId,
                                ConferenceId = cp.ConferenceId,
                                Conference = new Conference
                                {
                                    ConferenceId = c.ConferenceId,
                                    ConferenceName = c.ConferenceName
                                }
                            },
                            RegisteredDate = t.RegisteredDate,

                        };
            return await query.ToListAsync();


        }
    }
}
