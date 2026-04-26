namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsResponse
{
    public MarketPositionInsight MarketPosition { get; init; } = null!;
    public ListingQualityInsight ListingQuality { get; init; } = null!;
}

public class MarketPositionInsight
{
    public decimal ListingPrice { get; init; }
    public decimal? MarketMedianPrice { get; init; }
    public double? PriceDifferencePercent { get; init; }
    public int? MarketListingCount { get; init; }
    public int DaysListed { get; init; }
    public bool HasMarketData { get; init; }
}

public class ListingQualityInsight
{
    public int QualityScore { get; init; }
    public bool HasDescription { get; init; }
    public bool HasPhotos { get; init; }
    public int PhotoCount { get; init; }
    public bool HasVin { get; init; }
    public bool HasColour { get; init; }
    public List<string> Suggestions { get; init; } = [];
}
