namespace Automotive.Marketplace.Domain.Entities
{
    public class Image : BaseEntity
    {
        public string ImagePath { get; set; } = string.Empty;

        public string AltText { get; set; } = string.Empty;

        public Guid ListingId { get; set; }

        public Listing Listing { get; set; } = new Listing();
    }
}
