namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Meeting : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public DateTime ProposedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? LocationText { get; set; }

    public decimal? LocationLat { get; set; }

    public decimal? LocationLng { get; set; }

    public MeetingStatus Status { get; set; }

    public Guid? ParentMeetingId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual Meeting? ParentMeeting { get; set; }

    public virtual ICollection<Meeting> CounterMeetings { get; set; } = [];

    public virtual Message? Message { get; set; }
}
