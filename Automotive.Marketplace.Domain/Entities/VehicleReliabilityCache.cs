namespace Automotive.Marketplace.Domain.Entities;

public class VehicleReliabilityCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int RecallCount { get; set; }
    public int ComplaintCrashes { get; set; }
    public int ComplaintInjuries { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
