using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Domain.Entities;

public class Car : BaseEntity
{
    public DateTime Year { get; set; }

    public Fuel Fuel { get; set; }

    public Transmission Transmission { get; set; }

    public BodyType BodyType { get; set; }

    public Drivetrain Drivetrain { get; set; }

    public int DoorCount { get; set; }

    public Guid ModelId { get; set; }

    public virtual Model Model { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = [];
}
