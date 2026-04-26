---
name: fe-i18n-ui-design
description: Use when designing or implementing UI components in React—ensure i18n compatibility with Lithuanian as the default priority language for the automotive marketplace
---

# Frontend i18n UI Design

## Overview

All UI components must support internationalization (i18n) from the start. Lithuanian (`lt`) is the default and fallback language. React i18next handles language switching, translation management, and locale-specific formatting. Design components assuming they'll be multilingual and Lithuanian-first.

## When to Use

You're building or updating a React UI component and need to:
- Display user-facing text (buttons, labels, messages, headings)
- Format numbers, currency, or dates based on locale
- Support language switching without redeploying
- Maintain Lithuanian as the primary language with English fallback

**When NOT to use:**
- Backend/API code (separate i18n strategy)
- Hardcoded technical text that doesn't display to users (error logs, debug output)
- Component prop TypeScript interfaces and type definitions

## Core Pattern

### 1. Use Translation Keys Instead of Hardcoded Strings

**❌ Bad:**
```tsx
<h1>Filter Vehicles</h1>
<button>Search</button>
```

**✅ Good:**
```tsx
import { useTranslation } from "react-i18next";

const MyComponent = () => {
  const { t } = useTranslation("listings");
  
  return (
    <>
      <h1>{t("filter.title")}</h1>
      <button>{t("filter.search")}</button>
    </>
  );
};
```

### 2. Namespace Organization

Organize translation keys by feature. Namespace and key path should match component scope:

**Structure in `locales/{lt,en}/*.json`:**
```
├── auth.json          // Login, Register, Password reset
├── listings.json      // Filter, Search, List display, Vehicle details
├── chat.json          // Conversations, Messages
├── common.json        // Shared: buttons, labels, navigation, toasts
├── validation.json    // Form error messages
├── myListings.json    // User's created listings
├── saved.json         // Bookmarked listings
├── compare.json       // Comparison tool
├── admin.json         // Admin features
└── toasts.json        // Toast/notification messages
```

**Key naming pattern (dot-notation):**
```
namespace.feature.element
// Examples:
t("listings.filter.title")
t("listings.filter.priceRange")
t("auth.login.submit")
t("common.button.close")
```

### 3. Declare i18n Hook in Component

Always use namespace parameter to scope your translations:

```tsx
const { t } = useTranslation("listings"); // scope to listings.json
const { t: tCommon } = useTranslation("common"); // access common.json
const { t: tValidation } = useTranslation("validation");
```

**Exception:** If component uses only default namespace, you can omit it:
```tsx
const { t } = useTranslation(); // defaults to "common"
```

### 4. Lithuanian-First Implementation

Lithuanian is the fallback language. When adding features:

1. **Create Lithuanian translation first** in `locales/lt/{namespace}.json`
2. **Create English translation second** in `locales/en/{namespace}.json` (will fall back to Lithuanian if missing)
3. **Language detection:** Stored in `localStorage` (key: `"language"`), defaults to `"lt"` if not set
4. **Change language:** Use `i18n.changeLanguage("en")` or `i18n.changeLanguage("lt")`

### 5. Locale-Specific Formatting

Use helpers from `/src/lib/i18n/` for locale-aware formatting:

**Numbers & Currency:**
```tsx
import { formatNumber } from "@/lib/i18n/formatNumber";

const price = 15000;
<span>{formatNumber(price)}</span> // "15 000" (lt) or "15,000" (en)
```

**Dates:**
```tsx
import { useDateLocale } from "@/lib/i18n/dateLocale"; // (if available)

const locale = useDateLocale();
const formatted = format(new Date(), "PPP", { locale });
// Lithuanian: "2025 m. balandžio 25 d."
// English: "April 25, 2025"
```

**Translated Select Options & Enums:**
```tsx
// If vehicle types are stored as constants, create a mapper:
const getVehicleTypeLabel = (type: string, t: any) => {
  const labels: Record<string, string> = {
    car: t("listings.vehicleTypes.car"),
    truck: t("listings.vehicleTypes.truck"),
    motorcycle: t("listings.vehicleTypes.motorcycle"),
  };
  return labels[type] ?? type;
};
```

## Real Codebase Examples

### Example 1: Login Component
Location: `src/app/pages/Login.tsx`

```tsx
import { useTranslation } from "react-i18next";

const Login = () => {
  const { t } = useTranslation("auth");
  
  return (
    <div>
      <h2>{t("login.title")}</h2> {/* t("login.title") from auth.json */}
      <input placeholder={t("login.fields.emailPlaceholder")} />
      <button>{t("login.submit")}</button>
    </div>
  );
};
```

**Translation file structure (`locales/lt/auth.json`):**
```json
{
  "login": {
    "title": "Prisijungimas",
    "submit": "Prisijungti",
    "fields": {
      "email": "El. paštas",
      "emailPlaceholder": "jusu@pastas.lt",
      "password": "Slaptažodis"
    }
  }
}
```

### Example 2: Chat Messages Component
Location: `src/features/chat/components/MessageThread.tsx`

```tsx
import { useTranslation } from "react-i18next";

const MessageThread = ({ messages }: Props) => {
  const { t } = useTranslation("chat");
  
  return (
    <div>
      {messages.map((msg) => (
        <div key={msg.id}>
          <p>{msg.content}</p>
          <small>{t("common.sentAt")}: {formatDate(msg.timestamp)}</small>
        </div>
      ))}
    </div>
  );
};
```

### Example 3: Vehicle Filter (Best Practice)
Location: `src/features/listings/components/VehicleFilter.tsx`

```tsx
import { useTranslation } from "react-i18next";
import { formatNumber } from "@/lib/i18n/formatNumber";

const VehicleFilter = ({ onSearch }: Props) => {
  const { t } = useTranslation("listings");
  const [priceMin, setPriceMin] = useState(0);
  const [priceMax, setPriceMax] = useState(100000);

  return (
    <form>
      <h2>{t("filter.title")}</h2>
      
      <label>{t("filter.priceRange")}</label>
      <span>{formatNumber(priceMin)}</span>
      <span>{formatNumber(priceMax)}</span>
      
      <button onClick={onSearch}>{t("filter.search")}</button>
    </form>
  );
};
```

### Example 4: API Data Translation (getTranslatedName)
Location: `src/lib/i18n/getTranslatedName.ts`

When backend returns multilingual data (e.g., vehicle make with translations):

```tsx
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { useTranslation } from "react-i18next";

const VehicleCard = ({ vehicle }: Props) => {
  const { i18n } = useTranslation();
  const makeName = getTranslatedName(vehicle.makes, i18n.language);
  
  return <div>{makeName}</div>;
};
```

The helper automatically:
1. Looks for translation matching current language (`i18n.language`)
2. Falls back to English if current language not found
3. Falls back to first available if neither language exists

## Implementation Checklist

Before marking a UI component complete:

- [ ] All user-visible text is in translation keys (`t(...)`)
- [ ] Correct namespace used: `useTranslation("namespace")`
- [ ] Lithuanian translation added first to `locales/lt/{namespace}.json`
- [ ] English translation added to `locales/en/{namespace}.json`
- [ ] Placeholders, tooltips, and help text are translated
- [ ] Dates/numbers formatted with locale helpers if needed
- [ ] No hardcoded language-specific text (e.g., "Filter", "Фильтр", "Filtro")
- [ ] Keys follow dot-notation pattern: `feature.element.property`
- [ ] If component has conditional text, all branches are translated
- [ ] Form error messages use `validation` namespace
- [ ] Toast/notification messages use `toasts` namespace

## Common Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| Hardcoding text: `<h1>Login</h1>` | Text doesn't change when language switches | Use `t("auth.login.title")` |
| Missing Lithuanian JSON | Fallback fails for missing English keys | Create `lt` translation before `en` |
| Wrong namespace: `useTranslation("common")` in listings feature | Can't find keys in correct namespace | Use `useTranslation("listings")` |
| Formatting prices/dates as strings | Locale-specific formatting ignored | Use `formatNumber()`, `format()` with locale |
| Nested translation keys too deep | Hard to find and maintain translations | Keep nesting to 3 levels max: `feature.element.property` |
| Conditional text without i18n | Only some text translates | Translate every branch: `value ? t("key1") : t("key2")` |
| Using language code directly (e.g., `if (i18n.language === "lt")`) | Code duplication, hard to maintain | Use translation keys instead, let i18n.changeLanguage() handle it |

## Red Flags - STOP Before Committing

- ❌ Hardcoded text that users will see
- ❌ Translation keys added to only English (not Lithuanian)
- ❌ Missing translation namespace declaration
- ❌ Formatting numbers/dates without locale helpers
- ❌ Conditional UI branches with untranslated strings
- ❌ Using `i18n.language` check for component logic instead of i18n keys

**All of these mean: Review your translations and key structure before shipping.**

## Language Switching

Users switch language via the UI (typically a language selector in navbar/settings). The app stores preference in localStorage and persists across sessions:

```tsx
// Change language to Lithuanian
i18n.changeLanguage("lt");

// Change language to English
i18n.changeLanguage("en");

// Get current language
console.log(i18n.language); // "lt" or "en"
```

Components automatically re-render when language changes because `useTranslation()` is a hook that triggers updates.

## Integration with RTK & TanStack Query

When fetching data that triggers UI updates:

```tsx
const { t } = useTranslation("listings");
const { data } = useQuery({
  queryKey: ["vehicles"],
  queryFn: fetchVehicles,
  enabled: !!userId,
});

// Translate API response labels
const translatedType = t(`listings.vehicleTypes.${data.type}`);
```

Error messages from API responses can be displayed with translations:

```tsx
const { t: tToasts } = useTranslation("toasts");
onError: (error) => {
  showToast({
    message: tToasts(`errors.${error.code}`),
    type: "error",
  });
};
```

## Testing Considerations

When writing tests for translated components:

```tsx
import { useTranslation } from "react-i18next";
import { render, screen } from "@testing-library/react";

// Mock useTranslation if testing logic separately
jest.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "lt", changeLanguage: jest.fn() },
  }),
}));

// Or test with real i18n (recommended for integration tests)
describe("VehicleFilter", () => {
  it("displays translated title", () => {
    render(<VehicleFilter />);
    // i18n will provide real translations
    expect(screen.getByText(/Filtruoti/)).toBeInTheDocument();
  });
});
```

## Summary

**The golden rule:** No hardcoded user-facing text. Every string goes through `t()` with a namespace-scoped key. Lithuanian is your fallback—implement Lithuanian first, then English. Use locale helpers for formatting. Design components assuming they'll support multiple languages from day one.
