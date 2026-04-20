using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
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
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Fuel)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Transmission)
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Include(l => l.LikeUsers)
            .Where(listing => listing.Status == Status.Available)
            .Where(listing => request.MakeId == null || request.MakeId == listing.Variant.Model.MakeId)
            .Where(listing => !request.Models.Any() || request.Models.Contains(listing.Variant.ModelId))
            .Where(listing => request.City == null || request.City.ToLower() == listing.City.ToLower())
            .Where(listing => request.IsUsed == null || request.IsUsed == listing.IsUsed)
            .Where(listing => request.MinYear == null || request.MinYear <= listing.Variant.Year)
            .Where(listing => request.MaxYear == null || request.MaxYear >= listing.Variant.Year)
            .Where(listing => request.MinPrice == null || request.MinPrice <= listing.Price)
            .Where(listing => request.MaxPrice == null || request.MaxPrice >= listing.Price)
            .Where(listing => request.MinMileage == null || request.MinMileage <= listing.Mileage)
            .Where(listing => request.MaxMileage == null || request.MaxMileage >= listing.Mileage)
            .Where(listing => request.MinPower == null || request.MinPower <= listing.Variant.PowerKw)
            .Where(listing => request.MaxPower == null || request.MaxPower >= listing.Variant.PowerKw)
            .ToListAsync(cancellationToken);

        List<GetAllListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetAllListingsResponse>(listing);
            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage != null)
            {
                mappedListing.Thumbnail = new Automotive.Marketplace.Application.Models.ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey),
                    AltText = firstImage.AltText
                };
            }
            if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            {
                mappedListing.IsLiked = listing.LikeUsers.Any(u => u.Id == request.UserId.Value);
            }
            response.Add(mappedListing);
        }

        return response;
    }
}
