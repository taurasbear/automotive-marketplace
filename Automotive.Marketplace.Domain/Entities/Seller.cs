namespace Automotive.Marketplace.Domain.Entities
{
    using System.Collections.ObjectModel;

    public class Seller : Client
    {
        public string PhoneNumber { get; set; } = string.Empty;

        public ICollection<Listing> Listings { get; set; } = new Collection<Listing>();
    }
}
