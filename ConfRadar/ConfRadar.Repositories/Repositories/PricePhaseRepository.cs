using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPricePhaseRepository
    {
        Task<PricePhase?> GetPricePhaseByPricePhaseId(string pricePhaseId);
    }
    public class PricePhaseRepository : GenericRepository<PricePhase>, IPricePhaseRepository
    {
        public PricePhaseRepository(ConfRadarDbContext context) : base(context)
        {
        }
        public async Task<PricePhase?> GetPricePhaseByPricePhaseId(string pricePhaseId)
        {
            return await GetByIdAsync(pricePhaseId);
        }
    }
}
