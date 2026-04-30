# Notifications & Inbox Badge Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** (1) Fix the unread badge count not incrementing when offers, meetings, or contract events arrive; (2) show browser desktop notifications when new messages or chat events arrive.

**Architecture:** The unread badge is incremented via a SignalR event `UpdateUnreadCount` emitted from `ChatHub.cs`. Currently this event is only sent for text `SendMessage`. We add the same emit for `MakeOffer`, `ProposeMeeting`, `ShareAvailability`, `RequestContract`, `SubmitContractSellerForm`, `SubmitContractBuyerForm`. Browser notifications are added in the FE `useChatHub.ts` hook, which handles all incoming SignalR events.

**Tech Stack:** ASP.NET Core 8 SignalR (backend), React + TypeScript + SignalR client, Web Notifications API (frontend).

---

### Task 1: Emit UpdateUnreadCount for Offer/Meeting/Contract Events

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

**Context:** The `SendMessage` hub method sends `UpdateUnreadCount` to the recipient with their new unread count. The count is derived from `IMediator.Send(new GetUnreadCountQuery { UserId = ... })`. Other event methods (`MakeOffer`, `ProposeMeeting`, etc.) do not send this event.

- [ ] **Step 1: Identify the GetUnreadCountQuery and how SendMessage calls it**

Read the top portion of `ChatHub.cs`:

```bash
cat Automotive.Marketplace.Server/Hubs/ChatHub.cs | head -80
```

Find the `SendMessage` method and the line that emits `UpdateUnreadCount`. Note the exact method signature `GetUnreadCountQuery` uses so you can replicate it in the other methods.

- [ ] **Step 2: Create a private helper in ChatHub for sending unread count**

Add a private method at the bottom of the `ChatHub` class to avoid repeating the pattern:

```csharp
private async Task NotifyUnreadCount(Guid recipientId, CancellationToken cancellationToken = default)
{
    var unreadResult = await mediator.Send(
        new GetUnreadCountQuery { UserId = recipientId }, cancellationToken);
    await Clients
        .Group($"user-{recipientId}")
        .SendAsync("UpdateUnreadCount", unreadResult.UnreadCount, cancellationToken);
}
```

Note: Check the actual method/property names in `GetUnreadCountQuery` and its response by reading the file:
```bash
cat Automotive.Marketplace.Application/Features/MessageFeatures/GetUnreadCount/GetUnreadCountQuery.cs
cat Automotive.Marketplace.Application/Features/MessageFeatures/GetUnreadCount/GetUnreadCountQueryHandler.cs
```

- [ ] **Step 3: Call NotifyUnreadCount in MakeOffer**

After the `await Clients.Group(...).SendAsync("OfferMade", ...)` line in the `MakeOffer` method, add:

```csharp
await NotifyUnreadCount(result.RecipientId);
```

- [ ] **Step 4: Call NotifyUnreadCount in ProposeMeeting**

After the `await Clients.Group(...).SendAsync("MeetingProposed", ...)` line in `ProposeMeeting`:

```csharp
await NotifyUnreadCount(result.RecipientId);
```

- [ ] **Step 5: Call NotifyUnreadCount in ShareAvailability**

After the `await Clients.Group(...).SendAsync("AvailabilityShared", ...)` line in `ShareAvailability` (or whatever the event name is for sharing meeting slots):

```csharp
await NotifyUnreadCount(result.RecipientId);
```

- [ ] **Step 6: Call NotifyUnreadCount in RequestContract**

After the `await Clients.Group(...).SendAsync("ContractRequested", ...)` line:

```csharp
await NotifyUnreadCount(result.RecipientId);
```

- [ ] **Step 7: Call NotifyUnreadCount in SubmitContractSellerForm and SubmitContractBuyerForm**

Same pattern — after the `SendAsync` call in each method:

```csharp
await NotifyUnreadCount(result.RecipientId);
```

- [ ] **Step 8: Build backend**

```bash
dotnet build ./Automotive.Marketplace.sln
```
Expected: build succeeds, no compilation errors.

- [ ] **Step 9: Commit**

```bash
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs
git commit -m "fix: emit UpdateUnreadCount for offers, meetings, and contract events

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Browser Desktop Notifications for Incoming Chat Events

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

**Context:** `useChatHub.ts` handles all incoming SignalR events for the chat feature. Each event is processed in the same `useEffect` where the connection is set up. The hook has access to the current user's ID and the full conversation data.

The browser's `Notification` API is used:
- `Notification.permission` is checked and requested at connection time.
- Only notifications for messages/events that were NOT sent by the current user are shown.
- The app should not show a notification if the browser tab is already focused (use `document.hasFocus()`).

The following events should trigger a notification:
| Event | Message |
|-------|---------|
| `RECEIVE_MESSAGE` | "New message from {senderName}" |
| `OFFER_MADE` | "Price offer from {senderName}" |
| `OFFER_STATUS_UPDATED` | "Offer status updated by {senderName}" |
| `MEETING_PROPOSED` | "Meeting proposed by {senderName}" |
| `MEETING_CANCELLED` | "Meeting cancelled by {senderName}" |
| `CONTRACT_REQUESTED` | "Contract requested by {senderName}" |
| `CONTRACT_SELLER_SUBMITTED` | "Contract submitted by {senderName}" |
| `CONTRACT_BUYER_SUBMITTED` | "Contract submitted by {senderName}" |

**Important:** Use exact event names as they appear in `useChatHub.ts` — read the file to get correct casing.

- [ ] **Step 1: Add notification permission request when hub connects**

In the `useEffect` that establishes the SignalR connection, after the connection starts, add:

```ts
// Request browser notification permission once on connect
if ("Notification" in window && Notification.permission === "default") {
  void Notification.requestPermission();
}
```

- [ ] **Step 2: Create a helper to show a notification**

Add this utility at the top of the hook file (or inline if the file is not too large):

```ts
function showBrowserNotification(title: string, body: string) {
  if (
    !("Notification" in window) ||
    Notification.permission !== "granted" ||
    document.hasFocus()
  ) {
    return;
  }
  new Notification(title, {
    body,
    icon: "/favicon.ico",
  });
}
```

- [ ] **Step 3: Call showBrowserNotification in each relevant event handler**

**RECEIVE_MESSAGE** — only when the sender is not the current user:
```ts
// After updating query cache, add:
if (payload.senderId !== currentUserId) {
  showBrowserNotification(
    t("notifications.newMessage", { ns: "chat" }),
    payload.senderName,
  );
}
```

**OFFER_MADE** — only when the sender is not the current user:
```ts
if (payload.senderId !== currentUserId) {
  showBrowserNotification(
    t("notifications.offerReceived", { ns: "chat" }),
    payload.senderName,
  );
}
```

Apply the same pattern for `MEETING_PROPOSED`, `MEETING_CANCELLED`, `CONTRACT_REQUESTED`, `CONTRACT_SELLER_SUBMITTED`, `CONTRACT_BUYER_SUBMITTED`, `OFFER_STATUS_UPDATED` — check the actual payload field names from the type definitions.

- [ ] **Step 4: Add notification i18n keys to en and lt chat translations**

Check what the chat namespace file is called:
```bash
ls automotive.marketplace.client/src/lib/i18n/locales/en/
ls automotive.marketplace.client/src/lib/i18n/locales/lt/
```

Add to the EN chat translation file:
```json
{
  "notifications": {
    "newMessage": "New message",
    "offerReceived": "New price offer",
    "offerUpdated": "Offer status updated",
    "meetingProposed": "New meeting proposal",
    "meetingCancelled": "Meeting cancelled",
    "contractRequested": "Contract requested",
    "contractSubmitted": "Contract submitted"
  }
}
```

Add to the LT chat translation file:
```json
{
  "notifications": {
    "newMessage": "Nauja žinutė",
    "offerReceived": "Naulas kainos pasiūlymas",
    "offerUpdated": "Pasiūlymo statusas atnaujintas",
    "meetingProposed": "Naujas susitikimo pasiūlymas",
    "meetingCancelled": "Susitikimas atšauktas",
    "contractRequested": "Prašoma sutarties",
    "contractSubmitted": "Sutartis pateikta"
  }
}
```

- [ ] **Step 5: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

- [ ] **Step 6: Commit**

```bash
git add \
  automotive.marketplace.client/src/features/chat/api/useChatHub.ts \
  automotive.marketplace.client/src/lib/i18n/locales/
git commit -m "feat: add browser desktop notifications for incoming chat events

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
