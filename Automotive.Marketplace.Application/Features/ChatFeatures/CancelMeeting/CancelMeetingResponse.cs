using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public sealed record CancelMeetingResponse
{
    public Guid MeetingId { get; set; }

    public Guid ConversationId { get; set; }

    public MeetingStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid RecipientId { get; set; }
}
