# Makes & Models vPIC Sync Design

**Goal:** Replace the manually seeded, Bogus-generated `Make` and `Model` data with real vehicle data sourced from the NHTSA vPIC API. Syncs on startup and every 30 days, filtered to passenger cars only. Niche US-only brands are excluded via a seeded `MakeExclusion` table. Both raw (`VpicName`) and display (`Name`) names are stored to support a future translation pass.

---

## Context

`Make` and `Model` entities already exist with GUID primary keys. They are currently populated by `MakeSeeder` and `ModelSeeder` using Bogus random data. This design replaces that mechanism with a reliable, repeatable sync from the vPIC API, following the same pattern already established for `Municipality`.

The vPIC API uses integer IDs. We add a `VpicId` (int) column to both `Make` and `Model` as the external sync key, preserving GUIDs as primary keys throughout the rest of the app.

---

## API Endpoints Used

| Purpose | Endpoint |
|---|---|
| Fetch all passenger car makes | `GET /api/vehicles/GetMakesForVehicleType/car?format=json` |
| Fetch passenger car models for a make | `GET /api/vehicles/GetModelsForMakeIdYear/makeId/{vpicId}/vehicletype/car?format=json` |

The makes endpoint returns ~193 makes. Each make is fetched for its models separately (193 requests), limited to 10 concurrent requests via `SemaphoreSlim` to respect rate limits.

---

## Domain & Schema

### `Make` entity changes
Three new fields added alongside existing `Id` (GUID PK) and `Name`:

| Field | Type | Notes |
|---|---|---|
| `VpicId` | `int?` | Nullable (pre-existing rows have none); unique index; sync key |
| `VpicName` | `string` | Raw name from vPIC, e.g. `"ASTON MARTIN"` |
| `SyncedAt` | `DateTime` | Used for staleness check (>30 days triggers re-sync) |

`Name` (existing) serves as the display name. Initially set to Title-Cased `VpicName`. A future translation pass may overwrite it without touching `VpicName`.

### `Model` entity changes
Same three fields: `VpicId` (int?, unique), `VpicName` (string), `SyncedAt` (DateTime). Existing `MakeId` GUID FK is unchanged.

### New `MakeExclusion` entity
Not a `BaseEntity` — no audit fields required.

| Field | Type | Notes |
|---|---|---|
| `VpicId` | `int` | PK; the vPIC integer ID of the make to exclude |
| `VpicName` | `string` | Human-readable reference |

The initializer loads all `MakeExclusion.VpicId` values into a `HashSet<int>` and skips any fetched make whose `VpicId` is in the set.

### EF Configuration
- `MakeConfiguration`: add unique index on `VpicId` (nullable — PostgreSQL correctly allows multiple `NULL`s under a unique index, so pre-existing rows without a `VpicId` coexist safely)
- `ModelConfiguration`: same unique index on `VpicId`
- New `MakeExclusionConfiguration`: `VpicId` as PK, `ValueGeneratedNever()`

### Migration
One new migration adds the three columns to `Makes` and `Models`, and creates the `MakeExclusions` table.

---

## Sync Architecture

### `IVehicleDataApiClient` (Application layer)
```csharp
public record VpicMakeDto(int VpicId, string VpicName);
public record VpicModelDto(int VpicId, string VpicName, int MakeVpicId);

public interface IVehicleDataApiClient
{
    Task<IEnumerable<VpicMakeDto>> FetchCarMakesAsync(CancellationToken ct = default);
    Task<IEnumerable<VpicModelDto>> FetchModelsForMakeAsync(int vpicMakeId, CancellationToken ct = default);
}
```

Located at: `Application/Interfaces/Services/IVehicleDataApiClient.cs`

### `VpicVehicleDataApiClient` (Infrastructure)
Typed `HttpClient` implementation. Title-Cases names on the way in (e.g. `"ASTON MARTIN"` → `"Aston Martin"`). Deserializes the two vPIC JSON shapes:
- Makes response: `{ Results: [{ MakeId, MakeName }] }`
- Models response: `{ Results: [{ Model_ID, Model_Name, Make_ID }] }`

Located at: `Infrastructure/Services/VpicVehicleDataApiClient.cs`

### `IVehicleDataInitializer` (Infrastructure interface)
```csharp
public interface IVehicleDataInitializer
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
```
Located at: `Infrastructure/Interfaces/IVehicleDataInitializer.cs`

### `VehicleDataInitializer` (Infrastructure)
Located at: `Infrastructure/Sync/VehicleDataInitializer.cs`

**Algorithm:**
1. Check staleness: query `Make` for any row with a non-null `SyncedAt`. If found and the minimum `SyncedAt` is within 30 days → log and return early. If all rows have `null` `SyncedAt` (e.g., old hand-seeded data) or the table is empty → treat as stale and proceed.
2. Fetch all car makes via `IVehicleDataApiClient.FetchCarMakesAsync`.
3. Load excluded `VpicId`s from `MakeExclusion` into a `HashSet<int>`.
4. Filter the fetched makes, discarding excluded ones.
5. Upsert makes by `VpicId`:
   - Existing `VpicId` match → update `Name`, `VpicName`, `SyncedAt`
   - No match → insert new with a new GUID, Title-Cased `Name`, `VpicName`, `SyncedAt`
6. Fetch models per allowed make with `SemaphoreSlim(10)` limiting concurrency.
7. Upsert models by `VpicId`:
   - Existing `VpicId` match → update `Name`, `VpicName`, `SyncedAt`, `MakeId`
   - No match → insert new with a new GUID, linked `MakeId` from the upserted make
8. `SaveChangesAsync` after processing each make's models (per-make save) to avoid holding 5,000+ tracked entities in memory simultaneously. Makes are saved together in one pass before model fetching begins.
9. Catch all exceptions, log, and return without re-throwing (app starts with stale data).

### `VehicleDataSyncService` (Server layer)
Located at: `Server/Services/VehicleDataSyncService.cs`

`BackgroundService` that delays 24 hours then re-invokes `IVehicleDataInitializer.RunAsync`. Identical structure to `MunicipalitySyncService`. The initializer's 30-day staleness check ensures no unnecessary API calls.

### `Program.cs` changes
- Call `vehicleDataInitializer.RunAsync(app.Lifetime.ApplicationStopping)` after `municipalityInitializer.RunAsync(...)` in the startup scope.
- Register `VehicleDataSyncService` as a hosted service alongside `MunicipalitySyncService`.

### `ServiceExtensions.cs` changes
- Register `AddHttpClient<IVehicleDataApiClient, VpicVehicleDataApiClient>()`
- Register `AddScoped<IVehicleDataInitializer, VehicleDataInitializer>()`

---

## MakeExclusion Seeding

A `MakeExclusionSeeder` implements `IDevelopmentSeeder` but is registered unconditionally (not just in dev) because exclusions are required in production. It skips if any exclusions already exist.

The initial exclusion list is determined by dispatching sub-agents with batches of the 193 vPIC makes. Sub-agents identify brands that are not sold in European markets or are obsolete US-only marques (e.g., Hudson, Kaiser-Frazer, Saleen, Panoz, SsangYong's US sub-brands, etc.). The resulting `VpicId` set is hardcoded in the seeder.

**Registration**: `MakeExclusionSeeder` must run in all environments (not just dev). It is called directly in the `Program.cs` startup scope — outside the `if (app.Environment.IsDevelopment())` block — as a one-shot call: `await scope.ServiceProvider.GetRequiredService<MakeExclusionSeeder>().SeedAsync(...)`. Registered as `AddScoped<MakeExclusionSeeder>()` (not via `IDevelopmentSeeder`).

---

## Dev Seeder Changes

**Removed**: `MakeSeeder`, `ModelSeeder`, and their `AddScoped<IDevelopmentSeeder, ...>` registrations in `ServiceExtensions.cs`.

**`VariantSeeder` update**: Currently creates 3 variants per every model in the DB. With ~5,000+ vPIC models this would create an unmanageable number of dev records. Updated to take the **first 20 models alphabetically** and create 3 variants each → 60 variants → ~60 dev listings. Keeps dev startup fast.

**`ListingSeeder`**: No changes needed.

**`MakeBuilder` / `ModelBuilder`**: Kept as-is. Tests use these builders to create entities directly (without `VpicId`) for handler integration tests that don't involve sync logic.

---

## Controller

No new controller needed. Existing `MakeController` and `ModelController` serve makes/models as before. The sync is purely a data population mechanism. Existing `GET /api/makes` and `GET /api/models?makeId=...` endpoints are unchanged.

---

## Testing

Located at: `Tests/Features/HandlerTests/VehicleDataHandlerTests/VehicleDataInitializerTests.cs`

All tests use `DatabaseFixture<VehicleDataInitializerTests>`, mock `IVehicleDataApiClient` with NSubstitute, and reset DB between tests.

| Test | Scenario |
|---|---|
| `RunAsync_EmptyTable_ShouldFetchAndInsertMakesAndModels` | Happy path: empty DB, inserts makes and models |
| `RunAsync_FreshData_ShouldSkipApiCall` | SyncedAt within 30 days → API never called |
| `RunAsync_StaleData_ShouldUpdateExistingMakes` | SyncedAt > 30 days → names updated, SyncedAt refreshed |
| `RunAsync_ExcludedMake_ShouldNotBeUpserted` | Make with excluded VpicId → not in DB after sync |
| `RunAsync_ApiFailure_ShouldLogAndNotThrow` | API throws → exception caught, no re-throw |

Existing `GetAllMakesQueryHandlerTests` and model handler tests need no changes.

---

## File Map

### New files
| File | Purpose |
|---|---|
| `Domain/Entities/MakeExclusion.cs` | MakeExclusion entity |
| `Application/Interfaces/Services/IVehicleDataApiClient.cs` | API client interface + DTOs |
| `Infrastructure/Interfaces/IVehicleDataInitializer.cs` | Initializer interface |
| `Infrastructure/Services/VpicVehicleDataApiClient.cs` | HTTP client impl |
| `Infrastructure/Sync/VehicleDataInitializer.cs` | Startup sync logic |
| `Infrastructure/Data/Configuration/MakeExclusionConfiguration.cs` | EF type config |
| `Infrastructure/Data/Seeders/MakeExclusionSeeder.cs` | Seeds excluded makes |
| `Server/Services/VehicleDataSyncService.cs` | Background sync service |
| `Tests/Features/HandlerTests/VehicleDataHandlerTests/VehicleDataInitializerTests.cs` | Initializer tests |

### Modified files
| File | Change |
|---|---|
| `Domain/Entities/Make.cs` | Add `VpicId`, `VpicName`, `SyncedAt` |
| `Domain/Entities/Model.cs` | Add `VpicId`, `VpicName`, `SyncedAt` |
| `Infrastructure/Data/Configuration/MakeConfiguration.cs` | Add unique index on `VpicId` (create new file if none exists) |
| `Infrastructure/Data/Configuration/ModelConfiguration.cs` | Add unique index on `VpicId` |
| `Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` | Add `DbSet<MakeExclusion>` |
| `Infrastructure/ServiceExtensions.cs` | Register HTTP client + initializer; remove MakeSeeder/ModelSeeder |
| `Infrastructure/Data/Seeders/VariantSeeder.cs` | Limit to first 20 models |
| `Server/Program.cs` | Call `VehicleDataInitializer.RunAsync`, register `VehicleDataSyncService` |
| Migration (new file) | Add columns to Makes/Models, create MakeExclusions table |
