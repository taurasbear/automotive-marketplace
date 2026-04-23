using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public sealed record ShareAvailabilityCommand : IRequest<ShareAvailabilityResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public List<SlotData> Slots { get; set; } = [];

    public sealed record SlotData
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}