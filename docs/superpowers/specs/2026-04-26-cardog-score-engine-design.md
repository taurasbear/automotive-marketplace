# Spec 1: CarDog Integration + Vehicle Score Engine

**Date:** 2026-04-26  
**Status:** Approved for implementation

## Problem & Approach

Listings currently have no external quality signal — buyers must manually assess whether a price is fair, whether fuel economy is good, or whether the model has a reliability track record. This spec adds a **vehicle score** (0–100) backed by CarDog API data, displayed on listing detail pages and in the comparison view.

CarDog API is limited to 50 requests/month. Data is cached in the DB by make/model/year so that all listings of the same model share a single cache entry.

---

## Backend

### New Infrastructure: `CardogApiClient`

A new infrastructure service alongside `VpicVehicleDataApiClient`. Implements a new application-layer interface `ICardogApiClient`.

**Interface methods:**
```csharp
Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, string? trim, CancellationToken ct);
Task<CardogMarketResult?> GetMarketOverviewAsync(string make, string model, int year, CancellationToken ct);
Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, string country, CancellationToken ct);
```

- Base URL: `https://api.cardog.io/v1`, API key from config (`CardogApiKey`)  
- Efficiency endpoint: `GET /v1/efficiency/:make/:model/:year/:trim` (falls back to all-trims if trim match fails)
- Market endpoint: `GET /v1/market/:make/:model/:year/overview`
- Reliability: two parallel calls — `GET /v1/recalls/:country/search` (country=`us`) + `GET /v1/complaints/search` — with `make`, `model`, `yearMin=year-1`, `yearMax=year+1` filters

**Result DTOs (Application models):**
```csharp
record CardogEfficiencyResult(decimal? CombinedLPer100Km, decimal? CombinedKwhPer100Km, decimal? Co2GramsPerKm, decimal? ElectricRangeKm, string PowertrainType);
record CardogMarketResult(decimal AvgPrice, decimal MedianPrice, int ListingCount);
record CardogReliabilityResult(int RecallCount, int ComplaintCount, int ComplaintsWithCrashes, int ComplaintsWithInjuries, string Country);
```

### New DB Tables (EF Core migrations)

**`VehicleEfficiencyCache`**
```
Id (Guid PK), Make, Model, Year (int), Trim (nullable string),
CombinedLPer100Km (decimal?), CombinedKwhPer100Km (decimal?),
Co2GramsPerKm (decimal?), ElectricRangeKm (decimal?), PowertrainType (string),
FetchedAt (DateTimeOffset), ExpiresAt (DateTimeOffset)
```
TTL: 30 days. Unique index on `(Make, Model, Year)`. The `Trim` column records which trim was matched by CarDog (for informational purposes only — it is not a cache key dimension; all trims for a make/model/year share one cache entry).

**`VehicleMarketCache`**
```
Id (Guid PK), Make, Model, Year (int),
AvgPrice (decimal), MedianPrice (decimal), ListingCount (int),
FetchedAt (DateTimeOffset), ExpiresAt (DateTimeOffset)
```
TTL: 24 hours. Unique index on `(Make, Model, Year)`.

**`VehicleReliabilityCache`**
```
Id (Guid PK), Make, Model, Country (string, "us"),
RecallCount (int), ComplaintCount (int), ComplaintsWithCrashes (int), ComplaintsWithInjuries (int),
FetchedAt (DateTimeOffset), ExpiresAt (DateTimeOffset)
```
TTL: 7 days. Unique index on `(Make, Model, Country)`.

### New Application Service: `IVehicleInsightsService`

Orchestrates cache checking, CarDog API calls, and score calculation. Lives in Application layer.

```csharp
Task<VehicleInsightsResult> GetInsightsAsync(Guid listingId, CancellationToken ct);
```

`VehicleInsightsResult` contains:
- `EfficiencyData`: nullable, from cache or fresh fetch
- `MarketData`: nullable
- `ReliabilityData`: nullable
- Pre-populated `ScoreResult` (computed immediately from available data)

**Cache-or-fetch logic** (per data type):
1. Check DB for a non-expired record matching `(make, model, year)`
2. If found: return cached data
3. If not: call CarDog, store to DB, return fresh data
4. If CarDog call fails (error, quota exceeded): return `null` for that factor

### Score Calculation (pure function, no async)

`ListingScoreCalculator` in Application layer. Accepts listing fields + nullable CarDog results + optional user weight overrides. Returns `ScoreResult`.

**Default weights:** Value 30%, Efficiency 25%, Reliability 25%, Mileage 20%.

**Factor formulas:**

| Factor | Formula |
|---|---|
| Value (0–100) | `clamp((medianPrice - listingPrice) / medianPrice, -0.5, +0.3)` → scaled to 0–100. Score 50 = at market, 100 = 30%+ below, 0 = 50%+ above |
| Efficiency ICE | `clamp((12 - L/100km) / 6, 0, 1) × 100`. 6 L/100km = 100, 12 L/100km = 0 |
| Efficiency EV | `clamp((25 - kWh/100km) / 10, 0, 1) × 100`. 15 kWh = 100, 25 kWh = 0 |
| Reliability | `100 - clamp((recalls×2 + crashes×3 + injuries×2) / 50, 0, 1) × 100` |
| Mileage | `clamp((ageYears×15000 - mileage) / (ageYears×15000), 0, 1) × 100`. 15,000 km/year benchmark |

Each factor returns a `FactorScore { Value: int, Status: "scored" | "missing" }`. If CarDog data is null → `Status = "missing"`, `Value = 50` (neutral, doesn't skew the score).

**`ScoreResult`:**
```csharp
record ScoreResult(
    int Overall,
    FactorScore Value,
    FactorScore Efficiency,
    FactorScore Reliability,
    FactorScore Mileage,
    bool HasMissingFactors  // true if any status == "missing"
);
```

### New CQRS Handler + Endpoint

**Query:** `GetListingScoreQuery { Guid ListingId }`  
**Response:** `GetListingScoreResponse` matching `ScoreResult` + `bool IsPersonalized` (false in Spec 1, used in Spec 2)

**Endpoint:** `GET /api/listings/{id}/score`  
Returns 200 with score, or 404 if listing not found.

---

## Frontend

### Score Card Component (listing details page)

Compact circular badge (72×72px, green/blue/orange based on score range: ≥70 green, 50–69 blue, <50 orange). Displays `overall` score.

Below the badge: `{N} of 4 factors scored` and a row of Lucide-icon pills for each factor with its name and color.

A `ChevronDown` icon below expands to show the full breakdown (4 factor rows, each with a label, Lucide icon, score number, and a thin progress bar). If `HasMissingFactors` is true, an `AlertTriangle` icon appears next to the score. Hovering/clicking the icon shows a tooltip listing which factors couldn't be fetched and why.

**Data fetch:** `useQuery` on `GET /api/listings/{id}/score`, fires in parallel with the main listing query. Shows a skeleton while loading.

### Score in Comparison Page

A sticky banner between the comparison header and the comparison table. Contains:
- Left side: score badge + car name for Listing A
- Center: "vs" text
- Right side: score badge + car name for Listing B
- Bottom of banner: AI summary section (from Spec 3; in Spec 1, this section shows "AI insight not yet enabled")

Both score badges use the same Score Card component. Missing factors show `AlertTriangle` inline.

---

## What's NOT in this spec

- User preference weights (Spec 2)
- AI summary content (Spec 3)
- Seller insight panel (Spec 3)
- Fuel prices endpoint (not needed for scoring)
- VIN-based CarDog endpoints (often return empty results; not worth the quota)

---

## Out-of-scope CarDog endpoints

The following CarDog endpoints were evaluated and excluded:

| Endpoint | Reason excluded |
|---|---|
| `/v1/fuel/:fuelType` | Fuel prices not part of scoring — too location-dependent |
| `/v1/research/make/:make/model/:model/year/:year` | Safety feature specs already partially in our listings; low ROI vs. quota cost |
| `/v1/market/analysis/vin/:vin` | Useful but our VIN field is optional; market overview by make/model/year is sufficient |
| `/v1/market/listings/:listingId/position` | Uses CarDog's own listing IDs, not ours |
