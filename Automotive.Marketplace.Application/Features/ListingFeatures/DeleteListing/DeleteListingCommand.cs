using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;

public sealed record DeleteListingCommand : IRequest
{
    public Guid Id { get; set; }

    public Guid CurrentUserId { get; set; }

    public IReadOnlyList<string> Permissions { get; set; } = [];
};