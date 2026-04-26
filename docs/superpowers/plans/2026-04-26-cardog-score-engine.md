# CarDog Integration & Score Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate CarDog API for vehicle insights (efficiency, market value, reliability), build a score engine, expose a `GET /Listing/GetScore` endpoint, and render the score card on the listing details page and a sticky comparison banner.

**Architecture:** Cache-first approach — check DB cache tables (`VehicleEfficiencyCache`, `VehicleMarketCache`, `VehicleReliabilityCache`) before hitting CarDog API. Score is calculated in-process by a pure-function static class (`ListingScoreCalculator`). Frontend renders a collapsible `ScoreCard` with circular badge and factor breakdown bars.

**Tech Stack:** .NET 8, EF Core (Npgsql), MediatR, System.Net.Http.Json; React 19, TypeScript, TanStack Query, Lucide React, shadcn/ui.

---

## File Map

**New files — Backend:**
- `Automotive.Marketplace.Application/Interfaces/Services/ICardogApiClient.cs` — interface + result DTOs
- `Automotive.Marketplace.Infrastructure/Services/CardogApiClient.cs` — typed HttpClient impl
- `Automotive.Marketplace.Domain/Entities/VehicleEfficiencyCache.cs`
- `Automotive.Marketplace.Domain/Entities/VehicleMarketCache.cs`
- `Automotive.Marketplace.Domain/Entities/VehicleReliabilityCache.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleEfficiencyCacheConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleMarketCacheConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleReliabilityCacheConfiguration.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQuery.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs`
- `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingScoreQueryHandlerTests.cs`

**Modified files — Backend:**
- `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — add 3 new `DbSet<T>`
- `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs` — register CarDog client + config
- `Automotive.Marketplace.Server/Controllers/ListingController.cs` — add `GetScore` action
- `Automotive.Marketplace.Server/appsettings.json` — add `Cardog:ApiKey` placeholder

**New files — Frontend:**
- `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts`
- `automotive.marketplace.client/src/features/listingDetails/api/getListingScoreOptions.ts`
- `automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx`
- `automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx`

**Modified files — Frontend:**
- `automotive.marketplace.client/src/constants/endpoints.ts` — add `GET_SCORE`
- `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts` — add `score` key
- `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx` — add `ScoreCard`
- `automotive.marketplace.client/src/app/pages/Compare.tsx` — add `CompareScoreBanner`

---

### Task 1: CarDog Application interface + result DTOs

**Files:**
- Create: `Automotive.Marketplace.Application/Interfaces/Services/ICardogApiClient.cs`

- [ ] **Step 1: Create the interface file**

```csharp
namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ICardogApiClient
{
    Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<CardogMarketResult?> GetMarketOverviewAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, CancellationToken cancellationToken);
}

public record CardogEfficiencyResult(
    double? LitersPer100Km,
    double? KWhPer100Km);

public record CardogMarketResult(
    decimal MedianPrice,
    int TotalListings);

public record CardogReliabilityResult(
    int RecallCount,
    int ComplaintCrashes,
    int ComplaintInjuries);
```

- [ ] **Step 2: Build to confirm no errors**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Application/Interfaces/Services/ICardogApiClient.cs
git commit -m "feat: add ICardogApiClient interface and result DTOs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: CardogApiClient infrastructure implementation

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Services/CardogApiClient.cs`
- Modify: `Automotive.Marketplace.Server/appsettings.json`
- Modify: `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`

The CarDog API base URL is `https://api.cardog.io/v1/`. API key is sent as header `x-api-key`. All three endpoints called by this client:

- `GET /efficiency/{make}/{model}/{year}` → returns `{ data: [...], meta: { count: N } }`. Each element has `combinedLPer100km` (nullable) and `combinedKwhPer100km` (nullable). Take the average of non-null values across all returned trims.
- `GET /market/{make}/{model}/{year}/overview` → returns flat object with `medianPrice` and `totalListings`.
- Reliability = 2 calls: `GET /recalls/us/search?makes=MAKE&models=MODEL&year=YEAR&limit=1` (use `count` from response) + `GET /complaints/search?makes=MAKE&models=MODEL&yearMin=YEAR&yearMax=YEAR&crashInvolved=true&limit=1` (use `count` from response for crashes) + `GET /complaints/search?makes=MAKE&models=MODEL&yearMin=YEAR&yearMax=YEAR&hasInjuries=true&limit=1` (use `count` for injuries).

- [ ] **Step 1: Add appsettings placeholder**

In `Automotive.Marketplace.Server/appsettings.json`, add the Cardog section (put this alongside the existing `Logging` key):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cardog": {
    "ApiKey": ""
  }
}
```

- [ ] **Step 2: Create CardogApiClient**

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class CardogApiClient(HttpClient httpClient) : ICardogApiClient
{
    public async Task<CardogEfficiencyResult?> GetEfficiencyAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"efficiency/{Uri.EscapeDataString(make)}/{Uri.EscapeDataString(model)}/{year}";
            var response = await httpClient.GetFromJsonAsync<EfficiencyResponse>(url, cancellationToken);
            if (response?.Data is not { Count: > 0 }) return null;

            var avgLiters = response.Data
                .Where(d => d.CombinedLPer100km.HasValue)
                .Select(d => d.CombinedLPer100km!.Value)
                .DefaultIfEmpty()
                .Average();

            var avgKwh = response.Data
                .Where(d => d.CombinedKwhPer100km.HasValue)
                .Select(d => d.CombinedKwhPer100km!.Value)
                .DefaultIfEmpty()
                .Average();

            return new CardogEfficiencyResult(
                avgLiters > 0 ? avgLiters : null,
                avgKwh > 0 ? avgKwh : null);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CardogMarketResult?> GetMarketOverviewAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"market/{Uri.EscapeDataString(make)}/{Uri.EscapeDataString(model)}/{year}/overview";
            var response = await httpClient.GetFromJsonAsync<MarketOverviewResponse>(url, cancellationToken);
            if (response is null) return null;
            return new CardogMarketResult(response.MedianPrice, response.TotalListings);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CardogReliabilityResult?> GetReliabilityAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var makeEncoded = Uri.EscapeDataString(make.ToUpperInvariant());
            var modelEncoded = Uri.EscapeDataString(model.ToUpperInvariant());

            var recallsResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"recalls/us/search?makes={makeEncoded}&models={modelEncoded}&year={year}&limit=1",
                cancellationToken);

            var crashesResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"complaints/search?makes={makeEncoded}&models={modelEncoded}&yearMin={year}&yearMax={year}&crashInvolved=true&limit=1",
                cancellationToken);

            var injuriesResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"complaints/search?makes={makeEncoded}&models={modelEncoded}&yearMin={year}&yearMax={year}&hasInjuries=true&limit=1",
                cancellationToken);

            return new CardogReliabilityResult(
                recallsResponse?.Count ?? 0,
                crashesResponse?.Count ?? 0,
                injuriesResponse?.Count ?? 0);
        }
        catch
        {
            return null;
        }
    }

    // Private JSON models
    private class EfficiencyResponse
    {
        [JsonPropertyName("data")]
        public List<EfficiencyRecord> Data { get; set; } = [];
    }

    private class EfficiencyRecord
    {
        [JsonPropertyName("combinedLPer100km")]
        public double? CombinedLPer100km { get; set; }

        [JsonPropertyName("combinedKwhPer100km")]
        public double? CombinedKwhPer100km { get; set; }
    }

    private class MarketOverviewResponse
    {
        [JsonPropertyName("medianPrice")]
        public decimal MedianPrice { get; set; }

        [JsonPropertyName("totalListings")]
        public int TotalListings { get; set; }
    }

    private class CountResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
```

- [ ] **Step 3: Register in ServiceExtensions.cs**

In `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`, inside the `ConfigureInfrastructure` method, add registration near the other `AddHttpClient` calls:

```csharp
var cardogApiKey = configuration["Cardog:ApiKey"] ?? string.Empty;
services.AddHttpClient<ICardogApiClient, CardogApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.cardog.io/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", cardogApiKey);
});
```

Make sure the `ICardogApiClient` using directive is included at the top of `ServiceExtensions.cs`:
```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
```

- [ ] **Step 4: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Services/CardogApiClient.cs \
        Automotive.Marketplace.Infrastructure/ServiceExtensions.cs \
        Automotive.Marketplace.Server/appsettings.json
git commit -m "feat: add CardogApiClient with efficiency, market, and reliability endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Cache entity classes

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/VehicleEfficiencyCache.cs`
- Create: `Automotive.Marketplace.Domain/Entities/VehicleMarketCache.cs`
- Create: `Automotive.Marketplace.Domain/Entities/VehicleReliabilityCache.cs`

Cache entities extend `BaseEntity` so they're compatible with `IRepository`. The cache key for all three is `(Make, Model, Year)`. All listings sharing the same make/model/year share one cache entry.

- [ ] **Step 1: Create VehicleEfficiencyCache**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class VehicleEfficiencyCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public double? LitersPer100Km { get; set; }
    public double? KWhPer100Km { get; set; }
    public string? FetchedTrimName { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 2: Create VehicleMarketCache**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class VehicleMarketCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal MedianPrice { get; set; }
    public int TotalListings { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 3: Create VehicleReliabilityCache**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class VehicleReliabilityCache : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int RecallCount { get; set; }
    public int ComplaintCrashes { get; set; }
    public int ComplaintInjuries { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/VehicleEfficiencyCache.cs \
        Automotive.Marketplace.Domain/Entities/VehicleMarketCache.cs \
        Automotive.Marketplace.Domain/Entities/VehicleReliabilityCache.cs
git commit -m "feat: add vehicle cache domain entities

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: EF configurations + DbContext update + migration

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleEfficiencyCacheConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleMarketCacheConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleReliabilityCacheConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create VehicleEfficiencyCacheConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VehicleEfficiencyCacheConfiguration : IEntityTypeConfiguration<VehicleEfficiencyCache>
{
    public void Configure(EntityTypeBuilder<VehicleEfficiencyCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Make).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => new { e.Make, e.Model, e.Year }).IsUnique();
    }
}
```

- [ ] **Step 2: Create VehicleMarketCacheConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VehicleMarketCacheConfiguration : IEntityTypeConfiguration<VehicleMarketCache>
{
    public void Configure(EntityTypeBuilder<VehicleMarketCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Make).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MedianPrice).HasColumnType("decimal(18,2)");
        builder.HasIndex(e => new { e.Make, e.Model, e.Year }).IsUnique();
    }
}
```

- [ ] **Step 3: Create VehicleReliabilityCacheConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VehicleReliabilityCacheConfiguration : IEntityTypeConfiguration<VehicleReliabilityCache>
{
    public void Configure(EntityTypeBuilder<VehicleReliabilityCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Make).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => new { e.Make, e.Model, e.Year }).IsUnique();
    }
}
```

- [ ] **Step 4: Add DbSets to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add these three `DbSet` properties alongside the existing ones:

```csharp
public DbSet<VehicleEfficiencyCache> VehicleEfficiencyCaches { get; set; }
public DbSet<VehicleMarketCache> VehicleMarketCaches { get; set; }
public DbSet<VehicleReliabilityCaches> VehicleReliabilityCaches { get; set; }
```

⚠️ Fix typo: the third line above should read `VehicleReliabilityCache` (not `VehicleReliabilityCaches`) on the left side:
```csharp
public DbSet<VehicleReliabilityCache> VehicleReliabilityCaches { get; set; }
```

- [ ] **Step 5: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 6: Create EF migration**

Run from the repo root:

```bash
dotnet ef migrations add AddVehicleCacheTables \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected: A new migration file appears under `Automotive.Marketplace.Infrastructure/Migrations/`.

- [ ] **Step 7: Apply migration (dev database)**

```bash
dotnet ef database update \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

- [ ] **Step 8: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleEfficiencyCacheConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleMarketCacheConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/Configuration/VehicleReliabilityCacheConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs \
        Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat: add EF config, DbSets, and migration for vehicle cache tables

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: ListingScoreCalculator + unit tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorTests.cs`

The calculator is a pure static class — no DI, no async. It takes raw input data and returns a `GetListingScoreResponse`. This task defines both `GetListingScoreResponse`/`ScoreFactor` records and the calculator.

Default weights: Value 30 %, Efficiency 25 %, Reliability 25 %, Mileage 20 %.

Score formulas:
- **Value**: `ratio = (medianPrice - listingPrice) / medianPrice`, clamped `[-0.5, +0.3]`, then `(ratio + 0.5) / 0.8 * 100`
- **Efficiency (ICE)**: `clamp((12 - L/100km) / 6, 0, 1) * 100` — 6 L/100 km = 100 pts, 12 = 0 pts
- **Efficiency (EV)**: `clamp((25 - kWh/100km) / 10, 0, 1) * 100` — 15 kWh/100 km = 100 pts, 25 = 0 pts
- **Reliability**: `100 - clamp((recalls*2 + crashes*3 + injuries*2) / 50.0, 0, 1) * 100`
- **Mileage**: `clamp((ageYears*15000 - mileage) / (ageYears*15000), 0, 1) * 100`; if `ageYears == 0` score = 100

If a factor's input data is null → `Status = "missing"`, `Score = 50` (neutral), weight is preserved.
Overall score = weighted average renormalised to scored-only factors.

- [ ] **Step 1: Write the failing tests first**

Create `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Interfaces.Services;
using FluentAssertions;

namespace Automotive.Marketplace.Tests.Features.UnitTests;

public class ListingScoreCalculatorTests
{
    [Fact]
    public void Calculate_ListingBelowMarket_ReturnsHighValueScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 60000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null,
            reliability: null);

        // (10000 - 8000) / 10000 = 0.20 → (0.20 + 0.5) / 0.8 * 100 = 87.5
        result.Value.Score.Should().BeApproximately(87.5, 0.5);
        result.Value.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_ListingAtMarket_ReturnsApprox62Score()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null,
            reliability: null);

        // (0 + 0.5) / 0.8 * 100 = 62.5
        result.Value.Score.Should().BeApproximately(62.5, 0.5);
    }

    [Fact]
    public void Calculate_NullMarket_ValueFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Value.Status.Should().Be("missing");
        result.Value.Score.Should().Be(50);
        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().Contain("Market Value");
    }

    [Fact]
    public void Calculate_EfficientIceVehicle_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            reliability: null);

        // (12 - 6) / 6 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
        result.Efficiency.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_InefficientIceVehicle_ReturnsZeroEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 15.0, KWhPer100Km: null),
            reliability: null);

        // (12 - 15) / 6 clamped to 0 = 0
        result.Efficiency.Score.Should().BeApproximately(0.0, 0.5);
    }

    [Fact]
    public void Calculate_EfficientEv_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: null, KWhPer100Km: 15.0),
            reliability: null);

        // (25 - 15) / 10 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
    }

    [Fact]
    public void Calculate_NullEfficiency_EfficiencyFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Efficiency.Status.Should().Be("missing");
        result.MissingFactors.Should().Contain("Efficiency");
    }

    [Fact]
    public void Calculate_NoRecallsOrComplaints_ReliabilityScore100()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: new CardogReliabilityResult(RecallCount: 0, ComplaintCrashes: 0, ComplaintInjuries: 0));

        result.Reliability.Score.Should().BeApproximately(100.0, 0.5);
        result.Reliability.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_LowMileageForAge_HighMileageScore()
    {
        // 2020 car (5 years old), 30k km — expected: 30000 vs 75000 → score = (75000 - 30000) / 75000 * 100 = 60
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 30000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Mileage.Status.Should().Be("scored");
        result.Mileage.Score.Should().BeGreaterThan(50);
    }

    [Fact]
    public void Calculate_AllMissing_OverallScoreIs50()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        // Only mileage is scored (always available), so overall ≠ exactly 50
        // But HasMissingFactors should be true
        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
    }

    [Fact]
    public void Calculate_OverallScore_IsWeightedAverage()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 30000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            reliability: new CardogReliabilityResult(RecallCount: 0, ComplaintCrashes: 0, ComplaintInjuries: 0));

        result.OverallScore.Should().BeGreaterThan(70);
        result.HasMissingFactors.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~ListingScoreCalculatorTests" --no-build 2>&1 | tail -5
```
Expected: errors about `ListingScoreCalculator` not existing.

- [ ] **Step 3: Create ListingScoreCalculator**

Create `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs`:

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public static class ListingScoreCalculator
{
    private const double ValueWeight = 0.30;
    private const double EfficiencyWeight = 0.25;
    private const double ReliabilityWeight = 0.25;
    private const double MileageWeight = 0.20;

    public static GetListingScoreResponse Calculate(
        decimal listingPrice,
        int year,
        int mileageKm,
        CardogMarketResult? market,
        CardogEfficiencyResult? efficiency,
        CardogReliabilityResult? reliability,
        ScoreWeights? weights = null)
    {
        var w = weights ?? new ScoreWeights(ValueWeight, EfficiencyWeight, ReliabilityWeight, MileageWeight);

        var valueFactor = market != null
            ? ScoreValue(listingPrice, market.MedianPrice, w.Value)
            : MissingFactor(w.Value);

        var efficiencyFactor = efficiency != null
            ? ScoreEfficiency(efficiency, w.Efficiency)
            : MissingFactor(w.Efficiency);

        var reliabilityFactor = reliability != null
            ? ScoreReliability(reliability, w.Reliability)
            : MissingFactor(w.Reliability);

        var mileageFactor = ScoreMileage(year, mileageKm, w.Mileage);

        var allFactors = new[] { valueFactor, efficiencyFactor, reliabilityFactor, mileageFactor };
        var scoredFactors = allFactors.Where(f => f.Status == "scored").ToArray();
        var totalWeight = scoredFactors.Sum(f => f.Weight);
        var overallScore = totalWeight > 0
            ? (int)Math.Round(scoredFactors.Sum(f => f.Score * f.Weight) / totalWeight)
            : 50;

        var missingFactors = new List<string>();
        if (valueFactor.Status == "missing") missingFactors.Add("Market Value");
        if (efficiencyFactor.Status == "missing") missingFactors.Add("Efficiency");
        if (reliabilityFactor.Status == "missing") missingFactors.Add("Reliability");

        return new GetListingScoreResponse
        {
            OverallScore = overallScore,
            Value = valueFactor,
            Efficiency = efficiencyFactor,
            Reliability = reliabilityFactor,
            Mileage = mileageFactor,
            HasMissingFactors = missingFactors.Count > 0,
            MissingFactors = missingFactors,
        };
    }

    private static ScoreFactor ScoreValue(decimal listingPrice, decimal medianPrice, double weight)
    {
        var ratio = (double)((medianPrice - listingPrice) / medianPrice);
        var clamped = Math.Clamp(ratio, -0.5, 0.3);
        var score = (clamped + 0.5) / 0.8 * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreEfficiency(CardogEfficiencyResult efficiency, double weight)
    {
        double score;
        if (efficiency.KWhPer100Km.HasValue)
        {
            score = Math.Clamp((25.0 - efficiency.KWhPer100Km.Value) / 10.0, 0, 1) * 100.0;
        }
        else if (efficiency.LitersPer100Km.HasValue)
        {
            score = Math.Clamp((12.0 - efficiency.LitersPer100Km.Value) / 6.0, 0, 1) * 100.0;
        }
        else
        {
            return MissingFactor(weight);
        }
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreReliability(CardogReliabilityResult reliability, double weight)
    {
        var penalty = reliability.RecallCount * 2 + reliability.ComplaintCrashes * 3 + reliability.ComplaintInjuries * 2;
        var score = 100.0 - Math.Clamp(penalty / 50.0, 0, 1) * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreMileage(int year, int mileageKm, double weight)
    {
        var ageYears = Math.Max(DateTime.UtcNow.Year - year, 0);
        if (ageYears == 0)
            return new ScoreFactor(Score: 100.0, Status: "scored", Weight: weight);

        var expectedKm = ageYears * 15000.0;
        var score = Math.Clamp((expectedKm - mileageKm) / expectedKm, 0, 1) * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor MissingFactor(double weight) =>
        new(Score: 50, Status: "missing", Weight: weight);
}
```

Also create `GetListingScoreResponse.cs` (needed by tests):

```csharp
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreResponse
{
    public int OverallScore { get; init; }
    public ScoreFactor Value { get; init; } = null!;
    public ScoreFactor Efficiency { get; init; } = null!;
    public ScoreFactor Reliability { get; init; } = null!;
    public ScoreFactor Mileage { get; init; } = null!;
    public bool HasMissingFactors { get; init; }
    public List<string> MissingFactors { get; init; } = [];
}

public record ScoreFactor(double Score, string Status, double Weight);

public record ScoreWeights(double Value, double Efficiency, double Reliability, double Mileage);
```

- [ ] **Step 4: Build and run unit tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~ListingScoreCalculatorTests" -q
```
Expected: All 10 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ \
        Automotive.Marketplace.Tests/Features/UnitTests/
git commit -m "feat: add ListingScoreCalculator with unit tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: GetListingScoreQuery + Handler + integration tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingScoreQueryHandlerTests.cs`

The handler:
1. Loads listing (with `Variant.Fuel`, `Variant.Model.Make` includes)
2. For each of the 3 CarDog data types: checks DB cache by `(Make, Model, Year)` where `ExpiresAt > DateTime.UtcNow`; on cache miss calls the `ICardogApiClient` method and upserts (update if stale entry exists, create if none)
3. Calls `ListingScoreCalculator.Calculate()`
4. Returns `GetListingScoreResponse`

Cache TTLs: efficiency = 30 days, market = 24 hours, reliability = 7 days.

- [ ] **Step 1: Create GetListingScoreQuery**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQuery : IRequest<GetListingScoreResponse>
{
    public Guid ListingId { get; set; }
}
```

- [ ] **Step 2: Write the failing integration tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingScoreQueryHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingScoreQueryHandlerTests(
    DatabaseFixture<GetListingScoreQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingScoreQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingScoreQueryHandlerTests> _fixture = fixture;
    private readonly ICardogApiClient _cardogClient = Substitute.For<ICardogApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingScoreQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingScoreQueryHandler(repository, _cardogClient);
    }

    private async Task<Guid> SeedListingAsync(AutomotiveContext context, string makeName = "Honda", string modelName = "Accord", int year = 2020, decimal price = 15000m, int mileage = 80000)
    {
        var make = new MakeBuilder().With(m => m.Name, makeName).Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, modelName).Build();
        var fuel = new FuelBuilder().With(f => f.Name, "Gasoline").Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .WithMunicipality(municipality.Id)
            .WithYear(year).WithPrice(price).WithMileage(mileage)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_AllCardogDataAvailable_ReturnsFullScore()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, price: 15000m);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 18000m, TotalListings: 60));
        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogEfficiencyResult(LitersPer100Km: 7.5, KWhPer100Km: null));
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogReliabilityResult(RecallCount: 1, ComplaintCrashes: 0, ComplaintInjuries: 0));

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeFalse();
        result.OverallScore.Should().BeGreaterThan(0).And.BeLessOrEqualTo(100);
        result.Value.Status.Should().Be("scored");
        result.Efficiency.Status.Should().Be("scored");
        result.Reliability.Status.Should().Be("scored");
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_CardogReturnsNull_MissingFactorsReturned()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogMarketResult?)null);
        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogEfficiencyResult?)null);
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogReliabilityResult?)null);

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_CacheHit_DoesNotCallCardogApi()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Toyota", modelName: "Camry", year: 2021);

        // Pre-seed cache entries
        await context.VehicleMarketCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            MedianPrice = 22000m, TotalListings = 45,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23),
        });
        await context.VehicleEfficiencyCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleEfficiencyCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            LitersPer100Km = 8.5, KWhPer100Km = null,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.VehicleReliabilityCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleReliabilityCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            RecallCount = 2, ComplaintCrashes = 1, ComplaintInjuries = 0,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(6),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.DidNotReceive().GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.DidNotReceive().GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.HasMissingFactors.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ListingNotFound_ThrowsNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = async () => await handler.Handle(
            new GetListingScoreQuery { ListingId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingScoreQueryHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors about `GetListingScoreQueryHandler` not existing.

- [ ] **Step 4: Create GetListingScoreQueryHandler**

Create `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs`:

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQueryHandler(IRepository repository, ICardogApiClient cardogClient)
    : IRequestHandler<GetListingScoreQuery, GetListingScoreResponse>
{
    public async Task<GetListingScoreResponse> Handle(GetListingScoreQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Fuel)
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var year = listing.Year;

        var efficiency = await GetEfficiencyAsync(make, model, year, cancellationToken);
        var market = await GetMarketAsync(make, model, year, cancellationToken);
        var reliability = await GetReliabilityAsync(make, model, year, cancellationToken);

        return ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, market, efficiency, reliability);
    }

    private async Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogEfficiencyResult(cache.LitersPer100Km, cache.KWhPer100Km);

        var result = await cardogClient.GetEfficiencyAsync(make, model, year, ct);
        if (result != null)
            await UpsertEfficiencyCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task<CardogMarketResult?> GetMarketAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogMarketResult(cache.MedianPrice, cache.TotalListings);

        var result = await cardogClient.GetMarketOverviewAsync(make, model, year, ct);
        if (result != null)
            await UpsertMarketCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, CancellationToken ct)
    {
        var cache = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow, ct);

        if (cache != null)
            return new CardogReliabilityResult(cache.RecallCount, cache.ComplaintCrashes, cache.ComplaintInjuries);

        var result = await cardogClient.GetReliabilityAsync(make, model, year, ct);
        if (result != null)
            await UpsertReliabilityCacheAsync(make, model, year, result, ct);

        return result;
    }

    private async Task UpsertEfficiencyCacheAsync(string make, string model, int year, CardogEfficiencyResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleEfficiencyCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.LitersPer100Km = result.LitersPer100Km;
            existing.KWhPer100Km = result.KWhPer100Km;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(30);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleEfficiencyCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                LitersPer100Km = result.LitersPer100Km,
                KWhPer100Km = result.KWhPer100Km,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
            }, ct);
        }
    }

    private async Task UpsertMarketCacheAsync(string make, string model, int year, CardogMarketResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.MedianPrice = result.MedianPrice;
            existing.TotalListings = result.TotalListings;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddHours(24);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleMarketCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                MedianPrice = result.MedianPrice,
                TotalListings = result.TotalListings,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
            }, ct);
        }
    }

    private async Task UpsertReliabilityCacheAsync(string make, string model, int year, CardogReliabilityResult result, CancellationToken ct)
    {
        var existing = await repository.AsQueryable<VehicleReliabilityCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.RecallCount = result.RecallCount;
            existing.ComplaintCrashes = result.ComplaintCrashes;
            existing.ComplaintInjuries = result.ComplaintInjuries;
            existing.FetchedAt = DateTime.UtcNow;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            await repository.CreateAsync(new VehicleReliabilityCache
            {
                Id = Guid.NewGuid(),
                Make = make, Model = model, Year = year,
                RecallCount = result.RecallCount,
                ComplaintCrashes = result.ComplaintCrashes,
                ComplaintInjuries = result.ComplaintInjuries,
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }, ct);
        }
    }
}
```

Note: The correct exception class is `DbEntityNotFoundException` in `Automotive.Marketplace.Application.Common.Exceptions`. Add the using directive `using Automotive.Marketplace.Application.Common.Exceptions;` to the handler.

- [ ] **Step 5: Build and run all tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingScoreQueryHandlerTests|FullyQualifiedName~ListingScoreCalculatorTests" -q
```
Expected: All tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingScoreQueryHandlerTests.cs
git commit -m "feat: add GetListingScoreQueryHandler with DB cache + CarDog integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Controller endpoint

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Add GetScore action**

In `Automotive.Marketplace.Server/Controllers/ListingController.cs`, add the following imports at the top if not already present:
```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
```

Then add the action at the bottom of the class (before the closing `}`):

```csharp
[HttpGet]
public async Task<ActionResult<GetListingScoreResponse>> GetScore(
    [FromQuery] GetListingScoreQuery query,
    CancellationToken cancellationToken)
{
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

- [ ] **Step 2: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat: expose GET /Listing/GetScore endpoint

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Frontend API layer

**Files:**
- Create: `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts`
- Create: `automotive.marketplace.client/src/features/listingDetails/api/getListingScoreOptions.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`

- [ ] **Step 1: Create the response type**

```typescript
export type ScoreFactor = {
  score: number;
  status: "scored" | "missing";
  weight: number;
};

export type GetListingScoreResponse = {
  overallScore: number;
  value: ScoreFactor;
  efficiency: ScoreFactor;
  reliability: ScoreFactor;
  mileage: ScoreFactor;
  hasMissingFactors: boolean;
  missingFactors: string[];
};
```

- [ ] **Step 2: Add endpoint constant**

In `automotive.marketplace.client/src/constants/endpoints.ts`, inside the `LISTING` object, add:
```typescript
GET_SCORE: "/Listing/GetScore",
```

- [ ] **Step 3: Add query key**

In `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`, add `score` to the `listingKeys` object:

```typescript
score: (id: string) => [...listingKeys.all(), id, "score"],
```

- [ ] **Step 4: Create query options**

```typescript
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingScoreResponse } from "../types/GetListingScoreResponse";

export const getListingScoreOptions = (listingId: string) =>
  queryOptions({
    queryKey: listingKeys.score(listingId),
    queryFn: () =>
      axiosClient.get<GetListingScoreResponse>(ENDPOINTS.LISTING.GET_SCORE, {
        params: { listingId },
      }),
  });
```

- [ ] **Step 5: Build frontend**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: `✓ built in` with no errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts \
        automotive.marketplace.client/src/features/listingDetails/api/getListingScoreOptions.ts \
        automotive.marketplace.client/src/constants/endpoints.ts \
        automotive.marketplace.client/src/api/queryKeys/listingKeys.ts
git commit -m "feat: add frontend API layer for listing score endpoint

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: ScoreCard component

**Files:**
- Create: `automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx`

The `ScoreCard`:
- Shows a circular badge (72×72 px) with the overall score. Color: `text-green-600` if ≥ 70, `text-blue-600` if 50–69, `text-orange-500` if < 50.
- Shows "Un-personalized" label below the badge (Plan 2 will make this dynamic).
- Has a collapsible breakdown showing 4 factor rows with weight bars. Missing factors show an `AlertTriangle` icon with tooltip.
- Loading state: skeleton pulse.

- [ ] **Step 1: Create ScoreCard**

```tsx
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { getListingScoreOptions } from "../api/getListingScoreOptions";
import type { ScoreFactor } from "../types/GetListingScoreResponse";

type ScoreCardProps = {
  listingId: string;
};

const FACTOR_LABELS: Record<string, string> = {
  value: "Market Value",
  efficiency: "Efficiency",
  reliability: "Reliability",
  mileage: "Mileage",
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function FactorBar({ label, factor }: { label: string; factor: ScoreFactor }) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-between py-1 text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="text-muted-foreground flex items-center gap-1">
          <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
          No data
        </span>
      </div>
    );
  }
  return (
    <div className="py-1">
      <div className="mb-1 flex justify-between text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-medium">{Math.round(factor.score)}</span>
      </div>
      <div className="bg-muted h-1.5 w-full rounded-full">
        <div
          className={`h-1.5 rounded-full ${factor.score >= 70 ? "bg-green-500" : factor.score >= 50 ? "bg-blue-500" : "bg-orange-500"}`}
          style={{ width: `${Math.round(factor.score)}%` }}
        />
      </div>
    </div>
  );
}

export function ScoreCard({ listingId }: ScoreCardProps) {
  const [expanded, setExpanded] = useState(false);
  const { data, isLoading } = useQuery(getListingScoreOptions(listingId));

  if (isLoading) {
    return (
      <div className="border-border rounded-lg border p-4">
        <div className="flex items-center gap-4">
          <div className="bg-muted h-[72px] w-[72px] animate-pulse rounded-full" />
          <div className="flex-1 space-y-2">
            <div className="bg-muted h-4 w-32 animate-pulse rounded" />
            <div className="bg-muted h-3 w-24 animate-pulse rounded" />
          </div>
        </div>
      </div>
    );
  }

  if (!data) return null;

  const score = data.data;

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center gap-4">
        <div className="flex h-[72px] w-[72px] flex-shrink-0 flex-col items-center justify-center rounded-full border-2 border-current">
          <span className={`text-2xl font-bold leading-none ${scoreColor(score.overallScore)}`}>
            {score.overallScore}
          </span>
          <span className={`text-xs font-medium ${scoreColor(score.overallScore)}`}>/100</span>
        </div>
        <div className="flex-1">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-semibold">Vehicle Score</p>
              <p className="text-muted-foreground text-xs">Un-personalized</p>
            </div>
            <button
              onClick={() => setExpanded(!expanded)}
              className="text-muted-foreground hover:text-foreground"
              aria-label={expanded ? "Collapse score breakdown" : "Expand score breakdown"}
            >
              {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </button>
          </div>
          {score.hasMissingFactors && !expanded && (
            <p className="text-muted-foreground mt-1 flex items-center gap-1 text-xs">
              <AlertTriangle className="h-3 w-3 text-orange-400" />
              Missing: {score.missingFactors.join(", ")}
            </p>
          )}
        </div>
      </div>

      {expanded && (
        <div className="mt-4 space-y-1 border-t pt-3">
          <FactorBar label={FACTOR_LABELS.value} factor={score.value} />
          <FactorBar label={FACTOR_LABELS.efficiency} factor={score.efficiency} />
          <FactorBar label={FACTOR_LABELS.reliability} factor={score.reliability} />
          <FactorBar label={FACTOR_LABELS.mileage} factor={score.mileage} />
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx
git commit -m "feat: add ScoreCard component with circular badge and factor breakdown

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: Integrate ScoreCard into ListingDetailsContent

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Add ScoreCard import**

At the top of `ListingDetailsContent.tsx`, add the import:
```typescript
import { ScoreCard } from "./ScoreCard";
```

- [ ] **Step 2: Place ScoreCard in the JSX**

Find a natural placement in the listing details layout — after the price/main info section and before the description or chat panel. Add `ScoreCard` using the listing's id prop:

```tsx
<ScoreCard listingId={id} />
```

The exact insertion point depends on the current JSX structure in the file. Find where the listing price or key details are shown and add `ScoreCard` after that block.

- [ ] **Step 3: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: add ScoreCard to listing details page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: CompareScoreBanner component + Compare page integration

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Compare.tsx`

The `CompareScoreBanner` is a sticky div above the comparison table showing:
- Left panel: score badge + score breakdown summary for listing A
- Right panel: score badge + score breakdown summary for listing B
- A narrow center label column showing factor names

- [ ] **Step 1: Create CompareScoreBanner**

```tsx
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle } from "lucide-react";
import { getListingScoreOptions } from "@/features/listingDetails/api/getListingScoreOptions";
import type { GetListingScoreResponse, ScoreFactor } from "@/features/listingDetails/types/GetListingScoreResponse";

type CompareScoreBannerProps = {
  listingAId: string;
  listingBId: string;
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function MiniFactorRow({ factor, label }: { factor: ScoreFactor; label: string }) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-center gap-1 text-xs">
        <AlertTriangle className="h-3 w-3 text-orange-400" />
        <span className="text-muted-foreground">—</span>
      </div>
    );
  }
  return (
    <div className="text-center text-xs font-medium">
      {Math.round(factor.score)}
    </div>
  );
}

function ScoreColumn({ score, loading }: { score: GetListingScoreResponse | undefined; loading: boolean }) {
  if (loading) {
    return (
      <div className="flex flex-col items-center gap-2">
        <div className="bg-muted h-16 w-16 animate-pulse rounded-full" />
        <div className="bg-muted h-3 w-20 animate-pulse rounded" />
      </div>
    );
  }
  if (!score) return <div className="text-muted-foreground text-sm">No score</div>;

  return (
    <div className="flex flex-col items-center gap-2">
      <div className={`flex h-16 w-16 flex-col items-center justify-center rounded-full border-2 border-current ${scoreColor(score.overallScore)}`}>
        <span className="text-xl font-bold leading-none">{score.overallScore}</span>
        <span className="text-xs">/100</span>
      </div>
      <p className="text-muted-foreground text-xs">Un-personalized</p>
      <div className="w-full space-y-0.5 text-center">
        <MiniFactorRow factor={score.value} label="Value" />
        <MiniFactorRow factor={score.efficiency} label="Efficiency" />
        <MiniFactorRow factor={score.reliability} label="Reliability" />
        <MiniFactorRow factor={score.mileage} label="Mileage" />
      </div>
    </div>
  );
}

export function CompareScoreBanner({ listingAId, listingBId }: CompareScoreBannerProps) {
  const { data: aData, isLoading: aLoading } = useQuery(getListingScoreOptions(listingAId));
  const { data: bData, isLoading: bLoading } = useQuery(getListingScoreOptions(listingBId));

  return (
    <div className="bg-card border-border mb-4 rounded-lg border p-4">
      <div className="grid grid-cols-[1fr_auto_1fr] items-start gap-4">
        <ScoreColumn score={aData?.data} loading={aLoading} />
        <div className="flex flex-col items-center gap-2 pt-16 text-xs">
          <div className="text-muted-foreground">Value</div>
          <div className="text-muted-foreground">Efficiency</div>
          <div className="text-muted-foreground">Reliability</div>
          <div className="text-muted-foreground">Mileage</div>
        </div>
        <ScoreColumn score={bData?.data} loading={bLoading} />
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Export CompareScoreBanner from the compareListings feature index**

Find the `compareListings` feature `index.ts` (likely at `automotive.marketplace.client/src/features/compareListings/index.ts`) and add:
```typescript
export { CompareScoreBanner } from "./components/CompareScoreBanner";
```

- [ ] **Step 3: Add CompareScoreBanner to Compare page**

In `automotive.marketplace.client/src/app/pages/Compare.tsx`:

Add import:
```typescript
import { CompareScoreBanner } from "@/features/compareListings";
```

Add `CompareScoreBanner` between `CompareHeader` and `CompareTable` in the return JSX (inside the happy-path render after the `listingA`, `listingB` variables are destructured):
```tsx
<CompareScoreBanner listingAId={listingA.id} listingBId={listingB.id} />
```

Note: check what type `.id` is on `GetListingByIdResponse` — it might need `.toString()` if it's a `string` UUID, which it already will be. Also verify that `listingA.id` is the field name in the response (it was confirmed `listingA.Id` but JSON serialization in .NET defaults to camelCase, so frontend receives `id`).

- [ ] **Step 4: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 5: Run frontend lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -15
```
Expected: no errors or only pre-existing warnings.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx \
        automotive.marketplace.client/src/features/compareListings/index.ts \
        automotive.marketplace.client/src/app/pages/Compare.tsx
git commit -m "feat: add CompareScoreBanner to comparison page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 12: Run full test suite and verify

- [ ] **Step 1: Run all backend tests**

```bash
dotnet test ./Automotive.Marketplace.sln -q 2>&1 | tail -20
```
Expected: All tests pass.

- [ ] **Step 2: Build frontend**

```bash
cd automotive.marketplace.client && npm run build && npm run lint 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 3: Final commit**

```bash
git add -A
git commit -m "chore: Plan 1 complete — CarDog integration and score engine

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
