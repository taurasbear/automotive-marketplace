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
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        // Count only conversations that have at least one message (exclude ghost conversations
        // created when a buyer opened the chat panel but never sent anything)
        var listingIds = listings.Select(l => l.Id).ToList();
        var conversationCounts = await repository
            .AsQueryable<Conversation>()
            .Where(c => listingIds.Contains(c.ListingId) && c.Messages.Any())
            .GroupBy(c => c.ListingId)
            .Select(g => new { ListingId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var countByListingId = conversationCounts.ToDictionary(x => x.ListingId, x => x.Count);

        List<GetMyListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetMyListingsResponse>(listing);

            var nonDefectImages = listing.Images.Where(i => i.ListingDefectId == null).ToList();

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

            // Thumbnail reuses the already-signed first image — no second S3 call
            mappedListing.Thumbnail = images.FirstOrDefault();

            // Counts
            mappedListing.ImageCount = listing.Images.Count;
            mappedListing.DefectCount = listing.Defects.Count;
            mappedListing.LikeCount = listing.Likes.Count;
            mappedListing.ConversationCount = countByListingId.GetValueOrDefault(listing.Id, 0);

            response.Add(mappedListing);
        }

        return response;
    }
}