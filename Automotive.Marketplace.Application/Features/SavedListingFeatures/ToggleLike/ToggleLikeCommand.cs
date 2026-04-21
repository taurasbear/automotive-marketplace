using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public sealed record ToggleLikeCommand : IRequest<ToggleLikeResponse>
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }
}
