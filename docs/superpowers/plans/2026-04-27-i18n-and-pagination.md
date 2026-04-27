# i18n Completeness + Pagination Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** (1) Translate vehicle attribute names (fuel, transmission, drivetrain, body type, listing status) in the frontend using i18n instead of raw English backend values; (2) add server-side pagination to the listing search.

**Architecture:** Vehicle attributes are stored in English in the backend and returned as English strings. The FE will maintain a lookup map in the i18n JSON files. A utility function `translateVehicleAttr(key, t)` maps backend values to i18n keys. Pagination is added with a new `page`/`pageSize` query param pair backed by a `GetAllListingsWrapper` response with `total` for FE to compute page count.

**Tech Stack:** React 19 + TypeScript, i18next, TanStack Query, ASP.NET Core 8, Entity Framework Core.

---

### Task 1: Add Vehicle Attribute Translations to i18n Files

**Files:**
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/listings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/listings.json`

**Background:** Fuel, transmission, drivetrain, and body type values come back from the backend as English strings (e.g. `"Diesel"`, `"Automatic"`, `"Front-Wheel Drive"`, `"SUV"`). The seeded values in the DB are:

**Fuels (English):** `Diesel`, `Petrol`, `Electric`, `Petrol/LPG`, `Petrol/Electric`, `Plug-in Hybrid`
**Transmissions (English):** `Automatic`, `Manual`
**Drivetrains (English):** `Front-Wheel Drive`, `Rear-Wheel Drive`, `All-Wheel Drive`, `4WD`
**Body types (English):** `Sedan`, `Hatchback`, `SUV`, `Coupe`, `Convertible`, `Minivan`, `Pickup`, `Van`, `Wagon`
**Listing statuses (English):** `Available`, `Sold`, `Reserved`, `Pending`

- [ ] **Step 1: Add to English translations**

First run:
```bash
cat automotive.marketplace.client/src/lib/i18n/locales/en/listings.json | head -30
```
to confirm the existing structure and nesting. Then add a top-level `"vehicleAttributes"` key:

```json
{
  "vehicleAttributes": {
    "fuel": {
      "Diesel": "Diesel",
      "Petrol": "Petrol",
      "Electric": "Electric",
      "Petrol/LPG": "Petrol/LPG",
      "Petrol/Electric": "Petrol/Electric",
      "Plug-in Hybrid": "Plug-in Hybrid"
    },
    "transmission": {
      "Automatic": "Automatic",
      "Manual": "Manual"
    },
    "drivetrain": {
      "Front-Wheel Drive": "Front-Wheel Drive",
      "Rear-Wheel Drive": "Rear-Wheel Drive",
      "All-Wheel Drive": "All-Wheel Drive",
      "4WD": "4WD"
    },
    "bodyType": {
      "Sedan": "Sedan",
      "Hatchback": "Hatchback",
      "SUV": "SUV",
      "Coupe": "Coupe",
      "Convertible": "Convertible",
      "Minivan": "Minivan",
      "Pickup": "Pickup",
      "Van": "Van",
      "Wagon": "Wagon"
    },
    "status": {
      "Available": "Available",
      "Sold": "Sold",
      "Reserved": "Reserved",
      "Pending": "Pending"
    }
  }
}
```

- [ ] **Step 2: Add Lithuanian translations**

```json
{
  "vehicleAttributes": {
    "fuel": {
      "Diesel": "Dyzelinas",
      "Petrol": "Benzinas",
      "Electric": "Elektra",
      "Petrol/LPG": "Benzinas/Dujos",
      "Petrol/Electric": "Benzinas/Elektra",
      "Plug-in Hybrid": "Įkraunamas hibridas"
    },
    "transmission": {
      "Automatic": "Automatinis",
      "Manual": "Mechaninis"
    },
    "drivetrain": {
      "Front-Wheel Drive": "Priekinių ratų pavara",
      "Rear-Wheel Drive": "Galinių ratų pavara",
      "All-Wheel Drive": "Visų ratų pavara",
      "4WD": "4x4"
    },
    "bodyType": {
      "Sedan": "Sedanas",
      "Hatchback": "Hečbekas",
      "SUV": "Visureigis",
      "Coupe": "Kupė",
      "Convertible": "Kabrioletas",
      "Minivan": "Minivenas",
      "Pickup": "Pikapas",
      "Van": "Furgonas",
      "Wagon": "Universalas"
    },
    "status": {
      "Available": "Parduodamas",
      "Sold": "Parduotas",
      "Reserved": "Rezervuotas",
      "Pending": "Laukiamas"
    }
  }
}
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/lib/i18n/locales/
git commit -m "feat: add vehicle attribute translations to i18n files

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Create a translateVehicleAttr Utility

**Files:**
- Create: `automotive.marketplace.client/src/features/listings/utils/translateVehicleAttr.ts`

- [ ] **Step 1: Create utility**

The `t` function from `useTranslation("listings")` with the namespace `listings` will use the keys added in Task 1.

```ts
// translateVehicleAttr.ts
import type { TFunction } from "i18next";

export type VehicleAttrType =
  | "fuel"
  | "transmission"
  | "drivetrain"
  | "bodyType"
  | "status";

export function translateVehicleAttr(
  type: VehicleAttrType,
  value: string,
  t: TFunction<"listings">,
): string {
  const key = `vehicleAttributes.${type}.${value}`;
  const translated = t(key);
  // If i18next key is missing, it returns the key itself — fall back to raw value
  return translated === key ? value : translated;
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listings/utils/translateVehicleAttr.ts
git commit -m "feat: add translateVehicleAttr utility

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Apply Translations in ListingCard and SavedListingRow

**Files:**
- Modify: `automotive.marketplace.client/src/features/listings/components/ListingCard.tsx`
- Modify: `automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx` (or wherever saved listing rows display fuel/transmission)

- [ ] **Step 1: Use translations in ListingCard.tsx**

```tsx
import { useTranslation } from "react-i18next";
import { translateVehicleAttr } from "@/features/listings/utils/translateVehicleAttr";

// Inside the component:
const { t } = useTranslation("listings");

// Replace raw string renders, e.g.:
// Before:
<span>{listing.fuelName}</span>
// After:
<span>{translateVehicleAttr("fuel", listing.fuelName, t)}</span>

// Same for transmissionName, drivetrainName, bodyTypeName
```

- [ ] **Step 2: Apply translations in SavedListingRow.tsx**

Same pattern — import `useTranslation` and `translateVehicleAttr`, then replace raw attribute strings with the utility calls.

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add \
  automotive.marketplace.client/src/features/listings/components/ListingCard.tsx \
  automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx
git commit -m "feat: translate vehicle attributes in listing cards

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Apply Translations in Comparison Table

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.tsx`

- [ ] **Step 1: Use translations in CompareTable.tsx**

```tsx
import { useTranslation } from "react-i18next";
import { translateVehicleAttr } from "@/features/listings/utils/translateVehicleAttr";

// Inside the component:
const { t } = useTranslation("listings");

// Replace raw renders for fuelName, transmissionName, drivetrainName, bodyTypeName:
// Before:
row.value  // where the comparison value is the raw backend string

// After:
type === "fuelName"         ? translateVehicleAttr("fuel",         row.value, t)
: type === "transmissionName" ? translateVehicleAttr("transmission", row.value, t)
: type === "drivetrainName"   ? translateVehicleAttr("drivetrain",   row.value, t)
: type === "bodyTypeName"     ? translateVehicleAttr("bodyType",     row.value, t)
: type === "status"           ? translateVehicleAttr("status",       row.value, t)
: row.value
```

Adapt this pattern to how `CompareTable.tsx` actually renders each row (the exact JSX depends on the current structure — read the file first).

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/CompareTable.tsx
git commit -m "feat: translate vehicle attributes and status in comparison table

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Apply Translations in Listing Details Page

**Files:**
- Modify: whichever component renders `fuelName`, `transmissionName`, `drivetrainName`, `bodyTypeName` on the single-listing details page. Run:

```bash
grep -r "fuelName\|transmissionName\|drivetrainName\|bodyTypeName" automotive.marketplace.client/src/features/listingDetails/
```

to find the file(s). Apply the same `translateVehicleAttr` pattern there.

- [ ] **Step 1: Apply translations**

After identifying the file, apply the same import + utility call pattern as Task 3.

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/
git commit -m "feat: translate vehicle attributes on listing details page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Add Server-Side Pagination to Listing Search

**Context:** `GetAllListingsQuery` currently returns all listings with no paging. Adding pagination requires:
1. A `page`/`pageSize` parameter on the query
2. A wrapper response with `items` + `total`
3. BE EF Core `.Skip().Take()`
4. FE `Pagination` component updating the URL search params

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`
- Modify: `automotive.marketplace.client/src/features/listingList/api/getAllListingsOptions.ts`
- Modify: `automotive.marketplace.client/src/app/pages/Listings.tsx`
- Create: `automotive.marketplace.client/src/components/ui/Pagination.tsx`

- [ ] **Step 1: Update query**

```csharp
// GetAllListingsQuery.cs
public class GetAllListingsQuery : IRequest<PagedResult<GetAllListingsResponse>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    // ... existing filter properties unchanged ...
}
```

- [ ] **Step 2: Add PagedResult wrapper**

```csharp
// Create Automotive.Marketplace.Application/Common/Models/PagedResult.cs
namespace Automotive.Marketplace.Application.Common.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
```

- [ ] **Step 3: Update handler to apply Skip/Take**

```csharp
// GetAllListingsQueryHandler.cs — add after all filters are applied, before .ToListAsync():
var total = await query.CountAsync(cancellationToken);

var items = await query
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync(cancellationToken);

return new PagedResult<GetAllListingsResponse>
{
    Items = items,
    Total = total,
    Page = request.Page,
    PageSize = request.PageSize,
};
```

- [ ] **Step 4: Update controller endpoint to pass page params from query string**

```csharp
// In ListingController.cs, GetAll endpoint:
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] GetAllListingsQuery query)
{
    var result = await mediator.Send(query);
    return Ok(result);
}
```

The `[FromQuery]` attribute means ASP.NET will bind `?page=1&pageSize=20` into `query.Page`/`query.PageSize` automatically.

- [ ] **Step 5: Build backend**

```bash
dotnet build ./Automotive.Marketplace.sln
```

- [ ] **Step 6: Update FE API options**

```ts
// getAllListingsOptions.ts
import { queryOptions } from "@tanstack/react-query";
import axiosClient from "@/lib/axios/axiosClient";
import type { PagedResult } from "@/types/common";  // create if needed
import type { GetAllListingsResponse } from "./types";
import { listingKeys } from "@/features/listings/api/listingKeys";

export const getAllListingsOptions = (
  filters: ListingFilters,
  page: number = 1,
  pageSize: number = 20,
) =>
  queryOptions({
    queryKey: listingKeys.all(filters, page, pageSize),
    queryFn: () =>
      axiosClient.get<PagedResult<GetAllListingsResponse>>(
        ENDPOINTS.LISTING.GET_ALL,
        { params: { ...filters, page, pageSize } },
      ),
  });
```

Also add a `PagedResult<T>` type if one doesn't exist:

```ts
// src/types/common.ts (or wherever common types live)
export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

And update `listingKeys.all` to include page/pageSize in the query key so React Query caches pages separately.

- [ ] **Step 7: Create Pagination UI component**

```tsx
// src/components/ui/Pagination.tsx
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-center gap-2 py-4">
      <Button
        variant="outline"
        size="sm"
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
      >
        <ChevronLeft className="h-4 w-4" />
      </Button>

      {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
        const p = page <= 4 ? i + 1 : page + i - 3;
        if (p < 1 || p > totalPages) return null;
        return (
          <Button
            key={p}
            variant={p === page ? "default" : "outline"}
            size="sm"
            onClick={() => onPageChange(p)}
          >
            {p}
          </Button>
        );
      })}

      <Button
        variant="outline"
        size="sm"
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
      >
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  );
}
```

- [ ] **Step 8: Hook up pagination in Listings.tsx**

```tsx
// Listings.tsx
// Add page state (stored in URL search params like other filters):
const [page, setPage] = useState(1);

// Reset to page 1 when filters change
const handleFilterChange = (newFilters) => {
  setPage(1);
  // ... existing filter handling
};

// Use paged query
const { data } = useQuery(getAllListingsOptions(filters, page));
const listings = data?.data.items ?? [];
const totalPages = data?.data.totalPages ?? 1;

// Add Pagination below the listing grid:
<Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
```

- [ ] **Step 9: Verify full build**

```bash
dotnet build ./Automotive.Marketplace.sln
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 10: Commit**

```bash
git add \
  Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/ \
  Automotive.Marketplace.Application/Common/Models/PagedResult.cs \
  Automotive.Marketplace.Server/Controllers/ListingController.cs \
  automotive.marketplace.client/src/features/listingList/api/getAllListingsOptions.ts \
  automotive.marketplace.client/src/app/pages/Listings.tsx \
  automotive.marketplace.client/src/components/ui/Pagination.tsx
git commit -m "feat: add server-side pagination to listing search

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
