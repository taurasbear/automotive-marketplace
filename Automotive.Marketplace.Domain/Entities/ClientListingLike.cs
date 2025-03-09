namespace Automotive.Marketplace.Domain.Entities
{
    public class ClientListingLike : BaseEntity
    {
        public Guid ListingId { get; set; }

        public Listing Listing { get; set; } = null!;

        public Guid ClientId { get; set; }

        public Client Client { get; set; } = null!;
    }
}
