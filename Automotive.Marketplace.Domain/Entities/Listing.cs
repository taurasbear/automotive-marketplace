namespace Automotive.Marketplace.Domain.Entities
{
    public class Listing : BaseEntity
    {
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
    }
}
