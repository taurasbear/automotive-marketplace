# Listing & Variant Fixes + CreateListing Redesign

**Date:** 2026-04-21  
**Status:** Approved

## Problem

Several bugs introduced during the variant i18n and admin UI implementation work:

1. **Frontend type mismatches** — `GetAllListingsResponse` and `GetListingByIdResponse` types use stale field names (`make`, `model`, `fuelType`, `transmission`, `bodyType`, `drivetrain`, `seller`) that don't match the camelCased backend JSON (`makeName`, `modelName`, `fuelName`, `transmissionName`, `bodyTypeName`, `drivetrainName`, `sellerName`). Fields render as `undefined` at runtime.

2. **Missing fields in backend responses** — `GetListingByIdResponse` does not include `Colour`, `Vin`, or `IsSteeringWheelRight` (they exist on the `Listing` entity but were never mapped). `GetAllListingsResponse` is missing `PowerKw` and `EngineSizeMl` even though the listing card tries to display them.

3. **Poor CreateListing UX** — the variant picker is a compact dropdown crammed with info; once selected it cannot be deselected; the spec fields disappear instead of locking; the grid layout is uneven; images have no preview.

4. **Year in the wrong place** — `Year` lives on `Variant`, which means all listings sharing a variant are forced to have the same year. It should live on `Listing`.

## Approach

Fix the data model first (Year migration), then fix API response types in sync (backend + frontend), then redesign the CreateListing form.

---

## 1. Data Model: Move Year from Variant to Listing

### Domain

- Remove `Year` from `Variant` entity.
- Add `Year` to `Listing` entity.

### Migration

New EF Core migration (`MoveYearFromVariantToListing`):

1. Add nullable `Year` column to `Listings`.
2. Data migration: set each listing's `Year` from its current variant's `Year` via a raw SQL `UPDATE … JOIN`.
3. Make `Year` non-nullable on `Listings`.
4. Drop `Year` column from `Variants`.

### Admin variant feature updates (year removed everywhere)

| File | Change |
|------|--------|
| `CreateVariantCommand` | Remove `Year` parameter |
| `CreateVariantCommandHandler` | Remove `Year` from new `Variant` construction |
| `UpdateVariantCommand` | Remove `Year` parameter |
| `UpdateVariantCommandHandler` | Remove `Year` from entity update |
| `GetVariantsByModelResponse` | Remove `Year` property |
| `GetVariantsByModelQueryHandler` | Remove `.OrderBy(v => v.Year)` (order by `Id` or just unordered) |
| Variant mappings | Remove year mapping |

### Listing feature updates

| File | Change |
|------|--------|
| `CreateListingCommand` | `Year` stays (already present) — applied to `Listing`, not `Variant` |
| `CreateListingCommandHandler` | Set `listing.Year = request.Year`; remove `Year` from all variant create/dedup paths |
| Variant dedup logic | Remove `Year` from `FirstOrDefaultAsync` match criteria |
| `GetAllListingsQueryHandler` | Year filters (`MinYear`/`MaxYear`) now compare against `listing.Year`; remove `Include(l => l.Variant)…Year` traversal |
| `GetListingByIdQueryHandler` | Year now from `listing.Year` (no change needed in includes) |
| `ListingMappings` | `Year` maps from `src.Year` on `Listing` directly for both read responses; add `Year` to the `UpdateListingCommand → Listing` map (currently omitted) |

---

## 2. Backend: Add Missing Response Fields

### `GetListingByIdResponse`

Add:
- `string? Colour`
- `string? Vin`
- `bool IsSteeringWheelRight`

**Mapping** (`ListingMappings.cs`): map these three directly from `src.Colour`, `src.Vin`, `src.IsSteeringWheelRight`.

### `GetAllListingsResponse`

Add:
- `int PowerKw`
- `int EngineSizeMl`

**Mapping**: map from `src.Variant.PowerKw` and `src.Variant.EngineSizeMl` (same pattern as existing FuelName etc.).

---

## 3. Frontend: Fix Type Mismatches

### `GetAllListingsResponse` (frontend type)

| Current | Corrected |
|---------|-----------|
| `year: string` | `year: number` |
| `make: string` | `makeName: string` |
| `model: string` | `modelName: string` |
| `fuelType: string` | `fuelName: string` |
| `transmission: string` | `transmissionName: string` |
| *(missing)* | `powerKw: number` |
| *(missing)* | `engineSizeMl: number` |

### `GetListingByIdResponse` (frontend type)

| Current | Corrected |
|---------|-----------|
| `make: string` | `makeName: string` |
| `model: string` | `modelName: string` |
| `fuel: string` | `fuelName: string` |
| `transmission: string` | `transmissionName: string` |
| `bodyType: string` | `bodyTypeName: string` |
| `drivetrain: string` | `drivetrainName: string` |
| `seller: string` | `sellerName: string` |
| *(missing)* | `colour?: string` |
| *(missing)* | `vin?: string` |
| *(missing)* | `isSteeringWheelRight: boolean` |

### `Variant` type (frontend)

Remove `year: number`.

### Component field reference updates

| Component | Fields to fix |
|-----------|---------------|
| `ListingCard.tsx` | `listing.make` → `.makeName`, `listing.model` → `.modelName`, `listing.fuelType` → `.fuelName`, `listing.transmission` → `.transmissionName` |
| `ListingDetailsContent.tsx` | Same renames plus `listing.fuel` → `.fuelName`, `listing.bodyType` → `.bodyTypeName`, `listing.drivetrain` → `.drivetrainName`, `listing.seller` → `.sellerName` |
| `EditListingForm.tsx` | `listing.colour`, `listing.vin`, `listing.isSteeringWheelRight` are now populated correctly (backend now returns them) — no JSX changes needed |

---

## 4. Frontend: Variant Admin Form (year removed)

Remove the `year` field from:
- `variantFormSchema` — delete year validation
- `VariantFormData` — inferred from schema, automatic
- `VariantForm.tsx` — remove the Year `FormField`

Update `CreateVariantCommand.ts` and `UpdateVariantCommand.ts` (frontend API types) to remove `year`.

---

## 5. Frontend: CreateListing Form Redesign

### 5a. VariantTable component

New component `src/components/forms/VariantTable.tsx`.

**Props:**
```ts
type VariantTableProps = {
  modelId: string;
  selectedVariantId: string;
  onSelect: (variant: Variant | null) => void;
  disabled?: boolean;
};
```

**Behaviour:**
- Fetches variants via `getVariantsByModelIdOptions(modelId)`.
- Renders a full-width `<table>` with columns: Fuel, Transmission, Power (kW), Engine (ml), Body Type, Doors.
- Clicking a non-selected row selects it and calls `onSelect(variant)`.
- Clicking the already-selected row deselects it and calls `onSelect(null)`.
- Shows an empty state row when no variants are available.
- Disabled/hidden when `modelId` is empty.

### 5b. CreateListingForm state logic

**Fields always visible and always editable:**
- Make, Model, Year, Drivetrain, Price, Mileage, City, Colour, VIN, Description, Images, Steering wheel, Used car

**Spec fields** (Fuel, Transmission, Body Type, Power, Engine size, Doors):
- Always rendered (never conditionally hidden).
- **Locked** (read-only inputs/selects) when `variantId` is set and `isCustom` is `false`.
- **Editable** when no variant is selected OR `isModified` is `true`.

**"Car is modified" checkbox:**
- Visible only when a variant is selected (`variantId !== ""`).
- When checked: sets `isCustom = true`, clears `variantId` from submission, keeps spec field values editable.
- When unchecked: restores `isCustom = false` and `variantId`, re-locks spec fields.

**Variant selection flow (`handleVariantSelect(variant: Variant | null)`):**
- If `variant` is non-null:  
  - `form.setValue("variantId", variant.id)`  
  - `form.setValue("isCustom", false)`  
  - `form.setValue("fuelId", variant.fuelId)`  
  - `form.setValue("transmissionId", variant.transmissionId)`  
  - `form.setValue("bodyTypeId", variant.bodyTypeId)`  
  - `form.setValue("doorCount", variant.doorCount)`  
  - `form.setValue("powerKw", variant.powerKw)`  
  - `form.setValue("engineSizeMl", variant.engineSizeMl)`  
  - Reset `isModified` to `false`
- If `variant` is null:  
  - `form.setValue("variantId", "")`  
  - `form.setValue("isCustom", true)`  
  - Reset `isModified` to `false`

**`isModified` state:** local `useState<boolean>` (not part of the form schema). When `true`, spec fields are editable and `variantId` is excluded from the submit payload.

**Grid layout** — consistent 3-column (`md:grid-cols-3`) approach:
- Row 1: Make, Model, Year (3 cols)
- Row 2: VariantTable (full width, `col-span-3`)
- Row 3: Spec group (full-width bordered container with lock indicator and "Car is modified" checkbox header; 3-col grid inside)
- Row 4: Drivetrain (full width), Price, Mileage, City (3 cols)
- Row 5: Colour, VIN (2 cols)
- Row 6: Description (2 cols), Images (1 col)
- Row 7: Steering wheel checkbox, Used car checkbox, Submit button

### 5c. ImageUploadInput & ImagePreview

**`ImageUploadInput` changes:**
- Append newly selected files to existing `field.value` instead of replacing them.

**New `ImagePreview` component** (`src/features/createListing/components/ImagePreview.tsx`):

```ts
type ImagePreviewProps = {
  images: File[];
  onRemove: (index: number) => void;
};
```

- Renders a flex-wrap row of 56×56 px thumbnails using `URL.createObjectURL`.
- Each thumbnail has an `×` button (absolute-positioned, top-right) to remove that image.
- Thumbnails are revoked on unmount to avoid memory leaks (`useEffect` cleanup with `URL.revokeObjectURL`).
- Rendered directly below the `ImageUploadInput` within the `FormField`.

---

## 6. Checklist of All Files Changed

### Backend
- `Automotive.Marketplace.Domain/Entities/Variant.cs` — remove `Year`
- `Automotive.Marketplace.Domain/Entities/Listing.cs` — add `Year`
- New migration file
- `Automotive.Marketplace.Application/Features/VariantFeatures/CreateVariant/CreateVariantCommand.cs`
- `Automotive.Marketplace.Application/Features/VariantFeatures/CreateVariant/CreateVariantCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommand.cs`
- `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelResponse.cs`
- `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/CreateListing/CreateListingCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`
- `Automotive.Marketplace.Application/Mappings/ListingMappings.cs`
- Variant mappings (`Automotive.Marketplace.Application/Mappings/VariantMappings.cs`) — remove `Year` from `GetVariantsByModelResponse` mapping (auto-mapped by name convention; removing the property from both sides is sufficient)

### Frontend
- `src/features/variantList/types/Variant.ts` — remove `year`
- `src/features/variantList/schemas/variantFormSchema.ts` — remove `year`
- `src/features/variantList/components/VariantForm.tsx` — remove year field
- `src/features/variantList/api/useCreateVariant.ts` — remove year from command
- `src/features/variantList/api/useUpdateVariant.ts` — remove year from command
- `src/features/variantList/types/CreateVariantCommand.ts` — remove `year`
- `src/features/variantList/types/UpdateVariantCommand.ts` — remove `year`
- `src/features/listingList/types/GetAllListingsResponse.ts` — field renames + add powerKw/engineSizeMl
- `src/features/listingList/components/ListingCard.tsx` — update field references
- `src/features/listingDetails/types/GetListingByIdResponse.ts` — field renames + add colour/vin/isSteeringWheelRight
- `src/features/listingDetails/components/ListingDetailsContent.tsx` — update field references
- `src/components/forms/select/VariantSelect.tsx` — delete (replaced by VariantTable)
- `src/components/forms/VariantTable.tsx` — new file
- `src/features/createListing/components/CreateListingForm.tsx` — full redesign
- `src/features/createListing/components/ImageUploadInput.tsx` — append mode
- `src/features/createListing/components/ImagePreview.tsx` — new file

---

## Out of Scope

- EditListing form restructure (only type fixes flow through automatically)
- SavedListings page (types already correct: `fuelName`, `transmissionName`)
- Search/filter components (year filter on listings stays, just queries `listing.Year` now on backend)
- Any external data source for variants
