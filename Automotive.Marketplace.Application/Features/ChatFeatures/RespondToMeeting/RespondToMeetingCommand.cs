using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public sealed record RespondToMeetingCommand : IRequest<RespondToMeetingResponse>
{
    public Guid MeetingId { get; set; }

    public Guid ResponderId { get; set; }

    public MeetingResponseAction Action { get; set; }

    public RescheduleData? Reschedule { get; set; }

    public sealed record RescheduleData
    {
        public DateTime ProposedAt { get; set; }

        public int DurationMinutes { get; set; }

        public string? LocationText { get; set; }

        public decimal? LocationLat { get; set; }

        public decimal? LocationLng { get; set; }
    }
}

public enum MeetingResponseAction
{
    Accept,
    Decline,
    Reschedule
}