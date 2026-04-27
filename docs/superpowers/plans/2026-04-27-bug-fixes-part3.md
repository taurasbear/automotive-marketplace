# Bug Fixes Part 3 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 6 bugs: default permissions on registration, AI comparison prompt language, comparison title links, AI summary auto-caching on refresh, and seller insights i18n.

**Architecture:** Backend permission fix is in `RegisterUserCommandHandler` (add default `UserPermission` rows). Frontend fixes span `CompareHeader`, `CompareAiSummary`, `getListingComparisonAiSummaryOptions`, and `SellerInsightsPanel` with new i18n keys.

**Tech Stack:** ASP.NET Core 8, React 19, TypeScript, TanStack Query, react-i18next, xUnit + TestContainers

---

## Context

Working branch: `fixes`

Key files:
- `Automotive.Marketplace.Application/Features/AuthFeatures/RegisterUser/RegisterUserCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`
- `automotive.marketplace.client/src/features/compareListings/components/CompareHeader.tsx`
- `automotive.marketplace.client/src/features/compareListings/components/CompareAiSummary.tsx`
- `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`
- `automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx`
- `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`
- `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`

---

## Task 1: Default permissions on user registration

**Problem:** `RegisterUserCommandHandler` creates a `User` with no `UserPermissions`. A freshly registered user gets 403 on all protected endpoints including `GetMy`, `Create` (listing), `GetSellerInsights`, etc.

**Required default permissions** (everything except manage makes/models/variants):
- `ViewListings`, `CreateListings`, `ManageListings`
- `ViewModels`, `ViewVariants`, `ViewMakes`

Also update `UserSeeder.CreateRegularUser()` to match these permissions.

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/AuthFeatures/RegisterUser/RegisterUserCommandHandler.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Seeders/UserSeeder.cs`
- Modify (test): `Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/RegisterUserCommandHandlerTests.cs` (if it exists; otherwise skip)

- [ ] **Step 1: Read the handler and seeder**

Read `RegisterUserCommandHandler.cs` and `UserSeeder.cs`. Confirm:
- Handler creates `User` with no permissions
- Seeder's `CreateRegularUser` is missing `ManageListings` and `ViewMakes`

- [ ] **Step 2: Update `RegisterUserCommandHandler`**

Add default `UserPermission` rows after user creation:

```csharp
var user = new User
{
    Username = request.Username,
    Email = request.Email,
    HashedPassword = passwordHasher.Hash(request.Password),
    UserPermissions =
    [
        new() { Permission = Permission.ViewListings },
        new() { Permission = Permission.CreateListings },
        new() { Permission = Permission.ManageListings },
        new() { Permission = Permission.ViewModels },
        new() { Permission = Permission.ViewVariants },
        new() { Permission = Permission.ViewMakes },
    ],
};
```

The `Permission` enum is in `Automotive.Marketplace.Domain.Enums`. Add its `using` if not present.

- [ ] **Step 3: Update `UserSeeder.CreateRegularUser`**

Add `ManageListings` and `ViewMakes` permissions to match the new default:

```csharp
private User CreateRegularUser()
{
    var regularUser = new User
    {
        Username = "AverageBear",
        Email = "bear@regular.com",
        HashedPassword = passwordHasher.Hash("password"),
        UserPermissions =
        [
            new() { Permission = Permission.ViewListings },
            new() { Permission = Permission.ViewModels },
            new() { Permission = Permission.ViewVariants },
            new() { Permission = Permission.ViewMakes },
            new() { Permission = Permission.CreateListings },
            new() { Permission = Permission.ManageListings },
        ],
    };
    return regularUser;
}
```

- [ ] **Step 4: Write a test** (if test file exists)

If `RegisterUserCommandHandlerTests.cs` exists, add a test asserting that the registered user has the 6 expected default permissions:

```csharp
[Fact]
public async Task Handle_NewUser_HasDefaultPermissions()
{
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = CreateHandler(scope);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var command = new RegisterUserCommand
    {
        Username = "newuser",
        Email = "newuser@test.com",
        Password = "Password123!",
    };

    await handler.Handle(command, CancellationToken.None);

    var user = await context.Set<User>()
        .Include(u => u.UserPermissions)
        .FirstAsync(u => u.Email == command.Email);

    var expectedPermissions = new[]
    {
        Permission.ViewListings,
        Permission.CreateListings,
        Permission.ManageListings,
        Permission.ViewModels,
        Permission.ViewVariants,
        Permission.ViewMakes,
    };

    user.UserPermissions
        .Select(p => p.Permission)
        .Should().BeEquivalentTo(expectedPermissions);
}
```

- [ ] **Step 5: Build and test**

```bash
dotnet build ./Automotive.Marketplace.sln 2>&1 | tail -10
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~RegisterUser" 2>&1 | tail -20
```

Expected: build passes, tests pass (or no test file — skip test step).

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/AuthFeatures/RegisterUser/RegisterUserCommandHandler.cs
git add Automotive.Marketplace.Infrastructure/Data/Seeders/UserSeeder.cs
# Add test file if changed
git commit -m "fix: grant default permissions to newly registered users"
```

---

## Task 2: Use vehicle names instead of "Vehicle A/B" in AI comparison prompt

**Problem:** `BuildComparisonPrompt` in `GetListingComparisonAiSummaryQueryHandler.cs` uses "Vehicle A:" and "Vehicle B:" labels in the prompt. The AI echoes these English labels even when responding in Lithuanian.

**Fix:** Replace "Vehicle A" / "Vehicle B" with the actual `{year} {make} {model}` as the identifier. The AI will then naturally reference the cars by their proper names (language-neutral proper nouns like "2020 Skoda Octavia").

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs`

- [ ] **Step 1: Read the handler**

Read `GetListingComparisonAiSummaryQueryHandler.cs`. Locate `BuildComparisonPrompt`. Find all lines using "Vehicle A" and "Vehicle B" labels.

- [ ] **Step 2: Replace "Vehicle A/B" with car identifiers**

Change all "Vehicle A" → `$"{a.Year} {makeA} {modelA}"` and "Vehicle B" → `$"{b.Year} {makeB} {modelB}"` throughout `BuildComparisonPrompt`:

```csharp
var labelA = $"{a.Year} {makeA} {modelA}";
var labelB = $"{b.Year} {makeB} {modelB}";

var lines = new List<string>
{
    "You are an automotive assistant. Compare these two vehicle listings and recommend which is the better buy in 2-3 sentences.",
    $"{labelA}: {a.Price:0} EUR, {a.Mileage:N0} km, {a.Variant.Fuel.Name}, defects: {defectsA}",
    $"{labelB}: {b.Price:0} EUR, {b.Mileage:N0} km, {b.Variant.Fuel.Name}, defects: {defectsB}",
};

if (marketA != null)
    lines.Add($"{labelA} market data: median price {marketA.MedianPrice:0} EUR across {marketA.TotalListings} listings.");
if (marketB != null)
    lines.Add($"{labelB} market data: median price {marketB.MedianPrice:0} EUR across {marketB.TotalListings} listings.");
if (efficiencyA != null)
{
    if (efficiencyA.KWhPer100Km.HasValue) lines.Add($"{labelA} efficiency: {efficiencyA.KWhPer100Km.Value:F1} kWh/100km.");
    else if (efficiencyA.LitersPer100Km.HasValue) lines.Add($"{labelA} efficiency: {efficiencyA.LitersPer100Km.Value:F1} L/100km.");
}
if (efficiencyB != null)
{
    if (efficiencyB.KWhPer100Km.HasValue) lines.Add($"{labelB} efficiency: {efficiencyB.KWhPer100Km.Value:F1} kWh/100km.");
    else if (efficiencyB.LitersPer100Km.HasValue) lines.Add($"{labelB} efficiency: {efficiencyB.LitersPer100Km.Value:F1} L/100km.");
}
if (reliabilityA != null)
    lines.Add($"{labelA} reliability: {reliabilityA.RecallCount} recalls, {reliabilityA.ComplaintCrashes} crash complaints, {reliabilityA.ComplaintInjuries} injury complaints.");
if (reliabilityB != null)
    lines.Add($"{labelB} reliability: {reliabilityB.RecallCount} recalls, {reliabilityB.ComplaintCrashes} crash complaints, {reliabilityB.ComplaintInjuries} injury complaints.");
```

- [ ] **Step 3: Build**

```bash
dotnet build ./Automotive.Marketplace.sln 2>&1 | tail -10
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs
git commit -m "fix: use vehicle make/model names instead of 'Vehicle A/B' in AI comparison prompt"
```

---

## Task 3: Clickable listing titles in comparison header

**Problem:** In `CompareHeader.tsx`, the listing year/make/model title is a plain `<p>` tag. Clicking it does nothing.

**Fix:** Wrap the title text in a TanStack Router `<Link>` to `/listing/$id`. Keep `font-semibold` on the text. Style: `font-semibold text-foreground hover:underline` to stay bold black with a hover hint.

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareHeader.tsx`

- [ ] **Step 1: Read `CompareHeader.tsx`**

Confirm the `ListingCard` component has access to `listing.id` (it uses `GetListingByIdResponse` which has `id: string`).

- [ ] **Step 2: Wrap title in a `Link`**

In `CompareHeader.tsx`, find the `ListingCard` component's title `<p>` tag:

```tsx
<p className="mt-2 font-semibold">
  {listing.year} {listing.makeName} {listing.modelName}
</p>
```

Replace with:

```tsx
import { Link } from "@tanstack/react-router";

<Link
  to="/listing/$id"
  params={{ id: listing.id }}
  className="mt-2 block font-semibold text-foreground hover:underline"
>
  {listing.year} {listing.makeName} {listing.modelName}
</Link>
```

Note: `block` is needed so truncation/layout behaves correctly on an inline `<a>` element. `text-foreground` preserves the bold black color.

- [ ] **Step 3: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -15
```

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/CompareHeader.tsx
git commit -m "fix: make comparison listing titles clickable links to listing detail page"
```

---

## Task 4: Persist comparison AI summary across page refreshes

**Problem:** `getListingComparisonAiSummaryOptions` has `enabled: false`, so on every page refresh the React Query in-memory cache is empty and no fetch is triggered. The user sees no summary until they click "Generate" again.

The backend stores the summary in `ListingAiSummaryCache` (30-day TTL) and returns `isGenerated: false` when no summary exists. With `enabled: true`, the component auto-fetches on page load and the backend returns the cached result if one exists.

**Fix:** Change `enabled: false` to `enabled: true`.

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts`

- [ ] **Step 1: Read the options file**

Confirm `enabled: false` is set and that no other consumer depends on it being disabled.

- [ ] **Step 2: Change `enabled: false` → `enabled: true`**

```typescript
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
        {
          params: { listingAId, listingBId, language, forceRegenerate },
        },
      ),
    enabled: true,
  });
```

- [ ] **Step 3: Verify `CompareAiSummary.tsx` uses `useQuery` (not `useSuspenseQuery`)**

The component uses `useQuery` (not `useSuspenseQuery`), so the component renders while loading — no Suspense boundary is needed. Confirm this is still the case after your change.

- [ ] **Step 4: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -10
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts
git commit -m "fix: auto-fetch comparison AI summary on page load so verdict persists across refreshes"
```

---

## Task 5: Internationalize seller insights panel

**Problem:** `SellerInsightsPanel.tsx` has all hardcoded English strings: "Seller Insights", "Market Position", "Your price", "Market median", "No market data available yet", "Listing Quality", "Description", "Photos (N)", "VIN", "Colour", "% below/above market", etc.

**Fix:** Add `sellerInsights` keys to `en/myListings.json` and `lt/myListings.json`, then use `useTranslation("myListings")` in the component.

**Files:**
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`
- Modify: `automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx`

- [ ] **Step 1: Read `SellerInsightsPanel.tsx` fully**

List every hardcoded English string that needs translating.

- [ ] **Step 2: Add i18n keys to `en/myListings.json`**

Add this `sellerInsights` section:

```json
"sellerInsights": {
  "title": "Seller Insights",
  "marketPosition": {
    "title": "Market Position",
    "yourPrice": "Your price",
    "marketMedian": "Market median",
    "noData": "No market data available yet",
    "belowMarket": "{{percent}}% below market",
    "aboveMarket": "{{percent}}% above market",
    "similarListings": "{{count}} similar listings",
    "daysListed": "{{count}} days listed",
    "footer": "{{listings}} similar listings • {{days}} days listed"
  },
  "listingQuality": {
    "title": "Listing Quality",
    "description": "Description",
    "photos": "Photos ({{count}})",
    "vin": "VIN",
    "colour": "Colour"
  }
}
```

- [ ] **Step 3: Add i18n keys to `lt/myListings.json`**

Add the Lithuanian equivalent:

```json
"sellerInsights": {
  "title": "Pardavėjo įžvalgos",
  "marketPosition": {
    "title": "Rinkos pozicija",
    "yourPrice": "Jūsų kaina",
    "marketMedian": "Rinkos mediana",
    "noData": "Rinkos duomenų kol kas nėra",
    "belowMarket": "{{percent}}% žemiau rinkos",
    "aboveMarket": "{{percent}}% aukščiau rinkos",
    "similarListings": "{{count}} panašūs skelbimai",
    "daysListed": "{{count}} dienų rodomas",
    "footer": "{{listings}} panašūs skelbimai • {{days}} dienų rodomas"
  },
  "listingQuality": {
    "title": "Skelbimo kokybė",
    "description": "Aprašymas",
    "photos": "Nuotraukos ({{count}})",
    "vin": "VIN",
    "colour": "Spalva"
  }
}
```

- [ ] **Step 4: Update `SellerInsightsPanel.tsx`**

Add `useTranslation("myListings")` and replace all hardcoded strings:

```tsx
import { useTranslation } from "react-i18next";

export function SellerInsightsPanel({ listingId }: Props) {
  const { t } = useTranslation("myListings");
  // ...

  // Title:
  <span className="flex items-center gap-2 text-sm font-medium">
    <BarChart2 className="h-4 w-4" />
    {t("sellerInsights.title")}
  </span>

  // Market Position card title:
  <p className="text-xs font-semibold tracking-wide uppercase">
    {t("sellerInsights.marketPosition.title")}
  </p>

  // Your price label:
  <p className="text-muted-foreground text-xs">{t("sellerInsights.marketPosition.yourPrice")}</p>

  // Market median label:
  <p className="text-muted-foreground text-xs">{t("sellerInsights.marketPosition.marketMedian")}</p>

  // Price difference:
  marketPosition.priceDifferencePercent >= 0
    ? t("sellerInsights.marketPosition.belowMarket", { percent: marketPosition.priceDifferencePercent.toFixed(1) })
    : t("sellerInsights.marketPosition.aboveMarket", { percent: Math.abs(marketPosition.priceDifferencePercent).toFixed(1) })

  // Footer:
  t("sellerInsights.marketPosition.footer", {
    listings: marketPosition.marketListingCount,
    days: marketPosition.daysListed,
  })

  // No data:
  <p className="text-muted-foreground text-xs">{t("sellerInsights.marketPosition.noData")}</p>

  // Listing Quality title:
  <p className="text-xs font-semibold tracking-wide uppercase">
    {t("sellerInsights.listingQuality.title")}
  </p>

  // Quality check items — replace hardcoded labels:
  {[
    { check: listingQuality.hasDescription, icon: FileText, label: t("sellerInsights.listingQuality.description") },
    { check: listingQuality.hasPhotos, icon: Camera, label: t("sellerInsights.listingQuality.photos", { count: listingQuality.photoCount }) },
    { check: listingQuality.hasVin, icon: Tag, label: t("sellerInsights.listingQuality.vin") },
    { check: listingQuality.hasColour, icon: Palette, label: t("sellerInsights.listingQuality.colour") },
  ].map(({ check, label }) => (...))}
```

- [ ] **Step 5: Lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -15
```

Fix any new errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx
git add automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json
git add automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json
git commit -m "fix: internationalize seller insights panel with Lithuanian translations"
```
