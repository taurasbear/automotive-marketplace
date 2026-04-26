namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryResponse
{
    public string? Summary { get; init; }
    public bool IsGenerated { get; init; }
    public bool FromCache { get; init; }
}
