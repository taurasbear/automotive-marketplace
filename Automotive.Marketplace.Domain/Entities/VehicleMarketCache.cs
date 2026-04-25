namespace Automotive.Marketplace.Domain.Entities;

public class VehicleMarketCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal MedianPrice { get; set; }
    public int TotalListings { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
