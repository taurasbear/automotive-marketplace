using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public class ToggleLikeCommandHandler(IRepository repository)
    : IRequestHandler<ToggleLikeCommand, ToggleLikeResponse>
{
    public async Task<ToggleLikeResponse> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var existingLike = await repository
            .AsQueryable<UserListingLike>()
            .FirstOrDefaultAsync(
                like => like.UserId == request.UserId && like.ListingId == request.ListingId,
                cancellationToken);

        if (existingLike is not null)
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

            await repository.DeleteAsync(existingLike, cancellationToken);
            return new ToggleLikeResponse { IsLiked = false };
        }

        var newLike = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ListingId = request.ListingId
        };

        await repository.CreateAsync(newLike, cancellationToken);
        return new ToggleLikeResponse { IsLiked = true };
    }
}
