# i18n & Multi-Language Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Internationalize the entire automotive marketplace frontend with English and Lithuanian support, designed for easy addition of more languages.

**Architecture:** react-i18next with browser localStorage detection, namespace-based translation files (8 namespaces), date-fns locale integration, a navbar language switcher, and a utility for backend enum translations. Every user-facing string (~250+) across ~40+ components migrated to translation keys.

**Tech Stack:** i18next, react-i18next, i18next-browser-languagedetector, date-fns locales, shadcn DropdownMenu

---

## File Structure

### New Files

```
src/lib/i18n/
├── i18n.ts                          # i18next initialization & config
├── dateLocale.ts                    # useDateLocale() hook for date-fns
├── getTranslatedName.ts             # Backend enum translation selector utility
└── locales/
    ├── en/
    │   ├── common.json              # Navbar, shared UI, buttons, empty states
    │   ├── auth.json                # Login, register forms
    │   ├── chat.json                # All chat components
    │   ├── listings.json            # Create/edit listing, detail, list, search/filters
    │   ├── saved.json               # Saved listings page
    │   ├── compare.json             # Listing comparison feature
    │   ├── admin.json               # Makes, models, variants management
    │   └── validation.json          # Shared validation message templates
    └── lt/
        ├── common.json
        ├── auth.json
        ├── chat.json
        ├── listings.json
        ├── saved.json
        ├── compare.json
        ├── admin.json
        └── validation.json
src/components/LanguageSwitcher.tsx   # Navbar language dropdown
```

### Modified Files

```
src/app/main.tsx                                      # Import i18n config
src/components/layout/header/Header.tsx                # Add LanguageSwitcher + translate strings
src/features/auth/components/RegisterButton.tsx        # Translate "Sign up"
src/app/pages/Login.tsx                                # Translate form labels
src/app/pages/Register.tsx                             # Translate form labels
src/features/chat/components/ActionBar.tsx              # Translate strings + date locale
src/features/chat/components/MeetingCard.tsx            # Translate strings + date locale
src/features/chat/components/OfferCard.tsx              # Translate strings
src/features/chat/components/AvailabilityCardComponent.tsx # Translate strings + date locale
src/features/chat/components/ConversationList.tsx       # Translate strings + date locale
src/features/chat/components/MessageThread.tsx          # Translate strings + date locale
src/features/chat/components/ChatPanel.tsx              # Translate aria-label
src/features/chat/components/ListingCard.tsx            # Translate strings
src/features/chat/components/MakeOfferModal.tsx         # Translate strings
src/features/chat/components/ProposeMeetingModal.tsx    # Translate strings
src/features/chat/components/ShareAvailabilityModal.tsx # Translate strings
src/features/listingList/components/ListingCard.tsx     # Translate strings
src/features/listingList/components/RangeFilters.tsx    # Translate strings
src/features/listingList/components/BasicFilters.tsx    # Translate strings
src/features/search/components/ListingSearch.tsx        # Translate strings
src/features/search/components/ListingSearchFilters.tsx # Translate strings
src/features/listingDetails/components/ListingDetailsContent.tsx # Translate strings
src/features/listingDetails/components/EditListingDialog.tsx     # Translate strings
src/features/listingDetails/components/EditListingForm.tsx       # Translate strings
src/features/createListing/components/CreateListingForm.tsx      # Translate strings
src/features/savedListings/components/SavedListingsPage.tsx      # Translate strings
src/features/savedListings/components/SavedListingRow.tsx        # Translate strings
src/features/savedListings/components/NoteEditor.tsx             # Translate strings
src/features/savedListings/components/PropertyMentionPicker.tsx  # Translate strings
src/features/compareListings/components/CompareHeader.tsx        # Translate strings
src/features/compareListings/components/CompareTable.tsx         # Translate strings
src/features/compareListings/components/CompareSearchModal.tsx   # Translate strings
src/features/compareListings/components/DiffToggleFab.tsx        # Translate strings
src/features/variantList/components/CreateVariantDialog.tsx      # Translate strings
src/features/variantList/components/VariantForm.tsx              # Translate strings
src/features/variantList/components/VariantListTable.tsx         # Translate strings
src/features/modelList/components/CreateModelDialog.tsx          # Translate strings
src/features/modelList/components/ModelForm.tsx                  # Translate strings
src/features/modelList/components/ModelListTable.tsx             # Translate strings
src/features/makeList/components/CreateMakeDialog.tsx            # Translate strings
src/features/makeList/components/MakeForm.tsx                    # Translate strings
src/features/makeList/components/MakeListTable.tsx               # Translate strings
src/components/common/DatePicker.tsx                             # Translate placeholder
src/components/forms/VariantTable.tsx                            # Translate strings
src/components/forms/select/FuelSelect.tsx                       # Use getTranslatedName
src/components/forms/select/BodyTypeSelect.tsx                   # Use getTranslatedName
src/components/forms/select/MakeSelect.tsx                       # Translate label
src/components/forms/select/ModelSelect.tsx                      # Translate label
src/components/forms/select/LocationCombobox.tsx                 # Translate strings
src/components/forms/select/UsedSelect.tsx                       # Translate label
src/utils/validation.ts                                          # Accept t function for i18n
src/features/auth/schemas/loginSchema.ts                         # Use translated validation
src/features/auth/schemas/registerSchema.ts                      # Use translated validation
src/features/createListing/schemas/createListingSchema.ts        # Use translated validation
src/features/variantList/schemas/variantFormSchema.ts             # Use translated validation
src/features/makeList/schemas/makeFormSchema.ts                   # Use translated validation
```

---

## Task 1: Install Dependencies & Create i18n Infrastructure

**Files:**
- Modify: `automotive.marketplace.client/package.json`
- Create: `src/lib/i18n/i18n.ts`
- Create: `src/lib/i18n/dateLocale.ts`
- Create: `src/lib/i18n/getTranslatedName.ts`
- Modify: `src/app/main.tsx`

- [ ] **Step 1: Install i18n packages**

```bash
cd automotive.marketplace.client
npm install i18next react-i18next i18next-browser-languagedetector
```

- [ ] **Step 2: Create i18n configuration**

Create `src/lib/i18n/i18n.ts`:

```typescript
import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

import adminEn from "./locales/en/admin.json";
import authEn from "./locales/en/auth.json";
import chatEn from "./locales/en/chat.json";
import commonEn from "./locales/en/common.json";
import compareEn from "./locales/en/compare.json";
import listingsEn from "./locales/en/listings.json";
import savedEn from "./locales/en/saved.json";
import validationEn from "./locales/en/validation.json";

import adminLt from "./locales/lt/admin.json";
import authLt from "./locales/lt/auth.json";
import chatLt from "./locales/lt/chat.json";
import commonLt from "./locales/lt/common.json";
import compareLt from "./locales/lt/compare.json";
import listingsLt from "./locales/lt/listings.json";
import savedLt from "./locales/lt/saved.json";
import validationLt from "./locales/lt/validation.json";

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: {
        common: commonEn,
        auth: authEn,
        chat: chatEn,
        listings: listingsEn,
        saved: savedEn,
        compare: compareEn,
        admin: adminEn,
        validation: validationEn,
      },
      lt: {
        common: commonLt,
        auth: authLt,
        chat: chatLt,
        listings: listingsLt,
        saved: savedLt,
        compare: compareLt,
        admin: adminLt,
        validation: validationLt,
      },
    },
    fallbackLng: "en",
    defaultNS: "common",
    detection: {
      order: ["localStorage"],
      lookupLocalStorage: "language",
      caches: ["localStorage"],
    },
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;
```

- [ ] **Step 3: Create date locale hook**

Create `src/lib/i18n/dateLocale.ts`:

```typescript
import { useTranslation } from "react-i18next";
import { lt, enUS } from "date-fns/locale";
import type { Locale } from "date-fns/locale";

const localeMap: Record<string, Locale> = {
  lt: lt,
  en: enUS,
};

export const useDateLocale = (): Locale => {
  const { i18n } = useTranslation();
  return localeMap[i18n.language] ?? lt;
};
```

- [ ] **Step 4: Create backend enum translation utility**

Create `src/lib/i18n/getTranslatedName.ts`:

```typescript
import type { Translation } from "@/types/shared/Translation";

export const getTranslatedName = (
  translations: Translation[],
  language: string,
): string => {
  const match = translations.find((t) => t.languageCode === language);
  if (match) return match.name;

  const fallback = translations.find((t) => t.languageCode === "en");
  if (fallback) return fallback.name;

  return translations[0]?.name ?? "";
};
```

- [ ] **Step 5: Create empty translation files**

Create all 16 translation JSON files with empty objects `{}` so the app compiles. These will be filled in by subsequent tasks.

Create each of these with `{}` as content:
- `src/lib/i18n/locales/en/common.json`
- `src/lib/i18n/locales/en/auth.json`
- `src/lib/i18n/locales/en/chat.json`
- `src/lib/i18n/locales/en/listings.json`
- `src/lib/i18n/locales/en/saved.json`
- `src/lib/i18n/locales/en/compare.json`
- `src/lib/i18n/locales/en/admin.json`
- `src/lib/i18n/locales/en/validation.json`
- `src/lib/i18n/locales/lt/common.json`
- `src/lib/i18n/locales/lt/auth.json`
- `src/lib/i18n/locales/lt/chat.json`
- `src/lib/i18n/locales/lt/listings.json`
- `src/lib/i18n/locales/lt/saved.json`
- `src/lib/i18n/locales/lt/compare.json`
- `src/lib/i18n/locales/lt/admin.json`
- `src/lib/i18n/locales/lt/validation.json`

- [ ] **Step 6: Import i18n in main.tsx**

Modify `src/app/main.tsx` — add a single import at the top, before all other imports:

```typescript
import "@/lib/i18n/i18n";
```

This must be the **first import** in the file so i18n initializes before React renders.

- [ ] **Step 7: Verify the app compiles**

```bash
cd automotive.marketplace.client
npm run build
```

Expected: Build succeeds with no errors.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat(i18n): add i18next infrastructure, date locale hook, and translation utility"
```

---

## Task 2: Language Switcher + Common Namespace

**Files:**
- Create: `src/components/LanguageSwitcher.tsx`
- Modify: `src/components/layout/header/Header.tsx`
- Modify: `src/lib/i18n/locales/en/common.json`
- Modify: `src/lib/i18n/locales/lt/common.json`
- Modify: `src/features/auth/components/RegisterButton.tsx`
- Modify: `src/components/common/DatePicker.tsx`
- Modify: `src/components/forms/VariantTable.tsx`
- Modify: `src/components/forms/select/FuelSelect.tsx`
- Modify: `src/components/forms/select/BodyTypeSelect.tsx`
- Modify: `src/components/forms/select/MakeSelect.tsx`
- Modify: `src/components/forms/select/ModelSelect.tsx`
- Modify: `src/components/forms/select/LocationCombobox.tsx`
- Modify: `src/components/forms/select/UsedSelect.tsx`

- [ ] **Step 1: Write common.json English translations**

Replace `src/lib/i18n/locales/en/common.json`:

```json
{
  "header": {
    "title": "Automotive Marketplace",
    "makes": "Makes",
    "models": "Models",
    "variants": "Variants",
    "sellYourCar": "Sell your car",
    "inbox": "Inbox",
    "savedListings": "Saved listings",
    "signUp": "Sign up"
  },
  "datePicker": {
    "pickADate": "Pick a date"
  },
  "variantTable": {
    "loading": "Loading variants…",
    "failed": "Failed to load variants",
    "fuel": "Fuel",
    "transmission": "Transmission",
    "powerKw": "Power (kW)",
    "engineMl": "Engine (ml)",
    "bodyType": "Body Type",
    "doors": "Doors",
    "noVariants": "No variants available for this model"
  },
  "select": {
    "makes": "Makes",
    "models": "Models",
    "fuelTypes": "Fuel types",
    "bodyTypes": "Body types",
    "location": "Location",
    "searchLocation": "Search location",
    "noLocationFound": "No location found.",
    "usedNew": "Used/New"
  },
  "actions": {
    "cancel": "Cancel",
    "confirm": "Confirm",
    "delete": "Delete",
    "save": "Save"
  }
}
```

- [ ] **Step 2: Write common.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/common.json`:

```json
{
  "header": {
    "title": "Automobilių turgavietė",
    "makes": "Markės",
    "models": "Modeliai",
    "variants": "Variantai",
    "sellYourCar": "Parduok automobilį",
    "inbox": "Žinutės",
    "savedListings": "Išsaugoti skelbimai",
    "signUp": "Registruotis"
  },
  "datePicker": {
    "pickADate": "Pasirinkite datą"
  },
  "variantTable": {
    "loading": "Kraunami variantai…",
    "failed": "Nepavyko įkelti variantų",
    "fuel": "Kuras",
    "transmission": "Pavarų dėžė",
    "powerKw": "Galia (kW)",
    "engineMl": "Variklis (ml)",
    "bodyType": "Kėbulo tipas",
    "doors": "Durys",
    "noVariants": "Šiam modeliui nėra variantų"
  },
  "select": {
    "makes": "Markės",
    "models": "Modeliai",
    "fuelTypes": "Kuro tipai",
    "bodyTypes": "Kėbulo tipai",
    "location": "Vietovė",
    "searchLocation": "Ieškoti vietovės",
    "noLocationFound": "Vietovė nerasta.",
    "usedNew": "Naudotas/Naujas"
  },
  "actions": {
    "cancel": "Atšaukti",
    "confirm": "Patvirtinti",
    "delete": "Ištrinti",
    "save": "Išsaugoti"
  }
}
```

- [ ] **Step 3: Create LanguageSwitcher component**

Create `src/components/LanguageSwitcher.tsx`:

```tsx
import { useTranslation } from "react-i18next";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";

const languages = [
  { code: "lt", label: "LT", flag: "🇱🇹" },
  { code: "en", label: "EN", flag: "🇬🇧" },
] as const;

const LanguageSwitcher = () => {
  const { i18n } = useTranslation();

  const current = languages.find((l) => l.code === i18n.language) ?? languages[0];

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="sm" className="gap-1">
          <span>{current.flag}</span>
          <span>{current.label}</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        {languages.map((lang) => (
          <DropdownMenuItem
            key={lang.code}
            onClick={() => i18n.changeLanguage(lang.code)}
          >
            <span className="mr-2">{lang.flag}</span>
            {lang.label}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};

export default LanguageSwitcher;
```

- [ ] **Step 4: Migrate Header.tsx**

Modify `src/components/layout/header/Header.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Add import: `import LanguageSwitcher from "../../LanguageSwitcher";`
3. Inside the component, add: `const { t } = useTranslation("common");`
4. Replace all hardcoded strings:
   - `"Automotive Marketplace"` → `{t("header.title")}`
   - `"Makes"` → `{t("header.makes")}`
   - `"Models"` → `{t("header.models")}`
   - `"Variants"` → `{t("header.variants")}`
   - `"Sell your car"` → `{t("header.sellYourCar")}`
   - `"Inbox"` → `{t("header.inbox")}`
   - `title="Saved listings"` → `title={t("header.savedListings")}`
5. Add `<LanguageSwitcher />` between the saved-listings icon and `<ThemeToggle />`

- [ ] **Step 5: Migrate RegisterButton.tsx**

Modify `src/features/auth/components/RegisterButton.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Inside the component, add: `const { t } = useTranslation("common");`
3. Replace `"Sign up"` → `{t("header.signUp")}`

- [ ] **Step 6: Migrate DatePicker.tsx**

Modify `src/components/common/DatePicker.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Inside the component, add: `const { t } = useTranslation("common");`
3. Replace `"Pick a date"` → `{t("datePicker.pickADate")}`

- [ ] **Step 7: Migrate VariantTable.tsx**

Modify `src/components/forms/VariantTable.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Inside the component, add: `const { t } = useTranslation("common");`
3. Replace hardcoded strings:
   - `"Loading variants…"` → `{t("variantTable.loading")}`
   - `"Failed to load variants"` → `{t("variantTable.failed")}`
   - `"Fuel"` → `{t("variantTable.fuel")}`
   - `"Transmission"` → `{t("variantTable.transmission")}`
   - `"Power (kW)"` → `{t("variantTable.powerKw")}`
   - `"Engine (ml)"` → `{t("variantTable.engineMl")}`
   - `"Body Type"` → `{t("variantTable.bodyType")}`
   - `"Doors"` → `{t("variantTable.doors")}`
   - `"No variants available for this model"` → `{t("variantTable.noVariants")}`

- [ ] **Step 8: Migrate form select components**

For each select component, add `useTranslation("common")` and replace the label/placeholder:

**FuelSelect.tsx:**
- `import { useTranslation } from "react-i18next";` and `import { getTranslatedName } from "@/lib/i18n/getTranslatedName";`
- `const { t, i18n } = useTranslation("common");`
- Replace `<SelectLabel>Fuel types</SelectLabel>` → `<SelectLabel>{t("select.fuelTypes")}</SelectLabel>`
- Replace `placeholder="Petrol"` → `placeholder={t("select.fuelTypes")}`
- Replace `{fuel.name}` → `{getTranslatedName(fuel.translations, i18n.language)}`

**BodyTypeSelect.tsx:**
- Same pattern as FuelSelect
- Replace `<SelectLabel>Body types</SelectLabel>` → `<SelectLabel>{t("select.bodyTypes")}</SelectLabel>`
- Replace `placeholder="Sedan"` → `placeholder={t("select.bodyTypes")}`
- Replace `{body.name}` → `{getTranslatedName(body.translations, i18n.language)}`

**MakeSelect.tsx:**
- `const { t } = useTranslation("common");`
- Replace `"Makes"` → `t("select.makes")`

**ModelSelect.tsx:**
- `const { t } = useTranslation("common");`
- Replace `"Models"` → `t("select.models")`

**LocationCombobox.tsx:**
- `const { t } = useTranslation("common");`
- Replace `"Location"` → `t("select.location")`
- Replace `"Search location"` → `t("select.searchLocation")`
- Replace `"No location found."` → `t("select.noLocationFound")`

**UsedSelect.tsx:**
- `const { t } = useTranslation("common");`
- Replace `"Used/New"` → `t("select.usedNew")`

- [ ] **Step 9: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

Expected: Build succeeds.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat(i18n): add language switcher and migrate common namespace"
```

---

## Task 3: Auth Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/auth.json`
- Modify: `src/lib/i18n/locales/lt/auth.json`
- Modify: `src/app/pages/Login.tsx`
- Modify: `src/app/pages/Register.tsx`

- [ ] **Step 1: Write auth.json English translations**

Replace `src/lib/i18n/locales/en/auth.json`:

```json
{
  "login": {
    "title": "Login",
    "submit": "Login",
    "fields": {
      "email": "Email",
      "emailPlaceholder": "you@example.com",
      "password": "Password",
      "passwordPlaceholder": "Password"
    }
  },
  "register": {
    "title": "Register",
    "submit": "Register",
    "alreadyHaveAccount": "Already have an account?",
    "fields": {
      "username": "Username",
      "usernamePlaceholder": "Your username",
      "usernameDescription": "This will be your public display name.",
      "email": "Email",
      "emailPlaceholder": "you@example.com",
      "emailDescription": "We'll never share your email.",
      "password": "Password",
      "passwordPlaceholder": "Password",
      "passwordDescription": "At least 6 characters."
    }
  }
}
```

- [ ] **Step 2: Write auth.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/auth.json`:

```json
{
  "login": {
    "title": "Prisijungimas",
    "submit": "Prisijungti",
    "fields": {
      "email": "El. paštas",
      "emailPlaceholder": "jusu@pastas.lt",
      "password": "Slaptažodis",
      "passwordPlaceholder": "Slaptažodis"
    }
  },
  "register": {
    "title": "Registracija",
    "submit": "Registruotis",
    "alreadyHaveAccount": "Jau turite paskyrą?",
    "fields": {
      "username": "Vartotojo vardas",
      "usernamePlaceholder": "Jūsų vardas",
      "usernameDescription": "Tai bus jūsų viešas rodomas vardas.",
      "email": "El. paštas",
      "emailPlaceholder": "jusu@pastas.lt",
      "emailDescription": "Mes niekada nedalinsime jūsų el. pašto.",
      "password": "Slaptažodis",
      "passwordPlaceholder": "Slaptažodis",
      "passwordDescription": "Bent 6 simboliai."
    }
  }
}
```

- [ ] **Step 3: Migrate Login.tsx**

Modify `src/app/pages/Login.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Inside the component, add: `const { t } = useTranslation("auth");`
3. Replace hardcoded strings:
   - `<h2>Login</h2>` → `<h2>{t("login.title")}</h2>`
   - `<FormLabel>Email</FormLabel>` → `<FormLabel>{t("login.fields.email")}</FormLabel>`
   - `placeholder="you@example.com"` → `placeholder={t("login.fields.emailPlaceholder")}`
   - `<FormLabel>Password</FormLabel>` → `<FormLabel>{t("login.fields.password")}</FormLabel>`
   - `placeholder="Password"` → `placeholder={t("login.fields.passwordPlaceholder")}`
   - `>Login</Button>` → `>{t("login.submit")}</Button>`

- [ ] **Step 4: Migrate Register.tsx**

Modify `src/app/pages/Register.tsx`:

1. Add import: `import { useTranslation } from "react-i18next";`
2. Inside the component, add: `const { t } = useTranslation("auth");`
3. Replace hardcoded strings:
   - `<h2>Register</h2>` → `<h2>{t("register.title")}</h2>`
   - `<FormLabel>Username</FormLabel>` → `<FormLabel>{t("register.fields.username")}</FormLabel>`
   - `placeholder="Your username"` → `placeholder={t("register.fields.usernamePlaceholder")}`
   - `<FormDescription>This will be your public display name.</FormDescription>` → `<FormDescription>{t("register.fields.usernameDescription")}</FormDescription>`
   - `<FormLabel>Email</FormLabel>` → `<FormLabel>{t("register.fields.email")}</FormLabel>`
   - `placeholder="you@example.com"` → `placeholder={t("register.fields.emailPlaceholder")}`
   - `` <FormDescription>{`We'll never share your email.`}</FormDescription> `` → `<FormDescription>{t("register.fields.emailDescription")}</FormDescription>`
   - `<FormLabel>Password</FormLabel>` → `<FormLabel>{t("register.fields.password")}</FormLabel>`
   - `placeholder="Password"` → `placeholder={t("register.fields.passwordPlaceholder")}`
   - `<FormDescription>At least 6 characters.</FormDescription>` → `<FormDescription>{t("register.fields.passwordDescription")}</FormDescription>`
   - `>Register</Button>` (submit) → `>{t("register.submit")}</Button>`
   - `>Already have an account?</Button>` → `>{t("register.alreadyHaveAccount")}</Button>`

- [ ] **Step 5: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate auth namespace (login/register)"
```

---

## Task 4: Chat Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/chat.json`
- Modify: `src/lib/i18n/locales/lt/chat.json`
- Modify: 11 chat component files (ActionBar, MeetingCard, OfferCard, AvailabilityCardComponent, ConversationList, MessageThread, ChatPanel, ListingCard, MakeOfferModal, ProposeMeetingModal, ShareAvailabilityModal)

- [ ] **Step 1: Write chat.json English translations**

Replace `src/lib/i18n/locales/en/chat.json`:

```json
{
  "actionBar": {
    "offerAlreadyPending": "An offer is already pending in this conversation",
    "meetupAlreadyActive": "A meetup negotiation is already active",
    "makeAnOffer": "Make an Offer",
    "proposeATime": "Propose a time",
    "shareAvailability": "Share availability",
    "cancelExistingMeetup": "Cancel existing meetup?",
    "confirmedMeetupWarning": "You have a confirmed meetup on {{date}} at {{time}}. Starting a new negotiation will cancel it.",
    "activeMeetupWarning": "Starting a new negotiation will cancel the existing meetup.",
    "keepExisting": "Keep existing",
    "continue": "Continue"
  },
  "meetingCard": {
    "statusLabels": {
      "proposed": "Meetup Proposed",
      "confirmed": "Meetup Confirmed",
      "declined": "Meetup Declined",
      "rescheduled": "Reschedule Proposed",
      "expired": "Meetup Expired",
      "cancelled": "Meetup Cancelled",
      "superseded": "Superseded"
    },
    "subtitles": {
      "awaitingResponse": "Awaiting response",
      "seeYouThere": "See you there!",
      "notHappening": "Not happening",
      "noResponseInTime": "No response in time",
      "withdrawn": "Withdrawn"
    },
    "actions": {
      "accept": "Accept",
      "suggestAlternative": "Suggest alternative",
      "proposeCounterTime": "Propose a counter time",
      "shareMyAvailability": "Share my availability",
      "decline": "Decline",
      "cancelMeetup": "Cancel meetup"
    }
  },
  "offerCard": {
    "offer": "Offer",
    "counterOffer": "Counter-Offer",
    "statusLabels": {
      "pending": "Pending response",
      "accepted": "Offer Accepted",
      "declined": "Offer Declined",
      "expired": "Offer Expired"
    },
    "subtitles": {
      "listingOnHold": "Listing is now on hold",
      "noDealReached": "No deal reached",
      "awaitingResponse": "Awaiting response",
      "noResponseWithin48h": "No response within 48 hours"
    },
    "actions": {
      "accept": "Accept",
      "counter": "Counter",
      "decline": "Decline"
    }
  },
  "availabilityCard": {
    "statusLabels": {
      "shared": "Availability Shared",
      "responded": "Availability Responded",
      "expired": "Availability Expired",
      "cancelled": "Availability Cancelled"
    },
    "close": "Close",
    "propose": "Propose",
    "startTime": "Start time",
    "duration": "Duration",
    "startTimeMustBeAtOrAfter": "Start time must be at or after {{time}}",
    "meetingMustEndBy": "Meeting must end by {{time}}",
    "cancelAvailability": "Cancel availability",
    "noneOfTheseWork": "None of these work — share my availability instead"
  },
  "conversationList": {
    "noConversationsYet": "No conversations yet."
  },
  "messageThread": {
    "meetupConfirmed": "Meetup Confirmed",
    "failedToSend": "Failed to send message.",
    "placeholder": "Message {{username}}...",
    "send": "Send"
  },
  "chatPanel": {
    "closeChat": "Close chat"
  },
  "listingCard": {
    "viewListing": "View listing"
  },
  "makeOfferModal": {
    "counterOfferTitle": "Counter Offer",
    "makeOfferTitle": "Make an Offer",
    "listedPrice": "Listed price",
    "discount": "Discount",
    "yourOffer": "Your offer (€)",
    "minOffer": "Minimum offer is €{{amount}} (⅓ of asking price).",
    "maxOffer": "Offer cannot exceed the listing price of €{{price}}.",
    "sendCounter": "Send Counter",
    "sendOffer": "Send Offer"
  },
  "proposeMeetingModal": {
    "rescheduleTitle": "Reschedule Meeting",
    "proposeTitle": "Propose a Meetup",
    "date": "Date",
    "startTime": "Start time ({{timezone}})",
    "duration": "Duration",
    "durationMinutes": "{{d}} min",
    "locationOptional": "Location (optional)",
    "locationPlaceholder": "e.g. Central Park, 5th Ave entrance",
    "setPinOptional": "Set pin (optional)",
    "latitude": "Latitude",
    "latitudePlaceholder": "e.g. 40.7829",
    "longitude": "Longitude",
    "longitudePlaceholder": "e.g. -73.9654",
    "mustBeInFuture": "Meeting time must be in the future.",
    "sendReschedule": "Send Reschedule",
    "proposeMeetup": "Propose Meetup"
  },
  "shareAvailabilityModal": {
    "title": "Share Your Availability",
    "slot": "Slot {{number}}",
    "date": "Date",
    "from": "From ({{timezone}})",
    "to": "To ({{timezone}})",
    "addAnotherSlot": "+ Add another slot",
    "shareAvailability": "Share Availability"
  }
}
```

- [ ] **Step 2: Write chat.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/chat.json`:

```json
{
  "actionBar": {
    "offerAlreadyPending": "Šiame pokalbyje jau yra laukiantis pasiūlymas",
    "meetupAlreadyActive": "Susitikimo derybos jau vyksta",
    "makeAnOffer": "Pateikti pasiūlymą",
    "proposeATime": "Pasiūlyti laiką",
    "shareAvailability": "Dalintis prieinamumu",
    "cancelExistingMeetup": "Atšaukti esamą susitikimą?",
    "confirmedMeetupWarning": "Turite patvirtintą susitikimą {{date}}, {{time}}. Pradėjus naujas derybas, jis bus atšauktas.",
    "activeMeetupWarning": "Pradėjus naujas derybas, esamas susitikimas bus atšauktas.",
    "keepExisting": "Palikti esamą",
    "continue": "Tęsti"
  },
  "meetingCard": {
    "statusLabels": {
      "proposed": "Susitikimas pasiūlytas",
      "confirmed": "Susitikimas patvirtintas",
      "declined": "Susitikimas atmestas",
      "rescheduled": "Perkėlimas pasiūlytas",
      "expired": "Susitikimas baigė galioti",
      "cancelled": "Susitikimas atšauktas",
      "superseded": "Pakeistas"
    },
    "subtitles": {
      "awaitingResponse": "Laukiama atsakymo",
      "seeYouThere": "Iki pasimatymo!",
      "notHappening": "Neįvyks",
      "noResponseInTime": "Negautas atsakymas laiku",
      "withdrawn": "Atsiimta"
    },
    "actions": {
      "accept": "Priimti",
      "suggestAlternative": "Siūlyti alternatyvą",
      "proposeCounterTime": "Pasiūlyti kitą laiką",
      "shareMyAvailability": "Dalintis prieinamumu",
      "decline": "Atmesti",
      "cancelMeetup": "Atšaukti susitikimą"
    }
  },
  "offerCard": {
    "offer": "Pasiūlymas",
    "counterOffer": "Kontra-pasiūlymas",
    "statusLabels": {
      "pending": "Laukiama atsakymo",
      "accepted": "Pasiūlymas priimtas",
      "declined": "Pasiūlymas atmestas",
      "expired": "Pasiūlymas baigė galioti"
    },
    "subtitles": {
      "listingOnHold": "Skelbimas sulaikytas",
      "noDealReached": "Sandoris neįvyko",
      "awaitingResponse": "Laukiama atsakymo",
      "noResponseWithin48h": "Negautas atsakymas per 48 valandas"
    },
    "actions": {
      "accept": "Priimti",
      "counter": "Kontra",
      "decline": "Atmesti"
    }
  },
  "availabilityCard": {
    "statusLabels": {
      "shared": "Prieinamumas pasidalintas",
      "responded": "Prieinamumas atsakytas",
      "expired": "Prieinamumas baigė galioti",
      "cancelled": "Prieinamumas atšauktas"
    },
    "close": "Uždaryti",
    "propose": "Pasiūlyti",
    "startTime": "Pradžios laikas",
    "duration": "Trukmė",
    "startTimeMustBeAtOrAfter": "Pradžios laikas turi būti ne ankstesnis nei {{time}}",
    "meetingMustEndBy": "Susitikimas turi baigtis iki {{time}}",
    "cancelAvailability": "Atšaukti prieinamumą",
    "noneOfTheseWork": "Nė vienas netinka — dalintis savo prieinamumu"
  },
  "conversationList": {
    "noConversationsYet": "Pokalbių dar nėra."
  },
  "messageThread": {
    "meetupConfirmed": "Susitikimas patvirtintas",
    "failedToSend": "Nepavyko išsiųsti žinutės.",
    "placeholder": "Rašyti {{username}}...",
    "send": "Siųsti"
  },
  "chatPanel": {
    "closeChat": "Uždaryti pokalbį"
  },
  "listingCard": {
    "viewListing": "Žiūrėti skelbimą"
  },
  "makeOfferModal": {
    "counterOfferTitle": "Kontra-pasiūlymas",
    "makeOfferTitle": "Pateikti pasiūlymą",
    "listedPrice": "Skelbiama kaina",
    "discount": "Nuolaida",
    "yourOffer": "Jūsų pasiūlymas (€)",
    "minOffer": "Minimali suma – {{amount}} € (⅓ prašomos kainos).",
    "maxOffer": "Pasiūlymas negali viršyti skelbimo kainos – {{price}} €.",
    "sendCounter": "Siųsti kontra",
    "sendOffer": "Siųsti pasiūlymą"
  },
  "proposeMeetingModal": {
    "rescheduleTitle": "Perkelti susitikimą",
    "proposeTitle": "Pasiūlyti susitikimą",
    "date": "Data",
    "startTime": "Pradžios laikas ({{timezone}})",
    "duration": "Trukmė",
    "durationMinutes": "{{d}} min",
    "locationOptional": "Vieta (neprivaloma)",
    "locationPlaceholder": "pvz. Gedimino pr. 1, Vilnius",
    "setPinOptional": "Nustatyti žymeklį (neprivaloma)",
    "latitude": "Platuma",
    "latitudePlaceholder": "pvz. 54.6872",
    "longitude": "Ilguma",
    "longitudePlaceholder": "pvz. 25.2797",
    "mustBeInFuture": "Susitikimo laikas turi būti ateityje.",
    "sendReschedule": "Siųsti perkėlimą",
    "proposeMeetup": "Siūlyti susitikimą"
  },
  "shareAvailabilityModal": {
    "title": "Dalintis prieinamumu",
    "slot": "Laikas {{number}}",
    "date": "Data",
    "from": "Nuo ({{timezone}})",
    "to": "Iki ({{timezone}})",
    "addAnotherSlot": "+ Pridėti kitą laiką",
    "shareAvailability": "Dalintis prieinamumu"
  }
}
```

- [ ] **Step 3: Migrate chat components**

For **each** chat component, apply this pattern:

1. Add `import { useTranslation } from "react-i18next";`
2. Add `const { t } = useTranslation("chat");`
3. Replace hardcoded strings with `t("key")` calls
4. For components using date-fns, also add:
   ```typescript
   import { useDateLocale } from "@/lib/i18n/dateLocale";
   const locale = useDateLocale();
   ```
   Then pass `{ locale }` to every `format()`, `formatDistanceToNow()` call.

**Component-by-component replacements:**

**ActionBar.tsx** — uses `t("actionBar.*")`, date locale for meetup warning:
- `format(new Date(acceptedMeeting.proposedAt), "EEE, MMM d", { locale })` for date
- `format(new Date(acceptedMeeting.proposedAt), "HH:mm", { locale })` for time
- Pass these into `t("actionBar.confirmedMeetupWarning", { date, time })`

**MeetingCard.tsx** — uses `t("meetingCard.*")`, date locale for all `format()` calls:
- `format(proposedDate, "d", { locale })`, `format(proposedDate, "MMM", { locale })`, etc.

**OfferCard.tsx** — uses `t("offerCard.*")`

**AvailabilityCardComponent.tsx** — uses `t("availabilityCard.*")`, date locale for slot display

**ConversationList.tsx** — uses `t("conversationList.*")`, date locale for `formatDistanceToNow()`

**MessageThread.tsx** — uses `t("messageThread.*")`, date locale for `format()` calls:
- `t("messageThread.placeholder", { username })`

**ChatPanel.tsx** — uses `t("chatPanel.closeChat")` for aria-label

**ListingCard.tsx** (chat) — uses `t("listingCard.viewListing")`

**MakeOfferModal.tsx** — uses `t("makeOfferModal.*")` + `t("actions.cancel")` from common:
- `t("makeOfferModal.minOffer", { amount })` with interpolation
- `t("makeOfferModal.maxOffer", { price })` with interpolation
- For the cancel button, use `useTranslation(["chat", "common"])` and `t("common:actions.cancel")`

**ProposeMeetingModal.tsx** — uses `t("proposeMeetingModal.*")` + `t("actions.cancel")` from common:
- `t("proposeMeetingModal.durationMinutes", { d })` with interpolation
- `t("proposeMeetingModal.startTime", { timezone })` with interpolation

**ShareAvailabilityModal.tsx** — uses `t("shareAvailabilityModal.*")` + `t("actions.cancel")` from common:
- `t("shareAvailabilityModal.slot", { number: i + 1 })` with interpolation
- `t("shareAvailabilityModal.from", { timezone })` with interpolation
- `t("shareAvailabilityModal.to", { timezone })` with interpolation

- [ ] **Step 4: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate chat namespace (all chat components + date locales)"
```

---

## Task 5: Listings Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/listings.json`
- Modify: `src/lib/i18n/locales/lt/listings.json`
- Modify: `src/features/listingList/components/ListingCard.tsx`
- Modify: `src/features/listingList/components/RangeFilters.tsx`
- Modify: `src/features/listingList/components/BasicFilters.tsx`
- Modify: `src/features/search/components/ListingSearch.tsx`
- Modify: `src/features/search/components/ListingSearchFilters.tsx`
- Modify: `src/features/listingDetails/components/ListingDetailsContent.tsx`
- Modify: `src/features/listingDetails/components/EditListingDialog.tsx`
- Modify: `src/features/listingDetails/components/EditListingForm.tsx`
- Modify: `src/features/createListing/components/CreateListingForm.tsx`

- [ ] **Step 1: Write listings.json English translations**

Replace `src/lib/i18n/locales/en/listings.json`:

```json
{
  "card": {
    "used": "Used",
    "new": "New",
    "engine": "Engine",
    "fuelType": "Fuel Type",
    "gearBox": "Gear Box",
    "location": "Location",
    "checkOut": "Check out"
  },
  "filters": {
    "make": "Make",
    "usedNew": "Used/New",
    "location": "Location",
    "model": "Model",
    "year": "Year",
    "min": "Min",
    "max": "Max",
    "price": "Price (€)",
    "mileage": "Mileage (km)",
    "powerKw": "Power (kW)",
    "minYear": "Min year",
    "maxYear": "Max year",
    "minPrice": "Min price",
    "maxPrice": "Max price"
  },
  "search": {
    "lookUp": "Look up",
    "search": "Search"
  },
  "details": {
    "description": "Description",
    "specifications": "Specifications",
    "mileage": "Mileage",
    "engine": "Engine",
    "transmission": "Transmission",
    "drivetrain": "Drivetrain",
    "fuelType": "Fuel Type",
    "bodyType": "Body Type",
    "colour": "Colour",
    "doors": "Doors",
    "steering": "Steering",
    "rightHand": "Right-hand",
    "leftHand": "Left-hand",
    "vin": "VIN",
    "seller": "Seller",
    "contactSeller": "Contact Seller",
    "compareWithAnother": "Compare with another listing"
  },
  "edit": {
    "editListing": "Edit listing",
    "saveChanges": "Save Changes"
  },
  "form": {
    "carMake": "Car make*",
    "carModel": "Car model*",
    "year": "Year*",
    "specificationsLockedFromVariant": "Specifications (locked from variant)",
    "specifications": "Specifications",
    "carIsModified": "Car is modified",
    "fuelType": "Fuel type*",
    "transmission": "Transmission*",
    "bodyType": "Body type*",
    "enginePowerKw": "Engine power (kW)*",
    "engineSizeMl": "Engine size (ml)*",
    "doorCount": "Door count*",
    "colour": "Colour",
    "colourPlaceholder": "Crimson",
    "vinLabel": "VIN",
    "vinPlaceholder": "1G1JC524417418958",
    "drivetrain": "Drivetrain*",
    "carPrice": "Car price (€)*",
    "mileage": "Mileage (km)*",
    "city": "City*",
    "cityPlaceholder": "Kaunas",
    "descriptionLabel": "Description",
    "vehicleImages": "Vehicle images*",
    "steeringWheelRight": "Steering wheel on right",
    "usedCar": "Used car",
    "submit": "Submit"
  }
}
```

- [ ] **Step 2: Write listings.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/listings.json`:

```json
{
  "card": {
    "used": "Naudotas",
    "new": "Naujas",
    "engine": "Variklis",
    "fuelType": "Kuro tipas",
    "gearBox": "Pavarų dėžė",
    "location": "Vieta",
    "checkOut": "Peržiūrėti"
  },
  "filters": {
    "make": "Markė",
    "usedNew": "Naudotas/Naujas",
    "location": "Vietovė",
    "model": "Modelis",
    "year": "Metai",
    "min": "Min",
    "max": "Maks",
    "price": "Kaina (€)",
    "mileage": "Rida (km)",
    "powerKw": "Galia (kW)",
    "minYear": "Min metai",
    "maxYear": "Maks metai",
    "minPrice": "Min kaina",
    "maxPrice": "Maks kaina"
  },
  "search": {
    "lookUp": "Paieška",
    "search": "Ieškoti"
  },
  "details": {
    "description": "Aprašymas",
    "specifications": "Specifikacijos",
    "mileage": "Rida",
    "engine": "Variklis",
    "transmission": "Pavarų dėžė",
    "drivetrain": "Pavara",
    "fuelType": "Kuro tipas",
    "bodyType": "Kėbulo tipas",
    "colour": "Spalva",
    "doors": "Durys",
    "steering": "Vairas",
    "rightHand": "Dešinėje",
    "leftHand": "Kairėje",
    "vin": "VIN",
    "seller": "Pardavėjas",
    "contactSeller": "Susisiekti su pardavėju",
    "compareWithAnother": "Palyginti su kitu skelbimu"
  },
  "edit": {
    "editListing": "Redaguoti skelbimą",
    "saveChanges": "Išsaugoti pakeitimus"
  },
  "form": {
    "carMake": "Automobilio markė*",
    "carModel": "Automobilio modelis*",
    "year": "Metai*",
    "specificationsLockedFromVariant": "Specifikacijos (užrakinta iš varianto)",
    "specifications": "Specifikacijos",
    "carIsModified": "Automobilis modifikuotas",
    "fuelType": "Kuro tipas*",
    "transmission": "Pavarų dėžė*",
    "bodyType": "Kėbulo tipas*",
    "enginePowerKw": "Variklio galia (kW)*",
    "engineSizeMl": "Variklio tūris (ml)*",
    "doorCount": "Durų skaičius*",
    "colour": "Spalva",
    "colourPlaceholder": "Vyšninė",
    "vinLabel": "VIN",
    "vinPlaceholder": "1G1JC524417418958",
    "drivetrain": "Pavara*",
    "carPrice": "Automobilio kaina (€)*",
    "mileage": "Rida (km)*",
    "city": "Miestas*",
    "cityPlaceholder": "Kaunas",
    "descriptionLabel": "Aprašymas",
    "vehicleImages": "Automobilio nuotraukos*",
    "steeringWheelRight": "Vairas dešinėje",
    "usedCar": "Naudotas automobilis",
    "submit": "Pateikti"
  }
}
```

- [ ] **Step 3: Migrate listing list components**

**ListingCard.tsx** (listingList):
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace: `"Used"` → `t("card.used")`, `"New"` → `t("card.new")`, `"Engine"` → `t("card.engine")`, `"Fuel Type"` → `t("card.fuelType")`, `"Gear Box"` → `t("card.gearBox")`, `"Location"` → `t("card.location")`, `"Check out"` → `t("card.checkOut")`

**RangeFilters.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace: `"Year"` → `t("filters.year")`, `"Min"` → `t("filters.min")`, `"Max"` → `t("filters.max")`, `"Price (€)"` → `t("filters.price")`, `"Mileage (km)"` → `t("filters.mileage")`, `"Power (kW)"` → `t("filters.powerKw")`

**BasicFilters.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace: `"Make"` → `t("filters.make")`, `"Used/New"` → `t("filters.usedNew")`, `"Location"` → `t("filters.location")`

- [ ] **Step 4: Migrate search components**

**ListingSearch.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace: `"Look up"` → `t("search.lookUp")`, `"Search"` → `t("search.search")`

**ListingSearchFilters.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace: `"Make"` → `t("filters.make")`, `"Model"` → `t("filters.model")`, `"Location"` → `t("filters.location")`, `"Used/New"` → `t("filters.usedNew")`, `"Min year"` → `t("filters.minYear")`, `"Max year"` → `t("filters.maxYear")`, `"Min price"` → `t("filters.minPrice")`, `"Max price"` → `t("filters.maxPrice")`

- [ ] **Step 5: Migrate listing details components**

**ListingDetailsContent.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `import { getTranslatedName } from "@/lib/i18n/getTranslatedName";`
3. `const { t, i18n } = useTranslation("listings");`
4. Replace all spec labels with `t("details.*")` keys
5. For enum display values (transmission, drivetrain, fuelType, bodyType), use `getTranslatedName(entity.translations, i18n.language)` instead of `entity.name`

**EditListingDialog.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace `"Edit listing"` → `t("edit.editListing")`

**EditListingForm.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace all form labels with `t("form.*")` keys
4. Replace `"Save Changes"` → `t("edit.saveChanges")`

- [ ] **Step 6: Migrate CreateListingForm.tsx**

1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("listings");`
3. Replace all form labels, placeholders, and checkbox labels with `t("form.*")` keys
4. For enum dropdowns (fuel, transmission, bodyType, drivetrain), use `getTranslatedName` where applicable

- [ ] **Step 7: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate listings namespace (list, details, create/edit, search)"
```

---

## Task 6: Saved Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/saved.json`
- Modify: `src/lib/i18n/locales/lt/saved.json`
- Modify: `src/features/savedListings/components/SavedListingsPage.tsx`
- Modify: `src/features/savedListings/components/SavedListingRow.tsx`
- Modify: `src/features/savedListings/components/NoteEditor.tsx`
- Modify: `src/features/savedListings/components/PropertyMentionPicker.tsx`

- [ ] **Step 1: Write saved.json English translations**

Replace `src/lib/i18n/locales/en/saved.json`:

```json
{
  "page": {
    "title": "Saved Listings",
    "emptyState": "You haven't saved any listings yet.",
    "browseListings": "Browse listings"
  },
  "row": {
    "removeFromSaved": "Remove from saved",
    "noImage": "No image"
  },
  "noteEditor": {
    "placeholder": "Click to add a note…"
  },
  "propertyMention": {
    "mileage": "Mileage",
    "price": "Price",
    "fuel": "Fuel",
    "transmission": "Transmission",
    "city": "City"
  }
}
```

- [ ] **Step 2: Write saved.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/saved.json`:

```json
{
  "page": {
    "title": "Išsaugoti skelbimai",
    "emptyState": "Dar neišsaugojote jokių skelbimų.",
    "browseListings": "Naršyti skelbimus"
  },
  "row": {
    "removeFromSaved": "Pašalinti iš išsaugotų",
    "noImage": "Nėra nuotraukos"
  },
  "noteEditor": {
    "placeholder": "Spustelėkite, kad pridėtumėte pastabą…"
  },
  "propertyMention": {
    "mileage": "Rida",
    "price": "Kaina",
    "fuel": "Kuras",
    "transmission": "Pavarų dėžė",
    "city": "Miestas"
  }
}
```

- [ ] **Step 3: Migrate saved listing components**

**SavedListingsPage.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("saved");`
3. Replace: `"Saved Listings"` → `t("page.title")`, `"You haven't saved any listings yet."` → `t("page.emptyState")`, `"Browse listings"` → `t("page.browseListings")`

**SavedListingRow.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("saved");`
3. Replace: `"Remove from saved"` → `t("row.removeFromSaved")`, `"No image"` → `t("row.noImage")`

**NoteEditor.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("saved");`
3. Replace: `"Click to add a note…"` → `t("noteEditor.placeholder")`

**PropertyMentionPicker.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("saved");`
3. Replace each property label: `"Mileage"` → `t("propertyMention.mileage")`, `"Price"` → `t("propertyMention.price")`, `"Fuel"` → `t("propertyMention.fuel")`, `"Transmission"` → `t("propertyMention.transmission")`, `"City"` → `t("propertyMention.city")`

- [ ] **Step 4: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate saved namespace"
```

---

## Task 7: Compare Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/compare.json`
- Modify: `src/lib/i18n/locales/lt/compare.json`
- Modify: `src/features/compareListings/components/CompareHeader.tsx`
- Modify: `src/features/compareListings/components/CompareTable.tsx`
- Modify: `src/features/compareListings/components/CompareSearchModal.tsx`
- Modify: `src/features/compareListings/components/DiffToggleFab.tsx`

- [ ] **Step 1: Write compare.json English translations**

Replace `src/lib/i18n/locales/en/compare.json`:

```json
{
  "header": {
    "specification": "Specification",
    "change": "Change",
    "changeListingA": "Change listing A",
    "changeListingB": "Change listing B"
  },
  "table": {
    "basicInfo": "Basic Info",
    "make": "Make",
    "model": "Model",
    "bodyType": "Body Type",
    "year": "Year",
    "condition": "Condition",
    "used": "Used",
    "new": "New",
    "mileage": "Mileage",
    "city": "City",
    "engineAndPerformance": "Engine & Performance",
    "powerKw": "Power (kW)",
    "engineSizeMl": "Engine Size (ml)",
    "fuelType": "Fuel Type",
    "transmission": "Transmission",
    "drivetrain": "Drivetrain",
    "listingDetails": "Listing Details",
    "price": "Price",
    "status": "Status",
    "seller": "Seller"
  },
  "searchModal": {
    "title": "Compare with another listing",
    "searchDescription": "Search for a listing to compare",
    "searchPlaceholder": "Search by make, model, year, seller…",
    "yourSavedListings": "Your saved listings",
    "noImage": "No Image",
    "saved": "❤ Saved",
    "compare": "Compare",
    "noResults": "No results found"
  },
  "diffToggle": {
    "showAll": "Show All",
    "diffOnly": "Diff Only"
  }
}
```

- [ ] **Step 2: Write compare.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/compare.json`:

```json
{
  "header": {
    "specification": "Specifikacija",
    "change": "Keisti",
    "changeListingA": "Keisti skelbimą A",
    "changeListingB": "Keisti skelbimą B"
  },
  "table": {
    "basicInfo": "Pagrindinė informacija",
    "make": "Markė",
    "model": "Modelis",
    "bodyType": "Kėbulo tipas",
    "year": "Metai",
    "condition": "Būklė",
    "used": "Naudotas",
    "new": "Naujas",
    "mileage": "Rida",
    "city": "Miestas",
    "engineAndPerformance": "Variklis ir charakteristikos",
    "powerKw": "Galia (kW)",
    "engineSizeMl": "Variklio tūris (ml)",
    "fuelType": "Kuro tipas",
    "transmission": "Pavarų dėžė",
    "drivetrain": "Pavara",
    "listingDetails": "Skelbimo duomenys",
    "price": "Kaina",
    "status": "Būsena",
    "seller": "Pardavėjas"
  },
  "searchModal": {
    "title": "Palyginti su kitu skelbimu",
    "searchDescription": "Ieškokite skelbimo palyginimui",
    "searchPlaceholder": "Ieškoti pagal markę, modelį, metus, pardavėją…",
    "yourSavedListings": "Jūsų išsaugoti skelbimai",
    "noImage": "Nėra nuotraukos",
    "saved": "❤ Išsaugotas",
    "compare": "Palyginti",
    "noResults": "Rezultatų nerasta"
  },
  "diffToggle": {
    "showAll": "Rodyti viską",
    "diffOnly": "Tik skirtumai"
  }
}
```

- [ ] **Step 3: Migrate compare components**

**CompareHeader.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("compare");`
3. Replace: `"Specification"` → `t("header.specification")`, `"Change"` → `t("header.change")`, aria-labels with `t("header.changeListingA")` / `t("header.changeListingB")`

**CompareTable.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `import { getTranslatedName } from "@/lib/i18n/getTranslatedName";`
3. `const { t, i18n } = useTranslation("compare");`
4. Replace all section headers and row labels with `t("table.*")` keys
5. Replace `"Used"` / `"New"` → `t("table.used")` / `t("table.new")`
6. For enum values (bodyType, fuelType, transmission, drivetrain), use `getTranslatedName(entity.translations, i18n.language)`

**CompareSearchModal.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("compare");`
3. Replace all strings with `t("searchModal.*")` keys

**DiffToggleFab.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("compare");`
3. Replace: `"Show All"` → `t("diffToggle.showAll")`, `"Diff Only"` → `t("diffToggle.diffOnly")`

- [ ] **Step 4: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate compare namespace"
```

---

## Task 8: Admin Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/admin.json`
- Modify: `src/lib/i18n/locales/lt/admin.json`
- Modify: 9 admin component files (makes, models, variants)

- [ ] **Step 1: Write admin.json English translations**

Replace `src/lib/i18n/locales/en/admin.json`:

```json
{
  "makes": {
    "addMake": "Add make",
    "createNewMake": "Create new make",
    "makeName": "Make name",
    "tableDescription": "A list of makes",
    "name": "Name",
    "createdBy": "Created by",
    "createdAt": "Created at",
    "actions": "Actions",
    "deleteConfirm": "Delete {{name}}?",
    "deleteWarning": "This action cannot be undone."
  },
  "models": {
    "addModel": "Add model",
    "createNewModel": "Create new model",
    "modelName": "Model name",
    "make": "Make",
    "tableDescription": "A list of models",
    "name": "Name",
    "createdBy": "Created by",
    "createdAt": "Created at",
    "actions": "Actions"
  },
  "variants": {
    "addVariant": "Add variant",
    "createNewVariant": "Create new variant",
    "make": "Make*",
    "model": "Model*",
    "fuelType": "Fuel type*",
    "transmission": "Transmission*",
    "bodyType": "Body type*",
    "doors": "Doors*",
    "powerKw": "Power (kW)*",
    "engineSizeMl": "Engine size (ml)*",
    "customVariant": "Custom variant",
    "tableDescription": "A list of variants",
    "loading": "Loading variants…",
    "failed": "Failed to load variants.",
    "fuel": "Fuel",
    "transmission_col": "Transmission",
    "bodyType_col": "Body type",
    "doors_col": "Doors",
    "powerKw_col": "Power (kW)",
    "engineMl_col": "Engine (ml)",
    "custom": "Custom",
    "actions": "Actions",
    "yes": "Yes",
    "no": "No"
  }
}
```

- [ ] **Step 2: Write admin.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/admin.json`:

```json
{
  "makes": {
    "addMake": "Pridėti markę",
    "createNewMake": "Sukurti naują markę",
    "makeName": "Markės pavadinimas",
    "tableDescription": "Markių sąrašas",
    "name": "Pavadinimas",
    "createdBy": "Sukūrė",
    "createdAt": "Sukurta",
    "actions": "Veiksmai",
    "deleteConfirm": "Ištrinti {{name}}?",
    "deleteWarning": "Šio veiksmo negalima atšaukti."
  },
  "models": {
    "addModel": "Pridėti modelį",
    "createNewModel": "Sukurti naują modelį",
    "modelName": "Modelio pavadinimas",
    "make": "Markė",
    "tableDescription": "Modelių sąrašas",
    "name": "Pavadinimas",
    "createdBy": "Sukūrė",
    "createdAt": "Sukurta",
    "actions": "Veiksmai"
  },
  "variants": {
    "addVariant": "Pridėti variantą",
    "createNewVariant": "Sukurti naują variantą",
    "make": "Markė*",
    "model": "Modelis*",
    "fuelType": "Kuro tipas*",
    "transmission": "Pavarų dėžė*",
    "bodyType": "Kėbulo tipas*",
    "doors": "Durys*",
    "powerKw": "Galia (kW)*",
    "engineSizeMl": "Variklio tūris (ml)*",
    "customVariant": "Pasirinktinis variantas",
    "tableDescription": "Variantų sąrašas",
    "loading": "Kraunami variantai…",
    "failed": "Nepavyko įkelti variantų.",
    "fuel": "Kuras",
    "transmission_col": "Pavarų dėžė",
    "bodyType_col": "Kėbulo tipas",
    "doors_col": "Durys",
    "powerKw_col": "Galia (kW)",
    "engineMl_col": "Variklis (ml)",
    "custom": "Pasirinktinis",
    "actions": "Veiksmai",
    "yes": "Taip",
    "no": "Ne"
  }
}
```

- [ ] **Step 3: Migrate make components**

**CreateMakeDialog.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("admin");`
3. Replace: `"Add make"` → `t("makes.addMake")`, `"Create new make"` → `t("makes.createNewMake")`

**MakeForm.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation(["admin", "common"]);`
3. Replace: `"Make name"` → `t("admin:makes.makeName")`, `"Confirm"` → `t("common:actions.confirm")`

**MakeListTable.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation(["admin", "common"]);`
3. Replace all table headers and dialog strings with `t("admin:makes.*")` keys
4. Replace: `"Cancel"` → `t("common:actions.cancel")`, `"Delete"` → `t("common:actions.delete")`
5. Use `t("admin:makes.deleteConfirm", { name })` for interpolated delete confirmation

- [ ] **Step 4: Migrate model components**

**CreateModelDialog.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("admin");`
3. Replace: `"Add model"` → `t("models.addModel")`, `"Create new model"` → `t("models.createNewModel")`

**ModelForm.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation(["admin", "common"]);`
3. Replace: `"Model name"` → `t("admin:models.modelName")`, `"Make"` → `t("admin:models.make")`, `"Confirm"` → `t("common:actions.confirm")`

**ModelListTable.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("admin");`
3. Replace all table headers with `t("models.*")` keys

- [ ] **Step 5: Migrate variant components**

**CreateVariantDialog.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("admin");`
3. Replace: `"Add variant"` → `t("variants.addVariant")`, `"Create new variant"` → `t("variants.createNewVariant")`

**VariantForm.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation(["admin", "common"]);`
3. Replace all form labels with `t("admin:variants.*")` keys
4. Replace `"Confirm"` → `t("common:actions.confirm")`

**VariantListTable.tsx:**
1. `import { useTranslation } from "react-i18next";`
2. `const { t } = useTranslation("admin");`
3. Replace all table headers and status strings with `t("variants.*")` keys

- [ ] **Step 6: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate admin namespace (makes, models, variants)"
```

---

## Task 9: Validation Namespace

**Files:**
- Modify: `src/lib/i18n/locales/en/validation.json`
- Modify: `src/lib/i18n/locales/lt/validation.json`
- Modify: `src/utils/validation.ts`
- Modify: `src/features/auth/schemas/loginSchema.ts`
- Modify: `src/features/auth/schemas/registerSchema.ts`
- Modify: `src/features/createListing/schemas/createListingSchema.ts`
- Modify: `src/features/variantList/schemas/variantFormSchema.ts`
- Modify: `src/features/makeList/schemas/makeFormSchema.ts`

- [ ] **Step 1: Write validation.json English translations**

Replace `src/lib/i18n/locales/en/validation.json`:

```json
{
  "required": "{{label}} is required",
  "minLength": "{{label}} must be at least {{length}} characters long",
  "maxLength": "{{label}} must be {{length}} or fewer characters long",
  "minSize": "{{label}} must be at least {{size}}{{unit}}",
  "maxSize": "{{label}} must be {{size}}{{unit}} or less",
  "pleaseSelect": "Please select a {{field}}",
  "enter": "Enter {{field}}",
  "cityCannotBeEmpty": "City cannot be empty",
  "vinInvalid": "VIN code must have 17 characters and not include I, O, or Q",
  "invalidImage": "You did not upload a valid image file",
  "mustUploadImage": "You must upload at least one image"
}
```

- [ ] **Step 2: Write validation.json Lithuanian translations**

Replace `src/lib/i18n/locales/lt/validation.json`:

```json
{
  "required": "{{label}} yra privalomas",
  "minLength": "{{label}} turi būti bent {{length}} simbolių",
  "maxLength": "{{label}} turi būti {{length}} arba mažiau simbolių",
  "minSize": "{{label}} turi būti bent {{size}}{{unit}}",
  "maxSize": "{{label}} turi būti {{size}}{{unit}} arba mažiau",
  "pleaseSelect": "Pasirinkite {{field}}",
  "enter": "Įveskite {{field}}",
  "cityCannotBeEmpty": "Miestas negali būti tuščias",
  "vinInvalid": "VIN kodas turi turėti 17 simbolių ir neturėti I, O arba Q raidžių",
  "invalidImage": "Įkeltas failas nėra tinkamas paveikslėlis",
  "mustUploadImage": "Turite įkelti bent vieną nuotrauką"
}
```

- [ ] **Step 3: Refactor validation utility to accept t function**

Modify `src/utils/validation.ts`:

```typescript
import i18n from "@/lib/i18n/i18n";

export const validation = {
  required: ({ label }: { label: string }) =>
    i18n.t("required", { label, ns: "validation" }),

  minLength: ({ label, length }: { label: string; length: number }) =>
    i18n.t("minLength", { label, length, ns: "validation" }),

  maxLength: ({ label, length }: { label: string; length: number }) =>
    i18n.t("maxLength", { label, length, ns: "validation" }),

  minSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) =>
    i18n.t("minSize", {
      label,
      size,
      unit: unit ? ` ${unit}` : "",
      ns: "validation",
    }),

  maxSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) =>
    i18n.t("maxSize", {
      label,
      size,
      unit: unit ? ` ${unit}` : "",
      ns: "validation",
    }),
};
```

> **Note:** Since Zod schemas are defined at module scope (not inside React components), we use `i18n.t()` directly instead of the `useTranslation` hook. The validation messages will be resolved at the time the schema is evaluated, which happens on form submission — by that point i18n is initialized and the current language is active.

- [ ] **Step 4: Migrate hardcoded schema messages to i18n**

**createListingSchema.ts** — replace inline strings:
- `"Please select a make"` → `i18n.t("pleaseSelect", { field: "make", ns: "validation" })`
- `"Please select a model"` → `i18n.t("pleaseSelect", { field: "model", ns: "validation" })`
- `"Please select a fuel type"` → `i18n.t("pleaseSelect", { field: "fuel type", ns: "validation" })`
- `"Please select a transmission"` → `i18n.t("pleaseSelect", { field: "transmission", ns: "validation" })`
- `"Please select a body type"` → `i18n.t("pleaseSelect", { field: "body type", ns: "validation" })`
- `"Please select a drivetrain type"` → `i18n.t("pleaseSelect", { field: "drivetrain type", ns: "validation" })`
- `"Enter door count"` → `i18n.t("enter", { field: "door count", ns: "validation" })`
- `"Enter engine power"` → `i18n.t("enter", { field: "engine power", ns: "validation" })`
- `"Enter engine size"` → `i18n.t("enter", { field: "engine size", ns: "validation" })`
- `"Enter year"` → `i18n.t("enter", { field: "year", ns: "validation" })`
- `"City cannot be empty"` → `i18n.t("cityCannotBeEmpty", { ns: "validation" })`
- `"VIN code must have 17 characters and not include I, O, or Q"` → `i18n.t("vinInvalid", { ns: "validation" })`
- `"You did not upload a valid image file"` → `i18n.t("invalidImage", { ns: "validation" })`
- `"You must upload at least one image"` → `i18n.t("mustUploadImage", { ns: "validation" })`

Add `import i18n from "@/lib/i18n/i18n";` at the top.

**variantFormSchema.ts** — same pattern for `"Please select a ..."` messages. Add `import i18n from "@/lib/i18n/i18n";`.

**makeFormSchema.ts** — replace `"Name is required"` → `i18n.t("required", { label: "Name", ns: "validation" })`. Add `import i18n from "@/lib/i18n/i18n";`.

**loginSchema.ts** and **registerSchema.ts** — these already use the `validation` utility which now uses i18n. No additional changes needed beyond what was done in Step 3.

- [ ] **Step 5: Verify build**

```bash
cd automotive.marketplace.client
npm run build
```

- [ ] **Step 6: Run lint**

```bash
cd automotive.marketplace.client
npm run lint && npm run format:check
```

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(i18n): migrate validation namespace and refactor validation utility"
```

---

## Task 10: Final Verification & Cleanup

- [ ] **Step 1: Full build verification**

```bash
cd automotive.marketplace.client
npm run build
```

Expected: Clean build with no errors.

- [ ] **Step 2: Lint verification**

```bash
cd automotive.marketplace.client
npm run lint && npm run format:check
```

Expected: No lint or format errors.

- [ ] **Step 3: Manual smoke test checklist**

Start the dev server and verify:

```bash
cd automotive.marketplace.client
npm run dev
```

Check these pages in the browser:
1. Language switcher appears in navbar and toggles between 🇱🇹 LT and 🇬🇧 EN
2. All navbar links change language on toggle
3. Login/Register forms show translated labels
4. Listing cards display translated labels (Used/New, Engine, Fuel Type, etc.)
5. Listing detail page shows translated specs
6. Create listing form shows translated labels
7. Chat components show translated status labels
8. Saved listings page shows translated empty state
9. Compare page shows translated table headers
10. Admin tables show translated headers
11. Language persists across page reloads (check localStorage key `language`)
12. Date formatting respects locale (Lithuanian date names vs English)

- [ ] **Step 4: Final commit if any fixes were needed**

```bash
git add -A
git commit -m "fix(i18n): address issues found during manual verification"
```

---

## Notes

### What NOT to translate
- User-generated content (listing titles, descriptions, usernames, messages)
- Currency symbols (€ stays €)
- URLs and paths
- Console logs and error codes
- Technical identifiers (GUIDs, VINs entered by users)

### Validation approach
Zod schemas are module-scoped, so they use `i18n.t()` directly instead of the React `useTranslation` hook. The `validation` utility in `src/utils/validation.ts` is refactored to call `i18n.t()`, which means all existing schemas that already use the `validation` utility get i18n support automatically with no signature changes.

For schemas with inline hardcoded strings (like `createListingSchema.ts` and `variantFormSchema.ts`), those strings are replaced with direct `i18n.t()` calls.

### Adding new languages in the future
1. Create a new folder under `src/lib/i18n/locales/{code}/` with all 8 JSON files
2. Import them in `i18n.ts` and add to the `resources` object
3. Add the locale mapping in `dateLocale.ts`
4. Add the language option to `LanguageSwitcher.tsx`

### Enum translations
Backend already returns `translations: Translation[]` on enum entities (Fuel, BodyType, Drivetrain, Transmission). The `getTranslatedName()` utility selects the correct translation by `languageCode`, falling back to English, then to the first available. This is used in select dropdowns and anywhere enum names are displayed.
