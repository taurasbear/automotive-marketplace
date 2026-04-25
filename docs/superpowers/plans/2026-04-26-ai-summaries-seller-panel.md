# AI Summaries & Seller Insights Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate OpenAI to generate buyer verdict summaries for individual listings and comparison summaries; add a seller insight panel on the My Listings page showing market position and listing quality cards.

**Architecture:** `IOpenAiClient` (typed HttpClient) calls `POST /v1/responses` on OpenAI. Summaries are cached in `ListingAiSummaryCache` (DB) with a 30-day TTL and invalidated when the listing is modified. Comparison summaries normalise the listing pair (`min(a,b)` / `max(a,b)`) so (A,B) and (B,A) share one cache row. Seller insights use the existing `VehicleMarketCache` (from Plan 1) and compute listing quality score from completeness heuristics.

**Prerequisites:** Plan 1 must be complete (score engine + CarDog caches). Plan 2's `UserPreferences.AutoGenerateAiSummary` field is used by the AI summary endpoint to decide whether to auto-generate on page load; the endpoint gracefully falls back to `false` if that field is absent (i.e., works without Plan 2).

**Tech Stack:** .NET 8, EF Core, MediatR, System.Net.Http.Json; React 19, TypeScript, TanStack Query, shadcn/ui, Lucide React.

---

## File Map

**New files — Backend:**
- `Automotive.Marketplace.Application/Interfaces/Services/IOpenAiClient.cs`
- `Automotive.Marketplace.Infrastructure/Services/OpenAiClient.cs`
- `Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryResponse.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryResponse.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsQuery.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsResponse.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsQueryHandler.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingAiSummaryQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonAiSummaryQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetSellerListingInsightsQueryHandlerTests.cs`

**Modified files — Backend:**
- `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — add `DbSet<ListingAiSummaryCache>`
- `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs` — register OpenAI client
- `Automotive.Marketplace.Server/Controllers/ListingController.cs` — add 3 new actions
- `Automotive.Marketplace.Server/appsettings.json` — add `OpenAI:ApiKey` placeholder

**New files — Frontend:**
- `automotive.marketplace.client/src/features/listingDetails/types/GetListingAiSummaryResponse.ts`
- `automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts`
- `automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts`
- `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`
- `automotive.marketplace.client/src/features/listingDetails/types/GetSellerListingInsightsResponse.ts`
- `automotive.marketplace.client/src/features/listingDetails/api/getSellerListingInsightsOptions.ts`
- `automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx`
- `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`
- `automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx`

**Modified files — Frontend:**
- `automotive.marketplace.client/src/constants/endpoints.ts` — add 3 endpoint constants
- `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts` — add `aiSummary`, `comparisonAiSummary`, `sellerInsights` keys
- `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx` — add `AiSummarySection`
- `automotive.marketplace.client/src/app/pages/Compare.tsx` — add `CompareAiSummary` below score banner
- `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx` — add `SellerInsightsPanel`

---

### Task 1: OpenAI Application interface

**Files:**
- Create: `Automotive.Marketplace.Application/Interfaces/Services/IOpenAiClient.cs`

- [ ] **Step 1: Create the interface**

```csharp
namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IOpenAiClient
{
    Task<string?> GetResponseAsync(string prompt, CancellationToken cancellationToken);
}
```

- [ ] **Step 2: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Application/Interfaces/Services/IOpenAiClient.cs
git commit -m "feat: add IOpenAiClient interface

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: OpenAiClient infrastructure implementation

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Services/OpenAiClient.cs`
- Modify: `Automotive.Marketplace.Server/appsettings.json`
- Modify: `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`

The OpenAI Responses API endpoint is `POST https://api.openai.com/v1/responses`. Request body: `{ "model": "gpt-5.4-mini", "input": "..." }`. Response shape:
```json
{
  "output": [
    {
      "content": [
        { "type": "output_text", "text": "..." }
      ]
    }
  ]
}
```

- [ ] **Step 1: Add appsettings placeholder**

In `Automotive.Marketplace.Server/appsettings.json`, add `OpenAI` section:

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
  },
  "OpenAI": {
    "ApiKey": ""
  }
}
```

- [ ] **Step 2: Create OpenAiClient**

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class OpenAiClient(HttpClient httpClient) : IOpenAiClient
{
    private const string Model = "gpt-5.4-mini";

    public async Task<string?> GetResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var request = new { model = Model, input = prompt };
            var response = await httpClient.PostAsJsonAsync("responses", request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken);
            return result?.Output?.FirstOrDefault()?.Content?.FirstOrDefault(c => c.Type == "output_text")?.Text;
        }
        catch
        {
            return null;
        }
    }

    private class OpenAiResponse
    {
        [JsonPropertyName("output")]
        public List<OutputItem> Output { get; set; } = [];
    }

    private class OutputItem
    {
        [JsonPropertyName("content")]
        public List<ContentItem> Content { get; set; } = [];
    }

    private class ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 3: Register in ServiceExtensions.cs**

Inside `ConfigureInfrastructure`, add near the other `AddHttpClient` registrations:

```csharp
var openAiApiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
services.AddHttpClient<IOpenAiClient, OpenAiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");
});
```

Add using directive at the top of `ServiceExtensions.cs` if not already present:
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
git add Automotive.Marketplace.Infrastructure/Services/OpenAiClient.cs \
        Automotive.Marketplace.Infrastructure/ServiceExtensions.cs \
        Automotive.Marketplace.Server/appsettings.json
git commit -m "feat: add OpenAiClient typed HttpClient

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: ListingAiSummaryCache entity + EF config + migration

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

The unique key is the composite `(ListingId, SummaryType, ComparisonListingId)`. For comparison summaries, the caller must ensure `ListingId < ComparisonListingId` (sort GUIDs before storing/querying).

- [ ] **Step 1: Create ListingAiSummaryCache entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class ListingAiSummaryCache : BaseEntity
{
    public Guid ListingId { get; set; }
    public string SummaryType { get; set; } = string.Empty;
    public Guid? ComparisonListingId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] **Step 2: Create EF configuration**

```csharp
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
        builder.HasIndex(e => new { e.ListingId, e.SummaryType, e.ComparisonListingId }).IsUnique();
    }
}
```

- [ ] **Step 3: Add DbSet to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add:
```csharp
public DbSet<ListingAiSummaryCache> ListingAiSummaryCaches { get; set; }
```

- [ ] **Step 4: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 5: Create and apply migration**

```bash
dotnet ef migrations add AddListingAiSummaryCache \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server

dotnet ef database update \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs \
        Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs \
        Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat: add ListingAiSummaryCache entity, EF config, and migration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: GetListingAiSummary handler + tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingAiSummaryQueryHandlerTests.cs`

Cache invalidation rule: if `listing.ModifiedAt > cache.GeneratedAt`, regenerate the summary. Otherwise use cache if not expired.

Prompt template for the buyer verdict:
```
You are an automotive assistant. Provide a brief, neutral buyer verdict in 2-3 sentences for this vehicle listing.
Vehicle: {year} {make} {model}
Listed price: {price} EUR | Mileage: {mileage} km | Fuel: {fuel} | Power: {powerKw} kW
Be direct and practical. Focus on value, practicality, and any notable considerations.
```

- [ ] **Step 1: Create query and response**

```csharp
// GetListingAiSummaryQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;

public class GetListingAiSummaryQuery : IRequest<GetListingAiSummaryResponse>
{
    public Guid ListingId { get; set; }
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
}
```

- [ ] **Step 2: Write the failing tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingAiSummaryQueryHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingAiSummaryQueryHandlerTests(
    DatabaseFixture<GetListingAiSummaryQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingAiSummaryQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingAiSummaryQueryHandlerTests> _fixture = fixture;
    private readonly IOpenAiClient _openAiClient = Substitute.For<IOpenAiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingAiSummaryQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>(), _openAiClient);

    private async Task<Guid> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().With(m => m.Name, "Toyota").Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Corolla").Build();
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
            .WithMunicipality(municipality.Id).WithYear(2021).WithPrice(12500m)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_NoCacheEntry_CallsOpenAiAndCachesResult()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);
        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("This Toyota Corolla represents solid value for money.");

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.IsGenerated.Should().BeTrue();
        result.Summary.Should().Contain("Toyota");
        result.FromCache.Should().BeFalse();

        var cached = await context.ListingAiSummaryCaches
            .FirstOrDefaultAsync(c => c.ListingId == listingId && c.SummaryType == "buyer");
        cached.Should().NotBeNull();
        cached!.Summary.Should().Contain("Toyota");
    }

    [Fact]
    public async Task Handle_ValidCacheEntry_ReturnsCachedSummaryWithoutCallingApi()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);

        await context.ListingAiSummaryCaches.AddAsync(new ListingAiSummaryCache
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            SummaryType = "buyer",
            ComparisonListingId = null,
            Summary = "Cached summary text.",
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.FromCache.Should().BeTrue();
        result.Summary.Should().Be("Cached summary text.");
        await _openAiClient.DidNotReceive().GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OpenAiReturnsNull_ReturnsNotGenerated()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);
        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.IsGenerated.Should().BeFalse();
        result.Summary.Should().BeNull();
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingAiSummaryQueryHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors.

- [ ] **Step 4: Create GetListingAiSummaryQueryHandler**

```csharp
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

        // Return cache if fresh and not invalidated by listing update
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
```

- [ ] **Step 5: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingAiSummaryQueryHandlerTests" -q
```
Expected: All 3 tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingAiSummaryQueryHandlerTests.cs
git commit -m "feat: add GetListingAiSummaryQueryHandler with DB caching

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: GetListingComparisonAiSummary handler + tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonAiSummaryQueryHandlerTests.cs`

Pair normalisation: `keyId = Min(listingAId, listingBId)`, `comparisonId = Max(listingAId, listingBId)`. This ensures the same cache row is found regardless of which listing is A and which is B.

Comparison prompt:
```
You are an automotive assistant. Compare these two vehicle listings and recommend which is the better buy in 2-3 sentences.
Vehicle A: {yearA} {makeA} {modelA} — {priceA} EUR, {mileageA} km, {fuelA}
Vehicle B: {yearB} {makeB} {modelB} — {priceB} EUR, {mileageB} km, {fuelB}
Give a direct recommendation with the main reason. Be concise.
```

- [ ] **Step 1: Create query and response**

```csharp
// GetListingComparisonAiSummaryQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;

public class GetListingComparisonAiSummaryQuery : IRequest<GetListingComparisonAiSummaryResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
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
}
```

- [ ] **Step 2: Write the failing tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonAiSummaryQueryHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingComparisonAiSummaryQueryHandlerTests(
    DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests> _fixture = fixture;
    private readonly IOpenAiClient _openAiClient = Substitute.For<IOpenAiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingComparisonAiSummaryQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>(), _openAiClient);

    private async Task<Guid> SeedListingAsync(AutomotiveContext context, string makeName, string modelName)
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
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_NoCacheEntry_CallsOpenAiAndCaches()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var idA = await SeedListingAsync(context, "Honda", "Civic");
        var idB = await SeedListingAsync(context, "Toyota", "Corolla");

        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("The Toyota Corolla is the better choice due to reliability.");

        var result = await handler.Handle(
            new GetListingComparisonAiSummaryQuery { ListingAId = idA, ListingBId = idB },
            CancellationToken.None);

        result.IsGenerated.Should().BeTrue();
        result.Summary.Should().NotBeNullOrEmpty();
        result.FromCache.Should().BeFalse();

        var keyId = idA < idB ? idA : idB;
        var compId = idA < idB ? idB : idA;
        var cached = await context.ListingAiSummaryCaches
            .FirstOrDefaultAsync(c => c.ListingId == keyId && c.ComparisonListingId == compId && c.SummaryType == "comparison");
        cached.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReversedOrder_ReturnsSameCachedResult()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var idA = await SeedListingAsync(context, "Honda", "Civic");
        var idB = await SeedListingAsync(context, "Toyota", "Corolla");

        var keyId = idA < idB ? idA : idB;
        var compId = idA < idB ? idB : idA;

        // Pre-seed cache
        await context.ListingAiSummaryCaches.AddAsync(new ListingAiSummaryCache
        {
            Id = Guid.NewGuid(),
            ListingId = keyId,
            ComparisonListingId = compId,
            SummaryType = "comparison",
            Summary = "Corolla is better.",
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.SaveChangesAsync();

        // Call with reversed order
        var result = await handler.Handle(
            new GetListingComparisonAiSummaryQuery { ListingAId = idB, ListingBId = idA },
            CancellationToken.None);

        result.FromCache.Should().BeTrue();
        result.Summary.Should().Be("Corolla is better.");
        await _openAiClient.DidNotReceive().GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
```

- [ ] **Step 3: Run to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingComparisonAiSummaryQueryHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors.

- [ ] **Step 4: Create GetListingComparisonAiSummaryQueryHandler**

```csharp
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
        // Normalise pair so (A,B) and (B,A) produce the same key
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
```

- [ ] **Step 5: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingComparisonAiSummaryQueryHandlerTests" -q
```
Expected: Both tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonAiSummaryQueryHandlerTests.cs
git commit -m "feat: add GetListingComparisonAiSummaryQueryHandler with pair normalisation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: GetSellerListingInsights handler + tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetSellerListingInsightsQueryHandlerTests.cs`

This handler checks that the requesting user is the listing's seller (throws `ForbiddenException` if not). It reads from `VehicleMarketCache` (no new CarDog calls). It computes listing quality from completeness heuristics.

**Listing quality scoring:**
| Field | Points |
|---|---|
| Has description (≥ 20 chars) | 20 |
| Has 3+ photos | 20 |
| Has VIN | 15 |
| Has colour | 10 |
| Has 5+ photos | 10 (on top of 3+ bonus) |
| Has description (≥ 100 chars) | 10 (on top of ≥20 bonus) |
| Is used vehicle (not new) | 5 (bonus for stating clearly it's used/new) |

Max total: 90 points shown as percentage (multiply by 100/90).

Suggestions are generated for each missing/weak point.

- [ ] **Step 1: Create query and response**

```csharp
// GetSellerListingInsightsQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsQuery : IRequest<GetSellerListingInsightsResponse>
{
    public Guid ListingId { get; set; }
    public Guid UserId { get; set; }
}
```

```csharp
// GetSellerListingInsightsResponse.cs
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsResponse
{
    public MarketPositionInsight MarketPosition { get; init; } = null!;
    public ListingQualityInsight ListingQuality { get; init; } = null!;
}

public class MarketPositionInsight
{
    public decimal ListingPrice { get; init; }
    public decimal? MarketMedianPrice { get; init; }
    public double? PriceDifferencePercent { get; init; }
    public int? MarketListingCount { get; init; }
    public int DaysListed { get; init; }
    public bool HasMarketData { get; init; }
}

public class ListingQualityInsight
{
    public int QualityScore { get; init; }
    public bool HasDescription { get; init; }
    public bool HasPhotos { get; init; }
    public int PhotoCount { get; init; }
    public bool HasVin { get; init; }
    public bool HasColour { get; init; }
    public List<string> Suggestions { get; init; } = [];
}
```

- [ ] **Step 2: Write the failing tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetSellerListingInsightsQueryHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetSellerListingInsightsQueryHandlerTests(
    DatabaseFixture<GetSellerListingInsightsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetSellerListingInsightsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetSellerListingInsightsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetSellerListingInsightsQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private async Task<(Guid listingId, Guid sellerId)> SeedListingAsync(AutomotiveContext context, bool hasDescription = false, bool hasVin = false, bool hasColour = false)
    {
        var make = new MakeBuilder().With(m => m.Name, "Volkswagen").Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Golf").Build();
        var fuel = new FuelBuilder().Build();
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
            .Build();
        if (hasDescription) listing.Description = "This is a well-maintained Volkswagen Golf with full service history and no accidents.";
        if (hasVin) listing.Vin = "WVWZZZ1JZXW000001";
        if (hasColour) listing.Colour = "Blue";
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return (listing.Id, seller.Id);
    }

    [Fact]
    public async Task Handle_SellerRequest_ReturnsInsights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, hasDescription: true, hasVin: true, hasColour: true);

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.MarketPosition.ListingPrice.Should().BeGreaterThan(0);
        result.ListingQuality.HasDescription.Should().BeTrue();
        result.ListingQuality.HasVin.Should().BeTrue();
        result.ListingQuality.QualityScore.Should().BeGreaterThan(0).And.BeLessOrEqualTo(100);
    }

    [Fact]
    public async Task Handle_IncompleteListingWithNoDescription_SuggestsDescription()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context);

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        result.ListingQuality.HasDescription.Should().BeFalse();
        result.ListingQuality.Suggestions.Should().Contain(s => s.Contains("description"));
    }

    [Fact]
    public async Task Handle_NonSellerRequest_ThrowsException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, _) = await SeedListingAsync(context);
        var wrongUserId = Guid.NewGuid();

        var act = async () => await handler.Handle(
            new GetSellerListingInsightsQuery { ListingId = listingId, UserId = wrongUserId },
            CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 3: Run to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetSellerListingInsightsQueryHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors.

- [ ] **Step 4: Create GetSellerListingInsightsQueryHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsQueryHandler(IRepository repository)
    : IRequestHandler<GetSellerListingInsightsQuery, GetSellerListingInsightsResponse>
{
    public async Task<GetSellerListingInsightsResponse> Handle(GetSellerListingInsightsQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        if (listing.SellerId != request.UserId)
            throw new UnauthorizedAccessException("Access denied. You are not the seller of this listing.");

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;

        var marketCache = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(
                c => c.Make == make && c.Model == model && c.Year == listing.Year && c.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        var marketPosition = BuildMarketPosition(listing, marketCache);
        var listingQuality = BuildListingQuality(listing);

        return new GetSellerListingInsightsResponse
        {
            MarketPosition = marketPosition,
            ListingQuality = listingQuality,
        };
    }

    private static MarketPositionInsight BuildMarketPosition(Listing listing, VehicleMarketCache? cache)
    {
        double? priceDiff = null;
        if (cache != null && cache.MedianPrice > 0)
            priceDiff = (double)((cache.MedianPrice - listing.Price) / cache.MedianPrice) * 100.0;

        return new MarketPositionInsight
        {
            ListingPrice = listing.Price,
            MarketMedianPrice = cache?.MedianPrice,
            PriceDifferencePercent = priceDiff,
            MarketListingCount = cache?.TotalListings,
            DaysListed = (int)(DateTime.UtcNow - listing.CreatedAt).TotalDays,
            HasMarketData = cache != null,
        };
    }

    private static ListingQualityInsight BuildListingQuality(Listing listing)
    {
        var points = 0;
        var suggestions = new List<string>();
        var photoCount = listing.Images.Count;
        var hasDescription = !string.IsNullOrWhiteSpace(listing.Description) && listing.Description.Length >= 20;
        var hasPhotos = photoCount >= 3;
        var hasVin = !string.IsNullOrWhiteSpace(listing.Vin);
        var hasColour = !string.IsNullOrWhiteSpace(listing.Colour);

        if (hasDescription)
        {
            points += 20;
            if (listing.Description!.Length >= 100) points += 10;
        }
        else
        {
            suggestions.Add("Add a detailed description to attract more buyers.");
        }

        if (hasPhotos)
        {
            points += 20;
            if (photoCount >= 5) points += 10;
        }
        else
        {
            suggestions.Add("Add at least 3 photos to significantly improve visibility.");
        }

        if (hasVin) points += 15;
        else suggestions.Add("Include the VIN to build buyer confidence.");

        if (hasColour) points += 10;
        else suggestions.Add("Specify the colour to help buyers filter listings.");

        var qualityScore = (int)Math.Round(Math.Min(points, 90) / 90.0 * 100);

        return new ListingQualityInsight
        {
            QualityScore = qualityScore,
            HasDescription = hasDescription,
            HasPhotos = hasPhotos,
            PhotoCount = photoCount,
            HasVin = hasVin,
            HasColour = hasColour,
            Suggestions = suggestions,
        };
    }
}
```

Note: The handler uses `UnauthorizedAccessException` (built-in .NET exception) for the forbidden access check. Add `using Automotive.Marketplace.Application.Common.Exceptions;` for `DbEntityNotFoundException`. The test's `ThrowAsync<Exception>()` will catch both.

- [ ] **Step 5: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetSellerListingInsightsQueryHandlerTests" -q
```
Expected: All 3 tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetSellerListingInsightsQueryHandlerTests.cs
git commit -m "feat: add GetSellerListingInsightsQueryHandler with market position and quality scoring

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Controller endpoints for AI + seller insights

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Add 3 new actions**

Add these imports at the top of `ListingController.cs` if not already present:
```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;
```

Add these 3 actions to the class (before closing `}`):

```csharp
[HttpGet]
public async Task<ActionResult<GetListingAiSummaryResponse>> GetAiSummary(
    [FromQuery] GetListingAiSummaryQuery query,
    CancellationToken cancellationToken)
{
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}

[HttpGet]
public async Task<ActionResult<GetListingComparisonAiSummaryResponse>> GetComparisonAiSummary(
    [FromQuery] GetListingComparisonAiSummaryQuery query,
    CancellationToken cancellationToken)
{
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}

[HttpGet]
[Protect(Permission.ManageListings)]
public async Task<ActionResult<GetSellerListingInsightsResponse>> GetSellerInsights(
    [FromQuery] GetSellerListingInsightsQuery query,
    CancellationToken cancellationToken)
{
    var result = await mediator.Send(query with { UserId = UserId }, cancellationToken);
    return Ok(result);
}
```

Note: `GetSellerListingInsightsQuery` is a `class`, not a `record`, so `query with { UserId = UserId }` won't work. Set the property directly instead:
```csharp
query.UserId = UserId;
var result = await mediator.Send(query, cancellationToken);
```

- [ ] **Step 2: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat: expose AI summary and seller insights endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Frontend API layer for AI + seller insights

**Files:**
- Create: `automotive.marketplace.client/src/features/listingDetails/types/GetListingAiSummaryResponse.ts`
- Create: `automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`
- Create: `automotive.marketplace.client/src/features/listingDetails/types/GetSellerListingInsightsResponse.ts`
- Create: `automotive.marketplace.client/src/features/listingDetails/api/getSellerListingInsightsOptions.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`

- [ ] **Step 1: Create types**

```typescript
// GetListingAiSummaryResponse.ts
export type GetListingAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
};
```

```typescript
// GetListingComparisonAiSummaryResponse.ts
export type GetListingComparisonAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
};
```

```typescript
// GetSellerListingInsightsResponse.ts
export type MarketPositionInsight = {
  listingPrice: number;
  marketMedianPrice: number | null;
  priceDifferencePercent: number | null;
  marketListingCount: number | null;
  daysListed: number;
  hasMarketData: boolean;
};

export type ListingQualityInsight = {
  qualityScore: number;
  hasDescription: boolean;
  hasPhotos: boolean;
  photoCount: number;
  hasVin: boolean;
  hasColour: boolean;
  suggestions: string[];
};

export type GetSellerListingInsightsResponse = {
  marketPosition: MarketPositionInsight;
  listingQuality: ListingQualityInsight;
};
```

- [ ] **Step 2: Add endpoint constants**

In `automotive.marketplace.client/src/constants/endpoints.ts`, inside the `LISTING` object, add:
```typescript
GET_AI_SUMMARY: "/Listing/GetAiSummary",
GET_COMPARISON_AI_SUMMARY: "/Listing/GetComparisonAiSummary",
GET_SELLER_INSIGHTS: "/Listing/GetSellerInsights",
```

- [ ] **Step 3: Add query keys**

In `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`, add:
```typescript
aiSummary: (id: string) => [...listingKeys.all(), id, "ai-summary"],
comparisonAiSummary: (a: string, b: string) => [...listingKeys.all(), "comparison-ai-summary", a, b],
sellerInsights: (id: string) => [...listingKeys.all(), id, "seller-insights"],
```

- [ ] **Step 4: Create query options**

```typescript
// getListingAiSummaryOptions.ts
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingAiSummaryResponse } from "../types/GetListingAiSummaryResponse";

export const getListingAiSummaryOptions = (listingId: string) =>
  queryOptions({
    queryKey: listingKeys.aiSummary(listingId),
    queryFn: () =>
      axiosClient.get<GetListingAiSummaryResponse>(ENDPOINTS.LISTING.GET_AI_SUMMARY, {
        params: { listingId },
      }),
    enabled: false, // on-demand only; trigger via refetch()
  });
```

```typescript
// getListingComparisonAiSummaryOptions.ts
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonAiSummaryResponse } from "../types/GetListingComparisonAiSummaryResponse";

export const getListingComparisonAiSummaryOptions = (listingAId: string, listingBId: string) =>
  queryOptions({
    queryKey: listingKeys.comparisonAiSummary(listingAId, listingBId),
    queryFn: () =>
      axiosClient.get<GetListingComparisonAiSummaryResponse>(ENDPOINTS.LISTING.GET_COMPARISON_AI_SUMMARY, {
        params: { listingAId, listingBId },
      }),
    enabled: false, // on-demand only
  });
```

```typescript
// getSellerListingInsightsOptions.ts
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetSellerListingInsightsResponse } from "../types/GetSellerListingInsightsResponse";

export const getSellerListingInsightsOptions = (listingId: string) =>
  queryOptions({
    queryKey: listingKeys.sellerInsights(listingId),
    queryFn: () =>
      axiosClient.get<GetSellerListingInsightsResponse>(ENDPOINTS.LISTING.GET_SELLER_INSIGHTS, {
        params: { listingId },
      }),
  });
```

- [ ] **Step 5: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/types/GetListingAiSummaryResponse.ts \
        automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts \
        automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts \
        automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts \
        automotive.marketplace.client/src/features/listingDetails/types/GetSellerListingInsightsResponse.ts \
        automotive.marketplace.client/src/features/listingDetails/api/getSellerListingInsightsOptions.ts \
        automotive.marketplace.client/src/constants/endpoints.ts \
        automotive.marketplace.client/src/api/queryKeys/listingKeys.ts
git commit -m "feat: add frontend API layer for AI summaries and seller insights

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: AiSummarySection component

**Files:**
- Create: `automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx`

A panel with "Get AI Summary" button (Sparkles icon). On click, calls `refetch()` from the disabled query. Shows spinner while loading. Displays the summary text when available. Shows an error note if generation failed.

- [ ] **Step 1: Create AiSummarySection**

```tsx
import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getListingAiSummaryOptions } from "../api/getListingAiSummaryOptions";

type Props = {
  listingId: string;
};

export function AiSummarySection({ listingId }: Props) {
  const {
    data,
    isFetching,
    refetch,
  } = useQuery(getListingAiSummaryOptions(listingId));

  const summary = data?.data;
  const hasResult = summary?.isGenerated;

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">AI Buyer Verdict</span>
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
          {hasResult ? "Regenerate" : "Generate"}
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
        <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
      )}

      {!isFetching && data && !hasResult && (
        <p className="text-muted-foreground mt-3 text-sm italic">
          AI summary unavailable at this time.
        </p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          Click "Generate" to get an AI-powered buyer verdict for this listing.
        </p>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Add AiSummarySection to ListingDetailsContent**

In `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`, add:

Import:
```tsx
import { AiSummarySection } from "./AiSummarySection";
```

Add below `ScoreCard` (from Plan 1):
```tsx
<AiSummarySection listingId={id} />
```

- [ ] **Step 3: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx \
        automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: add AiSummarySection to listing details page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: CompareAiSummary component + Compare page integration

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Compare.tsx`

- [ ] **Step 1: Create CompareAiSummary**

```tsx
import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getListingComparisonAiSummaryOptions } from "../api/getListingComparisonAiSummaryOptions";

type Props = {
  listingAId: string;
  listingBId: string;
};

export function CompareAiSummary({ listingAId, listingBId }: Props) {
  const { data, isFetching, refetch } = useQuery(
    getListingComparisonAiSummaryOptions(listingAId, listingBId),
  );

  const summary = data?.data;
  const hasResult = summary?.isGenerated;

  return (
    <div className="border-border mb-4 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">AI Comparison Summary</span>
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
          {hasResult ? "Regenerate" : "Compare with AI"}
        </Button>
      </div>

      {isFetching && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
        </div>
      )}

      {!isFetching && hasResult && summary?.summary && (
        <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          Click "Compare with AI" to get a recommendation between these two listings.
        </p>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Export from compareListings index**

In `automotive.marketplace.client/src/features/compareListings/index.ts`, add:
```typescript
export { CompareAiSummary } from "./components/CompareAiSummary";
```

- [ ] **Step 3: Add to Compare page**

In `automotive.marketplace.client/src/app/pages/Compare.tsx`:

Add import:
```typescript
import { CompareAiSummary } from "@/features/compareListings";
```

Add `CompareAiSummary` below the `CompareScoreBanner` (from Plan 1) and above `CompareTable`:
```tsx
<CompareAiSummary listingAId={listingA.id} listingBId={listingB.id} />
```

- [ ] **Step 4: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx \
        automotive.marketplace.client/src/features/compareListings/index.ts \
        automotive.marketplace.client/src/app/pages/Compare.tsx
git commit -m "feat: add AI comparison summary to compare page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: SellerInsightsPanel component + MyListings integration

**Files:**
- Create: `automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx`
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`

The seller insights panel is a collapsible section showing two side-by-side cards:
1. **Market Position**: price comparison bar/indicator + days listed + market listing count
2. **Listing Quality**: circular quality score + improvement suggestions

- [ ] **Step 1: Create SellerInsightsPanel**

```tsx
import { useQuery } from "@tanstack/react-query";
import {
  BarChart2,
  CheckCircle,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  Camera,
  FileText,
  Tag,
  Palette,
} from "lucide-react";
import { useState } from "react";
import { getSellerListingInsightsOptions } from "../api/getSellerListingInsightsOptions";

type Props = {
  listingId: string;
};

export function SellerInsightsPanel({ listingId }: Props) {
  const [expanded, setExpanded] = useState(false);
  const { data, isLoading } = useQuery(getSellerListingInsightsOptions(listingId));

  if (isLoading) {
    return (
      <div className="mt-2 space-y-2 rounded-md border p-3">
        <div className="bg-muted h-4 w-32 animate-pulse rounded" />
        <div className="bg-muted h-3 w-full animate-pulse rounded" />
      </div>
    );
  }

  if (!data) return null;
  const { marketPosition, listingQuality } = data.data;

  const priceColor =
    marketPosition.priceDifferencePercent == null
      ? "text-muted-foreground"
      : marketPosition.priceDifferencePercent >= 0
        ? "text-green-600"
        : "text-orange-500";

  const qualityColor =
    listingQuality.qualityScore >= 70
      ? "text-green-600"
      : listingQuality.qualityScore >= 50
        ? "text-blue-600"
        : "text-orange-500";

  return (
    <div className="border-border mt-2 rounded-md border">
      <button
        onClick={() => setExpanded(!expanded)}
        className="flex w-full items-center justify-between p-3 text-left"
      >
        <span className="flex items-center gap-2 text-sm font-medium">
          <BarChart2 className="h-4 w-4" />
          Seller Insights
        </span>
        {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
      </button>

      {expanded && (
        <div className="grid grid-cols-2 gap-3 border-t p-3">
          {/* Market Position Card */}
          <div className="bg-muted/30 space-y-2 rounded-md p-3">
            <p className="text-xs font-semibold uppercase tracking-wide">Market Position</p>
            <div>
              <p className="text-muted-foreground text-xs">Your price</p>
              <p className="font-semibold">€{marketPosition.listingPrice.toLocaleString()}</p>
            </div>
            {marketPosition.hasMarketData ? (
              <>
                <div>
                  <p className="text-muted-foreground text-xs">Market median</p>
                  <p className="text-sm">€{marketPosition.marketMedianPrice?.toLocaleString()}</p>
                </div>
                <p className={`text-sm font-medium ${priceColor}`}>
                  {marketPosition.priceDifferencePercent != null &&
                    (marketPosition.priceDifferencePercent >= 0
                      ? `${marketPosition.priceDifferencePercent.toFixed(1)}% below market`
                      : `${Math.abs(marketPosition.priceDifferencePercent).toFixed(1)}% above market`)}
                </p>
                <p className="text-muted-foreground text-xs">
                  {marketPosition.marketListingCount} similar listings • {marketPosition.daysListed} days listed
                </p>
              </>
            ) : (
              <p className="text-muted-foreground text-xs">No market data available yet</p>
            )}
          </div>

          {/* Listing Quality Card */}
          <div className="bg-muted/30 space-y-2 rounded-md p-3">
            <p className="text-xs font-semibold uppercase tracking-wide">Listing Quality</p>
            <div className={`text-2xl font-bold ${qualityColor}`}>
              {listingQuality.qualityScore}
              <span className="text-muted-foreground text-sm font-normal">/100</span>
            </div>
            <div className="space-y-1">
              {[
                { check: listingQuality.hasDescription, icon: FileText, label: "Description" },
                { check: listingQuality.hasPhotos, icon: Camera, label: `Photos (${listingQuality.photoCount})` },
                { check: listingQuality.hasVin, icon: Tag, label: "VIN" },
                { check: listingQuality.hasColour, icon: Palette, label: "Colour" },
              ].map(({ check, icon: Icon, label }) => (
                <div key={label} className="flex items-center gap-1.5 text-xs">
                  {check ? (
                    <CheckCircle className="h-3.5 w-3.5 text-green-500" />
                  ) : (
                    <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
                  )}
                  <span className={check ? "" : "text-muted-foreground"}>{label}</span>
                </div>
              ))}
            </div>
            {listingQuality.suggestions.length > 0 && (
              <div className="space-y-1 border-t pt-2">
                {listingQuality.suggestions.map((s, i) => (
                  <p key={i} className="text-muted-foreground text-xs">
                    • {s}
                  </p>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
```

Note: This component imports `getSellerListingInsightsOptions` from `"../api/getSellerListingInsightsOptions"`. Since `SellerInsightsPanel` lives in `myListings/components/`, it needs to import from `listingDetails`. Update the import path:
```tsx
import { getSellerListingInsightsOptions } from "@/features/listingDetails/api/getSellerListingInsightsOptions";
```

- [ ] **Step 2: Add SellerInsightsPanel to MyListingCard**

First, view `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx` to understand its props and structure.

Add import at the top of `MyListingCard.tsx`:
```tsx
import { SellerInsightsPanel } from "./SellerInsightsPanel";
```

Find where the listing card renders and add `SellerInsightsPanel` at the bottom of the card:
```tsx
<SellerInsightsPanel listingId={listing.id} />
```

Replace `listing.id` with whatever the actual listing ID field is called in the `MyListingCard` props (check the type — it may be `listing.id` or accessed differently).

- [ ] **Step 3: Build and lint**

```bash
cd automotive.marketplace.client && npm run build && npm run lint 2>&1 | tail -15
```
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx \
        automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx
git commit -m "feat: add SellerInsightsPanel to My Listings page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 12: Run full test suite and verify

- [ ] **Step 1: Run all backend tests**

```bash
dotnet test ./Automotive.Marketplace.sln -q 2>&1 | tail -20
```
Expected: All tests pass.

- [ ] **Step 2: Build and lint frontend**

```bash
cd automotive.marketplace.client && npm run build && npm run lint 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 3: Final commit**

```bash
git add -A
git commit -m "chore: Plan 3 complete — AI summaries and seller insights panel

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
