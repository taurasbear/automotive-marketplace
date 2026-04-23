# i18n & Multi-Language Support

Full internationalization of the automotive marketplace frontend. English and Lithuanian to start, designed for easy addition of more languages.

## Decisions

- **Framework:** react-i18next (i18next + react-i18next + i18next-browser-languagedetector)
- **Storage:** Browser localStorage (key: `language`)
- **Default language:** Lithuanian (`lt`)
- **Fallback language:** English (`en`)
- **Scope:** Full app — every user-facing string
- **Date localization:** Yes, via date-fns locale integration
- **Language selector:** Navbar dropdown with flag + language code

## Architecture

### i18n Configuration

New file `src/lib/i18n/i18n.ts`:

- Initialize i18next with `react-i18next` and `i18next-browser-languagedetector`
- Configure detector to check `localStorage` first (key: `language`), then fall back to `lt`
- Load all namespaces eagerly (total bundle is small — ~378 strings × 2 languages)
- Import this file in `main.tsx` before React renders

### Translation File Structure

```
src/lib/i18n/
├── i18n.ts
├── dateLocale.ts
└── locales/
    ├── en/
    │   ├── common.json
    │   ├── auth.json
    │   ├── chat.json
    │   ├── listings.json
    │   ├── saved.json
    │   ├── compare.json
    │   ├── admin.json
    │   └── validation.json
    └── lt/
        ├── common.json
        ├── auth.json
        ├── chat.json
        ├── listings.json
        ├── saved.json
        ├── compare.json
        ├── admin.json
        └── validation.json
```

### Namespace Scope

| Namespace | Scope | Approx. Strings |
|-----------|-------|-----------------|
| `common` | Navbar, shared UI, buttons, empty states | ~15 |
| `auth` | Login, register forms | ~20 |
| `chat` | All chat components: messages, meetings, offers, availability | ~65 |
| `listings` | Create/edit listing, listing detail, listing list, search/filters | ~120 |
| `saved` | Saved listings page | ~25 |
| `compare` | Listing comparison feature | ~40 |
| `admin` | Makes, models, variants management | ~80 |
| `validation` | Shared validation message templates | ~15 templates |

### Key Naming Convention

Pattern: `{component}.{element}` within each namespace.

Examples:
```json
// chat.json
{
  "meetingCard": {
    "statusLabels": {
      "proposed": "Meetup Proposed",
      "confirmed": "Meetup Confirmed",
      "declined": "Meetup Declined",
      "rescheduled": "Reschedule Proposed",
      "expired": "Meetup Expired",
      "cancelled": "Meetup Cancelled"
    },
    "subtitles": {
      "awaitingResponse": "Awaiting response",
      "seeYouThere": "See you there!"
    },
    "actions": {
      "accept": "Accept",
      "decline": "Decline",
      "suggestAlternative": "Suggest alternative",
      "proposeCounterTime": "Propose a counter time",
      "shareMyAvailability": "Share my availability",
      "cancelMeetup": "Cancel meetup"
    }
  }
}

// validation.json
{
  "required": "{{label}} is required",
  "minLength": "{{label}} must be at least {{length}} characters long",
  "maxLength": "{{label}} must be {{length}} or fewer characters long",
  "minValue": "{{label}} must be at least {{value}}{{unit}}",
  "maxValue": "{{label}} must be {{value}}{{unit}} or less"
}
```

### Interpolation

i18next handles interpolation with `{{variable}}` syntax:

```tsx
t("validation.required", { label: t("auth.fields.email") })
// → "Email is required" / "El. paštas yra privalomas"

t("chat.actionBar.confirmedMeetupWarning", {
  date: format(meetingDate, "EEE, MMM d", { locale }),
  time: format(meetingDate, "HH:mm", { locale }),
})
// → "You have a confirmed meetup on Mon, Jul 21 at 14:00..."
```

## Language Switcher Component

**Location:** Navbar, next to the Inbox and Saved icons.

**Appearance:** A compact dropdown button showing the current language flag and code:
- 🇱🇹 LT (Lithuanian, default)
- 🇬🇧 EN (English)

**Behavior:**
- Clicking opens a dropdown with available languages
- Selecting a language calls `i18n.changeLanguage(code)`
- i18next-browser-languagedetector auto-persists to localStorage
- The entire UI re-renders with the new language (React context triggers re-render)
- No page reload needed

**Component:** `src/components/LanguageSwitcher.tsx` — uses shadcn DropdownMenu.

## Date Localization

**Helper:** `src/lib/i18n/dateLocale.ts`

```tsx
import { useTranslation } from "react-i18next";
import { lt, enUS } from "date-fns/locale";

const localeMap: Record<string, Locale> = {
  lt: lt,
  en: enUS,
};

export const useDateLocale = (): Locale => {
  const { i18n } = useTranslation();
  return localeMap[i18n.language] ?? lt;
};
```

**Usage in components:** Every `format()`, `formatDistanceToNow()`, and `formatRelative()` call from date-fns receives the locale:

```tsx
const locale = useDateLocale();
format(date, "EEE, MMM d", { locale });
formatDistanceToNow(date, { addSuffix: true, locale });
```

**Affected components:**
- `MeetingCard.tsx` — proposed date display
- `AvailabilityCardComponent.tsx` — slot date/time display
- `MessageThread.tsx` — sticky bar date, message timestamps
- `ConversationList.tsx` — "3 hours ago" relative times
- `ProposeMeetingModal.tsx` — timezone label
- `ShareAvailabilityModal.tsx` — timezone label

## Backend Enum Translation Integration

The backend already returns translation arrays on enum entities (Fuel, BodyType, Drivetrain, Transmission). Each translation has `languageCode` and `name`.

**Frontend approach:**
- Create a utility `getTranslatedName(translations: Translation[], language: string): string` that finds the matching translation by language code, falling back to English, then to the first available.
- Use this utility in all dropdowns and display components that show enum values (listing forms, listing detail, comparison table, admin panels, filters).

**No backend changes needed** — the existing API already returns all translations, and the frontend selects the right one based on current language.

## Migration Strategy

### Component Migration Pattern

For each component:

1. Add `const { t } = useTranslation("{namespace}");`
2. Replace each hardcoded string with `t("key")`
3. For date formatting, add `const locale = useDateLocale();` and pass `{ locale }` to date-fns calls
4. For validation schemas (Zod), pass `t` function and use it for error messages

### What NOT to Translate

- User-generated content (listing titles, descriptions, usernames, messages)
- Currency symbols (€ stays €)
- URLs and paths
- Console logs and error codes
- Technical identifiers

### Migration Order

1. **Infrastructure** — install packages, create i18n config, create empty translation files, create LanguageSwitcher, create dateLocale helper, create getTranslatedName utility
2. **Common namespace** — navbar, shared UI components
3. **Auth namespace** — login/register forms
4. **Chat namespace** — all chat components (largest feature area)
5. **Listings namespace** — create/edit, detail, list, search/filters
6. **Saved namespace** — saved listings page
7. **Compare namespace** — comparison feature
8. **Admin namespace** — makes/models/variants management
9. **Validation namespace** — shared validation patterns across all forms

## Files Affected

### New Files
- `src/lib/i18n/i18n.ts` — i18next configuration
- `src/lib/i18n/dateLocale.ts` — date-fns locale hook
- `src/lib/i18n/getTranslatedName.ts` — backend enum translation utility
- `src/lib/i18n/locales/en/*.json` — 8 English translation files
- `src/lib/i18n/locales/lt/*.json` — 8 Lithuanian translation files
- `src/components/LanguageSwitcher.tsx` — language switcher dropdown

### Modified Files
- `src/main.tsx` — import i18n config
- `src/components/Navbar.tsx` (or equivalent) — add LanguageSwitcher
- Every component with hardcoded strings (~40+ component files across all features)
- Every file using date-fns `format()`/`formatDistanceToNow()` — add locale param

### Dependencies
- `i18next` — core i18n framework
- `react-i18next` — React bindings
- `i18next-browser-languagedetector` — auto-detect/persist language from localStorage
