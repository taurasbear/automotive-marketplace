using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQueryHandler(IRepository repository, ICardogApiClient cardogClient)
    : IRequestHandler<GetListingScoreQuery, GetListingScoreResponse>
{
    public async Task<GetListingScoreResponse> Handle(GetListingScoreQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var year = listing.Year;

        var efficiency = await GetEfficiencyAsync(make, model, year, cancellationToken);
        var market = await GetMarketAsync(make, model, year, cancellationToken);
        var reliability = await GetReliabilityAsync(make, model, year, cancellationToken);

        return ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, market, efficiency, reliability);
    }

    private async Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogEfficiencyResult(cache.LitersPer100Km, cache.KWhPer100Km);

        var result = await cardogClient.GetEfficiencyAsync(make, model, year, ct);
        if (result != null)
            await UpsertEfficiencyCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task<CardogMarketResult?> GetMarketAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogMarketResult(cache.MedianPrice, cache.TotalListings);

        var result = await cardogClient.GetMarketOverviewAsync(make, model, year, ct);
        if (result != null)
            await UpsertMarketCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogReliabilityResult(cache.RecallCount, cache.ComplaintCrashes, cache.ComplaintInjuries);

        var result = await cardogClient.GetReliabilityAsync(make, model, year, ct);
        if (result != null)
            await UpsertReliabilityCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task UpsertEfficiencyCacheAsync(string make, string model, int year, CardogEfficiencyResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.LitersPer100Km = result.LitersPer100Km;
            existing.KWhPer100Km = result.KWhPer100Km;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(30);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleEfficiencyCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                LitersPer100Km = result.LitersPer100Km,
                KWhPer100Km = result.KWhPer100Km,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }

    private async Task UpsertMarketCacheAsync(string make, string model, int year, CardogMarketResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.MedianPrice = result.MedianPrice;
            existing.TotalListings = result.TotalListings;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddHours(24);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleMarketCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                MedianPrice = result.MedianPrice,
                TotalListings = result.TotalListings,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
            }, ct);
        }
    }

    private async Task UpsertReliabilityCacheAsync(string make, string model, int year, CardogReliabilityResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.RecallCount = result.RecallCount;
            existing.ComplaintCrashes = result.ComplaintCrashes;
            existing.ComplaintInjuries = result.ComplaintInjuries;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleReliabilityCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                RecallCount = result.RecallCount,
                ComplaintCrashes = result.ComplaintCrashes,
                ComplaintInjuries = result.ComplaintInjuries,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }, ct);
        }
    }
}
