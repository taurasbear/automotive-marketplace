using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;

public sealed record DeleteListingCommand : IRequest
{
    public required Guid Id { get; set; }
};