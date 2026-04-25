namespace Automotive.Marketplace.Domain.Entities;

public class Make : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? VpicId { get; set; }
    public string VpicName { get; set; } = string.Empty;
    public DateTime? SyncedAt { get; set; }

    public virtual ICollection<Model> Models { get; set; } = [];
}
