using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public sealed record GetListingEngagementsQuery : IRequest<GetListingEngagementsResponse>
{
    public Guid ListingId { get; set; }
    public Guid CurrentUserId { get; set; }
}
