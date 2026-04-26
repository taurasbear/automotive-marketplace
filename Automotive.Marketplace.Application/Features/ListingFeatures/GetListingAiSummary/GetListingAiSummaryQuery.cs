using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryQuery : IRequest<GetListingAiSummaryResponse>
{
    public Guid ListingId { get; set; }
}
