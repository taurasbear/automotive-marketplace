# Bug Fixes Part 2 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 10 distinct bugs spanning loading flashes, filter UX, pagination, guest access, Cardog API integration, like button visibility, saved listing navigation, meeting location UI, and compare AI regeneration.

**Architecture:** All frontend changes are in `automotive.marketplace.client/src/`. Backend change is in `Automotive.Marketplace.Application/Features/`. No new routes or migrations needed except a query parameter addition.

**Tech Stack:** React 19, TanStack Query v5, TanStack Router, TypeScript, ASP.NET Core 8, EF Core, MediatR

---

## Task 1: Fix loading flash — wrap Suspense boundaries around MessageThread and ListingList

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Inbox.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Listings.tsx`

**Problem:** `MessageThread` and `ConversationList` both use `useSuspenseQuery`, but they have no `<Suspense>` boundary around them. When they are opened/mounted, React suspends and falls back to the nearest boundary (which is the route-level), causing a full-page flash. Same pattern with `ListingList` on filter change.

- [ ] **Step 1: Wrap MessageThread in ChatPanel with Suspense**

In `automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx`, import `Suspense` from `react` and wrap the `MessageThread` with a `<Suspense>` fallback that renders an inline spinner inside the panel body:

```tsx
import { Suspense } from "react";
// ... existing imports

const ChatPanel = ({ conversation, onClose }: ChatPanelProps) => {
  const { t } = useTranslation("chat");
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onClose]);

  return (
    <div className="border-border bg-card fixed inset-y-0 right-0 z-50 flex w-80 flex-col border-l shadow-xl lg:w-96">
      <div className="border-border flex items-center gap-3 border-b px-4 py-3">
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold">
            {conversation.counterpartUsername}
          </p>
          {conversation.listingTitle && (
            <p className="text-muted-foreground truncate text-xs">
              {conversation.listingTitle}
            </p>
          )}
        </div>
        <Button
          variant="ghost"
          size="sm"
          onClick={onClose}
          aria-label={t("chatPanel.closeChat")}
          className="text-muted-foreground hover:text-foreground h-auto p-1 leading-none"
        >
          ✕
        </Button>
      </div>
      <div className="flex-1 overflow-hidden">
        <Suspense
          fallback={
            <div className="flex h-full items-center justify-center">
              <div className="border-primary h-6 w-6 animate-spin rounded-full border-2 border-t-transparent" />
            </div>
          }
        >
          <MessageThread conversation={conversation} showListingCard={false} />
        </Suspense>
      </div>
    </div>
  );
};
```

- [ ] **Step 2: Wrap MessageThread in Inbox with Suspense**

In `automotive.marketplace.client/src/app/pages/Inbox.tsx`, import `Suspense` from `react` and wrap the `<MessageThread>` in the `<main>` element:

```tsx
import { ConversationList, MessageThread } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";
import { useNavigate } from "@tanstack/react-router";
import { Suspense, useState } from "react";
import { useTranslation } from "react-i18next";

// ... rest of component unchanged ...

      <main className="flex min-w-0 flex-1 flex-col">
        {selected ? (
          <Suspense
            fallback={
              <div className="flex h-full items-center justify-center">
                <div className="border-primary h-6 w-6 animate-spin rounded-full border-2 border-t-transparent" />
              </div>
            }
          >
            <MessageThread conversation={selected} />
          </Suspense>
        ) : (
          <div className="text-muted-foreground flex h-full items-center justify-center text-sm">
            {t("inbox.emptyState")}
          </div>
        )}
      </main>
```

- [ ] **Step 3: Wrap ListingList in Listings page with Suspense**

In `automotive.marketplace.client/src/app/pages/Listings.tsx`, import `Suspense` from `react` and wrap `<ListingList>` with a skeleton fallback:

```tsx
import { Suspense } from "react";
import { Route } from "@/app/routes/listings";
import { Filters, ListingList } from "@/features/listingList";

const ListingListSkeleton = () => (
  <div className="flex w-204 flex-col gap-10">
    {Array.from({ length: 3 }).map((_, i) => (
      <div key={i} className="bg-card border-border h-48 w-full animate-pulse border-1 rounded" />
    ))}
  </div>
);

const Listings = () => {
  const searchParams = Route.useSearch();
  const navigate = Route.useNavigate();

  return (
    <div className="mt-12 mb-24 flex w-full flex-col items-start justify-start space-x-12 md:flex-row">
      <div className="sticky top-4 mt-12 flex-1 self-start max-h-[calc(100vh-5rem)] overflow-y-auto">
        <Filters
          searchParams={searchParams}
          onSearchParamChange={(searchParams) =>
            navigate({ search: searchParams })
          }
        />
      </div>
      <Suspense fallback={<ListingListSkeleton />}>
        <ListingList listingSearchQuery={searchParams} />
      </Suspense>
    </div>
  );
};

export default Listings;
```

Note: The `max-h-[calc(100vh-5rem)] overflow-y-auto` on the filter wrapper also fixes Bug 3 (filter scrollability) in the same change.

- [ ] **Step 4: Verify — run frontend lint**

```bash
cd automotive.marketplace.client && npm run lint
```

Expected: no new errors.

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx
git add automotive.marketplace.client/src/app/pages/Inbox.tsx
git add automotive.marketplace.client/src/app/pages/Listings.tsx
git commit -m "fix: add Suspense boundaries to prevent full-page loading flash in chat and listings

- Wrap MessageThread in ChatPanel with inline spinner
- Wrap MessageThread in Inbox with inline spinner
- Wrap ListingList in Listings page with skeleton fallback
- Also make filter panel scrollable (max-h + overflow-y-auto)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: Filter models — only show models that have available listings

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ModelFeatures/GetModelsByMakeId/GetModelsByMakeIdQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ModelFeatures/GetModelsByMakeId/GetModelsByMakeIdQueryHandler.cs`
- Modify: `automotive.marketplace.client/src/features/listingList/components/ModelFilter.tsx`

**Problem:** `ModelFilter` shows all models for a make, most of which have no listings. Need to add an optional `onlyWithListings` flag to the backend query and use it in the frontend filter.

- [ ] **Step 1: Add `OnlyWithListings` to query**

In `Automotive.Marketplace.Application/Features/ModelFeatures/GetModelsByMakeId/GetModelsByMakeIdQuery.cs`:

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;

public sealed record GetModelsByMakeIdQuery : IRequest<IEnumerable<GetModelsByMakeIdResponse>>
{
    public Guid MakeId { get; set; }
    public bool OnlyWithListings { get; set; } = false;
};
```

- [ ] **Step 2: Apply filter in handler**

In `Automotive.Marketplace.Application/Features/ModelFeatures/GetModelsByMakeId/GetModelsByMakeIdQueryHandler.cs`:

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;

public class GetModelsByMakeIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetModelsByMakeIdQuery, IEnumerable<GetModelsByMakeIdResponse>>
{
    public async Task<IEnumerable<GetModelsByMakeIdResponse>> Handle(GetModelsByMakeIdQuery request, CancellationToken cancellationToken)
    {
        var query = repository
            .AsQueryable<Model>()
            .Where(model => model.MakeId == request.MakeId);

        if (request.OnlyWithListings)
        {
            query = query.Where(model =>
                model.Variants.Any(v => v.Listings.Any(l => l.Status == Status.Available)));
        }

        var models = await query
            .OrderBy(model => model.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetModelsByMakeIdResponse>>(models);
    }
}
```

- [ ] **Step 3: Pass `onlyWithListings: true` from ModelFilter**

In `automotive.marketplace.client/src/features/listingList/components/ModelFilter.tsx`, update the `getModelsByMakeIdOptions` call to pass `onlyWithListings: true`:

```tsx
import { getModelsByMakeIdOptions } from "@/api/model/getModelsByMakeIdOptions";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { CheckedState } from "@radix-ui/react-checkbox";
import { useQuery } from "@tanstack/react-query";

type ModelFilterProps = {
  makeId: string;
  filteredModels: string[];
  onFilterChange: (value: string[]) => Promise<void>;
};

const ModelFilter = ({
  makeId,
  filteredModels,
  onFilterChange,
}: ModelFilterProps) => {
  const { data: modelsQuery } = useQuery({
    ...getModelsByMakeIdOptions({ makeId, onlyWithListings: true }),
    enabled: !!makeId,
  });

  const models = modelsQuery?.data || [];

  const handleCheckedChange = async (
    checked: CheckedState,
    modelId: string,
  ) => {
    const updatedFilteredModels =
      checked === true
        ? [...filteredModels, modelId]
        : filteredModels.filter((value) => value !== modelId);

    await onFilterChange(updatedFilteredModels);
  };

  return (
    <div className="columns-1 space-y-6">
      {models.map((model) => (
        <div key={model.id} className="flex items-center space-x-4">
          <Checkbox
            id={model.id}
            value={model.id}
            checked={filteredModels.includes(model.id)}
            onCheckedChange={(checked) =>
              handleCheckedChange(checked, model.id)
            }
          />
          <Label htmlFor={model.id}>{model.name}</Label>
        </div>
      ))}
    </div>
  );
};

export default ModelFilter;
```

- [ ] **Step 4: Update the frontend API query type**

In `automotive.marketplace.client/src/api/model/getModelsByMakeIdOptions.ts`, check the `GetModelsByMakeIdQuery` type includes `onlyWithListings`. Look at `src/types/model/GetModelsByMakeIdQuery.ts`:

```ts
// Expected content — add onlyWithListings if missing:
export type GetModelsByMakeIdQuery = {
  makeId: string;
  onlyWithListings?: boolean;
};
```

- [ ] **Step 5: Build backend to verify no errors**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -20
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ModelFeatures/GetModelsByMakeId/
git add automotive.marketplace.client/src/features/listingList/components/ModelFilter.tsx
git add automotive.marketplace.client/src/types/model/GetModelsByMakeIdQuery.ts
git commit -m "fix: only show models with available listings in model filter

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: Fix pagination — sync page number with URL

**Files:**
- Modify: `automotive.marketplace.client/src/features/search/schemas/listingSearchSchema.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/components/ListingList.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Listings.tsx`

**Problem:** `ListingList` stores `page` in local `useState`. Changing page works in-memory but the URL doesn't update, and the page selection doesn't persist on navigation. Fix: add `page` to the URL search schema and propagate it via props.

- [ ] **Step 1: Add page to search schema**

In `automotive.marketplace.client/src/features/search/schemas/listingSearchSchema.ts`:

```ts
import { VALIDATION } from "@/constants/validation";
import { z } from "zod";

export const ListingSearchSchema = z.object({
  makeId: z.string().regex(VALIDATION.GUID.REGEX).optional().catch(undefined),
  models: z
    .array(z.string().regex(VALIDATION.GUID.REGEX).catch(""))
    .optional()
    .catch(undefined),
  municipalityId: z
    .string()
    .regex(VALIDATION.GUID.REGEX)
    .optional()
    .catch(undefined),
  isUsed: z.boolean().optional().catch(undefined),
  minYear: z.coerce.number().positive().optional().catch(undefined),
  maxYear: z.coerce.number().positive().optional().catch(undefined),
  minPrice: z.coerce.number().positive().optional().catch(undefined),
  maxPrice: z.coerce.number().positive().optional().catch(undefined),
  minMileage: z.coerce.number().positive().optional().catch(undefined),
  maxMileage: z.coerce.number().positive().optional().catch(undefined),
  minPower: z.coerce.number().positive().optional().catch(undefined),
  maxPower: z.coerce.number().positive().optional().catch(undefined),
  page: z.coerce.number().int().positive().optional().catch(undefined),
});
```

- [ ] **Step 2: Update ListingList to accept page/onPageChange props**

In `automotive.marketplace.client/src/features/listingList/components/ListingList.tsx`:

```tsx
import { useSuspenseQuery } from "@tanstack/react-query";
import { Pagination } from "@/components/ui/Pagination";
import { getAllListingsOptions } from "../api/getAllListingsOptions";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import ListingCard from "./ListingCard";

type ListingListProps = {
  listingSearchQuery: GetAllListingsQuery;
  page: number;
  onPageChange: (page: number) => void;
};

const PAGE_SIZE = 20;

const ListingList = ({ listingSearchQuery, page, onPageChange }: ListingListProps) => {
  const { data: listingsQuery } = useSuspenseQuery(
    getAllListingsOptions({ ...listingSearchQuery, page, pageSize: PAGE_SIZE }),
  );

  const listings = listingsQuery.data.items;
  const totalPages = listingsQuery.data.totalPages;

  return (
    <div className="bg-background text-on-background flex w-204 flex-col gap-10">
      {listings.map((l) => (
        <ListingCard key={l.id} listing={l} />
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />
    </div>
  );
};

export default ListingList;
```

- [ ] **Step 3: Update Listings page to read page from URL and reset on filter change**

In `automotive.marketplace.client/src/app/pages/Listings.tsx`. The page is now read from `searchParams.page ?? 1`. When filters change via `onSearchParamChange`, include `page: 1` to reset pagination. When page changes via `onPageChange`, navigate with the new page number preserving all other search params:

```tsx
import { Suspense } from "react";
import { Route } from "@/app/routes/listings";
import { Filters, ListingList } from "@/features/listingList";

const ListingListSkeleton = () => (
  <div className="flex w-204 flex-col gap-10">
    {Array.from({ length: 3 }).map((_, i) => (
      <div key={i} className="bg-card border-border h-48 w-full animate-pulse rounded border-1" />
    ))}
  </div>
);

const Listings = () => {
  const searchParams = Route.useSearch();
  const navigate = Route.useNavigate();

  const currentPage = searchParams.page ?? 1;

  return (
    <div className="mt-12 mb-24 flex w-full flex-col items-start justify-start space-x-12 md:flex-row">
      <div className="sticky top-4 mt-12 max-h-[calc(100vh-5rem)] flex-1 self-start overflow-y-auto">
        <Filters
          searchParams={searchParams}
          onSearchParamChange={(updatedParams) =>
            navigate({ search: { ...updatedParams, page: 1 } })
          }
        />
      </div>
      <Suspense fallback={<ListingListSkeleton />}>
        <ListingList
          listingSearchQuery={searchParams}
          page={currentPage}
          onPageChange={(p) => navigate({ search: { ...searchParams, page: p } })}
        />
      </Suspense>
    </div>
  );
};

export default Listings;
```

- [ ] **Step 4: Run lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

Expected: no new errors.

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/search/schemas/listingSearchSchema.ts
git add automotive.marketplace.client/src/features/listingList/components/ListingList.tsx
git add automotive.marketplace.client/src/app/pages/Listings.tsx
git commit -m "fix: sync pagination page number with URL search params

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: Guest listing creation — allow access with a warning banner

**Files:**
- Modify: `automotive.marketplace.client/src/app/routes/listing/create.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/CreateListing.tsx`
- Modify: `automotive.marketplace.client/src/features/createListing/components/CreateListingForm.tsx`

**Problem:** The route `beforeLoad` redirects unauthenticated users to `/login` before they can see the form. Guests should be able to view the form but not submit it, with a warning banner explaining they must create an account.

- [ ] **Step 1: Remove redirect from route**

In `automotive.marketplace.client/src/app/routes/listing/create.tsx`:

```tsx
import CreateListing from "@/app/pages/CreateListing";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listing/create")({
  component: CreateListing,
});
```

- [ ] **Step 2: Add warning banner in CreateListing page**

In `automotive.marketplace.client/src/app/pages/CreateListing.tsx`:

```tsx
import { selectAccessToken } from "@/features/auth";
import { CreateListingForm } from "@/features/createListing";
import { useAppSelector } from "@/hooks/redux";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

const CreateListing = () => {
  const { t } = useTranslation("listings");
  const accessToken = useAppSelector(selectAccessToken);
  const isGuest = !accessToken;

  return (
    <div className="my-24 flex flex-col items-center space-y-12">
      <h1 className="text-xl font-semibold">{t("createPage.title")}</h1>
      {isGuest && (
        <div className="w-full max-w-2xl rounded-lg border border-yellow-400 bg-yellow-50 px-4 py-3 text-sm text-yellow-800 dark:border-yellow-600 dark:bg-yellow-900/30 dark:text-yellow-200">
          {t("createPage.guestWarning")}{" "}
          <Link to="/register" className="font-semibold underline">
            {t("createPage.guestWarningLink")}
          </Link>
        </div>
      )}
      <CreateListingForm
        className="border-border bg-card w-full rounded-2xl border-1 p-4"
        disabled={isGuest}
      />
    </div>
  );
};

export default CreateListing;
```

- [ ] **Step 3: Add `disabled` prop to CreateListingForm and disable submit**

Open `automotive.marketplace.client/src/features/createListing/components/CreateListingForm.tsx`. Find the submit button and the `className` prop. Add a `disabled?: boolean` prop to the form component. If `disabled` is true, the submit button should be disabled and show a tooltip-like hint.

Check the current signature and add the prop:

```tsx
// Find the type definition for CreateListingForm props and add:
type CreateListingFormProps = {
  className?: string;
  disabled?: boolean;
};
```

Then find the submit button and add `disabled={props.disabled}` to it (use the actual prop name from the component). It should look like:

```tsx
<Button type="submit" disabled={disabled || form.formState.isSubmitting}>
  {t("createPage.submit")}
</Button>
```

- [ ] **Step 4: Add i18n keys**

Find the listings translation file(s). Look in `automotive.marketplace.client/src/` for i18n json files:

```bash
find automotive.marketplace.client/src -name "*.json" -path "*/i18n/*" | head -10
# or
find automotive.marketplace.client/src -name "listings*.json" | head -10
```

Add these keys to the listings namespace (Lithuanian first, then English if separate):
- `createPage.guestWarning`: "Negalėsite pateikti skelbimo, kol nesukursite paskyros."
- `createPage.guestWarningLink`: "Sukurkite paskyrą"

- [ ] **Step 5: Run lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/app/routes/listing/create.tsx
git add automotive.marketplace.client/src/app/pages/CreateListing.tsx
git add automotive.marketplace.client/src/features/createListing/components/CreateListingForm.tsx
git commit -m "fix: allow guests to view listing creation page with warning banner

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: Cardog API for seller insights — call API when cache is missing

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/GetSellerListingInsightsQueryHandler.cs`

**Problem:** `GetSellerListingInsightsQueryHandler` only reads from `VehicleMarketCache` but never calls the Cardog API to populate it. The market data section shows "No market data available" even when `EnableVehicleScoring` is true in user preferences. Fix: check user preferences; if `EnableVehicleScoring` is true and no valid cache exists, call the Cardog API and upsert the cache (same pattern as `GetListingScoreQueryHandler`).

- [ ] **Step 1: Update handler to inject ICardogApiClient and call API when needed**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;

public class GetSellerListingInsightsQueryHandler(
    IRepository repository,
    ICardogApiClient cardogClient)
    : IRequestHandler<GetSellerListingInsightsQuery, GetSellerListingInsightsResponse>
{
    private const int MarketCacheDays = 1;
    private const int MarketFailureCacheHours = 2;

    public async Task<GetSellerListingInsightsResponse> Handle(GetSellerListingInsightsQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Model).ThenInclude(m => m.Make)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        if (listing.SellerId != request.UserId)
            throw new UnauthorizedAccessException("Access denied. You are not the seller of this listing.");

        var make = listing.Variant.Model.Make.Name;
        var model = listing.Variant.Model.Name;
        var year = listing.Year;

        var prefs = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        var marketCache = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(
                c => c.Make == make && c.Model == model && c.Year == year && c.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (marketCache == null && (prefs?.EnableVehicleScoring ?? false))
        {
            var result = await cardogClient.GetMarketOverviewAsync(make, model, year, cancellationToken);
            await UpsertMarketCacheAsync(make, model, year, result, cancellationToken);

            if (result != null)
            {
                marketCache = new VehicleMarketCache
                {
                    Make = make,
                    Model = model,
                    Year = year,
                    MedianPrice = result.MedianPrice,
                    TotalListings = result.TotalListings,
                    IsFetchFailed = false,
                    FetchedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(MarketCacheDays),
                };
            }
        }

        var marketPosition = BuildMarketPosition(listing, marketCache);
        var listingQuality = BuildListingQuality(listing);

        return new GetSellerListingInsightsResponse
        {
            MarketPosition = marketPosition,
            ListingQuality = listingQuality,
        };
    }

    private async Task UpsertMarketCacheAsync(string make, string model, int year, CardogMarketResult? result, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var isFailed = result is null;
        var expiry = isFailed ? now.AddHours(MarketFailureCacheHours) : now.AddDays(MarketCacheDays);

        var existing = await repository.AsQueryable<VehicleMarketCache>()
            .FirstOrDefaultAsync(c => c.Make == make && c.Model == model && c.Year == year, ct);

        if (existing != null)
        {
            existing.MedianPrice = result?.MedianPrice ?? 0;
            existing.TotalListings = result?.TotalListings ?? 0;
            existing.IsFetchFailed = isFailed;
            existing.FetchedAt = now;
            existing.ExpiresAt = expiry;
            await repository.UpdateAsync(existing, ct);
        }
        else
        {
            try
            {
                await repository.CreateAsync(new VehicleMarketCache
                {
                    Id = Guid.NewGuid(),
                    Make = make, Model = model, Year = year,
                    MedianPrice = result?.MedianPrice ?? 0,
                    TotalListings = result?.TotalListings ?? 0,
                    IsFetchFailed = isFailed,
                    FetchedAt = now,
                    ExpiresAt = expiry,
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert — ignore
            }
        }
    }

    private static MarketPositionInsight BuildMarketPosition(Listing listing, VehicleMarketCache? cache)
    {
        double? priceDiff = null;
        if (cache != null && cache.MedianPrice > 0)
            priceDiff = (double)((cache.MedianPrice - listing.Price) / cache.MedianPrice) * 100.0;

        return new MarketPositionInsight
        {
            ListingPrice = listing.Price,
            MarketMedianPrice = cache?.MedianPrice,
            PriceDifferencePercent = priceDiff,
            MarketListingCount = cache?.TotalListings,
            DaysListed = (int)(DateTime.UtcNow - listing.CreatedAt).TotalDays,
            HasMarketData = cache != null && !cache.IsFetchFailed,
        };
    }

    private static ListingQualityInsight BuildListingQuality(Listing listing)
    {
        var points = 0;
        var suggestions = new List<string>();
        var photoCount = listing.Images.Count;
        var hasDescription = !string.IsNullOrWhiteSpace(listing.Description) && listing.Description.Length >= 20;
        var hasPhotos = photoCount >= 3;
        var hasVin = !string.IsNullOrWhiteSpace(listing.Vin);
        var hasColour = !string.IsNullOrWhiteSpace(listing.Colour);

        if (hasDescription)
        {
            points += 20;
            if (listing.Description!.Length >= 100) points += 10;
        }
        else
        {
            suggestions.Add("Add a detailed description to attract more buyers.");
        }

        if (hasPhotos)
        {
            points += 20;
            if (photoCount >= 5) points += 10;
        }
        else
        {
            suggestions.Add("Add at least 3 photos to significantly improve visibility.");
        }

        if (hasVin) points += 15;
        else suggestions.Add("Include the VIN to build buyer confidence.");

        if (hasColour) points += 10;
        else suggestions.Add("Specify the colour to help buyers filter listings.");

        var qualityScore = (int)Math.Round(Math.Min(points, 90) / 90.0 * 100);

        return new ListingQualityInsight
        {
            QualityScore = qualityScore,
            HasDescription = hasDescription,
            HasPhotos = hasPhotos,
            PhotoCount = photoCount,
            HasVin = hasVin,
            HasColour = hasColour,
            Suggestions = suggestions,
        };
    }
}
```

Note: Also update `HasMarketData` in `BuildMarketPosition` to check `!cache.IsFetchFailed` (was previously just `cache != null`).

- [ ] **Step 2: Check ICardogApiClient namespace and CardogMarketResult type**

```bash
grep -r "CardogMarketResult\|ICardogApiClient" Automotive.Marketplace.Application/Interfaces --include="*.cs" -n
```

Confirm the type name and that it has `MedianPrice` and `TotalListings` properties.

- [ ] **Step 3: Build to verify**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -20
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetSellerListingInsights/
git commit -m "fix: call Cardog API in seller insights when EnableVehicleScoring is enabled

Previously the handler only read from VehicleMarketCache and never called
the Cardog API directly. Now it calls the API if cache is missing and
EnableVehicleScoring preference is true, matching the pattern used
by GetListingScoreQueryHandler.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: Hide like button for the listing's own seller

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx`

**Problem:** The like button is shown to any logged-in user including the seller of the listing. It should be hidden when the current user is the seller.

The `GetAllListingsResponse` already has `SellerId` mapped as `sellerId` in the response. The frontend `GetAllListingsResponse` type includes `sellerId`.

- [ ] **Step 1: Get current userId from Redux and hide like button for seller**

In `automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx`, import `useAppSelector` and the auth slice selector to get `userId`. Hide the like button when `userId === listing.sellerId`:

```tsx
import { Button } from "@/components/ui/button";
import { selectAccessToken } from "@/features/auth";
// eslint-disable-next-line no-restricted-imports
import { useToggleLike } from "@/features/savedListings/api/useToggleLike";
import { useAppSelector } from "@/hooks/redux";
import { router } from "@/lib/router";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
import { IoLocationOutline } from "react-icons/io5";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { PiEngine } from "react-icons/pi";
import { TbManualGearbox } from "react-icons/tb";
import { IoHeartOutline, IoHeart } from "react-icons/io5";
import { useTranslation } from "react-i18next";
import { GetAllListingsResponse } from "../types/GetAllListingsResponse";
import { translateVehicleAttr } from "../utils/translateVehicleAttr";
import ListingCardBadge from "./ListingCardBadge";
import ImageHoverGallery from "@/components/gallery/ImageHoverGallery";

interface ListingCardProps {
  listing: GetAllListingsResponse;
}

const ListingCard = ({ listing }: ListingCardProps) => {
  const { t } = useTranslation("listings");
  const accessToken = useAppSelector(selectAccessToken);
  const userId = useAppSelector((s) => s.auth.userId);
  const toggleLike = useToggleLike();

  const handleClick = async () => {
    await router.navigate({ to: "/listing/$id", params: { id: listing.id } });
  };

  const handleLikeClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleLike.mutate({ listingId: listing.id });
  };

  const isSeller = userId && listing.sellerId && userId.toLowerCase() === listing.sellerId.toLowerCase();
  const showLikeButton = accessToken && !isSeller;

  return (
    <div className="bg-card border-border grid w-full gap-8 border-1 md:grid-cols-2">
      <div className="group relative flex flex-shrink-0 py-5">
        <ImageHoverGallery
          images={listing.images}
          className="aspect-[4/3] w-full"
        />
        {showLikeButton && (
          <button
            onClick={handleLikeClick}
            className={`absolute top-7 left-2 flex h-9 w-9 items-center justify-center rounded-full transition-opacity ${
              listing.isLiked
                ? "bg-red-500 opacity-100"
                : "bg-black/50 opacity-0 group-hover:opacity-100"
            }`}
          >
            {listing.isLiked ? (
              <IoHeart className="h-5 w-5 text-white" />
            ) : (
              <IoHeartOutline className="h-5 w-5 text-white" />
            )}
          </button>
        )}
      </div>
      {/* ... rest of JSX unchanged ... */}
    </div>
  );
};
```

Important: Check what type `userId` is in auth slice (string UUID or Guid). Compare case-insensitively. Check if `listing.sellerId` is a `string` or a UUID object in the FE type — look at `GetAllListingsResponse` frontend type.

- [ ] **Step 2: Verify `sellerId` is in frontend GetAllListingsResponse type**

```bash
grep -r "sellerId\|SellerId" automotive.marketplace.client/src/features/listingList/types/ -n
```

If `sellerId` is missing from the frontend type, add it:

```ts
// In GetAllListingsResponse.ts:
export type GetAllListingsResponse = {
  // ... existing fields
  sellerId: string;
  // ...
};
```

- [ ] **Step 3: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx
git add automotive.marketplace.client/src/features/listingList/types/GetAllListingsResponse.ts
git commit -m "fix: hide like button on listings owned by the current user

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 7: Saved listing card — make title clickable to navigate to listing details

**Files:**
- Modify: `automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx`
- Modify: `automotive.marketplace.client/src/features/savedListings/types/SavedListing.ts` (if `listingId` maps to the listing's route ID)

**Problem:** The saved listing row displays the title as plain text. Clicking it should navigate to the listing details page at `/listing/$id`.

- [ ] **Step 1: Check the SavedListing type for the listing route ID**

```bash
cat automotive.marketplace.client/src/features/savedListings/types/SavedListing.ts
```

Confirm which field holds the listing ID used by the `/listing/$id` route. It is likely `listingId`.

- [ ] **Step 2: Make title a clickable link in SavedListingRow**

In `automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx`, import `router` from `@/lib/router` and make the title paragraph a clickable button that navigates to the listing:

```tsx
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { IoHeart } from "react-icons/io5";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import { router } from "@/lib/router";
import { useToggleLike } from "../api/useToggleLike";
import type { SavedListing } from "../types/SavedListing";
import NoteEditor from "./NoteEditor";

interface SavedListingRowProps {
  listing: SavedListing;
}

const SavedListingRow = ({ listing }: SavedListingRowProps) => {
  const { t } = useTranslation("saved");
  const { t: tListings } = useTranslation("listings");
  const [isHovered, setIsHovered] = useState(false);
  const toggleLike = useToggleLike();

  const handleUnlike = () => {
    toggleLike.mutate({ listingId: listing.listingId });
  };

  const handleTitleClick = () => {
    void router.navigate({ to: "/listing/$id", params: { id: listing.listingId } });
  };

  return (
    <div
      className="border-border hover:bg-muted/50 flex gap-4 border-b p-4 transition-colors"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Thumbnail */}
      <div className="h-20 w-28 flex-shrink-0 overflow-hidden rounded">
        {listing.thumbnail ? (
          <img
            src={listing.thumbnail.url}
            alt={listing.thumbnail.altText}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="bg-muted flex h-full w-full items-center justify-center text-xs">
            {t("row.noImage")}
          </div>
        )}
      </div>

      {/* Content */}
      <div className="flex min-w-0 flex-1 flex-col">
        <div className="flex items-start justify-between">
          <div className="min-w-0">
            <button
              onClick={handleTitleClick}
              className="hover:text-primary truncate text-left font-medium hover:underline"
            >
              {listing.title}
            </button>
            <p className="text-muted-foreground text-sm">
              {formatCurrency(listing.price)} € · {listing.municipalityName} ·{" "}
              {formatNumber(listing.mileage)} km ·{" "}
              {translateVehicleAttr("fuel", listing.fuelName, tListings)} ·{" "}
              {translateVehicleAttr("transmission", listing.transmissionName, tListings)}
            </p>
          </div>
          <button
            onClick={handleUnlike}
            className="ml-2 flex-shrink-0 text-red-500 transition-opacity hover:opacity-70"
            title={t("row.removeFromSaved")}
          >
            <IoHeart className="h-5 w-5" />
          </button>
        </div>

        <NoteEditor listing={listing} isExpanded={isHovered} />
      </div>
    </div>
  );
};

export default SavedListingRow;
```

- [ ] **Step 3: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx
git commit -m "fix: make saved listing title clickable to navigate to listing details

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 8: Remove meeting location UI from ProposeMeetingModal and MeetingCard

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`

**Problem:** Meeting creation and display includes location fields/data that should be temporarily hidden until the feature is needed.

- [ ] **Step 1: Remove location fields from ProposeMeetingModal**

In `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx`, remove:
- `locationText` state and `useState`
- `showCoords` state
- `lat`, `lng` states
- Location `<Input>` and `<Label>` block
- "Set pin" `<Button>` and `showCoords` coordinates block
- `locationText`, `locationLat`, `locationLng` from the `onSubmit` data
- `MapPin` import from lucide-react

The type `onSubmit` prop should remove the location fields:

```tsx
type ProposeMeetingModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: "propose" | "reschedule";
  initialMeeting?: Meeting;
  onSubmit: (data: {
    proposedAt: string;
    durationMinutes: number;
  }) => void;
};
```

The `handleSubmit` becomes:
```tsx
const handleSubmit = () => {
  if (!isValid || !proposedAt) return;
  onSubmit({
    proposedAt: proposedAt.toISOString(),
    durationMinutes: duration,
  });
  onOpenChange(false);
};
```

The component state becomes:
```tsx
const [date, setDate] = useState(defaultDate);
const [time, setTime] = useState(defaultTime);
const [duration, setDuration] = useState(initialMeeting?.durationMinutes ?? 60);
```

- [ ] **Step 2: Remove location display from MeetingCard**

In `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx`:
- Remove the `{meeting.locationText && (...)}` block in the rendered JSX
- Remove `MapPin` import from lucide-react if it's only used there

Also update the `onReschedule` prop type in `MeetingCardProps` to no longer include location fields:

```tsx
onReschedule: (
  meetingId: string,
  data: {
    proposedAt: string;
    durationMinutes: number;
  },
) => void;
```

- [ ] **Step 3: Remove location from sticky bar in MessageThread**

In `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`, remove the location display from the sticky accepted meeting bar. Find and remove:

```tsx
{acceptedMeeting.locationText && (
  <>
    <span className="text-green-300">·</span>
    <MapPin className="h-3 w-3 shrink-0" />
    <span className="truncate">{acceptedMeeting.locationText}</span>
  </>
)}
```

Remove `MapPin` from the lucide-react import if no longer used.

Also update the `onReschedule` call in `MessageThread` to not pass location fields:

```tsx
onReschedule={(meetingId, data) =>
  respondToMeeting({
    meetingId,
    action: "Reschedule",
    rescheduleData: data,
  })
}
```

Check that `respondToMeeting` / `RescheduleData` type in the chat hub types accepts `{ proposedAt, durationMinutes }` without location — they likely have optional location fields already, so no type change needed there.

- [ ] **Step 4: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx
git add automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx
git add automotive.marketplace.client/src/features/chat/components/MessageThread.tsx
git commit -m "fix: remove meeting location UI from proposal modal, meeting card, and sticky bar

Location feature is temporarily disabled until implementation is complete.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 9: Fix Compare AI summary regeneration to bypass cache

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`

**Problem:** `handleRegenerate` sets `forceRegenerate = true` then immediately calls `refetch()`. Because React state updates are async, the `refetch()` runs with the old `forceRegenerate = false` captured in the closure of the current query options. The fix: use `useQueryClient().fetchQuery(...)` with `forceRegenerate: true` explicitly passed — this bypasses the issue since it constructs fresh query options.

- [ ] **Step 1: Replace forceRegenerate state with queryClient.fetchQuery**

In `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`:

```tsx
import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Sparkles, RefreshCw, Info } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { getListingComparisonAiSummaryOptions } from "../api/getListingComparisonAiSummaryOptions";

type Props = {
  listingAId: string;
  listingBId: string;
};

const factorTranslationKeys: Record<string, string> = {
  MarketValue: "score.value",
  Efficiency: "score.efficiency",
  Reliability: "score.reliability",
};

export function CompareAiSummary({ listingAId, listingBId }: Props) {
  const { t, i18n } = useTranslation("compare");
  const { t: tPrefs } = useTranslation("userPreferences");
  const queryClient = useQueryClient();

  const [isForceFetching, setIsForceFetching] = useState(false);

  const { data, isFetching } = useQuery(
    getListingComparisonAiSummaryOptions(listingAId, listingBId, i18n.language),
  );

  const summary = data?.data;
  const hasResult = summary?.isGenerated;
  const unavailable = summary?.unavailableFactors ?? [];
  const translatedUnavailable = unavailable.map((f) =>
    tPrefs(factorTranslationKeys[f] ?? f),
  );

  const handleRegenerate = async () => {
    setIsForceFetching(true);
    try {
      await queryClient.fetchQuery(
        getListingComparisonAiSummaryOptions(listingAId, listingBId, i18n.language, true),
      );
    } finally {
      setIsForceFetching(false);
    }
  };

  const isLoading = isFetching || isForceFetching;

  return (
    <div className="border-border mb-4 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">{t("aiSummary.title")}</span>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={handleRegenerate}
          disabled={isLoading}
          className="flex items-center gap-1"
        >
          {isLoading ? (
            <RefreshCw className="h-3.5 w-3.5 animate-spin" />
          ) : (
            <Sparkles className="h-3.5 w-3.5" />
          )}
          {hasResult ? t("aiSummary.regenerate") : t("aiSummary.generate")}
        </Button>
      </div>

      {isLoading && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
        </div>
      )}

      {!isLoading && hasResult && summary?.summary && (
        <>
          <p className="text-muted-foreground mt-3 text-sm leading-relaxed">
            {summary.summary}
          </p>
          {unavailable.length > 0 && (
            <Alert variant="default" className="mt-3">
              <Info className="h-4 w-4" />
              <AlertDescription className="text-xs">
                {t("aiSummary.unavailableFactors", {
                  factors: translatedUnavailable.join(", "),
                })}
              </AlertDescription>
            </Alert>
          )}
        </>
      )}

      {!isLoading && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          {t("aiSummary.prompt")}
        </p>
      )}
    </div>
  );
}
```

Key changes:
- Removed `forceRegenerate` state
- `useQuery` now uses default options (no `forceRegenerate` arg — defaults to `false`)
- `handleRegenerate` calls `queryClient.fetchQuery(options with forceRegenerate: true)` which:
  1. Sends `forceRegenerate: true` to the server
  2. Stores the response under the same cache key (which excludes `forceRegenerate`)
  3. The `useQuery` above auto-updates from cache

Also update `getListingComparisonAiSummaryOptions` in the API file to remove `forceRegenerate` from the default call (no change needed, it already defaults to `false`).

- [ ] **Step 2: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -20
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx
git commit -m "fix: correctly bypass cache when regenerating compare AI summary

Previous approach set forceRegenerate state then called refetch(),
which ran with stale forceRegenerate=false due to React's async state.
Now uses queryClient.fetchQuery() with forceRegenerate=true directly,
which sends the flag to the server and updates the query cache.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Final Verification

- [ ] **Run backend build**

```bash
dotnet build ./Automotive.Marketplace.sln 2>&1 | tail -10
```

- [ ] **Run frontend lint and build**

```bash
cd automotive.marketplace.client && npm run lint && npm run build 2>&1 | tail -20
```

Expected: clean lint, successful build.
