using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;

public class UpdateListingCommandHandler(
    IRepository repository,
    IMapper mapper) : IRequestHandler<UpdateListingCommand>
{
    public async Task Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var existingListing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);
        
        var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
        if (existingListing.SellerId != request.CurrentUserId && !canManage)
            throw new UnauthorizedAccessException("You can only edit your own listings.");

        var updatedListing = mapper.Map(request, existingListing);

        await repository.UpdateAsync(updatedListing, cancellationToken);
    }
}