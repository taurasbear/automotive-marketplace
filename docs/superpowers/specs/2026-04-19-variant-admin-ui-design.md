# Variant Admin UI Design

## Problem & Approach

The `carList` feature folder was never replaced with a working `variantList` admin UI. The `Variants.tsx` page and `variantList` feature exist but only show a read-only table filtered by Make→Model. Admins have no way to create, edit, or delete variants. This spec covers the frontend-only work to complete that gap.

---

## 1. Shared Select Components — Entity ID Mode

`FuelSelect`, `TransmissionToggleGroup`, `BodyTypeSelect`, and `DrivetrainToggleGroup` currently call the legacy `GET /Enum/Get*Types` endpoints and return string enum values (e.g. `"Petrol"`, `"FWD"`). The backend now expects **GUIDs** for `fuelId`, `transmissionId`, `bodyTypeId`, `drivetrainId` in both `CreateVariantCommand` and `CreateListingCommand`.

Four options files were pre-created with TODO comments for exactly this change:
- `getAllFuelsOptions.ts` → `GET /Fuel/GetAll` → `{ id, name, translations }[]`
- `getAllTransmissionsOptions.ts` → `GET /Transmission/GetAll`
- `getAllBodyTypesOptions.ts` → `GET /BodyType/GetAll`
- `getAllDrivetrainsOptions.ts` → `GET /Drivetrain/GetAll`

**Change:** Switch all four shared components to use the entity-based options. Each component renders `entity.id` as the select value and `entity.name` as the label. This also fixes the pre-existing bug in `CreateListingForm` where those fields received string enum values instead of GUIDs.

Filter components (`BasicFilters`, `RangeFilters`, etc.) do **not** use these four components — no filter breakage.

---

## 2. `variantList` Feature — Full CRUD

### Types

| File | Contents |
|------|----------|
| `CreateVariantCommand` | `modelId, year, fuelId, transmissionId, bodyTypeId, isCustom, doorCount, powerKw, engineSizeMl` (all strings/numbers) |
| `UpdateVariantCommand` | Same fields + `id` |
| `DeleteVariantCommand` | `id: string` |
| `VariantFormData` | Inferred from `variantFormSchema` (includes `makeId` as cascade-only UI field) |

### Schema (`variantFormSchema`)

Validates:
- `makeId` — GUID regex
- `modelId` — GUID regex
- `year` — coerce int, min 1900, max current year
- `fuelId`, `transmissionId`, `bodyTypeId` — GUID regex
- `doorCount` — coerce int, min 1, max 9
- `powerKw` — coerce int, min 5, max 1000
- `engineSizeMl` — coerce int, min 300, max 10000
- `isCustom` — boolean

### API Mutations

- `useCreateVariant` — `POST /Variant/Create`, invalidates `variantKeys.all()`
- `useUpdateVariant` — `PUT /Variant/Update/{id}`, invalidates `variantKeys.all()`
- `useDeleteVariant` — `DELETE /Variant/Delete/{id}`, invalidates `variantKeys.all()`

### Components

**`VariantForm`**
- Grid form (4-col layout matching existing admin forms)
- Fields: Make→Model cascade selectors, Year, FuelSelect, TransmissionToggleGroup, BodyTypeSelect, DoorCount, PowerKw, EngineSizeMl, IsCustom checkbox
- `makeId` drives `ModelSelect` but is stripped before submit

**`CreateVariantDialog`**
- Props: `modelId: string`, `makeId: string`
- Pre-populates `modelId` and `makeId` in the form default values
- On submit: calls `useCreateVariant`, closes dialog

**`EditVariantDialog` + `EditVariantDialogContent`**
- Props: `variant: Variant`, `makeId: string`
- No re-fetch needed — passes variant data directly to the form
- On submit: calls `useUpdateVariant` with variant id

**`ViewVariantDialog` + `ViewVariantDialogContent`**
- Props: `variant: Variant`
- Read-only display of all variant fields

### Updated `VariantListTable`

- New props: `makeId: string` (passed down from `Variants.tsx`)
- Each row gets three action buttons: View, Edit (pencil), Delete (trash)
- Delete calls `useDeleteVariant` directly from the table

### Updated `Variants.tsx`

- `CreateVariantDialog` button visible when `selectedModel` is set
- Passes `selectedMake` to `VariantListTable`

### Updated `variantList/index.ts`

Exports `VariantListTable`, `CreateVariantDialog`.

---

## 3. `carList` Feature — Deletion

The entire `src/features/carList/` folder is dead code (backend Car API was replaced by Variant). It is deleted. No page, route, or component references it.

---

## Out of Scope

- Adding a `GET /Variant/GetById` backend endpoint (not needed — all fields are in the model-scoped list response)
- Adding a `GET /Variant/GetAll` backend endpoint (admin UI is intentionally model-scoped)
- Any changes to `CreateListingForm` beyond the select component fix
