# Meetup Planning UX Improvements + UI Polish

**Date:** 2025-07-21
**Branch:** `fixes-and-language-support`
**Depends on:** Meetup Planning base feature (complete on `planning-meetings` branch, merged)

---

## Overview

Seven improvements to the meetup planning feature covering cancel actions, timezone handling, availability slot picking, meeting card interactions, sticky confirmed meetings, and inbox URL routing.

## Items

### 1. Cancel Meeting / Cancel Availability (Proposer Action)

**Problem:** The proposer of a meeting or availability card has no way to retract it before the other party responds.

**Solution:**

- Add `Cancelled` status to `MeetingStatus` enum (value 5, after Expired=4).
- Add `Cancelled` status to `AvailabilityCardStatus` enum (value 3, after Expired=2).
- New CQRS command: `CancelMeeting` — validates the caller is the initiator and the meeting is `Pending`, sets status to `Cancelled`.
- New CQRS command: `CancelAvailability` — validates the caller is the initiator and the card is `Pending`, sets status to `Cancelled`.
- New SignalR hub methods: `CancelMeeting`, `CancelAvailability` — broadcast `MeetingCancelled` / `AvailabilityCancelled` events to both parties.
- Frontend: MeetingCard shows a "Cancel" button (ghost variant, destructive color) when `currentUserId === meeting.initiatorId && meeting.status === 'Pending'`.
- Frontend: AvailabilityCardComponent shows a "Cancel" button when `currentUserId === card.initiatorId && card.status === 'Pending'`.
- New card status config for `Cancelled`: grey header (`bg-muted-foreground/60`), label "Meetup Cancelled" / "Availability Cancelled", muted styling.
- `MeetingExpiryService` skips items with `Cancelled` status (already skips non-Pending, but worth being explicit).

**Types updated:**
- `MeetingStatus`: add `'Cancelled'`
- `AvailabilityCardStatus`: add `'Cancelled'`
- New event payloads: `MeetingCancelledPayload`, `AvailabilityCancelledPayload`

### 2. Inline Availability Slot Picker

**Problem:** Clicking "Propose →" on an availability slot currently creates a meeting using the full slot range. Users should pick a specific start time and duration within the slot.

**Solution:**

- When recipient clicks "Propose →" on a slot, the button text changes to "Close ▲" and an inline picker expands below the slot row.
- The picker contains:
  - **Start time** — `<input type="time">` constrained to the slot's start–end range.
  - **Duration** — `<select>` with options: 15, 30, 60, 90, 120 min. Default: 60 min. Minimum: 15 min.
  - **Propose** button — sends the selected time and duration.
- Validation: `startTime >= slot.startTime` and `startTime + duration <= slot.endTime`. Show error text if violated.
- Only one slot picker can be expanded at a time within the same availability card.
- The `onPickSlot` callback signature changes: `(cardId: string, slotId: string, startTime: string, durationMinutes: number) => void`.
- Backend `RespondToAvailability` handler: when action is `PickSlot`, use the provided `startTime` and `durationMinutes` instead of the full slot range. Add `StartTime` (DateTime) and `DurationMinutes` (int) to the command.

### 3. Browser Timezone Display

**Problem:** All times display as UTC. Users should see times in their local timezone.

**Solution:**

- **Storage:** No backend changes. All times remain UTC in the database.
- **Input:** Remove the `Z` suffix when constructing `Date` objects from form inputs, so the browser interprets them as local time: `new Date(\`${date}T${time}:00\`)` instead of `new Date(\`${date}T${time}:00Z\`)`.
- **Display:** `date-fns` `format()` already renders in local time. Remove the "UTC" suffix from all time labels.
- **Timezone indicator:** Add a helper function `getTimezoneOffsetLabel(): string` that returns the current offset like `UTC+3`, `UTC-5`, `UTC+0`. Uses `new Date().getTimezoneOffset()` to compute.
- **Where the label appears:**
  - MeetingCard: after the time range, e.g., "14:00 – 15:00 UTC+3"
  - AvailabilityCardComponent: after each slot time range
  - ProposeMeetingModal: label changes from "Start time (UTC)" to "Start time ({timezone})"
  - ShareAvailabilityModal: label changes from "From" / "To" to "From ({timezone})" / "To ({timezone})" — or a single timezone note below the grid
  - Sticky meeting bar: after the time range
- **Helper location:** Create `automotive.marketplace.client/src/features/chat/utils/timezone.ts` with the `getTimezoneOffsetLabel` function.

### 4. "Suggest Alternative" on Meeting Card

**Problem:** The "Reschedule" button only opens the ProposeMeetingModal. Users should also be able to share availability as an alternative.

**Solution:**

- Rename the "Reschedule" button to "Suggest alternative".
- On click, open a popover (not a modal) with two options:
  1. "Propose a counter time" — opens ProposeMeetingModal in reschedule mode (existing behavior).
  2. "Share my availability" — opens ShareAvailabilityModal.
- New prop on MeetingCard: `onShareAvailability: (meetingId: string, slots: { startTime: string; endTime: string }[]) => void`.
- When sharing availability from the meeting card context, the original meeting is declined (since the recipient is rejecting the specific time but offering alternatives).
- MessageThread wires up the new prop: calls `respondToMeeting` with action `'Reschedule'` for counter time (existing behavior — creates new meeting, sets old to Rescheduled), or for availability sharing: calls `respondToMeeting` with action `'Decline'` on the meeting, then calls `shareAvailability` to send the availability card.

### 5. Confirmed Meeting Guard

**Problem:** Users can start new meetup negotiations while a confirmed meeting exists, leading to confusion.

**Solution:**

- When the user clicks "Propose a time" or "Share availability" from the `+` menu:
  1. Check `messages` for any message with `messageType === 'Meeting' && meeting.status === 'Accepted'`.
  2. If found, show a confirmation dialog (shadcn `AlertDialog`):
     - Title: "Cancel existing meetup?"
     - Description: "You have a confirmed meetup on {date} at {time}. Starting a new negotiation will cancel it."
     - Actions: "Continue" (destructive) / "Keep existing"
  3. On "Continue": cancel the existing meeting (call `cancelMeeting` hub method), then open the requested modal.
  4. On "Keep existing": close dialog, do nothing.
- The guard lives in the ActionBar component since that's where the `+` menu lives.
- The `cancelMeeting` function from `useChatHub` is passed to ActionBar as a new prop.

### 6. Sticky Confirmed Meeting Bar

**Problem:** When users scroll past a confirmed meeting card in the message thread, they lose sight of the upcoming meetup details.

**Solution:**

- In `MessageThread`, find the accepted meeting: `messages.find(m => m.messageType === 'Meeting' && m.meeting?.status === 'Accepted')`.
- Attach a `ref` to the confirmed meeting card's DOM element.
- Use `IntersectionObserver` to detect when the card leaves the viewport (scrolls above the visible area).
- When the card is not visible (scrolled up past the top), render a sticky bar at the top of the scrollable message area:
  - Position: `sticky top-0 z-10`
  - Background: green-tinted to match the confirmed card header (`bg-green-900/95 backdrop-blur-sm`)
  - Content: checkmark icon + "Meetup Confirmed" + "Fri, Apr 25 · 14:00–15:00 UTC+3" + "· Central Park"
  - One compact line, ~40px height
- When the card re-enters the viewport, hide the sticky bar.
- If there's no accepted meeting, no sticky bar renders.

### 7. Conversation ID in Inbox URL

**Problem:** Refreshing `/inbox` loses the selected conversation.

**Solution:**

- Add route: `/inbox/$conversationId` as a child or sibling route to `/inbox`.
- `/inbox` — shows conversation list, right panel shows "Select a conversation" placeholder.
- `/inbox/:conversationId` — shows conversation list with that conversation selected + message thread.
- When user clicks a conversation in the list, navigate to `/inbox/:conversationId`.
- On page load at `/inbox/:conversationId`, auto-select that conversation.
- If the conversation ID doesn't exist in the user's conversations, redirect to `/inbox`.
- The existing TanStack Router setup should be extended with the new parameterized route.

**Router changes:**
- Existing route file for inbox (find it in `src/routes/`) needs a new parameterized variant.
- The inbox page component reads the `conversationId` param and passes it as the initially selected conversation.

---

## Files Affected

### Backend (new)
- `Automotive.Marketplace.Domain/Enums/MeetingStatus.cs` — add `Cancelled = 5`
- `Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs` — add `Cancelled = 3`
- `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/` — Command, Handler, Response
- `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/` — Command, Handler, Response
- `Automotive.Marketplace.Server/Hubs/ChatHub.cs` — add CancelMeeting, CancelAvailability methods

### Backend (modified)
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommand.cs` — add `StartTime`, `DurationMinutes` fields
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommandHandler.cs` — use provided start time and duration for PickSlot
- `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommand.cs` — add optional `CancelMeetingId` field
- `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommandHandler.cs` — cancel existing meeting if `CancelMeetingId` provided
- `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommand.cs` — add optional `CancelMeetingId` field
- `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommandHandler.cs` — cancel existing meeting if `CancelMeetingId` provided

### Frontend (new)
- `automotive.marketplace.client/src/features/chat/utils/timezone.ts` — `getTimezoneOffsetLabel()` helper
- `automotive.marketplace.client/src/features/chat/types/MeetingEventPayloads.ts` — add `MeetingCancelledPayload`, `AvailabilityCancelledPayload`
- `automotive.marketplace.client/src/routes/inbox/$conversationId.tsx` — (or similar, depending on TanStack Router file-based routing structure)

### Frontend (modified)
- `automotive.marketplace.client/src/features/chat/types/Meeting.ts` — add `'Cancelled'` to `MeetingStatus`
- `automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts` — add `'Cancelled'` to `AvailabilityCardStatus`
- `automotive.marketplace.client/src/features/chat/constants/chatHub.ts` — add `CANCEL_MEETING`, `CANCEL_AVAILABILITY`, `MEETING_CANCELLED`, `AVAILABILITY_CANCELLED`
- `automotive.marketplace.client/src/features/chat/api/useChatHub.ts` — add `cancelMeeting`, `cancelAvailability` functions + event listeners
- `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx` — cancel button, suggest alternative popover, cancelled status config, timezone display
- `automotive.marketplace.client/src/features/chat/components/AvailabilityCardComponent.tsx` — cancel button, inline slot picker, timezone display
- `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx` — timezone label, local time input
- `automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx` — timezone label, local time input
- `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx` — confirmed meeting guard dialog
- `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx` — sticky bar, pass new props, wire cancel/share-availability
- `automotive.marketplace.client/src/routes/inbox/` — route changes for parameterized conversation ID

---

## Out of Scope

- i18n / language support (Group D — separate spec)
- Backend timezone storage changes (stays UTC)
- Push notifications for meeting reminders
- Map/location picker integration
