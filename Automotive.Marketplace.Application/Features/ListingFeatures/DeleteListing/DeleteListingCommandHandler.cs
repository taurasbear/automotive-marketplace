using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;

public class DeleteListingCommandHandler(IRepository repository) : IRequestHandler<DeleteListingCommand>
{
    public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

        await repository.DeleteAsync(listing, cancellationToken);
    }
}