namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryResponse
{
    public string? Summary { get; init; }
    public bool IsGenerated { get; init; }
    public bool FromCache { get; init; }
    public List<string> UnavailableFactors { get; init; } = [];
}
