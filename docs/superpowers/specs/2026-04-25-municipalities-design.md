# Municipalities Feature Design

## Problem

Listing locations are currently a free-text `City` string seeded with random Bogus English city names. The `LocationCombobox` component in the search UI hardcodes three Lithuanian city values. There is no canonical list of valid locations and no way to keep it up to date.

## Proposed Approach

Introduce a `Municipality` entity backed by Lithuania's open government data API (`https://get.data.gov.lt/datasets/gov/rc/ar/grasavivaldybe/GraSavivaldybe/:format/json`). A sync service keeps the 60 municipalities current. `Listing.City` becomes a FK to `Municipality`. The frontend populates the location dropdown from the backend.

---

## 1. Domain & Data Model

### New entity: `Municipality`

Does **not** inherit `BaseEntity` (the audit fields `CreatedBy`, `ModifiedBy`, etc. are meaningless for external reference data).

| Property   | Type       | Notes                                          |
|------------|------------|------------------------------------------------|
| `Id`       | `Guid`     | From API `_id` — used as PK, stable across syncs |
| `Name`     | `string`   | From `pavadinimas` (e.g. `"Vilniaus m."`)      |
| `SyncedAt` | `DateTime` | UTC — set on every sync; used to detect staleness |

### `Listing` changes

- Remove `City string` property.
- Add `MunicipalityId Guid` (FK → `Municipality`).
- Add `Municipality` navigation property.
- EF delete behaviour: `Restrict` (a municipality cannot be deleted while listings reference it).

### Migration

A new EF Core migration drops `City`, adds `MunicipalityId`, adds the FK, and creates the `Municipalities` table.

---

## 2. Sync Service

### `IMunicipalityApiClient` (Application layer interface)

```csharp
public interface IMunicipalityApiClient
{
    Task<IEnumerable<MunicipalityDto>> FetchMunicipalitiesAsync(CancellationToken ct);
}
```

`MunicipalityDto` carries `Id` (Guid) and `Name` (string).

### `LithuanianMunicipalityApiClient` (Infrastructure)

- Registered via `IHttpClientFactory` with a named client.
- Calls the gov API, deserialises `_id` → `Id` and `pavadinimas` → `Name`.
- Returns all 60 records.

### `MunicipalityInitializer` (Infrastructure / startup)

Runs **eagerly in the startup scope** (after migrations, before dev seeders) so that municipalities are available when `ListingSeeder` runs.

Logic:
- If `municipalities.Count == 0` OR `municipalities.Min(m => m.SyncedAt) < UtcNow - 30 days` → fetch and upsert.
- Otherwise skip (fast path: a single DB count + min-date query).

Upsert semantics: insert new records; update `Name` and `SyncedAt` for existing ones. Never delete (prevents breaking listing FKs if a municipality disappears from the API).

### `MunicipalitySyncService` (BackgroundService in Server)

Handles **recurring monthly refreshes** in a running application.

- On each iteration: checks the same stale condition as above → syncs if needed.
- Sleeps 24 hours between checks (cheap: a count + min-date query per day).
- Survives server restarts — staleness is determined from DB, not in-memory timers.
- Errors are caught and logged; the service continues on failure.

---

## 3. API Endpoint

**`GET /api/municipalities`** — unauthenticated (location data is public reference data).

Response: `IEnumerable<GetAllMunicipalitiesResponse>` ordered alphabetically by `Name`.

```json
[
  { "id": "3b2317a2-...", "name": "Alytaus m." },
  ...
]
```

CQRS: `GetAllMunicipalitiesQuery` → `GetAllMunicipalitiesQueryHandler` (Application layer).

`MunicipalityController` in Server follows the same slim pattern as `FuelController`.

---

## 4. Frontend

### `useGetMunicipalities` query hook

- Location: `src/features/listingList/api/useGetMunicipalities.ts` (or shared `src/api/`).
- TanStack Query with `staleTime: Infinity` — municipality data is refreshed by the backend; no need for the client to re-fetch.
- Calls `GET /api/municipalities`.

### `LocationCombobox` update

- Remove the 3 hardcoded locations.
- Use `useGetMunicipalities` data instead.
- Show a loading state while fetching.
- Item `value` is municipality `id` (Guid string) rather than a lowercased slug.

### Filter state & query changes

- Wherever the location filter is stored (listing filter state), the value type changes from a hardcoded slug string to a municipality `id` (Guid string).
- The `UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE` sentinel (`"anyLocation"`) is unchanged for the "no filter" state.
- `GetAllListingsQuery.City` (string) → `GetAllListingsQuery.MunicipalityId` (Guid?).
- `GetAllListingsResponse.City` (string) → `GetAllListingsResponse.MunicipalityName` (string) — the handler projects `listing.Municipality.Name`.
- `GetAllListingsQueryHandler` filter clause changes from `listing.City.ToLower() == request.City.ToLower()` to `listing.MunicipalityId == request.MunicipalityId`.
- `SearchListingsResponse.City` stays as a string but is now populated from `listing.Municipality.Name`.

---

## 5. Startup Order

```
app.Run() startup scope:
  1. automotiveContext.Database.MigrateAsync()          ← existing
  2. municipalityInitializer.RunAsync()                 ← new (always; fast if already synced)
  3. foreach IDevelopmentSeeder (dev only)              ← existing, ListingSeeder updated
```

`MunicipalitySyncService` (BackgroundService) runs after `app.Run()` for recurring 24-hour checks.

### `ListingSeeder` update

Loads municipalities from DB, picks a random `MunicipalityId` for each seeded listing.

---

## 6. Error Handling

- If the gov API is unavailable at startup and the municipality table is empty, `MunicipalityInitializer` logs the error and continues — the app starts but the location dropdown will be empty until a successful sync.
- The `BackgroundService` catches all exceptions per iteration, logs them, and retries on the next 24-hour cycle.
- The `GET /municipalities` endpoint returns an empty array if the table is empty (no 500).

---

## 7. Testing

- Integration test for `GetAllMunicipalitiesQueryHandler` — seed a few municipalities, verify response shape and ordering.
- Integration test for `MunicipalityInitializer` — mock `IMunicipalityApiClient`; verify upsert on empty DB and skip when data is fresh.
- No frontend tests (per project conventions).
