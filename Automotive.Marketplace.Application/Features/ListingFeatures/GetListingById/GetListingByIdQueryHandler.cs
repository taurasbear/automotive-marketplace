
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public class GetListingByIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetListingByIdQuery, GetListingByIdResponse>
{
    public async Task<GetListingByIdResponse> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

        var response = mapper.Map<GetListingByIdResponse>(listing);
        return response;
    }
}