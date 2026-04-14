# Variant + i18n Infrastructure Design

## Problem & Approach

The codebase is transitioning from a `Car` entity (per-listing vehicle instance) to a `Variant` entity (shared vehicle template). Several enum-typed vehicle attributes (`Fuel`, `Transmission`, `BodyType`, `Drivetrain`) are being promoted to first-class DB entities to support future i18n (English + Lithuanian). Translation tables are added for each lookup type. The full stack — domain, infrastructure, application, tests, and frontend — needs to be updated to reflect these changes.

---

## 1. Domain Model

### `Variant` entity
- `IsSelectable` renamed to `IsCustom` (bool, default `false`). `false` = standard factory variant (shown in dropdowns). `true` = custom/modified variant (hidden from dropdowns).
- `MinYear` and `MaxYear` changed from `DateTime` to `int` (production year range).
- FKs: `ModelId`, `FuelId`, `TransmissionId`, `BodyTypeId`.
- Fields: `PowerKw` (int), `EngineSizeMl` (int), `DoorCount` (int).
- Nav: `virtual ICollection<Listing> Listings`.

### `Listing` entity
- `Year` (int) re-added — seller's specific model year.
- `Drivetrain` enum property removed; replaced with `Guid DrivetrainId` + `virtual Drivetrain Drivetrain`.
- `VariantId` FK (replacing `CarId`).

### New entity: `Drivetrain` (in `Domain/Entities/`)
```csharp
public class Drivetrain : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<DrivetrainTranslation> Translations { get; set; } = [];
}
```

### 4 translation entities (one per lookup table)
Each follows the same pattern:

```csharp
public class FuelTranslation : BaseEntity {
    public Guid FuelId { get; set; }
    public string LanguageCode { get; set; } = string.Empty; // "en", "lt"
    public string Name { get; set; } = string.Empty;
    public virtual Fuel Fuel { get; set; } = null!;
}
```

Entities: `FuelTranslation`, `TransmissionTranslation`, `BodyTypeTranslation`, `DrivetrainTranslation`.

Each corresponding lookup entity gains: `public virtual ICollection<{Type}Translation> Translations { get; set; } = [];`

The existing `Name` property on each lookup entity is the canonical English name (kept for backward compatibility). Translation records supplement with additional languages. The API currently returns `Name` directly; when full i18n is needed, callers will query the translation table by `LanguageCode`.

### Removed
- `Car.cs` entity (deleted).
- `Drivetrain` C# enum (`Domain/Enums/Drivetrain.cs` deleted — was already removed for the other 3 types).

---

## 2. Infrastructure

### EF Configurations
- Remove `CarConfiguration.cs`.
- Add `VariantConfiguration`: FK to Model, Fuel, Transmission, BodyType.
- Add `DrivetrainConfiguration`: FK from Listing.
- Add translation configurations for all 4 types with composite unique index on `({EntityId}, LanguageCode)`.
- Update `ListingConfiguration`: add `DrivetrainId` FK, add `Year`, remove Drivetrain enum string conversion.

### `AutomotiveContext`
- Replace `DbSet<Car> Cars` with `DbSet<Variant> Variants`.
- Add `DbSet<Drivetrain> Drivetrains`.
- Add `DbSet<FuelTranslation>`, `DbSet<TransmissionTranslation>`, `DbSet<BodyTypeTranslation>`, `DbSet<DrivetrainTranslation>`.

### Seeders (seeding reference data in source code is conventional for stable lookup tables)
- `FuelSeeder`: seeds Fuel records with `"en"` and `"lt"` translations.
- `TransmissionSeeder`: same pattern.
- `BodyTypeSeeder`: same pattern.
- `DrivetrainSeeder`: same pattern (FWD, RWD, AWD with Lithuanian display names).
- `VariantSeeder` (replaces `CarSeeder`): uses Bogus to generate variants per model, referencing seeded Fuel/Transmission/BodyType/DoorCount IDs.
- `ListingSeeder`: updated to use `VariantId` + `DrivetrainId` + `Year`.

### Builders
- `CarBuilder` removed; new `VariantBuilder` takes its place.
- `ListingBuilder` updated: `WithCar(Guid)` → `WithVariant(Guid)`, add `WithDrivetrain(Guid)`, add `WithYear(int)`.

---

## 3. Application Layer

### Enum Features (GetBodyTypes / GetFuelTypes / GetTransmissionTypes / GetDrivetrainTypes)
- Handlers query from DB (`repository.AsQueryable<Fuel>()` etc.) instead of `Enum.GetValues()`.
- Responses include `Id` (Guid) and `Name` (string) so the frontend can pass `Id` back in commands.

### Car Features → Variant Features
- `CarFeatures/` folder converted to `VariantFeatures/`: CreateVariant, DeleteVariant, GetAllVariants, GetVariantById, UpdateVariant.
- GetVariantsByModel query added for the listing creation dropdown (returns variants where `IsCustom == false`, filtered by ModelId + Year — i.e. where `MinYear <= Year <= MaxYear`).

### `CreateListingCommand`
- `VariantId` (Guid?) — optional.
- If `VariantId` is null, the following are required: `ModelId`, `FuelId`, `TransmissionId`, `BodyTypeId`, `DoorCount`, `PowerKw`, `EngineSizeMl`, `MinYear`, `MaxYear`, `IsCustom` (bool, default false).
- Always required: `DrivetrainId` (Guid), `Year` (int), plus all listing fields (Price, Description, Colour, Vin, Mileage, IsSteeringWheelRight, City, IsUsed, UserId, Images).

### `CreateListingCommandHandler` — find-or-create logic
1. If `VariantId` provided → use it directly.
2. If `VariantId` null:
   - If `IsCustom == true` → always create new `Variant` (no lookup).
   - If `IsCustom == false` → search for exact match on (ModelId, FuelId, TransmissionId, BodyTypeId, DoorCount, PowerKw, EngineSizeMl, MinYear, MaxYear). Use if found; create if not.

### Mappings
- `CarMappings` → `VariantMappings`.
- `ListingMappings` updated: all `src.Car.*` references updated to `src.Variant.*`, `src.Year` used for listing year.
- `EnumMappings` updated to map from entities (not C# enums).

### Filters in `GetAllListingsQueryHandler`
- `MinYear` / `MaxYear` → filter on `listing.Year`.
- `MinPower` / `MaxPower` → filter on `listing.Variant.PowerKw`.
- Make/Model filters → `listing.Variant.Model.*`.

### Responses
- `GetAllListingsResponse` / `GetListingByIdResponse`: `Power` → `PowerKw`, `EngineSize` → `EngineSizeMl`, use `listing.Year` for year.

---

## 4. Tests

- `CarBuilder` references replaced with `VariantBuilder`.
- `CreateListingCommandHandlerTests`: updated to new command shape (both VariantId path and full-spec find-or-create path tested).
- `GetAllListingsQueryHandlerTests`: seeding updated to use VariantBuilder + DrivetrainBuilder; Year filter tests use `listing.Year`.
- `DatabaseFixture`: no structural change beyond context DbSets reflecting new entities.

---

## 5. Frontend

- `GetBodyTypesResponse`, `GetFuelTypesResponse`, `GetTransmissionTypesResponse`, `GetDrivetrainTypesResponse`: add `id` field (Guid string).
- Add `GetVariantsResponse` type for variant dropdown.
- Car-related types updated or removed (CarFormData, GetAllCarsResponse, GetCarByIdResponse → Variant equivalents).
- `CreateListing.tsx` form: updated to new command shape (VariantId selector + fallback full-spec fields).

---

## Out of Scope

- Full UI language switching (language selector, translated display in components).
- Admin UI for managing variants/lookup tables.
- API endpoint for reading translations by language code (translations are in DB, ready for future use).
