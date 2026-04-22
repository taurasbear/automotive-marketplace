# Compare Listings — Swap & Liked Listings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the `/compare` page with "Change" buttons that let users swap either listing in-place, and surface saved/liked listings inside the search modal.

**Architecture:** Refactor `CompareSearchModal` from a self-contained navigator to a controlled component (`excludeIds` + `onSelect` props), then layer liked listings logic on top. Add optional `onChange` callbacks to `CompareHeader`/`ListingCard`. Wire up swap state (`swapSlot`) in `Compare.tsx`.

**Tech Stack:** React 19, TypeScript, TanStack Query (`useQuery`, `queryOptions`), Redux (`useAppSelector`), Vitest + React Testing Library (jsdom), shadcn/ui (`Button`, `Dialog`)

---

## File Map

| File | Change |
|------|--------|
| `src/features/compareListings/components/CompareSearchModal.tsx` | Refactor props; add liked listings logic |
| `src/features/compareListings/components/CompareSearchModal.test.tsx` | New test file |
| `src/features/compareListings/components/CompareHeader.tsx` | Add optional `onChange` buttons |
| `src/app/pages/Compare.tsx` | Add `swapSlot` state; render modal; pass Change callbacks |
| `src/app/pages/Compare.test.tsx` | Update mocks; add swap tests |
| `src/features/listingDetails/components/ListingDetailsContent.tsx` | Update to new modal prop API |

---

### Task 1: Write failing tests for the new `CompareSearchModal` prop API

**Files:**
- Create: `src/features/compareListings/components/CompareSearchModal.test.tsx`

- [ ] **Step 1: Create the test file**

```tsx
// src/features/compareListings/components/CompareSearchModal.test.tsx
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { CompareSearchModal } from "./CompareSearchModal";
import type { SearchListingsResponse } from "../types/SearchListingsResponse";

vi.mock("@/lib/router", () => ({
  router: { navigate: vi.fn() },
}));

const useAppSelectorMock = vi.fn();
vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) =>
    useAppSelectorMock(...args),
}));

const getSavedListingsOptionsMock = vi.fn();
vi.mock("@/features/savedListings/api/getSavedListingsOptions", () => ({
  getSavedListingsOptions: getSavedListingsOptionsMock,
}));

const searchListingsOptionsMock = vi.fn();
vi.mock("../api/searchListingsOptions", () => ({
  searchListingsOptions: searchListingsOptionsMock,
}));

const searchResults: SearchListingsResponse[] = [
  {
    id: "listing-1",
    makeName: "Toyota",
    modelName: "Camry",
    year: 2020,
    price: 15000,
    mileage: 50000,
    city: "Vilnius",
    sellerName: "John",
  },
  {
    id: "listing-2",
    makeName: "Honda",
    modelName: "Civic",
    year: 2019,
    price: 12000,
    mileage: 80000,
    city: "Kaunas",
    sellerName: "Jane",
  },
];

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

beforeEach(() => {
  useAppSelectorMock.mockReturnValue(null); // not logged in by default
  getSavedListingsOptionsMock.mockReturnValue({
    queryKey: ["saved-listings"],
    queryFn: async () => ({ data: [] }),
  });
  searchListingsOptionsMock.mockReturnValue({
    queryKey: ["search", "camry"],
    queryFn: async () => ({ data: searchResults }),
    enabled: true,
  });
});

describe("CompareSearchModal — new prop API", () => {
  it("calls onSelect with the listing id when Compare is clicked", async () => {
    const onSelect = vi.fn();
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={onSelect}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getAllByRole("button", { name: "Compare" })).toHaveLength(
        searchResults.length,
      ),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("listing-1");
    expect(onSelect).toHaveBeenCalledTimes(1);
  });

  it("filters out listings whose id is in excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["listing-1"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(
        screen.getByRole("button", { name: "Compare" }),
      ).toBeInTheDocument(),
    );

    expect(screen.queryByText("Toyota")).not.toBeInTheDocument();
    expect(screen.getByText("Honda")).toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareSearchModal.test.tsx
```

Expected: FAIL — `CompareSearchModal` still has `currentListingId` prop; TypeScript errors.

---

### Task 2: Refactor `CompareSearchModal` to `excludeIds` + `onSelect` API

**Files:**
- Modify: `src/features/compareListings/components/CompareSearchModal.tsx`

- [ ] **Step 1: Replace the component with the new prop API**

Replace the entire file content:

```tsx
// src/features/compareListings/components/CompareSearchModal.tsx
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { searchListingsOptions } from "../api/searchListingsOptions";

type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  excludeIds: string[];
  onSelect: (id: string) => void;
};

export const CompareSearchModal = ({
  open,
  onClose,
  excludeIds,
  onSelect,
}: CompareSearchModalProps) => {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedQuery(query), 300);
    return () => clearTimeout(timer);
  }, [query]);

  const { data } = useQuery(searchListingsOptions(debouncedQuery));
  const results = (data?.data ?? []).filter((r) => !excludeIds.includes(r.id));

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Compare with another listing</DialogTitle>
        </DialogHeader>
        <Input
          placeholder="Search by make, model, year, seller…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoFocus
        />
        <div className="mt-4 max-h-96 space-y-2 overflow-y-auto">
          {results.map((listing) => (
            <div
              key={listing.id}
              className="flex items-center gap-3 rounded-lg border p-3"
            >
              <img
                src={
                  listing.firstImageUrl ??
                  "https://placehold.co/80x60?text=No+Image"
                }
                alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
                className="h-14 w-20 rounded object-cover"
              />
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">
                  {listing.year} {listing.makeName} {listing.modelName}
                </p>
                <p className="text-muted-foreground text-sm">
                  {listing.price.toFixed(0)} € ·{" "}
                  {listing.mileage.toLocaleString()} km · {listing.city}
                </p>
                <p className="text-muted-foreground text-sm">
                  {listing.sellerName}
                </p>
              </div>
              <Button size="sm" onClick={() => onSelect(listing.id)}>
                Compare
              </Button>
            </div>
          ))}
          {debouncedQuery && results.length === 0 && (
            <p className="text-muted-foreground py-4 text-center">
              No results found
            </p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
```

- [ ] **Step 2: Run Task 1 tests to verify they pass**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareSearchModal.test.tsx
```

Expected: PASS — both `onSelect` and `excludeIds` tests pass.

- [ ] **Step 3: Run the full test suite to check for regressions**

```bash
cd automotive.marketplace.client && npx vitest run
```

Expected: existing `Compare.test.tsx` and `CompareTable.test.tsx` tests pass. `ListingDetailsContent` currently passes `currentListingId` which is now a TypeScript error — this is expected and will be fixed in Task 3.

- [ ] **Step 4: Commit**

```bash
git add src/features/compareListings/components/CompareSearchModal.tsx \
        src/features/compareListings/components/CompareSearchModal.test.tsx
git commit -m "refactor: replace CompareSearchModal currentListingId with excludeIds + onSelect

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Update `ListingDetailsContent.tsx` to new modal prop API

**Files:**
- Modify: `src/features/listingDetails/components/ListingDetailsContent.tsx:232-236`

- [ ] **Step 1: Replace the old modal usage**

In `ListingDetailsContent.tsx`, find:

```tsx
      <CompareSearchModal
        open={compareModalOpen}
        onClose={() => setCompareModalOpen(false)}
        currentListingId={id}
      />
```

Replace with:

```tsx
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

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

Expected: No errors.

- [ ] **Step 3: Commit**

```bash
git add src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "fix: update ListingDetailsContent to new CompareSearchModal prop API

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Write failing tests for liked listings in `CompareSearchModal`

**Files:**
- Modify: `src/features/compareListings/components/CompareSearchModal.test.tsx`

- [ ] **Step 1: Add liked listings test cases to the test file**

Append these `describe` blocks to `CompareSearchModal.test.tsx` (after the existing `describe` block):

```tsx
import type { SavedListing } from "@/features/savedListings/types/SavedListing";

const savedListings: SavedListing[] = [
  {
    listingId: "saved-111",
    title: "2021 BMW 3 Series",
    price: 25000,
    city: "Klaipeda",
    mileage: 30000,
    fuelName: "Diesel",
    transmissionName: "Automatic",
    thumbnail: { url: "https://example.com/bmw.jpg", altText: "BMW" },
    noteContent: null,
  },
  {
    listingId: "saved-222",
    title: "2018 Audi A4",
    price: 18000,
    city: "Vilnius",
    mileage: 60000,
    fuelName: "Petrol",
    transmissionName: "Automatic",
    thumbnail: null,
    noteContent: null,
  },
];

describe("CompareSearchModal — liked listings (empty query)", () => {
  beforeEach(() => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: savedListings }),
    });
    searchListingsOptionsMock.mockReturnValue({
      queryKey: ["search", ""],
      queryFn: async () => ({ data: [] }),
      enabled: false,
    });
  });

  it("shows Your saved listings section with saved items when query is empty and user is logged in", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("Your saved listings")).toBeInTheDocument(),
    );
    expect(screen.getByText("2021 BMW 3 Series")).toBeInTheDocument();
    expect(screen.getByText("2018 Audi A4")).toBeInTheDocument();
  });

  it("calls onSelect with the saved listing's listingId when Compare is clicked", async () => {
    const onSelect = vi.fn();
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={onSelect}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("2021 BMW 3 Series")).toBeInTheDocument(),
    );

    fireEvent.click(screen.getAllByRole("button", { name: "Compare" })[0]);
    expect(onSelect).toHaveBeenCalledWith("saved-111");
  });

  it("filters saved listings by excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["saved-111"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("Your saved listings")).toBeInTheDocument(),
    );
    expect(screen.queryByText("2021 BMW 3 Series")).not.toBeInTheDocument();
    expect(screen.getByText("2018 Audi A4")).toBeInTheDocument();
  });

  it("shows nothing when user is not logged in", () => {
    useAppSelectorMock.mockReturnValue(null);
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    expect(screen.queryByText("Your saved listings")).not.toBeInTheDocument();
  });
});

const searchResultsWithSaved: SearchListingsResponse[] = [
  {
    id: "saved-111",
    makeName: "BMW",
    modelName: "3 Series",
    year: 2021,
    price: 25000,
    mileage: 30000,
    city: "Klaipeda",
    sellerName: "Max",
  },
  {
    id: "listing-2",
    makeName: "Honda",
    modelName: "Civic",
    year: 2019,
    price: 12000,
    mileage: 80000,
    city: "Kaunas",
    sellerName: "Jane",
  },
];

describe("CompareSearchModal — liked listings (with query)", () => {
  beforeEach(() => {
    useAppSelectorMock.mockReturnValue("user-1");
    getSavedListingsOptionsMock.mockReturnValue({
      queryKey: ["saved-listings"],
      queryFn: async () => ({ data: savedListings }),
    });
    searchListingsOptionsMock.mockReturnValue({
      queryKey: ["search", "bmw"],
      queryFn: async () => ({ data: searchResultsWithSaved }),
      enabled: true,
    });
  });

  it("promotes saved matches to the top with a ❤ Saved badge", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={[]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText(/❤ Saved/)).toBeInTheDocument(),
    );

    const compareButtons = screen.getAllByRole("button", { name: "Compare" });
    expect(compareButtons).toHaveLength(2);

    // BMW (saved match) appears before Honda (unsaved)
    const allText = document.body.textContent ?? "";
    expect(allText.indexOf("BMW")).toBeLessThan(allText.indexOf("Honda"));
  });

  it("filters search results by excludeIds", async () => {
    render(
      <CompareSearchModal
        open={true}
        onClose={vi.fn()}
        excludeIds={["saved-111"]}
        onSelect={vi.fn()}
      />,
      { wrapper: createWrapper() },
    );

    await waitFor(() =>
      expect(screen.getByText("Honda")).toBeInTheDocument(),
    );
    expect(screen.queryByText("BMW")).not.toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Run tests to verify new ones fail**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareSearchModal.test.tsx
```

Expected: The two original tests from Task 1 still pass. All new liked-listings tests FAIL — the component doesn't have liked listings logic yet.

---

### Task 5: Add liked listings logic to `CompareSearchModal`

**Files:**
- Modify: `src/features/compareListings/components/CompareSearchModal.tsx`

- [ ] **Step 1: Replace the component with the full implementation**

```tsx
// src/features/compareListings/components/CompareSearchModal.tsx
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useAppSelector } from "@/hooks/redux";
import { getSavedListingsOptions } from "@/features/savedListings/api/getSavedListingsOptions";
import type { SavedListing } from "@/features/savedListings/types/SavedListing";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { searchListingsOptions } from "../api/searchListingsOptions";

type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  excludeIds: string[];
  onSelect: (id: string) => void;
};

export const CompareSearchModal = ({
  open,
  onClose,
  excludeIds,
  onSelect,
}: CompareSearchModalProps) => {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedQuery(query), 300);
    return () => clearTimeout(timer);
  }, [query]);

  const userId = useAppSelector((state) => state.auth.userId);

  const { data: savedData } = useQuery({
    ...getSavedListingsOptions(),
    enabled: userId !== null,
  });
  const savedListings: SavedListing[] = savedData?.data ?? [];

  const savedIdSet = useMemo(
    () => new Set(savedListings.map((s) => s.listingId)),
    [savedListings],
  );

  const { data: searchData } = useQuery(searchListingsOptions(debouncedQuery));
  const allResults = searchData?.data ?? [];

  const savedMatches = allResults.filter(
    (r) => savedIdSet.has(r.id) && !excludeIds.includes(r.id),
  );
  const otherResults = allResults.filter(
    (r) => !savedIdSet.has(r.id) && !excludeIds.includes(r.id),
  );

  const visibleSaved = savedListings.filter(
    (s) => !excludeIds.includes(s.listingId),
  );

  const showEmptyState = debouncedQuery === "";

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Compare with another listing</DialogTitle>
        </DialogHeader>
        <Input
          placeholder="Search by make, model, year, seller…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoFocus
        />
        <div className="mt-4 max-h-96 space-y-2 overflow-y-auto">
          {showEmptyState && visibleSaved.length > 0 && (
            <>
              <p className="text-muted-foreground px-1 text-xs font-semibold uppercase tracking-wide">
                Your saved listings
              </p>
              {visibleSaved.map((listing) => (
                <div
                  key={listing.listingId}
                  className="flex items-center gap-3 rounded-lg border p-3"
                >
                  <img
                    src={
                      listing.thumbnail?.url ??
                      "https://placehold.co/80x60?text=No+Image"
                    }
                    alt={listing.title}
                    className="h-14 w-20 rounded object-cover"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">{listing.title}</p>
                    <p className="text-muted-foreground text-sm">
                      {listing.price.toFixed(0)} € ·{" "}
                      {listing.mileage.toLocaleString()} km · {listing.city}
                    </p>
                  </div>
                  <Button size="sm" onClick={() => onSelect(listing.listingId)}>
                    Compare
                  </Button>
                </div>
              ))}
            </>
          )}

          {!showEmptyState && (
            <>
              {savedMatches.map((listing) => (
                <div
                  key={listing.id}
                  className="flex items-center gap-3 rounded-lg border p-3"
                >
                  <img
                    src={
                      listing.firstImageUrl ??
                      "https://placehold.co/80x60?text=No+Image"
                    }
                    alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
                    className="h-14 w-20 rounded object-cover"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">
                      {listing.year} {listing.makeName} {listing.modelName}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.price.toFixed(0)} € ·{" "}
                      {listing.mileage.toLocaleString()} km · {listing.city}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.sellerName}
                    </p>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <span className="rounded-full bg-red-100 px-2 py-0.5 text-xs font-semibold text-red-600">
                      ❤ Saved
                    </span>
                    <Button size="sm" onClick={() => onSelect(listing.id)}>
                      Compare
                    </Button>
                  </div>
                </div>
              ))}
              {otherResults.map((listing) => (
                <div
                  key={listing.id}
                  className="flex items-center gap-3 rounded-lg border p-3"
                >
                  <img
                    src={
                      listing.firstImageUrl ??
                      "https://placehold.co/80x60?text=No+Image"
                    }
                    alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
                    className="h-14 w-20 rounded object-cover"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">
                      {listing.year} {listing.makeName} {listing.modelName}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.price.toFixed(0)} € ·{" "}
                      {listing.mileage.toLocaleString()} km · {listing.city}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.sellerName}
                    </p>
                  </div>
                  <Button size="sm" onClick={() => onSelect(listing.id)}>
                    Compare
                  </Button>
                </div>
              ))}
              {debouncedQuery &&
                savedMatches.length === 0 &&
                otherResults.length === 0 && (
                  <p className="text-muted-foreground py-4 text-center">
                    No results found
                  </p>
                )}
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
```

- [ ] **Step 2: Run all `CompareSearchModal` tests**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareSearchModal.test.tsx
```

Expected: All tests PASS.

- [ ] **Step 3: Run the full suite to check for regressions**

```bash
cd automotive.marketplace.client && npx vitest run
```

Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/features/compareListings/components/CompareSearchModal.tsx \
        src/features/compareListings/components/CompareSearchModal.test.tsx
git commit -m "feat: add liked listings logic to CompareSearchModal

- Show Your saved listings section when query is empty and user is logged in
- Promote saved matches to top with ❤ Saved badge during search
- Both sections respect excludeIds filter

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Write failing tests for `CompareHeader` Change buttons

**Files:**
- Create: `src/features/compareListings/components/CompareHeader.test.tsx`

- [ ] **Step 1: Create the test file**

```tsx
// src/features/compareListings/components/CompareHeader.test.tsx
import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { CompareHeader } from "./CompareHeader";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

const listingA: GetListingByIdResponse = {
  id: "a1",
  makeName: "Toyota",
  modelName: "Camry",
  price: 15000,
  powerKw: 120,
  engineSizeMl: 1998,
  mileage: 50000,
  isSteeringWheelRight: false,
  city: "Vilnius",
  isUsed: true,
  year: 2020,
  transmissionName: "Automatic",
  fuelName: "Petrol",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "FWD",
  sellerName: "John",
  sellerId: "s1",
  status: "Available",
  images: [],
};

const listingB: GetListingByIdResponse = {
  ...listingA,
  id: "b1",
  makeName: "Honda",
  modelName: "Civic",
};

describe("CompareHeader — Change buttons", () => {
  it("does not render any Change button when no onChange callbacks are provided", () => {
    render(<CompareHeader listingA={listingA} listingB={listingB} />);
    expect(
      screen.queryByRole("button", { name: "Change" }),
    ).not.toBeInTheDocument();
  });

  it("renders a Change button for listing A when onChangeA is provided", () => {
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={vi.fn()}
      />,
    );
    expect(
      screen.getByRole("button", { name: "Change" }),
    ).toBeInTheDocument();
  });

  it("calls onChangeA when the first Change button is clicked", () => {
    const onChangeA = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={onChangeA}
        onChangeB={vi.fn()}
      />,
    );

    const buttons = screen.getAllByRole("button", { name: "Change" });
    fireEvent.click(buttons[0]);
    expect(onChangeA).toHaveBeenCalledTimes(1);
  });

  it("calls onChangeB when the second Change button is clicked", () => {
    const onChangeB = vi.fn();
    render(
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={vi.fn()}
        onChangeB={onChangeB}
      />,
    );

    const buttons = screen.getAllByRole("button", { name: "Change" });
    fireEvent.click(buttons[1]);
    expect(onChangeB).toHaveBeenCalledTimes(1);
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareHeader.test.tsx
```

Expected: FAIL — `CompareHeader` has no `onChangeA`/`onChangeB` props yet.

---

### Task 7: Add Change buttons to `CompareHeader`

**Files:**
- Modify: `src/features/compareListings/components/CompareHeader.tsx`

- [ ] **Step 1: Replace the component**

```tsx
// src/features/compareListings/components/CompareHeader.tsx
import { Button } from "@/components/ui/button";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

type ListingCardProps = {
  listing: GetListingByIdResponse;
  onChange?: () => void;
};

const ListingCard = ({ listing, onChange }: ListingCardProps) => (
  <div className="text-center">
    <img
      src={
        listing.images[0]?.url ??
        "https://placehold.co/200x150?text=No+Image"
      }
      alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
      className="mx-auto h-32 w-48 rounded object-cover"
    />
    <p className="mt-2 font-semibold">
      {listing.year} {listing.makeName} {listing.modelName}
    </p>
    <p className="text-primary font-bold">{listing.price.toFixed(0)} €</p>
    <p className="text-sm text-muted-foreground">{listing.city}</p>
    {onChange && (
      <Button
        variant="outline"
        size="sm"
        className="mt-2"
        onClick={onChange}
      >
        Change
      </Button>
    )}
  </div>
);

type CompareHeaderProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
  onChangeA?: () => void;
  onChangeB?: () => void;
};

export const CompareHeader = ({
  listingA,
  listingB,
  onChangeA,
  onChangeB,
}: CompareHeaderProps) => (
  <div className="sticky top-0 z-10 mb-4 grid grid-cols-3 rounded-lg border bg-background p-4 shadow-sm">
    <div className="flex items-center">
      <span className="text-sm font-semibold text-muted-foreground">
        Specification
      </span>
    </div>
    <ListingCard listing={listingA} onChange={onChangeA} />
    <ListingCard listing={listingB} onChange={onChangeB} />
  </div>
);
```

- [ ] **Step 2: Run Task 6 tests**

```bash
cd automotive.marketplace.client && npx vitest run src/features/compareListings/components/CompareHeader.test.tsx
```

Expected: All 4 tests PASS.

- [ ] **Step 3: Run full suite to check for regressions**

```bash
cd automotive.marketplace.client && npx vitest run
```

Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/features/compareListings/components/CompareHeader.tsx \
        src/features/compareListings/components/CompareHeader.test.tsx
git commit -m "feat: add optional Change buttons to CompareHeader

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Write failing tests for Compare swap orchestration

**Files:**
- Modify: `src/app/pages/Compare.test.tsx`

- [ ] **Step 1: Add mocks for new dependencies and swap tests**

Add these `vi.mock` calls near the top of `Compare.test.tsx`, after the existing mocks:

```typescript
// After existing vi.mock calls, add:
vi.mock("@/hooks/redux", () => ({
  useAppSelector: vi.fn().mockReturnValue(null),
}));

vi.mock("@/features/savedListings/api/getSavedListingsOptions", () => ({
  getSavedListingsOptions: vi.fn().mockReturnValue({
    queryKey: ["saved-listings"],
    queryFn: async () => ({ data: [] }),
  }),
}));

vi.mock("@/features/compareListings/api/searchListingsOptions", () => ({
  searchListingsOptions: vi.fn().mockReturnValue({
    queryKey: ["search", ""],
    queryFn: async () => ({ data: [] }),
    enabled: false,
  }),
}));
```

Then add a new `describe` block at the end of the file:

```typescript
describe("Compare page — swap orchestration", () => {
  it("opens the modal targeting slot A when the first Change button is clicked", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getAllByRole("button", { name: "Change" })).toHaveLength(2);
    });

    // Dialog should not be open yet
    expect(
      screen.queryByRole("dialog"),
    ).not.toBeInTheDocument();

    fireEvent.click(screen.getAllByRole("button", { name: "Change" })[0]);

    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("opens the modal targeting slot B when the second Change button is clicked", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getAllByRole("button", { name: "Change" })).toHaveLength(2);
    });

    fireEvent.click(screen.getAllByRole("button", { name: "Change" })[1]);

    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("closes the modal when onClose is triggered", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getAllByRole("button", { name: "Change" })).toHaveLength(2);
    });

    fireEvent.click(screen.getAllByRole("button", { name: "Change" })[0]);
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    // Press Escape to close the dialog
    fireEvent.keyDown(screen.getByRole("dialog"), { key: "Escape" });

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
  });
});
```

- [ ] **Step 2: Run tests to verify new ones fail**

```bash
cd automotive.marketplace.client && npx vitest run src/app/pages/Compare.test.tsx
```

Expected: Existing tests pass. New swap tests FAIL — `Compare.tsx` does not render `CompareSearchModal` or pass `onChangeA`/`onChangeB` to `CompareHeader` yet.

---

### Task 9: Wire swap orchestration in `Compare.tsx`

**Files:**
- Modify: `src/app/pages/Compare.tsx`

- [ ] **Step 1: Replace `Compare.tsx`**

```tsx
// src/app/pages/Compare.tsx
import { getListingComparisonOptions } from "@/features/compareListings/api/getListingComparisonOptions";
import {
  CompareHeader,
  CompareSearchModal,
  CompareTable,
  DiffToggleFab,
} from "@/features/compareListings";
import { computeDiff } from "@/features/compareListings/utils/computeDiff";
import { router } from "@/lib/router";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Route } from "@/app/routes/compare";

const Compare = () => {
  const { a, b } = Route.useSearch();
  const [diffOnly, setDiffOnly] = useState(false);
  const [swapSlot, setSwapSlot] = useState<"a" | "b" | null>(null);

  const { data: response, isLoading, isError } = useQuery(
    getListingComparisonOptions(a, b),
  );

  if (isLoading) {
    return (
      <div className="py-8">
        <div className="mb-4 h-48 animate-pulse rounded-lg bg-muted" />
        <div className="space-y-2">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="h-10 animate-pulse rounded bg-muted" />
          ))}
        </div>
      </div>
    );
  }

  if (isError || !response) {
    return (
      <div className="py-16 text-center">
        <p className="text-muted-foreground">
          One or more listings could not be found.
        </p>
      </div>
    );
  }

  const { listingA, listingB } = response.data;
  const diffMap = computeDiff(listingA, listingB);

  return (
    <div className="py-8">
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={() => setSwapSlot("a")}
        onChangeB={() => setSwapSlot("b")}
      />
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={diffMap}
        diffOnly={diffOnly}
      />
      <DiffToggleFab active={diffOnly} onToggle={() => setDiffOnly((d) => !d)} />
      <CompareSearchModal
        open={swapSlot !== null}
        onClose={() => setSwapSlot(null)}
        excludeIds={swapSlot === "a" ? [b] : [a]}
        onSelect={(id) => {
          setSwapSlot(null);
          void router.navigate({
            to: "/compare",
            search: swapSlot === "a" ? { a: id, b } : { a, b: id },
          });
        }}
      />
    </div>
  );
};

export default Compare;
```

- [ ] **Step 2: Run Task 8 tests**

```bash
cd automotive.marketplace.client && npx vitest run src/app/pages/Compare.test.tsx
```

Expected: All tests PASS.

- [ ] **Step 3: Run the full suite**

```bash
cd automotive.marketplace.client && npx vitest run
```

Expected: All tests pass (zero failures).

- [ ] **Step 4: TypeScript check**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

Expected: No errors.

- [ ] **Step 5: Lint check**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
```

Expected: No errors.

- [ ] **Step 6: Commit**

```bash
git add src/app/pages/Compare.tsx src/app/pages/Compare.test.tsx
git commit -m "feat: add swap slot orchestration to Compare page

- Add swapSlot state ('a' | 'b' | null)
- Pass onChangeA/onChangeB to CompareHeader
- Render CompareSearchModal with correct excludeIds and navigation onSelect

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
