using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public class UpsertListingNoteCommandHandler(IRepository repository)
    : IRequestHandler<UpsertListingNoteCommand>
{
    public async Task Handle(UpsertListingNoteCommand request, CancellationToken cancellationToken)
    {
        var likeExists = await repository
            .AsQueryable<UserListingLike>()
            .AnyAsync(
                like => like.UserId == request.UserId && like.ListingId == request.ListingId,
                cancellationToken);

        if (!likeExists)
        {
            throw new DbEntityNotFoundException(nameof(UserListingLike), request.ListingId);
        }

        var existingNote = await repository
            .AsQueryable<UserListingNote>()
            .FirstOrDefaultAsync(
                note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                cancellationToken);

        if (existingNote is not null)
        {
            existingNote.Content = request.Content;
            existingNote.ModifiedAt = DateTime.UtcNow;
            existingNote.ModifiedBy = request.UserId.ToString();
            await repository.UpdateAsync(existingNote, cancellationToken);
        }
        else
        {
            var newNote = new UserListingNote
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ListingId = request.ListingId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId.ToString()
            };
            await repository.CreateAsync(newNote, cancellationToken);
        }
    }
}
