using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.Contract;
using ConfRadar.Shared.DTO.General;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ICollaboratorContractRepository
    {
        Task<int> CreateCollaboratorContractAsync(CollaboratorContract collaboratorContract);
        Task<int> UpdateCollaboratorContractAsync(CollaboratorContract collaboratorContract);
        Task<CollaboratorContract?> GetCollaboratorContractByIdAsync(string collaboratorContractId);
        Task<List<CollaboratorContract>> GetListCollaboratorContractByUserIdAsync(string userId);

        Task<CollaboratorContract> GetCollaboratorContractByConferenceId (string conferenceId);
       

        Task<PagedResultResponseDto<CollaboratorContractResponse>> GetListCollaboratorContractWithFilter(CollaboratorContractSearchParam request);
    }
    public class CollaboratorContractRepository : GenericRepository<CollaboratorContract>, ICollaboratorContractRepository
    {
        public CollaboratorContractRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<int> CreateCollaboratorContractAsync(CollaboratorContract collaboratorContract)
        {
            return await CreateAsync(collaboratorContract);
        }

        public async Task<CollaboratorContract?> GetCollaboratorContractByIdAsync(string collaboratorContractId)
        {
            return await _context.CollaboratorContracts
                .FirstOrDefaultAsync(cc => cc.CollaboratorContractId == collaboratorContractId);
        }
        public async Task<List<CollaboratorContract>> GetListCollaboratorContractByUserIdAsync(string userId)
        {
            return await _context.CollaboratorContracts
                .Include(cc => cc.User)

                .Include(cc => cc.Conference)
                    .ThenInclude(c=>c.City)
                .Include(cc => cc.Conference)
                    .ThenInclude(c => c.ConferenceStatus)
                .Include(cc => cc.Conference)
                    .ThenInclude(c => c.ConferenceCategory)
                .Include(cc => cc.Conference)
                    .ThenInclude(c => c.CreatedByNavigation)
                .AsSplitQuery()
                .Where(cc => cc.UserId == userId).ToListAsync();
        }

        public async Task<int> UpdateCollaboratorContractAsync(CollaboratorContract collaboratorContract)
        {
            return await UpdateAsync(collaboratorContract);
        }



        public async Task<PagedResultResponseDto<CollaboratorContractResponse>> GetListCollaboratorContractWithFilter(CollaboratorContractSearchParam request)
        {
            var query = _context.CollaboratorContracts
                .Include(cc => cc.User)
                    .ThenInclude(u => u.Organization)
                .Include(cc => cc.Conference)
                    .ThenInclude(c => c.ConferenceStatus)

                .Include(cc => cc.Conference)
                .ThenInclude(c => c.ConferenceCategory)


                .AsQueryable();



            if (!string.IsNullOrEmpty(request.ConferenceName))
            {
                query = query.Where(cc =>
                    cc.Conference != null &&
                    cc.Conference.ConferenceName != null &&
                    cc.Conference.ConferenceName.Contains(request.ConferenceName));
            }
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(cc =>
                    cc.User != null &&
                    cc.User.UserId.Equals(request.UserId));
            }

            if (!string.IsNullOrEmpty(request.OrganizationId))
            {
                query = query.Where(cc =>
                    cc.User != null && cc.User.Organization != null &&
                    cc.User.Organization.OrganizationId == request.OrganizationId);
            }
            var totalCount = await query.CountAsync();
            var data = await query
            .OrderByDescending(cc => cc.SignDay)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

            var items = data.Select(cc => new CollaboratorContractResponse
            {
                CollaboratorContractId = cc.CollaboratorContractId,
                CollaboratorContractUserId = cc.UserId,
                OrganizationId = cc.User?.Organization?.OrganizationId,
                OrganizationDescription = cc.User?.Organization?.OrganizationDescription,
                OrganizationName = cc.User?.Organization?.OrganizationName,


                IsSponsorStep = cc.IsSponsorStep,
                IsMediaStep = cc.IsMediaStep,
                IsPolicyStep = cc.IsPolicyStep,
                IsSessionStep = cc.IsSessionStep,
                IsPriceStep = cc.IsPriceStep,
                IsTicketSelling = cc.IsTicketSelling,
                IsClosed = cc.IsClosed,
                SignDay = cc.SignDay,
                FinalizePaymentDate = cc.FinalizePaymentDate,
                Commission = cc.Commission,
                ContractUrl = cc.ContractUrl,
                ConferenceId = cc.ConferenceId,

                ConferenceName = cc.Conference?.ConferenceName,
                ConferenceDescription = cc.Conference?.Description,
                ConferenceStartDate = cc.Conference?.StartDate,
                ConferenceEndDate = cc.Conference?.EndDate,
                ConferenceTotalSlot = cc.Conference?.TotalSlot,
                ConferenceAvailableSlot = cc.Conference?.AvailableSlot,
                ConferenceAddress = cc.Conference?.Address,
                ConferenceBannerImageUrl = cc.Conference?.BannerImageUrl,
                ConferenceCreatedAt = cc.Conference?.CreatedAt,
                ConferenceTicketSaleStart = cc.Conference?.TicketSaleStart,
                ConferenceTicketSaleEnd = cc.Conference?.TicketSaleEnd,
                IsInternalHosted = cc.Conference?.IsInternalHosted,
                IsResearchConference = cc.Conference?.IsResearchConference,
                CityId = cc.Conference?.CityId,
                ConferenceCreatedBy = cc.Conference?.CreatedBy,
                ConferenceCategoryId = cc.Conference?.ConferenceCategoryId,
                ConferenceCategoryName = cc.Conference?.ConferenceCategory?.ConferenceCategoryName,
                ConferenceStatusId = cc.Conference?.ConferenceStatusId,
                ConferenceStatusName = cc.Conference?.ConferenceStatus?.ConferenceStatusName

            }).ToList();
            return new PagedResultResponseDto<CollaboratorContractResponse>()
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize


            };


        }


        public async Task<CollaboratorContract> GetCollaboratorContractByConferenceId(string conferenceId)
        {
            return await _context.CollaboratorContracts.FirstOrDefaultAsync(cc => cc.ConferenceId == conferenceId);
        }

    }
}
