using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;

public class DeleteListingNoteCommandHandler(IRepository repository)
    : IRequestHandler<DeleteListingNoteCommand>
{
    public async Task Handle(DeleteListingNoteCommand request, CancellationToken cancellationToken)
    {
        var existingNote = await repository
            .AsQueryable<UserListingNote>()
            .FirstOrDefaultAsync(
                note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                cancellationToken);

        if (existingNote is not null)
        {
            await repository.DeleteAsync(existingNote, cancellationToken);
        }
    }
}
