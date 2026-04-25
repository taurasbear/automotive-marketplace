# i18n Remaining Gaps — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close all remaining i18n gaps — locale-aware number formatting, hardcoded strings, and untranslated aria-labels.

**Architecture:** Create a shared `formatNumber` utility that reads the current i18n language and delegates to `Intl.NumberFormat`. Replace all `.toLocaleString()` and hardcoded `Intl.NumberFormat("en-US")` calls with this utility. Fix a handful of missed hardcoded strings and aria-labels.

**Tech Stack:** react-i18next (already installed), Intl.NumberFormat, existing i18n infrastructure.

---

## File Structure

### New Files
- `src/lib/i18n/formatNumber.ts` — locale-aware number formatting utility

### Modified Files

**Number formatting (15 call sites across 11 files):**
- `src/features/myListings/components/MyListingCard.tsx` — replace `Intl.NumberFormat("en-US")` with `formatNumber`
- `src/features/myListings/components/EditableField.tsx` — replace `.toLocaleString()` with `formatNumber`
- `src/features/listingList/components/ListingCard.tsx` — replace `.toFixed(0)` with `formatCurrency`
- `src/features/listingDetails/components/ListingDetailsContent.tsx` — replace `.toLocaleString()`
- `src/features/compareListings/components/CompareTable.tsx` — replace `.toLocaleString()`
- `src/features/compareListings/components/CompareSearchModal.tsx` — replace `.toLocaleString()` (2 sites)
- `src/features/savedListings/components/SavedListingRow.tsx` — replace `.toLocaleString()` (2 sites)
- `src/features/savedListings/components/PropertyMentionPicker.tsx` — replace `.toLocaleString()` (2 sites)
- `src/features/chat/components/MakeOfferModal.tsx` — replace `.toLocaleString()` (3 sites)
- `src/features/chat/components/ListingCard.tsx` — replace `.toLocaleString()`
- `src/features/chat/components/OfferCard.tsx` — replace `.toLocaleString()` (2 sites)

**Hardcoded strings (4 files):**
- `src/components/forms/DefectSelector.tsx` — "Unknown defect" → i18n key
- `src/components/forms/select/UsedSelect.tsx` — aria-label
- `src/components/forms/select/LocationCombobox.tsx` — aria-label
- `src/features/compareListings/components/CompareSearchModal.tsx` — aria-label

**Translation files (2 files):**
- `src/lib/i18n/locales/en/common.json` — add aria-label + fallback keys
- `src/lib/i18n/locales/lt/common.json` — Lithuanian translations

---

### Task 1: Create `formatNumber` Utility

**Files:**
- Create: `src/lib/i18n/formatNumber.ts`

The utility maps the i18n language code to the correct `Intl.NumberFormat` locale. Lithuanian uses space as thousands separator (1 000), English uses comma (1,000).

- [ ] **Step 1: Create the utility**

```typescript
// src/lib/i18n/formatNumber.ts
import i18n from "./i18n";

const localeMap: Record<string, string> = {
  lt: "lt-LT",
  en: "en-US",
};

const getLocale = (): string => localeMap[i18n.language] ?? "lt-LT";

export const formatNumber = (value: number): string => {
  return new Intl.NumberFormat(getLocale()).format(value);
};

export const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat(getLocale(), {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};
```

Two functions:
- `formatNumber` — general purpose (mileage, engine size, etc.)
- `formatCurrency` — for prices (no decimals, same locale-aware grouping)

Both read the current i18n language at call time, so they react to language changes.

- [ ] **Step 2: Verify it builds**

```bash
cd automotive.marketplace.client && npx tsc --noEmit --pretty 2>&1 | head -20
```

Expected: No new errors.

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat(i18n): add locale-aware number formatting utility

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Replace All Number Formatting Call Sites

**Files:**
- Modify: `src/features/myListings/components/MyListingCard.tsx`
- Modify: `src/features/myListings/components/EditableField.tsx`
- Modify: `src/features/listingList/components/ListingCard.tsx`
- Modify: `src/features/listingDetails/components/ListingDetailsContent.tsx`
- Modify: `src/features/compareListings/components/CompareTable.tsx`
- Modify: `src/features/compareListings/components/CompareSearchModal.tsx`
- Modify: `src/features/savedListings/components/SavedListingRow.tsx`
- Modify: `src/features/savedListings/components/PropertyMentionPicker.tsx`
- Modify: `src/features/chat/components/MakeOfferModal.tsx`
- Modify: `src/features/chat/components/ListingCard.tsx` (chat)
- Modify: `src/features/chat/components/OfferCard.tsx`

Replace every `.toLocaleString()`, `.toFixed(0)`, and `Intl.NumberFormat("en-US")` call with the new utility functions.

- [ ] **Step 1: Update MyListingCard.tsx**

Replace the two helper functions:

```typescript
// BEFORE:
const formatPrice = (price: number) => {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "EUR",
    minimumFractionDigits: 0,
  }).format(price);
};

const formatMileage = (mileage: number) => {
  return new Intl.NumberFormat("en-US").format(mileage);
};

// AFTER:
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
// Delete both helper functions entirely.
```

In the JSX, `formatPrice` previously returned `€1,234` (currency symbol included via `style: "currency"`). The new `formatCurrency` returns just the number with locale grouping. Find where `formatPrice` is called and replace with `{formatCurrency(listing.price)} €`. Replace `{formatMileage(listing.mileage)} km` with `{formatNumber(listing.mileage)} km`.

- [ ] **Step 2: Update EditableField.tsx**

```typescript
// BEFORE (line 49):
return value.toLocaleString();

// AFTER:
import { formatNumber } from "@/lib/i18n/formatNumber";
// ...
return formatNumber(value);
```

- [ ] **Step 3: Update ListingCard.tsx (listingList)**

```typescript
// BEFORE (line 62, 64):
<p className="font-sans text-xs">{listing.mileage} km</p>
{listing.price.toFixed(0)} €

// AFTER:
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
// ...
<p className="font-sans text-xs">{formatNumber(listing.mileage)} km</p>
{formatCurrency(listing.price)} €
```

- [ ] **Step 4: Update ListingDetailsContent.tsx**

```typescript
// BEFORE:
{listing.mileage.toLocaleString()} km

// AFTER:
import { formatNumber } from "@/lib/i18n/formatNumber";
// ...
{formatNumber(listing.mileage)} km
```

- [ ] **Step 5: Update CompareTable.tsx**

```typescript
// BEFORE:
format: (v) => `${(v as number).toLocaleString()} km`,

// AFTER:
import { formatNumber } from "@/lib/i18n/formatNumber";
// ...
format: (v) => `${formatNumber(v as number)} km`,
```

- [ ] **Step 6: Update CompareSearchModal.tsx**

```typescript
// BEFORE (2 sites):
{listing.mileage.toLocaleString()} km · {listing.city}

// AFTER:
import { formatNumber } from "@/lib/i18n/formatNumber";
// ...
{formatNumber(listing.mileage)} km · {listing.city}
```

- [ ] **Step 7: Update SavedListingRow.tsx**

```typescript
// BEFORE:
{listing.price.toLocaleString()} € · {listing.city} ·{" "}
{listing.mileage.toLocaleString()} km

// AFTER:
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
// ...
{formatCurrency(listing.price)} € · {listing.city} ·{" "}
{formatNumber(listing.mileage)} km
```

- [ ] **Step 8: Update PropertyMentionPicker.tsx**

```typescript
// BEFORE:
format: (v) => `${(v as number).toLocaleString()} km`,
format: (v) => `${(v as number).toLocaleString()} €`,

// AFTER:
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
// ...
format: (v) => `${formatNumber(v as number)} km`,
format: (v) => `${formatCurrency(v as number)} €`,
```

- [ ] **Step 9: Update MakeOfferModal.tsx**

```typescript
// BEFORE (3 sites):
€{listingPrice.toLocaleString()}
amount: Math.ceil(minAmount).toLocaleString(),
price: listingPrice.toLocaleString(),

// AFTER:
import { formatCurrency } from "@/lib/i18n/formatNumber";
// ...
€{formatCurrency(listingPrice)}
amount: formatCurrency(Math.ceil(minAmount)),
price: formatCurrency(listingPrice),
```

- [ ] **Step 10: Update chat/ListingCard.tsx**

```typescript
// BEFORE:
{listingPrice.toLocaleString()} €

// AFTER:
import { formatCurrency } from "@/lib/i18n/formatNumber";
// ...
{formatCurrency(listingPrice)} €
```

- [ ] **Step 11: Update OfferCard.tsx**

```typescript
// BEFORE:
€{offer.amount.toLocaleString()}
€{listingPrice.toLocaleString()}

// AFTER:
import { formatCurrency } from "@/lib/i18n/formatNumber";
// ...
€{formatCurrency(offer.amount)}
€{formatCurrency(listingPrice)}
```

- [ ] **Step 12: Build check**

```bash
cd automotive.marketplace.client && npx tsc --noEmit --pretty 2>&1 | head -20
```

Expected: No new errors.

- [ ] **Step 13: Commit**

```bash
git add -A && git commit -m "fix(i18n): use locale-aware number formatting across all components

Replace .toLocaleString(), .toFixed(0), and Intl.NumberFormat('en-US')
with formatNumber/formatCurrency utilities that respect i18n language.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Fix Remaining Hardcoded Strings and Aria-Labels

**Files:**
- Modify: `src/components/forms/DefectSelector.tsx`
- Modify: `src/components/forms/select/UsedSelect.tsx`
- Modify: `src/components/forms/select/LocationCombobox.tsx`
- Modify: `src/features/compareListings/components/CompareSearchModal.tsx`
- Modify: `src/lib/i18n/locales/en/common.json`
- Modify: `src/lib/i18n/locales/lt/common.json`

- [ ] **Step 1: Add i18n keys to common.json files**

Add to `en/common.json` (merge into existing object):
```json
{
  "aria": {
    "location": "Location",
    "conditionFilter": "Used, new or both",
    "savedListing": "Saved listing"
  },
  "defects": {
    "unknownDefect": "Unknown defect"
  }
}
```

Add to `lt/common.json` (merge into existing object):
```json
{
  "aria": {
    "location": "Vietovė",
    "conditionFilter": "Naudota, nauja arba abi",
    "savedListing": "Išsaugotas skelbimas"
  },
  "defects": {
    "unknownDefect": "Nežinomas defektas"
  }
}
```

- [ ] **Step 2: Fix DefectSelector.tsx**

```typescript
// BEFORE (line 141):
return "Unknown defect";

// AFTER:
// DefectSelector already imports i18n indirectly via getTranslatedName.
// Add useTranslation if not present:
import { useTranslation } from "react-i18next";
// Inside the component:
const { t } = useTranslation("common");
// Then:
return t("defects.unknownDefect");
```

If DefectSelector already has `useTranslation` from another namespace, use the `common:` prefix: `t("common:defects.unknownDefect")`.

- [ ] **Step 3: Fix UsedSelect.tsx aria-label**

```typescript
// BEFORE:
aria-label="Used, new or both"

// AFTER:
// Component likely already has useTranslation. Use:
aria-label={t("common:aria.conditionFilter")}
// Or if namespace is already "common":
aria-label={t("aria.conditionFilter")}
```

- [ ] **Step 4: Fix LocationCombobox.tsx aria-label**

```typescript
// BEFORE:
<PopoverTrigger aria-label="Location" asChild>

// AFTER:
<PopoverTrigger aria-label={t("common:aria.location")} asChild>
```

Add `useTranslation` import if not present.

- [ ] **Step 5: Fix CompareSearchModal.tsx aria-label**

```typescript
// BEFORE:
aria-label="Saved listing"

// AFTER:
aria-label={t("common:aria.savedListing")}
```

Component already has `useTranslation("compare")`, so use the `common:` prefix.

- [ ] **Step 6: Build check**

```bash
cd automotive.marketplace.client && npx tsc --noEmit --pretty 2>&1 | head -20
```

- [ ] **Step 7: Commit**

```bash
git add -A && git commit -m "fix(i18n): translate remaining hardcoded strings and aria-labels

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Final Verification

- [ ] **Step 1: Full build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -5
```

Expected: Build succeeds.

- [ ] **Step 2: Lint check**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -10
```

Expected: No new lint errors (pre-existing ones are OK).

- [ ] **Step 3: Grep for remaining `.toLocaleString()` — should be zero**

```bash
grep -rn "\.toLocaleString()" src/ --include="*.tsx" --include="*.ts"
```

Expected: Zero results.

- [ ] **Step 4: Grep for remaining `Intl.NumberFormat("en-US")` — should be zero**

```bash
grep -rn 'NumberFormat("en-US")' src/ --include="*.tsx" --include="*.ts"
```

Expected: Zero results (only `formatNumber.ts` should use `Intl.NumberFormat`).

- [ ] **Step 5: Format check**

```bash
cd automotive.marketplace.client && npm run format:check 2>&1 | tail -5
```

If files need formatting, run `npx prettier --write` on them and amend the last commit.
