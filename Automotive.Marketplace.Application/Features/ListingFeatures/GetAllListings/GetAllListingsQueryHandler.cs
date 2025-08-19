using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public class GetListingDetailsWithCarHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllListingsQuery, IEnumerable<GetAllListingsResponse>>
{
    public async Task<IEnumerable<GetAllListingsResponse>> Handle(
        GetAllListingsQuery request,
        CancellationToken cancellationToken)
    {
        var listingsDetailsWithCar = await repository.GetAllAsync<Listing>(cancellationToken);
        var response = mapper.Map<IEnumerable<GetAllListingsResponse>>(listingsDetailsWithCar);

        return response;
    }
}
