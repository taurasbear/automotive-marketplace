using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public sealed record GetMyListingsQuery : IRequest<IEnumerable<GetMyListingsResponse>>
{
    public Guid SellerId { get; set; }
}