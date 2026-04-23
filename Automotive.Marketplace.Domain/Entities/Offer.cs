namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Offer : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public decimal Amount { get; set; }

    public OfferStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public Guid? ParentOfferId { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual Offer? ParentOffer { get; set; }

    public virtual ICollection<Offer> CounterOffers { get; set; } = [];

    public virtual Message? Message { get; set; }
}
