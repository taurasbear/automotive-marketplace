using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;

public class UpdateListingCommandHandler(
    IRepository repository,
    IMapper mapper) : IRequestHandler<UpdateListingCommand>
{
    public async Task Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var existingListing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);
        var updatedListing = mapper.Map(request, existingListing);

        await repository.UpdateAsync(updatedListing, cancellationToken);
    }
}