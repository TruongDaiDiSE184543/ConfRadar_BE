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
        Task<Paper> GetAllIncludeById(string paper);
        Task<List<Paper>> GetAllAcceptedPaper(GlobalStatus acceptedStatus);

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
                .Include(p => p.ResearchConferencePhase)
                .Include(p => p.PaperAuthors)
                .Include(p => p.PaperPhase)
                .Include(p => p.Conference)
                    .ThenInclude(c => c.ResearchConferencePhases)
                        .ThenInclude(rcp => rcp.RevisionRoundDeadlines)
                .Include(p => p.Conference)
                    .ThenInclude(c => c.ResearchConferenceDetail)
                .AsSplitQuery()
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
            return await _context.Papers.FirstOrDefaultAsync(p => p.PaperId == paperId /*&& p.PresenterId == userId*/);
        }

        public async Task<Paper?> GetPaperByCameraReadyIdAsync(string cameraReadyId)
        {
            return await _context.Papers
                //.Include(p => p.Presenter)
                .FirstOrDefaultAsync(p => p.CameraReadyId == cameraReadyId);
        }

        public async Task<Paper?> GetPaperByFullPaperIdAsync(string fullPaperId)
        {
            return await _context.Papers
                //.Include(p => p.Presenter)
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
               .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId /* p.PresenterId == userId*/);
        }

        public async Task<List<UnAssignAbstractResponse>> GetUnAssignAbstract()
        {
            var unassignAbstract = await (from p in _context.Papers
                                          join a in _context.Abstracts.Include(a => a.GlobalStatus) on p.AbstractId equals a.AbstractId
                                          where !(from pr in _context.PaperReviewers
                                                  select pr.PaperId).Contains(p.PaperId)
                                          select new { p, a }
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

            var paper = await _context.Papers.AsNoTracking()
                            .Include(p => p.ResearchConferencePhase)
                             //paper phase
                             .Include(p => p.PaperPhase)
                            //full paper
                            .Include(p => p.FullPaper)
                                .ThenInclude(fp => fp.ReviewStatus)
                            //revise
                            .Include(p => p.RevisionPaper)
                            .ThenInclude(rp => rp.GlobalStatus)
                            //camera ready
                            .Include(p => p.CameraReady)
                                .ThenInclude(cr => cr.GlobalStatus)


                            .Include(p => p.Conference)
                                .ThenInclude(c => c.ResearchConferencePhases)
                                .ThenInclude(rcp => rcp.RevisionRoundDeadlines)

                            .Include(p => p.PaperReviewers)
                            .AsSplitQuery()
                            .FirstOrDefaultAsync(p => p.PaperId == paperId);
            if (paper == null)
            {
                return null;
            }


            var paperDetailResponse = new PaperDetailForReviewerResponse();
            if (paper.PaperPhase != null)
            {
                paperDetailResponse.CurrentPaperPhase = new PaperPhaseForReviewerResponse
                {
                    PaperPhaseId = paper.PaperPhase?.PaperPhaseId,
                    PhaseName = paper.PaperPhase?.PhaseName,
                };
            }

            var currentActivePhase = paper.ResearchConferencePhase;
            if (currentActivePhase != null)
            {
                paperDetailResponse.CurrentResearchConferencePhase = new CurrentResearchConferencePhaseForReviewerResponse()
                {
                    ResearchConferencePhaseId = currentActivePhase.ResearchConferencePhaseId,
                    ConferenceId = currentActivePhase.ConferenceId,
                    RegistrationStartDate = currentActivePhase.RegistrationStartDate,
                    RegistrationEndDate = currentActivePhase.RegistrationEndDate,
                    FullPaperStartDate = currentActivePhase.FullPaperStartDate,
                    FullPaperEndDate = currentActivePhase.FullPaperEndDate,
                    ReviewStartDate = currentActivePhase.ReviewStartDate,
                    ReviewEndDate = currentActivePhase.ReviewEndDate,
                    ReviseStartDate = currentActivePhase.ReviseStartDate,
                    ReviseEndDate = currentActivePhase.ReviseEndDate,
                    CameraReadyStartDate = currentActivePhase.CameraReadyStartDate,
                    CameraReadyEndDate = currentActivePhase.CameraReadyEndDate,
                    IsActive = currentActivePhase.IsActive,
                    IsWaitlist = currentActivePhase.IsWaitlist,
                    RevisionRoundsDetail = currentActivePhase.RevisionRoundDeadlines.Any() ? currentActivePhase.RevisionRoundDeadlines.Select(rrd => new RevisionRoundDeadLineDetailForReviewerResponse()
                    {
                        RevisionRoundDeadlineId = rrd.RevisionRoundDeadlineId,
                        StartSubmissionDate = rrd.StartSubmissionDate,
                        EndSubmissionDate = rrd.EndSubmissionDate,
                        ResearchConferencePhaseId = rrd.ResearchConferencePhaseId,
                        RoundNumber = rrd.RoundNumber
                    }).ToList() : new List<RevisionRoundDeadLineDetailForReviewerResponse>()
                };
            }



            var headReviewer = paper.PaperReviewers.FirstOrDefault(x => x.UserId == userId && x.IsHeadReviewer == true);
            bool isHeadReviewer = headReviewer != null;
            paperDetailResponse.IsHeadReviewer = isHeadReviewer;

            int totalReviewerCount = paper.PaperReviewers.Count;
            if (paper.FullPaper != null)
            {
                paperDetailResponse.FullPaper = new FullPaperDetailForReviewerResponse
                {
                    FullPaperId = paper.FullPaperId,
                    FullPaperUrl = paper.FullPaper.FullPaperUrl,
                    ReviewStatusId = paper.FullPaper.ReviewStatusId,
                    ReviewStatusName = paper.FullPaper.ReviewStatus?.Name,
                    Description = paper.FullPaper?.Description,
                    Title = paper.FullPaper?.Title,
                    FullPaperStartDate = currentActivePhase?.FullPaperStartDate,
                    FullPaperEndDate = currentActivePhase?.FullPaperEndDate,
                };

                if (isHeadReviewer)
                {
                    var fullPaperReviews = await _context.FullPaperReviews
                        .Where(fpr => fpr.FullPaperId == paper.FullPaperId)
                        .Include(fpr => fpr.Reviewer)
                        .Include(fpr => fpr.ReviewStatus)
                        .ToListAsync();

                    paperDetailResponse.FullPaper.FullPaperReviews = fullPaperReviews.Select(fpr => new FullPaperReviewForReviewerResponse
                    {
                        FullPaperReviewId = fpr.FullPaperReviewId,
                        ReviewStatusId = fpr.ReviewStatusId,
                        ReviewStatusName = fpr.ReviewStatus?.Name,
                        Note = fpr.Note,
                        CreatedAt = fpr.CreatedAt,
                        FeedbackToAuthor = fpr.FeedbackToAuthor,
                        FeedbackMaterialUrl = fpr.FeedbackMaterialUrl,
                        FullPaperId = fpr.FullPaperId,
                        ReviewerId = fpr.Reviewer?.UserId,
                        ReviewerName = fpr.Reviewer?.FullName,
                        ReviewerAvatarUrl = fpr.Reviewer?.AvatarUrl,

                    }).ToList();
                    var fullPaperReviewCount = fullPaperReviews.Select(f => f.ReviewerId).Distinct().Count();
                    paperDetailResponse.FullPaper.IsAllSubmittedFullPaperReview = fullPaperReviewCount == totalReviewerCount;
                }
            }
            if (paper.RevisionPaper != null)
            {
                paperDetailResponse.RevisionPaper = new RevisonPaperForReviewerResponse
                {
                    RevisionPaperId = paper.RevisionPaper.RevisionPaperId,
                    RevisionRound = paper.RevisionPaper.RevisionRound,
                    GlobalStatusId = paper.RevisionPaper.GlobalStatusId,
                    GlobalStatusName = paper.RevisionPaper.GlobalStatus?.Name,
                    ReviewStartDate = currentActivePhase?.ReviewStartDate,
                    ReviewEndDate = currentActivePhase?.ReviewEndDate,

                };
                var revisionPaperSubmission = await _context.RevisionPaperSubmissions
                                                                                    .Include(rps => rps.RevisionDeadlineRound)
                                                                                   .Include(rps => rps.RevisionSubmissionFeedbacks)
                                                                                   .ThenInclude(fb => fb.User)
                                                                                   .Where(rps => rps.RevisionPaperId == paper.RevisionPaperId).ToListAsync();
                paperDetailResponse.RevisionPaper.RevisionPaperSubmissions = revisionPaperSubmission.Select(rps => new RevisionPaperSubmissionForReviewerResponse()
                {
                    RevisionPaperSubmissionId = rps.RevisionPaperSubmissionId,
                    RevisionPaperUrl = rps.RevisionPaperUrl,
                    RevisionPaperId = rps.RevisionPaperId,
                    Title = rps.Title,
                    Description = rps.Description,
                    RevisionDeadlineRoundId = rps.RevisionDeadlineRoundId,
                    RevisionDeadlineStartSubmissionDate = rps.RevisionDeadlineRound?.StartSubmissionDate,
                    RevisionDeadlineEndSubmissionDate = rps.RevisionDeadlineRound?.EndSubmissionDate,
                    RevisionDeadlineRoundNumber = rps.RevisionDeadlineRound?.RoundNumber,
                    RevisionSubmissionFeedbacks = isHeadReviewer ? rps.RevisionSubmissionFeedbacks.Select(rsf => new RevisionPaperSubmissionFeedBackForReviewerResponse()
                    {
                        RevisionSubmissionFeedbackId = rsf.RevisionSubmissionFeedbackId,
                        UserId = rsf.UserId,
                        FullName = rsf.User?.FullName,
                        AvatarUrl = rsf.User?.AvatarUrl,
                        Feedback = rsf.Feedback,
                        Response = rsf.Response,
                        SortOrder = rsf.SortOrder,
                        CreatedAt = rsf.CreatedAt,

                    }).ToList() : new List<RevisionPaperSubmissionFeedBackForReviewerResponse>()
                }).ToList();
                if (isHeadReviewer)
                {
                    var revisionPaperReviews = await _context.RevisionPaperReviews
                        .Include(rpr => rpr.GlobalStatus)
                        .Include(rpr => rpr.Reviewer)
                        .Where(rpr => rpr.RevisionPaperId == paper.RevisionPaperId)
                        .ToListAsync();

                    paperDetailResponse.RevisionPaper.RevisionPaperReviews = revisionPaperReviews.Select(rpr => new RevisionPaperReviewForReviewerResponse()
                    {
                        RevisionPaperReviewId = rpr.RevisionPaperReviewId,
                        GlobalStatusId = rpr.GlobalStatusId,
                        GlobalStatusName = rpr.GlobalStatus?.Name,
                        Note = rpr.Note,
                        CreatedAt = rpr.CreatedAt,
                        FeedbackToAuthor = rpr.FeedbackToAuthor,
                        FeedbackMaterialUrl = rpr.FeedbackMaterialUrl,
                        ReviewerId = rpr.ReviewerId,
                        ReviewerName = rpr.Reviewer?.FullName,
                        ReviewerAvatarUrl = rpr.Reviewer?.AvatarUrl,
                        RevisionPaperId = rpr.RevisionPaperId,
                    }).ToList();


                    var revisionPaperReviewCount = revisionPaperReviews.Select(f => f.ReviewerId).Distinct().Count();
                    paperDetailResponse.RevisionPaper.IsAllSubmittedRevisionPaperReview = revisionPaperReviewCount == totalReviewerCount;

                    var revisionPaperSubmissionIds = revisionPaperSubmission.Select(rps => rps.RevisionPaperSubmissionId);
                    var revisionPaperFeedbacks = await _context.RevisionSubmissionFeedbacks.Where(rsf => revisionPaperSubmissionIds.Contains(rsf.RevisionPaperSubmissionId)).ToListAsync();
                    if (revisionPaperFeedbacks.Count > 0)
                    {
                        if (revisionPaperFeedbacks.Any(rps => rps.Feedback == null || rps.Response == null))
                        {
                            paperDetailResponse.RevisionPaper.IsAnsweredAllDiscussion = false;
                        }
                        else
                        {
                            paperDetailResponse.RevisionPaper.IsAnsweredAllDiscussion = true;
                        }

                    }

                }


            }
            if (paper.CameraReady != null)
            {
                paperDetailResponse.CameraReady = new CameraReadyPaperForReviewerResponse()
                {
                    PaperId = paper.PaperId,
                    CameraReadyId = paper.CameraReadyId,
                    GlobalStatusId = paper.CameraReady?.GlobalStatusId,
                    GlobalStatusName = paper.CameraReady?.GlobalStatus?.Name,
                    CameraReadyUrl = paper.CameraReady?.CameraReadyUrl,
                    Title = paper.CameraReady?.Title,
                    Description = paper.CameraReady?.Description,
                    CreatedAt = paper.CameraReady?.CreatedAt,
                    ReviewAt = paper.CameraReady?.ReviewAt,
                    CameraReadyStartDate = currentActivePhase?.CameraReadyStartDate,
                    CameraReadyEndDate = currentActivePhase?.CameraReadyEndDate
                };
            }
            return paperDetailResponse;
        }

        public async Task<Paper> GetAllIncludeById(string paper)
        {
            return await _context.Papers
                .Include(p => p.Abstract)
                .Include(p => p.FullPaperId)
                .Include(p => p.RevisionPaper)
                .Include(p => p.CameraReady)
                .Include(p => p.PaperReviewers)
                .Include(p => p.PresentAuthors)
                .Include(p => p.PaperAuthors)
                .Include(p => p.PaperPhase)
                .FirstOrDefaultAsync(p => p.PaperId == paper);
        }

        public async Task<List<Paper>> GetAllAcceptedPaper(GlobalStatus acceptedStatus)
        {
            return await _context.Papers
                .Include(p => p.CameraReady)
                .Where(p => p.CameraReady != null && p.CameraReady.GlobalStatusId == acceptedStatus.GlobalStatusId)
                .ToListAsync();
        }


    }

}
