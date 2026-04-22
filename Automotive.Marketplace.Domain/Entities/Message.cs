namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public MessageType MessageType { get; set; }

    public Guid? OfferId { get; set; }

    public virtual Offer? Offer { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
