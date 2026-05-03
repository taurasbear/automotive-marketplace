namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public sealed record CancelOfferResponse
{
    public Guid OfferId { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid ConversationId { get; set; }
}