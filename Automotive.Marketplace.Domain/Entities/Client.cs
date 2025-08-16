namespace Automotive.Marketplace.Domain.Entities;

using System.Collections.ObjectModel;

public class Client : Account
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public virtual ICollection<Listing> LikedListings { get; set; } = [];

    public override string RoleName => "Client";
}
