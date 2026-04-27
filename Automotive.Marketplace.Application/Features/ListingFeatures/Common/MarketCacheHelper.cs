using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.Common;

internal static class MarketCacheHelper
{
    internal const int MarketCacheDays = 1;
    internal const int MarketFailureCacheHours = 2;

    internal static async Task UpsertMarketCacheAsync(
        IRepository repo,
        string make, string model, int year,
        CardogMarketResult? result,
        CancellationToken ct)
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
}
