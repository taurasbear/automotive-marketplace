using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public sealed record ProposeMeetingResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public MeetingData Meeting { get; set; } = null!;

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
}