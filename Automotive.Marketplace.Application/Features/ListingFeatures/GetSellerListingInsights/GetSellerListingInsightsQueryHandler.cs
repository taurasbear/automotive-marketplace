using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsQueryHandler(IRepository repository, ICardogApiClient cardogClient)
    : IRequestHandler<GetSellerListingInsightsQuery, GetSellerListingInsightsResponse>
{
    private const int MarketCacheDays = 1;
    private const int MarketFailureCacheHours = 2;

    public async Task<GetSellerListingInsightsResponse> Handle(GetSellerListingInsightsQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        if (listing.SellerId != request.UserId)
            throw new UnauthorizedAccessException("Access denied. You are not the seller of this listing.");

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;

        var marketCache = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(
                c => c.Make == make && c.Model == model && c.Year == listing.Year && c.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (marketCache == null)
        {
            var prefs = await repository.AsQueryable<UserPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            if (prefs?.EnableVehicleScoring == true)
            {
                var result = await cardogClient.GetMarketOverviewAsync(make, model, listing.Year, cancellationToken);
                await UpsertMarketCacheAsync(repository, make, model, listing.Year, result, cancellationToken);

                if (result != null)
                    marketCache = new VehicleMarketCache { MedianPrice = result.MedianPrice, TotalListings = result.TotalListings };
            }
        }

        var marketPosition = BuildMarketPosition(listing, marketCache);
        var listingQuality = BuildListingQuality(listing);

        return new GetSellerListingInsightsResponse
        {
            MarketPosition = marketPosition,
            ListingQuality = listingQuality,
        };
    }

    private static MarketPositionInsight BuildMarketPosition(Listing listing, VehicleMarketCache? cache)
    {
        double? priceDiff = null;
        if (cache != null && cache.MedianPrice > 0)
            priceDiff = (double)((cache.MedianPrice - listing.Price) / cache.MedianPrice) * 100.0;

        return new MarketPositionInsight
        {
            ListingPrice = listing.Price,
            MarketMedianPrice = cache?.MedianPrice,
            PriceDifferencePercent = priceDiff,
            MarketListingCount = cache?.TotalListings,
            DaysListed = (int)(DateTime.UtcNow - listing.CreatedAt).TotalDays,
            HasMarketData = cache != null,
        };
    }

    private static async Task UpsertMarketCacheAsync(IRepository repo, string make, string model, int year, CardogMarketResult? result, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var isFailed = result is null;
        var expiry = isFailed ? now.AddHours(MarketFailureCacheHours) : now.AddDays(MarketCacheDays);

        var existing = await repo.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.MedianPrice = result?.MedianPrice ?? 0;
            existing.TotalListings = result?.TotalListings ?? 0;
            existing.IsFetchFailed = isFailed;
            existing.FetchedAt = now;
            existing.ExpiresAt = expiry;
            await repo.UpdateAsync(existing, ct);
        }
        else
        {
            try
            {
                await repo.CreateAsync(new VehicleMarketCache
                {
                    Id = Guid.NewGuid(),
                    Make = make, Model = model, Year = year,
                    MedianPrice = result?.MedianPrice ?? 0,
                    TotalListings = result?.TotalListings ?? 0,
                    IsFetchFailed = isFailed,
                    FetchedAt = now,
                    ExpiresAt = expiry,
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert — ignore
            }
        }
    }

    private static ListingQualityInsight BuildListingQuality(Listing listing)
    {
        var points = 0;
        var suggestions = new List<string>();
        var photoCount = listing.Images.Count;
        var hasDescription = !string.IsNullOrWhiteSpace(listing.Description) && listing.Description.Length >= 20;
        var hasPhotos = photoCount >= 3;
        var hasVin = !string.IsNullOrWhiteSpace(listing.Vin);
        var hasColour = !string.IsNullOrWhiteSpace(listing.Colour);

        if (hasDescription)
        {
            points += 20;
            if (listing.Description!.Length >= 100) points += 10;
        }
        else
        {
            suggestions.Add("Add a detailed description to attract more buyers.");
        }

        if (hasPhotos)
        {
            points += 20;
            if (photoCount >= 5) points += 10;
        }
        else
        {
            suggestions.Add("Add at least 3 photos to significantly improve visibility.");
        }

        if (hasVin) points += 15;
        else suggestions.Add("Include the VIN to build buyer confidence.");

        if (hasColour) points += 10;
        else suggestions.Add("Specify the colour to help buyers filter listings.");

        var qualityScore = (int)Math.Round(Math.Min(points, 90) / 90.0 * 100);

        return new ListingQualityInsight
        {
            QualityScore = qualityScore,
            HasDescription = hasDescription,
            HasPhotos = hasPhotos,
            PhotoCount = photoCount,
            HasVin = hasVin,
            HasColour = hasColour,
            Suggestions = suggestions,
        };
    }
}
