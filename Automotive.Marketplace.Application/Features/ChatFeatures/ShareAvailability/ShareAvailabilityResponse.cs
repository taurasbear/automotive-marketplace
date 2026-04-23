using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public sealed record ShareAvailabilityResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public AvailabilityCardData AvailabilityCard { get; set; } = null!;

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