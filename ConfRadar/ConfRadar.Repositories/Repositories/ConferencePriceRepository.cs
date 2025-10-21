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
    public interface IConferencePriceRepository
    {
        Task<ConferencePrice?> GetConferencePriceByConferencePriceId(string conferencePriceId);
    }
    public class ConferencePriceRepository : GenericRepository<ConferencePrice>, IConferencePriceRepository
    {
        public ConferencePriceRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<ConferencePrice?> GetConferencePriceByConferencePriceId(string conferencePriceId)
        {
            return await _context.ConferencePrices.Include(x=>x.PricePhase)
                .Include(x=>x.Conference).ThenInclude(x=>x.ConferenceSessions)
                .FirstOrDefaultAsync(x => x.ConferencePriceId == conferencePriceId);
        }
    }
}
