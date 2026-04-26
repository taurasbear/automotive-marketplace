# Score, AI & Settings Improvements — Design Spec

**Date:** 2026-04-26  
**Status:** Approved  

## Problem & Goal

Three recently implemented features (CarDog Score Engine, AI Summaries, Personalization Quiz) shipped with several gaps:

1. No dedicated settings page — scoring and AI preferences are buried or inaccessible
2. Vehicle score ignores seller-marked defects
3. AI summaries ignore available CarDog data (market price, efficiency, reliability) and always respond in English
4. All new UI strings are hardcoded in English — no i18next support
5. Several bugs: missing toast translation keys, preferences not loading back into the quiz modal, score weights not applying

This spec covers all five areas in one cohesive delivery.

---

## Architecture Overview

The changes span all four layers:

- **Domain / Infrastructure**: add `EnableVehicleScoring` to `UserPreferences`, add `Language` to `ListingAiSummaryCache`
- **Application**: extend `ListingScoreCalculator` with Condition factor, enrich AI prompts with CarDog data + defects + language instruction, add `UnavailableFactors` to AI response
- **Server**: pass `lang` query param through to AI summary query handlers
- **Frontend**: new `/settings` page, fix QuizModal initialisation, conditionally render ScoreCard based on `EnableVehicleScoring`, extract all hardcoded strings into i18next

---

## Section 1: Settings Page & UserPreferences

### New `UserPreferences` field

Add `EnableVehicleScoring` (bool, default `false`) to `UserPreferences` entity and EF configuration. This controls whether the frontend requests score calculations at all, saving CarDog API calls for users who don't want scores.

Existing `AutoGenerateAiSummary` remains unchanged.

Both fields are exposed through the existing `GET /UserPreferences/Get` and `PUT /UserPreferences/Upsert` endpoints — the command simply gains one new property.

### Settings page route

New route: `/settings`. The UserMenu's currently-disabled "Profile Settings" item is activated to link to `/settings`.

### Settings page layout

A `<Card>` with a single group: **Listing Preferences**. Two rows separated by a `<Separator>`:

1. **Vehicle scoring row** (`BarChart2` icon)
   - Label: `t('userPreferences:settings.scoringLabel')`
   - Description: `t('userPreferences:settings.scoringDescription')`
   - Right side: `<Switch>` bound to `EnableVehicleScoring`
2. **Scoring preferences row** (indented, `SlidersHorizontal` icon)
   - Label: `t('userPreferences:settings.scoringPreferencesLabel')`
   - Right side: `<Button variant="outline" size="sm">` that opens `QuizModal`
   - Disabled and visually muted when `EnableVehicleScoring = false`
3. `<Separator />`
4. **AI summaries row** (`Sparkles` icon)
   - Label: `t('userPreferences:settings.aiSummaryLabel')`
   - Description: `t('userPreferences:settings.aiSummaryDescription')`
   - Right side: `<Switch>` bound to `AutoGenerateAiSummary`

Both switches call `useUpsertUserPreferences` on change.

### ScoreCard conditional rendering

`ScoreCard` (and `CompareScoreBanner`) check whether the current user has `EnableVehicleScoring = true` before fetching the score. When disabled, they render a soft prompt: "Enable vehicle scoring in Settings to see a score for this listing."

---

## Section 2: Condition Factor in Score

### `ListingScoreCalculator` changes

Add a fifth factor `Condition` with a count-based formula:

```
conditionScore = max(0, 100 - defectCount × 20)
```

0 defects → 100, 3 defects → 40, 5+ defects → 0.

**Default weights (revised, sum = 1.0):**

| Factor      | Old weight | New weight |
|-------------|------------|------------|
| Value       | 0.30       | 0.26       |
| Efficiency  | 0.25       | 0.21       |
| Reliability | 0.25       | 0.21       |
| Mileage     | 0.20       | 0.17       |
| Condition   | —          | 0.15       |

User `UserPreferences` weights are extended with `ConditionWeight` (default 0.15). The sum-to-1.0 validation on upsert applies to all five weights.

### Handler changes

`GetListingScoreQueryHandler` loads `listing.Defects` via `.Include(l => l.Defects)`. It passes `defectCount = listing.Defects.Count` to the calculator. If the listing is not found or defects is null, defect count defaults to 0.

### Response changes

`GetListingScoreResponse` gains a `Condition` property (type `ScoreFactor`). `GetListingScoreResponse` already has `MissingFactors` — Condition is never "missing" (always scored since defect count is always known).

### Frontend: ScoreCard

Add `Condition` bar to the factor breakdown list. Under the bar, show defect count as secondary text: `t('userPreferences:score.defects', { count: n })` → "3 known defects" / "Jokių defektų" etc.

### Frontend: QuizModal

Add a `Condition` slider (5th row, `ShieldAlert` icon). All five sliders sum to 100%. Default condition slider: 15%.

---

## Section 3: AI Summary Enrichment

### CarDog data in prompts

The `GetListingAiSummaryQueryHandler` and `GetListingComparisonAiSummaryQueryHandler` load the CarDog cache tables before building the prompt:

- `VehicleMarketCache` keyed on `(Make, Model, Year)`
- `VehicleEfficiencyCache` keyed on `(Make, Model, Year)`
- `VehicleReliabilityCache` keyed on `(Make, Model, Year)`

No new CarDog API calls are made — the handler reads from cache only (same data the score engine populated). If a cache row is missing, that factor is omitted from the prompt and added to `UnavailableFactors`.

**Prompt additions (when data available):**

```
Market data: median price {x} EUR across {n} listings.
Efficiency: {x} L/100km (or {x} kWh/100km).
Reliability: {recalls} recalls, {crashes} crash complaints, {injuries} injury complaints.
Seller-marked defects: {defect list | "none"}.
```

### Missing-data transparency

**Backend:** `GetListingAiSummaryResponse` gains `UnavailableFactors: List<string>` (e.g. `["MarketValue", "Efficiency"]`). The prompt includes: "Note: the following data was unavailable and was not factored into your response: {list}."

**Frontend:** `AiSummarySection` shows a small `<Alert variant="info">` below the summary text when `UnavailableFactors` is non-empty:
> "Some data was unavailable (market value, efficiency). The verdict may be incomplete."

Same pattern in `CompareAiSummary`.

### Language parameter

All three AI summary endpoints accept a `?lang=` query param (e.g. `lt`, `en`). Default: `lt`.

`GetListingAiSummaryQuery` and `GetListingComparisonAiSummaryQuery` gain a `Language` string property. The prompt ends with: `"Respond in {languageName}."` (resolved from a simple `lang → display name` map: `lt → Lithuanian`, `en → English`).

**Cache key includes language.** `ListingAiSummaryCache` gains a `Language` column (varchar(8), default `"lt"`). Cache lookup filters on `(ListingId, SummaryType, ComparisonListingId, Language)`. A migration is needed for this column.

**Frontend:** `getListingAiSummaryOptions` and `getListingComparisonAiSummaryOptions` pass `i18n.language` as the `lang` param. Seller insights does not call OpenAI and requires no language param.

---

## Section 4: Bug Fixes

### Missing toast keys

`toasts.json` (both `en` and `lt`) are missing the `preferences` namespace that `useUpsertUserPreferences` references:

```json
// en
"preferences": {
  "saved": "Preferences saved!",
  "saveError": "Failed to save preferences"
}

// lt
"preferences": {
  "saved": "Nustatymai išsaugoti!",
  "saveError": "Nepavyko išsaugoti nustatymų"
}
```

### Preferences not loading in QuizModal

`QuizModal` accepts `initialWeights` but the callers (ScoreCard "Customize" button, settings page preferences row) don't fetch and pass them.

Fix:
1. Both callers call `useGetUserPreferences()`.
2. When `HasPreferences = true`, pass fetched weights as `initialWeights` and also pass `initialStep={2}` (go straight to sliders).
3. `QuizModal` respects `initialStep` prop: when `2`, renders step 2 directly on open.

### Score weights not affecting score

Two causes:

**Cause 1 — Normalization cancels weights when only one factor is scored.** The calculator computes `score = Σ(factor.Score × weight) / Σ(weights of scored factors)`. When CarDog is unavailable only Mileage is scored — the single weight divides itself and cancels out. Weights genuinely don't affect the score until 2+ factors are scored. This is by design. The `ScoreCard` should surface a notice when fewer than 2 factors are scored: "Score is limited — CarDog data is unavailable. Enable the API key for a full score."

**Cause 2 — `EnableVehicleScoring` never existed.** The frontend always called the score endpoint regardless of user intent. With the new field, only call `GET /Listing/GetScore` when `EnableVehicleScoring = true`.

The existing `UpsertUserPreferences` command must be extended to include `ConditionWeight` and `EnableVehicleScoring`. The controller already passes `UserId` from the JWT claim; no server-side change needed for weight application.

---

## Section 5: i18n

### New namespace: `userPreferences`

All new/untranslated strings for ScoreCard, QuizModal, and Settings page go into a new `userPreferences` namespace.

**Files created:**
- `src/lib/i18n/locales/en/userPreferences.json`
- `src/lib/i18n/locales/lt/userPreferences.json`

**Key groups:**

```
score.*        — ScoreCard labels (overallScore, value, efficiency, reliability, mileage, condition, defects, missingFactors, personalized, customize)
quiz.*         — QuizModal steps, labels, buttons (step0Title, step1Title, step2Title, styleOptions.*, priorities.*, save, saving, back, next, normalizeNotice)
settings.*     — Settings page section headers, row labels, descriptions
```

### Existing namespaces extended

**`listings`:**
- `aiSummary.title`, `aiSummary.generate`, `aiSummary.regenerate`, `aiSummary.loading`, `aiSummary.unavailableFactors`, `aiSummary.enableScoring`

**`compare`:**
- `aiSummary.title`, `aiSummary.regenerate`

**`toasts`:**
- `preferences.saved`, `preferences.saveError`

### Registration

`i18n.ts` imports and registers `userPreferencesEn` and `userPreferencesLt` under the `userPreferences` namespace.

All components in `userPreferences/`, `listingDetails/`, `compareListings/`, and `components/settings/` use `useTranslation('userPreferences')` or the relevant namespace. No hardcoded English strings remain in any of these components.

---

## Data Model Changes

| Entity | Change |
|--------|--------|
| `UserPreferences` | Add `EnableVehicleScoring` (bool, default false), `ConditionWeight` (double, default 0.15) |
| `ListingAiSummaryCache` | Add `Language` (varchar(8), default `"lt"`) |

Two EF migrations required.

---

## Error Handling & Edge Cases

- **CarDog cache miss:** Prompt omits that factor; `UnavailableFactors` lists it; AI is instructed not to speculate about missing data.
- **No defects loaded:** Defect count = 0; Condition score = 100 (not a "missing" factor).
- **All CarDog data missing + no defects:** Score is still calculated from Mileage + Condition. `HasMissingFactors = true`.
- **Language code unknown:** Backend maps unrecognised codes to `"Lithuanian"` (safe fallback for this app's market).
- **User not authenticated:** Score endpoint gracefully omits `UserId`; default weights apply.

---

## File Map

**Backend — modified:**
- `Automotive.Marketplace.Domain/Entities/UserPreferences.cs` — add `EnableVehicleScoring`, `ConditionWeight`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs` — configure new columns
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs` — add new fields
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs` — persist new fields, update weight validation to 5 weights
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs` — add new fields
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs` — return new fields with defaults
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ListingScoreCalculator.cs` — add Condition factor, update default weights
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs` — add `Condition` ScoreFactor
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQuery.cs` — no change (UserId already present)
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs` — Include Defects, pass defect count to calculator
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQuery.cs` — add `Language` property
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryResponse.cs` — add `UnavailableFactors`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingAiSummary/GetListingAiSummaryQueryHandler.cs` — load CarDog cache, include defects, enrich prompt, add language instruction, cache per language
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQuery.cs` — add `Language`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryResponse.cs` — add `UnavailableFactors`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparisonAiSummary/GetListingComparisonAiSummaryQueryHandler.cs` — enrich prompt, language instruction, cache per language
- `Automotive.Marketplace.Domain/Entities/ListingAiSummaryCache.cs` — add `Language` property
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingAiSummaryCacheConfiguration.cs` — configure `Language` column
- `Automotive.Marketplace.Server/Controllers/ListingController.cs` — accept `?lang=` param on AI summary endpoints
- EF migrations x2

**Backend — new:**
- `Automotive.Marketplace.Tests/Features/UnitTests/ListingScoreCalculatorConditionTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesWithNewFieldsTests.cs`

**Frontend — new:**
- `src/app/pages/Settings.tsx`
- `src/lib/i18n/locales/en/userPreferences.json`
- `src/lib/i18n/locales/lt/userPreferences.json`

**Frontend — modified:**
- `src/app/App.tsx` — add `/settings` route
- `src/components/layout/header/UserMenu.tsx` — activate "Profile Settings" link to `/settings`
- `src/features/userPreferences/components/QuizModal.tsx` — add `initialStep` prop, add Condition slider, i18n all strings
- `src/features/userPreferences/api/getUserPreferencesOptions.ts` — no change needed
- `src/features/userPreferences/api/useUpsertUserPreferences.ts` — no change needed
- `src/features/userPreferences/types/UserPreferencesResponse.ts` — add `EnableVehicleScoring`, `ConditionWeight`
- `src/features/userPreferences/types/UpsertUserPreferencesCommand.ts` — add `EnableVehicleScoring`, `ConditionWeight`
- `src/features/listingDetails/components/ScoreCard.tsx` — add Condition factor bar, gate on `EnableVehicleScoring`, i18n all strings
- `src/features/listingDetails/components/AiSummarySection.tsx` — pass `lang`, show unavailable factors notice, i18n
- `src/features/listingDetails/api/getListingAiSummaryOptions.ts` — add `lang` param
- `src/features/listingDetails/types/GetListingAiSummaryResponse.ts` — add `UnavailableFactors`
- `src/features/listingDetails/types/GetListingScoreResponse.ts` — add `condition` ScoreFactor
- `src/features/compareListings/components/CompareAiSummary.tsx` — pass `lang`, show unavailable factors notice, i18n
- `src/features/compareListings/components/CompareScoreBanner.tsx` — gate on `EnableVehicleScoring`, i18n
- `src/features/compareListings/api/getListingComparisonAiSummaryOptions.ts` — add `lang` param
- `src/features/compareListings/types/GetListingComparisonAiSummaryResponse.ts` — add `UnavailableFactors`
- `src/lib/i18n/locales/en/toasts.json` — add `preferences` keys
- `src/lib/i18n/locales/lt/toasts.json` — add `preferences` keys
- `src/lib/i18n/locales/en/listings.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/lt/listings.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/en/compare.json` — add `aiSummary.*` keys
- `src/lib/i18n/locales/lt/compare.json` — add `aiSummary.*` keys
- `src/lib/i18n/i18n.ts` — register `userPreferences` namespace
