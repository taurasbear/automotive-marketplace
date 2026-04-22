# Compare Listings — Swap & Liked Listings Design

**Date:** 2026-04-22  
**Feature:** Extend the `/compare` page to allow swapping either listing in-place, and surface liked/saved listings inside the search modal.

---

## Goal

Two improvements to the existing listing comparison feature:

1. **Swap either listing from the compare page** — "Change" buttons on each listing card in the sticky header open the search modal for that specific slot. Selecting a new listing updates the URL and the comparison reloads.
2. **Liked listings in the modal** — When the search box is empty, the modal shows the user's saved/liked listings as a "Your saved listings" section. When the user types and a saved listing matches the results, it is promoted to the top with a ❤ Saved badge.

---

## Architecture

### Files modified (no new files)

| File | Change |
|------|--------|
| `src/features/compareListings/components/CompareSearchModal.tsx` | Refactor props; add liked listings logic |
| `src/features/compareListings/components/CompareHeader.tsx` | Add optional "Change" buttons below each card |
| `src/app/pages/Compare.tsx` | Add swap slot state; wire up modal and navigation |
| `src/features/listingDetails/components/ListingDetailsContent.tsx` | Update to new modal prop API |

---

## CompareSearchModal — new prop API

### Before
```typescript
type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  currentListingId: string;
};
```

### After
```typescript
type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  excludeIds: string[];        // listings to hide from results
  onSelect: (id: string) => void;  // caller handles navigation
};
```

- `currentListingId` is removed; replaced by `excludeIds: string[]` so callers can exclude one or both comparison listings.
- Internal `router.navigate` call is removed entirely. The modal calls `onSelect(id)` and the parent decides what to do.

### Liked listings logic

- `useAppSelector((state) => state.auth.userId)` — if `null`, user is not logged in and liked listings are not fetched.
- `useQuery(getSavedListingsOptions())` enabled only when `userId !== null`.
- A `savedIdSet: Set<string>` is derived from the fetched saved listings using `SavedListing.listingId` (note: the `SavedListing` type uses `listingId`, not `id`) for O(1) lookup.

**Empty search box (`debouncedQuery === ""`)**:
- Show a "Your saved listings" section header.
- Display saved listings filtered by `excludeIds` (compare against `SavedListing.listingId`), using `SavedListing.thumbnail?.url` for the image and `SavedListing.title` for the name.
- Clicking Compare calls `onSelect(listing.listingId)`.
- If no saved listings (or not logged in): show nothing in this state (just the placeholder input).

**With query (`debouncedQuery !== ""`)**:
- Fetch `searchListingsOptions(debouncedQuery)` as before.
- Partition results: `savedMatches` = results where `id ∈ savedIdSet`; `otherResults` = the rest.
- Render `savedMatches` first, each with a `❤ Saved` badge (red pill), then `otherResults`.
- Both lists are filtered by `excludeIds`.

---

## CompareHeader — "Change" buttons

```typescript
type CompareHeaderProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
  onChangeA?: () => void;   // optional; button only rendered when provided
  onChangeB?: () => void;
};
```

- `ListingCard` gains an optional `onChange?: () => void` prop.
- When provided, a small `"Change"` button (variant `"outline"`, size `"sm"`) is rendered below the listing info inside the card.
- When not provided (e.g., if `CompareHeader` is ever reused without swap capability), no button appears.

---

## Compare.tsx — swap orchestration

```typescript
const [swapSlot, setSwapSlot] = useState<"a" | "b" | null>(null);
```

- `onChangeA` → `setSwapSlot("a")`
- `onChangeB` → `setSwapSlot("b")`
- Modal `open={swapSlot !== null}`
- Modal `onClose={() => setSwapSlot(null)}`
- Modal `excludeIds={swapSlot === "a" ? [b] : [a]}` — exclude the *other* listing from results
- Modal `onSelect`:
  ```typescript
  (id) => {
    setSwapSlot(null);
    void router.navigate({
      to: "/compare",
      search: swapSlot === "a" ? { a: id, b } : { a, b: id },
    });
  }
  ```

When the URL updates, TanStack Router reruns `validateSearch` and the `useQuery(getListingComparisonOptions(a, b))` key changes, triggering a fresh fetch.

---

## ListingDetailsContent.tsx — prop update

Replace:
```typescript
<CompareSearchModal
  open={compareModalOpen}
  onClose={() => setCompareModalOpen(false)}
  currentListingId={id}
/>
```

With:
```typescript
<CompareSearchModal
  open={compareModalOpen}
  onClose={() => setCompareModalOpen(false)}
  excludeIds={[id]}
  onSelect={(selectedId) => {
    setCompareModalOpen(false);
    void router.navigate({ to: "/compare", search: { a: id, b: selectedId } });
  }}
/>
```

---

## Error handling

- Liked listings fetch failure: silently ignored — the modal degrades to search-only mode (no saved section shown).
- Not logged in: `getSavedListingsOptions` not enabled; empty state shows nothing.

---

## Testing

- **`CompareSearchModal` unit tests**: add tests for (a) liked listings shown when query is empty and user is logged in, (b) saved match promoted to top with badge when query matches, (c) `onSelect` called with the correct ID instead of navigating internally.
- **`Compare.tsx` tests**: add test that "Change" buttons open the modal for the correct slot and that `onSelect` triggers navigation with the correct updated `a`/`b` params.
- Existing `computeDiff`, `CompareTable`, and `Compare` page tests are unaffected.

---

## Out of scope

- Swapping A and B with each other (flipping sides without searching).
- Adding likes/unliking from within the modal.
- Backend changes — `GET /Listing/Search` already supports this feature fully.
