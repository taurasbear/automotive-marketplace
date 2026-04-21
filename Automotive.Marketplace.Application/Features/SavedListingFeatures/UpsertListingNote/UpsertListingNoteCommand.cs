using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public sealed record UpsertListingNoteCommand : IRequest
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;
}
