# Saved Listings (Likes + Notes) — Design Spec

**Date:** 2026-04-20

## Problem & Approach

Users need a way to bookmark listings they're interested in and attach private research notes to them. Two related needs (liking and noting) are unified into a single "Saved Listings" hub rather than separate lists, since a user will often both like and annotate the same listing.

The `UserListingLike` entity already exists in the domain. This feature exposes it via the API, adds a `UserListingNote` entity alongside it, and builds the hub page and listing card interactions.

---

## Scope

### In scope
- Heart icon on listing cards (hover → like/unlike)
- `/saved` hub page — dense list of liked listings with inline note editing
- Property-mention chip UI in the note editor (no backend binding yet)
- Backend: ToggleLike, GetSavedListings, UpsertListingNote, DeleteListingNote

### Out of scope (deferred)
- Property-pinned notes backend logic (structured field binding, change tracking)
- Price-drop or status-change notifications
- Note access from the listing details page
- Like counts visible to other users

---

## Data Model

### Existing (no changes needed)
```csharp
// Already in domain
public class UserListingLike : BaseEntity
{
    public Guid ListingId { get; set; }
    public virtual Listing Listing { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}

// Listing already has:
public virtual ICollection<User> LikeUsers { get; set; } = [];
```

### New entity — `UserListingNote`
```csharp
public class UserListingNote : BaseEntity
{
    public Guid ListingId { get; set; }
    public virtual Listing Listing { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}
```

One note per user per listing (unique constraint on `UserId + ListingId`). Upsert semantics — create or replace on save. Deleting a like cascade-deletes the associated note.

---

## Backend Operations

### `ToggleLikeCommand`
- Input: `ListingId`
- Behaviour: if like exists → delete it (and cascade-delete the note); otherwise → create it
- Returns: `{ IsLiked: bool }`

### `GetSavedListingsQuery`
- Input: authenticated user (from JWT)
- Returns: list of liked listings with the user's note content included — single JOIN query, no N+1
- Response shape per item:
  ```
  ListingId, Title (year/make/model — requires join through Variant → Model → Make),
  Price, City, Mileage, Fuel, Transmission, ThumbnailUrl (nullable), NoteContent (nullable)
  ```

### `UpsertListingNoteCommand`
- Input: `ListingId`, `Content`
- Validates: user must have a like for this listing
- Behaviour: insert or update the note record

### `DeleteListingNoteCommand`
- Input: `ListingId`
- Behaviour: deletes the note record if it exists (like remains intact)
- Triggered when the user clears all text and blurs the note field

---

## Frontend Architecture

### New route
`/saved` — protected (redirects to login if unauthenticated)

### New feature folder
`src/features/savedListings/`
```
api/
  useGetSavedListings.ts
  useToggleLike.ts
  useUpsertListingNote.ts
  useDeleteListingNote.ts
components/
  SavedListingsPage.tsx
  SavedListingsList.tsx
  SavedListingRow.tsx
  PropertyMentionPicker.tsx
index.ts
types.ts
```

### Modified
- `src/features/listingList/components/ListingCard.tsx` — add heart icon overlay
- App header — add nav icon link to `/saved`

---

## UI Behaviour

### Heart icon on listing cards (`/listings`)
- Position: top-left corner of the listing image
- Visible: on image hover (CSS hover) + always visible when already liked
- Not liked: translucent dark circle with a ghost heart icon
- Liked: solid red circle with filled red heart
- Unauthenticated: icon hidden entirely
- Click: fires `ToggleLike`, optimistic update

### `/saved` hub page

**Layout:** Vertical dense list. Each row contains:
- Thumbnail (left)
- Title · Price / City · Mileage · Fuel · Transmission (middle)
- Note area (middle, below specs, full available width)
- ♥ unlike button (right, clicking removes the row)

**Row states:**

1. **Collapsed (default):** Note snippet truncated to one line, full available width. No note → nothing shown.
2. **Hovered:** Row and note container expand vertically to show the full note text. Note shown with red left-border accent. No note → faint "Click to add a note…" placeholder appears.
3. **Editing (clicked note text):** Text becomes directly editable in place. Blinking cursor visible. A `+` button appears inline near the cursor.
4. **Property picker open:** Clicking `+` opens a dropdown listing the listing's fields with their current live values (Mileage, Price, Engine, Transmission, Fuel, City).
5. **Chip inserted:** Selecting a field inserts a styled inline chip, e.g. `📌 Mileage · 85,000 km`. Stored as literal text for now.
6. **Save:** Auto-saves on blur (`useUpsertListingNote`). If text is empty on blur → fires `useDeleteListingNote`.

**Empty state:** Friendly message — "You haven't saved any listings yet." with a link to `/listings`.

**Property picker (UI-only for this iteration):**
- Renders a list of the listing's known fields with current values fetched from the already-loaded row data
- Selecting a field inserts formatted text; no structured backend binding
- Schema note: a `PropertyNotes` JSON column can be added later without breaking the existing `Content` field

---

## Edge Cases

| Scenario | Behaviour |
|---|---|
| Unlike from hub | Row removed optimistically; note deleted server-side |
| Clear note text + blur | Note record deleted; no empty string stored |
| Like from `/listings` while already on `/saved` | Hub query invalidated and refetched |
| Unauthenticated visit to `/saved` | Redirect to login |
| Listing is deleted by seller | Filtered out from `GetSavedListings` results silently (inner join excludes orphaned likes) |
| Network error on auto-save | Show subtle error indicator on the row; retain local text |

---

## Testing Notes

- Handler tests: ToggleLike (add + remove + cascade note delete), GetSavedListings (with/without note), UpsertNote (create + update), DeleteNote
- Frontend: optimistic like toggle on card, row expand/collapse interaction, auto-save on blur, empty-text → delete behaviour
