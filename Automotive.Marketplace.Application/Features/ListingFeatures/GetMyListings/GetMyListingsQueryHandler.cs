using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Application.Models;
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
            .Include(l => l.Variant)
                .ThenInclude(v => v.Fuel)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Transmission)
            .Include(l => l.Images)
            .Include(l => l.Defects)
            .Include(l => l.Likes)
            .Include(l => l.Conversations)
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        List<GetMyListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetMyListingsResponse>(listing);

            var nonDefectImages = listing.Images.Where(i => i.ListingDefectId == null).ToList();

            // Thumbnail
            var firstImage = nonDefectImages.FirstOrDefault();
            if (firstImage != null)
            {
                mappedListing.Thumbnail = new ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey),
                    AltText = firstImage.AltText
                };
            }

            // All non-defect images for hover gallery
            var images = new List<ImageDto>();
            foreach (var image in nonDefectImages)
            {
                images.Add(new ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                    AltText = image.AltText
                });
            }
            mappedListing.Images = images;

            // Counts
            mappedListing.ImageCount = listing.Images.Count;
            mappedListing.DefectCount = listing.Defects.Count;
            mappedListing.LikeCount = listing.Likes.Count;
            mappedListing.ConversationCount = listing.Conversations.Count;

            response.Add(mappedListing);
        }

        return response;
    }
}