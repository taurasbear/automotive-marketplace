# vPIC Makes & Models Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Bogus-seeded `Make`/`Model` dev data with auto-synced real vehicle data from the NHTSA vPIC passenger-car API, running on startup and re-checking every 24 h (with a 30-day staleness window).

**Architecture:** A `VehicleDataInitializer` (mirrors `MunicipalityInitializer`) fetches ~75 allowed car makes from vPIC, upserts them, then concurrently fetches models per make (SemaphoreSlim(10)) and upserts those too. Both entities gain `VpicId` (int?, sync key with unique index), `VpicName` (raw API string), and `SyncedAt` (DateTime?). A `MakeExclusion` table (seeded at startup in all environments) holds 118 niche US-only brand IDs to skip. A `VehicleDataSyncService` BackgroundService wakes every 24 h to re-run the initializer. `MakeSeeder` and `ModelSeeder` are deleted; `VariantSeeder` is capped at 20 models for dev sanity.

**Tech Stack:** .NET 8, EF Core (Npgsql), `System.Globalization.CultureInfo` (Title Case), `SemaphoreSlim`, `System.Net.Http.Json`, NSubstitute, FluentAssertions, xUnit, TestContainers, Respawn.

---

## File Map

### New files
| File | Purpose |
|---|---|
| `Automotive.Marketplace.Domain/Entities/MakeExclusion.cs` | Exclusion entity (not BaseEntity) |
| `Automotive.Marketplace.Application/Interfaces/Services/IVehicleDataApiClient.cs` | DTOs + API client interface |
| `Automotive.Marketplace.Infrastructure/Interfaces/IVehicleDataInitializer.cs` | Initializer interface |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeConfiguration.cs` | Unique index on VpicId |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeExclusionConfiguration.cs` | PK config for MakeExclusion |
| `Automotive.Marketplace.Infrastructure/Services/VpicVehicleDataApiClient.cs` | HTTP client impl |
| `Automotive.Marketplace.Infrastructure/Sync/VehicleDataInitializer.cs` | Startup + 30-day sync logic |
| `Automotive.Marketplace.Infrastructure/Data/Seeders/MakeExclusionSeeder.cs` | Seeds 118 excluded makes |
| `Automotive.Marketplace.Server/Services/VehicleDataSyncService.cs` | BackgroundService (24 h check) |
| `Automotive.Marketplace.Tests/Features/HandlerTests/VehicleDataHandlerTests/VehicleDataInitializerTests.cs` | Initializer integration tests |
| New EF migration | Adds columns + MakeExclusions table |

### Modified files
| File | Change |
|---|---|
| `Automotive.Marketplace.Domain/Entities/Make.cs` | Add `VpicId`, `VpicName`, `SyncedAt` |
| `Automotive.Marketplace.Domain/Entities/Model.cs` | Add `VpicId`, `VpicName`, `SyncedAt` |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/ModelConfiguration.cs` | Add unique index on `VpicId` |
| `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` | Add `DbSet<MakeExclusion>` |
| `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs` | Remove `MakeSeeder`/`ModelSeeder`; add HTTP client, initializer, exclusion seeder |
| `Automotive.Marketplace.Infrastructure/Data/Seeders/VariantSeeder.cs` | Cap to first 20 models |
| `Automotive.Marketplace.Server/Program.cs` | Call `MakeExclusionSeeder`, `VehicleDataInitializer`; register `VehicleDataSyncService` |

### Deleted files
| File |
|---|
| `Automotive.Marketplace.Infrastructure/Data/Seeders/MakeSeeder.cs` |
| `Automotive.Marketplace.Infrastructure/Data/Seeders/ModelSeeder.cs` |

---

## Task 1: Update Domain Entities and EF Configuration

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/Make.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Model.cs`
- Create: `Automotive.Marketplace.Domain/Entities/MakeExclusion.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/ModelConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeExclusionConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Add VpicId, VpicName, SyncedAt to Make**

Replace the contents of `Automotive.Marketplace.Domain/Entities/Make.cs`:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Make : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? VpicId { get; set; }
    public string VpicName { get; set; } = string.Empty;
    public DateTime? SyncedAt { get; set; }

    public virtual ICollection<Model> Models { get; set; } = [];
}
```

- [ ] **Step 2: Add VpicId, VpicName, SyncedAt to Model**

Replace the contents of `Automotive.Marketplace.Domain/Entities/Model.cs`:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Model : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? VpicId { get; set; }
    public string VpicName { get; set; } = string.Empty;
    public DateTime? SyncedAt { get; set; }

    public Guid MakeId { get; set; }

    public virtual Make Make { get; set; } = null!;

    public virtual ICollection<Variant> Variants { get; set; } = [];
}
```

- [ ] **Step 3: Create MakeExclusion entity**

Create `Automotive.Marketplace.Domain/Entities/MakeExclusion.cs`:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class MakeExclusion
{
    public int VpicId { get; set; }
    public string VpicName { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create MakeConfiguration**

Create `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeConfiguration.cs`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MakeConfiguration : IEntityTypeConfiguration<Make>
{
    public void Configure(EntityTypeBuilder<Make> builder)
    {
        builder.HasIndex(m => m.VpicId).IsUnique();
        builder.Property(m => m.VpicName).HasMaxLength(200);
    }
}
```

- [ ] **Step 5: Update ModelConfiguration with unique index**

Replace the contents of `Automotive.Marketplace.Infrastructure/Data/Configuration/ModelConfiguration.cs`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.HasOne(model => model.Make)
            .WithMany(make => make.Models)
            .HasForeignKey(model => model.MakeId);

        builder.HasIndex(m => m.VpicId).IsUnique();
        builder.Property(m => m.VpicName).HasMaxLength(200);
    }
}
```

- [ ] **Step 6: Create MakeExclusionConfiguration**

Create `Automotive.Marketplace.Infrastructure/Data/Configuration/MakeExclusionConfiguration.cs`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MakeExclusionConfiguration : IEntityTypeConfiguration<MakeExclusion>
{
    public void Configure(EntityTypeBuilder<MakeExclusion> builder)
    {
        builder.HasKey(e => e.VpicId);
        builder.Property(e => e.VpicId).ValueGeneratedNever();
        builder.Property(e => e.VpicName).IsRequired().HasMaxLength(200);
    }
}
```

- [ ] **Step 7: Add DbSet<MakeExclusion> to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add after the existing `DbSet<Municipality>` line:

```csharp
    public DbSet<MakeExclusion> MakeExclusions { get; set; }
```

- [ ] **Step 8: Build to confirm no compile errors**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add VpicId/VpicName/SyncedAt to Make and Model, add MakeExclusion entity and EF config"
```

---

## Task 2: Generate and Apply Migration

**Files:**
- Create: new migration files under `Automotive.Marketplace.Infrastructure/Migrations/`

- [ ] **Step 1: Generate the migration**

Run from the repository root:

```bash
dotnet ef migrations add AddVpicFieldsAndMakeExclusions \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected output: `Done. To undo this action, use 'ef migrations remove'`

- [ ] **Step 2: Verify the migration is correct**

Open the generated `*_AddVpicFieldsAndMakeExclusions.cs` and confirm it contains:
- `AddColumn` for `VpicId` (int, nullable) on `Makes` and `Models`
- `AddColumn` for `VpicName` (varchar(200), not null, defaultValue: `""`) on `Makes` and `Models`
- `AddColumn` for `SyncedAt` (timestamp, nullable) on `Makes` and `Models`
- `CreateTable` for `MakeExclusions` with columns `VpicId` (int, PK) and `VpicName` (varchar(200))
- `CreateIndex` for unique index on `Makes.VpicId` and `Models.VpicId`

If `VpicName` is generated as nullable, open the migration and change `nullable: true` to `nullable: false, defaultValue: ""` for both tables. EF may not infer the default from the C# initializer.

- [ ] **Step 3: Commit the migration**

```bash
git add -A
git commit -m "feat: migration AddVpicFieldsAndMakeExclusions"
```

---

## Task 3: Application Layer Interface

**Files:**
- Create: `Automotive.Marketplace.Application/Interfaces/Services/IVehicleDataApiClient.cs`

- [ ] **Step 1: Create the interface with DTOs**

Create `Automotive.Marketplace.Application/Interfaces/Services/IVehicleDataApiClient.cs`:

```csharp
namespace Automotive.Marketplace.Application.Interfaces.Services;

public record VpicMakeDto(int VpicId, string VpicName);
public record VpicModelDto(int VpicId, string VpicName, int MakeVpicId);

public interface IVehicleDataApiClient
{
    Task<IEnumerable<VpicMakeDto>> FetchCarMakesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VpicModelDto>> FetchModelsForMakeAsync(int vpicMakeId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

---

## Task 4: Infrastructure Initializer Interface

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Interfaces/IVehicleDataInitializer.cs`

- [ ] **Step 1: Create the interface**

Create `Automotive.Marketplace.Infrastructure/Interfaces/IVehicleDataInitializer.cs`:

```csharp
namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface IVehicleDataInitializer
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
```

---

## Task 5: Write Failing Tests (TDD)

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/VehicleDataHandlerTests/VehicleDataInitializerTests.cs`

- [ ] **Step 1: Create the test directory and file**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/VehicleDataHandlerTests/VehicleDataInitializerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Sync;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VehicleDataHandlerTests;

public class VehicleDataInitializerTests(
    DatabaseFixture<VehicleDataInitializerTests> fixture)
    : IClassFixture<DatabaseFixture<VehicleDataInitializerTests>>, IAsyncLifetime
{
    private readonly IVehicleDataApiClient _apiClient = Substitute.For<IVehicleDataApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private VehicleDataInitializer CreateInitializer(AutomotiveContext context)
        => new(context, _apiClient, NullLogger<VehicleDataInitializer>.Instance);

    [Fact]
    public async Task RunAsync_EmptyTable_ShouldFetchAndInsertMakesAndModels()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var makeDto = new VpicMakeDto(474, "HONDA");
        var modelDto = new VpicModelDto(1861, "Accord", 474);
        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>()).Returns([makeDto]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>()).Returns([modelDto]);

        await CreateInitializer(context).RunAsync();

        var makes = context.Set<Make>().ToList();
        var models = context.Set<Model>().ToList();

        makes.Should().HaveCount(1);
        makes[0].VpicId.Should().Be(474);
        makes[0].VpicName.Should().Be("HONDA");
        makes[0].Name.Should().Be("Honda");
        makes[0].SyncedAt.Should().NotBeNull();
        makes[0].SyncedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        models.Should().HaveCount(1);
        models[0].VpicId.Should().Be(1861);
        models[0].VpicName.Should().Be("Accord");
        models[0].Name.Should().Be("Accord");
        models[0].MakeId.Should().Be(makes[0].Id);
        models[0].SyncedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_FreshData_ShouldSkipApiCall()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.Set<Make>().AddAsync(new Make
        {
            Id = Guid.NewGuid(),
            Name = "Honda",
            VpicId = 474,
            VpicName = "HONDA",
            SyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        });
        await context.SaveChangesAsync();

        await CreateInitializer(context).RunAsync();

        await _apiClient.DidNotReceive().FetchCarMakesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_StaleData_ShouldUpdateExistingMake()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var existingId = Guid.NewGuid();
        await context.Set<Make>().AddAsync(new Make
        {
            Id = existingId,
            Name = "Old Name",
            VpicId = 474,
            VpicName = "OLD",
            SyncedAt = DateTime.UtcNow.AddDays(-31),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        });
        await context.SaveChangesAsync();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns([new VpicMakeDto(474, "HONDA")]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<VpicModelDto>());

        await CreateInitializer(context).RunAsync();

        var updated = context.Set<Make>().Single(m => m.Id == existingId);
        updated.Name.Should().Be("Honda");
        updated.VpicName.Should().Be("HONDA");
        updated.SyncedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updated.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task RunAsync_ExcludedMake_ShouldNotBeUpserted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.Set<MakeExclusion>().AddAsync(new MakeExclusion
        {
            VpicId = 465,
            VpicName = "MERCURY"
        });
        await context.SaveChangesAsync();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns([new VpicMakeDto(465, "MERCURY"), new VpicMakeDto(474, "HONDA")]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<VpicModelDto>());

        await CreateInitializer(context).RunAsync();

        var makes = context.Set<Make>().ToList();
        makes.Should().HaveCount(1);
        makes[0].VpicId.Should().Be(474);
        await _apiClient.DidNotReceive().FetchModelsForMakeAsync(465, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ApiFailure_ShouldLogAndNotThrow()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IEnumerable<VpicMakeDto>>(new HttpRequestException("Network error")));

        var act = async () => await CreateInitializer(context).RunAsync();

        await act.Should().NotThrowAsync();
        context.Set<Make>().Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run the tests to confirm they fail (VehicleDataInitializer does not exist yet)**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "FullyQualifiedName~VehicleDataInitializerTests" \
  --no-build 2>&1 | tail -20
```

Expected: Compilation error — `VehicleDataInitializer` type not found. This confirms the tests are wired correctly and ready to drive the implementation.

- [ ] **Step 3: Commit the failing tests**

```bash
git add -A
git commit -m "test: add VehicleDataInitializerTests (failing — implementation pending)"
```

---

## Task 6: Implement VpicVehicleDataApiClient

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Services/VpicVehicleDataApiClient.cs`

- [ ] **Step 1: Create the HTTP client**

Create `Automotive.Marketplace.Infrastructure/Services/VpicVehicleDataApiClient.cs`:

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class VpicVehicleDataApiClient(HttpClient httpClient) : IVehicleDataApiClient
{
    private const string MakesUrl =
        "https://vpic.nhtsa.dot.gov/api/vehicles/GetMakesForVehicleType/car?format=json";

    private static string ModelsUrl(int makeId) =>
        $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeIdYear/makeId/{makeId}/vehicletype/car?format=json";

    public async Task<IEnumerable<VpicMakeDto>> FetchCarMakesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<MakesResponse>(MakesUrl, cancellationToken);
        return response?.Results?
                   .Select(r => new VpicMakeDto(r.MakeId, r.MakeName))
               ?? [];
    }

    public async Task<IEnumerable<VpicModelDto>> FetchModelsForMakeAsync(
        int vpicMakeId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ModelsResponse>(
            ModelsUrl(vpicMakeId), cancellationToken);
        return response?.Results?
                   .Select(r => new VpicModelDto(r.ModelId, r.ModelName, r.MakeId))
               ?? [];
    }

    private class MakesResponse
    {
        [JsonPropertyName("Results")]
        public List<MakeResult> Results { get; set; } = [];
    }

    private class MakeResult
    {
        [JsonPropertyName("MakeId")]
        public int MakeId { get; set; }

        [JsonPropertyName("MakeName")]
        public string MakeName { get; set; } = string.Empty;
    }

    private class ModelsResponse
    {
        [JsonPropertyName("Results")]
        public List<ModelResult> Results { get; set; } = [];
    }

    private class ModelResult
    {
        [JsonPropertyName("Model_ID")]
        public int ModelId { get; set; }

        [JsonPropertyName("Model_Name")]
        public string ModelName { get; set; } = string.Empty;

        [JsonPropertyName("Make_ID")]
        public int MakeId { get; set; }
    }
}
```

- [ ] **Step 2: Build to verify**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

---

## Task 7: Implement VehicleDataInitializer (make tests pass)

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Sync/VehicleDataInitializer.cs`

- [ ] **Step 1: Create the initializer**

Create `Automotive.Marketplace.Infrastructure/Sync/VehicleDataInitializer.cs`:

```csharp
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Automotive.Marketplace.Infrastructure.Sync;

public class VehicleDataInitializer(
    AutomotiveContext context,
    IVehicleDataApiClient apiClient,
    ILogger<VehicleDataInitializer> logger) : IVehicleDataInitializer
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(30);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasSynced = await context.Set<Make>()
                .AnyAsync(m => m.SyncedAt.HasValue, cancellationToken);

            var isStale = !hasSynced
                || await context.Set<Make>()
                    .Where(m => m.SyncedAt.HasValue)
                    .MinAsync(m => m.SyncedAt!.Value, cancellationToken)
                   < DateTime.UtcNow - SyncInterval;

            if (!isStale)
            {
                logger.LogInformation("Vehicle data is fresh, skipping sync.");
                return;
            }

            logger.LogInformation("Syncing vehicle data from vPIC API...");
            var syncedAt = DateTime.UtcNow;

            var allMakes = (await apiClient.FetchCarMakesAsync(cancellationToken)).ToList();
            logger.LogInformation("Fetched {Count} makes from vPIC.", allMakes.Count);

            var excludedVpicIds = await context.Set<MakeExclusion>()
                .Select(e => e.VpicId)
                .ToHashSetAsync(cancellationToken);

            var allowedMakes = allMakes.Where(m => !excludedVpicIds.Contains(m.VpicId)).ToList();
            logger.LogInformation("{Allowed} makes allowed after exclusion filter.", allowedMakes.Count);

            var existingMakesByVpicId = await context.Set<Make>()
                .Where(m => m.VpicId.HasValue)
                .ToDictionaryAsync(m => m.VpicId!.Value, cancellationToken);

            var upsertedMakes = new Dictionary<int, Guid>();

            foreach (var dto in allowedMakes)
            {
                if (existingMakesByVpicId.TryGetValue(dto.VpicId, out var existing))
                {
                    existing.Name = ToTitleCase(dto.VpicName);
                    existing.VpicName = dto.VpicName;
                    existing.SyncedAt = syncedAt;
                    upsertedMakes[dto.VpicId] = existing.Id;
                }
                else
                {
                    var newMake = new Make
                    {
                        Id = Guid.NewGuid(),
                        VpicId = dto.VpicId,
                        VpicName = dto.VpicName,
                        Name = ToTitleCase(dto.VpicName),
                        SyncedAt = syncedAt,
                        CreatedAt = syncedAt,
                        CreatedBy = "system"
                    };
                    await context.Set<Make>().AddAsync(newMake, cancellationToken);
                    upsertedMakes[dto.VpicId] = newMake.Id;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Makes upserted: {Count}.", allowedMakes.Count);

            var semaphore = new SemaphoreSlim(10);
            var modelFetchTasks = allowedMakes.Select(async make =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var models = await apiClient.FetchModelsForMakeAsync(make.VpicId, cancellationToken);
                    return (make.VpicId, models.ToList());
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var allMakeModels = await Task.WhenAll(modelFetchTasks);
            logger.LogInformation("Model data fetched for all makes.");

            var existingModelsByVpicId = await context.Set<Model>()
                .Where(m => m.VpicId.HasValue)
                .ToDictionaryAsync(m => m.VpicId!.Value, cancellationToken);

            foreach (var (makeVpicId, models) in allMakeModels)
            {
                if (!upsertedMakes.TryGetValue(makeVpicId, out var makeGuid))
                    continue;

                foreach (var dto in models)
                {
                    if (existingModelsByVpicId.TryGetValue(dto.VpicId, out var existingModel))
                    {
                        existingModel.Name = ToTitleCase(dto.VpicName);
                        existingModel.VpicName = dto.VpicName;
                        existingModel.SyncedAt = syncedAt;
                        existingModel.MakeId = makeGuid;
                    }
                    else
                    {
                        var newModel = new Model
                        {
                            Id = Guid.NewGuid(),
                            VpicId = dto.VpicId,
                            VpicName = dto.VpicName,
                            Name = ToTitleCase(dto.VpicName),
                            MakeId = makeGuid,
                            SyncedAt = syncedAt,
                            CreatedAt = syncedAt,
                            CreatedBy = "system"
                        };
                        await context.Set<Model>().AddAsync(newModel, cancellationToken);
                        existingModelsByVpicId[dto.VpicId] = newModel;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Vehicle data sync complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Vehicle data sync failed. App will start with existing data.");
        }
    }

    private static string ToTitleCase(string s) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run the tests**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "FullyQualifiedName~VehicleDataInitializerTests" \
  --no-restore -q
```

Expected: 5 tests pass.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: implement VpicVehicleDataApiClient and VehicleDataInitializer"
```

---

## Task 8: VehicleDataSyncService

**Files:**
- Create: `Automotive.Marketplace.Server/Services/VehicleDataSyncService.cs`

- [ ] **Step 1: Create the background service**

Create `Automotive.Marketplace.Server/Services/VehicleDataSyncService.cs`:

```csharp
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Server.Services;

public class VehicleDataSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<VehicleDataSyncService> logger) : BackgroundService
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
                var initializer = scope.ServiceProvider.GetRequiredService<IVehicleDataInitializer>();
                await initializer.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Vehicle data sync check failed.");
            }
        }
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

---

## Task 9: MakeExclusionSeeder

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Seeders/MakeExclusionSeeder.cs`

The following 118 makes are niche US-only brands, defunct marques, or brands not sold in European markets. They are excluded so only relevant car brands appear in the Lithuanian marketplace.

- [ ] **Step 1: Create the seeder**

Create `Automotive.Marketplace.Infrastructure/Data/Seeders/MakeExclusionSeeder.cs`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class MakeExclusionSeeder(AutomotiveContext context)
{
    private static readonly (int VpicId, string VpicName)[] Exclusions =
    [
        (465, "MERCURY"),
        (470, "HOLDEN"),
        (472, "GMC"),
        (475, "ACURA"),
        (536, "PONTIAC"),
        (606, "AM GENERAL"),
        (629, "CREATIVE COACHWORKS"),
        (771, "AC PROPULSION"),
        (972, "FALCON"),
        (986, "EV INNOVATIONS"),
        (992, "FAW JIAXING HAPPY MESSENGER"),
        (1056, "SATURN"),
        (1124, "AMERICAN MOTORS"),
        (1142, "FORMULA 1 STREET COM"),
        (1146, "GEO"),
        (1151, "FORTUNESPORT VES"),
        (1288, "AAS"),
        (1292, "EQUUS AUTOMOTIVE"),
        (1393, "ELECTRIC MOBILE CARS"),
        (1498, "AVERA MOTORS"),
        (1683, "BAKKURA MOBILITY"),
        (1777, "CODA"),
        (1869, "CONTEMPORARY CLASSIC CARS (CCC)"),
        (2018, "KANDI"),
        (2049, "KEPLER MOTORS"),
        (2131, "MAKING YOU MOBILE"),
        (2376, "MYCAR"),
        (2408, "EAGLE"),
        (2409, "PLYMOUTH"),
        (2665, "NJD AUTOMOTIVE LLC"),
        (2745, "PHOENIX MOTORCARS"),
        (3176, "ROCKET SLED MOTORS"),
        (3440, "VISION INDUSTRIES"),
        (3540, "WARHAWK PERFORMANCE"),
        (3583, "UKEYCHEYMA"),
        (3706, "TOTAL ELECTRIC VEHICLES"),
        (4162, "OLDSMOBILE"),
        (4220, "PANOZ"),
        (4355, "SALEEN"),
        (4410, "SOLECTRIA"),
        (4451, "YESTER YEAR AUTO"),
        (4596, "BXR"),
        (4634, "ENGINE CONNECTION"),
        (4764, "MOSLER"),
        (4859, "REVOLOGY"),
        (5015, "EMA"),
        (5042, "COSTIN SPORTS CAR"),
        (5208, "MATRIX MOTOR COMPANY"),
        (5367, "ARMBRUSTER STAGEWAY"),
        (5381, "LUMEN"),
        (5464, "ASUNA"),
        (5468, "MERKUR"),
        (5552, "AVANTI"),
        (5555, "STERLING MOTOR CAR"),
        (5557, "CONSULIER GTP"),
        (5760, "VINTAGE AUTO"),
        (5767, "LONDONCOACH INC"),
        (5848, "MGS GRAND SPORT (MARDIKIAN)"),
        (5938, "PANTHER"),
        (6018, "DAYTONA COACH BUILDERS"),
        (6069, "UCC"),
        (6263, "RS SPIDER"),
        (6264, "GRUPPE B"),
        (6265, "RALLY SPORT"),
        (6313, "RENAISSANCE"),
        (6364, "JAC 427"),
        (6408, "HUNTER DESIGN GROUP, LLC"),
        (6473, "BLACKWATER"),
        (6663, "GULLWING INTERNATIONAL MOTORS, LTD."),
        (6759, "AMERITECH CORPORATION"),
        (6792, "STANFORD CUSTOMS"),
        (6880, "CLASSIC ROADSTERS"),
        (6986, "HERITAGE"),
        (7099, "COBRA CARS"),
        (7207, "C-R CHEETAH RACE CARS"),
        (7225, "PAS"),
        (7425, "BUG MOTORS"),
        (7477, "EXCALIBUR AUTOMOBILE CORPORATION"),
        (7765, "IVES MOTORS CORPORATION (IMC)"),
        (7836, "AUTODELTA USA INC"),
        (8395, "AUTOCAR LTD"),
        (8605, "BBC"),
        (8785, "PHOENIX SPORTS CARS, INC."),
        (9250, "VECTOR AEROMOTIVE CORPORATION"),
        (9326, "CARBODIES"),
        (9364, "CREATIVE COACHWORKS INC."),
        (9376, "WESTFALL MOTORS CORP."),
        (9401, "CLENET"),
        (9448, "ELECTRIC CAR COMPANY"),
        (9458, "CX AUTOMOTIVE"),
        (9533, "LA EXOTICS"),
        (9572, "CLASSIC SPORTS CARS"),
        (9720, "SF MOTORS INC."),
        (10029, "VINTAGE CRUISER"),
        (10030, "VINTAGE MICROBUS"),
        (10031, "VINTAGE ROVER"),
        (10047, "LITE CAR"),
        (10623, "DONGFENG"),
        (10647, "CRUISE"),
        (11076, "CALMOTORS"),
        (11832, "SHELBY"),
        (11938, "ZOOX"),
        (12074, "ECOCAR"),
        (12400, "SUPERCAR SYSTEM"),
        (12706, "KINDIG"),
        (12771, "SSC NORTH AMERICA"),
        (12783, "BALLISTIC"),
        (12894, "MEYERS MANX"),
        (12948, "1955 CUSTOM BELAIR"),
        (12980, "ELKINGTON"),
        (13018, "SHAY REPRODUCTION"),
        (13024, "CLENET COACHWORKS"),
        (13025, "CHECKER"),
        (13028, "CAMELOT"),
        (13380, "BACKDRAFT"),
        (13391, "FALCON MOTORS"),
        (13585, "MAYHEM AUTOWORKZ"),
        (13887, "HEDLEY STUDIOS"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Set<MakeExclusion>().AnyAsync(cancellationToken))
            return;

        var exclusions = Exclusions.Select(e => new MakeExclusion
        {
            VpicId = e.VpicId,
            VpicName = e.VpicName
        });

        await context.Set<MakeExclusion>().AddRangeAsync(exclusions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

---

## Task 10: Remove MakeSeeder/ModelSeeder, Update VariantSeeder

**Files:**
- Delete: `Automotive.Marketplace.Infrastructure/Data/Seeders/MakeSeeder.cs`
- Delete: `Automotive.Marketplace.Infrastructure/Data/Seeders/ModelSeeder.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Seeders/VariantSeeder.cs`

- [ ] **Step 1: Delete MakeSeeder.cs**

```bash
rm Automotive.Marketplace.Infrastructure/Data/Seeders/MakeSeeder.cs
```

- [ ] **Step 2: Delete ModelSeeder.cs**

```bash
rm Automotive.Marketplace.Infrastructure/Data/Seeders/ModelSeeder.cs
```

- [ ] **Step 3: Limit VariantSeeder to first 20 models**

Replace the contents of `Automotive.Marketplace.Infrastructure/Data/Seeders/VariantSeeder.cs`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class VariantSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Variant>().AnyAsync(cancellationToken))
            return;

        var models = await context.Set<Model>()
            .OrderBy(m => m.Name)
            .Take(20)
            .ToListAsync(cancellationToken);

        var fuels = await context.Set<Fuel>().ToListAsync(cancellationToken);
        var transmissions = await context.Set<Transmission>().ToListAsync(cancellationToken);
        var bodyTypes = await context.Set<BodyType>().ToListAsync(cancellationToken);

        if (!models.Any() || !fuels.Any() || !transmissions.Any() || !bodyTypes.Any())
            return;

        var random = new Random(42);

        foreach (var model in models)
        {
            for (int i = 0; i < 3; i++)
            {
                var variant = new VariantBuilder()
                    .WithModel(model.Id)
                    .WithFuel(fuels[random.Next(fuels.Count)].Id)
                    .WithTransmission(transmissions[random.Next(transmissions.Count)].Id)
                    .WithBodyType(bodyTypes[random.Next(bodyTypes.Count)].Id)
                    .Build();

                await context.AddAsync(variant, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

---

## Task 11: Wire Up Service Registrations and Program.cs

**Files:**
- Modify: `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`
- Modify: `Automotive.Marketplace.Server/Program.cs`

- [ ] **Step 1: Update ServiceExtensions.cs**

Replace the contents of `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`:

```csharp
using Amazon.S3;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Data.Seeders;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Automotive.Marketplace.Infrastructure.Services;
using Automotive.Marketplace.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Infrastructure;

public static class ServiceExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration, string? connectionString)
    {
        services.AddDbContext<AutomotiveContext>(opt => opt
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IImageStorageService, S3ImageStorageService>();

        services.AddScoped<IDevelopmentSeeder, UserSeeder>();
        services.AddScoped<IDevelopmentSeeder, FuelSeeder>();
        services.AddScoped<IDevelopmentSeeder, TransmissionSeeder>();
        services.AddScoped<IDevelopmentSeeder, BodyTypeSeeder>();
        services.AddScoped<IDevelopmentSeeder, DrivetrainSeeder>();
        services.AddScoped<IDevelopmentSeeder, DefectCategorySeeder>();
        services.AddScoped<IDevelopmentSeeder, VariantSeeder>();
        services.AddScoped<IDevelopmentSeeder, ListingSeeder>();

        services.AddHttpClient<IMunicipalityApiClient, LithuanianMunicipalityApiClient>();
        services.AddScoped<IMunicipalityInitializer, MunicipalityInitializer>();

        services.AddHttpClient<IVehicleDataApiClient, VpicVehicleDataApiClient>();
        services.AddScoped<IVehicleDataInitializer, VehicleDataInitializer>();
        services.AddScoped<MakeExclusionSeeder>();

        var minioServerURL = configuration["MinIO:ServerURL"];
        var accessKey = configuration["MinIO:AccessKey"];
        var secretKey = configuration["MinIO:SecretKey"];

        services.AddSingleton<IAmazonS3>(opt =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = minioServerURL,
                ForcePathStyle = true,
                UseHttp = true,
            };

            return new AmazonS3Client(accessKey, secretKey, config);
        });
    }
}
```

- [ ] **Step 2: Update Program.cs**

In `Automotive.Marketplace.Server/Program.cs`, update the startup scope block to include the exclusion seeder (before the initializer) and the vehicle data initializer. Replace the startup scope block:

```csharp
using (var scope = app.Services.CreateScope())
{
    var automotiveContext = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
    await automotiveContext.Database.MigrateAsync();

    var makeExclusionSeeder = scope.ServiceProvider.GetRequiredService<MakeExclusionSeeder>();
    await makeExclusionSeeder.SeedAsync(app.Lifetime.ApplicationStopping);

    var municipalityInitializer = scope.ServiceProvider.GetRequiredService<IMunicipalityInitializer>();
    await municipalityInitializer.RunAsync(app.Lifetime.ApplicationStopping);

    var vehicleDataInitializer = scope.ServiceProvider.GetRequiredService<IVehicleDataInitializer>();
    await vehicleDataInitializer.RunAsync(app.Lifetime.ApplicationStopping);

    if (app.Environment.IsDevelopment())
    {
        foreach (var seeder in scope.ServiceProvider.GetServices<IDevelopmentSeeder>())
        {
            await seeder.SeedAsync(CancellationToken.None);
        }
    }
}
```

Also add the `VehicleDataSyncService` hosted service registration. Find the line:

```csharp
builder.Services.AddHostedService<Automotive.Marketplace.Server.Services.MunicipalitySyncService>();
```

And add immediately after it:

```csharp
builder.Services.AddHostedService<Automotive.Marketplace.Server.Services.VehicleDataSyncService>();
```

Also add the missing using at the top of Program.cs if not already present:

```csharp
using Automotive.Marketplace.Infrastructure.Data.Seeders;
```

- [ ] **Step 3: Build**

```bash
dotnet build Automotive.Marketplace.sln --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Run all tests**

```bash
dotnet test Automotive.Marketplace.sln --no-restore -q 2>&1 | tail -30
```

Expected: All tests pass. The 5 `VehicleDataInitializerTests` tests pass. No regressions.

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "feat: wire up vPIC makes/models sync — VehicleDataSyncService, MakeExclusionSeeder, remove MakeSeeder/ModelSeeder, cap VariantSeeder to 20 models"
```

---

## Notes for Implementers

- **Existing dev databases**: If your local dev DB was seeded with Bogus `Make`/`Model` data before this change, those records will remain (null `VpicId`). The sync adds new vPIC records alongside them. Reset your DB with `docker compose down -v && docker compose up -d` to get a clean state.
- **vPIC rate limits**: The API has automated rate control. If you see 429 errors during sync, reduce the `SemaphoreSlim` concurrency from 10 to 5 in `VehicleDataInitializer`.
- **First sync duration**: ~20–40 seconds for 75 makes × ~20 models each. This is a one-time startup cost every 30 days.
- **BMW issue**: `ToTitleCase("BMW")` → `"Bmw"`. This is a known limitation of the Title Case normalization. The `VpicName` column preserves `"BMW"` for the future translation pass.
