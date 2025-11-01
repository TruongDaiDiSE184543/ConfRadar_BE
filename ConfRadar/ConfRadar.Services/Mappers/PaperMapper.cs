using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.DTOs.FullPaper;

namespace ConfRadar.Services.Mappers
{
    public static class PaperMapper
    {
        public static FullPaper toModel(this CreateFullPaperRequest request, string paperURL, string pendingStatus)
        {
            return new FullPaper
            {
                FullPaperId = Guid.NewGuid().ToString(),
                FullPaperUrl = paperURL,
                ReviewStatusId = pendingStatus
            };
        }

        public static FullPaperResponse toResponse(this FullPaper model)
        {
            return new FullPaperResponse
            {
                FullPaperURL = model.FullPaperUrl,
                ReviewStatus = model.ReviewStatusId
            };
        }
    }
}
