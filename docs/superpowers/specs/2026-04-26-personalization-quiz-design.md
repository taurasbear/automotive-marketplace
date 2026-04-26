# Spec 2: Personalization Quiz + Score Weighting

**Date:** 2026-04-26  
**Status:** Approved for implementation  
**Depends on:** Spec 1 (CarDog + Score Engine) must be complete first

## Problem & Approach

The vehicle score from Spec 1 uses fixed default weights. This spec adds a personalization layer: a short quiz (3 scenario questions + adjustable sliders) that lets buyers set custom weights. Preferences are stored in the database tied to their account and applied automatically when they view any score.

---

## Backend

### DB Schema Update: `UserPreferences` table

New entity linked 1:1 to `ApplicationUser`:

```
Id (Guid PK), UserId (Guid FK, unique),
WeightValue (int), WeightEfficiency (int),
WeightReliability (int), WeightMileage (int),
AutoGenerateAiSummary (bool, default false),   ← stored here for Spec 3
CreatedAt (DateTimeOffset), UpdatedAt (DateTimeOffset)
```

Constraint: `WeightValue + WeightEfficiency + WeightReliability + WeightMileage = 100`.

### New CQRS Handlers

**`GetUserPreferencesQuery`** → returns `GetUserPreferencesResponse` with all four weights + `AutoGenerateAiSummary`. If no preferences row exists for the user, returns the defaults (30/25/25/20, false).

**`UpsertUserPreferencesCommand`** → accepts the four weights + `AutoGenerateAiSummary`. Validates weights sum to 100 (returns `ValidationException` if not). Creates or updates the `UserPreferences` row.

### Score Endpoint Update (Spec 1 extension)

`GET /api/listings/{id}/score` — when the request is authenticated, the handler fetches `UserPreferences` for the requesting user and passes the custom weights to `ListingScoreCalculator`. Response gains `IsPersonalized: true` and `UserWeights: { value, efficiency, reliability, mileage }`.

New endpoint: `GET /api/users/me/preferences` and `PUT /api/users/me/preferences`.

---

## Frontend

### User Preferences Settings Section

New section on the user settings page: **"Score Preferences"**. Shows the current weights as 4 sliders (same design as quiz step 3). Includes a "Reset to defaults" button. Saves via `PUT /api/users/me/preferences`. The `AutoGenerateAiSummary` toggle also lives here (used by Spec 3).

### Personalization Quiz

A modal/sheet (reuses the app's existing modal pattern). Three steps, each as a single page inside the modal. Progress bar shows step N of 3.

**Step 1 — Driving style** (sets efficiency weight):
- "City driving" → efficiency weight +5 (highway weight unchanged)
- "Highway / long distance" → efficiency weight -5
- "Mix of both" → use defaults

**Step 2 — What matters most** (card selection, pick one):
- "Getting the best deal" → value weight +10, others -3 each
- "Low running costs" → efficiency weight +10, others -3 each
- "A reliable car I won't worry about" → reliability weight +10, others -3 each
- "Low mileage for its age" → mileage weight +10, others -3 each

After each step, weights are adjusted within bounds (min 5%, max 50% per factor) and re-normalized to sum to 100.

**Step 3 — Review & adjust sliders** (same design as settings page, but in modal context). Shows `{N}% → your adjusted weights`. Save button → calls `PUT /api/users/me/preferences` → closes modal → score card re-fetches with updated weights.

### Quiz Entry Points

Three places the quiz can be opened:

1. **Inside score card (listing details page):** A small `"Personalize →"` text link with a `SlidersHorizontal` Lucide icon, below the factor pills. Shown only when score is un-personalized. Replaced by `"Personalized ✓"` label when preferences are set.

2. **User settings page:** A "Set up score preferences" card in the Score Preferences section; clicking opens the same quiz modal.

3. **Comparison page sticky banner:** When preferences aren't set, a subtle text below the scores: `"Scores are un-personalized — set your preferences →"`. Clicking opens the quiz modal.

### Score Card Update

When `IsPersonalized = true`:
- Badge gains a small `User` Lucide icon overlay in the bottom-right corner
- Expanded breakdown shows the user's custom weights next to each factor label: `"Value (35%)"` instead of `"Value"`
