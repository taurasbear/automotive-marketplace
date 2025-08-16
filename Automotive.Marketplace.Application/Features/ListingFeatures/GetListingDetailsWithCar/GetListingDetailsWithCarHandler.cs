namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetListingDetailsWithCarHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetListingDetailsWithCarRequest, GetListingsDetailsWithCarResponse>
{
    public async Task<GetListingsDetailsWithCarResponse> Handle(
        GetListingDetailsWithCarRequest request,
        CancellationToken cancellationToken)
    {
        var listingsDetailsWithCar = await repository.GetAllAsync<Listing>(cancellationToken);
        return mapper.Map<GetListingsDetailsWithCarResponse>(listingsDetailsWithCar);
    }
}
