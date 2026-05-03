namespace Automotive.Marketplace.Application.Features.ListingFeatures.ReactivateListing;

public sealed record ReactivateListingResponse
{
    public Guid ListingId { get; set; }
    public IReadOnlyList<CancelledOfferInfo> CancelledOffers { get; set; } = [];

    public sealed record CancelledOfferInfo
    {
        public Guid OfferId { get; set; }
        public Guid ConversationId { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
    }
}
