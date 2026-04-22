# Listing Comparison Feature — Design Spec

**Date:** 2026-04-21  
**Status:** Approved

---

## Problem & Approach

Users need a way to evaluate two car listings side by side before making a purchase decision — similar to how RTings.com compares headphones. The feature adds a dedicated compare page at `/compare?a={id}&b={id}` with a shareable URL, a 3-column comparison table (Spec / Listing A / Listing B), difference highlighting, and a floating toggle to show only differing rows.

---

## User Flow

1. User is on a listing detail page and clicks **"Compare with another listing"**.
2. A modal opens with a free-text search field. The user types a make, model, year, seller name, or pastes a listing ID. Results appear as cards with thumbnail, price, mileage, city, and seller.
3. The user clicks **Compare** on a result — the browser navigates to `/compare?a=<current-id>&b=<selected-id>`.
4. The compare page fetches both listings in a single backend call and renders the comparison table.
5. The URL is shareable — anyone opening it sees the same comparison.

---

## Backend

### 1. `SearchListings` query

**Purpose:** Flexible text search for the compare modal. Not connected to the existing structured filter search.

- **Endpoint:** `GET /api/listings/search?q={text}&limit={n}`
- **Handler:** `SearchListingsQueryHandler` in `Application/Features/ListingFeatures/SearchListings/`
- **Query:** `SearchListingsQuery { string Q, int Limit = 10 }`
- **Search scope:** make name, model name, year (if `q` is a 4-digit number), seller name, exact listing ID match
- **Response:** `SearchListingsResponse` (lightweight — id, make, model, year, price, mileage, city, seller name, first image URL)
- **Returns:** Empty list if no results; does not 404.
- **Auth:** Public (unauthenticated), consistent with existing listing GET endpoints.

### 2. `GetListingComparison` query

**Purpose:** Fetch full details for two listings in one request for the compare page.

- **Endpoint:** `GET /api/listings/compare?a={guid}&b={guid}`
- **Handler:** `GetListingComparisonQueryHandler` in `Application/Features/ListingFeatures/GetListingComparison/`
- **Query:** `GetListingComparisonQuery { Guid ListingAId, Guid ListingBId }`
- **Response:** `GetListingComparisonResponse { ListingA, ListingB }` where each is the full listing shape (same fields as `GetListingByIdResponse`)
- **Validation:** Returns `404` if either listing ID does not exist.
- **Auth:** Public (unauthenticated), consistent with existing listing GET endpoints.
- **Extension point:** This handler is intentionally separate from `GetListingByIdQueryHandler` to allow future additions such as AI-generated comparison summaries, market price benchmarking, or cross-listing analytics.

---

## Frontend

### Route

- Path: `/compare` with required search params `a` (Guid) and `b` (Guid)
- TanStack Router route file: `src/routes/compare.tsx`
- Redirects to home if either param is missing or invalid.

### Feature folder

`src/features/compareListings/`

```
compareListings/
  api/
    getListingComparison.ts      # useGetListingComparison(idA, idB) TanStack Query hook
    searchListings.ts            # useSearchListings(q) TanStack Query hook (debounced)
  components/
    CompareHeader.tsx            # Sticky top row: "Specification" label + two listing cards with thumbnails
    CompareTable.tsx             # Full table rendering rows grouped by section
    CompareRow.tsx               # Single row: spec label + two values with diff highlight
    CompareSearchModal.tsx       # Dialog with search input + results list
    DiffToggleFab.tsx            # Floating action button (bottom-right), toggles diff-only mode
  utils/
    computeDiff.ts               # Pure function: given two listings, returns DiffMap
  types/
    diff.ts                      # DiffMap type: Record<field, 'equal' | 'a-better' | 'b-better' | 'different'>
  index.ts
```

### Compare page (`src/routes/compare.tsx`)

- Reads `a` and `b` from URL search params
- Calls `useGetListingComparison(a, b)` — single fetch
- Passes both listings + computed `DiffMap` down to `CompareHeader` and `CompareTable`
- Shows skeleton/loading state while fetching; shows error message if 404

### Comparison table

Spec rows are grouped into sections:

| Section | Fields |
|---|---|
| Basic Info | Make, Model, Body Type, Year, Condition, Mileage, City |
| Engine & Performance | Power (kW), Engine Size (ml), Fuel Type, Transmission, Drivetrain |
| Listing Details | Price, Status, Seller |

**Difference highlighting rules:**
- Numeric fields where higher is better (Power, Year): higher value → green ↑, lower → orange ↓
- Numeric fields where lower is better (Mileage, Price): lower value → green ↑, higher → orange ↓
- String fields: if different → both cells get a subtle amber tint; if same → neutral
- Row background: differing rows get `rgba(249,115,22,0.05)` tint

### `computeDiff(a, b): DiffMap`

Pure utility function. Takes two `GetListingByIdResponse`-shaped objects, returns a record mapping each field name to one of:
- `'equal'` — values are the same
- `'a-better'` — A has the better numeric value
- `'b-better'` — B has the better numeric value
- `'different'` — string fields that differ

The "better" direction is encoded in a static config map inside `computeDiff.ts`.

### `DiffToggleFab`

Floating button, fixed bottom-right. When active, `CompareTable` filters to only render rows where `diffMap[field] !== 'equal'`. State is local to the page (no URL persistence needed).

### `CompareSearchModal`

- Opens as a dialog (Radix UI Dialog, consistent with existing app patterns)
- Input: debounced (300 ms) free-text, calls `useSearchListings(q)`
- Excludes the current listing from results (filters by ID client-side after fetch)
- Each result card: thumbnail, make + model + year, price, mileage, city, seller
- Clicking **Compare** closes the modal and navigates to `/compare?a=<current>&b=<selected>`

---

## Out of Scope

- Comparing more than 2 listings simultaneously
- Saving or bookmarking comparisons server-side
- AI summary / market price integration (future extension via `GetListingComparisonQueryHandler`)
- Mobile-specific layout (responsive at the table level is acceptable for now)

---

## Testing

**Backend:**
- `GetListingComparisonQueryHandlerTests`: happy path (both exist), 404 when A missing, 404 when B missing, same ID for both
- `SearchListingsQueryHandlerTests`: text match on make, model, year, seller, listing ID; empty result; limit respected

**Frontend:**
- Unit tests for `computeDiff` covering all field types and direction rules
- Component tests for `CompareTable` with diff highlighting
- Integration test: compare page loads correctly given two valid IDs in the URL
