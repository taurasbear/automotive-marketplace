using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQueryHandler(
    IRepository repository,
    ICardogApiClient cardogClient,
    IFuelEconomyApiClient fuelEconomyClient,
    INhtsaApiClient nhtsaClient,
    IServiceScopeFactory scopeFactory)
    : IRequestHandler<GetListingScoreQuery, GetListingScoreResponse>
{
    private const int EfficiencyCacheDays = 30;
    private const int MarketCacheDays = 1;

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
        var recallsTask = nhtsaClient.GetRecallsAsync(make, model, year, cancellationToken);
        var complaintsTask = nhtsaClient.GetComplaintsAsync(make, model, year, cancellationToken);
        var safetyRatingTask = nhtsaClient.GetSafetyRatingAsync(make, model, year, cancellationToken);

        await Task.WhenAll(efficiencyTask, marketTask, recallsTask, complaintsTask, safetyRatingTask);

        var scoreResult = ListingScoreCalculator.Calculate(
            listing.Price, year, listing.Mileage, listing.Defects.Count,
            marketTask.Result,
            efficiencyTask.Result,
            recallsTask.Result,
            complaintsTask.Result,
            safetyRatingTask.Result,
            weights);

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

    private async Task<FuelEconomyEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

        var cache = await repo.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new FuelEconomyEfficiencyResult(cache.LitersPer100Km, cache.KWhPer100Km);

        var result = await fuelEconomyClient.GetFuelEfficiencyAsync(make, model, year, ct);
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

    private static async Task UpsertEfficiencyCacheAsync(IRepository repo, string make, string model, int year, FuelEconomyEfficiencyResult result, CancellationToken ct)
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
}
