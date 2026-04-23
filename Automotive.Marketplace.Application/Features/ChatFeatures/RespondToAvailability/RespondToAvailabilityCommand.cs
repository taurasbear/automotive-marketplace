using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public sealed record RespondToAvailabilityCommand : IRequest<RespondToAvailabilityResponse>
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ResponderId { get; set; }

    public AvailabilityResponseAction Action { get; set; }

    public Guid? SlotId { get; set; }

    public List<ShareBackSlot>? ShareBackSlots { get; set; }

    public sealed record ShareBackSlot
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}

public enum AvailabilityResponseAction
{
    PickSlot,
    ShareBack
}