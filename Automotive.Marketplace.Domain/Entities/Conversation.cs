namespace Automotive.Marketplace.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid ListingId { get; set; }

    public Guid BuyerId { get; set; }

    public DateTime LastMessageAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = [];
}
