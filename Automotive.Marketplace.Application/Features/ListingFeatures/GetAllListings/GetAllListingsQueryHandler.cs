using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public class GetAllListingsQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<GetAllListingsQuery, IEnumerable<GetAllListingsResponse>>
{
    public async Task<IEnumerable<GetAllListingsResponse>> Handle(
        GetAllListingsQuery request,
        CancellationToken cancellationToken)
    {
        var listings = await repository
            .AsQueryable<Listing>()
            .Where(listing => request.MakeId == null || request.MakeId == listing.Car.Model.MakeId)
            .Where(listing => !request.Models.Any() || request.Models.Contains(listing.Car.ModelId))
            .Where(listing => request.City == null || request.City.ToLower() == listing.City.ToLower())
            .Where(listing => request.IsUsed == null || request.IsUsed == listing.IsUsed)
            .Where(listing => request.MinYear == null || request.MinYear <= listing.Car.Year.Year)
            .Where(listing => request.MaxYear == null || request.MaxYear >= listing.Car.Year.Year)
            .Where(listing => request.MinPrice == null || request.MinPrice <= listing.Price)
            .Where(listing => request.PriceTo == null || request.PriceTo >= listing.Price)
            .ToListAsync(cancellationToken);

        List<GetAllListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetAllListingsResponse>(listing);
            foreach (var image in listing.Images)
            {
                var imageUrl = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey);
                var responseImage = new GetAllListingsResponse.Image
                {
                    Url = imageUrl,
                    AltText = image.AltText
                };
                mappedListing.Images.Add(responseImage);
            }
            response.Add(mappedListing);
        }

        return response;
    }
}
