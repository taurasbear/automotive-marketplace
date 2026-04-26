namespace Automotive.Marketplace.Domain.Entities;

public class VehicleEfficiencyCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public double? LitersPer100Km { get; set; }
    public double? KWhPer100Km { get; set; }
    public string? FetchedTrimName { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
