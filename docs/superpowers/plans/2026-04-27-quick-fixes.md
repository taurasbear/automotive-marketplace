# Quick Fixes Bundle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 11 small-to-medium independent bugs across auth, listings, chat, and search.

**Architecture:** Pure bug fixes with minimal surface area — each task touches 1–3 files. No new backend entities or migrations required except Task 10 (AI regenerate adds one query param).

**Tech Stack:** React 19 + TypeScript (frontend), ASP.NET Core 8 + C# (backend), TanStack Router, React Hook Form / Zod, TanStack Query.

---

### Task 1: Reveal Password Button on Login and Register

**Files:**
- Modify: `automotive.marketplace.client/src/app/pages/Login.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Register.tsx`

- [ ] **Step 1: Update Login.tsx**

Add `useState` import already present. Add `showPassword` state and Eye icon toggle button:

```tsx
// At top of file — add Eye, EyeOff to lucide-react import
import { Eye, EyeOff } from "lucide-react";

// Inside the Login component
const [showPassword, setShowPassword] = useState(false);

// Replace the password FormField's Input:
<FormControl>
  <div className="relative">
    <Input
      type={showPassword ? "text" : "password"}
      placeholder={t("login.fields.passwordPlaceholder")}
      {...field}
    />
    <button
      type="button"
      onClick={() => setShowPassword((p) => !p)}
      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
      tabIndex={-1}
    >
      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
    </button>
  </div>
</FormControl>
```

- [ ] **Step 2: Update Register.tsx with identical pattern**

```tsx
// Add to imports
import { Eye, EyeOff } from "lucide-react";

// Inside Register component
const [showPassword, setShowPassword] = useState(false);

// Replace the password FormField's Input the same way as Login above
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/app/pages/Login.tsx automotive.marketplace.client/src/app/pages/Register.tsx
git commit -m "feat: add reveal password toggle to login and register forms

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Update AutoMapper

**Files:**
- Modify: `Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj`

- [ ] **Step 1: Update the package**

```bash
cd /path/to/repo
dotnet add Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj package AutoMapper --version 14.0.0
```

Check https://www.nuget.org/packages/AutoMapper for the latest non-vulnerable version (currently 14.0.0 is safe; if a newer version exists use that). The CVE is in versions prior to 13.0.1. Version 14.x is safe.

Actually, first check what's available:
```bash
dotnet package search AutoMapper --take 5
```
Use the latest stable version shown. If 14.0.0 is already the latest, no change needed — but confirm it is not listed in any audit:

```bash
dotnet restore && dotnet list package --vulnerable
```

- [ ] **Step 2: Run backend build**

```bash
dotnet build ./Automotive.Marketplace.sln
```
Expected: build succeeds.

- [ ] **Step 3: Run tests**

```bash
dotnet test ./Automotive.Marketplace.sln
```
Expected: all tests pass.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj
git commit -m "chore: update AutoMapper to latest version

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Redirect to Home on 403 Forbidden

**Files:**
- Modify: `automotive.marketplace.client/src/lib/axios/axiosErrorHandler.ts`

- [ ] **Step 1: Add 403 handling**

In `handleAxiosError`, add a 403 case that redirects to `/` with a toast:

```ts
// In handleAxiosError, after the status === 401 branch:
if (status === 403) {
  if (!isRedirecting) {
    isRedirecting = true;
    toast.error("Access denied.");
    await router.navigate({ to: "/" });
    isRedirecting = false;
  }
  return Promise.reject(error);
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/lib/axios/axiosErrorHandler.ts
git commit -m "fix: redirect to home on 403 forbidden response

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Sticky Filters Sidebar on Listings Page

**Files:**
- Modify: `automotive.marketplace.client/src/app/pages/Listings.tsx`

- [ ] **Step 1: Make filter sidebar sticky**

The filter sidebar has `mt-48` which is a large top margin. Change the filter wrapper div to be sticky:

```tsx
// Change the div wrapping <Filters> from:
<div className="mt-48 flex-1">

// To:
<div className="sticky top-4 mt-12 flex-1 self-start">
```

The `self-start` prevents the sticky container from stretching to the row height; `top-4` gives a small gap from the viewport top when stuck.

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/app/pages/Listings.tsx
git commit -m "fix: make filters sidebar sticky on scroll

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Prevent Guests from Submitting the Create Listing Form

**Files:**
- Modify: `automotive.marketplace.client/src/app/routes/listing/create.tsx`

- [ ] **Step 1: Add beforeLoad auth guard**

The pattern is already established in `settings.tsx` and `saved.tsx`:

```tsx
import CreateListing from "@/app/pages/CreateListing";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { store } from "@/lib/redux/store";

export const Route = createFileRoute("/listing/create")({
  beforeLoad: () => {
    const { auth } = store.getState();
    if (!auth.userId) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error
      throw redirect({ to: "/login" });
    }
  },
  component: CreateListing,
});
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/app/routes/listing/create.tsx
git commit -m "fix: require authentication to access create listing page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Prevent Liking Your Own Listings

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommandHandler.cs`

- [ ] **Step 1: Load the listing and check SellerId**

The `ToggleLikeCommand` has `UserId` and `ListingId`. Load the `Listing` to get `SellerId`, then throw if they match:

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public class ToggleLikeCommandHandler(IRepository repository)
    : IRequestHandler<ToggleLikeCommand, ToggleLikeResponse>
{
    public async Task<ToggleLikeResponse> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var listing = await repository
            .AsQueryable<Listing>()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException("Listing", request.ListingId);

        if (listing.SellerId == request.UserId)
            throw new ValidationException("You cannot like your own listing.");

        var existingLike = await repository
            .AsQueryable<UserListingLike>()
            .FirstOrDefaultAsync(
                like => like.UserId == request.UserId && like.ListingId == request.ListingId,
                cancellationToken);

        if (existingLike is not null)
        {
            var existingNote = await repository
                .AsQueryable<UserListingNote>()
                .FirstOrDefaultAsync(
                    note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                    cancellationToken);

            if (existingNote is not null)
                await repository.DeleteAsync(existingNote, cancellationToken);

            await repository.DeleteAsync(existingLike, cancellationToken);
            return new ToggleLikeResponse { IsLiked = false };
        }

        var newLike = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ListingId = request.ListingId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId.ToString()
        };

        await repository.CreateAsync(newLike, cancellationToken);
        return new ToggleLikeResponse { IsLiked = true };
    }
}
```

> Note: Check how `ValidationException` is defined in `Automotive.Marketplace.Application.Common.Exceptions`. If no such exception exists, use the one that maps to HTTP 400 (e.g., `FluentValidation.ValidationException` or a custom domain exception used elsewhere in the project). Look at other handlers for the correct exception type.

- [ ] **Step 2: Build backend**

```bash
dotnet build ./Automotive.Marketplace.sln
```

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommandHandler.cs
git commit -m "fix: prevent users from liking their own listings

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Invalidate Conversations List After Meeting Cancellation

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

**Context:** The `MEETING_CANCELLED` handler updates the message state and invalidates the messages query, but doesn't invalidate `chatKeys.conversations()`. Other events (offer status, etc.) DO invalidate conversations to refresh the last-message preview. Meeting cancellation needs to match.

- [ ] **Step 1: Add conversations invalidation**

Find the `MEETING_CANCELLED` handler in `useChatHub.ts`. After the existing `void queryClient.invalidateQueries({ queryKey: chatKeys.messages(payload.conversationId) })`, add:

```ts
void queryClient.invalidateQueries({
  queryKey: chatKeys.conversations(),
});
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/api/useChatHub.ts
git commit -m "fix: invalidate conversations list after meeting cancellation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Enforce Max 2 Decimal Places on Price Fields

**Files:**
- Modify: `automotive.marketplace.client/src/features/createListing/schemas/createListingSchema.ts`

**Context:** The DB column is `decimal(18,2)` (max 2 decimal places). The FE schema should enforce this before submit.

- [ ] **Step 1: Add decimal precision refinement**

Find the `price` field in `createListingSchema.ts` and add a `.refine`:

```ts
price: z.coerce
  .number()
  // ...existing min/max validations...
  .refine(
    (val) => Number.isInteger(Math.round(val * 100)),
    { message: "Price must have at most 2 decimal places." },
  ),
```

The `Math.round(val * 100)` approach handles floating-point precision (e.g. 1.005). If the project has a `VALIDATION` constants file with error helpers, use the same pattern as the other price validations in the file.

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/createListing/schemas/createListingSchema.ts
git commit -m "fix: enforce max 2 decimal places on listing price

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: Translate "Available" Listing Status in My Listings

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`

**Context:** The BE returns `Status.Available.ToString()` = `"Available"`. The `StatusBadge` only checks for `"Active"` and `"Approved"` but never matches `"Available"`, so it falls through and renders the raw English string.

- [ ] **Step 1: Add "Available" to the isActive condition**

Find the `StatusBadge` function in `MyListingCard.tsx`:

```tsx
// Change:
const isActive = status === "Active" || status === "Approved";

// To:
const isActive = status === "Active" || status === "Approved" || status === "Available";
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx
git commit -m "fix: translate 'Available' listing status in my listings

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: AI Summary Force-Regenerate (Skip Cache on Explicit Regenerate Click)

**Context:** Both single-listing and comparison AI summaries use a cache. Clicking "Regenerate" currently calls `refetch()` which may return the cached result if the cache hasn't expired. We need a `forceRegenerate` param that skips the cache.

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`
- Modify: `automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx`
- Modify: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`

- [ ] **Step 1: Add ForceRegenerate to single-listing AI summary query**

```csharp
// GetListingAiSummaryQuery.cs
public class GetListingAiSummaryQuery : IRequest<GetListingAiSummaryResponse>
{
    public Guid ListingId { get; set; }
    public string Language { get; set; } = "lt";
    public bool ForceRegenerate { get; set; } = false;
}
```

- [ ] **Step 2: Skip cache in single-listing handler when ForceRegenerate = true**

In `GetListingAiSummaryQueryHandler.cs`, wrap the cache check:

```csharp
// Replace the cache-return block:
if (!request.ForceRegenerate && cache != null && cache.ExpiresAt > DateTime.UtcNow)
{
    var listingModifiedAt = listing.ModifiedAt ?? listing.CreatedAt;
    if (cache.GeneratedAt >= listingModifiedAt)
        return new GetListingAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
}
```

- [ ] **Step 3: Add ForceRegenerate to comparison AI summary query**

```csharp
// GetListingComparisonAiSummaryQuery.cs
public class GetListingComparisonAiSummaryQuery : IRequest<GetListingComparisonAiSummaryResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
    public string Language { get; set; } = "lt";
    public bool ForceRegenerate { get; set; } = false;
}
```

- [ ] **Step 4: Skip cache in comparison handler when ForceRegenerate = true**

```csharp
// Replace the cache-return block:
if (!request.ForceRegenerate && cache != null && cache.ExpiresAt > DateTime.UtcNow)
{
    var aModified = listingA.ModifiedAt ?? listingA.CreatedAt;
    var bModified = listingB.ModifiedAt ?? listingB.CreatedAt;
    if (cache.GeneratedAt >= aModified && cache.GeneratedAt >= bModified)
        return new GetListingComparisonAiSummaryResponse { Summary = cache.Summary, IsGenerated = true, FromCache = true };
}
```

- [ ] **Step 5: Add forceRegenerate param to FE single-listing options**

```ts
// getListingAiSummaryOptions.ts
export const getListingAiSummaryOptions = (
  listingId: string,
  language: string = "lt",
  forceRegenerate: boolean = false,
) =>
  queryOptions({
    queryKey: listingKeys.aiSummary(listingId, language),
    queryFn: () =>
      axiosClient.get<GetListingAiSummaryResponse>(
        ENDPOINTS.LISTING.GET_AI_SUMMARY,
        { params: { listingId, language, forceRegenerate } },
      ),
    enabled: false,
  });
```

- [ ] **Step 6: Track regenerate intent in AiSummarySection.tsx**

```tsx
// In AiSummarySection.tsx
const [forceRegenerate, setForceRegenerate] = useState(false);
const { data, isFetching, refetch } = useQuery(
  getListingAiSummaryOptions(listingId, i18n.language, forceRegenerate),
);
const hasResult = data?.data?.isGenerated;

const handleRegenerate = async () => {
  setForceRegenerate(true);
  await refetch();
  setForceRegenerate(false);
};

// Change the Regenerate button onClick from `() => refetch()` to `handleRegenerate`
// Only use forceRegenerate=true when hasResult (i.e., user explicitly clicks Regenerate on existing result)
```

- [ ] **Step 7: Add forceRegenerate param to FE comparison options**

```ts
// getListingComparisonAiSummaryOptions.ts
export const getListingComparisonAiSummaryOptions = (
  listingAId: string,
  listingBId: string,
  language: string = "lt",
  forceRegenerate: boolean = false,
) =>
  queryOptions({
    queryKey: listingKeys.comparisonAiSummary(listingAId, listingBId, language),
    queryFn: () =>
      axiosClient.get<GetListingComparisonAiSummaryResponse>(
        ENDPOINTS.LISTING.GET_COMPARISON_AI_SUMMARY,
        { params: { listingAId, listingBId, language, forceRegenerate } },
      ),
    enabled: false,
  });
```

- [ ] **Step 8: Track regenerate intent in CompareAiSummary.tsx**

Apply the same pattern as Step 6 — add `forceRegenerate` state, update the `useQuery` call, and use `handleRegenerate` for the button's onClick.

- [ ] **Step 9: Build and verify**

```bash
dotnet build ./Automotive.Marketplace.sln
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 10: Commit**

```bash
git add \
  Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/ \
  Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/ \
  automotive.marketplace.client/src/features/listingDetails/api/getListingAiSummaryOptions.ts \
  automotive.marketplace.client/src/features/listingDetails/components/AiSummarySection.tsx \
  automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts \
  automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx
git commit -m "feat: force-regenerate AI summary bypasses cache

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: Fix Make/Model Filter Reset Bug

**Context:** Two places:
1. **Main page `ListingSearch`**: When user picks a make + model then switches back to "All Makes", the model becomes empty instead of "All Models".
2. **Listings page `Filters`**: When user picks a model then changes make to "All Makes", the model filter still applies in the search URL.

**Files:**
- Modify: `automotive.marketplace.client/src/features/search/components/ListingSearch.tsx`
- Modify: `automotive.marketplace.client/src/features/listingList/components/Filters.tsx`

- [ ] **Step 1: Fix ListingSearch — reset model when make is cleared**

```tsx
// In ListingSearch.tsx, change the MakeSelect's onValueChange to also reset models:
onValueChange={(value) => {
  updateSearchValue("makeId", value);
  if (value === UI_CONSTANTS.SELECT.ALL_MAKES.VALUE) {
    updateSearchValue("models", [UI_CONSTANTS.SELECT.ALL_MODELS.VALUE]);
  }
}}
```

- [ ] **Step 2: Fix Filters — reset models when make changes to All Makes**

```tsx
// In Filters.tsx, update handleFilterChange to clear models when makeId resets:
const handleFilterChange = async <K extends keyof ListingFilterStateValues>(
  key: K,
  value: string | string[],
) => {
  let updatedFilterValues = { ...filterValues, [key]: value };
  if (key === "makeId" && value === UI_CONSTANTS.SELECT.ALL_MAKES.VALUE) {
    updatedFilterValues = { ...updatedFilterValues, models: [] };
  }
  const updatedSearchParams = mapFilterValuesToSearchParams(updatedFilterValues);
  await onSearchParamChange(updatedSearchParams);
};
```

Note: `UI_CONSTANTS` needs to be imported in `Filters.tsx` if not already present. Check the existing imports.

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add \
  automotive.marketplace.client/src/features/search/components/ListingSearch.tsx \
  automotive.marketplace.client/src/features/listingList/components/Filters.tsx
git commit -m "fix: reset model filter when make is cleared to All Makes

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
