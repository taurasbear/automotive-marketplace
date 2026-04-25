# My Listings Redesign & Seller Interaction Dashboard

Full redesign of the My Listings page: listing cards now match the search result card layout, and sellers can see who liked or messaged their listings with a quick-access chat panel.

## Decisions

- **Card layout:** Matches `ListingCard` from the search results page (ImageHoverGallery, spec badge grid, mileage/price)
- **Seller controls:** Status badge on image, Edit/Delete in bottom row, defect count badge
- **Interaction stats:** Like count + conversation count visible on each card; expandable buyer panel per card
- **Buyer panel tabs:** Conversations tab (people who messaged/offered/proposed meetings) and Likes tab (people who only liked)
- **Likes are named:** Seller sees who liked their listing and can start a chat with them
- **Chat opens:** Slide-over `ChatPanel` at `MyListingsPage` level — same pattern as listing details page
- **Sold listings:** Same card layout, 50% opacity, no Edit/Delete, no buyer panel
- **Backend:** Extend `GetMyListings` with images/counts; new `GetListingEngagements` endpoint lazy-loaded on expand

## Backend

### GetMyListingsResponse Extensions

Add three fields to `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs`:

```csharp
public IEnumerable<ImageDto> Images { get; set; } = [];
public int LikeCount { get; set; }
public int ConversationCount { get; set; }
```

Update `GetMyListingsQueryHandler.cs`:
- Include non-defect images (same `.Where(i => i.ListingDefectId == null)` filter used in GetListingById)
- Count `listing.Likes.Count` (requires `.Include(l => l.Likes)`)
- Count `listing.Conversations.Count` (requires `.Include(l => l.Conversations)`)

### GetListingEngagements Query

New CQRS query handler at `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/`.

**Request:** `GetListingEngagementsQuery { Guid ListingId, Guid CurrentUserId }`

**Authorization:** Handler checks `listing.SellerId == CurrentUserId`. Throws `UnauthorizedAccessException` if not.

**Response types:**

```csharp
// GetListingEngagementsResponse.cs
public sealed record GetListingEngagementsResponse
{
    public IEnumerable<ListingConversationEngagement> Conversations { get; set; } = [];
    public IEnumerable<ListingLikerEngagement> Likers { get; set; } = [];
}

// ListingConversationEngagement.cs
public sealed record ListingConversationEngagement
{
    public Guid ConversationId { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public string LastMessageType { get; set; } = string.Empty; // "Text" | "Offer" | "Meeting" | "Availability"
    public DateTime LastInteractionAt { get; set; }
}

// ListingLikerEngagement.cs
public sealed record ListingLikerEngagement
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime LikedAt { get; set; }
}
```

**Handler logic:**
1. Load listing with includes: `Conversations` (with `Messages` and `Buyer`), `Likes` (with `User`)
2. Map conversations: take the last message per conversation for type; sort by `LastMessageAt` descending
3. Map likers: users in `Likes` who do NOT have a conversation (pure likers); sort by `CreatedAt` descending
4. Return combined response

**Controller endpoint:**

Add to `ListingController.cs` (or new `ListingEngagementsController.cs`):
```csharp
[HttpGet("{id}/engagements")]
[Authorize]
public async Task<IActionResult> GetEngagements(Guid id)
```

Injects current user ID from JWT claims.

## Frontend

### Type Updates

Update `GetMyListingsResponse` TypeScript type (in `src/features/myListings/types/` or wherever it lives):

```typescript
export type GetMyListingsResponse = {
  id: string;
  price: number;
  mileage: number;
  isUsed: boolean;
  city: string;
  status: string;
  year: number;
  makeName: string;
  modelName: string;
  thumbnail: { url: string; altText: string } | null;
  images: { url: string; altText: string }[];  // NEW
  imageCount: number;
  defectCount: number;
  fuelName: string;
  transmissionName: string;
  engineSizeMl: number;
  powerKw: number;
  likeCount: number;        // NEW
  conversationCount: number; // NEW
};
```

New API types for engagements:

```typescript
// src/features/myListings/types/GetListingEngagementsResponse.ts
export type ListingConversationEngagement = {
  conversationId: string;
  buyerId: string;
  buyerUsername: string;
  lastMessageType: "Text" | "Offer" | "Meeting" | "Availability";
  lastInteractionAt: string;
};

export type ListingLikerEngagement = {
  userId: string;
  username: string;
  likedAt: string;
};

export type GetListingEngagementsResponse = {
  conversations: ListingConversationEngagement[];
  likers: ListingLikerEngagement[];
};
```

### New API Hook

`src/features/myListings/api/getListingEngagementsOptions.ts` — `queryOptions` for `GET /api/listings/{id}/engagements`.

### File Structure

```
src/features/myListings/
├── api/
│   ├── getMyListingsOptions.ts        (existing)
│   ├── useDeleteMyListing.ts          (existing)
│   └── getListingEngagementsOptions.ts  (NEW)
├── components/
│   ├── MyListingsPage.tsx             (modified)
│   ├── MyListingCard.tsx              (full rewrite)
│   └── ListingBuyerPanel.tsx          (NEW)
├── types/
│   ├── GetMyListingsResponse.ts       (modified)
│   └── GetListingEngagementsResponse.ts (NEW)
└── index.ts
```

### MyListingCard Component

Full rewrite. Layout mirrors `ListingCard` from `src/features/listingList/components/ListingCard.tsx`:

**Left panel (~40% width):**
- `ImageHoverGallery` with `listing.images` (zone-based hover cycling)
- Overlay top-left: Status badge (`Active` = green, `Sold` = gray, other = yellow)
- Overlay top-right: image count (if > 1)

**Right panel (~60% width), vertical flex:**
- `Used` / `New` label (small, muted)
- Title: `{year} {makeName} {modelName}` (large, semibold)
- Mileage: `{formatNumber(mileage)} km` (small, muted)
- Price: `{formatCurrency(price)} €` (large, bold)
- 2×2 spec badge grid:
  - Engine + power (PiEngine icon)
  - Fuel name (MdOutlineLocalGasStation icon)
  - Transmission (TbManualGearbox icon)
  - City (IoLocationOutline icon)
- Bottom row:
  - Defect badge (AlertTriangle icon, only if `defectCount > 0`)
  - Spacer
  - Interaction stats pill: Heart icon · `{likeCount}`, MessageSquare icon · `{conversationCount}`, `{t("buyerPanel.buyers")}` ChevronDown icon — toggles buyer panel
  - Edit button (navigates to `/my-listings/{id}`) — only if not Sold
  - Delete button with confirmation dialog — only if not Sold

**Sold listings:** `opacity-50` wrapper, no Edit/Delete, buyer panel toggle hidden.

**Props:**
```typescript
type MyListingCardProps = {
  listing: GetMyListingsResponse;
  onStartChat: (conversation: ConversationSummary) => void;
};
```

### ListingBuyerPanel Component

`src/features/myListings/components/ListingBuyerPanel.tsx`

**Props:**
```typescript
type ListingBuyerPanelProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingThumbnail: { url: string; altText: string } | null;
  sellerId: string;
  onStartChat: (conversation: ConversationSummary) => void;
};
```

**Behavior:**
- Uses `useQuery(getListingEngagementsOptions(listingId))` — query fires the first time the panel renders
- Renders shadcn `Tabs` with two tabs:
  - `Conversations ({conversations.length})` — up to 5 shown; if more: `+ {n} {t("buyerPanel.moreConversations")} →` link navigates to `/inbox`
  - `Likes ({likers.length})` — up to 5 shown; same overflow pattern
- Each conversation row: avatar initials circle (colored by hash of userId) + username + interaction type badge (see icons below) + time-ago string + `Chat` button
- Each liker row: avatar + username + Heart icon + time-ago + `Chat` button
- `Chat` button: calls `useGetOrCreateConversation({ listingId })`, then constructs a `ConversationSummary` and calls `onStartChat`

**Interaction type icons (Lucide):**
- `"Offer"` → DollarSign icon, red badge
- `"Meeting"` → Calendar icon, green badge
- `"Availability"` → Clock icon, blue badge
- `"Text"` → MessageSquare icon, gray badge
- liked → Heart icon, amber badge

**Loading state:** Skeleton rows (2 per tab) while query is pending.

### MyListingsPage Changes

- Add `const [activeChatConversation, setActiveChatConversation] = useState<ConversationSummary | null>(null)`
- Pass `onStartChat={setActiveChatConversation}` to each `MyListingCard`
- Render `ChatPanel` from `src/features/chat` at page level when `activeChatConversation !== null`
  - Same pattern used in `ListingDetailsContent`
  - `ChatPanel` prop `onClose={() => setActiveChatConversation(null)}`

## i18n

Add to `en/myListings.json` and `lt/myListings.json` under `"buyerPanel"` key:

**English:**
```json
{
  "buyerPanel": {
    "buyers": "Buyers",
    "conversations": "Conversations",
    "likes": "Likes",
    "moreConversations": "more conversations",
    "moreLikes": "more likes",
    "chat": "Chat",
    "interactionTypes": {
      "offer": "Price offer",
      "meeting": "Meeting proposal",
      "availability": "Availability",
      "message": "Message",
      "liked": "Liked"
    }
  }
}
```

**Lithuanian:**
```json
{
  "buyerPanel": {
    "buyers": "Pirkėjai",
    "conversations": "Pokalbiai",
    "likes": "Patiktukai",
    "moreConversations": "daugiau pokalbių",
    "moreLikes": "daugiau patiktukų",
    "chat": "Rašyti",
    "interactionTypes": {
      "offer": "Kainos pasiūlymas",
      "meeting": "Susitikimo pasiūlymas",
      "availability": "Laisvas laikas",
      "message": "Žinutė",
      "liked": "Patiko"
    }
  }
}
```

## Files Affected

### New Backend Files
- `Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQuery.cs`
- `Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs`
- `Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsResponse.cs`
- `Application/Features/ListingFeatures/GetListingEngagements/ListingConversationEngagement.cs`
- `Application/Features/ListingFeatures/GetListingEngagements/ListingLikerEngagement.cs`

### Modified Backend Files
- `Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs` — add Images, LikeCount, ConversationCount
- `Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs` — include images/counts
- `Server/Controllers/ListingController.cs` — add `GET /{id}/engagements` endpoint

### New Frontend Files
- `src/features/myListings/api/getListingEngagementsOptions.ts`
- `src/features/myListings/components/ListingBuyerPanel.tsx`
- `src/features/myListings/types/GetListingEngagementsResponse.ts`

### Modified Frontend Files
- `src/features/myListings/components/MyListingCard.tsx` — full rewrite
- `src/features/myListings/components/MyListingsPage.tsx` — add chat state + ChatPanel
- `src/features/myListings/types/GetMyListingsResponse.ts` — add images/likeCount/conversationCount
- `src/lib/i18n/locales/en/myListings.json` — add buyerPanel keys
- `src/lib/i18n/locales/lt/myListings.json` — add buyerPanel keys

## What NOT to Change

- `ListingCard.tsx` (search results) — untouched
- Existing chat infrastructure — reused as-is
- `MyListingDetail.tsx` (inline edit page) — untouched
- Inbox/conversation routing — the "+ N more" link navigates to the root Inbox; full conversation filtering is a future enhancement
