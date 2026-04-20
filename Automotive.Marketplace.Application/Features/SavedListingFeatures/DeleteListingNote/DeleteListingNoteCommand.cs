using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;

public sealed record DeleteListingNoteCommand : IRequest
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }
}
