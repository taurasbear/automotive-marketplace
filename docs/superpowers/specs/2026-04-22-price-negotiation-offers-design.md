# Price Negotiation (Offers) Feature — Design Spec

**Date:** 2026-04-22  
**Status:** Approved

---

## Problem Statement

Buyers and sellers need a structured way to negotiate price within the chat. Currently, the `ActionBar` has a placeholder "Make an Offer" button that does nothing. This feature replaces that placeholder with a full offer negotiation flow, similar to Vinted, where offers appear as cards inside the existing message thread.

---

## Feature Overview

- The **buyer** can make a price offer to the seller at any time (one active offer per conversation at a time).
- The **seller** can make the first offer only if the buyer has already **liked** the listing (`UserListingLike` exists).
- The **receiving party** (buyer or seller) can **Accept**, **Decline**, or **Counter** any pending offer.
- A **counter-offer** flips the responding role — the original sender now receives and must respond.
- All offers appear as **cards** in the chat thread, in chronological order alongside text messages.
- Accepting an offer immediately sets the listing `Status` to `OnHold`.
- Offers **expire automatically after 48 hours** of inactivity.
- Both parties receive **real-time notifications** via SignalR for every offer event.

---

## Constraints & Validation

| Rule | Detail |
|---|---|
| One active offer per conversation | If `Status = Pending` exists, no new offer can be made (counter-offers replace, they don't stack) |
| Minimum offer amount | Offer must be ≥ 1/3 of the listing's current asking price |
| Maximum offer amount | Offer must be ≤ the listing's current asking price |
| Seller-initiates guard | Seller can only make the first offer if a `UserListingLike` record exists for the buyer |
| Listing must be available | Offers can only be made on listings with `Status = Available` |
| Expiry | Pending offers auto-expire after 48 hours via background service |

---

## Data Model

### New Enum: `OfferStatus`

```csharp
public enum OfferStatus
{
    Pending,
    Accepted,
    Declined,
    Countered,
    Expired
}
```

### New Entity: `Offer`

```csharp
public class Offer : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid InitiatorId { get; set; }       // who sent this offer
    public decimal Amount { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }     // CreatedAt + 48h
    public Guid? ParentOfferId { get; set; }    // null for first offer; set for counter-offers

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User Initiator { get; set; } = null!;
    public virtual Offer? ParentOffer { get; set; }
    public virtual ICollection<Offer> CounterOffers { get; set; } = [];
    public virtual Message? Message { get; set; }
}
```

### Updated Entity: `Message`

Two new columns are added to `Message`:

```csharp
public MessageType MessageType { get; set; }   // defaults to Text
public Guid? OfferId { get; set; }             // nullable FK → Offer (only set when MessageType = Offer)
public virtual Offer? Offer { get; set; }
```

### New Enum: `MessageType`

```csharp
public enum MessageType
{
    Text,
    Offer
}
```

**Rationale for linking Offer to Message:** Every offer action (make, counter) creates both an `Offer` record and a `Message` record of type `Offer`. This ensures offers appear in the correct chronological position within the message feed, reusing the existing ordering and streaming infrastructure without a separate merge/sort on the frontend.

---

## Backend: CQRS Features

### `MakeOfferCommand` / `MakeOfferCommandHandler`

**Input:** `ConversationId`, `InitiatorId`, `Amount`

**Handler logic:**
1. Load conversation with listing (include `Listing.LikeUsers`, `Listing.Seller`)
2. Determine the recipient (the other party in the conversation)
3. If initiator is the seller: verify a `UserListingLike` exists for the buyer; throw `UnauthorizedException` if not
4. Verify listing `Status == Available`; throw `BadRequestException` if not
5. Verify no `Pending` offer exists for the conversation; throw `BadRequestException` if one does
6. Validate `Amount >= listing.Price / 3` and `Amount <= listing.Price`; throw `ValidationException` if invalid
7. Create `Offer` entity (status=Pending, expiresAt=UtcNow+48h)
8. Create `Message` entity (type=Offer, offerId=offer.Id, content="", sentAt=UtcNow)
9. Update `conversation.LastMessageAt`
10. Persist via repository
11. Return `MakeOfferResponse` with full offer + message data

**Response:** Includes `OfferId`, `Amount`, `ExpiresAt`, `InitiatorId`, `RecipientId`, `PercentageOff`, `MessageId`, plus conversation metadata for SignalR broadcast.

---

### `RespondToOfferCommand` / `RespondToOfferCommandHandler`

**Input:** `OfferId`, `ResponderId`, `Action` (Accept | Decline | Counter), `CounterAmount?`

**Handler logic:**
1. Load offer with conversation + listing
2. Verify `Status == Pending`; throw if not (offer already resolved)
3. Verify `ResponderId != offer.InitiatorId` (can't respond to your own offer); throw `UnauthorizedException`
4. Verify `ExpiresAt > UtcNow`; throw `BadRequestException` if expired (the expiry service may not have run yet)

**If Accept:**
- Set offer `Status = Accepted`
- Set `listing.Status = OnHold`
- Persist both

**If Decline:**
- Set offer `Status = Declined`
- Persist

**If Counter:**
- Validate `CounterAmount` (same min/max rules as making an offer)
- Set current offer `Status = Countered`
- Create new `Offer` (parentOfferId = current offer.Id, status=Pending, expiresAt=UtcNow+48h)
- Create new `Message` (type=Offer, offerId=newOffer.Id)
- Update `conversation.LastMessageAt`
- Persist all

5. Return `RespondToOfferResponse` with updated offer, new offer if counter, and recipient info for SignalR

---

### Background Service: `OfferExpiryService`

A `BackgroundService` registered in Infrastructure that runs every 15 minutes:

1. Query all `Offer` records where `Status == Pending` and `ExpiresAt < UtcNow`
2. For each: set `Status = Expired`
3. Persist batch update
4. Push `OfferExpired` SignalR event to both initiator and recipient user groups

---

## Backend: ChatHub Extensions

Two new methods added to `ChatHub`:

```csharp
public async Task MakeOffer(Guid conversationId, decimal amount)
// → mediates MakeOfferCommand, then broadcasts OfferMade to both user groups

public async Task RespondToOffer(Guid offerId, string action, decimal? counterAmount)
// → mediates RespondToOfferCommand, then broadcasts the appropriate event to both user groups
```

**New client-side events** (pushed to both conversation participants):

| Event | Payload | Trigger |
|---|---|---|
| `OfferMade` | Full offer + message data | New offer created |
| `OfferAccepted` | Offer data + listing status | Offer accepted |
| `OfferDeclined` | Offer data | Offer declined |
| `OfferCountered` | Original offer update + new counter offer + message | Counter-offer sent |
| `OfferExpired` | Offer data | Background service expiry |

---

## Frontend

### New Types

```typescript
type OfferStatus = 'Pending' | 'Accepted' | 'Declined' | 'Countered' | 'Expired';

type Offer = {
  id: string;
  conversationId: string;
  initiatorId: string;
  amount: number;
  listingPrice: number;          // for % calculation
  percentageOff: number;
  status: OfferStatus;
  expiresAt: string;
  parentOfferId?: string;
};
```

`Message` type extended:

```typescript
type Message = {
  // ...existing fields
  messageType: 'Text' | 'Offer';
  offer?: Offer;                 // present when messageType === 'Offer'
};
```

### New Components

**`OfferCard`** (`features/chat/components/OfferCard.tsx`)

- Receives `offer`, `currentUserId`, `conversationBuyerId`, `onAccept`, `onDecline`, `onCounter` callbacks
- Renders the dark-header card with color-coded status (dark=pending, green=accepted, red=declined, purple=countered, grey=expired)
- Shows amount, strikethrough original price, percentage off badge using Lucide icons (not emojis)
- Renders Accept / Counter / Decline action buttons **only** when `status === 'Pending'` AND `currentUserId !== offer.initiatorId`
- Uses the project's existing Tailwind theme and shadcn/ui `Button` components

**`MakeOfferModal`** (`features/chat/components/MakeOfferModal.tsx`)

- Dialog (shadcn/ui `Dialog`) with a price input
- Live preview: shows the % discount as the user types
- Validation: disables submit if amount < listingPrice/3 or amount > listingPrice
- On submit: calls `sendOffer` from `useChatHub`

**`MakeOfferModal`** also handles the counter-offer case via a `mode: 'offer' | 'counter'` prop and an optional `initialAmount` prop. When `mode === 'counter'`, the heading reads "Counter Offer" and the input is pre-populated. No separate component is needed.

### Updated Components

**`MessageThread`**

- For each message, if `m.messageType === 'Offer'` and `m.offer` exists, render `<OfferCard>` instead of the text bubble div
- Pass callbacks that call `respondToOffer` from `useChatHub`

**`ActionBar`**

- Receives `listingPrice`, `currentUserId`, `sellerId`, `buyerId`, `buyerHasLiked`, `hasActiveOffer` as props (passed down from `MessageThread` via `conversation`)
- "Make an Offer" button:
  - **Buyer:** always shown; disabled if `hasActiveOffer === true`
  - **Seller:** shown only if `buyerHasLiked === true`; disabled if `hasActiveOffer === true`
  - On click: opens `MakeOfferModal`

`hasActiveOffer` is derived on the frontend from the loaded messages (any message with `messageType === 'Offer'` and `offer.status === 'Pending'`). No extra API field needed.

`buyerHasLiked` must come from the API. The `GetConversations` query response (`ConversationSummary`) and the `GetMessages` response both need a `buyerHasLiked: boolean` field added (populated by checking `UserListingLike` for the conversation's `BuyerId`).

### Updated API / SignalR hooks

**`useChatHub`** extended with:
- `sendOffer(conversationId, amount)` — invokes hub `MakeOffer`
- `respondToOffer(offerId, action, counterAmount?)` — invokes hub `RespondToOffer`
- Handles incoming events: `OfferMade`, `OfferAccepted`, `OfferDeclined`, `OfferCountered`, `OfferExpired`
  - Each event upserts the relevant message/offer in the TanStack Query messages cache via `queryClient.setQueryData`

---

## Notification Strategy

All offer events are delivered **only via SignalR** (same pattern as messages). There are no REST polling endpoints for offer state — the client keeps its cache updated via the real-time events. The `getMessages` query data is the source of truth for the rendered thread.

---

## Migration

A new EF Core migration adds:
1. `Offers` table (all `Offer` fields)
2. `MessageType` column on `Messages` (default 0 = Text)
3. `OfferId` nullable FK column on `Messages` with foreign key constraint to `Offers`

---

## Out of Scope

- Email or push notification delivery (in-app SignalR only)
- Offer history/analytics screen
- Seller setting a minimum acceptable price on the listing
- Automatic purchase flow after offer acceptance (listing stays OnHold; sale is finalised outside the app)
