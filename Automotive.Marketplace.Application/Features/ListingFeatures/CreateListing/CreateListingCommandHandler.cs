using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public class CreateListingCommandHandler(IMapper mapper, IRepository repository) : IRequestHandler<CreateListingCommand>
{
    public async Task Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var car = mapper.Map<Car>(request);
        var listing = mapper.Map<Listing>(request);

        listing.Car = car;

        await repository.CreateAsync(listing, cancellationToken);

        return;
    }
}