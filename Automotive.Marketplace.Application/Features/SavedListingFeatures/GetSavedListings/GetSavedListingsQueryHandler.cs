using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public class GetSavedListingsQueryHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<GetSavedListingsQuery, IEnumerable<GetSavedListingsResponse>>
{
    public async Task<IEnumerable<GetSavedListingsResponse>> Handle(
        GetSavedListingsQuery request,
        CancellationToken cancellationToken)
    {
        var likes = await repository
            .AsQueryable<UserListingLike>()
            .Where(like => like.UserId == request.UserId)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Model)
                        .ThenInclude(model => model.Make)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Fuel)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Transmission)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Images)
            .ToListAsync(cancellationToken);

        var listingIds = likes.Select(l => l.ListingId).ToList();

        var notes = await repository
            .AsQueryable<UserListingNote>()
            .Where(note => note.UserId == request.UserId && listingIds.Contains(note.ListingId))
            .ToDictionaryAsync(note => note.ListingId, note => note.Content, cancellationToken);

        var result = new List<GetSavedListingsResponse>();

        foreach (var like in likes)
        {
            var listing = like.Listing;
            var variant = listing.Variant;

            string? thumbnailUrl = null;
            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage is not null)
            {
                thumbnailUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey);
            }

            result.Add(new GetSavedListingsResponse
            {
                ListingId = listing.Id,
                Title = $"{variant.Year} {variant.Model.Make.Name} {variant.Model.Name}",
                Price = listing.Price,
                City = listing.City,
                Mileage = listing.Mileage,
                FuelName = variant.Fuel.Name,
                TransmissionName = variant.Transmission.Name,
                Thumbnail = thumbnailUrl is not null
                    ? new Application.Models.ImageDto { Url = thumbnailUrl, AltText = firstImage!.AltText }
                    : null,
                NoteContent = notes.GetValueOrDefault(listing.Id)
            });
        }

        return result;
    }
}
