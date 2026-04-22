using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public sealed record MakeOfferResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public OfferData Offer { get; set; } = null!;

    public sealed record OfferData
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal ListingPrice { get; set; }

        public decimal PercentageOff { get; set; }

        public OfferStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid InitiatorId { get; set; }

        public Guid? ParentOfferId { get; set; }
    }
}
