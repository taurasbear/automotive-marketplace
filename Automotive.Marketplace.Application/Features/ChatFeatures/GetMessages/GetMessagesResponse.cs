using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public sealed record GetMessagesResponse
{
    public Guid ConversationId { get; set; }

    public List<Message> Messages { get; set; } = [];

    public sealed record Message
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public MessageType MessageType { get; set; }

        public OfferData? Offer { get; set; }

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
}
