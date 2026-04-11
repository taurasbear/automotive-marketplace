# Chat Feature Design Spec
**Date:** 2026-04-11  
**Status:** Approved (pending implementation)

---

## Problem & Goal

Buyers browsing listings have no way to contact sellers without leaving the platform. This feature adds real-time buyer↔seller messaging tied to individual listings, using SignalR (WebSockets) for live delivery and PostgreSQL for persistent history.

The `Conversation` entity is designed as a first-class domain object to serve as the foundation for future listing-scoped features: price bidding, meet-up scheduling, and contract generation.

---

## Decisions Made

| Decision | Choice | Rationale |
|---|---|---|
| Real-time transport | SignalR | Idiomatic for .NET 8; `Hubs/` folder already present; handles WS fallbacks + reconnection |
| Chat scope | Per `(buyer, listing)` pair | One conversation per buyer per listing |
| Message persistence | PostgreSQL via EF Core | Full history; users can return to past conversations |
| Who can initiate | Any authenticated user who is not the listing's seller | "Contact Seller" button hidden from the listing owner; backend validates too |
| Unread badge | Yes — real-time count in nav | SignalR pushes count on each new message |
| UI placement | Panel + Inbox Page | Slide-in panel on listing page; `/inbox` for full conversation management |
| Inbox layout | Listing-card thread | Car thumbnail in rows; pinned listing card at top of thread |
| Future actions | Sub-header action bar | "Make an Offer" always visible; "More ▾" dropdown for scheduling/contracts |
| SellerId in Conversation | No — derived via JOIN | One extra JOIN to `Listing` is cheap; not worth the denormalisation complexity |
| Message pagination | Not in v1 | Conversations will be short; `GetMessagesQuery` can accept `skip`/`take` later with no entity changes |

---

## Domain Entities

### `Conversation`
```
Id            Guid (PK)
ListingId     Guid (FK → Listing)
BuyerId       Guid (FK → User)
CreatedAt     DateTime
LastMessageAt DateTime    ← updated on each new message; used for ordering
```
Unique constraint: `(BuyerId, ListingId)` — one conversation per buyer per listing.

SellerId is not stored — it is always `Listing.SellerId` and is retrieved via JOIN when needed.

### `Message`
```
Id             Guid (PK)
ConversationId Guid (FK → Conversation)
SenderId       Guid (FK → User)
Content        string
SentAt         DateTime
IsRead         bool (default: false)
```

---

## Application Layer (CQRS)

| Handler | Type | Description |
|---|---|---|
| `GetOrCreateConversationCommand` | Command | Idempotent — finds existing or creates new conversation for `(buyerId, listingId)`. Returns `ConversationId`. Throws `UnprocessableEntityException` if the caller is the listing's seller. |
| `SendMessageCommand` | Command | Persists message, updates `Conversation.LastMessageAt`, triggers SignalR broadcast. |
| `GetConversationsQuery` | Query | Returns all conversations for the current user (as buyer or as seller via `Listing.SellerId`), ordered by `LastMessageAt` desc. Includes listing thumbnail, last message preview, and unread count. |
| `GetMessagesQuery` | Query | Returns all messages for a conversation. Validates caller is a participant. Accepts optional `skip`/`take` for future pagination — ignored in v1. |
| `MarkMessagesReadCommand` | Command | Marks all unread messages in a conversation as read for the calling user (i.e. messages sent by the other party). Pushes updated total unread count via SignalR. |

---

## Server Layer

### `ChatHub` — `/hubs/chat`

**Authentication:** JWT token passed via query string `?access_token=...` (required for WebSocket handshake). The existing `JwtBearer` options in `Program.cs` need an `OnMessageReceived` event to read the token from the query string for SignalR paths — standard ASP.NET Core pattern.

| Direction | Method | Description |
|---|---|---|
| Client → Server | `SendMessage(conversationId, content)` | Hub method; persists + broadcasts |
| Server → Client | `ReceiveMessage(message)` | Pushed to both participants in the conversation |
| Server → Client | `UpdateUnreadCount(count)` | Pushed to the recipient's personal group with their new total unread count |

On `OnConnectedAsync`: user joins group `user-{userId}` so the server can target them by identity regardless of connection.

### `ChatController` — REST endpoints

| Method | Route | Handler | Auth |
|---|---|---|---|
| `POST` | `/api/Chat/GetOrCreateConversation` | `GetOrCreateConversationCommand` | Required |
| `GET` | `/api/Chat/GetConversations` | `GetConversationsQuery` | Required |
| `GET` | `/api/Chat/GetMessages` | `GetMessagesQuery` | Required |
| `PUT` | `/api/Chat/MarkMessagesRead` | `MarkMessagesReadCommand` | Required |

REST is used for initial data loading. SignalR is used for real-time events only.

---

## Frontend

### New Routes
- `/inbox` — full inbox page
- `/inbox/$conversationId` — inbox with a specific conversation pre-selected (TanStack Router param syntax)

### Shared Infrastructure Changes

**`src/constants/endpoints.ts`** — add `CHAT` entry:
```ts
CHAT: {
  GET_OR_CREATE_CONVERSATION: "/Chat/GetOrCreateConversation",
  GET_CONVERSATIONS: "/Chat/GetConversations",
  GET_MESSAGES: "/Chat/GetMessages",
  MARK_MESSAGES_READ: "/Chat/MarkMessagesRead",
}
```

**`src/api/queryKeys/chatKeys.ts`** — new file, same pattern as `listingKeys.ts`:
```ts
export const chatKeys = {
  all: () => ["chat"],
  conversations: () => [...chatKeys.all(), "conversations"],
  messages: (conversationId: string) => [...chatKeys.all(), "messages", conversationId],
};
```

### New Feature: `features/chat/`

```
features/chat/
  api/
    getConversationsOptions.ts       # queryOptions → GET /Chat/GetConversations
    getMessagesOptions.ts            # queryOptions → GET /Chat/GetMessages
    useGetOrCreateConversation.ts    # useMutation  → POST /Chat/GetOrCreateConversation
    useMarkMessagesRead.ts           # useMutation  → PUT /Chat/MarkMessagesRead
    useChatHub.ts                    # SignalR hub connection — manages lifecycle,
                                     # ReceiveMessage + UpdateUnreadCount callbacks
  components/
    ConversationList.tsx             # Left panel: rows with thumbnail + last message + unread badge
    MessageThread.tsx                # Right panel: message bubbles + ActionBar + input
    ListingCard.tsx                  # Pinned card at top of thread linking back to listing
    ChatPanel.tsx                    # Slide-in drawer — used by listingDetails feature
    ActionBar.tsx                    # Sub-header: "Make an Offer" + "More ▾" (all disabled in v1)
    UnreadBadge.tsx                  # Badge count for nav icon
  types/
    Conversation.ts
    ConversationSummary.ts           # Shape returned by GetConversationsQuery (with preview + unread)
    Message.ts
    GetOrCreateConversationCommand.ts
    GetConversationsQuery.ts
    GetMessagesQuery.ts
    SendMessageCommand.ts            # Shape sent via SignalR hub method
  pages/
    InboxPage.tsx                    # /inbox route — two-panel layout
  index.ts                           # Barrel: export ChatPanel, UnreadBadge, InboxPage
```

### Changes to `features/listingDetails/`

`ListingDetailsContent.tsx` is expanded (not extracted to a new feature) to:

1. Import `ChatPanel` from `features/chat/`
2. Add a "Contact Seller" button in the right-hand sidebar card — visible only when the logged-in user is **not** the listing's seller and the user is authenticated
3. "Contact Seller" calls `useGetOrCreateConversation` then opens `ChatPanel` with the returned `conversationId`

New types added to `features/listingDetails/types/`:
- `GetOrCreateConversationResult.ts` — response shape (just `conversationId`)

### SignalR Connection Lifecycle

`useChatHub` is initialised inside the **authenticated route layout component** (the wrapper that already guards all protected routes) so the connection is established once on login and torn down on logout — not per-page.

1. Authenticated user navigates to any protected route → `useChatHub` establishes connection to `/hubs/chat?access_token=...`
2. Hub adds user to group `user-{userId}`
3. On `ReceiveMessage`: updates the TanStack Query cache for the relevant conversation's messages; bumps unread count if that conversation is not currently open
4. On `UpdateUnreadCount`: updates the nav badge count (stored in component state or Redux — TBD at implementation)
5. On disconnect: SignalR client auto-reconnects with exponential backoff

---

## Inbox Page Layout (`/inbox`)

Two-panel layout:
- **Left panel:** `ConversationList` — each row shows car thumbnail, listing title, counterpart's username, last message preview, unread badge, relative timestamp
- **Right panel:** `MessageThread` — pinned `ListingCard` at top, message bubbles, `ActionBar` sub-header, message input

On mobile (responsive): single panel; conversation list is the default view; selecting a conversation navigates into the thread with a back button.

---

## Chat Panel Layout (Listing Detail Page)

Slide-in drawer from the right edge of the listing page:

```
┌─────────────────────────────────────────────┐
│ [Counterpart name]   [Corolla 2019 · €X  →] │  ← header
├─────────────────────────────────────────────┤
│ [Make an Offer]  │  [More ▾]                │  ← ActionBar (all disabled in v1)
├─────────────────────────────────────────────┤
│                                             │
│   message bubbles                           │  ← MessageThread (no ListingCard here —
│                                             │    already in context on listing page)
├─────────────────────────────────────────────┤
│ [ Message [name]...               ] [Send]  │  ← input
└─────────────────────────────────────────────┘
```

---

## Action Bar — Extensibility Plan

The `ActionBar` component is rendered in v1 with all buttons disabled. Future releases enable buttons independently — no UI restructuring needed:

| Button | Location | Enables in |
|---|---|---|
| Make an Offer | Always visible | v2 (price bidding) |
| Schedule Viewing | "More ▾" dropdown | v3 (meet-up planning) |
| Generate Contract | "More ▾" dropdown | v4 (contract generation) |

---

## Out of Scope (v1)

- Push notifications (browser/mobile)
- Message editing or deletion
- Per-message read receipts in the UI (`IsRead` is tracked in DB but not surfaced per-bubble)
- File/image attachments
- Seller-initiated conversations
- Price bidding, meet-up scheduling, contract generation (ActionBar placeholders only)
- Message pagination (query supports `skip`/`take` params; implementation loads all messages)
