using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.Abstract;
using ConfRadar.Shared.DTO.Paper;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaperRepository
    {
        Task<int> CreatePaperAsync(Paper paper);
        Task<int> UpdatePaperAsync(Paper paper);
        Task<bool> DeletePaperAsync(Paper paper);
        Task<Paper?> GetPaperByIdAsync(string paperId);
        Task<Paper?> GetPaperByPaperIdAndUserIdAsync(string paperId, string userId);
        Task<Paper?> GetPaperByCameraReadyIdAsync(string cameraReadyId);
        Task<Paper?> GetPaperByFullPaperIdAsync(string fullPaperId);
        Task<List<Paper>> GetAllPapersAsync();
        Task<Paper?> GetPaperByIdWithPhaseAsync(string paperId);
        Task<Paper?> GetPaperByUserAndConference(string conferenceId, string userId);
        Task<List<UnAssignAbstractResponse>> GetUnAssignAbstract();

        Task<PaperDetailForReviewerResponse?> GetPaperDetailForReviewer(string paperId, string userId);



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
            return await _context.Papers
                .Include(p => p.PaperPhase)
                .Include(p => p.Conference)
                    .ThenInclude(c => c.ResearchConferencePhases)
                        .ThenInclude(rcp => rcp.RevisionRoundDeadlines)
                .Include(p => p.Conference)
                    .ThenInclude(c => c.ResearchConferenceDetail)
       .FirstOrDefaultAsync(p => p.PaperId == paperId);
        }

        public async Task<List<Paper>> GetAllPapersAsync()
        {
            return await _context.Papers
                 .Include(p => p.PaperPhase)
                    .Include(p => p.Conference)
                .ThenInclude(p => p.ResearchConferenceDetail)
                .ToListAsync();
        }

        public async Task<Paper?> GetPaperByPaperIdAndUserIdAsync(string paperId, string userId)
        {
            return await _context.Papers.Include(p => p.Presenter).FirstOrDefaultAsync(p => p.PaperId == paperId && p.PresenterId == userId);
        }

        public async Task<Paper?> GetPaperByCameraReadyIdAsync(string cameraReadyId)
        {
            return await _context.Papers
                .Include(p => p.Presenter)
                .FirstOrDefaultAsync(p => p.CameraReadyId == cameraReadyId);
        }

        public async Task<Paper?> GetPaperByFullPaperIdAsync(string fullPaperId)
        {
            return await _context.Papers
                .Include(p => p.Presenter)
                .FirstOrDefaultAsync(p => p.FullPaperId == fullPaperId);
        }

        public async Task<Paper?> GetPaperByIdWithPhaseAsync(string paperId)
        {
            return await _context.Papers
                .Include(p => p.PaperPhase)
                .Include(p => p.CameraReady)
                .FirstOrDefaultAsync(p => p.PaperId == paperId);
        }

       
        public async Task<Paper?> GetPaperByUserAndConference(string conferenceId, string userId)
        {
            return await _context.Papers
               .Include(p => p.Conference)
               .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.PresenterId == userId);
        }

        public async Task<List<UnAssignAbstractResponse>> GetUnAssignAbstract()
        {
            var unassignAbstract = await (from p in _context.Papers
                                          join a in _context.Abstracts.Include(a => a.GlobalStatus) on p.AbstractId equals a.AbstractId
                                          where !(from pr in _context.PaperReviewers
                                                 select pr.PaperId).Contains(p.PaperId)
                                          select new {p,a}
                                          ).ToListAsync();
            var result = unassignAbstract.Select(x => new UnAssignAbstractResponse()
            {
                AbstractId = x.a.AbstractId,
                AbstractUrl = x.a.AbstractUrl,
                GlobalStatusId = x.a.GlobalStatusId,
                GlobalStatusName = x.a.GlobalStatus?.Name ?? null,
                PaperId = x.p.PaperId,
            }).ToList();
            return result;
        }

        public async Task<PaperDetailForReviewerResponse?> GetPaperDetailForReviewer(string paperId, string userId)
        {
            var result = await (
       from p in _context.Papers.Include(p=>p.Conference)
                                    .ThenInclude(c=>c.ResearchConferencePhases)
                                        .ThenInclude(rcp=>rcp.RevisionRoundDeadlines)

       join a in _context.Abstracts on p.AbstractId equals a.AbstractId

       join f in _context.FullPapers.Include(f => f.ReviewStatus) on p.FullPaperId equals f.FullPaperId into fps
       from f in fps.DefaultIfEmpty()

       join r in _context.RevisionPapers.Include(rp => rp.GlobalStatus) on p.RevisionPaperId equals r.RevisionPaperId into rps
       from r in rps.DefaultIfEmpty()

       join c in _context.Conferences on p.ConferenceId equals c.ConferenceId

       join pr in _context.PaperReviewers on p.PaperId equals pr.PaperId
       where p.PaperId == paperId && pr.UserId == userId
       select new { p, a, f, r, c, pr }
   ).FirstOrDefaultAsync();

            if (result == null)
                return null;

            var paperDetailResponse = new PaperDetailForReviewerResponse
            {
                IsHeadReviewer = result.pr.IsHeadReviewer ?? false
            };
            var paperReviewerList = await _context.PaperReviewers.Where(pr => pr.PaperId == result.p.PaperId).ToListAsync();
            // Check Full Paper
            if (result.f != null)
            {
                paperDetailResponse.FullPaper = new FullPaperDetailForReviewerResponse()
                {
                    FullPaperId = result.f.FullPaperId,
                    FullPaperUrl = result.f.FullPaperUrl,
                    ReviewStatusId = result.f.ReviewStatusId,
                    ReviewStatusName = result.f.ReviewStatus?.Name
                };
                if (paperDetailResponse.IsHeadReviewer)
                {
                    var fullPaperReviewerList = await _context.FullPaperReviews.Where(fp => fp.FullPaperId == result.f.FullPaperId).ToListAsync();
                    //paperReviewerList = await _context.PaperReviewers.Where(pr => pr.PaperId == result.p.PaperId).ToListAsync();

                    if (fullPaperReviewerList.Count == paperReviewerList.Count)
                    {
                        paperDetailResponse.FullPaper.IsAllSubmittedFullPaperReview = true;
                    }
                    else
                    {
                        paperDetailResponse.FullPaper.IsAllSubmittedFullPaperReview = false;
                    }
                }
                else
                {
                    paperDetailResponse.FullPaper.IsAllSubmittedFullPaperReview = null;
                }

            }

            // Check Revision Paper
            if (result.r != null)
            {
               

                var revisionSubmissionsQuery = _context.RevisionPaperSubmissions
                    .Where(rs => rs.RevisionPaperId == result.r.RevisionPaperId)
                    .Include(rs=>rs.RevisionDeadlineRound)
                    .OrderBy(rs => rs.RevisionDeadlineRound.RoundNumber) 
                    .ThenBy(rs => rs.RevisionDeadlineRound.EndDate)
                    .Select(rs => new RevisionPaperSubmissionForReviewerResponse
                    {
                        RevisionPaperSubmissionId = rs.RevisionPaperSubmissionId,
                        RevisionPaperId = rs.RevisionPaperId,
                        RevisionDeadlineRoundId = rs.RevisionDeadlineRoundId,
                        EndDate = rs.RevisionDeadlineRound.EndDate,
                        RoundNumber = rs.RevisionDeadlineRound.RoundNumber,
                        RevisionPaperUrl = rs.RevisionPaperUrl,
                        RevisionSubmissionFeedbacks = new List<RevisionPaperSubmissionFeedBackForReviewerResponse>()
                    });

                var revisionSubmissions = await revisionSubmissionsQuery.ToListAsync();

                //nếu là head reviewer include submission
                if (paperDetailResponse.IsHeadReviewer)
                {
                    foreach (var rs in revisionSubmissions)
                    {
                        rs.RevisionSubmissionFeedbacks = await _context.RevisionSubmissionFeedbacks
                            .Include(fb=>fb.User)
                            .Where(fb => fb.RevisionPaperSubmissionId == rs.RevisionPaperSubmissionId)
                            .OrderBy(fb => fb.SortOrder)
                            .Select(fb => new RevisionPaperSubmissionFeedBackForReviewerResponse
                            {
                                RevisionSubmissionFeedbackId = fb.RevisionSubmissionFeedbackId,
                                UserId = fb.UserId,
                                FullName = fb.User != null ? fb.User.FullName : null,
                                AvatarUrl = fb.User!=null ? fb.User.AvatarUrl : null,
                                Feedback = fb.Feedback,
                                Response = fb.Response,
                                SortOrder = fb.SortOrder,
                                CreatedAt = fb.CreatedAt
                            })
                            .ToListAsync();
                    }
                    paperDetailResponse.RevisionPaper = new RevisonPaperForReviewerResponse
                    {
                        RevisionPaperId = result.r.RevisionPaperId,
                        GlobalStatusId = result.r.GlobalStatusId,
                        GlobalStatusName = result.r.GlobalStatus?.Name,
                        RevisionRound = result.r.RevisionRound ?? 0,
                        RevisionPaperSubmissions = revisionSubmissions
                    };

                    var allResearchConferencePhases = result.p.Conference?.ResearchConferencePhases;
                    if (allResearchConferencePhases == null || !allResearchConferencePhases.Any())
                    {
                        paperDetailResponse.RevisionPaper.IsAnsweredAllDiscussion = null;
                    }
                    var allPhaseIds = allResearchConferencePhases.Select(p=>p.ResearchConferencePhaseId).ToList();

                    var allDeadlines = await _context.RevisionRoundDeadlines
                    .Where(r => allPhaseIds.Contains(r.ResearchConferencePhaseId))
                    .ToListAsync();

                    var allDeadLineIds = allDeadlines.Select(d=>d.RevisionRoundDeadlineId).ToList();

                    var allSubmissions = await _context.RevisionPaperSubmissions
                    .Where(rps => allDeadLineIds.Contains(rps.RevisionDeadlineRoundId))
                    .ToListAsync();

                    var allSubmissionIds = allSubmissions.Select(s => s.RevisionPaperSubmissionId).ToList();

                    var allFeedbacks = await _context.RevisionSubmissionFeedbacks
                    .Where(fb => allSubmissionIds.Contains(fb.RevisionPaperSubmissionId))
                    .ToListAsync();
                    bool hasIncompleteDiscussion = allFeedbacks.Any(fb => fb.Feedback == null || fb.Response == null);
                    paperDetailResponse.RevisionPaper.IsAnsweredAllDiscussion = !hasIncompleteDiscussion;

                }
                else
                {
                    paperDetailResponse.RevisionPaper.IsAnsweredAllDiscussion = null;
                }

                
                if (paperDetailResponse.IsHeadReviewer)
                {
                    var fullRevisionPaperReviewList = await _context.RevisionPaperReviews.Where(rpr => rpr.RevisionPaperId == result.r.RevisionPaperId).ToListAsync();
                    //var paperReviewerList = await _context.PaperReviewers.Where(pr => pr.PaperId == result.p.PaperId).ToListAsync();

                    if (fullRevisionPaperReviewList.Count == paperReviewerList.Count)
                    {
                        paperDetailResponse.RevisionPaper.IsAllSubmittedRevisionPaperReview= true;
                    }
                    else
                    {
                        paperDetailResponse.RevisionPaper.IsAllSubmittedRevisionPaperReview = false;
                    }
                }
                else
                {
                    paperDetailResponse.RevisionPaper.IsAllSubmittedRevisionPaperReview = null;
                }

            }

            return paperDetailResponse;

        }
    }
}
