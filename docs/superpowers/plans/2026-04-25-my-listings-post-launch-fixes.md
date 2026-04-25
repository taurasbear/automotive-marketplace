# My Listings Post-Launch Fixes — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 7 issues in the My Listings seller interaction dashboard: authorization, card layout, empty conversation noise, liker chat engagement, ChatPanel context, tab labelling, and pending edit visibility.

**Architecture:** Two backend handler changes (conversation filtering, authorization), five frontend changes (card layout, panel behaviour, ChatPanel header, tab label, EditableField pending state). No new files — all changes are targeted edits to existing files.

**Tech Stack:** .NET 8 / EF Core / MediatR (backend); React 19 / TanStack Query v5 / shadcn/ui / Tailwind CSS / react-i18next (frontend). xUnit / TestContainers / FluentAssertions / Bogus builders (tests).

---

## File Map

| File | Change |
|------|--------|
| `Automotive.Marketplace.Server/Controllers/ListingController.cs` | Fix 1: Add `CreateListings` to `[Protect]` on `GetMy` and `GetEngagements` |
| `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs` | Fix 3a: Filter out conversations with no messages |
| `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs` | Fix 3b: Count only conversations with ≥1 message |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs` | Tests for Fix 3a |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetMyListingsQueryHandlerTests.cs` | Tests for Fix 3b |
| `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx` | Fix 2: Tighten badge grid gaps |
| `automotive.marketplace.client/src/features/myListings/components/ListingBuyerPanel.tsx` | Fix 4: `buyerHasEngaged: true` for likers; Fix 6: rename "Likes" tab to "Liked only" |
| `automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx` | Fix 5: Show listing title in panel header |
| `automotive.marketplace.client/src/features/myListings/components/EditableField.tsx` | Fix 7a: Accept and render `pendingValue` prop |
| `automotive.marketplace.client/src/features/myListings/components/MyListingDetail.tsx` | Fix 7b: Pass pending values to `EditableField` |

---

## Task 1: Backend — Fix 403 Unauthorized (Fix 1)

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Change the `[Protect]` attribute on `GetMy`**

Find the `GetMy` action (around line 85). It currently reads:
```csharp
[HttpGet]
[Protect(Permission.ManageListings)]
public async Task<ActionResult<IEnumerable<GetMyListingsResponse>>> GetMy(
```

Change to:
```csharp
[HttpGet]
[Protect(Permission.CreateListings, Permission.ManageListings)]
public async Task<ActionResult<IEnumerable<GetMyListingsResponse>>> GetMy(
```

- [ ] **Step 2: Change the `[Protect]` attribute on `GetEngagements`**

Find the `GetEngagements` action (around line 95). It currently reads:
```csharp
[HttpGet]
[Protect(Permission.ManageListings)]
public async Task<ActionResult<GetListingEngagementsResponse>> GetEngagements(
```

Change to:
```csharp
[HttpGet]
[Protect(Permission.CreateListings, Permission.ManageListings)]
public async Task<ActionResult<GetListingEngagementsResponse>> GetEngagements(
```

- [ ] **Step 3: Build to verify no errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "fix: allow CreateListings permission to access own listing endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: Backend — Filter Empty Conversations (Fix 3)

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetMyListingsQueryHandlerTests.cs`

### Part A: GetListingEngagements — exclude empty conversations

- [ ] **Step 1: Write a failing test for empty conversation exclusion in GetListingEngagements**

Add this test to `GetListingEngagementsQueryHandlerTests.cs` (after the existing `Handle_WithConversation_ShouldReturnConversationWithCorrectLastMessageType` test):

```csharp
[Fact]
public async Task Handle_WithConversationHavingNoMessages_ShouldNotAppearInConversations()
{
    // Arrange
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = CreateHandler(scope);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
    var (listingId, sellerId) = await SeedListingAsync(context);

    var buyer = new UserBuilder().Build();
    var emptyConversation = new ConversationBuilder().WithListing(listingId).WithBuyer(buyer.Id).Build();
    // No messages seeded

    await context.AddRangeAsync(buyer, emptyConversation);
    await context.SaveChangesAsync();

    var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Conversations.Should().BeEmpty();
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~Handle_WithConversationHavingNoMessages_ShouldNotAppearInConversations"
```

Expected: FAIL — the handler currently returns the empty conversation.

- [ ] **Step 3: Fix GetListingEngagementsQueryHandler to filter empty conversations**

In `GetListingEngagementsQueryHandler.cs`, add `.Where(c => c.Messages.Any())` to the conversations query. Replace the current conversations query:

```csharp
var conversations = await repository
    .AsQueryable<Conversation>()
    .Include(c => c.Messages)
    .Include(c => c.Buyer)
    .Where(c => c.ListingId == request.ListingId)
    .ToListAsync(cancellationToken);
```

With:

```csharp
var conversations = await repository
    .AsQueryable<Conversation>()
    .Include(c => c.Messages)
    .Include(c => c.Buyer)
    .Where(c => c.ListingId == request.ListingId && c.Messages.Any())
    .ToListAsync(cancellationToken);
```

- [ ] **Step 4: Run the new test to verify it passes**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~Handle_WithConversationHavingNoMessages_ShouldNotAppearInConversations"
```

Expected: PASS.

- [ ] **Step 5: Run all GetListingEngagements tests to verify nothing regressed**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingEngagementsQueryHandlerTests"
```

Expected: All PASS.

### Part B: GetMyListings — count only conversations with messages

- [ ] **Step 6: Write a failing test for ConversationCount excluding empty conversations**

Add this test to `GetMyListingsQueryHandlerTests.cs` (after the existing `Handle_ListingWithConversations_ShouldPopulateConversationCount` test):

```csharp
[Fact]
public async Task Handle_ListingWithMixedConversations_ShouldCountOnlyConversationsWithMessages()
{
    // Arrange
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = CreateHandler(scope);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var make = new MakeBuilder().Build();
    var model = new ModelBuilder().WithMake(make.Id).Build();
    var fuel = new FuelBuilder().Build();
    var transmission = new TransmissionBuilder().Build();
    var bodyType = new BodyTypeBuilder().Build();
    var drivetrain = new DrivetrainBuilder().Build();
    var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
    var seller = new UserBuilder().Build();
    var buyer1 = new UserBuilder().Build();
    var buyer2 = new UserBuilder().Build();
    var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
    // conversation with a message
    var conversationWithMessage = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer1.Id).Build();
    var message = new MessageBuilder().WithConversation(conversationWithMessage.Id).WithSender(buyer1.Id).Build();
    // conversation with no messages (buyer opened chat but sent nothing)
    var emptyConversation = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer2.Id).Build();

    await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, buyer1, buyer2, listing);
    await context.AddRangeAsync(conversationWithMessage, message, emptyConversation);
    await context.SaveChangesAsync();

    var query = new GetMyListingsQuery { SellerId = seller.Id };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Single().ConversationCount.Should().Be(1);
}
```

- [ ] **Step 7: Run test to verify it fails**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~Handle_ListingWithMixedConversations_ShouldCountOnlyConversationsWithMessages"
```

Expected: FAIL — handler currently counts 2 (includes the empty conversation).

- [ ] **Step 8: Update the existing `Handle_ListingWithConversations_ShouldPopulateConversationCount` test to add messages**

This test will now fail because empty conversations are excluded. Update it to add a message to each conversation so the test remains valid:

```csharp
[Fact]
public async Task Handle_ListingWithConversations_ShouldPopulateConversationCount()
{
    // Arrange
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = CreateHandler(scope);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var make = new MakeBuilder().Build();
    var model = new ModelBuilder().WithMake(make.Id).Build();
    var fuel = new FuelBuilder().Build();
    var transmission = new TransmissionBuilder().Build();
    var bodyType = new BodyTypeBuilder().Build();
    var drivetrain = new DrivetrainBuilder().Build();
    var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
    var seller = new UserBuilder().Build();
    var buyer1 = new UserBuilder().Build();
    var buyer2 = new UserBuilder().Build();
    var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
    var conv1 = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer1.Id).Build();
    var conv2 = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer2.Id).Build();
    var msg1 = new MessageBuilder().WithConversation(conv1.Id).WithSender(buyer1.Id).Build();
    var msg2 = new MessageBuilder().WithConversation(conv2.Id).WithSender(buyer2.Id).Build();
    await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, buyer1, buyer2, listing);
    await context.AddRangeAsync(conv1, conv2, msg1, msg2);
    await context.SaveChangesAsync();

    var query = new GetMyListingsQuery { SellerId = seller.Id };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Single().ConversationCount.Should().Be(2);
}
```

- [ ] **Step 9: Fix GetMyListingsQueryHandler to count only conversations with messages**

Open `GetMyListingsQueryHandler.cs`. The current handler includes `.Include(l => l.Conversations)` and sets `mappedListing.ConversationCount = listing.Conversations.Count`.

Replace the existing query (which includes `.Include(l => l.Conversations)`) and the count assignment with a separate efficient count query. The complete updated handler:

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public class GetMyListingsQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<GetMyListingsQuery, IEnumerable<GetMyListingsResponse>>
{
    public async Task<IEnumerable<GetMyListingsResponse>> Handle(GetMyListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Fuel)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Transmission)
            .Include(l => l.Images)
            .Include(l => l.Defects)
            .Include(l => l.Likes)
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        // Count only conversations that have at least one message (exclude ghost conversations
        // created when a buyer opened the chat panel but never sent anything)
        var listingIds = listings.Select(l => l.Id).ToList();
        var conversationCounts = await repository
            .AsQueryable<Conversation>()
            .Where(c => listingIds.Contains(c.ListingId) && c.Messages.Any())
            .GroupBy(c => c.ListingId)
            .Select(g => new { ListingId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var countByListingId = conversationCounts.ToDictionary(x => x.ListingId, x => x.Count);

        List<GetMyListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetMyListingsResponse>(listing);

            var nonDefectImages = listing.Images.Where(i => i.ListingDefectId == null).ToList();

            // All non-defect images for hover gallery
            var images = new List<ImageDto>();
            foreach (var image in nonDefectImages)
            {
                images.Add(new ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                    AltText = image.AltText
                });
            }
            mappedListing.Images = images;

            // Thumbnail reuses the already-signed first image — no second S3 call
            mappedListing.Thumbnail = images.FirstOrDefault();

            // Counts
            mappedListing.ImageCount = listing.Images.Count;
            mappedListing.DefectCount = listing.Defects.Count;
            mappedListing.LikeCount = listing.Likes.Count;
            mappedListing.ConversationCount = countByListingId.GetValueOrDefault(listing.Id, 0);

            response.Add(mappedListing);
        }

        return response;
    }
}
```

- [ ] **Step 10: Run all GetMyListings tests to verify both pass**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetMyListingsQueryHandlerTests"
```

Expected: All PASS.

- [ ] **Step 11: Run full test suite to verify nothing regressed**

```bash
dotnet test ./Automotive.Marketplace.sln
```

Expected: All PASS.

- [ ] **Step 12: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs \
        Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs \
        Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetMyListingsQueryHandlerTests.cs
git commit -m "fix: exclude empty conversations from engagement counts and seller panel

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: Frontend — Tighten MyListingCard Layout (Fix 2)

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`

- [ ] **Step 1: Tighten the badge grid**

In `MyListingCard.tsx`, find the badge grid `<div>` (line ~123):

```tsx
<div className="grid grid-cols-2 gap-x-0 gap-y-4">
```

Change to:

```tsx
<div className="justify-items-stretched grid grid-cols-2 gap-x-0 gap-y-2">
```

Two changes: `gap-y-4` → `gap-y-2` (halves the vertical gap between badge rows), and adds `justify-items-stretched` to match `ListingCard`.

- [ ] **Step 2: Commit**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
cd ..
git add automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx
git commit -m "fix: reduce MyListingCard badge grid gap to match ListingCard

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: Frontend — Fix ListingBuyerPanel (Fix 4 + Fix 6)

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/ListingBuyerPanel.tsx`

### Fix 4: Let seller send offers to likers

- [ ] **Step 1: Set `buyerHasEngaged: true` in `handleOpenLikerChat`**

In `ListingBuyerPanel.tsx`, find `handleOpenLikerChat` (line ~133). Change:

```tsx
const handleOpenLikerChat = async (liker: ListingLikerEngagement) => {
  try {
    const res = await getOrCreate({ listingId, buyerId: liker.userId });
    onStartChat({
      id: res.data.conversationId,
      listingId,
      listingTitle,
      listingThumbnail,
      listingPrice,
      counterpartId: liker.userId,
      counterpartUsername: liker.username,
      lastMessage: null,
      lastMessageAt: new Date().toISOString(),
      unreadCount: 0,
      buyerId: liker.userId,
      sellerId,
      buyerHasEngaged: false,
    });
  } catch {
    // error already handled via mutation meta toast
  }
};
```

To:

```tsx
const handleOpenLikerChat = async (liker: ListingLikerEngagement) => {
  try {
    const res = await getOrCreate({ listingId, buyerId: liker.userId });
    onStartChat({
      id: res.data.conversationId,
      listingId,
      listingTitle,
      listingThumbnail,
      listingPrice,
      counterpartId: liker.userId,
      counterpartUsername: liker.username,
      lastMessage: null,
      lastMessageAt: new Date().toISOString(),
      unreadCount: 0,
      buyerId: liker.userId,
      sellerId,
      buyerHasEngaged: true,
    });
  } catch {
    // error already handled via mutation meta toast
  }
};
```

### Fix 6: Rename the "Likes" tab to "Liked only"

- [ ] **Step 2: Rename the `TabsTrigger` for the likes tab**

In `ListingBuyerPanel.tsx`, find the `TabsTrigger` for "likes" (line ~187):

```tsx
<TabsTrigger value="likes">
  {t("buyerPanel.likes")} ({likers.length})
</TabsTrigger>
```

Change to:

```tsx
<TabsTrigger value="likes">
  {t("buyerPanel.likedOnly")} ({likers.length})
</TabsTrigger>
```

- [ ] **Step 3: Add the `likedOnly` translation key**

Open `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`. In the `buyerPanel` section, add `"likedOnly"` after the existing `"likes"` key:

```json
"likes": "Likes",
"likedOnly": "Liked only",
```

Open `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`. In the `buyerPanel` section, add `"likedOnly"` after the existing `"likes"` key:

```json
"likes": "Patiktukai",
"likedOnly": "Tik patiko",
```

- [ ] **Step 4: Lint and commit**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
cd ..
git add automotive.marketplace.client/src/features/myListings/components/ListingBuyerPanel.tsx \
        automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json \
        automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json
git commit -m "fix: allow seller to send offers to likers; rename Likes tab to Liked only

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: Frontend — Show Listing Title in ChatPanel (Fix 5)

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx`

- [ ] **Step 1: Add the listing title subtitle to the ChatPanel header**

In `ChatPanel.tsx`, the current header is:

```tsx
<div className="border-border flex items-center gap-3 border-b px-4 py-3">
  <p className="flex-1 text-sm font-semibold">
    {conversation.counterpartUsername}
  </p>
  <Button
    variant="ghost"
    size="sm"
    onClick={onClose}
    aria-label={t("chatPanel.closeChat")}
    className="text-muted-foreground hover:text-foreground h-auto p-1 leading-none"
  >
    ✕
  </Button>
</div>
```

Change to:

```tsx
<div className="border-border flex items-center gap-3 border-b px-4 py-3">
  <div className="flex-1 min-w-0">
    <p className="text-sm font-semibold truncate">
      {conversation.counterpartUsername}
    </p>
    {conversation.listingTitle && (
      <p className="text-muted-foreground truncate text-xs">
        {conversation.listingTitle}
      </p>
    )}
  </div>
  <Button
    variant="ghost"
    size="sm"
    onClick={onClose}
    aria-label={t("chatPanel.closeChat")}
    className="text-muted-foreground hover:text-foreground h-auto p-1 leading-none"
  >
    ✕
  </Button>
</div>
```

- [ ] **Step 2: Lint and commit**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
cd ..
git add automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx
git commit -m "fix: show listing title in ChatPanel header for context

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: Frontend — Show Pending Changes in EditableField (Fix 7)

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/EditableField.tsx`
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingDetail.tsx`

### Part A: Update EditableField to accept and render pendingValue

- [ ] **Step 1: Add `pendingValue` prop to `EditableField`**

Open `EditableField.tsx`. Update the props type to include `pendingValue?` and update the display-mode render to show the pending change indicator. Replace the entire file content with:

```tsx
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Toggle } from "@/components/ui/toggle";
import { formatNumber } from "@/lib/i18n/formatNumber";
import { Check, Pencil, X } from "lucide-react";
import { useState } from "react";

type EditableFieldProps = {
  label: string;
  value: string | number | boolean;
  pendingValue?: string | number | boolean;
  displayValue?: string;
  type: "text" | "number" | "textarea" | "toggle";
  toggleLabels?: { on: string; off: string };
  onConfirm: (newValue: string | number | boolean) => void;
};

const EditableField = ({
  label,
  value,
  pendingValue,
  displayValue,
  type,
  toggleLabels,
  onConfirm,
}: EditableFieldProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(value);

  const handleEdit = () => {
    setEditValue(pendingValue !== undefined ? pendingValue : value);
    setIsEditing(true);
  };

  const handleCancel = () => {
    setEditValue(value);
    setIsEditing(false);
  };

  const handleConfirm = () => {
    onConfirm(editValue);
    setIsEditing(false);
  };

  const formatDisplayValue = (v: string | number | boolean) => {
    if (displayValue && v === value) return displayValue;
    if (type === "toggle" && toggleLabels) {
      return v ? toggleLabels.on : toggleLabels.off;
    }
    if (type === "number" && typeof v === "number") {
      return formatNumber(v);
    }
    return String(v);
  };

  const hasPendingChange =
    pendingValue !== undefined && pendingValue !== value;

  if (isEditing) {
    return (
      <div className="space-y-2 py-2">
        <label className="text-muted-foreground text-sm">{label}</label>
        <div className="flex items-center gap-2">
          {type === "text" && (
            <Input
              value={String(editValue)}
              onChange={(e) => setEditValue(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
            />
          )}
          {type === "number" && (
            <Input
              type="number"
              value={String(editValue)}
              onChange={(e) => setEditValue(Number(e.target.value) || 0)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
            />
          )}
          {type === "textarea" && (
            <Textarea
              value={String(editValue)}
              onChange={(e) => setEditValue(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter" && e.metaKey) handleConfirm();
                if (e.key === "Escape") handleCancel();
              }}
              autoFocus
              className="flex-1"
              rows={3}
            />
          )}
          {type === "toggle" && toggleLabels && (
            <div className="flex flex-1 items-center gap-2">
              <Toggle
                pressed={Boolean(editValue)}
                onPressedChange={setEditValue}
                variant="outline"
              >
                {editValue ? toggleLabels.on : toggleLabels.off}
              </Toggle>
            </div>
          )}
          <Button size="sm" onClick={handleConfirm} variant="outline">
            <Check className="h-4 w-4" />
          </Button>
          <Button size="sm" onClick={handleCancel} variant="outline">
            <X className="h-4 w-4" />
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-start justify-between py-2">
      <div>
        <div className="text-muted-foreground text-sm">{label}</div>
        {hasPendingChange ? (
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground line-through">
              {formatDisplayValue(value)}
            </span>
            <span className="text-amber-600 font-medium">
              {formatDisplayValue(pendingValue!)}
            </span>
          </div>
        ) : (
          <div className="font-medium">{formatDisplayValue(value)}</div>
        )}
      </div>
      <Button size="sm" variant="ghost" onClick={handleEdit}>
        <Pencil className="h-4 w-4" />
      </Button>
    </div>
  );
};

export default EditableField;
```

### Part B: Pass pending values from MyListingDetail

- [ ] **Step 2: Pass `pendingValue` to each `EditableField` in `MyListingDetail.tsx`**

In `MyListingDetail.tsx`, find the Editable Details card section (around line 227). Each `EditableField` needs a `pendingValue` prop that reads from `pendingChanges`. Update each one:

```tsx
<EditableField
  label={t("fields.price")}
  value={listing.price}
  pendingValue={pendingChanges.price as number | undefined}
  type="number"
  onConfirm={(value) => handleFieldChange("price", value)}
/>
<EditableField
  label={t("fields.city")}
  value={listing.city}
  pendingValue={pendingChanges.city as string | undefined}
  type="text"
  onConfirm={(value) => handleFieldChange("city", value)}
/>
<EditableField
  label={t("fields.mileage")}
  value={listing.mileage}
  pendingValue={pendingChanges.mileage as number | undefined}
  type="number"
  onConfirm={(value) => handleFieldChange("mileage", value)}
/>
<EditableField
  label={t("fields.description")}
  value={listing.description || ""}
  pendingValue={pendingChanges.description as string | undefined}
  type="textarea"
  onConfirm={(value) => handleFieldChange("description", value)}
/>
<EditableField
  label={t("fields.colour")}
  value={listing.colour || ""}
  pendingValue={pendingChanges.colour as string | undefined}
  type="text"
  onConfirm={(value) => handleFieldChange("colour", value)}
/>
<EditableField
  label={t("fields.vin")}
  value={listing.vin || ""}
  pendingValue={pendingChanges.vin as string | undefined}
  type="text"
  onConfirm={(value) => handleFieldChange("vin", value)}
/>
<EditableField
  label={t("fields.isUsed")}
  value={listing.isUsed}
  pendingValue={pendingChanges.isUsed as boolean | undefined}
  type="toggle"
  toggleLabels={{ on: t("fields.used"), off: t("fields.new") }}
  onConfirm={(value) => handleFieldChange("isUsed", value)}
/>
<EditableField
  label={t("fields.steeringWheel")}
  value={listing.isSteeringWheelRight}
  pendingValue={pendingChanges.isSteeringWheelRight as boolean | undefined}
  type="toggle"
  toggleLabels={{ on: t("fields.right"), off: t("fields.left") }}
  onConfirm={(value) =>
    handleFieldChange("isSteeringWheelRight", value)
  }
/>
```

- [ ] **Step 3: Lint and commit**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
cd ..
git add automotive.marketplace.client/src/features/myListings/components/EditableField.tsx \
        automotive.marketplace.client/src/features/myListings/components/MyListingDetail.tsx
git commit -m "fix: show pending field changes in EditableField with strikethrough

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Final Verification

- [ ] **Run full backend test suite**

```bash
dotnet test ./Automotive.Marketplace.sln
```

Expected: All PASS.

- [ ] **Run frontend lint + build**

```bash
cd automotive.marketplace.client && npm run build
```

Expected: `Build succeeded` with no errors.
