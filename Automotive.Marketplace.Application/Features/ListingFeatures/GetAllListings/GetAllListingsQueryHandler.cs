using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public class GetAllListingsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllListingsQuery, IEnumerable<GetAllListingsResponse>>
{
    public async Task<IEnumerable<GetAllListingsResponse>> Handle(
        GetAllListingsQuery request,
        CancellationToken cancellationToken)
    {
        var listings = await repository
            .AsQueryable<Listing>()
            .Where(listing => request.MakeId == null || request.MakeId == listing.CarDetails.Car.Model.MakeId)
            .Where(listing => request.ModelId == null || request.ModelId == listing.CarDetails.Car.ModelId)
            .Where(listing => request.City == null || request.City.ToLower() == listing.City.ToLower())
            .Where(listing => request.IsUsed == null || request.IsUsed == listing.CarDetails.Used)
            .Where(listing => request.YearFrom == null || request.YearFrom <= listing.CarDetails.Car.Year.Year)
            .Where(listing => request.YearTo == null || request.YearTo >= listing.CarDetails.Car.Year.Year)
            .Where(listing => request.PriceFrom == null || request.PriceFrom <= listing.Price)
            .Where(listing => request.PriceTo == null || request.PriceTo >= listing.Price)
            .ToListAsync(cancellationToken);

        var response = mapper.Map<IEnumerable<GetAllListingsResponse>>(listings);

        return response;
    }
}
