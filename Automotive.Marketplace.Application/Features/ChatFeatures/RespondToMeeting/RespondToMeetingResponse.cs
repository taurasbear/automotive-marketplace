using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public sealed record RespondToMeetingResponse
{
    public Guid MeetingId { get; set; }

    public Guid ConversationId { get; set; }

    public MeetingStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ResponderId { get; set; }

    public ProposeMeeting.ProposeMeetingResponse? RescheduledMeeting { get; set; }
}