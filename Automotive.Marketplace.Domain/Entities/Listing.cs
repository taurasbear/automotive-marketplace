using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Domain.Entities;

public class Listing : BaseEntity
{
    public decimal Price { get; set; }

    public string City { get; set; } = string.Empty;

    public Status Status { get; set; }

    public string? Description { get; set; }

    public string? Vin { get; set; }

    public string? Colour { get; set; }

    public bool IsUsed { get; set; }

    public int Year { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public Guid DrivetrainId { get; set; }

    public virtual Drivetrain Drivetrain { get; set; } = null!;

    public Guid VariantId { get; set; }

    public virtual Variant Variant { get; set; } = null!;

    public Guid SellerId { get; set; }

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = [];

    public virtual ICollection<ListingDefect> Defects { get; set; } = [];

    public virtual ICollection<User> LikeUsers { get; set; } = [];
}
