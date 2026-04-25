# My Listings, Image Gallery & Defect Marking

Adds owner-facing listing management with inline editing, a multi-image gallery across the app, and a defect marking system with photo attachments.

## Decisions

- **My Listings access:** User avatar dropdown in navbar (replaces standalone logout button), grouped into sections: My Listings, Admin (permission-gated), Account
- **Editing model:** Inline field editing on a detail view — pencil icon per field, edit-in-place, floating save bar for batch save
- **Defect categories:** Backend enum with translations (like Fuel, BodyType), plus custom defect support
- **Defect photos:** Optional, up to 3 per defect, stored as Image entities linked to a ListingDefect
- **Image gallery:** Card hover-swipe (AliExpress-style zones) + detail page arrow gallery with thumbnail reel
- **Gallery polish level:** Functional first pass — no animations/transitions/preloading
- **My Listings layout:** Single column card list

## Backend Architecture

### New Entities

**DefectCategory** — enum entity following existing pattern (like Fuel, BodyType):
- `Id: Guid`
- `Name: string` (English default)
- `Translations: Translation[]` (EN/LT)
- Predefined values: Scratch, Dent, Rust, Paint Damage, Crack, Corrosion, Stain, Mechanical Wear

**ListingDefect** — joins a Listing to a DefectCategory:
- `Id: Guid`
- `ListingId: Guid`
- `DefectCategoryId: Guid?` (null for custom defects)
- `CustomName: string?` (populated when DefectCategoryId is null)
- `Listing: Listing` (navigation)
- `DefectCategory: DefectCategory?` (navigation)
- `Images: ICollection<Image>` (defect photos)

**Extend Image entity:**
- Add `ListingDefectId: Guid?` (nullable FK) — when set, this image is a defect photo rather than a listing photo
- Add `ListingDefect: ListingDefect?` (navigation)
- An image belongs to either a listing directly (ListingDefectId = null) or to a defect (ListingDefectId != null). Both still have ListingId set for the parent listing.

**Extend Listing entity:**
- Add `Defects: ICollection<ListingDefect>` navigation property

### New CQRS Handlers

**Queries:**
- `GetMyListings` — returns current user's listings with status, thumbnail, defect count, image count. Uses `ICurrentUserService` to get the seller ID. Returns `GetMyListingsResponse` with a list of listing summaries.
- `GetDefectCategories` — returns all defect categories with translations (follows existing enum query pattern like GetAllFuels).

**Commands:**
- `AddListingDefect` — adds a defect to a listing. Accepts `ListingId`, `DefectCategoryId?`, `CustomName?`. Returns the created defect's ID.
- `RemoveListingDefect` — removes a defect and its associated images from a listing. Deletes images from MinIO.
- `AddDefectImage` — uploads an image and links it to a ListingDefect. Max 3 per defect (validate in handler). Stores in MinIO like existing listing images.
- `RemoveDefectImage` — removes a defect image. Deletes from MinIO.

**Extended Handlers:**
- `GetListingById` — extend response to include `defects: ListingDefectDto[]` where each defect has `id`, `defectCategoryId?`, `defectCategoryName?`, `customName?`, `images: ImageDto[]`
- `GetAllListings` — extend response to include `defectCount: int` and `imageCount: int` (total including defect images)
- `CreateListing` — extend command to accept an optional `defects` array with `{ defectCategoryId?, customName?, images: IFormFile[] }`
- `UpdateListing` — extend to accept partial field updates (only send changed fields)

### New Controller

**DefectController** (`/api/defect`):
- `GET /defect/categories` → GetDefectCategories
- `POST /defect` → AddListingDefect (requires ManageListings or ownership)
- `DELETE /defect/{id}` → RemoveListingDefect
- `POST /defect/{defectId}/image` → AddDefectImage
- `DELETE /defect/image/{imageId}` → RemoveDefectImage

**Extend ListingController:**
- `GET /listing/my` → GetMyListings (uses current user from JWT)

### Database Migration

- Add `DefectCategories` table with seed data (8 predefined categories with EN/LT translations)
- Add `ListingDefects` table
- Add `ListingDefectId` nullable FK column to `Images` table
- Configure relationships in EF Core configuration classes

## Frontend Architecture

### Navbar Reorganization

Replace the standalone logout button with a user avatar dropdown using shadcn `DropdownMenu`.

**Avatar trigger:** Circle with user's first initial (from JWT claims or Redux store), replaces the logout button position in the header.

**Dropdown sections (separated by dividers):**

1. **My Listings** section:
   - "My Listings" — navigates to `/my-listings`

2. **Admin** section (only visible with ManageListings permission):
   - "Makes" — navigates to `/makes`
   - "Models" — navigates to `/models`
   - "Variants" — navigates to `/variants`

3. **Account** section:
   - "Profile Settings" — disabled/greyed out with tooltip "Coming soon" (no page behind it)
   - "Log out" — styled in red, calls existing logout logic

**Implementation:** New `UserMenu.tsx` component in `src/components/layout/header/`. Remove the existing logout button and admin nav links from the header. Add this dropdown in their place.

### My Listings Page

**Route:** `/my-listings`
**Feature folder:** `src/features/myListings/`

**Page layout:** Single column list of listing cards showing:
- Thumbnail image
- Title (Year Make Model)
- City, mileage, price
- Status badge (Active = green, Sold = grey)
- Defect count badge (amber, only if > 0)
- Image count indicator
- Edit and Delete action buttons

**Card behavior:**
- Clicking "Edit" or the card itself navigates to the inline edit view at `/my-listings/{id}`
- Sold listings appear dimmed with no action buttons
- "Create Listing" button in the page header links to existing `/create-listing`

**Empty state:** "You haven't created any listings yet. Create your first listing!"

**API:** Use existing `GetAllListings` with `UserId` filter for now, or the new `GET /listing/my` endpoint.

### Inline Edit View

**Route:** `/my-listings/{id}`
**Component:** `MyListingDetail.tsx` in the myListings feature folder

**Layout:**
- Back button → navigates to `/my-listings`
- Image gallery (arrow gallery + thumbnail reel — reusable component)
- Title (Year Make Model) — read-only (derived from variant/make/model)
- Status badge + Delete button in header
- Stacked field rows, each with label, value, and pencil edit icon
- Defects section at the bottom

**Editable fields** (each row shows label + value + pencil icon):
- Price (number input)
- City (text input)
- Mileage (number input)
- Description (textarea)
- Colour (text input)
- VIN (text input)
- Is Used (toggle/switch)
- Steering Wheel (toggle: Left/Right)

**Non-editable fields** (displayed without pencil icon):
- Make, Model, Variant — these define the core vehicle identity
- Fuel, Transmission, Body Type, Drivetrain, Doors, Engine Power, Engine Size — part of variant

**Edit flow:**
1. Click pencil icon → field value becomes an input (appropriate type for the field)
2. Confirm (✓) or cancel (✗) the individual field edit
3. Confirmed changes are tracked locally but not yet saved
4. A floating save bar appears at the bottom: "N unsaved changes" with "Discard" and "Save Changes" buttons
5. "Save Changes" sends all modified fields to `UpdateListing` in a single request
6. "Discard" reverts all pending changes

**Delete flow:** Confirmation dialog → calls `DeleteListing` → redirects to `/my-listings`

### Defects Section (in Create Form + Inline Edit)

**Reusable component:** `DefectSelector.tsx` in `src/components/forms/`

**Layout:**
1. "Known Defects" header with "(optional)" label
2. 2-column checkbox grid of predefined defect categories (fetched from `GetDefectCategories`, displayed with `getTranslatedName`)
3. "Add custom defect" text input + Add button below the grid
4. "Selected Defects" section below — shows each checked/added defect with:
   - Defect name (amber left border)
   - Photo count "N / 3 photos"
   - Photo thumbnails with remove (✗) buttons
   - "+" button to add more photos (up to 3)

**In create listing form:** Appears as a new section below the image upload. Selected defects and their photos are included in the `CreateListing` command.

**In inline edit view:** Appears at the bottom of the field list. Uses `AddListingDefect`/`RemoveListingDefect` and `AddDefectImage`/`RemoveDefectImage` commands. Changes are immediate (not part of the floating save bar) since they're separate API calls.

### Image Gallery Components

**1. `ImageHoverGallery.tsx`** — for listing cards

Wrap the card's image area. Divides the image container into N horizontal zones (where N = number of images). As the mouse moves across the image:
- Calculate which zone the cursor is in based on x-position
- Display the corresponding image
- Show dot indicators at the bottom (active dot = filled white, others = translucent)
- On mouse leave, revert to the first image

**Props:** `images: { url: string; altText: string }[]`, `fallbackUrl: string`

**Used in:** `ListingCard.tsx` (replaces the current single `<img>` tag)

**2. `ImageArrowGallery.tsx`** — for listing detail page

Full gallery with:
- Large main image area with left/right arrow buttons
- Image counter "N / M" in top-right corner
- Thumbnail strip below the main image
- Click a thumbnail to jump to that image
- Active thumbnail has an indigo border
- Defect thumbnails (after a thin separator) have an amber border + defect name label below

**Props:** `images: GalleryImage[]` where:
```typescript
type GalleryImage = {
  url: string;
  altText: string;
  defectName?: string; // If set, this is a defect photo
};
```

**Used in:** `ListingDetailsContent.tsx` (replaces current single image) and `MyListingDetail.tsx`

**Defect photo ordering:** Regular listing images first (ordered by upload time), then defect images grouped by defect, each group labeled.

### API Response Changes (Frontend Types)

**GetAllListingsResponse** — add:
- `imageCount: number`
- `defectCount: number`
- `images: { url: string; altText: string }[]` (all listing images for hover gallery)

**GetListingByIdResponse** — add:
- `defects: ListingDefectDto[]`

**New types:**
```typescript
type ListingDefectDto = {
  id: string;
  defectCategoryId?: string;
  defectCategoryName?: string;
  customName?: string;
  images: { url: string; altText: string }[];
};

type DefectCategory = {
  id: string;
  name: string;
  translations: Translation[];
};
```

## What NOT to Build

- Profile settings page (placeholder link only)
- Listing status management (mark as sold, etc.) — future feature
- Image reordering/drag-and-drop
- Defect severity levels
- Image zoom/lightbox
- Pagination on My Listings (simple list for now)

## i18n

All new UI strings must be translated to both English and Lithuanian, following the existing i18n patterns:
- New namespace keys added to existing namespaces (common for navbar, listings for gallery, admin for defect categories)
- Or new `myListings` namespace if string count warrants it
- Use `getTranslatedName` for defect category names from the API
- Use `useDateLocale` for any date formatting

## Files Affected

### New Files (Backend)
- `Domain/Entities/DefectCategory.cs`
- `Domain/Entities/ListingDefect.cs`
- `Infrastructure/Data/Configuration/DefectCategoryConfiguration.cs`
- `Infrastructure/Data/Configuration/ListingDefectConfiguration.cs`
- `Application/Features/DefectFeatures/GetDefectCategories/` (query + handler + response)
- `Application/Features/DefectFeatures/AddListingDefect/` (command + handler)
- `Application/Features/DefectFeatures/RemoveListingDefect/` (command + handler)
- `Application/Features/DefectFeatures/AddDefectImage/` (command + handler)
- `Application/Features/DefectFeatures/RemoveDefectImage/` (command + handler)
- `Application/Features/ListingFeatures/GetMyListings/` (query + handler + response)
- `Server/Controllers/DefectController.cs`
- EF Core migration file

### New Files (Frontend)
- `src/components/layout/header/UserMenu.tsx`
- `src/components/forms/DefectSelector.tsx`
- `src/components/gallery/ImageHoverGallery.tsx`
- `src/components/gallery/ImageArrowGallery.tsx`
- `src/features/myListings/` (page, components, types, API hooks)
- `src/api/defect/` (query options + mutation hooks)

### Modified Files (Backend)
- `Domain/Entities/Image.cs` — add ListingDefectId FK
- `Domain/Entities/Listing.cs` — add Defects navigation property
- `Infrastructure/Data/Configuration/ImageConfiguration.cs` — add defect relationship
- `Infrastructure/Data/ApplicationDbContext.cs` — add DefectCategories, ListingDefects DbSets
- `Application/Features/ListingFeatures/GetListingById/` — extend response with defects
- `Application/Features/ListingFeatures/GetAllListings/` — extend response with imageCount, defectCount, all image URLs
- `Application/Features/ListingFeatures/CreateListing/` — accept defects in command
- `Application/Features/ListingFeatures/UpdateListing/` — support partial updates
- `Server/Controllers/ListingController.cs` — add /listing/my endpoint

### Modified Files (Frontend)
- `src/components/layout/header/Header.tsx` — replace logout button with UserMenu, remove admin links
- `src/features/listingList/components/ListingCard.tsx` — use ImageHoverGallery
- `src/features/listingDetails/components/ListingDetailsContent.tsx` — use ImageArrowGallery, show defects
- `src/features/createListing/components/CreateListingForm.tsx` — add DefectSelector section
- `src/app/Router.tsx` — add /my-listings and /my-listings/:id routes
- Various i18n JSON files for new translation keys
