using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListingStatus;

public sealed record UpdateListingStatusCommand : IRequest
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CurrentUserId { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}