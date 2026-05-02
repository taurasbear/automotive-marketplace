using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListingStatus;

public class UpdateListingStatusCommandHandler(IRepository repository)
    : IRequestHandler<UpdateListingStatusCommand>
{
    public async Task Handle(UpdateListingStatusCommand request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

        var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
        if (listing.SellerId != request.CurrentUserId && !canManage)
            throw new UnauthorizedAccessException("You can only update your own listings.");

        if (!Enum.TryParse<Status>(request.Status, out var newStatus))
            throw new ArgumentException($"Invalid status: {request.Status}");

        listing.Status = newStatus;
        await repository.UpdateAsync(listing, cancellationToken);
    }
}