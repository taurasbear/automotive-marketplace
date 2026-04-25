namespace Automotive.Marketplace.Domain.Entities;

public class Municipality : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}
