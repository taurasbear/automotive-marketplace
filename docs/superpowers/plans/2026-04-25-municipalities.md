# Municipalities Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the hardcoded `Listing.City` string with a proper `Municipality` entity sourced from Lithuania's open government data API, add auto-sync (startup + monthly), and update the frontend location combobox to use the backend.

**Architecture:** A new `Municipality` entity (no BaseEntity inheritance) stores the 60 Lithuanian municipalities synced from `https://get.data.gov.lt/datasets/gov/rc/ar/grasavivaldybe/GraSavivaldybe/:format/json`. A `MunicipalityInitializer` runs eagerly at startup (syncs if table is empty or data is >30 days old); a `MunicipalitySyncService` BackgroundService repeats the check every 24 hours. `Listing.City` (string) becomes `Listing.MunicipalityId` (FK). All existing response `City` fields are renamed `MunicipalityName`; filter/command `City` fields become `MunicipalityId`. The frontend `LocationCombobox` calls `GET /api/municipalities` and uses municipality UUIDs as values.

**Tech Stack:** .NET 8 / ASP.NET Core, EF Core (Npgsql), MediatR, AutoMapper, xUnit + TestContainers + Respawn + NSubstitute + FluentAssertions, React 19 + TypeScript, TanStack Query, Zod, i18next.

---

## File Map

### New files
| File | Purpose |
|---|---|
| `Automotive.Marketplace.Domain/Entities/Municipality.cs` | Municipality entity |
| `Automotive.Marketplace.Application/Interfaces/Services/IMunicipalityApiClient.cs` | API client interface + MunicipalityDto |
| `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQuery.cs` | CQRS query |
| `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQueryHandler.cs` | CQRS handler |
| `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesResponse.cs` | Response DTO |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/MunicipalityConfiguration.cs` | EF type config |
| `Automotive.Marketplace.Infrastructure/Interfaces/IMunicipalityInitializer.cs` | Initializer interface |
| `Automotive.Marketplace.Infrastructure/Sync/MunicipalityInitializer.cs` | Startup sync logic |
| `Automotive.Marketplace.Infrastructure/Services/LithuanianMunicipalityApiClient.cs` | HTTP client impl |
| `Automotive.Marketplace.Server/Controllers/MunicipalityController.cs` | API controller |
| `Automotive.Marketplace.Server/Services/MunicipalitySyncService.cs` | Background sync service |
| `Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/GetAllMunicipalitiesQueryHandlerTests.cs` | Handler tests |
| `Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/MunicipalityInitializerTests.cs` | Initializer tests |
| `automotive.marketplace.client/src/features/listingList/api/getMunicipalitiesOptions.ts` | TanStack Query options |

### Modified files (backend)
- `Domain/Entities/Listing.cs` — remove `City`, add `MunicipalityId` FK + nav prop
- `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQuery.cs` — `City?` → `MunicipalityId?`
- `Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs` — `City` → `MunicipalityName`
- `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs` — filter + Include Municipality
- `Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs` — `City` → `MunicipalityName`, add `MunicipalityId`
- `Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs` — Include Municipality
- `Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs` — `City` → `MunicipalityName`
- `Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs` — Include Municipality
- `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsResponse.cs` — `City` → `MunicipalityName`
- `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs` — Include Municipality + assignment
- `Application/Features/ListingFeatures/SearchListings/SearchListingsResponse.cs` — `City` → `MunicipalityName`
- `Application/Features/ListingFeatures/SearchListings/SearchListingsQueryHandler.cs` — Include Municipality + assignment
- `Application/Features/ListingFeatures/CreateListing/CreateListingCommand.cs` — `string City` → `Guid MunicipalityId`
- `Application/Features/ListingFeatures/CreateListing/CreateListingCommandHandler.cs` — assign `MunicipalityId`
- `Application/Features/ListingFeatures/UpdateListing/UpdateListingCommand.cs` — `string City` → `Guid MunicipalityId`
- `Application/Mappings/ListingMappings.cs` — update all City → MunicipalityName mappings + UpdateCommand → Listing
- `Infrastructure/Data/Configuration/ListingConfiguration.cs` — add Municipality FK
- `Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — add `DbSet<Municipality>`
- `Infrastructure/ServiceExtensions.cs` — register HttpClient + Initializer
- `Infrastructure/Data/Builders/ListingBuilder.cs` — remove City, add `WithMunicipality(Guid)`
- `Infrastructure/Data/Seeders/ListingSeeder.cs` — load municipalities, use `WithMunicipality`
- `Server/Program.cs` — call `MunicipalityInitializer` + register `MunicipalitySyncService`
- All listing test files that call `ListingBuilder` — seed Municipality, call `.WithMunicipality(id)`

### Modified files (frontend)
- `src/constants/endpoints.ts` — add `MUNICIPALITY.GET_ALL`
- `src/features/listingList/types/GetAllListingsQuery.ts` — `city?` → `municipalityId?`
- `src/features/listingList/types/GetAllListingsResponse.ts` — `city` → `municipalityName`
- `src/features/listingList/types/basicFilter.ts` — `city` → `municipalityId`
- `src/features/listingDetails/types/GetListingByIdResponse.ts` — `city` → `municipalityName`, add `municipalityId`
- `src/features/listingDetails/types/UpdateListingCommand.ts` — `city` → `municipalityId`
- `src/features/createListing/types/CreateListingCommand.ts` — `city` → `municipalityId`
- `src/features/savedListings/types/SavedListing.ts` — `city` → `municipalityName`
- `src/features/compareListings/types/SearchListingsResponse.ts` — `city` → `municipalityName`
- `src/features/search/types/listingSearchStateValues.ts` — `city` → `municipalityId`
- `src/features/search/schemas/listingSearchSchema.ts` — `city` → `municipalityId` (GUID validation)
- `src/features/search/utils/listingSearchUtils.ts` — `city` → `municipalityId` throughout
- `src/features/createListing/schemas/createListingSchema.ts` — `city` field → `municipalityId` (GUID)
- `src/features/listingDetails/schemas/updateListingSchema.ts` — update city → municipalityId
- `src/components/forms/select/LocationCombobox.tsx` — use API instead of hardcoded values
- `src/features/listingList/components/BasicFilters.tsx` — `city` → `municipalityId`
- `src/features/listingList/components/Filters.tsx` — `city` → `municipalityId`
- `src/features/search/components/ListingSearchFilters.tsx` — `city` → `municipalityId`
- `src/features/search/components/ListingSearch.tsx` — initial state key rename
- `src/features/createListing/components/CreateListingForm.tsx` — Input → LocationCombobox for municipality
- `src/features/listingDetails/components/EditListingForm.tsx` — Input → LocationCombobox for municipality
- `src/features/myListings/components/MyListingDetail.tsx` — EditableField for city → LocationCombobox
- `src/features/listingList/components/ListingCard.tsx` — `city` → `municipalityName`
- `src/features/myListings/components/MyListingCard.tsx` — `city` → `municipalityName`
- `src/features/listingDetails/components/ListingDetailsContent.tsx` — `city` → `municipalityName`
- `src/features/savedListings/components/SavedListingRow.tsx` — `city` → `municipalityName`
- `src/features/savedListings/components/PropertyMentionPicker.tsx` — key `"city"` → `"municipalityName"`
- `src/features/compareListings/components/CompareTable.tsx` — field `"city"` → `"municipalityName"`
- `src/features/compareListings/components/CompareHeader.tsx` — `listing.city` → `listing.municipalityName`
- `src/features/compareListings/components/CompareSearchModal.tsx` — `listing.city` → `listing.municipalityName`

---

## Task 1: Municipality Domain Entity

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/Municipality.cs`

- [ ] **Step 1: Create the entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Municipality
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}
```

- [ ] **Step 2: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/Municipality.cs
git commit -m "feat: add Municipality domain entity

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: Application Layer — IMunicipalityApiClient + GetAllMunicipalities

**Files:**
- Create: `Automotive.Marketplace.Application/Interfaces/Services/IMunicipalityApiClient.cs`
- Create: `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQueryHandler.cs`

- [ ] **Step 1: Create IMunicipalityApiClient with MunicipalityDto**

```csharp
// Automotive.Marketplace.Application/Interfaces/Services/IMunicipalityApiClient.cs
namespace Automotive.Marketplace.Application.Interfaces.Services;

public record MunicipalityDto(Guid Id, string Name);

public interface IMunicipalityApiClient
{
    Task<IEnumerable<MunicipalityDto>> FetchMunicipalitiesAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create GetAllMunicipalitiesResponse**

```csharp
// Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesResponse.cs
namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public sealed record GetAllMunicipalitiesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Create GetAllMunicipalitiesQuery**

```csharp
// Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public sealed record GetAllMunicipalitiesQuery : IRequest<IEnumerable<GetAllMunicipalitiesResponse>>;
```

- [ ] **Step 4: Create GetAllMunicipalitiesQueryHandler**

```csharp
// Automotive.Marketplace.Application/Features/MunicipalityFeatures/GetAllMunicipalities/GetAllMunicipalitiesQueryHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public class GetAllMunicipalitiesQueryHandler(IRepository repository)
    : IRequestHandler<GetAllMunicipalitiesQuery, IEnumerable<GetAllMunicipalitiesResponse>>
{
    public async Task<IEnumerable<GetAllMunicipalitiesResponse>> Handle(
        GetAllMunicipalitiesQuery request, CancellationToken cancellationToken)
    {
        return await repository
            .AsQueryable<Municipality>()
            .OrderBy(m => m.Name)
            .Select(m => new GetAllMunicipalitiesResponse { Id = m.Id, Name = m.Name })
            .ToListAsync(cancellationToken);
    }
}
```

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/
git commit -m "feat: add GetAllMunicipalities CQRS feature and IMunicipalityApiClient interface

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: EF Configuration + DbContext

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/MunicipalityConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create MunicipalityConfiguration**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/MunicipalityConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
    }
}
```

- [ ] **Step 2: Add FK relationship to ListingConfiguration**

In `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingConfiguration.cs`, add inside the `Configure` method:

```csharp
builder.HasOne(listing => listing.Municipality)
    .WithMany()
    .HasForeignKey(listing => listing.MunicipalityId)
    .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **Step 3: Add DbSet<Municipality> to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add after the existing `DbSet` properties:

```csharp
public DbSet<Municipality> Municipalities { get; set; }
```

Also add the using at the top if not already present:
```csharp
using Automotive.Marketplace.Domain.Entities;
```

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/
git commit -m "feat: add Municipality EF configuration and DbSet

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: Update Listing Entity + All Consumers (Backend Breaking Change)

This is the largest backend task. All changes must be made together to maintain compilation.

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/Listing.cs`
- Modify: `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQuery.cs`
- Modify: `Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- Modify: `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`
- Modify: `Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`
- Modify: `Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs`
- Modify: `Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs`
- Modify: `Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs`
- Modify: `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsResponse.cs`
- Modify: `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs`
- Modify: `Application/Features/ListingFeatures/SearchListings/SearchListingsResponse.cs`
- Modify: `Application/Features/ListingFeatures/SearchListings/SearchListingsQueryHandler.cs`
- Modify: `Application/Features/ListingFeatures/CreateListing/CreateListingCommand.cs`
- Modify: `Application/Features/ListingFeatures/CreateListing/CreateListingCommandHandler.cs`
- Modify: `Application/Features/ListingFeatures/UpdateListing/UpdateListingCommand.cs`
- Modify: `Application/Mappings/ListingMappings.cs`

- [ ] **Step 1: Update Listing.cs — remove City, add MunicipalityId + navigation**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public Guid MunicipalityId { get; set; }

public virtual Municipality Municipality { get; set; } = null!;
```

- [ ] **Step 2: Update GetAllListingsQuery.cs — rename City to MunicipalityId**

Replace:
```csharp
public string? City { get; set; }
```
With:
```csharp
public Guid? MunicipalityId { get; set; }
```

- [ ] **Step 3: Update GetAllListingsResponse.cs — rename City to MunicipalityName**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public string MunicipalityName { get; set; } = string.Empty;
```

- [ ] **Step 4: Update GetAllListingsQueryHandler.cs — fix filter and add Include**

Replace the City filter line:
```csharp
.Where(listing => request.City == null || request.City.ToLower() == listing.City.ToLower())
```
With:
```csharp
.Where(listing => request.MunicipalityId == null || request.MunicipalityId == listing.MunicipalityId)
```

Add `.Include(l => l.Municipality)` to the query chain (after the last existing `.Include` and before the first `.Where`):
```csharp
.Include(l => l.Municipality)
```

- [ ] **Step 5: Update GetListingByIdResponse.cs — rename City, add MunicipalityId**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public Guid MunicipalityId { get; set; }
public string MunicipalityName { get; set; } = string.Empty;
```

- [ ] **Step 6: Update GetListingByIdQueryHandler.cs — add Include Municipality**

In the query chain (after the existing `.Include` statements and before `.FirstOrDefaultAsync`), add:
```csharp
.Include(l => l.Municipality)
```

- [ ] **Step 7: Update GetMyListingsResponse.cs — rename City to MunicipalityName**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public string MunicipalityName { get; set; } = string.Empty;
```

- [ ] **Step 8: Update GetMyListingsQueryHandler.cs — add Include Municipality**

In the query chain (after the existing includes, before `.Where(l => l.SellerId == request.SellerId)`), add:
```csharp
.Include(l => l.Municipality)
```

- [ ] **Step 9: Update GetSavedListingsResponse.cs — rename City to MunicipalityName**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public string MunicipalityName { get; set; } = string.Empty;
```

- [ ] **Step 10: Update GetSavedListingsQueryHandler.cs — Include Municipality + fix assignment**

In the query chain (after `.Include(like => like.Listing).ThenInclude(listing => listing.Images)`), add:
```csharp
.Include(like => like.Listing)
    .ThenInclude(listing => listing.Municipality)
```

In the `result.Add(new GetSavedListingsResponse { ... })` block, replace:
```csharp
City = listing.City,
```
With:
```csharp
MunicipalityName = listing.Municipality?.Name ?? string.Empty,
```

- [ ] **Step 11: Update SearchListingsResponse.cs — rename City to MunicipalityName**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public string MunicipalityName { get; set; } = string.Empty;
```

- [ ] **Step 12: Update SearchListingsQueryHandler.cs — Include Municipality + fix projection**

In the query chain (after the existing `.Include` statements), add:
```csharp
.Include(l => l.Municipality)
```

In the `response = response with { ... }` or the `new SearchListingsResponse { ... }` block, replace:
```csharp
City = listing.City,
```
With:
```csharp
MunicipalityName = listing.Municipality?.Name ?? string.Empty,
```

- [ ] **Step 13: Update CreateListingCommand.cs — rename City to MunicipalityId**

Replace:
```csharp
string City,
```
With:
```csharp
Guid MunicipalityId,
```

- [ ] **Step 14: Update CreateListingCommandHandler.cs — use MunicipalityId**

Replace:
```csharp
City = request.City,
```
With:
```csharp
MunicipalityId = request.MunicipalityId,
```

- [ ] **Step 15: Update UpdateListingCommand.cs — rename City to MunicipalityId**

Replace:
```csharp
public string City { get; set; } = string.Empty;
```
With:
```csharp
public Guid MunicipalityId { get; set; }
```

- [ ] **Step 16: Update ListingMappings.cs — fix all City references**

**a.** In `CreateMap<Listing, GetAllListingsResponse>()`, add an explicit member mapping (before `.ForMember(dest => dest.Thumbnail, ...)`):
```csharp
.ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : string.Empty))
```

**b.** In `CreateMap<UpdateListingCommand, Listing>()`, replace:
```csharp
.ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
```
With:
```csharp
.ForMember(dest => dest.MunicipalityId, opt => opt.MapFrom(src => src.MunicipalityId))
```

**c.** In `CreateMap<Listing, GetListingByIdResponse>()`, add explicit mappings (before `.ForMember(dest => dest.Images, ...)`):
```csharp
.ForMember(dest => dest.MunicipalityId, opt => opt.MapFrom(src => src.MunicipalityId))
.ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : string.Empty))
```

**d.** In `CreateMap<Listing, GetMyListingsResponse>()`, add explicit mapping (before `.ForMember(dest => dest.Status, ...)`):
```csharp
.ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : string.Empty))
```

- [ ] **Step 17: Build to check for compilation errors**

Run: `dotnet build ./Automotive.Marketplace.sln`

Expected: build succeeds (all City references are resolved). Fix any remaining compile errors before proceeding.

- [ ] **Step 18: Commit**

```bash
git add .
git commit -m "feat: replace Listing.City string with MunicipalityId FK across application layer

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: Infrastructure — LithuanianMunicipalityApiClient

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Services/LithuanianMunicipalityApiClient.cs`

- [ ] **Step 1: Create the HTTP client implementation**

```csharp
// Automotive.Marketplace.Infrastructure/Services/LithuanianMunicipalityApiClient.cs
using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class LithuanianMunicipalityApiClient(HttpClient httpClient) : IMunicipalityApiClient
{
    private const string ApiUrl =
        "https://get.data.gov.lt/datasets/gov/rc/ar/grasavivaldybe/GraSavivaldybe/:format/json";

    public async Task<IEnumerable<MunicipalityDto>> FetchMunicipalitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ApiResponse>(ApiUrl, cancellationToken);
        return response?.Data?.Select(item => new MunicipalityDto(item.Id, item.Pavadinimas))
               ?? [];
    }

    private class ApiResponse
    {
        [JsonPropertyName("_data")]
        public List<ApiItem> Data { get; set; } = [];
    }

    private class ApiItem
    {
        [JsonPropertyName("_id")]
        public Guid Id { get; set; }

        [JsonPropertyName("pavadinimas")]
        public string Pavadinimas { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Services/LithuanianMunicipalityApiClient.cs
git commit -m "feat: add LithuanianMunicipalityApiClient HTTP implementation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: Infrastructure — MunicipalityInitializer

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Interfaces/IMunicipalityInitializer.cs`
- Create: `Automotive.Marketplace.Infrastructure/Sync/MunicipalityInitializer.cs`

- [ ] **Step 1: Create IMunicipalityInitializer interface**

```csharp
// Automotive.Marketplace.Infrastructure/Interfaces/IMunicipalityInitializer.cs
namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface IMunicipalityInitializer
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create MunicipalityInitializer implementation**

```csharp
// Automotive.Marketplace.Infrastructure/Sync/MunicipalityInitializer.cs
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Infrastructure.Sync;

public class MunicipalityInitializer(
    AutomotiveContext context,
    IMunicipalityApiClient apiClient,
    ILogger<MunicipalityInitializer> logger) : IMunicipalityInitializer
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(30);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasAny = await context.Set<Municipality>().AnyAsync(cancellationToken);
            var isStale = !hasAny
                || await context.Set<Municipality>().MinAsync(m => m.SyncedAt, cancellationToken)
                   < DateTime.UtcNow - SyncInterval;

            if (!isStale)
            {
                logger.LogInformation("Municipality data is fresh, skipping sync.");
                return;
            }

            logger.LogInformation("Syncing municipality data from government API...");
            var municipalities = (await apiClient.FetchMunicipalitiesAsync(cancellationToken)).ToList();
            var syncedAt = DateTime.UtcNow;

            foreach (var dto in municipalities)
            {
                var existing = await context.Set<Municipality>()
                    .FindAsync([dto.Id], cancellationToken);

                if (existing is null)
                    await context.Set<Municipality>().AddAsync(
                        new Municipality { Id = dto.Id, Name = dto.Name, SyncedAt = syncedAt },
                        cancellationToken);
                else
                {
                    existing.Name = dto.Name;
                    existing.SyncedAt = syncedAt;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Municipality sync complete. {Count} records upserted.", municipalities.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Municipality sync failed. App will start with existing data.");
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/
git commit -m "feat: add MunicipalityInitializer for startup sync

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 7: Update ServiceExtensions + Program.cs + Server Components

**Files:**
- Modify: `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`
- Create: `Automotive.Marketplace.Server/Controllers/MunicipalityController.cs`
- Create: `Automotive.Marketplace.Server/Services/MunicipalitySyncService.cs`
- Modify: `Automotive.Marketplace.Server/Program.cs`

- [ ] **Step 1: Register client and initializer in ServiceExtensions.cs**

Add to `ConfigureInfrastructure` method (after the existing `AddScoped` calls):
```csharp
services.AddHttpClient<IMunicipalityApiClient, LithuanianMunicipalityApiClient>();
services.AddScoped<IMunicipalityInitializer, MunicipalityInitializer>();
```

Also add the required using statements at the top of `ServiceExtensions.cs`:
```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Automotive.Marketplace.Infrastructure.Services;
using Automotive.Marketplace.Infrastructure.Sync;
```

- [ ] **Step 2: Create MunicipalityController**

```csharp
// Automotive.Marketplace.Server/Controllers/MunicipalityController.cs
using Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class MunicipalityController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMunicipalitiesResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllMunicipalitiesQuery(), cancellationToken);
        return Ok(result);
    }
}
```

- [ ] **Step 3: Create MunicipalitySyncService**

```csharp
// Automotive.Marketplace.Server/Services/MunicipalitySyncService.cs
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Server.Services;

public class MunicipalitySyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<MunicipalitySyncService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var initializer = scope.ServiceProvider.GetRequiredService<IMunicipalityInitializer>();
                await initializer.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Municipality sync check failed.");
            }
        }
    }
}
```

- [ ] **Step 4: Update Program.cs — run initializer at startup + register BackgroundService**

Add the `AddHostedService` call alongside the other hosted services (after `MeetingExpiryService`):
```csharp
builder.Services.AddHostedService<Automotive.Marketplace.Server.Services.MunicipalitySyncService>();
```

In the startup scope block (after `await automotiveContext.Database.MigrateAsync()` and **before** the `if (app.Environment.IsDevelopment())` seeder block), add:
```csharp
var municipalityInitializer = scope.ServiceProvider.GetRequiredService<IMunicipalityInitializer>();
await municipalityInitializer.RunAsync();
```

Add the using at the top of `Program.cs`:
```csharp
using Automotive.Marketplace.Infrastructure.Interfaces;
```

- [ ] **Step 5: Build to verify no compilation errors**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat: add MunicipalityController, MunicipalitySyncService, and startup integration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 8: EF Core Migration + Update ListingBuilder and ListingSeeder

**Files:**
- New migration file (generated by EF)
- Modify: `Automotive.Marketplace.Infrastructure/Data/Builders/ListingBuilder.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Seeders/ListingSeeder.cs`

- [ ] **Step 1: Generate the migration**

```bash
cd /path/to/automotive-marketplace
dotnet ef migrations add AddMunicipalityAndUpdateListing \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected: New migration file created under `Automotive.Marketplace.Infrastructure/Migrations/`.

- [ ] **Step 2: Inspect the migration to confirm it drops `City`, adds `Municipalities` table, adds `MunicipalityId` FK on `Listings`**

Open the generated migration file and verify these operations are present. If anything is wrong (e.g. City is not dropped), fix the entity and re-generate.

- [ ] **Step 3: Update ListingBuilder — remove City rule, add WithMunicipality**

In `Automotive.Marketplace.Infrastructure/Data/Builders/ListingBuilder.cs`:

Remove:
```csharp
.RuleFor(listing => listing.City, f => f.Address.City())
```

Add a new builder method:
```csharp
public ListingBuilder WithMunicipality(Guid municipalityId)
{
    _faker.RuleFor(listing => listing.MunicipalityId, municipalityId);
    return this;
}
```

- [ ] **Step 4: Update ListingSeeder — load municipalities and use WithMunicipality**

In `Automotive.Marketplace.Infrastructure/Data/Seeders/ListingSeeder.cs`, update `SeedAsync`:

```csharp
public async Task SeedAsync(CancellationToken cancellationToken)
{
    if (await context.Set<Listing>().AnyAsync(cancellationToken))
        return;

    var users = await context.Set<User>().ToListAsync(cancellationToken);
    var variants = await context.Set<Variant>().ToListAsync(cancellationToken);
    var drivetrains = await context.Set<Drivetrain>().ToListAsync(cancellationToken);
    var municipalities = await context.Set<Municipality>().ToListAsync(cancellationToken);

    if (!users.Any() || !variants.Any() || !drivetrains.Any() || !municipalities.Any())
        return;

    for (int i = 0; i < variants.Count; i++)
    {
        var user = users.Skip(i % users.Count).First();
        var drivetrain = drivetrains[i % drivetrains.Count];
        var municipality = municipalities[i % municipalities.Count];

        var listing = new ListingBuilder()
            .WithVariant(variants[i].Id)
            .WithSeller(user.Id)
            .WithDrivetrain(drivetrain.Id)
            .WithMunicipality(municipality.Id)
            .Build();

        await context.AddAsync(listing, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);
}
```

Add the `Municipality` using at the top:
```csharp
using Automotive.Marketplace.Domain.Entities;
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat: generate EF migration for Municipality, update ListingBuilder and ListingSeeder

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 9: Write GetAllMunicipalitiesQueryHandler Tests (TDD)

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/GetAllMunicipalitiesQueryHandlerTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/GetAllMunicipalitiesQueryHandlerTests.cs
using Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MunicipalityHandlerTests;

public class GetAllMunicipalitiesQueryHandlerTests(
    DatabaseFixture<GetAllMunicipalitiesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllMunicipalitiesQueryHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private GetAllMunicipalitiesQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetAllMunicipalitiesQueryHandler(repository);
    }

    [Fact]
    public async Task Handle_NoMunicipalities_ShouldReturnEmpty()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMunicipalities_ShouldReturnAllSortedAlphabetically()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.AddRangeAsync(
            new Municipality { Id = Guid.NewGuid(), Name = "Vilniaus m.", SyncedAt = DateTime.UtcNow },
            new Municipality { Id = Guid.NewGuid(), Name = "Alytaus m.", SyncedAt = DateTime.UtcNow },
            new Municipality { Id = Guid.NewGuid(), Name = "Klaipėdos m.", SyncedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var result = (await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alytaus m.");
        result[1].Name.Should().Be("Klaipėdos m.");
        result[2].Name.Should().Be("Vilniaus m.");
    }

    [Fact]
    public async Task Handle_WithMunicipalities_ShouldReturnCorrectIdAndName()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var id = Guid.NewGuid();
        await context.AddAsync(new Municipality { Id = id, Name = "Kauno m.", SyncedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var result = (await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None)).Single();

        result.Id.Should().Be(id);
        result.Name.Should().Be("Kauno m.");
    }
}
```

- [ ] **Step 2: Run the tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~GetAllMunicipalitiesQueryHandlerTests" ./Automotive.Marketplace.sln`
Expected: All 3 tests PASS.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/
git commit -m "test: add GetAllMunicipalitiesQueryHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 10: Write MunicipalityInitializer Tests (TDD)

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/MunicipalityInitializerTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/MunicipalityHandlerTests/MunicipalityInitializerTests.cs
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Sync;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MunicipalityHandlerTests;

public class MunicipalityInitializerTests(
    DatabaseFixture<MunicipalityInitializerTests> fixture)
    : IClassFixture<DatabaseFixture<MunicipalityInitializerTests>>, IAsyncLifetime
{
    private readonly IMunicipalityApiClient _apiClient = Substitute.For<IMunicipalityApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private MunicipalityInitializer CreateInitializer(AutomotiveContext context)
        => new(context, _apiClient, NullLogger<MunicipalityInitializer>.Instance);

    [Fact]
    public async Task RunAsync_EmptyTable_ShouldFetchAndInsertAllRecords()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var dto1 = new MunicipalityDto(Guid.NewGuid(), "Vilniaus m.");
        var dto2 = new MunicipalityDto(Guid.NewGuid(), "Kauno m.");
        _apiClient.FetchMunicipalitiesAsync(Arg.Any<CancellationToken>())
            .Returns([dto1, dto2]);

        await CreateInitializer(context).RunAsync();

        var saved = context.Set<Municipality>().ToList();
        saved.Should().HaveCount(2);
        saved.Should().ContainSingle(m => m.Id == dto1.Id && m.Name == dto1.Name);
        saved.Should().ContainSingle(m => m.Id == dto2.Id && m.Name == dto2.Name);
    }

    [Fact]
    public async Task RunAsync_FreshData_ShouldSkipApiCall()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.AddAsync(new Municipality
        {
            Id = Guid.NewGuid(),
            Name = "Vilniaus m.",
            SyncedAt = DateTime.UtcNow // fresh
        });
        await context.SaveChangesAsync();

        await CreateInitializer(context).RunAsync();

        await _apiClient.DidNotReceive().FetchMunicipalitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_StaleData_ShouldUpdateExistingRecord()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var existingId = Guid.NewGuid();
        await context.AddAsync(new Municipality
        {
            Id = existingId,
            Name = "Old Name",
            SyncedAt = DateTime.UtcNow.AddDays(-31) // stale
        });
        await context.SaveChangesAsync();

        _apiClient.FetchMunicipalitiesAsync(Arg.Any<CancellationToken>())
            .Returns([new MunicipalityDto(existingId, "Updated Name")]);

        await CreateInitializer(context).RunAsync();

        var updated = context.Set<Municipality>().Single(m => m.Id == existingId);
        updated.Name.Should().Be("Updated Name");
        updated.SyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
```

- [ ] **Step 2: Run the tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~MunicipalityInitializerTests" ./Automotive.Marketplace.sln`
Expected: All 3 tests PASS.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/
git commit -m "test: add MunicipalityInitializer integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 11: Update Existing Listing Tests

All test helper methods that use `ListingBuilder` must seed a `Municipality` and call `.WithMunicipality(municipality.Id)`.

**Files to update** (search for `new ListingBuilder()` or `ListingBuilder()` in the test project):
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetAllListingsQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/CreateListingCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/SearchListingsQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetMyListingsQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs`
- Any chat handler tests that seed listings

- [ ] **Step 1: For every `SeedListingsAsync` (or equivalent) in the listing test files, add municipality seeding**

The pattern to apply in each seeding helper — add this before creating listings:
```csharp
using Automotive.Marketplace.Domain.Entities;
// ...

var municipality = new Municipality
{
    Id = Guid.NewGuid(),
    Name = "Vilniaus m.",
    SyncedAt = DateTime.UtcNow
};
await context.AddAsync(municipality);
await context.SaveChangesAsync(); // save municipality before listings
```

Then add `.WithMunicipality(municipality.Id)` to each `ListingBuilder` chain. For example:
```csharp
// Before:
var listing = new ListingBuilder()
    .WithSeller(seller.Id)
    .WithVariant(variant.Id)
    .WithDrivetrain(drivetrain.Id)
    .Build();

// After:
var listing = new ListingBuilder()
    .WithSeller(seller.Id)
    .WithVariant(variant.Id)
    .WithDrivetrain(drivetrain.Id)
    .WithMunicipality(municipality.Id)
    .Build();
```

For tests that filter by city (e.g., `GetAllListingsQueryHandlerTests` has a city filter test), update the test to filter by `MunicipalityId` instead:
```csharp
// Before: var query = new GetAllListingsQuery { City = "some-city" };
// After:
var query = new GetAllListingsQuery { MunicipalityId = municipality.Id };
```

For `CreateListingCommandHandlerTests`, any `CreateListingCommand` that had `City = "..."` must change to `MunicipalityId = municipality.Id`.

- [ ] **Step 2: Run all listing tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ListingHandlerTests" ./Automotive.Marketplace.sln`
Expected: All tests PASS.

- [ ] **Step 3: Run the full test suite to check for any remaining failures**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Tests/
git commit -m "test: update existing listing tests to use MunicipalityId FK

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 12: Frontend — Types, Endpoints, and Query Hook

**Files:**
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/GetAllListingsQuery.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/GetAllListingsResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/basicFilter.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/UpdateListingCommand.ts`
- Modify: `automotive.marketplace.client/src/features/createListing/types/CreateListingCommand.ts`
- Modify: `automotive.marketplace.client/src/features/savedListings/types/SavedListing.ts`
- Modify: `automotive.marketplace.client/src/features/compareListings/types/SearchListingsResponse.ts`
- Modify: `automotive.marketplace.client/src/features/search/types/listingSearchStateValues.ts`
- Create: `automotive.marketplace.client/src/features/listingList/api/getMunicipalitiesOptions.ts`

- [ ] **Step 1: Add MUNICIPALITY endpoint to endpoints.ts**

In `src/constants/endpoints.ts`, add after `DRIVETRAIN`:
```typescript
MUNICIPALITY: {
  GET_ALL: "/Municipality/GetAll",
},
```

- [ ] **Step 2: Update GetAllListingsQuery.ts — rename city to municipalityId**

```typescript
export type GetAllListingsQuery = {
  makeId?: string;
  models?: string[];
  municipalityId?: string;
  isUsed?: boolean;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
};
```

- [ ] **Step 3: Update GetAllListingsResponse.ts — rename city to municipalityName**

Replace `city: string;` with `municipalityName: string;`

- [ ] **Step 4: Update basicFilter.ts — rename city to municipalityId**

```typescript
import { CarConditionKey } from "@/constants/carConditions";

export type BasicFilter = {
  makeId: string;
  isUsed: CarConditionKey;
  municipalityId: string;
};
```

- [ ] **Step 5: Update GetListingByIdResponse.ts — rename city, add municipalityId**

Replace:
```typescript
city: string;
```
With:
```typescript
municipalityId: string;
municipalityName: string;
```

- [ ] **Step 6: Update UpdateListingCommand.ts — rename city to municipalityId**

Replace `city: string;` with `municipalityId: string;`

- [ ] **Step 7: Update CreateListingCommand.ts — rename city to municipalityId**

Replace `city: string;` with `municipalityId: string;`

- [ ] **Step 8: Update SavedListing.ts — rename city to municipalityName**

Replace `city: string;` with `municipalityName: string;`

- [ ] **Step 9: Update compareListings/types/SearchListingsResponse.ts — rename city to municipalityName**

Replace `city: string;` with `municipalityName: string;`

- [ ] **Step 10: Update listingSearchStateValues.ts — rename city to municipalityId**

```typescript
import { CarConditionKey } from "@/constants/carConditions";

export type ListingSearchStateValues = {
  makeId: string;
  models: string[];
  municipalityId: string;
  isUsed: CarConditionKey;
  minYear: string;
  maxYear: string;
  minPrice: string;
  maxPrice: string;
};
```

- [ ] **Step 11: Create getMunicipalitiesOptions.ts**

```typescript
// src/features/listingList/api/getMunicipalitiesOptions.ts
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";

export type MunicipalityOption = {
  id: string;
  name: string;
};

const getMunicipalities = () =>
  axiosClient.get<MunicipalityOption[]>(ENDPOINTS.MUNICIPALITY.GET_ALL);

export const getMunicipalitiesOptions = () =>
  queryOptions({
    queryKey: ["municipalities"],
    queryFn: getMunicipalities,
    staleTime: Infinity,
  });
```

- [ ] **Step 12: Commit**

```bash
git add automotive.marketplace.client/src/
git commit -m "feat(fe): update frontend types for MunicipalityId/MunicipalityName rename and add getMunicipalitiesOptions

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 13: Frontend — Schemas + Search Utils

**Files:**
- Modify: `src/features/search/schemas/listingSearchSchema.ts`
- Modify: `src/features/search/utils/listingSearchUtils.ts`
- Modify: `src/features/createListing/schemas/createListingSchema.ts`
- Modify: `src/features/listingDetails/schemas/updateListingSchema.ts`

- [ ] **Step 1: Update listingSearchSchema.ts — rename city to municipalityId with GUID validation**

Replace:
```typescript
city: z.string().optional().catch(undefined),
```
With:
```typescript
municipalityId: z.string().regex(VALIDATION.GUID.REGEX).optional().catch(undefined),
```

- [ ] **Step 2: Update listingSearchUtils.ts — rename city → municipalityId throughout**

In `mapSearchValuesToSearchParams`, replace:
```typescript
city:
  searchValues.city === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
    ? undefined
    : searchValues.city,
```
With:
```typescript
municipalityId:
  searchValues.municipalityId === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
    ? undefined
    : searchValues.municipalityId,
```

In `mapSearchParamsToSearchValues`, replace:
```typescript
city: searchParams.city ?? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
```
With:
```typescript
municipalityId: searchParams.municipalityId ?? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
```

In `mapFilterValuesToSearchParams` (which spreads from `mapSearchValuesToSearchParams`), no change needed as it spreads the result.

In `mapSearchParamsToFilterValues`, no change needed (spreads from `mapSearchParamsToSearchValues`).

- [ ] **Step 3: Update createListingSchema.ts — replace city validation with municipalityId GUID validation**

Replace the `city` field in the schema:
```typescript
city: z
  .string()
  .nonempty({ error: () => i18n.t("cityCannotBeEmpty", { ns: "validation" }) })
  .max(VALIDATION.NAME.LONG, {
    error: () => validation.maxLength({ label: "City", length: VALIDATION.NAME.LONG }),
  }),
```
With:
```typescript
municipalityId: z.string().regex(VALIDATION.GUID.REGEX, {
  error: () => i18n.t("pleaseSelect", { field: "municipality", ns: "validation" }),
}),
```

- [ ] **Step 4: Update updateListingSchema.ts — replace city reference with municipalityId**

Replace:
```typescript
city: CreateListingSchema.shape.city,
```
With:
```typescript
municipalityId: CreateListingSchema.shape.municipalityId,
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/
git commit -m "feat(fe): update listing search and form schemas for municipalityId

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 14: Frontend — Update LocationCombobox

**Files:**
- Modify: `src/components/forms/select/LocationCombobox.tsx`

- [ ] **Step 1: Replace hardcoded locations with API call**

Replace the entire file content:

```typescript
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { getMunicipalitiesOptions } from "@/features/listingList/api/getMunicipalitiesOptions";
import { UI_CONSTANTS } from "@/constants/uiConstants";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { ChevronDown } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

type LocationComboboxProps = {
  value: string;
  onValueChange: (value: string) => void;
  className?: string;
};

const LocationCombobox = ({
  value,
  onValueChange,
  className,
}: LocationComboboxProps) => {
  const { t } = useTranslation("common");
  const [isPopoverOpen, setIsPopoverOpen] = useState<boolean>(false);
  const { data: municipalities = [] } = useQuery(getMunicipalitiesOptions());

  const selectedName =
    value === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
      ? null
      : municipalities.find((m) => m.id === value)?.name;

  return (
    <div>
      <Popover open={isPopoverOpen} onOpenChange={setIsPopoverOpen}>
        <PopoverTrigger aria-label={t("aria.location")} asChild>
          <Button
            variant="outline"
            role="location-combobox"
            className={cn(
              "w-full justify-between bg-transparent font-normal",
              className,
            )}
          >
            <div className="grid grid-cols-1 justify-items-start">
              <span className="text-muted-foreground text-xs">
                {t("select.location")}
              </span>
              <span className="truncate text-sm">
                {selectedName ?? t("select.anyLocation")}
              </span>
            </div>
            <ChevronDown className="opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent>
          <Command>
            <CommandInput placeholder={t("select.searchLocation")} />
            <CommandList>
              <CommandEmpty>{t("select.noLocationFound")}</CommandEmpty>
              <CommandGroup>
                {municipalities.map((municipality) => (
                  <CommandItem
                    key={municipality.id}
                    value={municipality.name}
                    onSelect={() => {
                      onValueChange(
                        municipality.id === value
                          ? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
                          : municipality.id,
                      );
                      setIsPopoverOpen(false);
                    }}
                  >
                    {municipality.name}
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    </div>
  );
};

export default LocationCombobox;
```

> **Note on CommandItem value:** Using `municipality.name` as the `value` prop on `CommandItem` enables the built-in search/filter in the `CommandInput` to match against Lithuanian municipality names. The `onSelect` handler uses `municipality.id` (UUID) as the actual combobox value.

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/components/forms/select/LocationCombobox.tsx
git commit -m "feat(fe): update LocationCombobox to source municipalities from API

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 15: Frontend — Update CreateListingForm + EditListingForm

**Files:**
- Modify: `src/features/createListing/components/CreateListingForm.tsx`
- Modify: `src/features/listingDetails/components/EditListingForm.tsx`

- [ ] **Step 1: Update CreateListingForm.tsx — replace city Input with LocationCombobox**

In `CreateListingForm.tsx`:

a. Replace the `city` field's default value in `form.reset(...)` / initial values from `city: ""` to `municipalityId: ""` (or to `UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE`).

b. Add the import for `LocationCombobox` and `UI_CONSTANTS`:
```typescript
import LocationCombobox from "@/components/forms/select/LocationCombobox";
import { UI_CONSTANTS } from "@/constants/uiConstants";
```

c. Replace the city `FormField`:
```tsx
// Before:
<FormField
  name="city"
  control={form.control}
  render={({ field }) => (
    <FormItem className="flex flex-col justify-start">
      <FormLabel>{t("form.city")}</FormLabel>
      <FormControl>
        <Input
          placeholder={t("form.cityPlaceholder")}
          type="text"
          {...field}
        />
      </FormControl>
      <FormMessage />
    </FormItem>
  )}
/>

// After:
<FormField
  name="municipalityId"
  control={form.control}
  render={({ field }) => (
    <FormItem className="flex flex-col justify-start">
      <FormLabel>{t("form.city")}</FormLabel>
      <FormControl>
        <LocationCombobox
          value={field.value || UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE}
          onValueChange={field.onChange}
        />
      </FormControl>
      <FormMessage />
    </FormItem>
  )}
/>
```

d. In the form default values (wherever `city: ""` appears), replace with `municipalityId: UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE`.

- [ ] **Step 2: Update EditListingForm.tsx — replace city Input with LocationCombobox**

In `EditListingForm.tsx`:

a. Add imports for `LocationCombobox` and `UI_CONSTANTS`.

b. Change the initial form values: `city: listing.city` → `municipalityId: listing.municipalityId`

c. Replace the city `FormField` with the same `LocationCombobox` pattern as above (field name `"municipalityId"`).

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/createListing/ \
        automotive.marketplace.client/src/features/listingDetails/components/EditListingForm.tsx
git commit -m "feat(fe): replace city text inputs with LocationCombobox in listing forms

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 16: Frontend — Update Filter and Search Components

**Files:**
- Modify: `src/features/listingList/components/BasicFilters.tsx`
- Modify: `src/features/listingList/components/Filters.tsx`
- Modify: `src/features/search/components/ListingSearchFilters.tsx`
- Modify: `src/features/search/components/ListingSearch.tsx`

- [ ] **Step 1: Update BasicFilters.tsx — city → municipalityId**

Replace:
```tsx
value={filters.city}
onValueChange={(value) => onFilterChange("city", value)}
```
With:
```tsx
value={filters.municipalityId}
onValueChange={(value) => onFilterChange("municipalityId", value)}
```

- [ ] **Step 2: Update Filters.tsx — city → municipalityId**

Replace:
```tsx
city: filterValues.city,
```
With:
```tsx
municipalityId: filterValues.municipalityId,
```

Also update `onFilterChange("city", ...)` → `onFilterChange("municipalityId", ...)` if applicable.

- [ ] **Step 3: Update ListingSearchFilters.tsx — city → municipalityId**

Replace:
```tsx
value={searchValues.city}
onValueChange={(value) => updateSearchValue("city", value)}
```
With:
```tsx
value={searchValues.municipalityId}
onValueChange={(value) => updateSearchValue("municipalityId", value)}
```

- [ ] **Step 4: Update ListingSearch.tsx — rename city in initial state**

In the `useState` initial value, replace:
```typescript
city: UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
```
With:
```typescript
municipalityId: UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/
git commit -m "feat(fe): update filter and search components for municipalityId rename

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 17: Frontend — Update MyListingDetail Municipality Editing

**Files:**
- Modify: `src/features/myListings/components/MyListingDetail.tsx`

- [ ] **Step 1: Replace the city EditableField with a LocationCombobox**

In `MyListingDetail.tsx`:

a. Add imports:
```typescript
import LocationCombobox from "@/components/forms/select/LocationCombobox";
import { UI_CONSTANTS } from "@/constants/uiConstants";
```

b. In the `pendingChanges` state tracking and `handleFieldChange`, the `"city"` key changes to `"municipalityId"`. Find and replace:
```typescript
city: (pendingChanges.city as string) ?? listing.city,
```
With:
```typescript
municipalityId: (pendingChanges.municipalityId as string) ?? listing.municipalityId,
```

c. Replace the `EditableField` for city with a `LocationCombobox`:
```tsx
// Before:
<EditableField
  label={t("fields.city")}
  value={listing.city}
  pendingValue={pendingChanges.city as string | undefined}
  type="text"
  onConfirm={(value) => handleFieldChange("city", value)}
/>

// After:
<div>
  <label className="text-sm font-medium">{t("fields.city")}</label>
  <LocationCombobox
    value={
      (pendingChanges.municipalityId as string | undefined)
      ?? listing.municipalityId
      ?? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
    }
    onValueChange={(value) => handleFieldChange("municipalityId", value)}
  />
</div>
```

d. In the `handleFieldChange("city", ...)` call chain → make sure any submission code sends `municipalityId` to the `UpdateListingCommand`.

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/MyListingDetail.tsx
git commit -m "feat(fe): update MyListingDetail to use LocationCombobox for municipality editing

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 18: Frontend — Update Display Components (city → municipalityName)

These are all read-only display changes. Replace every `listing.city` (or `x.city`) with `listing.municipalityName` (or `x.municipalityName`).

**Files:**
- `src/features/listingList/components/ListingCard.tsx`
- `src/features/myListings/components/MyListingCard.tsx`
- `src/features/listingDetails/components/ListingDetailsContent.tsx`
- `src/features/savedListings/components/SavedListingRow.tsx`
- `src/features/savedListings/components/PropertyMentionPicker.tsx`
- `src/features/compareListings/components/CompareTable.tsx`
- `src/features/compareListings/components/CompareHeader.tsx`
- `src/features/compareListings/components/CompareSearchModal.tsx`

- [ ] **Step 1: ListingCard.tsx**

Replace `listing.city` with `listing.municipalityName`.

- [ ] **Step 2: MyListingCard.tsx**

Replace `listing.city` with `listing.municipalityName`.

- [ ] **Step 3: ListingDetailsContent.tsx**

Replace `listing.city` with `listing.municipalityName`.

- [ ] **Step 4: SavedListingRow.tsx**

Replace `listing.city` with `listing.municipalityName`.

- [ ] **Step 5: PropertyMentionPicker.tsx**

Replace:
```typescript
{ key: "city", labelKey: "propertyMention.city", format: (v) => v as string },
```
With:
```typescript
{ key: "municipalityName", labelKey: "propertyMention.city", format: (v) => v as string },
```

- [ ] **Step 6: CompareTable.tsx**

Replace:
```typescript
{ field: "city", label: t("table.city") },
```
With:
```typescript
{ field: "municipalityName", label: t("table.city") },
```

- [ ] **Step 7: CompareHeader.tsx**

Replace `listing.city` with `listing.municipalityName`.

- [ ] **Step 8: CompareSearchModal.tsx**

Replace `listing.city` (in both lines, around the mileage display) with `listing.municipalityName`.

- [ ] **Step 9: Commit**

```bash
git add automotive.marketplace.client/src/
git commit -m "feat(fe): rename city to municipalityName in all display components

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 19: Frontend Build + Lint Verification

- [ ] **Step 1: Run lint and format check**

```bash
cd automotive.marketplace.client
npm run lint && npm run format:check
```

Expected: No errors or warnings.

- [ ] **Step 2: Run TypeScript build**

```bash
npm run build
```

Expected: Build succeeds with 0 type errors.

- [ ] **Step 3: Fix any remaining type errors or lint issues, then commit**

```bash
git add automotive.marketplace.client/
git commit -m "fix(fe): resolve any lint or type errors from municipalities feature

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 20: Final Verification

- [ ] **Step 1: Run the full backend test suite**

```bash
dotnet test ./Automotive.Marketplace.sln
```

Expected: All tests PASS.

- [ ] **Step 2: Final commit if any remaining changes**

```bash
git add .
git commit -m "feat: municipalities feature — complete

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
