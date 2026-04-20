using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public sealed record GetSavedListingsQuery : IRequest<IEnumerable<GetSavedListingsResponse>>
{
    public Guid UserId { get; set; }
}
