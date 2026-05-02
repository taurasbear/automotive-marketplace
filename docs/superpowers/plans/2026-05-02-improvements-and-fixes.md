# Automotive Marketplace Improvements & Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement 14 improvements and fixes across 7 groups: bug fixes, permissions, chat features, listing features, UI redesign, external API data display, and a real-time dashboard.

**Architecture:** Changes span the full stack -- Domain enums/entities, Application CQRS handlers, Server controllers/hubs, and React frontend components/translations. Each group is independent and can be committed separately.

**Tech Stack:** ASP.NET Core 8, EF Core, MediatR, SignalR, React 19, TypeScript, TanStack Query, i18next, Tailwind CSS, shadcn/ui

**Spec:** `docs/superpowers/specs/2026-05-02-improvements-and-fixes-design.md`

---

## Group 1: Bug Fixes

### Task 1: Fix price offer decimal formatting

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx:98`

- [ ] **Step 1: Fix the placeholder formatting**

In `MakeOfferModal.tsx`, line 98, the placeholder shows raw floats. Change:

```tsx
// Before (line 98):
placeholder={`${Math.ceil(minAmount)} – ${listingPrice}`}

// After:
placeholder={`${formatCurrency(Math.ceil(minAmount))} – ${formatCurrency(listingPrice)}`}
```

`formatCurrency` is already imported on line 10.

- [ ] **Step 2: Verify the fix**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds with no errors.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx
git commit -m "fix: format price range in offer modal placeholder

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Fix "Kontra" translation

**Files:**
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/chat.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/chat.json`

- [ ] **Step 1: Update Lithuanian translations**

In `lt/chat.json`, change these keys:

```json
// offerCard section (line 44):
"counterOffer": "Kitas kainos pasiūlymas",

// offerCard.actions section (line 59):
"counter": "Siūlyti kitą kainą",

// makeOfferModal section (line 99):
"counterOfferTitle": "Siūlyti kitą kainą",

// makeOfferModal section (line 106):
"sendCounter": "Siųsti pasiūlymą"
```

- [ ] **Step 2: Update English translations for consistency**

In `en/chat.json`, verify the English counterparts are clear. The English keys should remain as-is ("Counter Offer", "Counter", etc.) unless they also say "Kontra".

- [ ] **Step 3: Verify the text fits in OfferCard**

The OfferCard has `max-w-[280px]` and the counter button uses `flex-1` with `text-xs`. "Siūlyti kitą kainą" is longer than "Siūlyti kitokią" but with `text-xs` and `flex-1` it should fit in the 3-button layout. If it doesn't, we can abbreviate to "Kita kaina" for the button only.

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/lib/i18n/locales/lt/chat.json automotive.marketplace.client/src/lib/i18n/locales/en/chat.json
git commit -m "fix: replace Kontra translation with Siūlyti kitą kainą

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Fix seller insights panel width

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx:262`

- [ ] **Step 1: Constrain SellerInsightsPanel width**

In `MyListingCard.tsx`, the `SellerInsightsPanel` is rendered at line 262 outside the card's `w-204` container. Wrap it or constrain it:

```tsx
// Before (line 262):
<SellerInsightsPanel listingId={listing.id} />

// After:
<div className="w-204">
  <SellerInsightsPanel listingId={listing.id} />
</div>
```

Alternatively, if the card's outer div already has `w-204` (it does, at line 89), move the `SellerInsightsPanel` inside the outer div but below the card, keeping it within the same width constraint. Looking at the structure, the outer `<div>` at line 88 wraps everything. The issue is that `SellerInsightsPanel` at line 262 is inside that wrapper but the card grid at line 89 has `w-204` while the panel sits below without that constraint. The fix is to add `max-w-204` to the panel's root div or to its wrapper:

```tsx
// After (line 262):
<div className="max-w-204">
  <SellerInsightsPanel listingId={listing.id} />
</div>
```

- [ ] **Step 2: Verify visually**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds. The insights panel should now be the same width as the card above it.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx
git commit -m "fix: constrain seller insights panel width to match listing card

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Fix OnHold status translation

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx:48-64`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`

- [ ] **Step 1: Add missing status translation keys**

In `lt/myListings.json`, add status keys under `card`:

```json
"card": {
  "active": "Aktyvus",
  "sold": "Parduotas",
  "onHold": "Sulaikytas",
  "removed": "Pašalintas",
  "defects": "{{count}} defektai",
  "edit": "Redaguoti",
  "delete": "Ištrinti",
  "location": "Vieta"
}
```

In `en/myListings.json`, add the same keys:

```json
"card": {
  "active": "Active",
  "sold": "Sold",
  "onHold": "On Hold",
  "removed": "Removed",
  "defects": "{{count}} defects",
  "edit": "Edit",
  "delete": "Delete",
  "location": "Location"
}
```

- [ ] **Step 2: Fix StatusBadge to use translation keys for all statuses**

In `MyListingCard.tsx`, replace the `StatusBadge` component (lines 48-65):

```tsx
function StatusBadge({ status }: { status: string }) {
  const { t } = useTranslation("myListings");
  const isSold = status === "Sold" || status === "Bought";
  const isActive = status === "Active" || status === "Approved" || status === "Available";

  const statusKey = (() => {
    if (isSold) return "card.sold";
    if (isActive) return "card.active";
    if (status === "OnHold") return "card.onHold";
    if (status === "Removed") return "card.removed";
    return "card.active";
  })();

  return (
    <span
      className={`rounded px-2 py-0.5 text-xs font-medium ${
        isSold
          ? "bg-gray-700/80 text-gray-100"
          : isActive
            ? "bg-green-600/90 text-white"
            : "bg-yellow-500/90 text-white"
      }`}
    >
      {t(statusKey)}
    </span>
  );
}
```

- [ ] **Step 3: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json
git commit -m "fix: translate OnHold listing status to Lithuanian

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 2: Permissions Fix

### Task 5: Remove ManageListings from default permissions

**Files:**
- Modify: `Automotive.Marketplace.Domain/Constants/DefaultUserPermissions.cs`

- [ ] **Step 1: Remove ManageListings from defaults**

```csharp
// Before:
public static readonly IReadOnlyList<Permission> All =
[
    Permission.ViewListings,
    Permission.CreateListings,
    Permission.ManageListings,
    Permission.ViewModels,
    Permission.ViewVariants,
    Permission.ViewMakes,
];

// After:
public static readonly IReadOnlyList<Permission> All =
[
    Permission.ViewListings,
    Permission.CreateListings,
    Permission.ViewModels,
    Permission.ViewVariants,
    Permission.ViewMakes,
];
```

- [ ] **Step 2: Commit**

```bash
git add Automotive.Marketplace.Domain/Constants/DefaultUserPermissions.cs
git commit -m "fix: remove ManageListings from default user permissions

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Add ownership checks to UpdateListing handler

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/UpdateListing/UpdateListingCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/UpdateListing/UpdateListingCommandHandler.cs`

- [ ] **Step 1: Add CurrentUserId and Permissions to UpdateListingCommand**

Check if `UpdateListingCommand` already has `CurrentUserId`. If not, add it:

```csharp
public sealed record UpdateListingCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid CurrentUserId { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
    // ... existing properties
}
```

- [ ] **Step 2: Add ownership check to UpdateListingCommandHandler**

```csharp
public async Task Handle(UpdateListingCommand request, CancellationToken cancellationToken)
{
    var existingListing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

    var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
    if (existingListing.SellerId != request.CurrentUserId && !canManage)
        throw new ForbiddenAccessException("You can only edit your own listings.");

    var updatedListing = mapper.Map(request, existingListing);
    await repository.UpdateAsync(updatedListing, cancellationToken);
}
```

Check what exception type is used in the project. Look for `ForbiddenAccessException` or similar in `Automotive.Marketplace.Application/`. If it doesn't exist, use `UnauthorizedAccessException` which the existing `UnauthorizedExceptionFilter` handles.

- [ ] **Step 3: Update the controller to pass CurrentUserId**

Find the controller method that calls `UpdateListingCommand` and ensure `CurrentUserId` is populated from the JWT claim, same pattern as `ChatHub.UserId`.

- [ ] **Step 4: Run tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All existing tests pass.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/UpdateListing/
git commit -m "fix: add ownership check to UpdateListing handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Add ownership checks to DeleteListing handler

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/DeleteListing/DeleteListingCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/DeleteListing/DeleteListingCommandHandler.cs`

- [ ] **Step 1: Add CurrentUserId and Permissions to DeleteListingCommand**

```csharp
public sealed record DeleteListingCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid CurrentUserId { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
```

- [ ] **Step 2: Add ownership check to handler**

```csharp
public async Task Handle(DeleteListingCommand request, CancellationToken cancellationToken)
{
    var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

    var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
    if (listing.SellerId != request.CurrentUserId && !canManage)
        throw new UnauthorizedAccessException("You can only delete your own listings.");

    await repository.DeleteAsync(listing, cancellationToken);
}
```

- [ ] **Step 3: Update the controller to pass CurrentUserId**

Same pattern as Task 6.

- [ ] **Step 4: Run tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All existing tests pass.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/DeleteListing/
git commit -m "fix: add ownership check to DeleteListing handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Update frontend permission checks

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Fix edit/delete button visibility**

In `ListingDetailsContent.tsx`, change line 42 and the conditional at line 157:

```tsx
// Before (line 42):
const canManageListing = permissions.includes(PERMISSIONS.ManageListings);

// Keep this as-is, but change the conditional (line 157):
// Before:
{canManageListing && (

// After:
{(isSeller || canManageListing) && (
```

This shows edit/delete for the listing owner AND for admins with ManageListings.

- [ ] **Step 2: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "fix: show edit/delete buttons for listing owners

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 3: Chat Features

### Task 9: Add CancelOffer backend

**Files:**
- Modify: `Automotive.Marketplace.Domain/Enums/OfferStatus.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelOffer/CancelOfferCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelOffer/CancelOfferCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelOffer/CancelOfferResponse.cs`

- [ ] **Step 1: Add Cancelled to OfferStatus enum**

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum OfferStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Countered = 3,
    Expired = 4,
    Cancelled = 5,
}
```

- [ ] **Step 2: Create CancelOfferCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public sealed record CancelOfferCommand : IRequest<CancelOfferResponse>
{
    public Guid OfferId { get; set; }
    public Guid RequesterId { get; set; }
}
```

- [ ] **Step 3: Create CancelOfferResponse**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public sealed record CancelOfferResponse
{
    public Guid OfferId { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid ConversationId { get; set; }
}
```

- [ ] **Step 4: Create CancelOfferCommandHandler**

Follow the pattern from `RespondToOfferCommandHandler`. The handler should:
- Load the offer with its Conversation (to get the other party's ID)
- Validate: `offer.InitiatorId == request.RequesterId` (only the person who made the offer can cancel)
- Validate: `offer.Status == OfferStatus.Pending`
- Set `offer.Status = OfferStatus.Cancelled`
- Save changes
- Return CancelOfferResponse with both party IDs

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public class CancelOfferCommandHandler(IRepository repository)
    : IRequestHandler<CancelOfferCommand, CancelOfferResponse>
{
    public async Task<CancelOfferResponse> Handle(CancelOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await repository.Query<Offer>()
            .Include(o => o.Conversation)
            .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken)
            ?? throw new KeyNotFoundException("Offer not found.");

        if (offer.InitiatorId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the offer initiator can cancel.");

        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("Only pending offers can be cancelled.");

        offer.Status = OfferStatus.Cancelled;
        await repository.UpdateAsync(offer, cancellationToken);

        var recipientId = offer.Conversation.BuyerId == request.RequesterId
            ? offer.Conversation.SellerId
            : offer.Conversation.BuyerId;

        return new CancelOfferResponse
        {
            OfferId = offer.Id,
            InitiatorId = offer.InitiatorId,
            RecipientId = recipientId,
            ConversationId = offer.ConversationId,
        };
    }
}
```

Note: Check the `Conversation` entity for exact property names (`BuyerId`/`SellerId` vs `User1Id`/`User2Id`). Adjust accordingly.

- [ ] **Step 5: Run tests**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/Enums/OfferStatus.cs Automotive.Marketplace.Application/Features/ChatFeatures/CancelOffer/
git commit -m "feat: add CancelOffer command and handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: Add CancelOffer to ChatHub and frontend

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`
- Modify: `automotive.marketplace.client/src/features/chat/components/OfferCard.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/chat.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/chat.json`

- [ ] **Step 1: Add CancelOffer method to ChatHub**

Add after the `RespondToOffer` method (~line 84), following the same pattern:

```csharp
public async Task CancelOffer(Guid offerId)
{
    var result = await mediator.Send(new CancelOfferCommand
    {
        OfferId = offerId,
        RequesterId = UserId,
    });

    await Clients.Group($"user-{result.InitiatorId}").SendAsync("OfferCancelled", result);
    await Clients.Group($"user-{result.RecipientId}").SendAsync("OfferCancelled", result);
}
```

Add the using: `using Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;`

- [ ] **Step 2: Add Cancelled status config to OfferCard**

In `OfferCard.tsx`, add to `statusConfig` after `Expired`:

```tsx
Cancelled: {
  headerClass: "bg-muted-foreground/60",
  borderClass: "border-border",
  labelKey: "offerCard.statusLabels.cancelled",
  icon: Ban,
  labelClass: "text-muted",
  subLabelKey: "offerCard.subtitles.cancelled",
  subLabelClass: "text-muted-foreground",
  priceClass: "text-muted-foreground line-through",
  badgeClass: "bg-muted text-muted-foreground",
},
```

Import `Ban` from lucide-react (add to existing import on line 2).

- [ ] **Step 3: Add cancel button to OfferCard**

Add `onCancel` prop and cancel button. Update the props type:

```tsx
type OfferCardProps = {
  offer: Offer;
  currentUserId: string;
  listingPrice: number;
  onAccept: (offerId: string) => void;
  onDecline: (offerId: string) => void;
  onCounter: (offerId: string, amount: number) => void;
  onCancel: (offerId: string) => void;
};
```

Add cancel logic after `canRespond`:

```tsx
const canCancel =
  offer.status === "Pending" && currentUserId === offer.initiatorId;
```

Add cancel button after the `canRespond` buttons block (after line 158):

```tsx
{canCancel && (
  <div className="mt-3 flex gap-2">
    <Button
      size="sm"
      variant="ghost"
      className="text-muted-foreground hover:text-destructive h-7 w-full text-xs"
      onClick={() => onCancel(offer.id)}
    >
      {t("offerCard.actions.cancel")}
    </Button>
  </div>
)}
```

- [ ] **Step 4: Add translations**

In `lt/chat.json`, add under `offerCard.statusLabels`:
```json
"cancelled": "Pasiūlymas atšauktas"
```

Add under `offerCard.subtitles`:
```json
"cancelled": "Atšaukta iniciatoriaus"
```

Add under `offerCard.actions`:
```json
"cancel": "Atšaukti pasiūlymą"
```

Do the same for `en/chat.json`:
```json
// statusLabels:
"cancelled": "Offer Cancelled"
// subtitles:
"cancelled": "Withdrawn by sender"
// actions:
"cancel": "Cancel Offer"
```

- [ ] **Step 5: Handle OfferCancelled event in useChatHub**

In `useChatHub.ts`, add a listener for `OfferCancelled` following the pattern of `handleOfferStatusUpdate`. The event should update the offer's status to "Cancelled" in the message cache.

- [ ] **Step 6: Update Offer type**

In the `Offer` type file (likely `automotive.marketplace.client/src/features/chat/types/Offer.ts`), add `"Cancelled"` to the status union type.

- [ ] **Step 7: Wire up onCancel in the parent component**

Find where `OfferCard` is rendered (likely in the chat message thread) and pass the `onCancel` prop that calls `hub.invoke("CancelOffer", offerId)`.

- [ ] **Step 8: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add cancel offer UI and SignalR integration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: Update contract cancellation

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommandHandler.cs`
- Modify: `automotive.marketplace.client/src/features/chat/components/ContractCard.tsx`

- [ ] **Step 1: Update CancelContract handler to allow either party**

The current handler only allows the initiator to cancel, and only when status is `Pending`. Update to allow either party (buyer or seller) and expand the allowed statuses:

```csharp
public async Task<CancelContractResponse> Handle(CancelContractCommand request, CancellationToken cancellationToken)
{
    var contract = await repository.Query<ContractCard>()
        .Include(c => c.Conversation)
        .FirstOrDefaultAsync(c => c.Id == request.ContractCardId, cancellationToken)
        ?? throw new KeyNotFoundException("Contract not found.");

    // Either party can cancel
    var buyerId = contract.Conversation.BuyerId;
    var sellerId = contract.Conversation.SellerId;
    if (request.RequesterId != buyerId && request.RequesterId != sellerId)
        throw new UnauthorizedAccessException("Only conversation participants can cancel.");

    // Can cancel if not Complete
    var cancellableStatuses = new[]
    {
        ContractCardStatus.Pending,
        ContractCardStatus.Active,
        ContractCardStatus.SellerSubmitted,
        ContractCardStatus.BuyerSubmitted,
    };
    if (!cancellableStatuses.Contains(contract.Status))
        throw new InvalidOperationException("Contract cannot be cancelled in its current state.");

    contract.Status = ContractCardStatus.Cancelled;
    await repository.UpdateAsync(contract, cancellationToken);

    // return response...
}
```

Note: Adjust property names based on actual Conversation entity structure.

- [ ] **Step 2: Add cancel button to Active/Submitted contract states in frontend**

In `ContractCard.tsx`, the cancel button currently only shows for `Pending && isInitiator`. Add a cancel button for Active/SellerSubmitted/BuyerSubmitted states as well.

After the existing `["Active", "SellerSubmitted", "BuyerSubmitted"].includes(card.status)` block (~line 163-193), add:

```tsx
{["Active", "SellerSubmitted", "BuyerSubmitted"].includes(card.status) && (
  <Button
    size="sm"
    variant="ghost"
    className="text-destructive hover:text-destructive h-7 text-xs w-full"
    onClick={() => onCancel(card.id)}
  >
    {t("contractCard.cancel")}
  </Button>
)}
```

Also update the Pending state: allow both initiator AND recipient to cancel (remove the `isInitiator` restriction for the cancel button, or add cancel for recipient too).

- [ ] **Step 3: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln && cd automotive.marketplace.client && npm run build`
Expected: Both builds succeed.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: allow either party to cancel contract before completion

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 4: Listing Features

### Task 12: Edit own listing navigates to edit page

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Replace EditListingDialog with navigation for sellers**

In `ListingDetailsContent.tsx`, change the edit/delete section (around line 157-168):

```tsx
{(isSeller || canManageListing) && (
  <div className="flex flex-shrink-0 gap-2">
    {isSeller ? (
      <Button
        variant="secondary"
        size="sm"
        onClick={() => router.navigate({ to: "/my-listings/$id", params: { id } })}
      >
        <Pencil className="mr-1 h-4 w-4" />
      </Button>
    ) : (
      <EditListingDialog listing={listing} id={id} />
    )}
    <Button
      variant="destructive"
      size="sm"
      onClick={handleDelete}
    >
      <Trash />
    </Button>
  </div>
)}
```

Import `Pencil` from lucide-react if not already imported.

- [ ] **Step 2: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: navigate to edit page for own listings instead of dialog

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: Mark listing as sold

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`

- [ ] **Step 1: Add translation keys**

In `lt/myListings.json` under `card`:
```json
"markAsSold": "Pažymėti kaip parduotą",
"markAsSoldConfirmTitle": "Pažymėti kaip parduotą?",
"markAsSoldConfirmDescription": "Skelbimas bus pašalintas iš paieškos rezultatų."
```

In `en/myListings.json` under `card`:
```json
"markAsSold": "Mark as Sold",
"markAsSoldConfirmTitle": "Mark as sold?",
"markAsSoldConfirmDescription": "This listing will be removed from search results."
```

- [ ] **Step 2: Add Mark as Sold button to MyListingCard**

Find or create a mutation hook for updating listing status (check if `useUpdateListingStatus` exists, or create one using the existing `UpdateListingStatus` endpoint).

In `MyListingCard.tsx`, add a "Mark as Sold" button in the action buttons section. Add it before the Edit button (around line 195), visible only when `listing.status === "Available"`:

```tsx
{listing.status === "Available" && (
  <AlertDialog>
    <AlertDialogTrigger asChild>
      <Button size="sm" variant="outline">
        {t("card.markAsSold")}
      </Button>
    </AlertDialogTrigger>
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{t("card.markAsSoldConfirmTitle")}</AlertDialogTitle>
        <AlertDialogDescription>{t("card.markAsSoldConfirmDescription")}</AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>{t("detail.cancel")}</AlertDialogCancel>
        <AlertDialogAction onClick={() => updateStatus.mutate({ id: listing.id, status: "Bought" })}>
          {t("detail.confirm")}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
)}
```

- [ ] **Step 3: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: add mark as sold button for sellers

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 14: Like from listing details page

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Add isLiked to backend response**

In `GetListingByIdResponse.cs`, add:
```csharp
public bool IsLiked { get; set; }
```

- [ ] **Step 2: Populate isLiked in query handler**

In `GetListingByIdQueryHandler.cs`, after loading the listing, check if the current user has liked it:

```csharp
// After mapping the response:
if (request.CurrentUserId != null)
{
    response.IsLiked = await repository.Query<UserListingLike>()
        .AnyAsync(l => l.ListingId == request.Id && l.UserId == request.CurrentUserId, cancellationToken);
}
```

Note: Check if `GetListingByIdQuery` has `CurrentUserId`. If not, add it and pass from the controller.

- [ ] **Step 3: Add isLiked to frontend type**

In `GetListingByIdResponse.ts`, add:
```typescript
isLiked?: boolean;
```

- [ ] **Step 4: Add like button to ListingDetailsContent**

In `ListingDetailsContent.tsx`, import the toggle like hook and heart icons:

```tsx
import { useToggleLike } from "@/features/savedListings/api/useToggleLike";
import { IoHeartOutline, IoHeart } from "react-icons/io5";
```

Add in the title card area, alongside the edit/delete buttons (inside the flex gap-2 div):

```tsx
{userId && !isSeller && (
  <button
    onClick={() => toggleLike.mutate({ listingId: id })}
    className={`flex h-9 w-9 items-center justify-center rounded-full transition-colors ${
      listing.isLiked
        ? "bg-red-500 text-white"
        : "bg-secondary text-muted-foreground hover:text-red-500"
    }`}
  >
    {listing.isLiked ? (
      <IoHeart className="h-5 w-5" />
    ) : (
      <IoHeartOutline className="h-5 w-5" />
    )}
  </button>
)}
```

Add the hook call near the top of the component:
```tsx
const toggleLike = useToggleLike();
```

- [ ] **Step 5: Verify builds**

Run: `dotnet build ./Automotive.Marketplace.sln && cd automotive.marketplace.client && npm run build`
Expected: Both builds succeed.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: add like button to listing details page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 5: UI Redesign

### Task 15: Create Footer component and move links

**Files:**
- Create: `automotive.marketplace.client/src/components/layout/footer/Footer.tsx`
- Modify: `automotive.marketplace.client/src/app/routes/__root.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/MainPage.tsx`

- [ ] **Step 1: Create Footer component**

```tsx
import { PiGithubLogo, PiLinkedinLogo } from "react-icons/pi";
import { useTranslation } from "react-i18next";

export default function Footer() {
  const { t } = useTranslation("common");

  return (
    <footer className="border-border mt-auto border-t">
      <div className="mx-8 flex items-center justify-between py-6 xl:mx-auto xl:max-w-6xl">
        <div className="flex items-center gap-6">
          <a
            href="https://github.com/taurasbear/automotive-marketplace"
            target="_blank"
            rel="noopener noreferrer"
            className="text-muted-foreground hover:text-foreground flex items-center gap-2 text-sm transition-colors"
          >
            <PiGithubLogo className="h-5 w-5" />
            <span>GitHub</span>
          </a>
          <a
            href="https://www.linkedin.com/in/tauras-narvilas/"
            target="_blank"
            rel="noopener noreferrer"
            className="text-muted-foreground hover:text-foreground flex items-center gap-2 text-sm transition-colors"
          >
            <PiLinkedinLogo className="h-5 w-5" />
            <span>LinkedIn</span>
          </a>
        </div>
      </div>
    </footer>
  );
}
```

- [ ] **Step 2: Add Footer to root layout**

In `__root.tsx`, import and add Footer:

```tsx
import Footer from "@/components/layout/footer/Footer";

const RootLayout = () => {
  useChatHub();
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="mx-8 flex-1 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
      <Footer />
    </div>
  );
};
```

- [ ] **Step 3: Remove links from MainPage**

In `MainPage.tsx`, remove the links section and the icon imports:

```tsx
import { ListingSearch } from "@/features/search";

const MainPage = () => {
  return (
    <div className="flex flex-col">
      <ListingSearch className="mt-16 w-full sm:mt-64" />
    </div>
  );
};

export default MainPage;
```

- [ ] **Step 4: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: move GitHub and LinkedIn links to page footer

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 16: Listing details page redesign

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`
- Create: `automotive.marketplace.client/src/features/listingDetails/components/ListingKeySpecs.tsx`
- Create: `automotive.marketplace.client/src/features/listingDetails/components/ListingSecondaryDetails.tsx`

- [ ] **Step 1: Create ListingKeySpecs component**

This reuses `ListingCardBadge` from the listing list feature for consistency:

```tsx
import { ListingCardBadge } from "@/features/listingList";
import { PiEngine } from "react-icons/pi";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { TbManualGearbox } from "react-icons/tb";
import { IoSpeedometerOutline } from "react-icons/io5";
import { Calendar, Cog } from "lucide-react";
import { useTranslation } from "react-i18next";
import { formatNumber } from "@/lib/i18n/formatNumber";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

type Props = {
  listing: GetListingByIdResponse;
};

export function ListingKeySpecs({ listing }: Props) {
  const { t } = useTranslation("listings");

  return (
    <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
      <h3 className="mb-4 text-lg font-semibold">{t("details.keySpecs")}</h3>
      <div className="grid grid-cols-2 gap-x-0 gap-y-4 sm:grid-cols-3">
        <ListingCardBadge
          Icon={<PiEngine className="h-8 w-8" />}
          title={t("card.engine")}
          stat={`${listing.engineSizeMl / 1000} l ${listing.powerKw} kW`}
        />
        <ListingCardBadge
          Icon={<MdOutlineLocalGasStation className="h-8 w-8" />}
          title={t("card.fuelType")}
          stat={translateVehicleAttr("fuel", listing.fuelName, t)}
        />
        <ListingCardBadge
          Icon={<TbManualGearbox className="h-8 w-8" />}
          title={t("card.gearBox")}
          stat={translateVehicleAttr("transmission", listing.transmissionName, t)}
        />
        <ListingCardBadge
          Icon={<IoSpeedometerOutline className="h-8 w-8" />}
          title={t("details.mileage")}
          stat={`${formatNumber(listing.mileage)} km`}
        />
        <ListingCardBadge
          Icon={<Calendar className="h-7 w-7" />}
          title={t("details.year")}
          stat={String(listing.year)}
        />
        <ListingCardBadge
          Icon={<Cog className="h-7 w-7" />}
          title={t("details.drivetrain")}
          stat={translateVehicleAttr("drivetrain", listing.drivetrainName, t)}
        />
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Create ListingSecondaryDetails component**

```tsx
import { useTranslation } from "react-i18next";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

type Props = {
  listing: GetListingByIdResponse;
};

export function ListingSecondaryDetails({ listing }: Props) {
  const { t } = useTranslation("listings");

  const details = [
    { label: t("details.bodyType"), value: translateVehicleAttr("bodyType", listing.bodyTypeName, t) },
    ...(listing.colour ? [{ label: t("details.colour"), value: listing.colour }] : []),
    { label: t("details.doors"), value: String(listing.doorCount) },
    {
      label: t("details.steering"),
      value: listing.isSteeringWheelRight ? t("details.rightHand") : t("details.leftHand"),
    },
    ...(listing.vin ? [{ label: t("details.vin"), value: listing.vin }] : []),
    { label: t("details.seller"), value: listing.sellerName },
  ];

  return (
    <div className="bg-card text-card-foreground rounded-lg border shadow-sm">
      <div className="p-6">
        <h3 className="text-lg font-semibold">{t("details.additionalDetails")}</h3>
      </div>
      <div className="border-t">
        <dl className="divide-border divide-y">
          {details.map(({ label, value }) => (
            <div key={label} className="grid grid-cols-2 px-6 py-3">
              <dt className="text-muted-foreground text-sm font-medium">{label}</dt>
              <dd className="text-right text-sm">{value}</dd>
            </div>
          ))}
        </dl>
      </div>
    </div>
  );
}
```

- [ ] **Step 3: Add translation keys**

In `lt/listings.json` under `details`, add:
```json
"keySpecs": "Pagrindinės specifikacijos",
"additionalDetails": "Papildoma informacija",
"year": "Metai",
"bodyType": "Kėbulo tipas"
```

In `en/listings.json` under `details`, add:
```json
"keySpecs": "Key Specs",
"additionalDetails": "Additional Details",
"year": "Year",
"bodyType": "Body Type"
```

- [ ] **Step 4: Restructure ListingDetailsContent layout**

Replace the left column content in `ListingDetailsContent.tsx`. The left 2 columns should now be:

1. Image gallery
2. Two spec cards side by side (grid-cols-2)
3. Description card
4. Defects card

Remove the specifications `<dl>` section from the right sidebar.

The right sidebar should only contain:
- Title/price card (with like button, edit/delete)
- Contact Seller button
- Compare button
- ScoreCard
- AiSummarySection

```tsx
{/* Left column - main content */}
<div className="space-y-6 lg:col-span-2">
  <ImageArrowGallery images={galleryImages} className="w-full rounded-lg shadow-lg" />

  {/* Two spec cards side by side */}
  <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
    <ListingKeySpecs listing={listing} />
    <ListingSecondaryDetails listing={listing} />
  </div>

  {listing.description && (
    <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
      <h2 className="mb-4 text-2xl font-semibold">{t("details.description")}</h2>
      <p className="text-muted-foreground">{listing.description}</p>
    </div>
  )}

  {listing.defects.length > 0 && (
    <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
      <h2 className="mb-4 text-2xl font-semibold">{t("details.defects")}</h2>
      <div className="flex flex-wrap gap-2">
        {listing.defects.map((defect) => (
          <span key={defect.id} className="rounded-full border border-amber-500/20 bg-amber-500/10 px-3 py-1 text-sm text-amber-700 dark:text-amber-400">
            {getDefectDisplayName(defect)}
            {defect.images.length > 0 && (
              <span className="ml-1 inline-flex items-center gap-0.5">
                ({defect.images.length} <Camera className="inline h-3.5 w-3.5" />)
              </span>
            )}
          </span>
        ))}
      </div>
    </div>
  )}
</div>
```

- [ ] **Step 5: Verify build**

Run: `cd automotive.marketplace.client && npm run build`
Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: redesign listing details with spec cards layout

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 6: External API Data

### Task 17: Add market price setting to user preferences

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/UserPreferences.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs`
- Create: EF migration
- Modify: `automotive.marketplace.client/src/app/pages/Settings.tsx`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/userPreferences.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/userPreferences.json`

- [ ] **Step 1: Add EnableMarketPriceApi to entity**

In `UserPreferences.cs`:
```csharp
public bool EnableMarketPriceApi { get; set; } = false;
```

- [ ] **Step 2: Add to command**

In `UpsertUserPreferencesCommand.cs`:
```csharp
public bool EnableMarketPriceApi { get; set; }
```

- [ ] **Step 3: Update handler to map the new field**

Check the handler to ensure it maps `EnableMarketPriceApi` to the entity.

- [ ] **Step 4: Create EF migration**

```bash
cd Automotive.Marketplace.Infrastructure
dotnet ef migrations add AddEnableMarketPriceApi --startup-project ../Automotive.Marketplace.Server
```

- [ ] **Step 5: Add toggle to Settings page**

In `Settings.tsx`, add after the AI summary toggle section (before the Reset section):

```tsx
<Separator />

<div className="flex items-center justify-between py-4">
  <div className="flex items-center gap-3">
    <CircleDollarSign className="text-muted-foreground h-5 w-5" />
    <div>
      <p className="text-sm font-medium">
        {t("settings.marketPriceLabel")}
      </p>
      <p className="text-muted-foreground text-xs">
        {t("settings.marketPriceDescription")}
      </p>
    </div>
  </div>
  <Switch
    checked={prefs?.enableMarketPriceApi ?? false}
    onCheckedChange={handleMarketPriceToggle}
  />
</div>
```

Import `CircleDollarSign` from lucide-react. Add the handler:

```tsx
const handleMarketPriceToggle = async (checked: boolean) => {
  if (!prefs) return;
  await upsert({
    ...prefs,
    enableMarketPriceApi: checked,
  });
};
```

Update the other toggle handlers (`handleScoringToggle`, `handleAiSummaryToggle`, `handleResetDefaults`) to also include `enableMarketPriceApi` in their upsert calls.

- [ ] **Step 6: Add translations**

In `lt/userPreferences.json`:
```json
"settings.marketPriceLabel": "Automatinis rinkos kainos tikrinimas",
"settings.marketPriceDescription": "Naudoja išorinę API rinkos kainai nustatyti. Išjungus, rodomi tik ankstesni duomenys."
```

In `en/userPreferences.json`:
```json
"settings.marketPriceLabel": "Automatic Market Price Lookup",
"settings.marketPriceDescription": "Uses an external API to determine market price. When disabled, only cached data is shown."
```

- [ ] **Step 7: Verify builds**

Run: `dotnet build ./Automotive.Marketplace.sln && cd automotive.marketplace.client && npm run build`
Expected: Both succeed.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: add market price API toggle to user preferences

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 18: Show external API data in comparison and listing details

**Files:**
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.tsx`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/compare.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/compare.json`

This task depends on what data is already available in the backend responses. The `GetListingByIdResponse` currently does NOT include external API data fields. Before implementing the frontend, the backend response needs to be extended.

- [ ] **Step 1: Investigate backend external API data availability**

Check what data the backend already caches/returns from external APIs (Cardog, NHTSA, FuelEconomy). Look at:
- `GetListingByIdResponse.cs` -- what fields exist
- `GetListingComparisonQueryHandler.cs` -- what data it returns
- The vehicle data service responses

If external data is not yet in the responses, extend them to include optional fields:
```csharp
// Add to GetListingByIdResponse.cs
public decimal? MarketMedianPrice { get; set; }
public int? MarketListingCount { get; set; }
public double? FuelEconomyMpgCity { get; set; }
public double? FuelEconomyMpgHighway { get; set; }
public int? SafetyRating { get; set; }
public int? RecallCount { get; set; }
```

- [ ] **Step 2: Add comparison table sections for external data**

In `CompareTable.tsx`, add new sections to `TABLE_SECTIONS`:

```tsx
{
  label: t("table.externalData"),
  rows: [
    {
      field: "marketMedianPrice" as keyof GetListingByIdResponse,
      label: t("table.marketPrice"),
      format: (v) => v ? `${(v as number).toFixed(0)} €` : "—",
    },
    {
      field: "safetyRating" as keyof GetListingByIdResponse,
      label: t("table.safetyRating"),
      format: (v) => v ? `${v}/5` : "—",
    },
    {
      field: "fuelEconomyMpgCity" as keyof GetListingByIdResponse,
      label: t("table.fuelEconomyCity"),
      format: (v) => v ? `${v} MPG` : "—",
    },
    {
      field: "fuelEconomyMpgHighway" as keyof GetListingByIdResponse,
      label: t("table.fuelEconomyHighway"),
      format: (v) => v ? `${v} MPG` : "—",
    },
    {
      field: "recallCount" as keyof GetListingByIdResponse,
      label: t("table.recalls"),
      format: (v) => v != null ? String(v) : "—",
    },
  ],
},
```

- [ ] **Step 3: Add translations**

In `lt/compare.json`:
```json
"table.externalData": "Išoriniai duomenys",
"table.marketPrice": "Rinkos mediana",
"table.safetyRating": "Saugumo įvertinimas",
"table.fuelEconomyCity": "Degalų sąnaudos (miestas)",
"table.fuelEconomyHighway": "Degalų sąnaudos (magistralė)",
"table.recalls": "Atšaukimai"
```

In `en/compare.json`:
```json
"table.externalData": "External Data",
"table.marketPrice": "Market Median Price",
"table.safetyRating": "Safety Rating",
"table.fuelEconomyCity": "Fuel Economy (City)",
"table.fuelEconomyHighway": "Fuel Economy (Highway)",
"table.recalls": "Recalls"
```

- [ ] **Step 4: Update frontend type**

In `GetListingByIdResponse.ts`, add optional fields:
```typescript
marketMedianPrice?: number;
marketListingCount?: number;
fuelEconomyMpgCity?: number;
fuelEconomyMpgHighway?: number;
safetyRating?: number;
recallCount?: number;
```

- [ ] **Step 5: Verify builds**

Run: `dotnet build ./Automotive.Marketplace.sln && cd automotive.marketplace.client && npm run build`
Expected: Both succeed.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: display external API data in comparison and listing details

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Group 7: Real-Time Dashboard

### Task 19: Create dashboard backend

**Files:**
- Create: `Automotive.Marketplace.Application/Features/DashboardFeatures/GetDashboardSummary/GetDashboardSummaryQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/DashboardFeatures/GetDashboardSummary/GetDashboardSummaryQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/DashboardFeatures/GetDashboardSummary/GetDashboardSummaryResponse.cs`
- Create: `Automotive.Marketplace.Server/Controllers/DashboardController.cs`

- [ ] **Step 1: Create GetDashboardSummaryResponse**

```csharp
namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public sealed record GetDashboardSummaryResponse
{
    public OfferSummary Offers { get; set; } = new();
    public MeetingSummary Meetings { get; set; } = new();
    public ContractSummary Contracts { get; set; } = new();
    public AvailabilitySummary Availability { get; set; } = new();

    public sealed record OfferSummary
    {
        public int PendingCount { get; set; }
        public string? NewestOfferListing { get; set; }
        public decimal? NewestOfferAmount { get; set; }
        public string? NewestOfferFrom { get; set; }
    }

    public sealed record MeetingSummary
    {
        public int UpcomingCount { get; set; }
        public DateTime? NextMeetingAt { get; set; }
        public string? NextMeetingCounterpart { get; set; }
        public string? NextMeetingListing { get; set; }
    }

    public sealed record ContractSummary
    {
        public int ActionNeededCount { get; set; }
        public string? NextActionListing { get; set; }
        public string? NextActionType { get; set; }
    }

    public sealed record AvailabilitySummary
    {
        public int PendingCount { get; set; }
    }
}
```

- [ ] **Step 2: Create GetDashboardSummaryQuery**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IRequest<GetDashboardSummaryResponse>
{
    public Guid CurrentUserId { get; set; }
}
```

- [ ] **Step 3: Create GetDashboardSummaryQueryHandler**

This handler queries multiple entities to build the summary. Follow the pattern from `GetSellerListingInsightsQueryHandler`:

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(IRepository repository)
    : IRequestHandler<GetDashboardSummaryQuery, GetDashboardSummaryResponse>
{
    public async Task<GetDashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.CurrentUserId;

        // Pending offers where user needs to respond (they are NOT the initiator)
        var pendingOffers = await repository.Query<Offer>()
            .Include(o => o.Conversation)
                .ThenInclude(c => c.Listing)
            .Include(o => o.Initiator)
            .Where(o => o.Status == OfferStatus.Pending
                && o.InitiatorId != userId
                && (o.Conversation.BuyerId == userId || o.Conversation.SellerId == userId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        // Upcoming confirmed meetings
        var upcomingMeetings = await repository.Query<Meeting>()
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Listing)
            .Where(m => m.Status == MeetingStatus.Accepted
                && m.ProposedAt > DateTime.UtcNow
                && (m.Conversation.BuyerId == userId || m.Conversation.SellerId == userId))
            .OrderBy(m => m.ProposedAt)
            .ToListAsync(cancellationToken);

        // Contracts needing action
        var actionContracts = await repository.Query<ContractCard>()
            .Include(c => c.Conversation)
                .ThenInclude(c => c.Listing)
            .Where(c => (c.Conversation.BuyerId == userId || c.Conversation.SellerId == userId)
                && (c.Status == ContractCardStatus.Pending
                    || c.Status == ContractCardStatus.Active
                    || c.Status == ContractCardStatus.SellerSubmitted
                    || c.Status == ContractCardStatus.BuyerSubmitted))
            .ToListAsync(cancellationToken);

        // Pending availability requests
        var pendingAvailability = await repository.Query<AvailabilityCard>()
            .Where(a => a.Status == AvailabilityStatus.Shared
                && a.InitiatorId != userId
                && (a.Conversation.BuyerId == userId || a.Conversation.SellerId == userId))
            .CountAsync(cancellationToken);

        var newestOffer = pendingOffers.FirstOrDefault();
        var nextMeeting = upcomingMeetings.FirstOrDefault();
        var nextContract = actionContracts.FirstOrDefault();

        return new GetDashboardSummaryResponse
        {
            Offers = new GetDashboardSummaryResponse.OfferSummary
            {
                PendingCount = pendingOffers.Count,
                NewestOfferListing = newestOffer?.Conversation?.Listing?.MakeName + " " + newestOffer?.Conversation?.Listing?.ModelName,
                NewestOfferAmount = newestOffer?.Amount,
                NewestOfferFrom = newestOffer?.Initiator?.UserName,
            },
            Meetings = new GetDashboardSummaryResponse.MeetingSummary
            {
                UpcomingCount = upcomingMeetings.Count,
                NextMeetingAt = nextMeeting?.ProposedAt,
                NextMeetingListing = nextMeeting?.Conversation?.Listing?.MakeName + " " + nextMeeting?.Conversation?.Listing?.ModelName,
            },
            Contracts = new GetDashboardSummaryResponse.ContractSummary
            {
                ActionNeededCount = actionContracts.Count,
                NextActionListing = nextContract?.Conversation?.Listing?.MakeName + " " + nextContract?.Conversation?.Listing?.ModelName,
            },
            Availability = new GetDashboardSummaryResponse.AvailabilitySummary
            {
                PendingCount = pendingAvailability,
            },
        };
    }
}
```

Note: Entity names (`Meeting`, `AvailabilityCard`, etc.) and their properties (`ProposedAt`, `Status`, etc.) and enum names (`MeetingStatus`, `AvailabilityStatus`) need to be verified against the actual domain entities. Adjust accordingly.

- [ ] **Step 4: Create DashboardController**

```csharp
using System.Security.Claims;
using Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new GetDashboardSummaryQuery
        {
            CurrentUserId = userId,
        }, cancellationToken);
        return Ok(result);
    }
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds. Fix any entity name mismatches.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: add dashboard summary backend query and controller

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 20: Add DashboardUpdated SignalR event

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

- [ ] **Step 1: Add DashboardUpdated notification to mutation methods**

In `ChatHub.cs`, after each mutation that affects the dashboard, notify the affected user(s). Add to the end of these methods:

- `MakeOffer` -- notify the recipient
- `RespondToOffer` -- notify both parties
- `CancelOffer` -- notify both parties
- `ProposeMeeting` -- notify the recipient
- `RespondToMeeting` -- notify both parties
- `CancelMeeting` -- notify both parties
- `ShareAvailability` -- notify the recipient
- `RespondToAvailability` -- notify both parties
- `RequestContract` -- notify the recipient
- `RespondToContract` -- notify both parties
- `CancelContract` -- notify both parties

The notification is lightweight -- just a signal to refresh:

```csharp
// Add this helper method to ChatHub:
private async Task NotifyDashboardUpdate(Guid userId)
{
    await Clients.Group($"user-{userId}").SendAsync("DashboardUpdated");
}
```

Then call `await NotifyDashboardUpdate(recipientId);` at the end of each relevant method. For methods that affect both parties, call it for both.

Example for `MakeOffer` (add after the existing NotifyUnreadCount call):
```csharp
await NotifyDashboardUpdate(result.RecipientId);
```

- [ ] **Step 2: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs
git commit -m "feat: add DashboardUpdated SignalR notifications

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 21: Create dashboard frontend

**Files:**
- Create: `automotive.marketplace.client/src/features/dashboard/components/Dashboard.tsx`
- Create: `automotive.marketplace.client/src/features/dashboard/components/DashboardTile.tsx`
- Create: `automotive.marketplace.client/src/features/dashboard/api/getDashboardSummaryOptions.ts`
- Create: `automotive.marketplace.client/src/features/dashboard/api/useDashboardHub.ts`
- Create: `automotive.marketplace.client/src/features/dashboard/types/GetDashboardSummaryResponse.ts`
- Create: `automotive.marketplace.client/src/features/dashboard/index.ts`
- Create: `automotive.marketplace.client/src/lib/i18n/locales/lt/dashboard.json`
- Create: `automotive.marketplace.client/src/lib/i18n/locales/en/dashboard.json`
- Modify: `automotive.marketplace.client/src/app/pages/MainPage.tsx`

- [ ] **Step 1: Create response type**

```typescript
// src/features/dashboard/types/GetDashboardSummaryResponse.ts
export type GetDashboardSummaryResponse = {
  offers: {
    pendingCount: number;
    newestOfferListing: string | null;
    newestOfferAmount: number | null;
    newestOfferFrom: string | null;
  };
  meetings: {
    upcomingCount: number;
    nextMeetingAt: string | null;
    nextMeetingCounterpart: string | null;
    nextMeetingListing: string | null;
  };
  contracts: {
    actionNeededCount: number;
    nextActionListing: string | null;
    nextActionType: string | null;
  };
  availability: {
    pendingCount: number;
  };
};
```

- [ ] **Step 2: Create query options**

```typescript
// src/features/dashboard/api/getDashboardSummaryOptions.ts
import { queryOptions } from "@tanstack/react-query";
import { axiosClient } from "@/lib/axios/axiosClient";
import type { GetDashboardSummaryResponse } from "../types/GetDashboardSummaryResponse";

export const getDashboardSummaryOptions = queryOptions({
  queryKey: ["dashboard-summary"],
  queryFn: async () => {
    const { data } = await axiosClient.get<GetDashboardSummaryResponse>(
      "/api/dashboard/summary",
    );
    return data;
  },
});
```

- [ ] **Step 3: Create useDashboardHub hook**

```typescript
// src/features/dashboard/api/useDashboardHub.ts
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";

export function useDashboardHub(connection: signalR.HubConnection | null) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!connection || connection.state !== HubConnectionState.Connected) return;

    const handler = () => {
      queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
    };

    connection.on("DashboardUpdated", handler);
    return () => {
      connection.off("DashboardUpdated", handler);
    };
  }, [connection, queryClient]);
}
```

Note: The SignalR connection is already established via `useChatHub` in the root layout. The dashboard hook needs access to this same connection. Check how the connection is exposed (likely via a context or returned from the hook). If `useChatHub` returns the connection, pass it to `useDashboardHub`. If it's in a context, consume it.

- [ ] **Step 4: Create DashboardTile component**

```tsx
// src/features/dashboard/components/DashboardTile.tsx
import type { ReactNode } from "react";

type DashboardTileProps = {
  icon: ReactNode;
  title: string;
  count: number;
  subtitle: string;
  detail: string;
  isHighlighted?: boolean;
  onClick?: () => void;
};

export function DashboardTile({
  icon,
  title,
  count,
  subtitle,
  detail,
  isHighlighted,
  onClick,
}: DashboardTileProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-lg border p-3 text-left transition-colors hover:bg-muted/50 ${
        isHighlighted ? "border-amber-500/50" : "border-border"
      }`}
    >
      <div className="mb-2 flex items-center justify-between">
        <span className="text-muted-foreground">{icon}</span>
        {count > 0 && (
          <span className="rounded-full bg-primary px-2 py-0.5 text-[10px] font-semibold text-primary-foreground">
            {count}
          </span>
        )}
      </div>
      <div className="text-sm font-semibold">{title}</div>
      <div className="text-muted-foreground text-xs">{subtitle}</div>
      {detail && (
        <div className="mt-1 truncate text-[10px] text-muted-foreground">{detail}</div>
      )}
    </button>
  );
}
```

- [ ] **Step 5: Create Dashboard component**

```tsx
// src/features/dashboard/components/Dashboard.tsx
import { useQuery } from "@tanstack/react-query";
import { Calendar, CircleDollarSign, FileText, Clock } from "lucide-react";
import { useTranslation } from "react-i18next";
import { getDashboardSummaryOptions } from "../api/getDashboardSummaryOptions";
import { DashboardTile } from "./DashboardTile";
import { formatCurrency } from "@/lib/i18n/formatNumber";

export function Dashboard() {
  const { t } = useTranslation("dashboard");
  const { data, isLoading } = useQuery(getDashboardSummaryOptions);

  if (isLoading) {
    return (
      <div className="mx-auto max-w-3xl animate-pulse space-y-3 py-4">
        <div className="bg-muted h-4 w-40 rounded" />
        <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="bg-muted h-24 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) return null;

  const totalActions =
    data.offers.pendingCount +
    data.meetings.upcomingCount +
    data.contracts.actionNeededCount +
    data.availability.pendingCount;

  if (totalActions === 0) return null;

  return (
    <div className="mx-auto max-w-3xl py-4">
      <h3 className="mb-3 text-base font-semibold">{t("title")}</h3>
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <DashboardTile
          icon={<CircleDollarSign className="h-5 w-5" />}
          title={t("offers.title")}
          count={data.offers.pendingCount}
          subtitle={
            data.offers.pendingCount > 0
              ? t("offers.awaiting", { count: data.offers.pendingCount })
              : t("offers.none")
          }
          detail={
            data.offers.newestOfferAmount
              ? `${formatCurrency(data.offers.newestOfferAmount)} € — ${data.offers.newestOfferFrom}`
              : "—"
          }
          isHighlighted={data.offers.pendingCount > 0}
        />
        <DashboardTile
          icon={<Calendar className="h-5 w-5" />}
          title={t("meetings.title")}
          count={data.meetings.upcomingCount}
          subtitle={
            data.meetings.nextMeetingAt
              ? t("meetings.next", { date: new Date(data.meetings.nextMeetingAt).toLocaleDateString() })
              : t("meetings.none")
          }
          detail={data.meetings.nextMeetingListing ?? "—"}
        />
        <DashboardTile
          icon={<FileText className="h-5 w-5" />}
          title={t("contracts.title")}
          count={data.contracts.actionNeededCount}
          subtitle={
            data.contracts.actionNeededCount > 0
              ? t("contracts.needsAction", { count: data.contracts.actionNeededCount })
              : t("contracts.none")
          }
          detail={data.contracts.nextActionListing ?? "—"}
        />
        <DashboardTile
          icon={<Clock className="h-5 w-5" />}
          title={t("availability.title")}
          count={data.availability.pendingCount}
          subtitle={
            data.availability.pendingCount > 0
              ? t("availability.pending", { count: data.availability.pendingCount })
              : t("availability.none")
          }
          detail="—"
        />
      </div>
    </div>
  );
}
```

- [ ] **Step 6: Create translations**

```json
// src/lib/i18n/locales/lt/dashboard.json
{
  "title": "Jūsų suvestinė",
  "offers": {
    "title": "Kainos pasiūlymai",
    "awaiting": "{{count}} laukia atsakymo",
    "none": "Nėra laukiančių"
  },
  "meetings": {
    "title": "Susitikimai",
    "next": "Kitas: {{date}}",
    "none": "Nėra artėjančių"
  },
  "contracts": {
    "title": "Sutartys",
    "needsAction": "{{count}} reikia veiksmo",
    "none": "Nėra laukiančių"
  },
  "availability": {
    "title": "Prieinamumas",
    "pending": "{{count}} laukia",
    "none": "Nėra užklausų"
  }
}
```

```json
// src/lib/i18n/locales/en/dashboard.json
{
  "title": "Your Dashboard",
  "offers": {
    "title": "Price Offers",
    "awaiting": "{{count}} awaiting response",
    "none": "No pending offers"
  },
  "meetings": {
    "title": "Meetings",
    "next": "Next: {{date}}",
    "none": "No upcoming meetings"
  },
  "contracts": {
    "title": "Contracts",
    "needsAction": "{{count}} need action",
    "none": "No pending contracts"
  },
  "availability": {
    "title": "Availability",
    "pending": "{{count}} pending",
    "none": "No requests"
  }
}
```

- [ ] **Step 7: Register translations in i18n config**

Check `src/lib/i18n/i18n.ts` and add the `dashboard` namespace to both `lt` and `en` resource configurations.

- [ ] **Step 8: Create feature index**

```typescript
// src/features/dashboard/index.ts
export { Dashboard } from "./components/Dashboard";
export { useDashboardHub } from "./api/useDashboardHub";
```

- [ ] **Step 9: Add Dashboard to MainPage**

```tsx
import { ListingSearch } from "@/features/search";
import { Dashboard } from "@/features/dashboard";
import { useAppSelector } from "@/hooks/redux";

const MainPage = () => {
  const userId = useAppSelector((state) => state.auth.userId);

  return (
    <div className="flex flex-col">
      {userId && <Dashboard />}
      <ListingSearch className={userId ? "mt-8 w-full" : "mt-16 w-full sm:mt-64"} />
    </div>
  );
};

export default MainPage;
```

- [ ] **Step 10: Wire up useDashboardHub**

In `__root.tsx` or wherever the SignalR connection is available, call `useDashboardHub(connection)` so the dashboard auto-refreshes on real-time events. This depends on how `useChatHub` exposes the connection.

- [ ] **Step 11: Verify builds**

Run: `dotnet build ./Automotive.Marketplace.sln && cd automotive.marketplace.client && npm run build`
Expected: Both succeed.

- [ ] **Step 12: Run all tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All tests pass.

- [ ] **Step 13: Commit**

```bash
git add -A
git commit -m "feat: add real-time dashboard to main page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Final Verification

### Task 22: Full build and test verification

- [ ] **Step 1: Full backend build and test**

```bash
dotnet build ./Automotive.Marketplace.sln
dotnet test ./Automotive.Marketplace.sln
```

- [ ] **Step 2: Full frontend build and lint**

```bash
cd automotive.marketplace.client
npm run build
npm run lint
npm run format:check
```

- [ ] **Step 3: Fix any issues found**

Address any build errors, test failures, or lint warnings.

- [ ] **Step 4: Final commit if any fixes were needed**

```bash
git add -A
git commit -m "chore: fix issues from final verification

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
