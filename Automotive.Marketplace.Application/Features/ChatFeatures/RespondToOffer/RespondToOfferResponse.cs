using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public sealed record RespondToOfferResponse
{
    public Guid OfferId { get; set; }

    public Guid ConversationId { get; set; }

    public OfferStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ResponderId { get; set; }

    public MakeOffer.MakeOfferResponse? CounterOffer { get; set; }
}
