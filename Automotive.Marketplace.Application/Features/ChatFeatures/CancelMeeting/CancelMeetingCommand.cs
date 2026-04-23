using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public sealed record CancelMeetingCommand : IRequest<CancelMeetingResponse>
{
    public Guid MeetingId { get; set; }

    public Guid CancellerId { get; set; }
}
