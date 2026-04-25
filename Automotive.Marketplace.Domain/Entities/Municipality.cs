namespace Automotive.Marketplace.Domain.Entities;

public class Municipality
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}
