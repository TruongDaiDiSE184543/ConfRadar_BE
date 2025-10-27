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
    public interface IPaperRepository
    {
        Task<int> CreatePaperAsync(Paper paper);
        Task<int> UpdatePaperAsync(Paper paper);
        Task<bool> DeletePaperAsync(Paper paper);
        Task<Paper?> GetPaperByIdAsync(string paperId);
        Task<List<Paper>> GetAllPapersAsync();
    }
    public class PaperRepository : GenericRepository<Paper>, IPaperRepository
    {
        public PaperRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreatePaperAsync(Paper paper)
        {
            return await CreateAsync(paper);
        }

        public async Task<int> UpdatePaperAsync(Paper paper)
        {
            return await UpdateAsync(paper);
        }

        public async Task<bool> DeletePaperAsync(Paper paper)
        {
            return await RemoveAsync(paper);
        }

        public async Task<Paper?> GetPaperByIdAsync(string paperId)
        {
            return await GetByIdAsync(paperId);
        }

        public async Task<List<Paper>> GetAllPapersAsync()
        {
            return await GetAllAsync();
        }

       
    }
}
