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
            if (papers == null || papers.Count == 0)
            {
                return totalReviewedPapersDetailResponse;
            }

            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var acceptedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Accepted.GetDescription());

            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
            var rejectedReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());


            //var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());


            if (acceptedGlobalStatus == null || acceptedReviewStatus == null || rejectedReviewStatus == null || rejectedGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
            }
            foreach (var paper in papers)
            {
                var paperReviewRole = paper.PaperReviewers.FirstOrDefault(pa => pa.UserId == userId);
                bool isHeadReviewer = paperReviewRole?.IsHeadReviewer ?? false;
                bool isAllReviewed = false;
                //var paperPhase = paper.PaperPhase;
                if (paper.FullPaper != null)
                {
                    if (paper.FullPaper.ReviewStatusId == acceptedReviewStatus.ReviewStatusId|| paper.FullPaper.ReviewStatusId == rejectedReviewStatus.ReviewStatusId) 
                        isAllReviewed = true;
                }
                if (paper.RevisionPaper != null)
                {
                    if (paper.RevisionPaper.GlobalStatusId == acceptedGlobalStatus.GlobalStatusId || paper.RevisionPaper.GlobalStatusId==rejectedGlobalStatus.GlobalStatusId) 
                        isAllReviewed = true;
                }
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
            if (papers ==null || papers.Count <= 0)
            {
                return totalPendingReview;
            }
            //papers = papers.Where(p => p.Ticket != null && p.Ticket.IsRefunded == false).ToList();
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var pendingReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Pending.GetDescription());

            //var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
          

            if (pendingGlobalStatus == null || pendingReviewStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái");
            }
            foreach (var paper in papers)
            {
                var paperReviewRole = paper.PaperReviewers.FirstOrDefault(pa => pa.UserId == userId);
                bool isHeadReviewer = paperReviewRole?.IsHeadReviewer ?? false; 
                //var paperPhase = paper.PaperPhase;
                if (isHeadReviewer)
                {
                    bool isPending = false;
                   
                    if (paper.FullPaper != null)
                    {
                        if (paper.FullPaper.ReviewStatusId == pendingReviewStatus.ReviewStatusId)
                            isPending = true;
                    }
                    if (paper.RevisionPaper != null)
                    {
                        if (paper.RevisionPaper.GlobalStatusId == pendingGlobalStatus.GlobalStatusId)
                            isPending = true;
                    }
                   
                    if (isPending)
                    {
                        totalPendingReview.TotalPendingReview += 1;
                        var paperDetail = CreatePaperDetail(paper, isHeadReviewer);
                        totalPendingReview.PaperDetails.Add(paperDetail);
                    }
                }
                else
                {
                   
                    bool isPending = false;
                    if (paper.FullPaper != null)
                    {
                        if (paper.FullPaper.ReviewStatusId == pendingReviewStatus.ReviewStatusId)
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
