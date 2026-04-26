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

    public async Task<GetListingAiSummaryResponse> Handle(GetListingAiSummaryQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        var cache = await repository.AsQueryable<ListingAiSummaryCache>()
            .FirstOrDefaultAsync(
                c => c.ListingId == request.ListingId && c.SummaryType == SummaryType && c.ComparisonListingId == null,
                cancellationToken);

        if (cache != null && cache.ExpiresAt > DateTime.UtcNow)
        {
            var listingModifiedAt = listing.ModifiedAt ?? listing.CreatedAt;
            if (cache.GeneratedAt >= listingModifiedAt)
                return new GetListingAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
        }

        var prompt = BuildBuyerPrompt(listing);
        var summary = await openAiClient.GetResponseAsync(prompt, cancellationToken);

        if (summary is null)
            return new GetListingAiSummaryResponse { IsGenerated = false };

        await UpsertCacheAsync(request.ListingId, summary, cache, cancellationToken);

        return new GetListingAiSummaryResponse { Summary = summary, IsGenerated = true, FromCache = false };
    }

    private static string BuildBuyerPrompt(Listing listing)
    {
        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var fuel = listing.Variant.Fuel.Name;
        return $"""
            You are an automotive assistant. Provide a brief, neutral buyer verdict in 2-3 sentences for this vehicle listing.
            Vehicle: {listing.Year} {make} {model}
            Listed price: {listing.Price:0} EUR | Mileage: {listing.Mileage:N0} km | Fuel: {fuel} | Power: {listing.Variant.PowerKw} kW
            Be direct and practical. Focus on value, practicality, and any notable considerations.
            """;
    }

    private async Task UpsertCacheAsync(Guid listingId, string summary, ListingAiSummaryCache? existing, CancellationToken ct)
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
                Summary = summary,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }
}
