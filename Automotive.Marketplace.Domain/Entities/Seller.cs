namespace Automotive.Marketplace.Domain.Entities;

public class Seller : Client
{
    public string PhoneNumber { get; set; } = string.Empty;

    public virtual ICollection<Listing> Listings { get; set; } = [];

    public override string RoleName => "Seller";
}
