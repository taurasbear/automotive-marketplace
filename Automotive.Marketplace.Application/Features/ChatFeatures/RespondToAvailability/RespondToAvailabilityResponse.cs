using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public sealed record RespondToAvailabilityResponse
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ConversationId { get; set; }

    public AvailabilityResponseAction Action { get; set; }

    public ProposeMeetingResponse? PickedSlotMeeting { get; set; }

    public ShareAvailabilityResponse? SharedBackAvailability { get; set; }
}