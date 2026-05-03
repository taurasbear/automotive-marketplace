using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.ReactivateListing;

public sealed record ReactivateListingCommand : IRequest<ReactivateListingResponse>
{
    public Guid ListingId { get; set; }
    public Guid CurrentUserId { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
