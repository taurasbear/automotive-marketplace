# Spec: Free API Migration (NHTSA + FuelEconomy.gov)

**Date:** 2026-04-26  
**Status:** Approved for implementation

## Problem & Approach

The vehicle score engine currently uses Cardog API for three data types: market price, fuel efficiency, and reliability (recalls/complaints). Cardog's efficiency and reliability endpoints have proven unreliable (complaints endpoint was broken) and the API is expensive (quota-limited). Free, authoritative US government datasets are available as direct replacements for everything except market price:

- **fueleconomy.gov** (DOE/EPA) replaces Cardog efficiency
- **NHTSA** replaces Cardog reliability — and adds NCAP safety star ratings (a new signal we never had)

Cardog is retained exclusively for market price data. All other Cardog methods are removed.

---

## Architecture

Approach A: two new API clients added alongside the existing pattern (`VpicVehicleDataApiClient`, `CardogApiClient`). The `GetListingScoreQueryHandler` is updated to route each data type to the appropriate client. No aggregator layer is introduced.

---

## New API Clients

### `INhtsaApiClient` / `NhtsaApiClient`

**Location:** `Application.Interfaces.Services` / `Infrastructure.Services`  
**Base URL:** `https://api.nhtsa.gov/`  
**Authentication:** none (public API)

Three methods called in parallel per vehicle:

```csharp
Task<NhtsaRecallsResult?> GetRecallsAsync(string make, string model, int year, CancellationToken ct);
Task<NhtsaComplaintsResult?> GetComplaintsAsync(string make, string model, int year, CancellationToken ct);
Task<NhtsaSafetyRatingResult?> GetSafetyRatingAsync(string make, string model, int year, CancellationToken ct);
```

**Result DTOs:**
```csharp
record NhtsaRecallsResult(int Count);
record NhtsaComplaintsResult(int Total);
record NhtsaSafetyRatingResult(int OverallRating);  // 1–5; null return = not crash-tested
```

**Endpoint mapping:**
- Recalls: `GET /recalls/recallsByVehicle?make={make}&model={model}&modelYear={year}` → `Count` from top-level JSON
- Complaints: `GET /complaints/complaintsByVehicle?make={make}&model={model}&modelYear={year}` → `count` from top-level JSON
- Safety rating: 2-step:
  1. `GET /SafetyRatings/modelyear/{year}/make/{make}/model/{model}` → take first variant's `VehicleId`
  2. `GET /SafetyRatings/VehicleId/{id}` → read `OverallRating`; null if no variants returned or `OverallRating == "Not Rated"`

All three are fired concurrently within `NhtsaApiClient`; any failure returns `null` for that result (logged as warning, never throws).

---

### `IFuelEconomyApiClient` / `FuelEconomyApiClient`

**Location:** `Application.Interfaces.Services` / `Infrastructure.Services`  
**Base URL:** `https://www.fueleconomy.gov/ws/rest/`  
**Authentication:** none (public API)

```csharp
Task<FuelEfficiencyResult?> GetFuelEfficiencyAsync(string make, string model, int year, CancellationToken ct);
```

**Result DTO:**
```csharp
record FuelEfficiencyResult(double? LitersPer100Km, double? KWhPer100Km);
```

**3-step vehicle ID resolution (sequential — each step depends on the previous):**
1. `GET /vehicle/menu/model?year={year}&make={make}` — fetch model list; find first entry whose `text` value starts with `{model}` (case-insensitive prefix match)
2. `GET /vehicle/menu/options?year={year}&make={make}&model={matched_model}` — take first returned `value` as the vehicle ID
3. `GET /vehicle/{id}` — read fuel economy fields

**Conversions:**
- ICE/hybrid: `comb08` (combined MPG) → `L/100km = 235.215 / mpg`
- EV: `combE` (kWh/100 miles) → `kWh/100km = kWh * 0.621371`
- If `combE > 0` the vehicle is electric; use `KWhPer100Km`, set `LitersPer100Km = null`

If no model prefix match is found, or any step fails: return `null` (logged, never throws).

---

## Cache Changes

### `VehicleEfficiencyCache`

No structural changes. TTL changes from 30 days → **90 days** (fuel economy ratings are stable). Data source changes from Cardog → fueleconomy.gov; existing cache entries expire naturally.

### `VehicleReliabilityCache`

Two new nullable columns added via migration:

| Column | Type | Notes |
|---|---|---|
| `ComplaintCount` | `int` | Total NHTSA complaint count |
| `OverallSafetyRating` | `int?` | NHTSA NCAP overall rating 1–5; null = not crash-tested |

`ComplaintCrashes` and `ComplaintInjuries` columns are **kept** (no data loss) but are no longer written or read by new code. They will read as their existing values in any existing cache rows until those rows expire.

TTL changes from 7 days → **30 days**.

### `VehicleMarketCache`

One new column added via migration:

| Column | Type | Notes |
|---|---|---|
| `IsFetchFailed` | `bool` | `true` = this row is a failure sentinel (Cardog call failed) |

When `IsFetchFailed = true`, the row's `ExpiresAt` is set to `UtcNow + 2 hours` (vs 24 hours for successful results). The handler checks `IsFetchFailed` and returns `null` immediately for such rows — no Cardog call is made. When Cardog succeeds, any existing failed sentinel is replaced with a real result.

---

## Handler Changes (`GetListingScoreQueryHandler`)

### Routing

| Data type | Old source | New source |
|---|---|---|
| Fuel efficiency | `ICardogApiClient.GetEfficiencyAsync` | `IFuelEconomyApiClient.GetFuelEfficiencyAsync` |
| Reliability | `ICardogApiClient.GetReliabilityAsync` | `INhtsaApiClient` (3 parallel calls) |
| Market price | `ICardogApiClient.GetMarketOverviewAsync` | `ICardogApiClient.GetMarketOverviewAsync` (unchanged) |

### Cardog market failure caching

`GetMarketAsync` updated:
1. Check DB for non-expired entry (including `IsFetchFailed` sentinels)
2. If `IsFetchFailed = true` and not expired → return `null` immediately
3. If valid non-failed entry → return cached result
4. Call Cardog:
   - **Success** → upsert with `IsFetchFailed = false`, `ExpiresAt = now + 24h`
   - **Failure** → upsert with `IsFetchFailed = true`, `ExpiresAt = now + 2h`; return `null`

### `ICardogApiClient` trimmed

`GetEfficiencyAsync` and `GetReliabilityAsync` methods are removed from the interface and implementation. `CardogEfficiencyResult` and `CardogReliabilityResult` DTOs are deleted. Only `GetMarketOverviewAsync` / `CardogMarketResult` remain.

---

## Score Formula Update (`ListingScoreCalculator`)

### Updated `ScoreReliability`

New inputs: `NhtsaRecallsResult?`, `NhtsaComplaintsResult?`, `NhtsaSafetyRatingResult?` (replacing `CardogReliabilityResult?`).

If all three are null → `MissingFactor` (neutral 50, same as before).

```
RecallScore    = 100 − clamp(recallCount / 10, 0, 1) × 100
                 0 recalls → 100 | 10+ recalls → 0

ComplaintScore = 100 − clamp(complaintsTotal / 500, 0, 1) × 100
                 0 complaints → 100 | 500+ → 0

SafetyScore    = (overallRating − 1) / 4 × 100
                 1★ → 0 | 5★ → 100

If safety-rated:
  ReliabilityScore = 0.40 × RecallScore + 0.20 × ComplaintScore + 0.40 × SafetyScore

If not safety-rated:
  ReliabilityScore = 0.60 × RecallScore + 0.40 × ComplaintScore
```

**Example — 2020 Toyota Camry** (3 recalls, 256 complaints, 5★):  
RecallScore = 70, ComplaintScore = 48.8, SafetyScore = 100  
→ `0.4×70 + 0.2×48.8 + 0.4×100` = 28 + 9.8 + 40 = **77.8** ✅

---

## DI Registration

`ServiceExtensions.ConfigureInfrastructure`:
```csharp
services.AddHttpClient<INhtsaApiClient, NhtsaApiClient>(client =>
    client.BaseAddress = new Uri("https://api.nhtsa.gov/"));

services.AddHttpClient<IFuelEconomyApiClient, FuelEconomyApiClient>(client =>
    client.BaseAddress = new Uri("https://www.fueleconomy.gov/ws/rest/"));
```

`GetListingScoreQueryHandler` constructor adds `INhtsaApiClient nhtsaClient` and `IFuelEconomyApiClient fuelEconomyClient` parameters; `ICardogApiClient` remains.

---

## Migrations

One EF Core migration:
- **`AddFreeApiCacheColumns`**: adds `ComplaintCount (int, default 0)`, `OverallSafetyRating (int?, nullable)` to `VehicleReliabilityCache`; adds `IsFetchFailed (bool, default false)` to `VehicleMarketCache`.

No new tables or table deletions.

---

## What's NOT in this spec

- Retirement of `ComplaintCrashes` / `ComplaintInjuries` columns (deferred — no migration needed now)
- NHTSA investigations endpoint (not used for scoring)
- VIN-based NHTSA lookups (our VIN field is optional)
- User-facing display of safety star ratings in the UI (score computation change only)
