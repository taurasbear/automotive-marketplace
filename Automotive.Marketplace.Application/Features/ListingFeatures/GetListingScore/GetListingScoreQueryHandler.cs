using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQueryHandler(IRepository repository, ICardogApiClient cardogClient, IServiceScopeFactory scopeFactory)
    : IRequestHandler<GetListingScoreQuery, GetListingScoreResponse>
{
    private const int EfficiencyCacheDays = 30;
    private const int MarketCacheDays = 1;
    private const int ReliabilityCacheDays = 7;

    public async Task<GetListingScoreResponse> Handle(GetListingScoreQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Defects)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var year = listing.Year;

        ScoreWeights? weights = null;
        var isPersonalized = false;

        if (request.UserId.HasValue)
        {
            var prefs = await repository.AsQueryable<UserPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId.Value, cancellationToken);

            if (prefs != null)
            {
                weights = new ScoreWeights(prefs.ValueWeight, prefs.EfficiencyWeight, prefs.ReliabilityWeight, prefs.MileageWeight, prefs.ConditionWeight);
                isPersonalized = true;
            }
        }

        var efficiencyTask = GetEfficiencyAsync(make, model, year, cancellationToken);
        var marketTask = GetMarketAsync(make, model, year, cancellationToken);
        var reliabilityTask = GetReliabilityAsync(make, model, year, cancellationToken);

        await Task.WhenAll(efficiencyTask, marketTask, reliabilityTask);

        var efficiency = efficiencyTask.Result;
        var market = marketTask.Result;
        var reliability = reliabilityTask.Result;

        var scoreResult = ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, listing.Defects.Count, market, efficiency, reliability, weights);
        return new GetListingScoreResponse
        {
            OverallScore = scoreResult.OverallScore,
            Value = scoreResult.Value,
            Efficiency = scoreResult.Efficiency,
            Reliability = scoreResult.Reliability,
            Mileage = scoreResult.Mileage,
            Condition = scoreResult.Condition,
            HasMissingFactors = scoreResult.HasMissingFactors,
            MissingFactors = scoreResult.MissingFactors,
            IsPersonalized = isPersonalized,
        };
    }

    private async Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

        var cache = await repo.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogEfficiencyResult(cache.LitersPer100Km, cache.KWhPer100Km);

        var result = await cardogClient.GetEfficiencyAsync(make, model, year, ct);
        if (result != null)
            await UpsertEfficiencyCacheAsync(repo, make, model, year, result, ct);

        return result;
    }

    private async Task<CardogMarketResult?> GetMarketAsync(string make, string model, int year, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

        var cache = await repo.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogMarketResult(cache.MedianPrice, cache.TotalListings);

        var result = await cardogClient.GetMarketOverviewAsync(make, model, year, ct);
        if (result != null)
            await UpsertMarketCacheAsync(repo, make, model, year, result, ct);

        return result;
    }

    private async Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

        var cache = await repo.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogReliabilityResult(cache.RecallCount, cache.ComplaintCrashes, cache.ComplaintInjuries);

        var result = await cardogClient.GetReliabilityAsync(make, model, year, ct);
        if (result != null)
            await UpsertReliabilityCacheAsync(repo, make, model, year, result, ct);

        return result;
    }

    private static async Task UpsertEfficiencyCacheAsync(IRepository repo, string make, string model, int year, CardogEfficiencyResult result, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var existing = await repo.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.LitersPer100Km = result.LitersPer100Km;
            existing.KWhPer100Km = result.KWhPer100Km;
            existing.FetchedAt = now;
            existing.ExpiresAt = now.AddDays(EfficiencyCacheDays);
            await repo.UpdateAsync(existing, ct);
        }
        else
        {
            try
            {
                await repo.CreateAsync(new VehicleEfficiencyCache
                {
                    Id = Guid.NewGuid(),
                    Make = make,
                    Model = model,
                    Year = year,
                    LitersPer100Km = result.LitersPer100Km,
                    KWhPer100Km = result.KWhPer100Km,
                    FetchedAt = now,
                    ExpiresAt = now.AddDays(EfficiencyCacheDays),
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert by another request — ignore, data is equivalent
            }
        }
    }

    private static async Task UpsertMarketCacheAsync(IRepository repo, string make, string model, int year, CardogMarketResult result, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var existing = await repo.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.MedianPrice = result.MedianPrice;
            existing.TotalListings = result.TotalListings;
            existing.FetchedAt = now;
            existing.ExpiresAt = now.AddDays(MarketCacheDays);
            await repo.UpdateAsync(existing, ct);
        }
        else
        {
            try
            {
                await repo.CreateAsync(new VehicleMarketCache
                {
                    Id = Guid.NewGuid(),
                    Make = make,
                    Model = model,
                    Year = year,
                    MedianPrice = result.MedianPrice,
                    TotalListings = result.TotalListings,
                    FetchedAt = now,
                    ExpiresAt = now.AddDays(MarketCacheDays),
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert by another request — ignore, data is equivalent
            }
        }
    }

    private static async Task UpsertReliabilityCacheAsync(IRepository repo, string make, string model, int year, CardogReliabilityResult result, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var existing = await repo.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.RecallCount = result.RecallCount;
            existing.ComplaintCrashes = result.ComplaintCrashes;
            existing.ComplaintInjuries = result.ComplaintInjuries;
            existing.FetchedAt = now;
            existing.ExpiresAt = now.AddDays(ReliabilityCacheDays);
            await repo.UpdateAsync(existing, ct);
        }
        else
        {
            try
            {
                await repo.CreateAsync(new VehicleReliabilityCache
                {
                    Id = Guid.NewGuid(),
                    Make = make,
                    Model = model,
                    Year = year,
                    RecallCount = result.RecallCount,
                    ComplaintCrashes = result.ComplaintCrashes,
                    ComplaintInjuries = result.ComplaintInjuries,
                    FetchedAt = now,
                    ExpiresAt = now.AddDays(ReliabilityCacheDays),
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert by another request — ignore, data is equivalent
            }
        }
    }
}
