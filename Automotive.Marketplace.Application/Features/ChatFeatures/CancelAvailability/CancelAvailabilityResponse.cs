using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public sealed record CancelAvailabilityResponse
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ConversationId { get; set; }

    public AvailabilityCardStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid RecipientId { get; set; }
}
