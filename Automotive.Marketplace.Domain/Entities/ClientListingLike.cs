namespace Automotive.Marketplace.Domain.Entities;

public class ClientListingLike : BaseEntity
{
    public Guid ListingId { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public Guid ClientId { get; set; }

    public virtual Client Client { get; set; } = null!;
}
