using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public class GetMyListingsQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<GetMyListingsQuery, IEnumerable<GetMyListingsResponse>>
{
    public async Task<IEnumerable<GetMyListingsResponse>> Handle(GetMyListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Images)
            .Include(l => l.Defects)
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        List<GetMyListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetMyListingsResponse>(listing);
            
            // Set thumbnail to first non-defect image
            var firstImage = listing.Images.Where(i => i.ListingDefectId == null).FirstOrDefault();
            if (firstImage != null)
            {
                mappedListing.Thumbnail = new Automotive.Marketplace.Application.Models.ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey),
                    AltText = firstImage.AltText
                };
            }
            
            // Set counts
            mappedListing.ImageCount = listing.Images.Count;
            mappedListing.DefectCount = listing.Defects.Count;
            
            response.Add(mappedListing);
        }

        return response;
    }
}