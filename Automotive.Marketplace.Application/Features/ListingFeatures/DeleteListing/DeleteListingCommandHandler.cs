using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;

public class DeleteListingCommandHandler(IRepository repository) : IRequestHandler<DeleteListingCommand>
{
    public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

        var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
        if (listing.SellerId != request.CurrentUserId && !canManage)
            throw new UnauthorizedAccessException("You can only delete your own listings.");

        await repository.DeleteAsync(listing, cancellationToken);
    }
}