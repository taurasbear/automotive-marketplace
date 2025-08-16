namespace Automotive.Marketplace.Domain.Entities;

using System.Collections.ObjectModel;

public class Listing : BaseEntity
{
    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public Status Status { get; set; }

    public Guid CarDetailsId { get; set; }

    public virtual CarDetails CarDetails { get; set; } = null!;

    public Guid SellerId { get; set; }

    public virtual Seller Seller { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = [];

    public virtual ICollection<Client> LikeClients { get; set; } = [];
}
