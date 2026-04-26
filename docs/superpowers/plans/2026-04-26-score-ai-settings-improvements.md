# Score, AI & Settings Improvements — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix gaps in CarDog Score Engine, AI Summaries, and Personalization Quiz — add Settings page, Condition factor, AI enrichment with CarDog data + language, bug fixes, and i18n.

**Architecture:** Changes span all four layers (Domain → Application → Server → Frontend). Backend adds `EnableVehicleScoring` + `ConditionWeight` to `UserPreferences`, `Language` to `ListingAiSummaryCache`, Condition factor in calculator, CarDog data + language in AI prompts. Frontend adds `/settings` route, gates ScoreCard on `EnableVehicleScoring`, adds Condition slider to QuizModal, passes `lang` to AI endpoints, and extracts all hardcoded strings into i18next.

**Tech Stack:** ASP.NET Core 8, EF Core, MediatR, PostgreSQL, React 19, TanStack Query/Router, i18next, shadcn/ui

---

## File Structure

**Backend — Modified:**
- `Automotive.Marketplace.Domain/Entities/UserPreferences.cs` — add `EnableVehicleScoring`, `ConditionWeight`
- `Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs` — add `Language`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs` — configure new columns
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs` — add `Language` to unique index
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs` — add new fields
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs` — persist new fields, 5-weight validation
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs` — add new fields
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs` — return new fields
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs` — add Condition factor, update default weights
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs` — add `Condition` ScoreFactor, extend `ScoreWeights`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs` — Include Defects, pass defect count
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs` — add `Language`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryResponse.cs` — add `UnavailableFactors`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs` — load CarDog cache, defects, enrich prompt, language, cache per language
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs` — add `Language`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryResponse.cs` — add `UnavailableFactors`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs` — enrich prompt, language, cache per language
- `Automotive.Marketplace.Server/Controllers/ListingController.cs` — accept `lang` param on AI summary endpoints

**Backend — New:**
- `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorConditionTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesWithNewFieldsTests.cs`
- EF migrations x2

**Frontend — New:**
- `src/app/pages/Settings.tsx`
- `src/app/routes/settings.tsx`
- `src/lib/i18n/locales/en/userPreferences.json`
- `src/lib/i18n/locales/lt/userPreferences.json`

**Frontend — Modified:**
- `src/features/userPreferences/types/UserPreferencesResponse.ts` — add new fields
- `src/features/userPreferences/types/UpsertUserPreferencesCommand.ts` — add new fields
- `src/features/userPreferences/components/QuizModal.tsx` — add Condition slider, `initialStep`, i18n
- `src/features/userPreferences/index.ts` — export Settings page if needed
- `src/features/listingDetails/types/GetListingScoreResponse.ts` — add `condition` ScoreFactor
- `src/features/listingDetails/types/GetListingAiSummaryResponse.ts` — add `unavailableFactors`
- `src/features/listingDetails/components/ScoreCard.tsx` — add Condition bar, gate on `EnableVehicleScoring`, i18n
- `src/features/listingDetails/components/AiSummarySection.tsx` — pass `lang`, show unavailable factors, i18n
- `src/features/listingDetails/api/getListingAiSummaryOptions.ts` — add `lang` param
- `src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts` — add `unavailableFactors`
- `src/features/compareListings/components/CompareScoreBanner.tsx` — gate on `EnableVehicleScoring`, i18n
- `src/features/compareListings/components/CompareAiSummary.tsx` — pass `lang`, show unavailable factors, i18n
- `src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts` — add `lang` param
- `src/components/layout/header/UserMenu.tsx` — activate Settings link
- `src/lib/i18n/i18n.ts` — register `userPreferences` namespace
- `src/lib/i18n/locales/en/toasts.json` — add `preferences` keys
- `src/lib/i18n/locales/lt/toasts.json` — add `preferences` keys
- `src/lib/i18n/locales/en/listings.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/lt/listings.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/en/compare.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/lt/compare.json` — add `aiSummary.*` keys

---

### Task 1: Domain — Add `EnableVehicleScoring` and `ConditionWeight` to `UserPreferences`

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/UserPreferences.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs`

- [ ] **Step 1: Add new properties to `UserPreferences` entity**

```csharp
// Automotive.Marketplace.Domain/Entities/UserPreferences.cs
namespace Automotive.Marketplace.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }

    public double ValueWeight { get; set; } = 0.26;
    public double EfficiencyWeight { get; set; } = 0.21;
    public double ReliabilityWeight { get; set; } = 0.21;
    public double MileageWeight { get; set; } = 0.17;
    public double ConditionWeight { get; set; } = 0.15;

    public bool AutoGenerateAiSummary { get; set; }
    public bool EnableVehicleScoring { get; set; }

    public virtual User User { get; set; } = null!;
}
```

Note: Default weights change to the new 5-factor defaults (sum = 1.0). Existing rows in DB will have old values which is fine — they get the new `ConditionWeight` default via migration.

- [ ] **Step 2: No config changes needed**

`UserPreferencesConfiguration.cs` doesn't need changes — the new properties are simple scalars that EF Core auto-discovers. The `HasKey`, `HasIndex`, and `HasOne` relationships remain the same.

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: add EnableVehicleScoring and ConditionWeight to UserPreferences entity

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Domain — Add `Language` to `ListingAiSummaryCache`

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs`

- [ ] **Step 1: Add `Language` property to entity**

```csharp
// Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs
namespace Automotive.Marketplace.Domain.Entities;

public class ListingAiSummaryCache : BaseEntity
{
    public Guid ListingId { get; set; }
    public string SummaryType { get; set; } = string.Empty;
    public Guid? ComparisonListingId { get; set; }
    public string Language { get; set; } = "lt";
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 2: Update EF configuration to include `Language` in unique index**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingAiSummaryCacheConfiguration : IEntityTypeConfiguration<ListingAiSummaryCache>
{
    public void Configure(EntityTypeBuilder<ListingAiSummaryCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SummaryType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Summary).IsRequired();
        builder.Property(e => e.Language).HasMaxLength(8).HasDefaultValue("lt");
        builder.HasIndex(e => new { e.ListingId, e.SummaryType, e.ComparisonListingId, e.Language }).IsUnique();
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: add Language column to ListingAiSummaryCache with composite unique index

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: EF Migrations

**Files:**
- Create: two new migrations in `Automotive.Marketplace.Infrastructure/Migrations/`

- [ ] **Step 1: Create migration for UserPreferences changes**

Run from repo root:
```bash
dotnet ef migrations add AddUserPreferencesScoreAndCondition \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```
Expected: Migration files created in `Infrastructure/Migrations/`

- [ ] **Step 2: Create migration for ListingAiSummaryCache Language column**

```bash
dotnet ef migrations add AddLanguageToAiSummaryCache \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```
Expected: Migration files created

- [ ] **Step 3: Build to verify both migrations compile**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "chore: add EF migrations for UserPreferences and AiSummaryCache changes

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Backend — Extend UpsertUserPreferences with new fields

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs`

- [ ] **Step 1: Add new fields to command**

```csharp
// UpsertUserPreferencesCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommand : IRequest
{
    public Guid UserId { get; set; }
    public double ValueWeight { get; set; }
    public double EfficiencyWeight { get; set; }
    public double ReliabilityWeight { get; set; }
    public double MileageWeight { get; set; }
    public double ConditionWeight { get; set; }
    public bool AutoGenerateAiSummary { get; set; }
    public bool EnableVehicleScoring { get; set; }
}
```

- [ ] **Step 2: Update handler to validate 5 weights and persist new fields**

```csharp
// UpsertUserPreferencesCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommandHandler(IRepository repository)
    : IRequestHandler<UpsertUserPreferencesCommand>
{
    public async Task Handle(UpsertUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var total = request.ValueWeight + request.EfficiencyWeight + request.ReliabilityWeight
                    + request.MileageWeight + request.ConditionWeight;
        if (Math.Abs(total - 1.0) > 0.01)
            throw new ArgumentException("Score weights must sum to 1.0");

        var existing = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (existing != null)
        {
            existing.ValueWeight = request.ValueWeight;
            existing.EfficiencyWeight = request.EfficiencyWeight;
            existing.ReliabilityWeight = request.ReliabilityWeight;
            existing.MileageWeight = request.MileageWeight;
            existing.ConditionWeight = request.ConditionWeight;
            existing.AutoGenerateAiSummary = request.AutoGenerateAiSummary;
            existing.EnableVehicleScoring = request.EnableVehicleScoring;
            await repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await repository.CreateAsync(new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ValueWeight = request.ValueWeight,
                EfficiencyWeight = request.EfficiencyWeight,
                ReliabilityWeight = request.ReliabilityWeight,
                MileageWeight = request.MileageWeight,
                ConditionWeight = request.ConditionWeight,
                AutoGenerateAiSummary = request.AutoGenerateAiSummary,
                EnableVehicleScoring = request.EnableVehicleScoring,
            }, cancellationToken);
        }
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: extend UpsertUserPreferences with ConditionWeight and EnableVehicleScoring

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Backend — Extend GetUserPreferences response with new fields

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs`

- [ ] **Step 1: Add new fields to response**

```csharp
// GetUserPreferencesResponse.cs
namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesResponse
{
    public double ValueWeight { get; init; }
    public double EfficiencyWeight { get; init; }
    public double ReliabilityWeight { get; init; }
    public double MileageWeight { get; init; }
    public double ConditionWeight { get; init; }
    public bool AutoGenerateAiSummary { get; init; }
    public bool EnableVehicleScoring { get; init; }
    public bool HasPreferences { get; init; }
}
```

- [ ] **Step 2: Update handler to return new fields with defaults**

```csharp
// GetUserPreferencesQueryHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IRepository repository)
    : IRequestHandler<GetUserPreferencesQuery, GetUserPreferencesResponse>
{
    public async Task<GetUserPreferencesResponse> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (prefs is null)
        {
            return new GetUserPreferencesResponse
            {
                ValueWeight = 0.26,
                EfficiencyWeight = 0.21,
                ReliabilityWeight = 0.21,
                MileageWeight = 0.17,
                ConditionWeight = 0.15,
                AutoGenerateAiSummary = false,
                EnableVehicleScoring = false,
                HasPreferences = false,
            };
        }

        return new GetUserPreferencesResponse
        {
            ValueWeight = prefs.ValueWeight,
            EfficiencyWeight = prefs.EfficiencyWeight,
            ReliabilityWeight = prefs.ReliabilityWeight,
            MileageWeight = prefs.MileageWeight,
            ConditionWeight = prefs.ConditionWeight,
            AutoGenerateAiSummary = prefs.AutoGenerateAiSummary,
            EnableVehicleScoring = prefs.EnableVehicleScoring,
            HasPreferences = true,
        };
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: extend GetUserPreferences with ConditionWeight and EnableVehicleScoring

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Backend — Add Condition factor to ListingScoreCalculator (TDD)

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorConditionTests.cs`

- [ ] **Step 1: Extend `ScoreWeights` and `GetListingScoreResponse` with Condition**

```csharp
// GetListingScoreResponse.cs
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreResponse
{
    public int OverallScore { get; init; }
    public ScoreFactor Value { get; init; } = null!;
    public ScoreFactor Efficiency { get; init; } = null!;
    public ScoreFactor Reliability { get; init; } = null!;
    public ScoreFactor Mileage { get; init; } = null!;
    public ScoreFactor Condition { get; init; } = null!;
    public bool HasMissingFactors { get; init; }
    public List<string> MissingFactors { get; init; } = [];
    public bool IsPersonalized { get; init; }
}

public record ScoreFactor(double Score, string Status, double Weight);

public record ScoreWeights(double Value, double Efficiency, double Reliability, double Mileage, double Condition);
```

- [ ] **Step 2: Write failing tests for Condition factor**

```csharp
// Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorConditionTests.cs
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using FluentAssertions;

namespace Automotive.Marketplace.Tests.Features.UnitTests;

public class ListingScoreCalculatorConditionTests
{
    [Fact]
    public void Calculate_ZeroDefects_ConditionScore100()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 0,
            market: null,
            efficiency: null,
            reliability: null);

        result.Condition.Score.Should().BeApproximately(100.0, 0.5);
        result.Condition.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_ThreeDefects_ConditionScore40()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 3,
            market: null,
            efficiency: null,
            reliability: null);

        // max(0, 100 - 3 * 20) = 40
        result.Condition.Score.Should().BeApproximately(40.0, 0.5);
        result.Condition.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_FiveOrMoreDefects_ConditionScore0()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 5,
            market: null,
            efficiency: null,
            reliability: null);

        result.Condition.Score.Should().BeApproximately(0.0, 0.5);
    }

    [Fact]
    public void Calculate_DefaultWeights_SumToOne()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 0,
            market: null,
            efficiency: null,
            reliability: null);

        var totalWeight = result.Value.Weight + result.Efficiency.Weight +
                          result.Reliability.Weight + result.Mileage.Weight + result.Condition.Weight;
        totalWeight.Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void Calculate_ConditionNeverMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 2,
            market: null,
            efficiency: null,
            reliability: null);

        result.Condition.Status.Should().Be("scored");
        result.MissingFactors.Should().NotContain("Condition");
    }

    [Fact]
    public void Calculate_WithCustomWeights_AppliesConditionWeight()
    {
        var weights = new ScoreWeights(0.20, 0.20, 0.20, 0.20, 0.20);
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            defectCount: 0,
            market: null,
            efficiency: null,
            reliability: null,
            weights: weights);

        result.Condition.Weight.Should().BeApproximately(0.20, 0.01);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~ListingScoreCalculatorConditionTests" ./Automotive.Marketplace.sln`
Expected: FAIL (compilation errors — `Calculate` doesn't accept `defectCount` yet)

- [ ] **Step 4: Update `ListingScoreCalculator` to add Condition factor**

```csharp
// ListingScoreCalculator.cs
using Automotive.Marketplace.Application.Interfaces.Services;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public static class ListingScoreCalculator
{
    private const double ValueWeight = 0.26;
    private const double EfficiencyWeight = 0.21;
    private const double ReliabilityWeight = 0.21;
    private const double MileageWeight = 0.17;
    private const double ConditionWeight = 0.15;

    public static GetListingScoreResponse Calculate(
        decimal listingPrice,
        int year,
        int mileageKm,
        int defectCount,
        CardogMarketResult? market,
        CardogEfficiencyResult? efficiency,
        CardogReliabilityResult? reliability,
        ScoreWeights? weights = null)
    {
        var w = weights ?? new ScoreWeights(ValueWeight, EfficiencyWeight, ReliabilityWeight, MileageWeight, ConditionWeight);

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
        var conditionFactor = ScoreCondition(defectCount, w.Condition);

        var allFactors = new[] { valueFactor, efficiencyFactor, reliabilityFactor, mileageFactor, conditionFactor };
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
            Condition = conditionFactor,
            HasMissingFactors = missingFactors.Count > 0,
            MissingFactors = missingFactors,
        };
    }

    private static ScoreFactor ScoreCondition(int defectCount, double weight)
    {
        var score = Math.Max(0, 100 - defectCount * 20);
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
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

- [ ] **Step 5: Fix existing `ListingScoreCalculatorTests` to pass `defectCount: 0`**

Update every call to `ListingScoreCalculator.Calculate(...)` in `ListingScoreCalculatorTests.cs` to include `defectCount: 0` as the 4th argument (after `mileageKm`). Also update assertions that check factor weights — they now use the new default weights. Add `.Condition` property assertions where `result` is checked.

Every existing test call like:
```csharp
var result = ListingScoreCalculator.Calculate(
    listingPrice: 8000m,
    year: 2020,
    mileageKm: 60000,
    market: ...,
    efficiency: ...,
    reliability: ...);
```
becomes:
```csharp
var result = ListingScoreCalculator.Calculate(
    listingPrice: 8000m,
    year: 2020,
    mileageKm: 60000,
    defectCount: 0,
    market: ...,
    efficiency: ...,
    reliability: ...);
```

- [ ] **Step 6: Fix existing `GetListingScoreQueryHandler` call site (temporary — will be fully updated in Task 7)**

In `GetListingScoreQueryHandler.cs` line 55, update the `Calculate` call to pass `defectCount: 0` temporarily:
```csharp
var scoreResult = ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, 0, market, efficiency, reliability, weights);
```

Also update the `ScoreWeights` construction (line 40) to include `ConditionWeight`:
```csharp
weights = new ScoreWeights(prefs.ValueWeight, prefs.EfficiencyWeight, prefs.ReliabilityWeight, prefs.MileageWeight, prefs.ConditionWeight);
```

And add `Condition = scoreResult.Condition,` to the response construction.

- [ ] **Step 7: Run all score tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ListingScoreCalculator" ./Automotive.Marketplace.sln`
Expected: ALL PASS

- [ ] **Step 8: Commit**

```bash
git add -A && git commit -m "feat: add Condition factor to ListingScoreCalculator with TDD tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Backend — Update GetListingScoreQueryHandler to include Defects

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingScoreQueryHandlerTests.cs`

- [ ] **Step 1: Write failing test for defect count in score**

Add to `GetListingScoreQueryHandlerTests.cs`:

```csharp
[Fact]
public async Task Handle_ListingWithDefects_ConditionFactorReflectsDefectCount()
{
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = CreateHandler(scope);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var listingId = await SeedListingAsync(context, makeName: "Volvo", modelName: "XC60", year: 2022);

    // Add 3 defects to the listing
    var listing = await context.Listings.FirstAsync(l => l.Id == listingId);
    for (var i = 0; i < 3; i++)
    {
        await context.ListingDefects.AddAsync(new Automotive.Marketplace.Domain.Entities.ListingDefect
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            CustomName = $"Defect {i + 1}",
        });
    }
    await context.SaveChangesAsync();

    _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns((CardogMarketResult?)null);
    _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns((CardogEfficiencyResult?)null);
    _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
        .Returns((CardogReliabilityResult?)null);

    var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

    // 3 defects → max(0, 100 - 3*20) = 40
    result.Condition.Status.Should().Be("scored");
    result.Condition.Score.Should().BeApproximately(40.0, 0.5);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~Handle_ListingWithDefects" ./Automotive.Marketplace.sln`
Expected: FAIL (handler doesn't include Defects yet or doesn't return `Condition`)

- [ ] **Step 3: Update handler to include Defects and pass defect count**

In `GetListingScoreQueryHandler.cs`, update the listing query to include Defects:

```csharp
var listing = await repository.AsQueryable<Listing>()
    .Include(l => l.Variant).ThenInclude(v => v.Fuel)
    .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
    .Include(l => l.Defects)
    .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
    ?? throw new DbEntityNotFoundException("Listing", request.ListingId);
```

Update the `Calculate` call:
```csharp
var defectCount = listing.Defects?.Count ?? 0;
var scoreResult = ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, defectCount, market, efficiency, reliability, weights);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~GetListingScoreQueryHandlerTests" ./Automotive.Marketplace.sln`
Expected: ALL PASS

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: include listing defects in score calculation handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Backend — Enrich AI Summary handler with CarDog data, defects, and language

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs`

- [ ] **Step 1: Add `Language` to query and `UnavailableFactors` to response**

```csharp
// GetListingAiSummaryQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryQuery : IRequest<GetListingAiSummaryResponse>
{
    public Guid ListingId { get; set; }
    public string Language { get; set; } = "lt";
}
```

```csharp
// GetListingAiSummaryResponse.cs
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryResponse
{
    public string? Summary { get; init; }
    public bool IsGenerated { get; init; }
    public bool FromCache { get; init; }
    public List<string> UnavailableFactors { get; init; } = [];
}
```

- [ ] **Step 2: Update handler to load CarDog cache, defects, add language to prompt and cache key**

```csharp
// GetListingAiSummaryQueryHandler.cs
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
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: enrich AI buyer summary with CarDog data, defects, language, and unavailable factors

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: Backend — Enrich Comparison AI Summary handler

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`

- [ ] **Step 1: Add `Language` to query and `UnavailableFactors` to response**

```csharp
// GetListingComparisonAiSummaryQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryQuery : IRequest<GetListingComparisonAiSummaryResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
    public string Language { get; set; } = "lt";
}
```

```csharp
// GetListingComparisonAiSummaryResponse.cs
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryResponse
{
    public string? Summary { get; init; }
    public bool IsGenerated { get; init; }
    public bool FromCache { get; init; }
    public List<string> UnavailableFactors { get; init; } = [];
}
```

- [ ] **Step 2: Update handler — same enrichment pattern as buyer handler**

Apply the same pattern as Task 8 to `GetListingComparisonAiSummaryQueryHandler.cs`:

1. Add `LanguageNames` dictionary
2. Add `lang` variable from `request.Language ?? "lt"`
3. Include `.Include(l => l.Defects)` in `LoadListingAsync`
4. Update cache lookup to filter on `Language == lang`
5. Load CarDog cache data for both listings (use listing A's make/model/year for cache — same vehicle type context)
6. Build enriched comparison prompt with CarDog data, defects, language instruction
7. Include `Language = language` when upserting cache
8. Return `UnavailableFactors` in response

The `LoadListingAsync` method:
```csharp
private async Task<Listing> LoadListingAsync(Guid id, CancellationToken ct) =>
    await repository.AsQueryable<Listing>()
        .Include(l => l.Variant).ThenInclude(v => v.Fuel)
        .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
        .Include(l => l.Defects)
        .FirstOrDefaultAsync(l => l.Id == id, ct)
    ?? throw new DbEntityNotFoundException("Listing", id);
```

The `BuildComparisonPrompt` method (includes all three CarDog cache types — market, efficiency, reliability — per spec):
```csharp
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
```

The handler `Handle` method must load all three cache types for both listings (using each listing's make/model/year) and track unavailable factors accordingly. Load pattern per listing:
```csharp
var makeA = listingA.Variant.Model.Make.Name;
var modelA = listingA.Variant.Model.Name;
var yearA = listingA.Year;

var marketA = await repository.AsQueryable<VehicleMarketCache>()
    .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
var efficiencyA = await repository.AsQueryable<VehicleEfficiencyCache>()
    .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
var reliabilityA = await repository.AsQueryable<VehicleReliabilityCache>()
    .FirstOrDefaultAsync(c => c.Make == makeA && c.Model == modelA && c.Year == yearA && c.ExpiresAt > DateTime.UtcNow, cancellationToken);
// Repeat for listing B
```

Unavailable factors: track at the listing-pair level. If either listing is missing market data, add "MarketValue". Same for efficiency and reliability.
```

The cache lookup and upsert add `Language` filtering/storage identically to Task 8.

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: enrich comparison AI summary with CarDog data, defects, language

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: Backend — Controller changes for `lang` param

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Add `lang` query param to AI summary endpoints**

Update the `GetAiSummary` endpoint:
```csharp
[HttpGet]
public async Task<ActionResult<GetListingAiSummaryResponse>> GetAiSummary(
    [FromQuery] GetListingAiSummaryQuery query,
    CancellationToken cancellationToken)
{
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

No change needed — `GetListingAiSummaryQuery` already has `Language` as a public property. ASP.NET model binding will automatically bind `?lang=...` if the query param name matches. However, the property is named `Language`, not `lang`.

We need to decide: either rename the query property to `Lang` or accept `?language=lt`. Per the spec, the endpoint accepts `?lang=`. So rename the property or add `[FromQuery(Name = "lang")]`. Simplest approach: the frontend can pass `language=lt` matching the property name. OR we add a separate param.

The cleanest approach: keep `Language` in the query class and have the frontend send `?language=lt`. This matches ASP.NET conventions without extra attributes.

- [ ] **Step 2: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Run all backend tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: ALL PASS

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: AI summary endpoints accept language query param via model binding

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: Frontend — i18n setup (new namespace + toast fixes + all translation keys)

**Files:**
- Create: `automotive.marketplace.client/src/lib/i18n/locales/en/userPreferences.json`
- Create: `automotive.marketplace.client/src/lib/i18n/locales/lt/userPreferences.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/i18n.ts`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/toasts.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/toasts.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/listings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/listings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/compare.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/compare.json`

- [ ] **Step 1: Create `userPreferences` namespace — English**

```json
// src/lib/i18n/locales/en/userPreferences.json
{
  "score": {
    "title": "Vehicle Score",
    "overallScore": "Overall Score",
    "value": "Market Value",
    "efficiency": "Efficiency",
    "reliability": "Reliability",
    "mileage": "Mileage",
    "condition": "Condition",
    "defects": "{{count}} known defect(s)",
    "defectsNone": "No known defects",
    "noData": "No data",
    "missingFactors": "Missing: {{factors}}",
    "personalized": "Personalized",
    "unPersonalized": "Un-personalized",
    "customize": "Personalize",
    "enableScoring": "Enable vehicle scoring in Settings to see a score for this listing.",
    "limitedNotice": "Score is limited — CarDog data is unavailable. Enable the API key for a full score."
  },
  "quiz": {
    "step0Title": "What describes your driving style?",
    "step1Title": "What matters most to you?",
    "step2Title": "Review your priorities",
    "styleCity": "City Commuter",
    "styleCityDesc": "Mostly urban stop-and-go driving",
    "styleHighway": "Highway Driver",
    "styleHighwayDesc": "Long-distance highway cruising",
    "styleMixed": "Mixed Use",
    "styleMixedDesc": "A bit of everything",
    "priorityValue": "Value for Money",
    "priorityEfficiency": "Eco-Friendly",
    "priorityReliability": "Dependability",
    "priorityMileage": "Low Mileage",
    "priorityCondition": "Vehicle Condition",
    "sliderTotal": "Total: {{total}}%",
    "normalizeNotice": "(will be normalized to 100%)",
    "save": "Save preferences",
    "saving": "Saving...",
    "back": "Back",
    "next": "Next"
  },
  "settings": {
    "title": "Settings",
    "listingPreferences": "Listing Preferences",
    "scoringLabel": "Vehicle Scoring",
    "scoringDescription": "Calculate scores for listings using CarDog market data",
    "scoringPreferencesLabel": "Scoring Preferences",
    "scoringPreferencesButton": "Customize",
    "aiSummaryLabel": "AI Summaries",
    "aiSummaryDescription": "Auto-generate AI buyer verdicts for listings"
  }
}
```

- [ ] **Step 2: Create `userPreferences` namespace — Lithuanian**

```json
// src/lib/i18n/locales/lt/userPreferences.json
{
  "score": {
    "title": "Automobilio Įvertinimas",
    "overallScore": "Bendras Įvertinimas",
    "value": "Rinkos Vertė",
    "efficiency": "Efektyvumas",
    "reliability": "Patikimumas",
    "mileage": "Rida",
    "condition": "Būklė",
    "defects": "{{count}} žinomas(-i) defektas(-ai)",
    "defectsNone": "Jokių žinomų defektų",
    "noData": "Nėra duomenų",
    "missingFactors": "Trūksta: {{factors}}",
    "personalized": "Personalizuotas",
    "unPersonalized": "Nepersonalizuotas",
    "customize": "Pritaikyti",
    "enableScoring": "Įjunkite automobilio įvertinimą Nustatymuose, kad matytumėte šio skelbimo balą.",
    "limitedNotice": "Balas ribotas — CarDog duomenys neprieinami. Įjunkite API raktą pilnam balui."
  },
  "quiz": {
    "step0Title": "Koks jūsų vairavimo stilius?",
    "step1Title": "Kas jums svarbiausia?",
    "step2Title": "Peržiūrėkite savo prioritetus",
    "styleCity": "Miesto vairuotojas",
    "styleCityDesc": "Daugiausia miesto eismo kamščiai",
    "styleHighway": "Magistralės vairuotojas",
    "styleHighwayDesc": "Ilgų atstumų kelionės greitkeliu",
    "styleMixed": "Mišrus naudojimas",
    "styleMixedDesc": "Viskas po truputį",
    "priorityValue": "Kainos ir vertės santykis",
    "priorityEfficiency": "Ekologiškumas",
    "priorityReliability": "Patikimumas",
    "priorityMileage": "Maža rida",
    "priorityCondition": "Automobilio būklė",
    "sliderTotal": "Iš viso: {{total}}%",
    "normalizeNotice": "(bus normalizuota iki 100%)",
    "save": "Išsaugoti nustatymus",
    "saving": "Saugoma...",
    "back": "Atgal",
    "next": "Toliau"
  },
  "settings": {
    "title": "Nustatymai",
    "listingPreferences": "Skelbimų Nustatymai",
    "scoringLabel": "Automobilio Įvertinimas",
    "scoringDescription": "Skaičiuoti skelbimų balus naudojant CarDog rinkos duomenis",
    "scoringPreferencesLabel": "Įvertinimo Nustatymai",
    "scoringPreferencesButton": "Pritaikyti",
    "aiSummaryLabel": "AI Apžvalgos",
    "aiSummaryDescription": "Automatiškai generuoti AI pirkėjo verdiktus skelbimams"
  }
}
```

- [ ] **Step 3: Register `userPreferences` namespace in i18n.ts**

Add to imports:
```typescript
import userPreferencesEn from "./locales/en/userPreferences.json";
import userPreferencesLt from "./locales/lt/userPreferences.json";
```

Add to resources:
```typescript
en: {
  // ... existing
  userPreferences: userPreferencesEn,
},
lt: {
  // ... existing
  userPreferences: userPreferencesLt,
},
```

- [ ] **Step 4: Add missing `preferences` keys to `toasts.json` (en)**

Add to the end of `en/toasts.json`:
```json
"preferences": {
  "saved": "Preferences saved!",
  "saveError": "Failed to save preferences"
}
```

- [ ] **Step 5: Add missing `preferences` keys to `toasts.json` (lt)**

Add to the end of `lt/toasts.json`:
```json
"preferences": {
  "saved": "Nustatymai išsaugoti!",
  "saveError": "Nepavyko išsaugoti nustatymų"
}
```

- [ ] **Step 6: Add `aiSummary` keys to `listings.json` (en)**

Add to `en/listings.json`:
```json
"aiSummary": {
  "title": "AI Buyer Verdict",
  "generate": "Generate",
  "regenerate": "Regenerate",
  "loading": "Generating AI summary...",
  "unavailableFactors": "Some data was unavailable ({{factors}}). The verdict may be incomplete.",
  "unavailable": "AI summary unavailable at this time.",
  "prompt": "Click \"Generate\" to get an AI-powered buyer verdict for this listing."
}
```

- [ ] **Step 7: Add `aiSummary` keys to `listings.json` (lt)**

Add to `lt/listings.json`:
```json
"aiSummary": {
  "title": "AI Pirkėjo Verdiktas",
  "generate": "Generuoti",
  "regenerate": "Pergeneruoti",
  "loading": "Generuojama AI apžvalga...",
  "unavailableFactors": "Kai kurie duomenys neprieinami ({{factors}}). Verdiktas gali būti nepilnas.",
  "unavailable": "AI apžvalga šiuo metu nepasiekiama.",
  "prompt": "Spauskite „Generuoti", kad gautumėte AI pirkėjo verdiktą šiam skelbimui."
}
```

- [ ] **Step 8: Add `aiSummary` keys to `compare.json` (en)**

Add to `en/compare.json`:
```json
"aiSummary": {
  "title": "AI Comparison Summary",
  "generate": "Compare with AI",
  "regenerate": "Regenerate",
  "unavailableFactors": "Some data was unavailable ({{factors}}). The comparison may be incomplete.",
  "prompt": "Click \"Compare with AI\" to get a recommendation between these two listings."
}
```

- [ ] **Step 9: Add `aiSummary` keys to `compare.json` (lt)**

Add to `lt/compare.json`:
```json
"aiSummary": {
  "title": "AI Palyginimo Apžvalga",
  "generate": "Palyginti su AI",
  "regenerate": "Pergeneruoti",
  "unavailableFactors": "Kai kurie duomenys neprieinami ({{factors}}). Palyginimas gali būti nepilnas.",
  "prompt": "Spauskite „Palyginti su AI", kad gautumėte rekomendaciją tarp šių dviejų skelbimų."
}
```

- [ ] **Step 10: Build frontend to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED

- [ ] **Step 11: Commit**

```bash
git add -A && git commit -m "feat: add userPreferences i18n namespace, toast keys, and AI summary translations

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 12: Frontend — Update TypeScript types

**Files:**
- Modify: `automotive.marketplace.client/src/features/userPreferences/types/UserPreferencesResponse.ts`
- Modify: `automotive.marketplace.client/src/features/userPreferences/types/UpsertUserPreferencesCommand.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingAiSummaryResponse.ts`
- Modify: `automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts`

- [ ] **Step 1: Update `UserPreferencesResponse.ts`**

```typescript
export type UserPreferencesResponse = {
  valueWeight: number;
  efficiencyWeight: number;
  reliabilityWeight: number;
  mileageWeight: number;
  conditionWeight: number;
  autoGenerateAiSummary: boolean;
  enableVehicleScoring: boolean;
  hasPreferences: boolean;
};
```

- [ ] **Step 2: Update `UpsertUserPreferencesCommand.ts`**

```typescript
export type UpsertUserPreferencesCommand = {
  valueWeight: number;
  efficiencyWeight: number;
  reliabilityWeight: number;
  mileageWeight: number;
  conditionWeight: number;
  autoGenerateAiSummary: boolean;
  enableVehicleScoring: boolean;
};
```

- [ ] **Step 3: Update `GetListingScoreResponse.ts`**

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
  condition: ScoreFactor;
  hasMissingFactors: boolean;
  missingFactors: string[];
  isPersonalized: boolean;
};
```

- [ ] **Step 4: Update `GetListingAiSummaryResponse.ts`**

```typescript
export type GetListingAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
  unavailableFactors: string[];
};
```

- [ ] **Step 5: Update `GetListingComparisonAiSummaryResponse.ts`**

```typescript
export type GetListingComparisonAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
  unavailableFactors: string[];
};
```

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: update frontend types for Condition factor, EnableVehicleScoring, and UnavailableFactors

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: Frontend — Settings page + route + UserMenu activation

**Files:**
- Create: `automotive.marketplace.client/src/app/pages/Settings.tsx`
- Create: `automotive.marketplace.client/src/app/routes/settings.tsx`
- Modify: `automotive.marketplace.client/src/components/layout/header/UserMenu.tsx`

- [ ] **Step 1: Create `Settings.tsx` page**

```tsx
// src/app/pages/Settings.tsx
import { useQuery } from "@tanstack/react-query";
import { BarChart2, SlidersHorizontal, Sparkles } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { getUserPreferencesOptions, QuizModal } from "@/features/userPreferences";
import { useUpsertUserPreferences } from "@/features/userPreferences/api/useUpsertUserPreferences";

export default function Settings() {
  const { t } = useTranslation("userPreferences");
  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const { mutateAsync: upsert } = useUpsertUserPreferences();
  const [quizOpen, setQuizOpen] = useState(false);

  const prefs = prefsData?.data;

  const handleScoringToggle = async (checked: boolean) => {
    if (!prefs) return;
    await upsert({
      valueWeight: prefs.valueWeight,
      efficiencyWeight: prefs.efficiencyWeight,
      reliabilityWeight: prefs.reliabilityWeight,
      mileageWeight: prefs.mileageWeight,
      conditionWeight: prefs.conditionWeight,
      autoGenerateAiSummary: prefs.autoGenerateAiSummary,
      enableVehicleScoring: checked,
    });
  };

  const handleAiSummaryToggle = async (checked: boolean) => {
    if (!prefs) return;
    await upsert({
      valueWeight: prefs.valueWeight,
      efficiencyWeight: prefs.efficiencyWeight,
      reliabilityWeight: prefs.reliabilityWeight,
      mileageWeight: prefs.mileageWeight,
      conditionWeight: prefs.conditionWeight,
      autoGenerateAiSummary: checked,
      enableVehicleScoring: prefs.enableVehicleScoring,
    });
  };

  const scoringEnabled = prefs?.enableVehicleScoring ?? false;

  return (
    <div className="container mx-auto max-w-2xl py-8">
      <h1 className="mb-6 text-2xl font-bold">{t("settings.title")}</h1>
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{t("settings.listingPreferences")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-0">
          {/* Vehicle Scoring */}
          <div className="flex items-center justify-between py-4">
            <div className="flex items-center gap-3">
              <BarChart2 className="text-muted-foreground h-5 w-5" />
              <div>
                <p className="text-sm font-medium">{t("settings.scoringLabel")}</p>
                <p className="text-muted-foreground text-xs">{t("settings.scoringDescription")}</p>
              </div>
            </div>
            <Switch checked={scoringEnabled} onCheckedChange={handleScoringToggle} />
          </div>

          {/* Scoring Preferences (indented) */}
          <div className={`flex items-center justify-between py-3 pl-8 ${!scoringEnabled ? "opacity-50" : ""}`}>
            <div className="flex items-center gap-3">
              <SlidersHorizontal className="text-muted-foreground h-4 w-4" />
              <p className="text-sm font-medium">{t("settings.scoringPreferencesLabel")}</p>
            </div>
            <Button
              variant="outline"
              size="sm"
              disabled={!scoringEnabled}
              onClick={() => setQuizOpen(true)}
            >
              {t("settings.scoringPreferencesButton")}
            </Button>
          </div>

          <Separator />

          {/* AI Summaries */}
          <div className="flex items-center justify-between py-4">
            <div className="flex items-center gap-3">
              <Sparkles className="text-muted-foreground h-5 w-5" />
              <div>
                <p className="text-sm font-medium">{t("settings.aiSummaryLabel")}</p>
                <p className="text-muted-foreground text-xs">{t("settings.aiSummaryDescription")}</p>
              </div>
            </div>
            <Switch
              checked={prefs?.autoGenerateAiSummary ?? false}
              onCheckedChange={handleAiSummaryToggle}
            />
          </div>
        </CardContent>
      </Card>

      <QuizModal
        open={quizOpen}
        onOpenChange={setQuizOpen}
        initialWeights={prefs?.hasPreferences ? {
          valueWeight: prefs.valueWeight,
          efficiencyWeight: prefs.efficiencyWeight,
          reliabilityWeight: prefs.reliabilityWeight,
          mileageWeight: prefs.mileageWeight,
          conditionWeight: prefs.conditionWeight,
        } : undefined}
        initialStep={prefs?.hasPreferences ? 2 : undefined}
      />
    </div>
  );
}
```

- [ ] **Step 2: Create route file `settings.tsx`**

```tsx
// src/app/routes/settings.tsx
import Settings from "@/app/pages/Settings";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { store } from "@/lib/redux/store";

export const Route = createFileRoute("/settings")({
  beforeLoad: () => {
    const { auth } = store.getState();
    if (!auth.userId) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error
      throw redirect({ to: "/login" });
    }
  },
  component: Settings,
});
```

- [ ] **Step 3: Activate Settings link in UserMenu**

Replace the disabled "Profile Settings" `DropdownMenuItem` in `UserMenu.tsx`:

Replace:
```tsx
<DropdownMenuItem disabled>
  <Settings className="mr-2 h-4 w-4" />
  {t("common:userMenu.profileSettings")}
  <span className="text-muted-foreground ml-auto text-xs">
    ({t("common:userMenu.comingSoon")})
  </span>
</DropdownMenuItem>
```

With:
```tsx
<DropdownMenuItem asChild>
  <Link to="/settings">
    <Settings className="mr-2 h-4 w-4" />
    {t("common:userMenu.profileSettings")}
  </Link>
</DropdownMenuItem>
```

- [ ] **Step 4: Build frontend to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED (may have type errors from QuizModal props — will fix in next task)

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: add Settings page with route and activate UserMenu link

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 14: Frontend — Update QuizModal with Condition slider, initialStep, and i18n

**Files:**
- Modify: `automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx`

- [ ] **Step 1: Update QuizModal props, types, and logic**

Key changes:
1. Add `conditionWeight` to `initialWeights` type
2. Add `initialStep?: number` prop
3. Add `"condition"` to `Priority` type and `PRIORITIES` array
4. Update `STYLE_ADJUSTMENTS` to include `condition`
5. Update `PRIORITY_BONUSES` for 5 items
6. Update `computeSliders` and `normalizeSliders` for 5 keys
7. Add `conditionWeight` to `handleSave` mutation
8. Preserve `enableVehicleScoring` and `autoGenerateAiSummary` from prefs during save
9. i18n all hardcoded strings

```tsx
// src/features/userPreferences/components/QuizModal.tsx
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import {
  Car,
  Gauge,
  Compass,
  BadgeDollarSign,
  Leaf,
  ShieldCheck,
  TrendingDown,
  ShieldAlert,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useUpsertUserPreferences } from "../api/useUpsertUserPreferences";
import { getUserPreferencesOptions } from "../api/getUserPreferencesOptions";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialWeights?: {
    valueWeight: number;
    efficiencyWeight: number;
    reliabilityWeight: number;
    mileageWeight: number;
    conditionWeight: number;
  };
  initialStep?: number;
};

type DrivingStyle = "city" | "highway" | "mixed";
type Priority = "value" | "efficiency" | "reliability" | "mileage" | "condition";

const STYLE_ADJUSTMENTS: Record<DrivingStyle, Record<Priority, number>> = {
  city: { efficiency: 15, reliability: 10, value: -15, mileage: -10, condition: 0 },
  highway: { efficiency: 15, value: 10, reliability: -15, mileage: -10, condition: 0 },
  mixed: { efficiency: 0, value: 0, reliability: 0, mileage: 0, condition: 0 },
};

const PRIORITY_BONUSES = [15, 10, 5, 3, 0];

function computeSliders(style: DrivingStyle, priorities: Priority[]): Record<Priority, number> {
  const base: Record<Priority, number> = { value: 20, efficiency: 20, reliability: 20, mileage: 20, condition: 20 };
  const adj = STYLE_ADJUSTMENTS[style];
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.max(5, Math.min(60, base[k] + (adj[k] ?? 0)));
  });
  priorities.forEach((p, i) => {
    base[p] = Math.min(60, base[p] + (PRIORITY_BONUSES[i] ?? 0));
  });
  const total = Object.values(base).reduce((a, b) => a + b, 0);
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.round((base[k] / total) * 100);
  });
  return base;
}

function normalizeSliders(sliders: Record<Priority, number>): Record<Priority, number> {
  const total = Object.values(sliders).reduce((a, b) => a + b, 0);
  if (total === 0) return { value: 20, efficiency: 20, reliability: 20, mileage: 20, condition: 20 };
  const result = { ...sliders };
  (Object.keys(result) as Priority[]).forEach((k) => {
    result[k] = Math.round((result[k] / total) * 100);
  });
  return result;
}

export function QuizModal({ open, onOpenChange, initialWeights, initialStep }: Props) {
  const { t } = useTranslation("userPreferences");
  const [step, setStep] = useState(initialStep ?? 0);
  const [drivingStyle, setDrivingStyle] = useState<DrivingStyle>("mixed");
  const [priorities, setPriorities] = useState<Priority[]>([]);
  const [sliders, setSliders] = useState<Record<Priority, number>>(() => {
    if (initialWeights) {
      return {
        value: Math.round(initialWeights.valueWeight * 100),
        efficiency: Math.round(initialWeights.efficiencyWeight * 100),
        reliability: Math.round(initialWeights.reliabilityWeight * 100),
        mileage: Math.round(initialWeights.mileageWeight * 100),
        condition: Math.round(initialWeights.conditionWeight * 100),
      };
    }
    return { value: 20, efficiency: 20, reliability: 20, mileage: 20, condition: 20 };
  });

  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const { mutateAsync: upsert, isPending } = useUpsertUserPreferences();

  const DRIVING_STYLES = [
    { id: "city" as DrivingStyle, label: t("quiz.styleCity"), icon: Car, description: t("quiz.styleCityDesc") },
    { id: "highway" as DrivingStyle, label: t("quiz.styleHighway"), icon: Gauge, description: t("quiz.styleHighwayDesc") },
    { id: "mixed" as DrivingStyle, label: t("quiz.styleMixed"), icon: Compass, description: t("quiz.styleMixedDesc") },
  ];

  const PRIORITIES = [
    { id: "value" as Priority, label: t("quiz.priorityValue"), icon: BadgeDollarSign },
    { id: "efficiency" as Priority, label: t("quiz.priorityEfficiency"), icon: Leaf },
    { id: "reliability" as Priority, label: t("quiz.priorityReliability"), icon: ShieldCheck },
    { id: "mileage" as Priority, label: t("quiz.priorityMileage"), icon: TrendingDown },
    { id: "condition" as Priority, label: t("quiz.priorityCondition"), icon: ShieldAlert },
  ];

  const handleStyleSelect = (style: DrivingStyle) => setDrivingStyle(style);

  const handlePriorityToggle = (priority: Priority) => {
    setPriorities((prev) =>
      prev.includes(priority) ? prev.filter((p) => p !== priority) : [...prev, priority],
    );
  };

  const handleNextToStep2 = () => setStep(1);

  const handleNextToStep3 = () => {
    setSliders(computeSliders(drivingStyle, priorities));
    setStep(2);
  };

  const handleSliderChange = (key: Priority, value: number) => {
    setSliders((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    const normalized = normalizeSliders(sliders);
    const total = Object.values(normalized).reduce((a, b) => a + b, 0);
    const fraction = (v: number) => Math.round((v / total) * 1000) / 1000;
    const prefs = prefsData?.data;
    await upsert({
      valueWeight: fraction(normalized.value),
      efficiencyWeight: fraction(normalized.efficiency),
      reliabilityWeight: fraction(normalized.reliability),
      mileageWeight: fraction(normalized.mileage),
      conditionWeight: fraction(normalized.condition),
      autoGenerateAiSummary: prefs?.autoGenerateAiSummary ?? false,
      enableVehicleScoring: prefs?.enableVehicleScoring ?? false,
    });
    onOpenChange(false);
    setStep(initialStep ?? 0);
  };

  const sliderTotal = Object.values(sliders).reduce((a, b) => a + b, 0);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>
            {step === 0 && t("quiz.step0Title")}
            {step === 1 && t("quiz.step1Title")}
            {step === 2 && t("quiz.step2Title")}
          </DialogTitle>
        </DialogHeader>

        {step === 0 && (
          <div className="grid grid-cols-3 gap-3">
            {DRIVING_STYLES.map(({ id, label, icon: Icon, description }) => (
              <button
                key={id}
                onClick={() => handleStyleSelect(id)}
                className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                  drivingStyle === id
                    ? "border-primary bg-primary/5"
                    : "border-border hover:border-muted-foreground"
                }`}
              >
                <Icon className="h-6 w-6" />
                <span className="text-sm font-medium">{label}</span>
                <span className="text-muted-foreground text-xs">{description}</span>
              </button>
            ))}
          </div>
        )}

        {step === 1 && (
          <div className="grid grid-cols-2 gap-3">
            {PRIORITIES.map(({ id, label, icon: Icon }) => {
              const rank = priorities.indexOf(id);
              const isSelected = rank !== -1;
              return (
                <button
                  key={id}
                  onClick={() => handlePriorityToggle(id)}
                  className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                    isSelected
                      ? "border-primary bg-primary/5"
                      : "border-border hover:border-muted-foreground"
                  }`}
                >
                  <Icon className="h-5 w-5" />
                  <span className="text-sm font-medium">{label}</span>
                  {isSelected && (
                    <span className="bg-primary text-primary-foreground flex h-5 w-5 items-center justify-center rounded-full text-xs">
                      {rank + 1}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        )}

        {step === 2 && (
          <div className="space-y-4">
            <p className="text-muted-foreground text-sm">
              {t("quiz.sliderTotal", { total: sliderTotal })}
              {sliderTotal !== 100 && (
                <span className="ml-1 text-orange-500">{t("quiz.normalizeNotice")}</span>
              )}
            </p>
            {PRIORITIES.map(({ id, label }) => (
              <div key={id} className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span>{label}</span>
                  <span className="font-medium">{sliders[id]}%</span>
                </div>
                <Slider
                  min={5}
                  max={60}
                  step={5}
                  value={[sliders[id]]}
                  onValueChange={([v]) => handleSliderChange(id, v)}
                />
              </div>
            ))}
          </div>
        )}

        <DialogFooter className="gap-2">
          {step > 0 && (
            <Button variant="outline" onClick={() => setStep(step - 1)}>
              {t("quiz.back")}
            </Button>
          )}
          {step < 2 && (
            <Button onClick={step === 0 ? handleNextToStep2 : handleNextToStep3}>
              {t("quiz.next")}
            </Button>
          )}
          {step === 2 && (
            <Button onClick={handleSave} disabled={isPending}>
              {isPending ? t("quiz.saving") : t("quiz.save")}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
```

- [ ] **Step 2: Build to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat: update QuizModal with Condition slider, initialStep prop, and i18n

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 15: Frontend — Update ScoreCard with Condition bar, EnableVehicleScoring gate, and i18n

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx`

- [ ] **Step 1: Rewrite ScoreCard with all changes**

Key changes:
1. Gate score fetching on `prefs?.enableVehicleScoring`
2. Show enable prompt when scoring is disabled
3. Add Condition factor bar with defect count text
4. Show "limited score" notice when fewer than 2 factors are scored
5. i18n all hardcoded strings
6. Pass `conditionWeight` to QuizModal `initialWeights`

```tsx
// src/features/listingDetails/components/ScoreCard.tsx
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ChevronDown, ChevronUp, SlidersHorizontal, Info } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppSelector } from "@/hooks/redux";
import { QuizModal, getUserPreferencesOptions } from "@/features/userPreferences";
import { getListingScoreOptions } from "../api/getListingScoreOptions";
import type { ScoreFactor } from "../types/GetListingScoreResponse";

type ScoreCardProps = {
  listingId: string;
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function FactorBar({ label, factor, secondaryText, t }: { label: string; factor: ScoreFactor; secondaryText?: string; t: (key: string) => string }) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-between py-1 text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="text-muted-foreground flex items-center gap-1">
          <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
          {t("score.noData")}
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
      {secondaryText && (
        <p className="text-muted-foreground mt-0.5 text-xs">{secondaryText}</p>
      )}
    </div>
  );
}

export function ScoreCard({ listingId }: ScoreCardProps) {
  const { t } = useTranslation("userPreferences");
  const [expanded, setExpanded] = useState(false);
  const [quizOpen, setQuizOpen] = useState(false);
  const { userId } = useAppSelector((state) => state.auth);
  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const isAuthenticated = !!userId;
  const prefs = prefsData?.data;
  const scoringEnabled = prefs?.enableVehicleScoring ?? false;

  const { data, isLoading } = useQuery({
    ...getListingScoreOptions(listingId),
    enabled: scoringEnabled,
  });

  if (!scoringEnabled) {
    return (
      <div className="border-border rounded-lg border p-4">
        <p className="text-muted-foreground text-sm">
          {t("score.enableScoring")}
        </p>
      </div>
    );
  }

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
  const scoredCount = [score.value, score.efficiency, score.reliability, score.mileage, score.condition]
    .filter(f => f.status === "scored").length;

  // Compute defect count from condition score: score = max(0, 100 - count*20) → count = (100 - score) / 20
  const defectCount = score.condition.status === "scored"
    ? Math.round((100 - score.condition.score) / 20)
    : 0;
  const conditionSecondary = defectCount > 0
    ? t("score.defects", { count: defectCount })
    : t("score.defectsNone");

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
              <p className="font-semibold">{t("score.title")}</p>
              <p className="text-muted-foreground text-xs">
                {score.isPersonalized ? t("score.personalized") : t("score.unPersonalized")}
              </p>
            </div>
            <button
              onClick={() => setExpanded(!expanded)}
              className="text-muted-foreground hover:text-foreground"
              aria-label={expanded ? "Collapse score breakdown" : "Expand score breakdown"}
            >
              {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </button>
            {isAuthenticated && (
              <button
                onClick={() => setQuizOpen(true)}
                className="text-muted-foreground hover:text-foreground ml-2"
                aria-label="Personalize score weights"
              >
                <SlidersHorizontal className="h-4 w-4" />
              </button>
            )}
          </div>
          {score.hasMissingFactors && !expanded && (
            <p className="text-muted-foreground mt-1 flex items-center gap-1 text-xs">
              <AlertTriangle className="h-3 w-3 text-orange-400" />
              {t("score.missingFactors", { factors: score.missingFactors.join(", ") })}
            </p>
          )}
        </div>
      </div>

      {expanded && (
        <div className="mt-4 space-y-1 border-t pt-3">
          <FactorBar label={t("score.value")} factor={score.value} t={t} />
          <FactorBar label={t("score.efficiency")} factor={score.efficiency} t={t} />
          <FactorBar label={t("score.reliability")} factor={score.reliability} t={t} />
          <FactorBar label={t("score.mileage")} factor={score.mileage} t={t} />
          <FactorBar label={t("score.condition")} factor={score.condition} secondaryText={conditionSecondary} t={t} />
          {scoredCount < 3 && (
            <p className="text-muted-foreground mt-2 flex items-center gap-1 text-xs">
              <Info className="h-3 w-3" />
              {t("score.limitedNotice")}
            </p>
          )}
        </div>
      )}
      <QuizModal
        open={quizOpen}
        onOpenChange={setQuizOpen}
        initialWeights={prefs?.hasPreferences ? {
          valueWeight: prefs.valueWeight,
          efficiencyWeight: prefs.efficiencyWeight,
          reliabilityWeight: prefs.reliabilityWeight,
          mileageWeight: prefs.mileageWeight,
          conditionWeight: prefs.conditionWeight,
        } : undefined}
        initialStep={prefs?.hasPreferences ? 2 : undefined}
      />
    </div>
  );
}
```

- [ ] **Step 2: Build to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat: update ScoreCard with Condition bar, EnableVehicleScoring gate, and i18n

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 16: Frontend — Update CompareScoreBanner with EnableVehicleScoring gate and i18n

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx`

- [ ] **Step 1: Update CompareScoreBanner**

Key changes:
1. Gate score queries on `enableVehicleScoring`
2. Show enable prompt when disabled
3. Add Condition `MiniFactorRow`
4. Add "Condition" label in center column
5. i18n all hardcoded strings
6. Pass `conditionWeight` to QuizModal

Replace the `ScoreColumn` function to add `MiniFactorRow` for `condition`. Add the center label for "Condition" after "Mileage". Gate the entire banner: if `!scoringEnabled`, render the enable prompt instead.

Update the `QuizModal` `initialWeights` to include `conditionWeight`.

- [ ] **Step 2: Build to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat: update CompareScoreBanner with EnableVehicleScoring gate and Condition factor

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 17: Frontend — Update AI Summary components with lang param and unavailable factors

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx`
- Modify: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`

- [ ] **Step 1: Add `language` param to `getListingAiSummaryOptions`**

```typescript
// src/features/listingDetails/api/getListingAiSummaryOptions.ts
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingAiSummaryResponse } from "../types/GetListingAiSummaryResponse";

export const getListingAiSummaryOptions = (listingId: string, language: string = "lt") =>
  queryOptions({
    queryKey: listingKeys.aiSummary(listingId, language),
    queryFn: () =>
      axiosClient.get<GetListingAiSummaryResponse>(ENDPOINTS.LISTING.GET_AI_SUMMARY, {
        params: { listingId, language },
      }),
    enabled: false,
  });
```

- [ ] **Step 2: Add `language` param to `getListingComparisonAiSummaryOptions`**

```typescript
// src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonAiSummaryResponse } from "../types/GetListingComparisonAiSummaryResponse";

export const getListingComparisonAiSummaryOptions = (listingAId: string, listingBId: string, language: string = "lt") =>
  queryOptions({
    queryKey: listingKeys.comparisonAiSummary(listingAId, listingBId, language),
    queryFn: () =>
      axiosClient.get<GetListingComparisonAiSummaryResponse>(ENDPOINTS.LISTING.GET_COMPARISON_AI_SUMMARY, {
        params: { listingAId, listingBId, language },
      }),
    enabled: false,
  });
```

- [ ] **Step 2.5: Update `listingKeys` to include `language` in AI summary query keys**

```typescript
// src/api/queryKeys/listingKeys.ts
import { GetAllListingsQuery } from "@/features/listingList";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
  byId: (id: string) => [...listingKeys.all(), id],
  score: (id: string) => [...listingKeys.all(), id, "score"],
  aiSummary: (id: string, language: string = "lt") => [...listingKeys.all(), id, "ai-summary", language],
  comparisonAiSummary: (a: string, b: string, language: string = "lt") => [...listingKeys.all(), "comparison-ai-summary", a, b, language],
  sellerInsights: (id: string) => [...listingKeys.all(), id, "seller-insights"],
  comparison: (a: string, b: string) => [
    ...listingKeys.all(),
    "comparison",
    a,
    b,
  ],
  search: (q: string) => [...listingKeys.all(), "search", q],
};
```

This ensures TanStack Query busts the cache when the user switches language.

- [ ] **Step 3: Update `AiSummarySection` with lang, unavailable factors, i18n**

```tsx
// src/features/listingDetails/components/AiSummarySection.tsx
import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw, Info } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { getListingAiSummaryOptions } from "../api/getListingAiSummaryOptions";

type Props = {
  listingId: string;
};

export function AiSummarySection({ listingId }: Props) {
  const { t, i18n } = useTranslation("listings");

  const {
    data,
    isFetching,
    refetch,
  } = useQuery(getListingAiSummaryOptions(listingId, i18n.language));

  const summary = data?.data;
  const hasResult = summary?.isGenerated;
  const unavailable = summary?.unavailableFactors ?? [];

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">{t("aiSummary.title")}</span>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => refetch()}
          disabled={isFetching}
          className="flex items-center gap-1"
        >
          {isFetching ? (
            <RefreshCw className="h-3.5 w-3.5 animate-spin" />
          ) : (
            <Sparkles className="h-3.5 w-3.5" />
          )}
          {hasResult ? t("aiSummary.regenerate") : t("aiSummary.generate")}
        </Button>
      </div>

      {isFetching && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
          <div className="bg-muted h-3 w-3/5 animate-pulse rounded" />
        </div>
      )}

      {!isFetching && hasResult && summary?.summary && (
        <>
          <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
          {unavailable.length > 0 && (
            <Alert variant="default" className="mt-3">
              <Info className="h-4 w-4" />
              <AlertDescription className="text-xs">
                {t("aiSummary.unavailableFactors", { factors: unavailable.join(", ") })}
              </AlertDescription>
            </Alert>
          )}
        </>
      )}

      {!isFetching && data && !hasResult && (
        <p className="text-muted-foreground mt-3 text-sm italic">
          {t("aiSummary.unavailable")}
        </p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          {t("aiSummary.prompt")}
        </p>
      )}
    </div>
  );
}
```

- [ ] **Step 4: Update `CompareAiSummary` with lang, unavailable factors, i18n**

Same pattern as `AiSummarySection`:
1. Import `useTranslation` from `react-i18next`
2. Use `i18n.language` as `language` param
3. Show unavailable factors `Alert` when non-empty
4. i18n all hardcoded strings using `compare` namespace

```tsx
// src/features/compareListings/components/CompareAiSummary.tsx
import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw, Info } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { getListingComparisonAiSummaryOptions } from "../api/getListingComparisonAiSummaryOptions";

type Props = {
  listingAId: string;
  listingBId: string;
};

export function CompareAiSummary({ listingAId, listingBId }: Props) {
  const { t, i18n } = useTranslation("compare");

  const { data, isFetching, refetch } = useQuery(
    getListingComparisonAiSummaryOptions(listingAId, listingBId, i18n.language),
  );

  const summary = data?.data;
  const hasResult = summary?.isGenerated;
  const unavailable = summary?.unavailableFactors ?? [];

  return (
    <div className="border-border mb-4 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">{t("aiSummary.title")}</span>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => refetch()}
          disabled={isFetching}
          className="flex items-center gap-1"
        >
          {isFetching ? (
            <RefreshCw className="h-3.5 w-3.5 animate-spin" />
          ) : (
            <Sparkles className="h-3.5 w-3.5" />
          )}
          {hasResult ? t("aiSummary.regenerate") : t("aiSummary.generate")}
        </Button>
      </div>

      {isFetching && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
        </div>
      )}

      {!isFetching && hasResult && summary?.summary && (
        <>
          <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
          {unavailable.length > 0 && (
            <Alert variant="default" className="mt-3">
              <Info className="h-4 w-4" />
              <AlertDescription className="text-xs">
                {t("aiSummary.unavailableFactors", { factors: unavailable.join(", ") })}
              </AlertDescription>
            </Alert>
          )}
        </>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          {t("aiSummary.prompt")}
        </p>
      )}
    </div>
  );
}
```

- [ ] **Step 5: Build to verify**

Run: `cd automotive.marketplace.client && npm run build`
Expected: BUILD SUCCEEDED

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: pass language to AI summary endpoints and show unavailable factors with i18n

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 18: Final — Lint, build, test all

**Files:** None (validation only)

- [ ] **Step 1: Run backend build and tests**

```bash
dotnet build ./Automotive.Marketplace.sln
dotnet test ./Automotive.Marketplace.sln
```
Expected: ALL PASS

- [ ] **Step 2: Run frontend lint and build**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check && npm run build
```
Expected: No lint errors, BUILD SUCCEEDED

- [ ] **Step 3: Final commit if any fixes needed**

```bash
git add -A && git commit -m "chore: fix lint and build issues

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```