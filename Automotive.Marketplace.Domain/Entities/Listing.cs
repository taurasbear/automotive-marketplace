namespace Automotive.Marketplace.Domain.Entities
{
    using System.Collections.ObjectModel;

    public class Listing : BaseEntity
    {
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public Guid CarDetailsId { get; set; }

        public CarDetails CarDetails { get; set; } = new CarDetails();

        public Guid SellerId { get; set; }

        public Seller Seller { get; set; } = new Seller();

        public ICollection<Image> Images { get; set; } = new Collection<Image>();

        public ICollection<Client> LikeClients { get; set; } = new Collection<Client>();
    }
}
