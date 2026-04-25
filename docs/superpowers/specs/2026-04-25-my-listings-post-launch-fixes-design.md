# My Listings Post-Launch Fixes — Design Spec

**Date:** 2026-04-25  
**Status:** Approved

## Overview

Seven targeted fixes to the My Listings page and related features following the initial launch of the seller interaction dashboard. Issues span backend authorization, frontend UI consistency, UX clarity, and chat panel improvements.

---

## Fix 1 — 403 Unauthorized on GetMyListings and GetEngagements

**Problem:** The `GetMy` and `GetEngagements` controller endpoints require `Permission.ManageListings`. Regular sellers only have `Permission.CreateListings`, so they get a 403 when trying to view their own listings.

**Fix:** Change the `[Protect]` attribute on both endpoints to accept either permission:
```csharp
[Protect(Permission.CreateListings, Permission.ManageListings)]
```

The `Protect` attribute already supports multiple permissions (allow if user has any one of them). This applies to `GetMy` and `GetEngagements` in `ListingController`.

---

## Fix 2 — MyListingCard Spec Badge Gaps / Card Size

**Problem:** The `MyListingCard` spec badge grid has `gap-y-4` (16px vertical gap) which, combined with the taller bottom row (Edit + Delete + buyer toggle buttons), causes the card to appear significantly larger than the `ListingCard` in search results.

**Fix:**
- Change badge grid from `gap-y-4` → `gap-y-2` in `MyListingCard` to tighten up the space between engine/fuel/transmission/location pills.
- Add `justify-items-stretched` to the badge grid class (matching `ListingCard`).

These two changes bring the visual appearance close to parity with the search results card.

---

## Fix 3 — Empty Conversations Counted and Shown

**Problem:** When a buyer clicks "Contact Seller" on the listing detail page, `GetOrCreateConversation` immediately creates a `Conversation` entity. If the buyer closes the chat panel without sending a message, the seller sees `ConversationCount: 1` on their card and a conversation entry in the buyer panel with no messages and a misleading "Text" badge.

**Fix (filter approach):** Only count and show conversations with at least one message.

- **`GetMyListingsQueryHandler`:** Replace the existing `.Include(l => l.Conversations)` with a filtered include: `.Include(l => l.Conversations.Where(c => c.Messages.Any()))`. EF Core filtered includes load only matching child rows. After this, `listing.Conversations.Count` gives the count of conversations with ≥1 message without loading message content. No messages are loaded — only the Conversation rows that satisfy the condition.
  
- **`GetListingEngagementsQueryHandler`:** Add `.Where(c => c.Messages.Any())` to the conversations LINQ query (before `ToListAsync`) so the DB returns only conversations with at least one message. The handler already loads `.Include(c => c.Messages)` so the `Any()` condition is evaluated in SQL.

Both changes keep the backend consistent: zero-message conversations are created but treated as invisible to the seller until the buyer actually sends something.

---

## Fix 4 — Seller Cannot Send Offers/Meetings to Likers

**Problem:** In `ListingBuyerPanel.handleOpenLikerChat`, the `ConversationSummary` passed to `onStartChat` sets `buyerHasEngaged: false`. The `ActionBar` hides the `+` button when `isSeller && !buyerHasEngaged`, so the seller cannot send offers or propose meetings to a buyer who only liked the listing.

**Fix:** Set `buyerHasEngaged: true` in `handleOpenLikerChat`. A user who liked a listing has expressed interest — this qualifies as engagement and the seller should be able to initiate offers and meeting proposals.

---

## Fix 5 — ChatPanel Shows No Listing Context

**Problem:** The `ChatPanel` slide-over header shows only `conversation.counterpartUsername`, giving the seller no indication of which listing the conversation is about.

**Fix:** Update `ChatPanel` to display the listing title as a secondary line below the counterpart username. Use a muted/small text style (e.g., `text-xs text-muted-foreground`) so it doesn't compete with the name. The `conversation.listingTitle` field is already available in `ConversationSummary`.

Example header layout:
```
Alex Johnson              [✕]
2019 Toyota Camry
```

---

## Fix 6 — Like/Message Count Mismatch Between Card and Panel

**Problem:** The card shows `likeCount` (all users who liked, including those who also have a conversation) and `conversationCount`. But the "Likes" tab in the buyer panel shows only "pure likers" (liked, no conversation). So when someone both liked and messaged, the card says "1 like, 1 message" but the panel shows "1 conversation, 0 likes."

**Decision:** Keep the current backend counts unchanged (they are semantically correct). Rename the "Likes" tab in `ListingBuyerPanel` to **"Liked only"** to make it clear this tab shows buyers who liked but have not messaged. The tab counts will still differ from the card badge counts when a buyer has done both, but the label eliminates confusion about what the tab contains.

Tab labels:
- `Conversations (N)` — unchanged  
- `Liked only (N)` — was `Likes (N)`

---

## Fix 7 — Pending Changes Not Visible Before Save

**Problem:** In `MyListingDetail`, when a user edits a field and closes the inline editor, the `EditableField` display shows the original server value — there is no indication that the field has a pending unsaved change. The user has no way to see what the new value will be until they click "Save Changes."

**Fix:** 
- Add a `pendingValue?: string | number | boolean` prop to `EditableField`.
- When `pendingValue` is provided and differs from `value` (the server value), the display mode shows:
  - The original value with strikethrough in muted style.
  - The new pending value in amber/orange color (e.g., `text-amber-600`).
- In `MyListingDetail`, pass `pendingValue={pendingChanges[fieldKey]}` for each `EditableField`. When no pending change exists for that field, `pendingValue` is `undefined` and the component behaves as before.

Visual example for "Price" field with a pending change:
```
Price
~~15,000~~  13,500  [✏]
           ↑ amber text, no strikethrough
```
The `pendingValue` replaces the strikethrough original to its right, both on the same line in display mode. When there is no pending change, only the current value shows normally.

---

## Scope

| # | Area | Files Changed |
|---|------|---------------|
| 1 | BE – Controller | `ListingController.cs` |
| 2 | FE – Card | `MyListingCard.tsx` |
| 3 | BE – Handlers | `GetMyListingsQueryHandler.cs`, `GetListingEngagementsQueryHandler.cs` |
| 4 | FE – Panel | `ListingBuyerPanel.tsx` |
| 5 | FE – ChatPanel | `ChatPanel.tsx` |
| 6 | FE – Panel | `ListingBuyerPanel.tsx` |
| 7 | FE – EditableField | `EditableField.tsx`, `MyListingDetail.tsx` |

## Out of Scope

- Redesigning the "Contact Seller" flow to be truly lazy (conversation created on first message send) — deferred.
- Any changes to the inbox page or full conversation view.
- Removing the `ConversationCount` card badge from including zero-message conversations (the filtering in fix 3 handles this).
