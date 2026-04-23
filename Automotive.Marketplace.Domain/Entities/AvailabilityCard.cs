namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class AvailabilityCard : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public AvailabilityCardStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual ICollection<AvailabilitySlot> Slots { get; set; } = [];

    public virtual Message? Message { get; set; }
}
