using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryQuery : IRequest<GetListingComparisonAiSummaryResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
    public string Language { get; set; } = "lt";
}
