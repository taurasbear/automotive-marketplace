using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public sealed record ProposeMeetingCommand : IRequest<ProposeMeetingResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public DateTime ProposedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? LocationText { get; set; }

    public decimal? LocationLat { get; set; }

    public decimal? LocationLng { get; set; }

    public Guid? ParentMeetingId { get; set; }
}