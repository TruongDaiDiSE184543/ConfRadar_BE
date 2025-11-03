using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceTimelineMappers
    {
        public static ConferenceTimeline ToModel(this CreateConferenceTimelineRequest request)
        {
            return new ConferenceTimeline
            {
                ConferenceTimelineId = Guid.NewGuid().ToString(),
                ConferenceId = request.ConferenceId,
                ChangeDate = request.ChangeDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                PreviousStatusId = request.PreviousStatusId,
                AfterwardStatusId = request.AfterwardStatusId,
                Reason = request.Reason
            };
        }

        public static ConferenceTimeline ToModel(this UpdateConferenceTimelineRequest request, string timelineId)
        {
            return new ConferenceTimeline
            {
                ConferenceTimelineId = timelineId,
                ConferenceId = request.ConferenceId,
                ChangeDate = request.ChangeDate,
                PreviousStatusId = request.PreviousStatusId,
                AfterwardStatusId = request.AfterwardStatusId,
                Reason = request.Reason
            };
        }

        public static ConferenceTimelineResponse ToResponse(this ConferenceTimeline model)
        {
            return new ConferenceTimelineResponse
            {
                ConferenceTimelineId = model.ConferenceTimelineId,
                ConferenceId = model.ConferenceId,
                ChangeDate = model.ChangeDate,
                PreviousStatusId = model.PreviousStatusId,
                AfterwardStatusId = model.AfterwardStatusId,
                Reason = model.Reason,
                PreviousStatusName = model.PreviousStatus?.ConferenceStatusName,
                AfterwardStatusName = model.AfterwardStatus?.ConferenceStatusName,
                ConferenceName = model.Conference?.ConferenceName
            };
        }

        public static List<ConferenceTimelineResponse> ToResponseList(this IEnumerable<ConferenceTimeline> models)
        {
            return models.Select(m => m.ToResponse()).ToList();
        }
    }

    public class CreateConferenceTimelineRequest
    {
        public string? ConferenceId { get; set; }
        public DateOnly? ChangeDate { get; set; }
        public string? PreviousStatusId { get; set; }
        public string? AfterwardStatusId { get; set; }
        public string? Reason { get; set; }
    }

    public class UpdateConferenceTimelineRequest
    {
        public string? ConferenceId { get; set; }
        public DateOnly? ChangeDate { get; set; }
        public string? PreviousStatusId { get; set; }
        public string? AfterwardStatusId { get; set; }
        public string? Reason { get; set; }
    }

    public class ConferenceTimelineResponse
    {
        public string? ConferenceTimelineId { get; set; }
        public string? ConferenceId { get; set; }
        public DateOnly? ChangeDate { get; set; }
        public string? PreviousStatusId { get; set; }
        public string? AfterwardStatusId { get; set; }
        public string? Reason { get; set; }
        public string? PreviousStatusName { get; set; }
        public string? AfterwardStatusName { get; set; }
        public string? ConferenceName { get; set; }
    }
}