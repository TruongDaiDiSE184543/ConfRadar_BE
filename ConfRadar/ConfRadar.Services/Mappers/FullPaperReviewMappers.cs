using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.FullPaperReview;

namespace ConfRadar.Services.Mappers
{
    public static class FullPaperReviewMappers
    {
        public static FullPaperReview ToModel(this CreateFullPaperReviewRequest request, string reviewStatusId)
        {
            return new FullPaperReview
            {
                FullPaperReviewId = Guid.NewGuid().ToString(),
                FullPaperId = request.FullPaperId,
                ReviewerId = null, // Will be set in the service
                ReviewStatusId = reviewStatusId,
                Note = request.Note,
                //FeedbackToAuthor = request.FeedbackToAuthor,
                FeedbackMaterialUrl = null, // Will be set in the service after file upload
                CreatedAt = DateTime.UtcNow
            };
        }

        public static FullPaperReviewResponse ToResponse(this FullPaperReview model)
        {
            return new FullPaperReviewResponse
            {
                FullPaperReviewId = model.FullPaperReviewId,
                GlobalStatusId = model.ReviewStatusId,
                GlobalStatusName = model.ReviewStatus?.Name,
                Note = model.Note,
                CreatedAt = model.CreatedAt,
                //FeedbackToAuthor = model.FeedbackToAuthor,
                FeedbackMaterialUrl = model.FeedbackMaterialUrl,
                ReviewerId = model.ReviewerId,
                ReviewerName = model.Reviewer?.FullName,
                ReviewerAvatarUrl = model.Reviewer?.AvatarUrl,
                FullPaperId = model.FullPaperId
            };
        }
    }
}