using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.Reviewer;

namespace ConfRadar.Services.Services
{
    public interface IReviewerService
    {
        Task<GetTotalAssignPapersDetailResponse> GetTotalAssignPapers(string userId);
        Task<GetTotalReviewedPapersDetailResponse> GetTotalReviewedPapers(string userId);
        Task<GetTotalPendingReviewsDetailResponse> GetTotalPendingReviews(string userId);
    }
    public class ReviewerService : IReviewerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReviewerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public async Task<GetTotalAssignPapersDetailResponse> GetTotalAssignPapers(string userId)
        {
            var totalPaper = await _unitOfWork.PaperReviewerRepository.getAllAssignedPapers(userId);

            var totalAssignPaperDetail = new GetTotalAssignPapersDetailResponse()
            {
                TotalPaperAssignPaper = totalPaper.Count(),
                PaperDetails = totalPaper.Select(p => new PapersDetailResponseForReviewer()
                {
                    IsHeadReviewer = p.PaperReviewers.Any(pa => (bool)pa.IsHeadReviewer && pa.UserId == userId),
                    PaperId = p.PaperId,
                    ConferenceId = p.ConferenceId,
                    ConferenceName = p.Conference?.ConferenceName,
                    PaperPhaseId = p.PaperPhaseId,
                    PaperPhaseName = p.PaperPhase?.PhaseName,
                    ResearchConferencePhaseId = p.ResearchConferencePhaseId,
                    PaperCreatedAt = p.CreatedAt,
                    PaperTitle = p.Title,
                    PaperDescription = p.Description,
                    PaperRefundedStatus = p.Ticket?.IsRefunded
                }).ToList()
            };
            return totalAssignPaperDetail;


        }



        public async Task<GetTotalReviewedPapersDetailResponse> GetTotalReviewedPapers(string userId)
        {
            var totalReviewedPapersDetailResponse = new GetTotalReviewedPapersDetailResponse();
            var papers = await _unitOfWork.PaperReviewerRepository.GetTotalPapersBelongToReviewer(userId);
            if (papers.Count <= 0)
            {
                return totalReviewedPapersDetailResponse;
            }

            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());

            var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            //var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            //var revisionPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            //var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            if (pendingGlobalStatus == null || pendingReviewStatus == null || abstractPhase == null /*|| fullPaperPhase ==null || revisionPaperPhase==null || cameraReadyPhase==null*/)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
            }
            foreach (var paper in papers)
            {
                var paperReviewRole = paper.PaperReviewers.FirstOrDefault(pa => pa.UserId == userId);
                bool isHeadReviewer = (bool)paperReviewRole.IsHeadReviewer;
                bool isAllReviewed = true;
                var paperPhase = paper.PaperPhase;
                if (paperPhase == abstractPhase)
                {
                    continue;
                }
                if (paper.FullPaper != null)
                {
                    if (paper.FullPaper.ReviewStatus == pendingReviewStatus) isAllReviewed = false;
                }
                if (paper.RevisionPaper != null)
                {
                    if (paper.RevisionPaper.GlobalStatus == pendingGlobalStatus) isAllReviewed = false;
                }
                //if (paper.CameraReady != null)
                //{
                //    if (paper.CameraReady.GlobalStatus == pendingGlobalStatus) isAllReviewed = false;
                //}
                if (isAllReviewed)
                {
                    totalReviewedPapersDetailResponse.TotalPaperReviewed += 1;
                    var paperDetail = CreatePaperDetail(paper, isHeadReviewer);
                    totalReviewedPapersDetailResponse.PaperDetails.Add(paperDetail);
                }
            }
            var result = totalReviewedPapersDetailResponse;
            return result;
        }

        public async Task<GetTotalPendingReviewsDetailResponse> GetTotalPendingReviews(string userId)
        {
            var totalPendingReview = new GetTotalPendingReviewsDetailResponse();
            var papers = await _unitOfWork.PaperReviewerRepository.GetTotalPapersBelongToReviewer(userId);
            if (papers.Count <= 0)
            {
                return totalPendingReview;
            }
            papers = papers.Where(p => p.Ticket != null && p.Ticket.IsRefunded == false).ToList();
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());

            var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            //var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            //var revisionPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            //var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());
            if (pendingGlobalStatus == null || pendingReviewStatus == null || abstractPhase == null /*|| fullPaperPhase == null || revisionPaperPhase == null || cameraReadyPhase == null*/)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
            }
            foreach (var paper in papers)
            {
                var paperReviewRole = paper.PaperReviewers.FirstOrDefault(pa => pa.UserId == userId);
                bool isHeadReviewer = (bool)paperReviewRole.IsHeadReviewer;
                var paperPhase = paper.PaperPhase;
                if (isHeadReviewer)
                {
                    bool isAllCompleted = true;
                    if (/*paper.Ticket?.IsRefunded == true || */ paperPhase == abstractPhase)
                    {
                        continue;
                    }
                    if (paper.FullPaper != null)
                    {
                        if (paper.FullPaper.ReviewStatus == pendingReviewStatus) isAllCompleted = false;
                    }
                    if (paper.RevisionPaper != null)
                    {
                        if (paper.RevisionPaper.GlobalStatus == pendingGlobalStatus) isAllCompleted = false;
                    }
                    //if (paper.CameraReady != null)
                    //{
                    //    if (paper.CameraReady.GlobalStatus == pendingGlobalStatus) isAllCompleted = false;
                    //}
                    if (!isAllCompleted)
                    {
                        totalPendingReview.TotalPendingReview += 1;
                        var paperDetail = CreatePaperDetail(paper, isHeadReviewer);
                        totalPendingReview.PaperDetails.Add(paperDetail);
                    }
                }
                else
                {
                    if (/*paper.Ticket?.IsRefunded == true ||*/ paperPhase == abstractPhase)
                    {
                        continue;
                    }
                    bool isPending = false;
                    if (paper.FullPaper != null && paper.FullPaper.ReviewStatus == pendingReviewStatus)
                    {
                        isPending = true;
                    }
                    if (paper.RevisionPaper != null && paper.RevisionPaper.GlobalStatus == pendingGlobalStatus)
                    {
                        isPending = true;
                    }
                    if (isPending)
                    {
                        totalPendingReview.TotalPendingReview += 1;
                        var paperDetail = CreatePaperDetail(paper, isHeadReviewer);
                        totalPendingReview.PaperDetails.Add(paperDetail);
                    }
                }
            }
            return totalPendingReview;
        }















        private PapersDetailResponseForReviewer CreatePaperDetail(Paper paper, bool? isHeadReviewer)
        {
            return new PapersDetailResponseForReviewer()
            {
                PaperId = paper.PaperId,
                ConferenceId = paper.ConferenceId,
                ConferenceName = paper.Conference?.ConferenceName,
                PaperPhaseId = paper.PaperPhaseId,
                PaperPhaseName = paper.PaperPhase?.PhaseName,
                ResearchConferencePhaseId = paper.ResearchConferencePhaseId,
                PaperCreatedAt = paper.CreatedAt,
                PaperTitle = paper.Title,
                PaperDescription = paper.Description,
                PaperRefundedStatus = paper.Ticket?.IsRefunded,
                IsHeadReviewer = isHeadReviewer

            };
        }
    }
}
