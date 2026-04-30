using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.Common;
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
    INhtsaApiClient nhtsaClient,
    IFuelEconomyApiClient fuelEconomyClient,
    IServiceScopeFactory scopeFactory)
    : IRequestHandler<GetListingScoreQuery, GetListingScoreResponse>
{
    private const int EfficiencyCacheDays = 90;
    private const int ReliabilityCacheDays = 30;

    private record NhtsaReliabilityData(
        NhtsaRecallsResult? Recalls,
        NhtsaComplaintsResult? Complaints,
        NhtsaSafetyRatingResult? SafetyRating);

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
        var reliabilityTask = GetReliabilityDataAsync(make, model, year, cancellationToken);

        await Task.WhenAll(efficiencyTask, marketTask, reliabilityTask);

        var efficiency = efficiencyTask.Result;
        var market = marketTask.Result;
        var reliability = reliabilityTask.Result;

        var scoreResult = ListingScoreCalculator.Calculate(
            listing.Price, year, listing.Mileage, listing.Defects.Count,
            market, efficiency,
            reliability.Recalls, reliability.Complaints, reliability.SafetyRating,
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
            return cache.IsFetchFailed ? null : new CardogMarketResult(cache.MedianPrice, cache.TotalListings);

        var result = await cardogClient.GetMarketOverviewAsync(make, model, year, ct);
        await MarketCacheHelper.UpsertMarketCacheAsync(repo, make, model, year, result, ct);
        return result;
    }

    private async Task<NhtsaReliabilityData> GetReliabilityDataAsync(string make, string model, int year, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

        var cache = await repo.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
        {
            return new NhtsaReliabilityData(
                new NhtsaRecallsResult(cache.RecallCount),
                new NhtsaComplaintsResult(cache.ComplaintCount),
                cache.OverallSafetyRating.HasValue ? new NhtsaSafetyRatingResult(cache.OverallSafetyRating.Value) : null);
        }

        var recallsTask = nhtsaClient.GetRecallsAsync(make, model, year, ct);
        var complaintsTask = nhtsaClient.GetComplaintsAsync(make, model, year, ct);
        var safetyRatingTask = nhtsaClient.GetSafetyRatingAsync(make, model, year, ct);
        await Task.WhenAll(recallsTask, complaintsTask, safetyRatingTask);

        var data = new NhtsaReliabilityData(recallsTask.Result, complaintsTask.Result, safetyRatingTask.Result);
        await UpsertReliabilityCacheAsync(repo, make, model, year, data, ct);
        return data;
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
                    Make = make, Model = model, Year = year,
                    LitersPer100Km = result.LitersPer100Km,
                    KWhPer100Km = result.KWhPer100Km,
                    FetchedAt = now,
                    ExpiresAt = now.AddDays(EfficiencyCacheDays),
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert — data is equivalent
            }
        }
    }

    private static async Task UpsertReliabilityCacheAsync(IRepository repo, string make, string model, int year, NhtsaReliabilityData data, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var existing = await repo.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.RecallCount = data.Recalls?.RecallCount ?? 0;
            existing.ComplaintCount = data.Complaints?.ComplaintCount ?? 0;
            existing.OverallSafetyRating = data.SafetyRating?.OverallRating;
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
                    Make = make, Model = model, Year = year,
                    RecallCount = data.Recalls?.RecallCount ?? 0,
                    ComplaintCount = data.Complaints?.ComplaintCount ?? 0,
                    OverallSafetyRating = data.SafetyRating?.OverallRating,
                    FetchedAt = now,
                    ExpiresAt = now.AddDays(ReliabilityCacheDays),
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert — ignore
            }
        }
    }
}
