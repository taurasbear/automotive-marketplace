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

        public MeetingData? Meeting { get; set; }

        public AvailabilityCardData? AvailabilityCard { get; set; }

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

        public sealed record MeetingData
        {
            public Guid Id { get; set; }

            public DateTime ProposedAt { get; set; }

            public int DurationMinutes { get; set; }

            public string? LocationText { get; set; }

            public decimal? LocationLat { get; set; }

            public decimal? LocationLng { get; set; }

            public MeetingStatus Status { get; set; }

            public DateTime ExpiresAt { get; set; }

            public Guid InitiatorId { get; set; }

            public Guid? ParentMeetingId { get; set; }
        }

        public sealed record AvailabilityCardData
        {
            public Guid Id { get; set; }

            public AvailabilityCardStatus Status { get; set; }

            public DateTime ExpiresAt { get; set; }

            public Guid InitiatorId { get; set; }

            public List<SlotData> Slots { get; set; } = [];

            public sealed record SlotData
            {
                public Guid Id { get; set; }

                public DateTime StartTime { get; set; }

                public DateTime EndTime { get; set; }
            }
        }
    }
}
