# Frontend Component Test Coverage — Design Spec

**Date:** 2026-05-03
**Goal:** 80% of feature components + pages have at least one test file

## Target Metric

| Metric | Value |
|--------|-------|
| Testable .tsx files (features + pages + non-shadcn shared) | 77 |
| 80% target | 62 files with tests |
| Already tested | 4 (CompareHeader, CompareSearchModal, CompareTable, Compare page) |
| New test files needed | ~58 |

### What counts

- Feature components under `src/features/`
- Page components under `src/app/pages/`
- Non-shadcn shared components under `src/components/` (Header, Footer, selects, galleries, DefectSelector, VariantTable, etc.)

### What does NOT count

- shadcn/ui primitives (Button, Card, Dialog, etc.)
- Pure .ts utility/hook/API files
- Route files under `src/app/routes/`
- Type definition files

## Test Framework

Already configured:

- **Vitest** (v4.1.5) with jsdom environment
- **React Testing Library** (v16.3.2) + user-event (v14.6.1)
- **@testing-library/jest-dom** (v6.9.1)
- Config in `vite.config.ts` → `test` block
- Setup file: `src/test/setup.ts`

## Test Depth

Each test file covers:

1. **Renders without crashing** — basic smoke test
2. **Key interactions** — click handlers, form submissions, toggle states
3. **Conditional rendering** — auth-based, permission-based, prop-based visibility
4. **Loading/error states** — where applicable (API-dependent components)

Does NOT require: snapshot tests, full branch coverage, visual regression, E2E flows.

## Mocking Strategy

Follow existing patterns established in `compareListings` tests:

- `vi.mock()` at module level for API hooks, router, i18n
- `vi.hoisted()` for mock function references used inside `vi.mock`
- `QueryClientProvider` wrapper with retry disabled
- Direct prop mocking for child component callbacks

### Mocked dependencies

| Dependency | Mock approach |
|-----------|--------------|
| API hooks (useQuery, useMutation) | `vi.mock("../api/useXxx")` returning mock data/functions |
| Redux store | `vi.mock("@/hooks/redux")` with configurable selectors |
| TanStack Router | `vi.mock("@tanstack/react-router")` with mock navigate/params |
| i18next | `vi.mock("react-i18next")` returning keys as-is |
| SignalR (chat only) | `vi.mock("@microsoft/signalr")` with mock hub connection |
| browser-image-compression | `vi.mock("browser-image-compression")` returning passthrough |

## Test Infrastructure

### Enhanced setup — `src/test/setup.ts`

Add global mocks:

- `window.matchMedia` (theme toggle tests)
- `IntersectionObserver` (lazy-loaded components)
- Silence `console.error` for expected React warnings in tests

### Custom render — `src/test/test-utils.tsx`

A `renderWithProviders()` function wrapping:

- `QueryClientProvider` (retry: false, gcTime: 0)
- Redux `Provider` with configurable initial state
- i18n provider (passthrough mode)

Re-exports everything from `@testing-library/react` so tests import from `@/test/test-utils` instead.

### Mock factories — `src/test/mocks/`

| File | Purpose |
|------|---------|
| `mockAuthState.ts` | Authenticated and guest Redux state presets |
| `mockListingData.ts` | Listing, make, model, variant fixture factories |
| `mockRouter.ts` | TanStack Router navigate/params/search mock helpers |
| `mockSignalR.ts` | SignalR HubConnection mock with on/invoke/start/stop |

### Test file placement

Co-located next to the source file:

```
src/features/auth/components/LogoutButton.tsx
src/features/auth/components/LogoutButton.test.tsx
```

This matches the existing pattern in `compareListings`.

## Phases

Tests are written feature-by-feature in business criticality order.

### Phase 1 — Auth (4 test files)

| Component | File | Complexity | Key tests |
|-----------|------|-----------|-----------|
| Login page | `src/app/pages/Login.test.tsx` | Medium | Form render, password toggle, submit valid/invalid, redirect on success |
| Register page | `src/app/pages/Register.test.tsx` | Medium | Form render, validation errors, submit, redirect |
| LogoutButton | `src/features/auth/components/LogoutButton.test.tsx` | Simple | Render, click dispatches clearCredentials |
| RegisterButton | `src/features/auth/components/RegisterButton.test.tsx` | Simple | Render, click navigates to /register |

### Phase 2 — Chat (14 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| MessageThread | Complex | Mock SignalR, render message types, send message, auto-scroll |
| ConversationList | Complex | Render list, click selects conversation, unread indicators |
| ContractFormDialog | Complex | Seller vs buyer form, validation, submit |
| ContractCard | Complex | Display contract, accept/reject |
| MeetingCard | Complex | Display meeting, accept/decline |
| OfferCard | Complex | Display offer, accept/counter/decline |
| ActionBar | Medium | Button clicks open correct modals |
| ChatPanel | Medium | Layout render, conditional sections |
| MakeOfferModal | Medium | Form validation, submit |
| ProposeMeetingModal | Medium | Date/time picker, submit |
| ShareAvailabilityModal | Medium | Calendar selection, submit |
| AvailabilityCardComponent | Medium | Render availability data |
| ListingCard (chat) | Simple | Render listing info |
| UnreadBadge | Simple | Conditional badge render |

### Phase 3 — Listing Details (8 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| ListingDetailsContent | Complex | Permission rendering (seller/admin/guest), like toggle, chat button |
| EditListingDialog | Medium | Open/close, form pre-fill |
| EditListingForm | Medium | Cascading selects, validation, submit |
| AiSummarySection | Medium | Conditional render, loading state |
| ScoreCard | Medium | Score display, conditional sections |
| ListingKeySpecs | Simple | Render specs from props |
| ListingSecondaryDetails | Simple | Render detail fields |
| ListingDetails page | Simple | Route param extraction |

### Phase 4 — Listing List & Browse (8 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| Listings page | Medium | Filter coordination, pagination |
| Filters | Medium | Multiple filter controls |
| BasicFilters | Medium | Select interactions |
| RangeFilters | Medium | Range slider |
| ModelFilter | Medium | Cascading make→model |
| ListingCard | Medium | Render, like toggle, navigation |
| ListingCardBadge | Simple | Badge render |
| ListingList | Simple | Map listings to cards |

### Phase 5 — My Listings (6 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| MyListingDetail | Complex | Inline editing, delete confirmation, defect management |
| MyListingCard | Complex | Status actions |
| MyListingsPage | Medium | List render, empty state |
| SellerInsightsPanel | Medium | Stats display |
| ListingBuyerPanel | Medium | Buyer info |
| EditableField | Medium | Inline edit toggle, save/cancel |

### Phase 6 — Create Listing (4 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| CreateListingForm | Complex | Multi-step form, cascading selects, image upload |
| CreateListing page | Medium | Guest warning, form disable |
| ImageUploadInput | Medium | File selection, preview |
| ImagePreview | Medium | Display, remove |

### Phase 7 — Saved Listings (4 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| NoteEditor | Complex | Debounce save, auto-save on unmount |
| SavedListingsPage | Medium | List render, empty state |
| SavedListingRow | Medium | Row render, unlike |
| PropertyMentionPicker | Medium | Dropdown, select mention |

### Phase 8 — Search & Dashboard (5 test files)

| Component | Complexity | Key tests |
|-----------|-----------|-----------|
| ListingSearch | Medium | Search input, filter toggles |
| ListingSearchFilters | Medium | Filter selects |
| Dashboard | Medium | Tile rendering |
| DashboardTile | Medium | Conditional content |
| MainPage page | Simple | Auth-conditional dashboard |

### Phase 9 — Admin CRUD (15 test files)

**Make List (3 + page):** MakeForm, MakeListTable, CreateMakeDialog, Makes page
**Model List (5 + page):** ModelForm, ModelListTable, EditModelDialogContent, ViewModelDialogContent, CreateModelDialog, Models page
**Variant List (5 + page):** VariantForm, VariantListTable, EditVariantDialogContent, ViewVariantDialogContent, CreateVariantDialog, Variants page

### Phase 10 — Shared Components & Remaining (~14 test files)

DefectSelector, MakeSelect, ModelSelect, VariantTable, Header, UserMenu, ImageArrowGallery, ImageHoverGallery, Footer, ThemeToggle, LanguageSwitcher, UserPreferencesDialog, Inbox page, Settings page, CompareAiSummary, CompareScoreBanner, remaining compare components.

## Complexity Distribution

| Complexity | Count | % | Avg test lines |
|-----------|-------|---|---------------|
| Simple | 25 | 32% | ~100 |
| Medium | 28 | 36% | ~400 |
| Complex | 24 | 31% | ~800 |

## Key Testing Patterns

### Redux auth state

```typescript
const mockUseAppSelector = vi.fn();
vi.mock("@/hooks/redux", () => ({ useAppSelector: (fn) => mockUseAppSelector(fn) }));
// Test: mockUseAppSelector.mockReturnValue("some-user-id") for authenticated
// Test: mockUseAppSelector.mockReturnValue(null) for guest
```

### React Query hooks

```typescript
const mockUseGetListing = vi.fn();
vi.mock("../api/useGetListing", () => ({ useGetListing: () => mockUseGetListing() }));
// Test: mockUseGetListing.mockReturnValue({ data: mockListing, isLoading: false })
```

### Form submission

```typescript
const user = userEvent.setup();
await user.type(screen.getByLabelText(/email/i), "test@example.com");
await user.type(screen.getByLabelText(/password/i), "Password123!");
await user.click(screen.getByRole("button", { name: /login/i }));
expect(mockLogin).toHaveBeenCalledWith({ email: "test@example.com", password: "Password123!" });
```

### Conditional rendering

```typescript
// Guest view
mockUseAppSelector.mockReturnValue(null);
render(<Component />);
expect(screen.queryByText(/edit/i)).not.toBeInTheDocument();

// Owner view
mockUseAppSelector.mockReturnValue("owner-id");
render(<Component />);
expect(screen.getByText(/edit/i)).toBeInTheDocument();
```

## Success Criteria

- ≥62 out of 77 testable .tsx files have a co-located `.test.tsx` file
- All tests pass with `npm run test`
- Each test covers render + key interactions (not just smoke tests)
- No flaky tests (proper async handling with `waitFor`)

## Out of Scope

- Line/branch coverage metrics (only file coverage counts)
- Pure .ts utility tests
- API hook tests (query options, mutations)
- E2E tests
- Visual regression tests
- shadcn/ui primitive tests
- Performance testing
