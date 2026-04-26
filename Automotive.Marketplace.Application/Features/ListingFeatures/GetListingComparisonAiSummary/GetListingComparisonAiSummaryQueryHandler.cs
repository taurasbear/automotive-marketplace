using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryQueryHandler(IRepository repository, IOpenAiClient openAiClient)
    : IRequestHandler<GetListingComparisonAiSummaryQuery, GetListingComparisonAiSummaryResponse>
{
    private const string SummaryType = "comparison";

    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        ["lt"] = "Lithuanian",
        ["en"] = "English",
    };

    public async Task<GetListingComparisonAiSummaryResponse> Handle(GetListingComparisonAiSummaryQuery request, CancellationToken cancellationToken)
    {
        var lang = request.Language ?? "lt";
        var keyId = request.ListingAId < request.ListingBId ? request.ListingAId : request.ListingBId;
        var compId = request.ListingAId < request.ListingBId ? request.ListingBId : request.ListingAId;

        var listingA = await LoadListingAsync(request.ListingAId, cancellationToken);
        var listingB = await LoadListingAsync(request.ListingBId, cancellationToken);

        var cache = await repository.AsQueryable<ListingAiSummaryCache>()
            .FirstOrDefaultAsync(
                c => c.ListingId == keyId && c.ComparisonListingId == compId
                     && c.SummaryType == SummaryType && c.Language == lang,
                cancellationToken);

        if (cache != null && cache.ExpiresAt > DateTime.UtcNow)
        {
            var aModified = listingA.ModifiedAt ?? listingA.CreatedAt;
            var bModified = listingB.ModifiedAt ?? listingB.CreatedAt;
            if (cache.GeneratedAt >= aModified && cache.GeneratedAt >= bModified)
                return new GetListingComparisonAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
        }

        var makeA = listingA.Variant.Model.Make.Name;
        var modelA = listingA.Variant.Model.Name;
        var yearA = listingA.Year;
        var makeB = listingB.Variant.Model.Make.Name;
        var modelB = listingB.Variant.Model.Name;
        var yearB = listingB.Year;

        var marketA = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var efficiencyA = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var reliabilityA = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);

        var marketB = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == makeB && c.Model == modelB && c.Year == yearB && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var efficiencyB = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == makeB && c.Model == modelB && c.Year == yearB && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var reliabilityB = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == makeB && c.Model == modelB && c.Year == yearB && c.ExpiresAt > DateTime.UtcNow, cancellationToken);

        var unavailableFactors = new List<string>();
        if (marketA == null || marketB == null) unavailableFactors.Add("MarketValue");
        if (efficiencyA == null || efficiencyB == null) unavailableFactors.Add("Efficiency");
        if (reliabilityA == null || reliabilityB == null) unavailableFactors.Add("Reliability");

        var prompt = BuildComparisonPrompt(listingA, listingB, marketA, marketB, efficiencyA, efficiencyB, reliabilityA, reliabilityB, unavailableFactors, lang);
        var summary = await openAiClient.GetResponseAsync(prompt, cancellationToken);

        if (summary is null)
            return new GetListingComparisonAiSummaryResponse { IsGenerated = false, UnavailableFactors = unavailableFactors };

        await UpsertCacheAsync(keyId, compId, lang, summary, cache, cancellationToken);

        return new GetListingComparisonAiSummaryResponse { Summary = summary, IsGenerated = true, FromCache = false, UnavailableFactors = unavailableFactors };
    }

    private async Task<Listing> LoadListingAsync(Guid id, CancellationToken ct) =>
        await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Defects)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
        ?? throw new DbEntityNotFoundException("Listing", id);

    private static string BuildComparisonPrompt(Listing a, Listing b,
        VehicleMarketCache? marketA, VehicleMarketCache? marketB,
        VehicleEfficiencyCache? efficiencyA, VehicleEfficiencyCache? efficiencyB,
        VehicleReliabilityCache? reliabilityA, VehicleReliabilityCache? reliabilityB,
        List<string> unavailableFactors, string lang)
    {
        var makeA = a.Variant.Model.Make.Name;
        var modelA = a.Variant.Model.Name;
        var makeB = b.Variant.Model.Make.Name;
        var modelB = b.Variant.Model.Name;
        var defectsA = a.Defects?.Count > 0
            ? string.Join(", ", a.Defects.Select(d => d.CustomName ?? "unnamed"))
            : "none";
        var defectsB = b.Defects?.Count > 0
            ? string.Join(", ", b.Defects.Select(d => d.CustomName ?? "unnamed"))
            : "none";
        var languageName = LanguageNames.GetValueOrDefault(lang, "Lithuanian");

        var lines = new List<string>
        {
            "You are an automotive assistant. Compare these two vehicle listings and recommend which is the better buy in 2-3 sentences.",
            $"Vehicle A: {a.Year} {makeA} {modelA} — {a.Price:0} EUR, {a.Mileage:N0} km, {a.Variant.Fuel.Name}, defects: {defectsA}",
            $"Vehicle B: {b.Year} {makeB} {modelB} — {b.Price:0} EUR, {b.Mileage:N0} km, {b.Variant.Fuel.Name}, defects: {defectsB}",
        };

        if (marketA != null)
            lines.Add($"Vehicle A market data: median price {marketA.MedianPrice:0} EUR across {marketA.TotalListings} listings.");
        if (marketB != null)
            lines.Add($"Vehicle B market data: median price {marketB.MedianPrice:0} EUR across {marketB.TotalListings} listings.");
        if (efficiencyA != null)
        {
            if (efficiencyA.KWhPer100Km.HasValue) lines.Add($"Vehicle A efficiency: {efficiencyA.KWhPer100Km.Value:F1} kWh/100km.");
            else if (efficiencyA.LitersPer100Km.HasValue) lines.Add($"Vehicle A efficiency: {efficiencyA.LitersPer100Km.Value:F1} L/100km.");
        }
        if (efficiencyB != null)
        {
            if (efficiencyB.KWhPer100Km.HasValue) lines.Add($"Vehicle B efficiency: {efficiencyB.KWhPer100Km.Value:F1} kWh/100km.");
            else if (efficiencyB.LitersPer100Km.HasValue) lines.Add($"Vehicle B efficiency: {efficiencyB.LitersPer100Km.Value:F1} L/100km.");
        }
        if (reliabilityA != null)
            lines.Add($"Vehicle A reliability: {reliabilityA.RecallCount} recalls, {reliabilityA.ComplaintCrashes} crash complaints, {reliabilityA.ComplaintInjuries} injury complaints.");
        if (reliabilityB != null)
            lines.Add($"Vehicle B reliability: {reliabilityB.RecallCount} recalls, {reliabilityB.ComplaintCrashes} crash complaints, {reliabilityB.ComplaintInjuries} injury complaints.");

        if (unavailableFactors.Count > 0)
            lines.Add($"Note: the following data was unavailable: {string.Join(", ", unavailableFactors)}.");

        lines.Add("Give a direct recommendation with the main reason. Be concise.");
        lines.Add($"Respond in {languageName}.");

        return string.Join("\n", lines);
    }

    private async Task UpsertCacheAsync(Guid keyId, Guid compId, string language, string summary, ListingAiSummaryCache? existing, CancellationToken ct)
    {
        if (existing != null)
        {
            existing.Summary = summary;
            existing.GeneratedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(30);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new ListingAiSummaryCache
            {
                Id = Guid.NewGuid(),
                ListingId = keyId,
                ComparisonListingId = compId,
                SummaryType = SummaryType,
                Language = language,
                Summary = summary,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }
}
