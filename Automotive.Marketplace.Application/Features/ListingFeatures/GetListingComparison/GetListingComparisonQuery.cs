using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public sealed record GetListingComparisonQuery : IRequest<GetListingComparisonResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
}
