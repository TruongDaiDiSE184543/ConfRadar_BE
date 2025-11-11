using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.ReviewContract;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IReviewerContractRepository
    {
        Task<int> CreateReviewerContractAsync(ReviewerContract contract);
        Task<int> CreateMultipleReviewerContractsAsync(List<ReviewerContract> contracts);
        Task<int> UpdateReviewerContractAsync(ReviewerContract contract);
        Task<bool> DeleteReviewerContractAsync(ReviewerContract contract);
        Task<ReviewerContract?> GetReviewerContractByIdAsync(string contractId);
        Task<List<ReviewerContract>> GetReviewerContractsByUserIdAsync(string userId);
        Task<List<ReviewerContract>> GetReviewerContractsByConferenceIdAsync(string conferenceId);
        Task<ReviewerContract?> GetContractByUserAndConferenceAsync(string userId, string conferenceId);
        Task<List<ReviewerContract>> GetAllReviewerContractsAsync();
        Task<List<ConferenceBelongToReviewContractResponse>> GetListConferenceBelongToReviewContractByUserId(string userId);
        Task<List<PaperDetailBelongToConferenceInReviewContractResposne>> GetPapersBelongToAConferenceByConferenceIdAndUserId(string conferenceId, string userId);
        Task<List<GetUsersForReviewerContractResponse>> GetUsersForReviewerContract(string conferenceId, List<string> systemRoles);
    }
    public class ReviewerContractRepository : GenericRepository<ReviewerContract>, IReviewerContractRepository
    {
        public ReviewerContractRepository(ConfRadarDbContext context) : base(context) { }

        public async Task<int> CreateReviewerContractAsync(ReviewerContract contract)
        {
            return await CreateAsync(contract);
        }

        public async Task<int> CreateMultipleReviewerContractsAsync(List<ReviewerContract> contracts)
        {
            await _context.ReviewerContracts.AddRangeAsync(contracts);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateReviewerContractAsync(ReviewerContract contract)
        {
            return await UpdateAsync(contract);
        }

        public async Task<bool> DeleteReviewerContractAsync(ReviewerContract contract)
        {
            return await RemoveAsync(contract);
        }

        public async Task<ReviewerContract?> GetReviewerContractByIdAsync(string contractId)
        {
            return await _context.ReviewerContracts
                .FirstOrDefaultAsync(c => c.ReviewerContractId == contractId);
        }

        public async Task<List<ReviewerContract>> GetReviewerContractsByUserIdAsync(string userId)
        {
            return await _context.ReviewerContracts
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<ReviewerContract>> GetReviewerContractsByConferenceIdAsync(string conferenceId)
        {
            return await _context.ReviewerContracts
                .Where(c => c.ConferenceId == conferenceId)
                .ToListAsync();
        }

        public async Task<ReviewerContract?> GetContractByUserAndConferenceAsync(string userId, string conferenceId)
        {
            return await _context.ReviewerContracts
                .Include(rc=>rc.User)
                .Include(rc=>rc.Conference)
                .FirstOrDefaultAsync(rc => rc.UserId == userId && rc.ConferenceId == conferenceId);
        }

        public async Task<List<ReviewerContract>> GetAllReviewerContractsAsync()
        {
            return await _context.ReviewerContracts.ToListAsync();
        }

        public async Task<List<ConferenceBelongToReviewContractResponse>> GetListConferenceBelongToReviewContractByUserId(string userId)
        {
            var listConferences = await _context.Conferences
                .AsNoTracking()
                .Where(c => c.ReviewerContracts.Any(rc => rc.UserId == userId))
                .Select(c => new ConferenceBelongToReviewContractResponse()
                {
                    ConferenceId = c.ConferenceId,
                    ConferenceName = c.ConferenceName,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalSlot = c.TotalSlot,
                    AvailableSlot = c.AvailableSlot,
                    Address = c.Address,
                    BannerImageUrl = c.BannerImageUrl,
                    CreatedAt = c.CreatedAt,
                    TicketSaleStart = c.TicketSaleStart,
                    TicketSaleEnd = c.TicketSaleEnd,
                    IsInternalHosted = c.IsInternalHosted,
                    IsResearchConference = c.IsResearchConference,
                    CityId = c.CityId,
                    CityName = c.City != null ? c.City.CityName : null,
                    ConferenceCategoryId = c.ConferenceCategoryId,
                    ConferenceCategoryName = c.ConferenceCategory != null ? c.ConferenceCategory.ConferenceCategoryName : null,
                    ConferenceStatusId = c.ConferenceStatusId,
                    ConferenceStatusName = c.ConferenceStatus != null ? c.ConferenceStatus.ConferenceStatusName : null,
                    ResearchConferenceDetail = c.ResearchConferenceDetail != null ? new ResearchConferenceDetailForReviewContract()
                    {
                        ConferenceId = c.ConferenceId,
                        Name = c.ResearchConferenceDetail.Name,
                        PaperFormat = c.ResearchConferenceDetail.PaperFormat,
                        NumberPaperAccept = c.ResearchConferenceDetail.NumberPaperAccept,
                        RevisionAttemptAllowed = c.ResearchConferenceDetail.RevisionAttemptAllowed,
                        RankingDescription = c.ResearchConferenceDetail.RankingDescription,
                        AllowListener = c.ResearchConferenceDetail.AllowListener,
                        RankValue = c.ResearchConferenceDetail.RankValue,
                        RankYear = c.ResearchConferenceDetail.RankYear,
                        ReviewFee = c.ResearchConferenceDetail.ReviewFee,
                        RankingCategoryId = c.ResearchConferenceDetail.RankingCategoryId,
                        RankCategoryName = c.ResearchConferenceDetail.RankingCategory != null ? c.ResearchConferenceDetail.RankingCategory.RankName : null,
                        RankCategoryDescription = c.ResearchConferenceDetail.RankingCategory != null ? c.ResearchConferenceDetail.RankingCategory.RankDescription : null,

                    } : null,
                }).ToListAsync();
            return listConferences;
        }

        public async Task<List<PaperDetailBelongToConferenceInReviewContractResposne>> GetPapersBelongToAConferenceByConferenceIdAndUserId(string conferenceId, string userId)
        {
            var listPaper = await _context.Papers
                .AsNoTracking()
                .Where(p => p.ConferenceId == conferenceId && p.Conference != null && p.Conference.ReviewerContracts.Any(rc => rc.UserId == userId))
                .Select(p => new PaperDetailBelongToConferenceInReviewContractResposne()
                {
                    PaperId = p.PaperId,
                    ConferenceId = p.ConferenceId,
                    PaperPhaseId = p.PaperPhaseId,
                    PhaseName = p.PaperPhase != null ? p.PaperPhase.PhaseName : null,
                    CreatedAt = p.CreatedAt,
                    Title = p.Title,
                    Description = p.Description,
                }).ToListAsync();
            return listPaper;
        }

        public async Task<List<GetUsersForReviewerContractResponse>> GetUsersForReviewerContract(string conferenceId, List<string> systemRoles)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u =>
            !_context.Papers.Any(p => p.ConferenceId == conferenceId && p.PaperAuthors.Any(pa => pa.UserId == u.UserId))
            && !_context.ReviewerContracts.Any(rc => rc.ConferenceId == conferenceId && rc.UserId == u.UserId)
            && u.IsActive == true && u.IsEmailConfirmed == true && u.UserRoles.All(ur => !systemRoles.Contains(ur.RoleId)))
                .Select(u => new GetUsersForReviewerContractResponse()
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    BioDescription = u.BioDescription,
                }).ToListAsync();
            return user;
        }
    }
}
