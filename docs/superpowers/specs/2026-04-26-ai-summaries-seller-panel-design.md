# Spec 3: AI Summaries + Seller Insight Panel

**Date:** 2026-04-26  
**Status:** Approved for implementation  
**Depends on:** Spec 1 (score engine + CarDog caches) must be complete first  
**Partial dependency on Spec 2:** `AutoGenerateAiSummary` preference stored in `UserPreferences` (added in Spec 2). If Spec 2 is not yet complete, this setting defaults to `false` for all users.

## Problem & Approach

Two additions in this spec:

1. **AI buyer verdicts** — OpenAI `gpt-5.4-mini` generates a 3–5 sentence buyer verdict for single listings and a comparison summary for two listings side-by-side. Generated on demand by default (button click); optionally auto-generated based on a user setting.

2. **Seller insight panel** — Sellers viewing their own listing see two cards: market position (price vs. CarDog market average) and listing quality (completeness checklist score).

---

## Backend

### New Infrastructure: `OpenAiClient`

New service implementing `IOpenAiClient` (Application interface).

```csharp
Task<string> GetCompletionAsync(string prompt, CancellationToken ct);
```

- Endpoint: `POST https://api.openai.com/v1/responses`
- Model: `gpt-5.4-mini`
- Auth: `Authorization: Bearer {OpenAiApiKey}` (from config)
- Max tokens: 300
- Temperature: 0.4 (low variation for factual summaries)

### New DB Table: `ListingAiSummaryCache`

```
Id (Guid PK), ListingId (Guid FK unique),
SummaryType (enum: "single" | "comparison"), 
ComparisonListingId (Guid?, nullable — populated for comparison summaries),
SummaryText (string), GeneratedAt (DateTimeOffset), ExpiresAt (DateTimeOffset)
```

TTL: 24 hours. Unique index on `(ListingId, SummaryType, ComparisonListingId)` — composite key to allow both a single-listing and a comparison summary to coexist for the same `ListingId`.

Invalidated if the listing's `UpdatedAt` changes. **Note:** The `Listing` entity must have an `UpdatedAt` (DateTimeOffset) column — add a migration if not already present.

For comparison summaries: `ListingId` = the first listing (lower GUID), `ComparisonListingId` = the second. Lookup always normalizes the pair (sorted by GUID) to avoid duplicate cache entries.

### Listing AI Summary Handler

**`GetListingAiSummaryQuery { Guid ListingId }`**

1. Check `ListingAiSummaryCache` for a non-expired `single` entry for `ListingId`. If found and `GeneratedAt > listing.UpdatedAt`, return cached text.
2. Build prompt from: listing fields (make, model, year, price, mileage, power kW, fuel type, drivetrain, body type, description) + available CarDog data (market avg, L/100km, recall count, complaint count) + score breakdown.
3. Send to `gpt-5.4-mini`. Receive a 3–5 sentence buyer verdict.
4. Store in cache. Return text.

**Prompt structure:**
```
You are a car-buying advisor. Given the following vehicle listing data, write a concise buyer verdict (3-5 sentences). Cover: whether the price is fair vs. market average, fuel efficiency compared to similar vehicles, any reliability concerns, and who this car suits best. Be factual and direct. Do not use lists or headers.

Listing: {year} {make} {model} — {price} — {mileage} km — {powerKw}kW — {fuelType} — {bodyType}
Market average: {marketAvg} (based on {listingCount} comparable listings)
Fuel efficiency: {L100km} L/100km
Reliability: {recallCount} recalls, {complaintCount} NHTSA complaints ({complaintsWithCrashes} crashes, {complaintsWithInjuries} injuries)
Score: {overall}/100 (Value: {value}, Efficiency: {efficiency}, Reliability: {reliability}, Mileage: {mileage})
[If any data is missing, omit that sentence and note data was unavailable]
```

**Endpoint:** `GET /api/listings/{id}/ai-summary`

### Comparison AI Summary Handler

**`GetListingComparisonAiSummaryQuery { Guid ListingAId, Guid ListingBId }`**

Same pattern — builds a comparison prompt with both listings' data, asks for a 3–5 sentence verdict: which wins on value, efficiency, reliability; overall recommendation.

**Endpoint:** `GET /api/listings/comparison/{a}/{b}/ai-summary`

### Seller Insights Handler

**`GetSellerListingInsightsQuery { Guid ListingId }`** — only accessible to the listing's owner (authorization check).

Returns:
```csharp
record SellerInsightsResponse(
    MarketPositionResult MarketPosition,
    ListingQualityResult ListingQuality
);
record MarketPositionResult(decimal ListingPrice, decimal MarketAvgPrice, decimal MarketMedianPrice, decimal PriceDiffPct, string Category); // Category: "Well Below Market" | "Below Market" | "At Market" | "Above Market"
record ListingQualityResult(int Score, int MaxScore, IEnumerable<QualityCheckItem> Checks);
record QualityCheckItem(string Label, bool Passed, string? Tip);
```

**Listing quality checks:**

| Check | Points | Tip if failing |
|---|---|---|
| Has at least 3 photos | 1 | "Add more photos to attract buyers" |
| Has at least 6 photos | 1 | "Listings with 6+ photos get more views" |
| VIN provided | 1 | "Adding a VIN builds buyer trust" |
| Description ≥ 50 words | 1 | "Add more detail to your description" |
| Municipality set | 1 | "Buyers filter by location — set your municipality" |

Total: 5 points. MarketPosition uses CarDog `VehicleMarketCache` (fetched/cached per Spec 1 logic).

**Endpoint:** `GET /api/listings/{id}/seller-insights` (requires authentication + listing ownership)

---

## Frontend

### AI Summary on Listing Details Page

Below the listing description (or in a separate "AI Insight" collapsible section). Default state: shows a `Sparkles` Lucide icon + `"Generate AI insight"` button. On click → fires `GET /api/listings/{id}/ai-summary` → shows loading spinner → renders summary text in a styled paragraph.

If user's `AutoGenerateAiSummary = true` (from Spec 2 preferences): the query fires automatically on page load. No button — shows spinner then text.

### AI Summary in Comparison Banner

The sticky comparison banner (from Spec 1) gains a second row below the scores. Default: `Sparkles` icon + `"Get AI comparison"` button. On click → fires comparison summary endpoint → renders 3–5 sentence text inline in the banner.

If `AutoGenerateAiSummary = true`: auto-fetches with the page.

### Seller Insight Panel (My Listings page / Listing Details as owner)

Shown only when the authenticated user is the listing's seller. Two cards side-by-side (responsive: stacks on mobile):

**Card 1 — Market Position:**
- Header: "Market Position" (with `TrendingUp` Lucide icon)
- Main metric: price difference percentage (e.g., `↓ 8% below market`) in green/amber/red
- Sub-lines: "Your price: €X" / "Market average: €Y (N listings)"
- Category badge: colored pill ("Competitive", "At Market", "Above Market")

**Card 2 — Listing Quality:**
- Header: "Listing Quality" (with `ClipboardCheck` Lucide icon)
- Score fraction: `4/5`
- Checklist: each item with `CheckCircle` (green) or `XCircle` (red) icon + label
- Failed items show tip text in amber

### User Setting: Auto-generate AI Summary

In the user settings page (alongside quiz sliders from Spec 2): a toggle switch labelled `"Auto-generate AI insights"` with description `"Automatically generate AI buyer verdicts when viewing listings. Counts against your usage."`. Default off. Saved via `PUT /api/users/me/preferences`.

---

## What's NOT in this spec

- AI-generated listing descriptions for sellers (not requested; can be added later)
- OpenAI streaming responses (batch response sufficient for this use case)
- Saving/sharing AI summaries
