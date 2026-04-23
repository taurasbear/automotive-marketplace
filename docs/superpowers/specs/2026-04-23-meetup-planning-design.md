# Meetup Planning Feature — Design Spec

**Date:** 2026-04-23  
**Status:** Approved

---

## Overview

Adds a meetup planning feature to the buyer–seller messaging interface. Either party can (1) propose a specific meeting time and location or (2) share a set of time slots they are free and let the other party pick one or respond with their own availability. Both flows live entirely inside the existing chat thread (listing details side panel and Inbox). Only one active meetup negotiation is allowed per conversation at a time.

---

## Goals

- Allow buyers and sellers to coordinate in-person meetups without leaving the platform.
- Two complementary flows: direct time proposal (negotiate like an offer) and availability sharing (asynchronous slot-based coordination).
- Real-time updates via the existing SignalR infrastructure.
- Consistent UX with the existing offer negotiation pattern.

---

## Non-Goals

- Full calendar integration (Google Calendar, iCal export) — future iteration.
- Full interactive map widget for location picking — text input + optional coordinates for v1; interactive map is a stretch goal.
- Push notifications outside the chat UI.

---

## Domain Model

### New Entities

#### `Meeting`

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK (from `BaseEntity`) |
| `ConversationId` | `Guid` | FK → `Conversation` |
| `InitiatorId` | `Guid` | FK → `User` |
| `ProposedAt` | `DateTime` | Meeting start time (UTC); must be in the future |
| `DurationMinutes` | `int` | Default 60; user-configurable |
| `LocationText` | `string?` | Free-text location description |
| `LocationLat` | `decimal?` | Optional latitude |
| `LocationLng` | `decimal?` | Optional longitude |
| `Status` | `MeetingStatus` | See enum below |
| `ParentMeetingId` | `Guid?` | FK → `Meeting` (self-referencing, for reschedule chains) |
| `ExpiresAt` | `DateTime` | 48 hours after creation |

Navigation properties: `Conversation`, `Initiator`, `ParentMeeting`, `CounterMeetings` (collection), `Message` (reverse nav from `Message`).

#### `AvailabilityCard`

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `ConversationId` | `Guid` | FK → `Conversation` |
| `InitiatorId` | `Guid` | FK → `User` |
| `Status` | `AvailabilityCardStatus` | See enum below |
| `ExpiresAt` | `DateTime` | 48 hours after creation |

Navigation properties: `Conversation`, `Initiator`, `Slots` (collection of `AvailabilitySlot`), `Message`.

#### `AvailabilitySlot`

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `AvailabilityCardId` | `Guid` | FK → `AvailabilityCard` |
| `StartTime` | `DateTime` | UTC; must be in the future at card creation time |
| `EndTime` | `DateTime` | UTC; must be after `StartTime` |

### New Enums

#### `MeetingStatus`
```
Pending    = 0  // awaiting response
Accepted   = 1
Declined   = 2
Rescheduled = 3  // superseded by a counter-proposal
Expired    = 4
```

#### `AvailabilityCardStatus`
```
Pending    = 0  // awaiting response
Responded  = 1  // other party picked a slot or sent their own availability
Expired    = 2
```

### Updates to Existing Types

**`MessageType` enum:** Add `Meeting = 2` and `Availability = 3`.

**`Message` entity:** Add nullable `MeetingId` (`Guid?`, FK → `Meeting`) and `AvailabilityCardId` (`Guid?`, FK → `AvailabilityCard`), parallel to the existing `OfferId`.

---

## Backend Architecture

### New CQRS Handlers (`Application/Features/ChatFeatures/`)

| Handler | Type | Description |
|---|---|---|
| `ProposeMeeting` | Command | Validates inputs, creates `Meeting` + `Message(Meeting)`, returns broadcast payload |
| `RespondToMeeting` | Command | Accept / Decline / Reschedule; on Reschedule creates a new `Meeting` with `ParentMeetingId` |
| `ShareAvailability` | Command | Creates `AvailabilityCard` + `AvailabilitySlot` records + `Message(Availability)` |
| `RespondToAvailability` | Command | `PickSlot`: creates a new `Meeting` proposal from the chosen slot, marks card as `Responded`; `ShareBack`: creates a new `AvailabilityCard`, marks old card as `Responded` |

**Validation rules enforced in handlers:**
- `ProposedAt` must be in the future (UTC).
- All availability `StartTime` values must be in the future; `EndTime > StartTime`.
- Only one active meetup negotiation allowed per conversation: check for any `Meeting` with `Status = Pending` or any `AvailabilityCard` with `Status = Pending`.
- Only the recipient (non-initiator) of a proposal can respond to it.

### SignalR Hub — `ChatHub` additions

**Client → Server methods:**

```
ProposeMeeting(conversationId, proposedAt, durationMinutes, locationText?, locationLat?, locationLng?)
RespondToMeeting(meetingId, action, rescheduleData?)
  // action: "Accept" | "Decline" | "Reschedule"
  // rescheduleData: { proposedAt, durationMinutes, locationText?, locationLat?, locationLng? }
ShareAvailability(conversationId, slots[{ startTime, endTime }])
RespondToAvailability(availabilityCardId, action, data?)
  // action: "PickSlot" | "ShareBack"
  // PickSlot data: { slotId }
  // ShareBack data: { slots[{ startTime, endTime }] }
```

**Server → Client events (broadcast to both parties):**

```
MeetingProposed      — payload: message + full meeting object
MeetingAccepted      — payload: meetingId, conversationId
MeetingDeclined      — payload: meetingId, conversationId
MeetingRescheduled   — payload: old meetingId, new meeting message object
MeetingExpired       — payload: meetingId, conversationId
AvailabilityShared   — payload: message + full availability card + slots
AvailabilityResponded — payload: availabilityCardId, action, (if PickSlot → new MeetingProposed payload; if ShareBack → new AvailabilityShared payload)
AvailabilityExpired  — payload: availabilityCardId, conversationId
```

### Background Service: `MeetingExpiryService`

Mirrors `OfferExpiryService`. Polls on a schedule, finds `Meeting` records with `Status = Pending` and `ExpiresAt ≤ now`, sets them to `Expired`, broadcasts `MeetingExpired` to both parties. Same for `AvailabilityCard` → `Expired` → `AvailabilityExpired`.

### EF Core Configurations

New `MeetingConfiguration`, `AvailabilityCardConfiguration`, `AvailabilitySlotConfiguration` files in `Infrastructure/Data/Configuration/`. New migration: `AddMeetupEntities`.

---

## Frontend Architecture

### New Types (`features/chat/types/`)

- `Meeting.ts` — mirrors `Offer.ts`; includes all meeting fields + status string union
- `AvailabilityCard.ts` — card metadata + `slots: AvailabilitySlot[]`
- `AvailabilitySlot.ts` — `{ id, startTime, endTime }`
- `MeetingEventPayloads.ts` — SignalR payload shapes for all meeting events (mirrors `OfferEventPayloads.ts`)

### Updated Types

- `Message.ts` — add `messageType: 'Text' | 'Offer' | 'Meeting' | 'Availability'`; add optional `meeting?: Meeting` and `availabilityCard?: AvailabilityCard`

### New Components (`features/chat/components/`)

#### `MeetingCard.tsx`
Stateful card rendered for `messageType === 'Meeting'`. Calendar-block style (day tile + time range). Status variants:

| Status | Header colour | Label | Actions |
|---|---|---|---|
| `Pending` | Blue (`#1e3a5f`) | "Meetup Proposed" | Accept / Reschedule / Decline (recipient only) |
| `Accepted` | Green | "Meetup Confirmed" | None |
| `Declined` | Red | "Meetup Declined" | None |
| `Rescheduled` | Violet | "Reschedule Proposed" | None (superseded card) |
| `Expired` | Muted | "Meetup Expired" | None |

Reschedule opens `ProposeMeetingModal` pre-populated with the current meeting's values.

#### `AvailabilityCard.tsx`
Rendered for `messageType === 'Availability'`. Purple header ("Availability Shared"). Body: list of slot rows, each showing the date, time range, and a "Propose →" button (recipient only; disabled if card is `Responded` or `Expired`). Below the list: "None of these work — share my availability instead" link (also recipient-only). Status variants: `Pending` (interactive), `Responded` (slots greyed out, label "Responded"), `Expired` (muted).

#### `ProposeMeetingModal.tsx`
Modal form with:
- Date picker
- Time picker (start time)
- Duration selector (minutes; default 60; common presets: 30, 60, 90, 120 min)
- Optional location text input
- Optional coordinates: a "Set pin" button that either uses the browser's geolocation API or allows manual lat/lng entry (map widget deferred to later iteration)

Used by both the "Propose a time" action and the "Reschedule" action on `MeetingCard`.

#### `ShareAvailabilityModal.tsx`
Modal form with a dynamic list of time slot entries. Each entry has a date picker, start time, and end time. A "+ Add another slot" button appends a new entry. Each slot has a trash icon to remove it. No hard limit on slot count. Submits when at least one valid slot is present.

### Updated Components

#### `ActionBar.tsx`
Add a "Plan Meetup 📅 ▾" dropdown button alongside "Make an Offer". Clicking it opens a popover with two options: "Propose a time 🗓️" and "Share availability ⏰". The button is:
- Shown under the same visibility conditions as "Make an Offer" (buyer always; seller if `buyerHasLiked`)
- Disabled (with tooltip) when `hasActiveMeeting` is true

`hasActiveMeeting` is derived in `MessageThread` identically to `hasActiveOffer`: `messages.some(m => (m.messageType === 'Meeting' && m.meeting?.status === 'Pending') || (m.messageType === 'Availability' && m.availabilityCard?.status === 'Pending'))`.

#### `MessageThread.tsx`
Add rendering branches for `'Meeting'` and `'Availability'` message types. Pass `hasActiveMeeting` down to `ActionBar`. Pass the new hub functions to the cards.

#### `useChatHub.ts`
Add invoke functions: `proposeMeeting`, `respondToMeeting`, `shareAvailability`, `respondToAvailability`. Add event listeners for all new server → client events, updating the TanStack Query cache using the same pattern as the offer events.

#### `chatHub.ts` (constants)
Add all new client → server and server → client method name constants.

---

## UX Flow Summary

### Flow 1: Direct Meeting Proposal

1. Either party clicks "Plan Meetup ▾" → "Propose a time".
2. `ProposeMeetingModal` opens. They fill in date, time, duration, optional location.
3. On submit: `ProposeMeeting` hub method → meeting card appears in thread for both.
4. Recipient sees the calendar-block card with Accept / Reschedule / Decline.
5. **Accept** → meeting confirmed (green card). **Decline** → meeting declined (red card). **Reschedule** → opens `ProposeMeetingModal` pre-filled → creates a new `Meeting` with `ParentMeetingId`; old card shows "Reschedule Proposed" (violet).
6. The new meeting goes back through the same cycle until accepted, declined, or expired.

### Flow 2: Availability Sharing

1. Either party clicks "Plan Meetup ▾" → "Share availability".
2. `ShareAvailabilityModal` opens. They add one or more time slots.
3. On submit: `ShareAvailability` hub method → availability card appears in thread.
4. Recipient sees the purple card with slot rows and "Propose →" per slot.
   - **Pick a slot ("Propose →"):** Creates a `Meeting` proposal (Flow 1 from step 4). Availability card becomes "Responded".
   - **"Share my availability instead":** Opens `ShareAvailabilityModal` → creates a new `AvailabilityCard`; old card becomes "Responded".
5. The picked-slot meeting then goes through the normal meeting negotiation cycle.

---

## Error Handling

- Validation errors (past date, no slots, active negotiation already exists) are thrown as domain exceptions, caught by existing exception filters, and surfaced in the UI via the same `sendError` state pattern used for messages.
- Expired cards/meetings update in real time via the background service → `MeetingExpired`/`AvailabilityExpired` events.

---

## Testing

Integration tests follow the `be-handler-tests` pattern (TestContainers + Respawn + Bogus builders):
- `ProposeMeetingHandlerTests`: happy path, past date rejection, active negotiation conflict
- `RespondToMeetingHandlerTests`: accept, decline, reschedule, non-recipient response rejected
- `ShareAvailabilityHandlerTests`: happy path, past slot rejection, active negotiation conflict
- `RespondToAvailabilityHandlerTests`: pick slot (creates meeting), share back (creates new card), non-recipient rejected
- `MeetingExpiryServiceTests`: expiry sets status and fires events
