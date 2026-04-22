using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public sealed record GetListingComparisonResponse
{
    public GetListingByIdResponse ListingA { get; set; } = null!;
    public GetListingByIdResponse ListingB { get; set; } = null!;
}
