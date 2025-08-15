namespace Automotive.Marketplace.Domain.Entities;

using System.Collections.ObjectModel;

public class Client : Account
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public ICollection<Listing> LikedListings { get; set; } = new Collection<Listing>();

    public override string RoleName => "Client";
}
