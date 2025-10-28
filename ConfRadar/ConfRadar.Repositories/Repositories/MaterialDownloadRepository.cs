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
    public interface IMaterialDownloadRepository
    {
        Task<int> CreateMaterialDownloadAsync(MaterialDownload materialDownload);
        Task<int> UpdateMaterialDownloadAsync(MaterialDownload materialDownload);
        Task<int> DeleteMaterialDownloadAsync(MaterialDownload materialDownload);
        Task<MaterialDownload?> GetMaterialDownloadByIdAsync(string materialDownloadId);
        Task<List<MaterialDownload>> GetMaterialsByConferenceIdAsync(string conferenceId);
    }

    public class MaterialDownloadRepository
        : GenericRepository<MaterialDownload>, IMaterialDownloadRepository
    {
        public MaterialDownloadRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateMaterialDownloadAsync(MaterialDownload materialDownload)
        {
            return await CreateAsync(materialDownload);
        }

        public async Task<int> UpdateMaterialDownloadAsync(MaterialDownload materialDownload)
        {
            return await UpdateAsync(materialDownload);
        }

        public async Task<int> DeleteMaterialDownloadAsync(MaterialDownload materialDownload)
        {
            _context.MaterialDownloads.Remove(materialDownload);
            return await _context.SaveChangesAsync();
        }

        public async Task<MaterialDownload?> GetMaterialDownloadByIdAsync(string materialDownloadId)
        {
            return await _context.MaterialDownloads
                .FirstOrDefaultAsync(m => m.MaterialDownloadId == materialDownloadId);
        }

        public async Task<List<MaterialDownload>> GetMaterialsByConferenceIdAsync(string conferenceId)
        {
            return await _context.MaterialDownloads
                .Where(m => m.ConferenceId == conferenceId)
                .ToListAsync();
        }
    }
}