# Frontend Component Test Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Achieve 80% file-level test coverage for frontend feature components and pages (~62 of 77 testable .tsx files).

**Architecture:** Feature-by-feature, co-located test files using Vitest + React Testing Library. Shared test infrastructure (custom render, mock factories) established first, then tests written in business-criticality order: Auth → Chat → Listings → rest. Each test covers render + key interactions.

**Tech Stack:** Vitest 4.1.5, @testing-library/react 16.3.2, @testing-library/user-event 14.6.1, @testing-library/jest-dom 6.9.1

**Spec:** `docs/superpowers/specs/2026-05-03-fe-component-test-coverage-design.md`

---

## File Structure

All test files are co-located next to their source files (matching existing pattern):

```
src/
  test/
    setup.ts                          (modify — add global mocks)
    test-utils.tsx                    (create — custom render with providers)
    mocks/
      handlers.ts                     (create — reusable mock setup helpers)
  features/
    auth/components/
      LogoutButton.test.tsx           (create)
      RegisterButton.test.tsx         (create)
    chat/components/
      MessageThread.test.tsx          (create)
      ConversationList.test.tsx       (create)
      ContractFormDialog.test.tsx     (create)
      ContractCard.test.tsx           (create)
      MeetingCard.test.tsx            (create)
      OfferCard.test.tsx              (create)
      ActionBar.test.tsx              (create)
      ChatPanel.test.tsx              (create)
      MakeOfferModal.test.tsx         (create)
      ProposeMeetingModal.test.tsx    (create)
      ShareAvailabilityModal.test.tsx (create)
      AvailabilityCardComponent.test.tsx (create)
      ListingCard.test.tsx            (create)
      UnreadBadge.test.tsx            (create)
    listingDetails/components/
      ListingDetailsContent.test.tsx  (create)
      EditListingDialog.test.tsx      (create)
      EditListingForm.test.tsx        (create)
      AiSummarySection.test.tsx       (create)
      ScoreCard.test.tsx              (create)
      ListingKeySpecs.test.tsx        (create)
      ListingSecondaryDetails.test.tsx (create)
    listingList/components/
      Filters.test.tsx                (create)
      BasicFilters.test.tsx           (create)
      RangeFilters.test.tsx           (create)
      ModelFilter.test.tsx            (create)
      ListingCard.test.tsx            (create)
      ListingCardBadge.test.tsx       (create)
      ListingList.test.tsx            (create)
    myListings/components/
      MyListingDetail.test.tsx        (create)
      MyListingCard.test.tsx          (create)
      MyListingsPage.test.tsx         (create)
      SellerInsightsPanel.test.tsx    (create)
      ListingBuyerPanel.test.tsx      (create)
      EditableField.test.tsx          (create)
    createListing/components/
      CreateListingForm.test.tsx      (create)
      ImageUploadInput.test.tsx       (create)
      ImagePreview.test.tsx           (create)
    savedListings/components/
      NoteEditor.test.tsx             (create)
      SavedListingsPage.test.tsx      (create)
      SavedListingRow.test.tsx        (create)
      PropertyMentionPicker.test.tsx  (create)
    search/components/
      ListingSearch.test.tsx          (create)
      ListingSearchFilters.test.tsx   (create)
    dashboard/components/
      Dashboard.test.tsx              (create)
      DashboardTile.test.tsx          (create)
    makeList/components/
      MakeForm.test.tsx               (create)
      MakeListTable.test.tsx          (create)
      CreateMakeDialog.test.tsx       (create)
    modelList/components/
      ModelForm.test.tsx              (create)
      ModelListTable.test.tsx         (create)
      EditModelDialogContent.test.tsx (create)
      ViewModelDialogContent.test.tsx (create)
      CreateModelDialog.test.tsx      (create)
    variantList/components/
      VariantForm.test.tsx            (create)
      VariantListTable.test.tsx       (create)
      EditVariantDialogContent.test.tsx (create)
      ViewVariantDialogContent.test.tsx (create)
      CreateVariantDialog.test.tsx    (create)
    compareListings/components/
      CompareAiSummary.test.tsx       (create)
      CompareScoreBanner.test.tsx     (create)
      CompareRow.test.tsx             (create)
      DiffToggleFab.test.tsx          (create)
    userPreferences/components/
      UserPreferencesDialog.test.tsx  (create)
  components/
    Header.test.tsx                   (create)
    Footer.test.tsx                   (create)
    UserMenu.test.tsx                 (create)
    ThemeToggle.test.tsx              (create)
    LanguageSwitcher.test.tsx         (create)
    MakeSelect.test.tsx               (create)
    ModelSelect.test.tsx              (create)
    DefectSelector.test.tsx           (create)
    VariantTable.test.tsx             (create)
    ImageArrowGallery.test.tsx        (create)
    ImageHoverGallery.test.tsx        (create)
  app/pages/
    Login.test.tsx                    (create)
    Register.test.tsx                 (create)
    MainPage.test.tsx                 (create)
    Listings.test.tsx                 (create)
    ListingDetails.test.tsx           (create)
    CreateListing.test.tsx            (create)
    Inbox.test.tsx                    (create)
    SavedListings.test.tsx            (create)
    Settings.test.tsx                 (create)
    Makes.test.tsx                    (create)
    Models.test.tsx                   (create)
    Variants.test.tsx                 (create)
```

---

## Established Patterns (from existing tests)

These patterns MUST be followed in every test file. Read the existing tests for reference:
- `src/app/pages/Compare.test.tsx` — page-level test with QueryClient wrapper, vi.mock, vi.hoisted
- `src/features/compareListings/components/CompareHeader.test.tsx` — component test with i18n mock, router mock, callback verification
- `src/features/compareListings/components/CompareTable.test.tsx` — pure component test with no mocks

### Pattern: QueryClient wrapper

```tsx
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};
```

### Pattern: vi.hoisted for mock references

```tsx
const { mockFn } = vi.hoisted(() => ({
  mockFn: vi.fn(),
}));

vi.mock("@/path/to/module", () => ({
  exportedName: mockFn,
}));
```

### Pattern: i18n mock

```tsx
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
  Trans: ({ i18nKey }: { i18nKey: string }) => <span>{i18nKey}</span>,
}));
```

### Pattern: Router mock

```tsx
vi.mock("@tanstack/react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@tanstack/react-router")>();
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
      <a href={String(to ?? "")}>{children}</a>
    ),
  };
});
```

### Pattern: Redux mock

```tsx
vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => mockDispatch,
}));
```

### Pattern: Mutation hook mock

```tsx
const { mockMutateAsync } = vi.hoisted(() => ({
  mockMutateAsync: vi.fn(),
}));

vi.mock("@/features/auth/api/useLoginUser", () => ({
  useLoginUser: () => ({ mutateAsync: mockMutateAsync }),
}));
```

---

### Task 0: Test Infrastructure

**Files:**
- Modify: `src/test/setup.ts`
- Create: `src/test/test-utils.tsx`
- Create: `src/test/mocks/handlers.ts`

- [ ] **Step 1: Enhance setup.ts with global mocks**

```typescript
// src/test/setup.ts
import "@testing-library/jest-dom";

// Mock window.matchMedia for components that use media queries or theme detection
Object.defineProperty(window, "matchMedia", {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock IntersectionObserver for lazy-loaded components
class MockIntersectionObserver {
  observe = vi.fn();
  unobserve = vi.fn();
  disconnect = vi.fn();
  readonly root = null;
  readonly rootMargin = "";
  readonly thresholds: readonly number[] = [];
  takeRecords = vi.fn().mockReturnValue([]);
}

Object.defineProperty(window, "IntersectionObserver", {
  writable: true,
  value: MockIntersectionObserver,
});

// Mock ResizeObserver
class MockResizeObserver {
  observe = vi.fn();
  unobserve = vi.fn();
  disconnect = vi.fn();
}

Object.defineProperty(window, "ResizeObserver", {
  writable: true,
  value: MockResizeObserver,
});
```

- [ ] **Step 2: Create test-utils.tsx with custom render**

```tsx
// src/test/test-utils.tsx
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import type { ReactElement, ReactNode } from "react";

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  });
}

export function createWrapper() {
  const queryClient = createTestQueryClient();
  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, "wrapper">,
) {
  const queryClient = createTestQueryClient();
  const Wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
  return { ...render(ui, { wrapper: Wrapper, ...options }), queryClient };
}

// Re-export everything from testing-library
export * from "@testing-library/react";
export { default as userEvent } from "@testing-library/user-event";
export { renderWithProviders };
```

- [ ] **Step 3: Create mock handlers**

```typescript
// src/test/mocks/handlers.ts

/** Standard i18n mock — returns translation keys as-is */
export const i18nMock = {
  useTranslation: (ns?: string) => ({
    t: (key: string) => key,
    i18n: { language: "lt", changeLanguage: vi.fn() },
  }),
  Trans: ({ i18nKey, children }: { i18nKey?: string; children?: React.ReactNode }) => (
    <span>{i18nKey ?? children}</span>
  ),
  initReactI18next: { type: "3rdParty", init: vi.fn() },
};

/** Creates a mock navigate function and router mock */
export function createRouterMock() {
  const mockNavigate = vi.fn();
  return {
    mockNavigate,
    routerMock: {
      useNavigate: () => mockNavigate,
      useParams: () => ({}),
      useSearch: () => ({}),
      Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
        <a href={String(to ?? "")}>{children}</a>
      ),
    },
  };
}

/** Creates mock Redux hooks with configurable auth state */
export function createReduxMock(overrides?: {
  userId?: string | null;
  accessToken?: string | null;
  permissions?: string[];
}) {
  const userId = overrides?.userId ?? null;
  const accessToken = overrides?.accessToken ?? null;
  const permissions = overrides?.permissions ?? [];
  const mockDispatch = vi.fn();

  const mockUseAppSelector = vi.fn().mockImplementation((selector: Function) => {
    const state = {
      auth: { userId, accessToken, permissions },
    };
    return selector(state);
  });

  return { mockDispatch, mockUseAppSelector };
}
```

- [ ] **Step 4: Run tests to verify infrastructure**

Run: `cd automotive.marketplace.client && npm run test`

Expected: All 5 existing tests still pass.

- [ ] **Step 5: Commit**

```bash
git add src/test/
git commit -m "test: add shared test infrastructure (setup, utils, mocks)"
```

---

### Task 1: Auth Tests (4 files)

**Files:**
- Create: `src/app/pages/Login.test.tsx`
- Create: `src/app/pages/Register.test.tsx`
- Create: `src/features/auth/components/LogoutButton.test.tsx`
- Create: `src/features/auth/components/RegisterButton.test.tsx`
- Test: `npm run test -- --reporter=verbose`

**Context files to read first:**
- `src/app/pages/Login.tsx` — form with email, password (toggle visibility), submit → loginUserAsync → setCredentials → navigate("/")
- `src/app/pages/Register.tsx` — form with username, email, password (toggle visibility), submit → registerUserAsync → setCredentials → navigate("/")
- `src/features/auth/components/LogoutButton.tsx` — button click → logoutUserAsync → clearCredentials → navigate("/login")
- `src/features/auth/components/RegisterButton.tsx` — button click → navigate("/register")
- `src/features/auth/state/authSlice.ts` — AuthState shape: { userId, permissions, accessToken }
- `src/features/auth/api/useLoginUser.ts` — useMutation returning { mutateAsync }
- `src/features/auth/api/useRegisterUser.ts` — useMutation returning { mutateAsync }
- `src/features/auth/schemas/loginSchema.ts` — Zod schema for login form
- `src/features/auth/schemas/registerSchema.ts` — Zod schema for register form

- [ ] **Step 1: Write Login.test.tsx**

```tsx
// src/app/pages/Login.test.tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Login from "./Login";

const { mockLoginAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockLoginAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("@/features/auth", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/features/auth")>();
  return {
    ...actual,
    useLoginUser: () => ({ mutateAsync: mockLoginAsync }),
  };
});

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

beforeEach(() => {
  mockLoginAsync.mockReset();
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("Login page", () => {
  it("renders email and password fields and a submit button", () => {
    render(<Login />);
    expect(screen.getByPlaceholderText("login.fields.emailPlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("login.fields.passwordPlaceholder")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "login.submit" })).toBeInTheDocument();
  });

  it("toggles password visibility when the eye icon is clicked", async () => {
    const user = userEvent.setup();
    render(<Login />);

    const passwordInput = screen.getByPlaceholderText("login.fields.passwordPlaceholder");
    expect(passwordInput).toHaveAttribute("type", "password");

    await user.click(screen.getByLabelText("login.fields.showPassword"));
    expect(passwordInput).toHaveAttribute("type", "text");

    await user.click(screen.getByLabelText("login.fields.hidePassword"));
    expect(passwordInput).toHaveAttribute("type", "password");
  });

  it("calls loginUserAsync, dispatches setCredentials, and navigates on successful submit", async () => {
    mockLoginAsync.mockResolvedValue({
      data: { accessToken: "tok-123", permissions: ["admin"], userId: "user-1" },
    });

    const user = userEvent.setup();
    render(<Login />);

    await user.type(screen.getByPlaceholderText("login.fields.emailPlaceholder"), "test@example.com");
    await user.type(screen.getByPlaceholderText("login.fields.passwordPlaceholder"), "Password123!");
    await user.click(screen.getByRole("button", { name: "login.submit" }));

    await waitFor(() => {
      expect(mockLoginAsync).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "Password123!",
      });
    });

    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({
        payload: { accessToken: "tok-123", permissions: ["admin"], userId: "user-1" },
      }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/" });
  });
});
```

- [ ] **Step 2: Write Register.test.tsx**

```tsx
// src/app/pages/Register.test.tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Register from "./Register";

const { mockRegisterAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockRegisterAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("@/features/auth", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/features/auth")>();
  return {
    ...actual,
    useRegisterUser: () => ({ mutateAsync: mockRegisterAsync }),
  };
});

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

beforeEach(() => {
  mockRegisterAsync.mockReset();
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("Register page", () => {
  it("renders username, email, and password fields and a submit button", () => {
    render(<Register />);
    expect(screen.getByPlaceholderText("register.fields.usernamePlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("register.fields.emailPlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("register.fields.passwordPlaceholder")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "register.submit" })).toBeInTheDocument();
  });

  it("toggles password visibility when the eye icon is clicked", async () => {
    const user = userEvent.setup();
    render(<Register />);

    const passwordInput = screen.getByPlaceholderText("register.fields.passwordPlaceholder");
    expect(passwordInput).toHaveAttribute("type", "password");

    await user.click(screen.getByLabelText("register.fields.showPassword"));
    expect(passwordInput).toHaveAttribute("type", "text");

    await user.click(screen.getByLabelText("register.fields.hidePassword"));
    expect(passwordInput).toHaveAttribute("type", "password");
  });

  it("renders a link to the login page", () => {
    render(<Register />);
    const loginLink = screen.getByRole("link");
    expect(loginLink).toHaveAttribute("href", "/login");
  });

  it("calls registerUserAsync, dispatches setCredentials, and navigates on successful submit", async () => {
    mockRegisterAsync.mockResolvedValue({
      data: { accessToken: "tok-456", userId: "user-2" },
    });

    const user = userEvent.setup();
    render(<Register />);

    await user.type(screen.getByPlaceholderText("register.fields.usernamePlaceholder"), "testuser");
    await user.type(screen.getByPlaceholderText("register.fields.emailPlaceholder"), "test@example.com");
    await user.type(screen.getByPlaceholderText("register.fields.passwordPlaceholder"), "Password123!");
    await user.click(screen.getByRole("button", { name: "register.submit" }));

    await waitFor(() => {
      expect(mockRegisterAsync).toHaveBeenCalledWith({
        username: "testuser",
        email: "test@example.com",
        password: "Password123!",
      });
    });

    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({
        payload: { accessToken: "tok-456", permissions: [], userId: "user-2" },
      }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/" });
  });
});
```

- [ ] **Step 3: Write LogoutButton.test.tsx**

```tsx
// src/features/auth/components/LogoutButton.test.tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import LogoutButton from "./LogoutButton";

const { mockLogoutAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockLogoutAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("../api/useLogoutUser", () => ({
  useLogoutUser: () => ({ mutateAsync: mockLogoutAsync }),
}));

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
}));

beforeEach(() => {
  mockLogoutAsync.mockReset().mockResolvedValue({});
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("LogoutButton", () => {
  it("renders a button", () => {
    render(<LogoutButton />);
    expect(screen.getByRole("button")).toBeInTheDocument();
  });

  it("calls logoutUserAsync, dispatches clearCredentials, and navigates to login on click", async () => {
    const user = userEvent.setup();
    render(<LogoutButton />);

    await user.click(screen.getByRole("button"));

    await waitFor(() => {
      expect(mockLogoutAsync).toHaveBeenCalledTimes(1);
    });
    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({ type: "auth/clearCredentials" }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/login" });
  });
});
```

- [ ] **Step 4: Write RegisterButton.test.tsx**

```tsx
// src/features/auth/components/RegisterButton.test.tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import RegisterButton from "./RegisterButton";

const { mockNavigate } = vi.hoisted(() => ({
  mockNavigate: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

describe("RegisterButton", () => {
  it("renders a button with the sign up text", () => {
    render(<RegisterButton />);
    expect(screen.getByRole("button", { name: "header.signUp" })).toBeInTheDocument();
  });

  it("navigates to /register on click", async () => {
    const user = userEvent.setup();
    render(<RegisterButton />);

    await user.click(screen.getByRole("button", { name: "header.signUp" }));

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith({ to: "/register" });
    });
  });
});
```

- [ ] **Step 5: Run tests**

Run: `cd automotive.marketplace.client && npm run test -- --reporter=verbose`

Expected: All auth tests pass alongside existing compare tests.

- [ ] **Step 6: Commit**

```bash
git add src/app/pages/Login.test.tsx src/app/pages/Register.test.tsx \
       src/features/auth/components/LogoutButton.test.tsx \
       src/features/auth/components/RegisterButton.test.tsx
git commit -m "test(auth): add Login, Register, LogoutButton, RegisterButton tests"
```

---

### Task 2: Chat Tests (14 files)

**Files:** Create test files for all 14 components in `src/features/chat/components/`

**Context files to read first:**
- Every `.tsx` file in `src/features/chat/components/`
- `src/features/chat/api/` — all API hook files to understand mutation/query signatures
- `src/features/chat/types/` — all type definitions
- `src/features/chat/hooks/` — useChatHub hook

**Mocking approach for ALL chat tests:**

```tsx
// Standard chat test mocks — use in every chat test file
vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  Trans: ({ i18nKey }: { i18nKey: string }) => <span>{i18nKey}</span>,
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
  useAppDispatch: () => mockDispatch,
}));
```

**For each component, the subagent must:**
1. Read the source component file completely
2. Identify all imported hooks, types, and child components
3. Mock every external dependency (API hooks, router, Redux, i18n)
4. Write tests for: renders without crashing, key interactions, conditional rendering

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **MessageThread** | Renders message list; renders different message types (text, offer, meeting, availability, contract); send message input and submit; auto-scrolls to bottom; shows empty state. Mock: useChatHub, useQuery for messages, mutation hooks. |
| **ConversationList** | Renders list of conversations; clicking a conversation calls onSelect; shows unread indicator; shows empty state. Mock: useQuery for conversations. |
| **ContractFormDialog** | Renders dialog when open; shows seller form fields vs buyer form fields based on role prop; validates required fields; calls mutation on submit. Mock: contract mutation hook. |
| **ContractCard** | Renders contract details; shows accept/reject buttons for pending contracts; calls accept/reject handlers. Mock: accept/reject mutation hooks. |
| **MeetingCard** | Renders meeting details with date/time; shows accept/decline for pending meetings; calls handlers. Mock: accept/decline mutations. |
| **OfferCard** | Renders offer amount; shows accept/counter/decline for pending offers; calls handlers. Mock: offer action mutations. |
| **ActionBar** | Renders action buttons (offer, meeting, availability, contract); clicking each button calls corresponding onOpen callback. Pure prop test, minimal mocking. |
| **ChatPanel** | Renders layout with listing card, action bar, and thread; conditional sections based on props. |
| **MakeOfferModal** | Renders dialog when open; shows price input; validates input; calls mutation on submit. Mock: offer mutation. |
| **ProposeMeetingModal** | Renders dialog when open; shows date/time inputs; calls mutation on submit. Mock: meeting mutation. |
| **ShareAvailabilityModal** | Renders dialog when open; shows calendar; calls mutation on submit. Mock: availability mutation. |
| **AvailabilityCardComponent** | Renders availability data display; shows time slots. |
| **ListingCard** (chat) | Renders listing thumbnail, title, price. Pure props test. |
| **UnreadBadge** | Renders badge with count when count > 0; renders nothing when count is 0. |

- [ ] **Step 1: Read all chat component source files and their dependencies**
- [ ] **Step 2: Write test files for all 14 components following the patterns above**
- [ ] **Step 3: Run tests**

Run: `cd automotive.marketplace.client && npm run test -- --reporter=verbose`

Expected: All chat tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/features/chat/components/*.test.tsx
git commit -m "test(chat): add tests for all 14 chat components"
```

---

### Task 3: Listing Details Tests (8 files)

**Files:** Create test files for all components in `src/features/listingDetails/components/` + `src/app/pages/ListingDetails.test.tsx`

**Context files to read first:**
- Every `.tsx` file in `src/features/listingDetails/components/`
- `src/features/listingDetails/api/` — all API hooks
- `src/features/listingDetails/types/` — GetListingByIdResponse and related types
- `src/app/pages/ListingDetails.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **ListingDetailsContent** | Renders listing data; shows edit/delete buttons for seller; hides edit/delete for guest; shows chat button for non-seller authenticated user; renders image gallery; renders key specs and secondary details; like toggle calls mutation. Mock: useQuery (listing), Redux (userId), mutations (delete, like). |
| **EditListingDialog** | Renders dialog when open; pre-fills form with listing data; closes on cancel. Mock: listing data prop. |
| **EditListingForm** | Renders all form fields; cascading make→model→variant selects; calls update mutation on submit; validates required fields. Mock: enum queries, update mutation. |
| **AiSummarySection** | Renders AI summary text when available; shows loading skeleton; shows disabled state when feature is off. Mock: useQuery (AI summary), user preferences. |
| **ScoreCard** | Renders score value and breakdown; shows conditional sections based on score data. |
| **ListingKeySpecs** | Renders specs (power, engine, fuel, transmission, drivetrain) from props. Pure render test. |
| **ListingSecondaryDetails** | Renders secondary info (year, mileage, doors, body type, steering, municipality) from props. Pure render test. |
| **ListingDetails page** | Extracts listing ID from route params; renders ListingDetailsContent. Mock: Route.useParams. |

- [ ] **Step 1: Read all listing details source files and their dependencies**
- [ ] **Step 2: Write test files for all 8 components**
- [ ] **Step 3: Run tests**

Run: `cd automotive.marketplace.client && npm run test -- --reporter=verbose`

- [ ] **Step 4: Commit**

```bash
git add src/features/listingDetails/components/*.test.tsx src/app/pages/ListingDetails.test.tsx
git commit -m "test(listing-details): add tests for all listing detail components"
```

---

### Task 4: Listing List & Browse Tests (8 files)

**Files:** Create test files for all components in `src/features/listingList/components/` + `src/app/pages/Listings.test.tsx`

**Context files to read first:**
- Every `.tsx` file in `src/features/listingList/components/`
- `src/features/listingList/api/` — API hooks
- `src/features/listingList/types/`
- `src/app/pages/Listings.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **Listings page** | Renders filters and listing list; filter changes reset page to 1; pagination updates URL search params. Mock: Route (useSearch, useNavigate), Suspense boundary. |
| **Filters** | Renders BasicFilters + RangeFilters + ModelFilter; passes filter values to children. |
| **BasicFilters** | Renders select components (fuel, body type, transmission, etc.); changing a select calls onChange. Mock: enum queries. |
| **RangeFilters** | Renders range sliders (price, mileage, power, year); changing range calls onChange. |
| **ModelFilter** | Renders make select; selecting make enables model select; cascading behavior. Mock: makes query, models query. |
| **ListingCard** | Renders listing image, title, price, specs; like button toggle calls mutation; clicking card navigates. Mock: like mutation, router. |
| **ListingCardBadge** | Renders badge with status text and appropriate styling. Pure props test. |
| **ListingList** | Renders list of ListingCard components from data array; shows empty state. |

- [ ] **Step 1: Read all listing list source files**
- [ ] **Step 2: Write test files for all 8 components**
- [ ] **Step 3: Run tests and commit**

```bash
git add src/features/listingList/components/*.test.tsx src/app/pages/Listings.test.tsx
git commit -m "test(listing-list): add tests for browse listings components"
```

---

### Task 5: My Listings Tests (6 files)

**Files:** Create test files for all components in `src/features/myListings/components/`

**Context files to read first:**
- Every `.tsx` file in `src/features/myListings/components/`
- `src/features/myListings/api/` — mutations (delete, update status, reactivate)
- `src/features/myListings/types/`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **MyListingDetail** | Renders listing details; inline edit fields; delete button shows confirmation dialog; confirming delete calls mutation; defect section renders. Mock: listing query, mutations. |
| **MyListingCard** | Renders card with status; activate/deactivate toggle; delete button; status badge. Mock: status mutation, delete mutation. |
| **MyListingsPage** | Renders list of MyListingCard; shows empty state when no listings. Mock: listings query. |
| **SellerInsightsPanel** | Renders stats (views, likes, messages). Pure props test. |
| **ListingBuyerPanel** | Renders buyer information. Pure props test. |
| **EditableField** | Renders display mode; clicking activates edit mode; save commits change; cancel reverts. |

- [ ] **Step 1: Read all my listings source files**
- [ ] **Step 2: Write test files for all 6 components**
- [ ] **Step 3: Run tests and commit**

```bash
git add src/features/myListings/components/*.test.tsx
git commit -m "test(my-listings): add tests for seller listing management"
```

---

### Task 6: Create Listing Tests (4 files)

**Files:** Create test files in `src/features/createListing/components/` + `src/app/pages/CreateListing.test.tsx`

**Context files to read first:**
- Every `.tsx` file in `src/features/createListing/components/`
- `src/features/createListing/api/` — create listing mutation, image upload
- `src/app/pages/CreateListing.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **CreateListingForm** | Renders all form sections; cascading make→model→variant; image upload section renders; defect selector renders; submitting with valid data calls mutation; validation errors show. Mock: enum queries, create mutation, image compression. |
| **CreateListing page** | Shows guest warning when not authenticated; disables form for guests; renders form for authenticated users. Mock: Redux (selectAccessToken). |
| **ImageUploadInput** | Renders file input; selecting files triggers onChange; shows selected file count. |
| **ImagePreview** | Renders image thumbnails; remove button calls onRemove callback. |

- [ ] **Step 1: Read all create listing source files**
- [ ] **Step 2: Write test files for all 4 components**
- [ ] **Step 3: Run tests and commit**

```bash
git add src/features/createListing/components/*.test.tsx src/app/pages/CreateListing.test.tsx
git commit -m "test(create-listing): add tests for listing creation flow"
```

---

### Task 7: Saved Listings Tests (4 files)

**Files:** Create test files in `src/features/savedListings/components/` + `src/app/pages/SavedListings.test.tsx`

**Context files to read first:**
- Every `.tsx` file in `src/features/savedListings/components/`
- `src/features/savedListings/api/`
- `src/features/savedListings/types/`
- `src/app/pages/SavedListings.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **NoteEditor** | Renders textarea with existing note; typing triggers debounced save; auto-saves on unmount; mention picker integration. Mock: upsert note mutation, vi.useFakeTimers for debounce. |
| **SavedListingsPage** | Renders list of saved listings; shows empty state. Mock: saved listings query. |
| **SavedListingRow** | Renders listing info; unlike button calls mutation; note preview shows. Mock: unlike mutation. |
| **PropertyMentionPicker** | Renders dropdown of properties; selecting a property inserts mention. |
| **SavedListings page** | Renders SavedListingsPage wrapper. |

- [ ] **Step 1: Read all saved listings source files**
- [ ] **Step 2: Write test files for all components (4-5 files)**
- [ ] **Step 3: Run tests and commit**

```bash
git add src/features/savedListings/components/*.test.tsx src/app/pages/SavedListings.test.tsx
git commit -m "test(saved-listings): add tests for saved listings feature"
```

---

### Task 8: Search, Dashboard & Simple Pages Tests (7 files)

**Files:**
- Create: `src/features/search/components/ListingSearch.test.tsx`
- Create: `src/features/search/components/ListingSearchFilters.test.tsx`
- Create: `src/features/dashboard/components/Dashboard.test.tsx`
- Create: `src/features/dashboard/components/DashboardTile.test.tsx`
- Create: `src/app/pages/MainPage.test.tsx`
- Create: `src/app/pages/Inbox.test.tsx`
- Create: `src/app/pages/Settings.test.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **ListingSearch** | Renders search input and filter toggles; typing in search calls onChange; filter toggles update. |
| **ListingSearchFilters** | Renders filter selects; changing filters calls onChange. Mock: enum queries. |
| **Dashboard** | Renders dashboard tiles with data. Mock: dashboard query. |
| **DashboardTile** | Renders tile with label, value, and optional icon. Pure props test. |
| **MainPage** | Renders ListingSearch; shows Dashboard for authenticated users; hides Dashboard for guests. Mock: Redux. |
| **Inbox** | Renders split layout; renders ConversationList; renders Suspense boundary for MessageThread. Mock: router params. |
| **Settings** | Renders toggle switches; toggling calls upsert mutation; reset button restores defaults. Mock: preferences query, upsert mutation. |

- [ ] **Step 1: Read all source files**
- [ ] **Step 2: Write test files for all 7 components**
- [ ] **Step 3: Run tests and commit**

```bash
git add src/features/search/components/*.test.tsx \
       src/features/dashboard/components/*.test.tsx \
       src/app/pages/MainPage.test.tsx src/app/pages/Inbox.test.tsx src/app/pages/Settings.test.tsx
git commit -m "test(search,dashboard,pages): add tests for search, dashboard, and simple pages"
```

---

### Task 9: Admin CRUD Tests (15 files)

**Files:** Create test files for Make, Model, and Variant CRUD components + pages

**Context files to read first:**
- `src/features/makeList/components/` — all files
- `src/features/modelList/components/` — all files
- `src/features/variantList/components/` — all files
- `src/app/pages/Makes.tsx`, `Models.tsx`, `Variants.tsx`

**All admin CRUD components follow the same patterns:**
- **Form**: React Hook Form + Zod, form fields, submit calls mutation
- **ListTable**: useQuery for data, table rows, edit/delete actions open dialogs
- **CreateDialog**: Dialog wrapper around Form, calls create mutation
- **EditDialogContent**: Pre-fills Form with existing data, calls update mutation
- **ViewDialogContent**: Read-only display of entity data

**Test cases — repeated pattern for each entity (Make, Model, Variant):**

| Component pattern | Test cases |
|-------------------|-----------|
| **XxxForm** | Renders form fields; validates required fields; calls onSubmit with form data. |
| **XxxListTable** | Renders table rows from data; edit button opens edit dialog; delete button shows confirmation; delete confirmation calls mutation. Mock: list query, delete mutation. |
| **CreateXxxDialog** | Renders dialog when open; form inside dialog; submit calls create mutation and closes dialog. Mock: create mutation. |
| **EditXxxDialogContent** | Pre-fills form with entity data; submit calls update mutation. Mock: update mutation. |
| **ViewXxxDialogContent** | Renders entity details in read-only mode. |
| **Page (Makes/Models/Variants)** | Renders create button and list table; create button opens create dialog. |

**Note for VariantForm:** This is the most complex — it has cascading make→model selects and additional fields. Read the source carefully.

- [ ] **Step 1: Read all admin CRUD source files**
- [ ] **Step 2: Write test files for Make components (3 + page = 4 files)**
- [ ] **Step 3: Write test files for Model components (5 + page = 6 files)**
- [ ] **Step 4: Write test files for Variant components (5 + page = 6 files)**
- [ ] **Step 5: Run tests and commit**

```bash
git add src/features/makeList/components/*.test.tsx \
       src/features/modelList/components/*.test.tsx \
       src/features/variantList/components/*.test.tsx \
       src/app/pages/Makes.test.tsx src/app/pages/Models.test.tsx src/app/pages/Variants.test.tsx
git commit -m "test(admin): add tests for Make, Model, Variant CRUD"
```

---

### Task 10: Shared Components & Remaining Tests (~15 files)

**Files:** Create test files for shared components, galleries, compare remainders, and UserPreferences

**Context files to read first:**
- `src/components/Header.tsx`, `Footer.tsx`, `UserMenu.tsx`, `ThemeToggle.tsx`, `LanguageSwitcher.tsx`
- `src/components/MakeSelect.tsx`, `ModelSelect.tsx`, `DefectSelector.tsx`, `VariantTable.tsx`
- `src/components/ImageArrowGallery.tsx`, `ImageHoverGallery.tsx`
- `src/features/compareListings/components/CompareAiSummary.tsx`, `CompareScoreBanner.tsx`, `CompareRow.tsx`, `DiffToggleFab.tsx`
- `src/features/userPreferences/components/UserPreferencesDialog.tsx`

**Test cases per component:**

| Component | Test cases |
|-----------|-----------|
| **Header** | Renders logo/nav; shows login/register buttons for guest; shows user menu for authenticated; shows admin links for admin permissions. Mock: Redux. |
| **Footer** | Renders footer content. Pure render test. |
| **UserMenu** | Renders dropdown trigger; clicking opens menu; menu items include settings, my listings, logout. Mock: Redux. |
| **ThemeToggle** | Renders toggle; clicking switches theme. |
| **LanguageSwitcher** | Renders language options; clicking switches language. Mock: i18n. |
| **MakeSelect** | Renders select with makes from API; selecting calls onChange. Mock: makes query. |
| **ModelSelect** | Renders select with models filtered by make; disabled when no make selected. Mock: models query. |
| **DefectSelector** | Form mode: renders checkboxes for defect categories; toggling checkbox updates state. API mode: toggling calls add/remove mutations; image upload per defect. Mock: defect categories query, mutations. |
| **VariantTable** | Renders table of variants; clicking row toggles selection; disabled state prevents selection. Mock: variants query, enum queries. |
| **ImageArrowGallery** | Renders images with navigation arrows; clicking arrows changes image; keyboard arrows work; shows image counter. |
| **ImageHoverGallery** | Renders first image by default; hovering shows image based on mouse position; shows dot indicators on hover. |
| **CompareAiSummary** | Renders AI summary text; shows loading state; shows empty state. |
| **CompareScoreBanner** | Renders score comparison; shows winner indication; conditional score display. |
| **CompareRow** | Renders row with label, value A, value B; applies diff styling. |
| **DiffToggleFab** | Renders floating button; clicking toggles diff-only mode; calls onToggle callback. |
| **UserPreferencesDialog** | Renders dialog when open; quiz flow with sliders; submitting saves preferences. Mock: preferences query, upsert mutation. |

- [ ] **Step 1: Read all shared component source files**
- [ ] **Step 2: Write test files for layout components (Header, Footer, UserMenu, ThemeToggle, LanguageSwitcher)**
- [ ] **Step 3: Write test files for form components (MakeSelect, ModelSelect, DefectSelector, VariantTable)**
- [ ] **Step 4: Write test files for gallery components (ImageArrowGallery, ImageHoverGallery)**
- [ ] **Step 5: Write test files for remaining compare components (CompareAiSummary, CompareScoreBanner, CompareRow, DiffToggleFab)**
- [ ] **Step 6: Write test for UserPreferencesDialog**
- [ ] **Step 7: Run tests and commit**

```bash
git add src/components/*.test.tsx \
       src/features/compareListings/components/CompareAiSummary.test.tsx \
       src/features/compareListings/components/CompareScoreBanner.test.tsx \
       src/features/compareListings/components/CompareRow.test.tsx \
       src/features/compareListings/components/DiffToggleFab.test.tsx \
       src/features/userPreferences/components/UserPreferencesDialog.test.tsx
git commit -m "test(shared): add tests for shared components, galleries, compare, user preferences"
```

---

### Task 11: Final Verification & Coverage Report

**Files:** No new files. Verification only.

- [ ] **Step 1: Run full test suite**

Run: `cd automotive.marketplace.client && npm run test -- --reporter=verbose`

Expected: All tests pass (0 failures).

- [ ] **Step 2: Count test coverage**

Run: `find src -name "*.test.tsx" | wc -l`

Expected: ≥62 test files.

Run: Count testable .tsx files vs tested .tsx files to confirm ≥80%.

- [ ] **Step 3: Run linter**

Run: `cd automotive.marketplace.client && npm run lint`

Expected: No new lint errors from test files.

- [ ] **Step 4: Final commit if any cleanup was needed**

```bash
git add -A
git commit -m "test: final cleanup after reaching 80% FE component test coverage"
```
