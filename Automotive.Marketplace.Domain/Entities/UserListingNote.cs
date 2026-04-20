namespace Automotive.Marketplace.Domain.Entities;

public class UserListingNote : BaseEntity
{
    public Guid ListingId { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
}
