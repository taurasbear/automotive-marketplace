namespace Automotive.Marketplace.Domain.Entities;

public class Variant : BaseEntity
{
    public bool IsCustom { get; set; }

    public int DoorCount { get; set; }

    public int PowerKw { get; set; }

    public int EngineSizeMl { get; set; }

    public Guid ModelId { get; set; }

    public virtual Model Model { get; set; } = null!;

    public Guid FuelId { get; set; }

    public virtual Fuel Fuel { get; set; } = null!;

    public Guid TransmissionId { get; set; }

    public virtual Transmission Transmission { get; set; } = null!;

    public Guid BodyTypeId { get; set; }

    public virtual BodyType BodyType { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = [];
}
