namespace Automotive.Marketplace.Domain.Entities;

public class ListingAiSummaryCache : BaseEntity
{
    public Guid ListingId { get; set; }
    public string SummaryType { get; set; } = string.Empty;
    public Guid? ComparisonListingId { get; set; }
    public string Language { get; set; } = "lt";
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
