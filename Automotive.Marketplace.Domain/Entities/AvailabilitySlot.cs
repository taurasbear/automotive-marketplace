namespace Automotive.Marketplace.Domain.Entities;

public class AvailabilitySlot : BaseEntity
{
    public Guid AvailabilityCardId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public virtual AvailabilityCard AvailabilityCard { get; set; } = null!;
}
