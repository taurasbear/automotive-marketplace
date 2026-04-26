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

    public async Task<GetListingComparisonAiSummaryResponse> Handle(GetListingComparisonAiSummaryQuery request, CancellationToken cancellationToken)
    {
        var keyId = request.ListingAId < request.ListingBId ? request.ListingAId : request.ListingBId;
        var compId = request.ListingAId < request.ListingBId ? request.ListingBId : request.ListingAId;

        var listingA = await LoadListingAsync(request.ListingAId, cancellationToken);
        var listingB = await LoadListingAsync(request.ListingBId, cancellationToken);

        var cache = await repository.AsQueryable<ListingAiSummaryCache>()
            .FirstOrDefaultAsync(
                c => c.ListingId == keyId && c.ComparisonListingId == compId && c.SummaryType == SummaryType,
                cancellationToken);

        if (cache != null && cache.ExpiresAt > DateTime.UtcNow)
        {
            var aModified = listingA.ModifiedAt ?? listingA.CreatedAt;
            var bModified = listingB.ModifiedAt ?? listingB.CreatedAt;
            if (cache.GeneratedAt >= aModified && cache.GeneratedAt >= bModified)
                return new GetListingComparisonAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
        }

        var prompt = BuildComparisonPrompt(listingA, listingB);
        var summary = await openAiClient.GetResponseAsync(prompt, cancellationToken);

        if (summary is null)
            return new GetListingComparisonAiSummaryResponse { IsGenerated = false };

        await UpsertCacheAsync(keyId, compId, summary, cache, cancellationToken);

        return new GetListingComparisonAiSummaryResponse { Summary = summary, IsGenerated = true, FromCache = false };
    }

    private async Task<Listing> LoadListingAsync(Guid id, CancellationToken ct) =>
        await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
        ?? throw new DbEntityNotFoundException("Listing", id);

    private static string BuildComparisonPrompt(Listing a, Listing b)
    {
        var makeA = a.Variant.Model.Make.Name;
        var modelA = a.Variant.Model.Name;
        var makeB = b.Variant.Model.Make.Name;
        var modelB = b.Variant.Model.Name;
        return $"""
            You are an automotive assistant. Compare these two vehicle listings and recommend which is the better buy in 2-3 sentences.
            Vehicle A: {a.Year} {makeA} {modelA} — {a.Price:0} EUR, {a.Mileage:N0} km, {a.Variant.Fuel.Name}
            Vehicle B: {b.Year} {makeB} {modelB} — {b.Price:0} EUR, {b.Mileage:N0} km, {b.Variant.Fuel.Name}
            Give a direct recommendation with the main reason. Be concise.
            """;
    }

    private async Task UpsertCacheAsync(Guid keyId, Guid compId, string summary, ListingAiSummaryCache? existing, CancellationToken ct)
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
                Summary = summary,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }
}
