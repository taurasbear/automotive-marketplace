using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryQueryHandler(IRepository repository, IOpenAiClient openAiClient)
    : IRequestHandler<GetListingAiSummaryQuery, GetListingAiSummaryResponse>
{
    private const string SummaryType = "buyer";

    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        ["lt"] = "Lithuanian",
        ["en"] = "English",
    };

    public async Task<GetListingAiSummaryResponse> Handle(GetListingAiSummaryQuery request, CancellationToken cancellationToken)
    {
        var lang = request.Language ?? "lt";

        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Defects)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        var cache = await repository.AsQueryable<ListingAiSummaryCache>()
            .FirstOrDefaultAsync(
                c => c.ListingId == request.ListingId && c.SummaryType == SummaryType
                     && c.ComparisonListingId == null && c.Language == lang,
                cancellationToken);

        if (cache != null && cache.ExpiresAt > DateTime.UtcNow)
        {
            var listingModifiedAt = listing.ModifiedAt ?? listing.CreatedAt;
            if (cache.GeneratedAt >= listingModifiedAt)
                return new GetListingAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
        }

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var year = listing.Year;

        var market = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var efficiency = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
        var reliability = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, cancellationToken);

        var unavailableFactors = new List<string>();
        if (market == null) unavailableFactors.Add("MarketValue");
        if (efficiency == null) unavailableFactors.Add("Efficiency");
        if (reliability == null) unavailableFactors.Add("Reliability");

        var prompt = BuildBuyerPrompt(listing, market, efficiency, reliability, unavailableFactors, lang);
        var summary = await openAiClient.GetResponseAsync(prompt, cancellationToken);

        if (summary is null)
            return new GetListingAiSummaryResponse { IsGenerated = false, UnavailableFactors = unavailableFactors };

        await UpsertCacheAsync(request.ListingId, lang, summary, cache, cancellationToken);

        return new GetListingAiSummaryResponse { Summary = summary, IsGenerated = true, FromCache = false, UnavailableFactors = unavailableFactors };
    }

    private static string BuildBuyerPrompt(Listing listing, VehicleMarketCache? market,
        VehicleEfficiencyCache? efficiency, VehicleReliabilityCache? reliability,
        List<string> unavailableFactors, string lang)
    {
        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var fuel = listing.Variant.Fuel.Name;
        var defects = listing.Defects?.Count > 0
            ? string.Join(", ", listing.Defects.Select(d => d.CustomName ?? "unnamed defect"))
            : "none";
        var languageName = LanguageNames.GetValueOrDefault(lang, "Lithuanian");

        var lines = new List<string>
        {
            "You are an automotive assistant. Provide a brief, neutral buyer verdict in 2-3 sentences for this vehicle listing.",
            $"Vehicle: {listing.Year} {make} {model}",
            $"Listed price: {listing.Price:0} EUR | Mileage: {listing.Mileage:N0} km | Fuel: {fuel} | Power: {listing.Variant.PowerKw} kW",
        };

        if (market != null)
            lines.Add($"Market data: median price {market.MedianPrice:0} EUR across {market.TotalListings} listings.");
        if (efficiency != null)
        {
            if (efficiency.KWhPer100Km.HasValue)
                lines.Add($"Efficiency: {efficiency.KWhPer100Km.Value:F1} kWh/100km.");
            else if (efficiency.LitersPer100Km.HasValue)
                lines.Add($"Efficiency: {efficiency.LitersPer100Km.Value:F1} L/100km.");
        }
        if (reliability != null)
            lines.Add($"Reliability: {reliability.RecallCount} recalls, {reliability.ComplaintCrashes} crash complaints, {reliability.ComplaintInjuries} injury complaints.");

        lines.Add($"Seller-marked defects: {defects}.");

        if (unavailableFactors.Count > 0)
            lines.Add($"Note: the following data was unavailable and was not factored into your response: {string.Join(", ", unavailableFactors)}.");

        lines.Add("Be direct and practical. Focus on value, practicality, and any notable considerations.");
        lines.Add($"Respond in {languageName}.");

        return string.Join("\n", lines);
    }

    private async Task UpsertCacheAsync(Guid listingId, string language, string summary, ListingAiSummaryCache? existing, CancellationToken ct)
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
                ListingId = listingId,
                SummaryType = SummaryType,
                ComparisonListingId = null,
                Language = language,
                Summary = summary,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }
}
