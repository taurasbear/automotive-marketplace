namespace Automotive.Marketplace.Domain.Entities;

public class Listing : BaseEntity
{
    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public Status Status { get; set; }

    public string Vin { get; set; } = string.Empty;

    public string Colour { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public int Power { get; set; }

    public int EngineSize { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public Guid CarId { get; set; }

    public virtual Car Car { get; set; } = null!;

    public Guid SellerId { get; set; }

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = [];

    public virtual ICollection<User> LikeUsers { get; set; } = [];
}
