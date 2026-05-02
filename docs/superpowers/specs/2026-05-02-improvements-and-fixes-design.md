# Automotive Marketplace - Improvements & Fixes Spec

**Date:** 2026-05-02
**Scope:** 14 items across 7 groups - bug fixes, permissions, chat features, listing features, UI redesign, external API data, and a real-time dashboard.

**User Preferences:**
- No emojis in UI -- use icons (lucide-react) instead
- Lithuanian is the priority language
- Dropdown select options for all user input

---

## Group 1: Bug Fixes

### 1.1 Price Offer Decimal Formatting (#5)

**Problem:** In `MakeOfferModal.tsx`, the min/max range suggestion (1/3 of listing price to listing price) shows unformatted floats with many decimal places.

**Fix:** Apply `Math.round()` to the computed min value (`listingPrice / 3`) and use `formatCurrency()` for display in the placeholder/hint text. Ensure both min and max values show clean whole numbers or at most 2 decimal places.

**Files:** `automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx`

### 1.2 "Kontra" Translation (#6)

**Problem:** "Kontra-pasiulymas" is a poor Lithuanian translation for counter-offer.

**Fix:** Replace with "Siulyti kita kaina" for the action button and related labels. Update all counter-offer translation keys in `lt/chat.json`:
- `offerCard.counterOffer` -> appropriate label using "kita kaina"
- `offerCard.actions.counter` -> "Siulyti kita kaina"
- `makeOfferModal.counterTitle` -> updated title
- Verify the new text fits within `OfferCard` (max-w-[280px]) and `MakeOfferModal` (sm:max-w-sm) components. Adjust width if needed.

**Files:** `automotive.marketplace.client/src/lib/i18n/locales/lt/chat.json`, `automotive.marketplace.client/src/lib/i18n/locales/en/chat.json`, `OfferCard.tsx`, `MakeOfferModal.tsx`

### 1.3 Seller Insights Panel Width (#12)

**Problem:** `SellerInsightsPanel` has no max-width, so it overflows wider than the `MyListingCard` above it (which uses `w-204`).

**Fix:** Constrain the panel's outer container to match the card width. Either add `max-w-204` to the panel's root div, or ensure it's nested within the same width-constrained parent as the card.

**Files:** `automotive.marketplace.client/src/features/myListings/components/SellerInsightsPanel.tsx`, `MyListingCard.tsx`

### 1.4 OnHold Status Translation (#14)

**Problem:** At the my-listings page, listing status shows raw English "OnHold" instead of the Lithuanian translation when Lithuanian is selected.

**Fix:** The status badge rendering in `MyListingCard.tsx` likely uses the raw enum string. Replace with a translation lookup: `t("card.status.onHold")` or use a status translation map. Ensure `lt/myListings.json` has all status keys mapped (`Available`, `Removed`, `Bought`, `OnHold`). Also check `lt/listings.json` for `vehicleAttributes.status` translations.

**Files:** `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`, `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`, `automotive.marketplace.client/src/lib/i18n/locales/lt/listings.json`

---

## Group 2: Permissions Fix

### 2.1 Authorization Model (#2)

**Problem:** `DefaultUserPermissions.cs` gives all new users `ManageListings`, allowing any authenticated user to edit/delete any listing.

**Design:**
- **Remove** `ManageListings` from `DefaultUserPermissions.cs` -- only admins get this permission
- **Add ownership checks** to `UpdateListingCommandHandler` and `DeleteListingCommandHandler`:
  ```
  if (listing.SellerId != request.CurrentUserId && !hasManageListingsPermission)
      throw ForbiddenAccessException
  ```
- `ManageListings` becomes an admin-only override for managing any listing
- Regular users can only manage their own listings via ownership validation

**Frontend:**
- `ListingDetailsContent.tsx` line 42: Change `canManageListing` check to `isSeller || canManageListing` for showing edit/delete buttons
- Same pattern in any other component that gates on `ManageListings`

**Files:**
- Backend: `DefaultUserPermissions.cs`, `UpdateListingCommandHandler.cs`, `DeleteListingCommandHandler.cs`
- Frontend: `ListingDetailsContent.tsx`, any other component checking `ManageListings`

---

## Group 3: Chat Features

### 3.1 Cancel Price Offer (#1)

**Problem:** No ability to cancel a pending price offer.

**Design:**
- **New enum value:** Add `Cancelled` to `OfferStatus.cs`
- **New CQRS feature:** `CancelOffer` command + handler in `Application/Features/ChatFeatures/CancelOffer/`
  - Validates: offer exists, user is the original offerer (`Offer.SenderId == CurrentUserId`), status is `Pending`
  - Sets `Offer.Status = Cancelled`
  - Sends SignalR notification to the counterparty via `ChatHub`
- **New API endpoint:** `POST /api/chat/offers/{offerId}/cancel` in `ChatController.cs`
- **Frontend:** Add "Cancel" / "Atsaukti" button on `OfferCard.tsx`
  - Only visible when: user is the offer sender AND status is `Pending`
  - Styled as a secondary/muted button
- **Translations:** Add `offerCard.actions.cancel: "Atsaukti"` to both `en/chat.json` and `lt/chat.json`
- **Status config in OfferCard:** Add `Cancelled` status with muted gray styling (similar to `Expired`)

**Files:**
- Backend: `OfferStatus.cs`, new `CancelOffer/` folder, `ChatController.cs`
- Frontend: `OfferCard.tsx`, `en/chat.json`, `lt/chat.json`, offer types
- Tests: New `CancelOfferCommandHandlerTests.cs`

### 3.2 Cancel Contract (#8)

**Problem:** Need ability for either party to cancel a contract before both submissions are complete.

**Design:**
- The `CancelContract` feature already exists in the backend. Verify/update:
  - **Validation:** Either party (buyer or seller) can cancel
  - **Condition:** Cancel allowed when contract status is `Pending`, `Active`, `SellerSubmitted`, or `BuyerSubmitted` -- meaning at least one party has NOT yet submitted their form. Once both have submitted (status is `Complete`), cancellation is blocked.
  - This means: if only the seller has submitted, the buyer can still cancel (and vice versa)
- **Frontend:** Add "Cancel Contract" / "Atsaukti sutarti" button on `ContractCard.tsx`
  - Visible when: status is `Pending`, `Active`, `SellerSubmitted`, or `BuyerSubmitted` (but NOT when both are submitted / `Complete`)
  - Confirmation dialog before cancellation
- **Translations:** Ensure `contractCard.actions.cancel: "Atsaukti sutarti"` exists

**Files:**
- Backend: `CancelContractCommandHandler.cs` (verify/update validation)
- Frontend: `ContractCard.tsx`, translation files
- Tests: Verify/update `CancelContractCommandHandlerTests.cs`

---

## Group 4: Listing Features

### 4.1 Edit Own Listing Navigation (#3)

**Problem:** Clicking edit on your own listing opens a dialog instead of navigating to the edit page.

**Design:**
- In `ListingDetailsContent.tsx`:
  - If `isSeller`: render a `<Button>` that navigates to `/my-listings/$id` (the existing edit page)
  - If `canManageListing && !isSeller` (admin): keep the `<EditListingDialog>` for quick inline editing
- This gives sellers the full-page editing experience while admins get the quick dialog

**Files:** `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

### 4.2 Mark Listing as Sold (#4)

**Problem:** No way for a seller to mark a listing as sold when the sale happened outside the platform.

**Design:**
- Reuse existing `Bought` status (no new enum value)
- **Backend:** Verify `UpdateListingStatus` handler allows setting status to `Bought`. Add ownership validation (only seller or admin).
- **Frontend:** Add "Mark as Sold" / "Pazymeti kaip parduota" button in `MyListingCard.tsx`
  - Only visible when listing status is `Available`
  - Shows confirmation dialog: "Are you sure? This will remove the listing from search results."
  - On confirm, calls `UpdateListingStatus` with `Bought`
- **Translation:** Add `card.markAsSold: "Pazymeti kaip parduota"` and confirmation text to `lt/myListings.json`

**Files:**
- Backend: `UpdateListingStatusCommandHandler.cs` (verify/update)
- Frontend: `MyListingCard.tsx`, translation files

### 4.3 Like from Listing Details (#7)

**Problem:** Can only like listings from the search results page, not from listing details.

**Design:**
- **Backend:** Verify `GetListingById` response includes `isLiked` field. If not, add it (query the `UserListingLikes` table for current user).
- **Frontend:** Add a heart icon button in `ListingDetailsContent.tsx`
  - Position: in the title card area, next to the edit/delete buttons
  - Only visible when: user is logged in AND `!isSeller`
  - Use existing `useToggleLike` hook
  - Heart icon: outline when not liked, filled red when liked (same pattern as `ListingCard.tsx`)
  - Use `IoHeartOutline` / `IoHeart` icons (already imported in ListingCard)

**Files:**
- Backend: `GetListingByIdQueryHandler.cs` (add `isLiked` if missing), `GetListingByIdResponse`
- Frontend: `ListingDetailsContent.tsx`, possibly `useToggleLike.ts`

---

## Group 5: UI Redesign

### 5.1 Footer Component (#10)

**Problem:** LinkedIn and GitHub links are on the main page body. Should be in a page footer visible on all pages.

**Design:**
- Create `src/components/layout/footer/Footer.tsx`
  - Horizontal layout: links on left (GitHub, LinkedIn with `PiGithubLogo`, `PiLinkedinLogo` icons), optional copyright text on right
  - Muted styling, consistent with the header's design language
  - `border-t` top border separator
- Add `<Footer />` to the root layout (alongside the Header) so it appears on every page
- Remove the links section from `MainPage.tsx`

**Files:**
- New: `automotive.marketplace.client/src/components/layout/footer/Footer.tsx`
- Edit: `MainPage.tsx` (remove links), root layout file (add Footer)

### 5.2 Listing Details Page Redesign (#13)

**Problem:** Current listing details has overly rounded corners (inconsistent with search results) and specs are buried in a narrow sidebar.

**Design (Option A -- approved):**

**Layout change:** Keep the 3-column grid (`lg:grid-cols-3`). Left 2 columns get the content reorganized:

1. **Image gallery** (full width of left 2 columns, as current)
2. **Two side-by-side cards** below the gallery:
   - **"Key Specs" card:** Uses pill/badge components reusing `ListingCardBadge` style from `ListingCard.tsx`. Shows: Engine (size + power), Fuel type, Transmission, Mileage, Year, Drivetrain. 2x3 grid of badge components with icons.
   - **"Details" card:** Clean label-value rows without icons. Shows: Body type, Colour, Doors, Steering, VIN, Seller. Uses `dl/dt/dd` with dividers (similar to current specs but moved here).
3. **Description card** below the spec cards
4. **Defects card** below description

**Right sidebar** (simplified):
- Title card with price, Used/New + location badges, like button (new), edit/delete buttons
- Contact Seller button
- Compare With Another button
- ScoreCard (stays as-is)
- AiSummarySection (stays as-is)

**Styling consistency:**
- All cards use `rounded-lg` (matching search results page -- NOT `rounded-xl` or larger)
- Consistent `border`, `shadow-sm`, `p-6` pattern across all cards

**Files:** `ListingDetailsContent.tsx`, possibly extract `ListingSpecsCard.tsx` and `ListingDetailsCard.tsx` as new components

---

## Group 6: External API Data

### 6.1 Show External Data in Comparison & Details (#11)

**Problem:** Fuel economy, recalls, safety rating, and market price data from external APIs is not shown in the comparison or listing details pages.

**Backend:**
- Verify `GetListingComparison` response includes external API data fields. If not, extend the handler to fetch/include cached data from VPIC, NHTSA, FuelEconomy, and Cardog APIs.
- Verify `GetListingById` response includes these fields. Extend if needed.
- Add conditional Cardog API calls: only call if user's `enableMarketPriceApi` setting is true AND data is not already cached. Always return cached data regardless of setting.

**User Preferences extension:**
- Add `enableMarketPriceApi: boolean` (default: `false`) to `UserPreferences` entity
- Add to `UpsertUserPreferences` command validation
- Add EF migration for the new column

**Frontend -- Comparison page (`CompareTable.tsx`):**
- Add new table sections:
  - "Fuel Economy" section: MPG city/highway, CO2 emissions (from FuelEconomy API)
  - "Safety" section: Overall rating, crash test results (from NHTSA API)
  - "Market Data" section: Median market price, total market listings (from Cardog API)
  - "Recalls" section: Number of recalls, most recent recall date
- Show "No data available" for missing fields
- Market price section: show "Enable in settings" message if setting is off and no cached data

**Frontend -- Listing details:**
- Add an "External Data" card in the main content area (below the two spec cards, above description)
- Show available data in a clean card format
- Market price: show "Enable in settings" link when no data and setting is off

**Frontend -- Settings page:**
- Add new toggle: "Enable automatic market price lookups" / "Ijungti automatini rinkos kainos tikrinima"
- Add explanatory note: "Market price data uses an external API that may incur costs. When disabled, cached data is still shown."

**Files:**
- Backend: `GetListingComparisonQueryHandler.cs`, `GetListingByIdQueryHandler.cs`, `UserPreferences.cs`, migration
- Frontend: `CompareTable.tsx`, `ListingDetailsContent.tsx`, Settings page, translation files

---

## Group 7: Real-Time Dashboard

### 7.1 Main Page Dashboard (#9)

**Problem:** No central place to see pending actions, upcoming meetings, and items requiring user attention.

**Design (Option A -- Compact Tile Grid, approved):**

**Backend:**
- New feature: `Application/Features/DashboardFeatures/GetDashboardSummary/`
  - `GetDashboardSummaryQuery.cs` with `CurrentUserId`
  - `GetDashboardSummaryQueryHandler.cs` -- aggregates:
    - Pending offers where user is the responder (count + newest offer details)
    - Upcoming meetings for user (count + next meeting datetime and counterparty)
    - Contracts needing user action (count + which action: respond/fill form)
    - Pending availability requests (count)
    - Unread messages count (optional, already shown in header)
  - `GetDashboardSummaryResponse.cs` with nested sections
- New API endpoint: `GET /api/dashboard/summary` in new `DashboardController.cs`

**SignalR real-time updates:**
- Extend notification pattern: when any mutation handler (MakeOffer, RespondToOffer, ProposeMeeting, RespondToMeeting, RequestContract, RespondToContract, ShareAvailability, etc.) completes, send a `DashboardUpdated` event to the affected user(s) via ChatHub
- The event payload is lightweight -- just a signal to refresh, not the full dashboard data
- Frontend receives the event and invalidates the TanStack Query cache for the dashboard query

**Frontend:**
- New feature: `src/features/dashboard/`
  - `components/Dashboard.tsx` -- main container, rendered in MainPage when user is logged in
  - `components/DashboardTile.tsx` -- reusable tile: icon, count badge, title, subtitle, detail text
  - `components/DashboardExpandedSection.tsx` -- expanded view when tile is clicked, shows list of items with action buttons
  - `api/getDashboardSummaryOptions.ts` -- query options
  - `api/useDashboardHub.ts` -- hook that listens for `DashboardUpdated` SignalR event and calls `queryClient.invalidateQueries(['dashboard-summary'])`
  - `types/GetDashboardSummaryResponse.ts`
- Icons: `Calendar`, `CircleDollarSign`, `FileText`, `Clock` from lucide-react (NO emojis)
- Tile colors: amber border for tiles with pending actions, default border for zero-count tiles
- "View all in chat" link on expanded sections navigates to the conversations page
- Responsive: 4 columns on desktop, 2 on tablet, 1 on mobile
- Translations: new `lt/dashboard.json` and `en/dashboard.json` namespace

**Files:**
- Backend: New `DashboardFeatures/` folder, `DashboardController.cs`, ChatHub modifications
- Frontend: New `dashboard/` feature directory, `MainPage.tsx` modification
- Tests: `GetDashboardSummaryQueryHandlerTests.cs`

---

## Summary of All Changes

| # | Item | Group | Complexity |
|---|------|-------|------------|
| 1 | Cancel price offer | 3 - Chat | Medium |
| 2 | Fix permissions | 2 - Permissions | Medium |
| 3 | Edit own listing navigation | 4 - Listing | Low |
| 4 | Mark listing as sold | 4 - Listing | Low-Medium |
| 5 | Price offer decimal format | 1 - Bug Fix | Low |
| 6 | Kontra translation | 1 - Bug Fix | Low |
| 7 | Like from listing details | 4 - Listing | Low-Medium |
| 8 | Cancel contract | 3 - Chat | Medium |
| 9 | Dashboard with real-time | 7 - Dashboard | High |
| 10 | Move links to footer | 5 - UI | Low |
| 11 | External API data display | 6 - External API | High |
| 12 | Seller insights width | 1 - Bug Fix | Low |
| 13 | Listing details redesign | 5 - UI | Medium-High |
| 14 | OnHold translation | 1 - Bug Fix | Low |

**Execution order:** Groups 1 through 7 sequentially, with each group's items done in parallel where possible.
