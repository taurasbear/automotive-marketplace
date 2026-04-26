using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsQuery : IRequest<GetSellerListingInsightsResponse>
{
    public Guid ListingId { get; set; }
    public Guid UserId { get; set; }
}
