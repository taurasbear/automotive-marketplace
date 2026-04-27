# Scoring System Rework Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the separate `QuizModal` + standalone settings controls with a single unified `UserPreferencesDialog` that:
- Shows the quiz flow when the user hasn't completed it yet
- Shows current weights as dials + personalization toggle + "Retake Quiz" when they have

**Architecture:** A new `HasCompletedQuiz: bool` flag is added to the `UserPreferences` entity and DB. A new `UserPreferencesDialog` component replaces `QuizModal` — it drives state with a `view` discriminator (`"quiz" | "settings"`) based on `HasCompletedQuiz`. `ScoreCard` and `Settings` both open this unified dialog instead of their separate flows.

**Tech Stack:** React 19 + TypeScript, Radix UI Dialog (via shadcn), ASP.NET Core 8, EF Core migrations.

---

### Task 1: Add HasCompletedQuiz to UserPreferences Entity and DB

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/UserPreferences.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs` (or AutoMapper profile)
- Add migration: run `dotnet ef migrations add AddHasCompletedQuiz`

- [ ] **Step 1: Add property to entity**

```csharp
// UserPreferences.cs — add after existing weight properties:
public bool HasCompletedQuiz { get; set; } = false;
```

- [ ] **Step 2: Add property to UpsertUserPreferencesCommand**

```csharp
// UpsertUserPreferencesCommand.cs — add:
public bool HasCompletedQuiz { get; set; }
```

- [ ] **Step 3: Set HasCompletedQuiz in handler**

In `UpsertUserPreferencesCommandHandler.cs`, when creating or updating the entity:

```csharp
preferences.HasCompletedQuiz = request.HasCompletedQuiz;
```

- [ ] **Step 4: Add property to GetUserPreferencesResponse**

```csharp
// GetUserPreferencesResponse.cs — add:
public bool HasCompletedQuiz { get; set; }
```

- [ ] **Step 5: Map HasCompletedQuiz in AutoMapper profile (or query handler)**

If using AutoMapper:
```csharp
// In the AutoMapper profile for UserPreferences → GetUserPreferencesResponse:
.ForMember(dest => dest.HasCompletedQuiz, opt => opt.MapFrom(src => src.HasCompletedQuiz));
```

If mapping manually, add `HasCompletedQuiz = preferences.HasCompletedQuiz` in the handler.

- [ ] **Step 6: Generate migration**

```bash
cd Automotive.Marketplace.Infrastructure
dotnet ef migrations add AddHasCompletedQuiz --startup-project ../Automotive.Marketplace.Server
```

- [ ] **Step 7: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln
dotnet test ./Automotive.Marketplace.sln
```

- [ ] **Step 8: Commit**

```bash
git add \
  Automotive.Marketplace.Domain/Entities/UserPreferences.cs \
  Automotive.Marketplace.Application/Features/UserPreferencesFeatures/ \
  Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat: add HasCompletedQuiz to UserPreferences entity

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Add HasCompletedQuiz to the Upsert Mutation

**Files:**
- Modify: `automotive.marketplace.client/src/features/userPreferences/api/useUpsertUserPreferences.ts` (or wherever the mutation hook is)
- Modify: `automotive.marketplace.client/src/features/userPreferences/api/types.ts` (or wherever UpsertUserPreferencesRequest is typed)

- [ ] **Step 1: Add HasCompletedQuiz to request type**

Find the TypeScript type/interface for the upsert request body (grep for `UpsertUserPreferences`):

```bash
grep -r "UpsertUserPreferences\|upsertUserPreferences" automotive.marketplace.client/src/ --include="*.ts" --include="*.tsx" -l
```

Add `hasCompletedQuiz: boolean` to the request interface.

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/userPreferences/
git commit -m "feat: include hasCompletedQuiz in user preferences mutation type

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Create UserPreferencesDialog Component

**Files:**
- Create: `automotive.marketplace.client/src/features/userPreferences/components/UserPreferencesDialog.tsx`

**Context:** This component replaces the usage of `QuizModal` in `ScoreCard` and `Settings`. It has two internal views:
1. `"quiz"` — the 3-step quiz from `QuizModal` (driving style → priority → sliders). Shown when `hasCompletedQuiz === false` OR when user clicks "Retake Quiz".
2. `"settings"` — dials/sliders of current weights, personalization toggle, "Retake Quiz" button. Shown when `hasCompletedQuiz === true`.

The component receives an `open` / `onOpenChange` prop so callers can control visibility.

- [ ] **Step 1: Read the existing QuizModal to understand its structure**

```bash
cat automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx
```

Note: the quiz has `step` state (0–2), a weights form, a `handleSubmit` that calls `upsertUserPreferences`. We'll migrate this logic into `UserPreferencesDialog` and add `hasCompletedQuiz: true` when submitting from quiz step.

- [ ] **Step 2: Create UserPreferencesDialog.tsx**

```tsx
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useGetUserPreferences } from "@/features/userPreferences/api/useGetUserPreferences";
import { useUpsertUserPreferences } from "@/features/userPreferences/api/useUpsertUserPreferences";
// Import whatever quiz step components or inline logic exists in QuizModal

type DialogView = "quiz" | "settings";

interface UserPreferencesDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** Force the quiz view regardless of hasCompletedQuiz (used for "Retake") */
  initialView?: DialogView;
}

export function UserPreferencesDialog({
  open,
  onOpenChange,
  initialView,
}: UserPreferencesDialogProps) {
  const { t } = useTranslation("userPreferences"); // adjust namespace
  const { data: prefs } = useGetUserPreferences();
  const hasCompletedQuiz = prefs?.data?.hasCompletedQuiz ?? false;

  const [view, setView] = useState<DialogView>(
    initialView ?? (hasCompletedQuiz ? "settings" : "quiz"),
  );

  const upsert = useUpsertUserPreferences();

  // When dialog opens, reset to the right view
  // (use useEffect on `open` to re-sync when reopened)
  // ...

  const handleQuizComplete = (weights: WeightsFormValues) => {
    upsert.mutate(
      { ...weights, hasCompletedQuiz: true },
      { onSuccess: () => setView("settings") },
    );
  };

  const handlePreferencesUpdate = (weights: WeightsFormValues) => {
    upsert.mutate(weights); // keep hasCompletedQuiz: true implicitly from BE
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {view === "quiz" ? t("quiz.title") : t("settings.title")}
          </DialogTitle>
        </DialogHeader>

        {view === "quiz" ? (
          <QuizStepsView onComplete={handleQuizComplete} />
        ) : (
          <SettingsView
            preferences={prefs?.data}
            onUpdate={handlePreferencesUpdate}
            onRetakeQuiz={() => setView("quiz")}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
```

The `QuizStepsView` component should contain the exact quiz steps from `QuizModal`. Either inline them here or extract into a sub-component. The `SettingsView` shows the weight dials/sliders, the personalization toggle, and a "Retake Quiz" button.

**Note on SettingsView:** Look at the current Settings.tsx page to see how weights and the personalization toggle are rendered. Extract that same UI into `SettingsView`.

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/userPreferences/components/UserPreferencesDialog.tsx
git commit -m "feat: create unified UserPreferencesDialog (quiz + settings view)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Replace QuizModal Usage in ScoreCard

**Files:**
- Modify: `automotive.marketplace.client/src/features/listings/components/ScoreCard.tsx`

**Context:** `ScoreCard` opens the `QuizModal` when the user clicks "Set up preferences". Replace this with `UserPreferencesDialog`. Since `UserPreferencesDialog` already picks the right view (quiz if not completed, settings if completed), no special prop is needed here.

- [ ] **Step 1: Replace QuizModal with UserPreferencesDialog in ScoreCard.tsx**

```tsx
// Remove:
import { QuizModal } from "@/features/userPreferences/components/QuizModal";

// Add:
import { UserPreferencesDialog } from "@/features/userPreferences/components/UserPreferencesDialog";

// Replace <QuizModal open={...} onOpenChange={...} /> with:
<UserPreferencesDialog open={open} onOpenChange={setOpen} />
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listings/components/ScoreCard.tsx
git commit -m "feat: replace QuizModal with UserPreferencesDialog in ScoreCard

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Replace Separate Quiz/Reset Controls in Settings Page

**Files:**
- Modify: `automotive.marketplace.client/src/app/pages/Settings.tsx`

**Context:** `Settings.tsx` currently has:
- A "Take Quiz" button (opens `QuizModal`)
- A "Reset Preferences" button (calls reset mutation)
- Possibly inline weight sliders

Replace all of that with a single "Manage Scoring Preferences" button that opens `UserPreferencesDialog` with `initialView="quiz"` for retaking. The reset flow should now live inside the dialog.

- [ ] **Step 1: Replace quiz/reset section in Settings.tsx**

```tsx
// Remove:
import { QuizModal } from "@/features/userPreferences/components/QuizModal";
// Remove quiz open state, reset mutation call, reset button, quiz button

// Add:
import { UserPreferencesDialog } from "@/features/userPreferences/components/UserPreferencesDialog";

// Add state:
const [prefsDialogOpen, setPrefsDialogOpen] = useState(false);

// Replace the quiz/reset button section with:
<Button onClick={() => setPrefsDialogOpen(true)}>
  {t("settings.manageScoringPreferences")}
</Button>
<UserPreferencesDialog
  open={prefsDialogOpen}
  onOpenChange={setPrefsDialogOpen}
/>
```

- [ ] **Step 2: Add the new i18n key**

In `en/settings.json` (or whatever the settings namespace is):
```json
{ "manageScoringPreferences": "Manage Scoring Preferences" }
```

In `lt/settings.json`:
```json
{ "manageScoringPreferences": "Tvarkyti vertinimo nuostatas" }
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add \
  automotive.marketplace.client/src/app/pages/Settings.tsx \
  automotive.marketplace.client/src/lib/i18n/locales/
git commit -m "feat: replace quiz/reset buttons in settings with unified preferences dialog

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Remove Standalone QuizModal Component

**Files:**
- Delete: `automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx`

- [ ] **Step 1: Confirm no remaining usages**

```bash
grep -r "QuizModal" automotive.marketplace.client/src/ --include="*.ts" --include="*.tsx"
```
Expected: no results.

- [ ] **Step 2: Delete the file**

```bash
rm automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 4: Commit**

```bash
git add -A automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx
git commit -m "refactor: remove standalone QuizModal component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
