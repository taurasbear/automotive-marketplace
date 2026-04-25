namespace Automotive.Marketplace.Domain.Entities;

public class Model : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? VpicId { get; set; }
    public string VpicName { get; set; } = string.Empty;
    public DateTime? SyncedAt { get; set; }

    public Guid MakeId { get; set; }

    public virtual Make Make { get; set; } = null!;

    public virtual ICollection<Variant> Variants { get; set; } = [];
}
