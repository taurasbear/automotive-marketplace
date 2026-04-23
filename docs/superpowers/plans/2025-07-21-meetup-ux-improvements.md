# Meetup UX Improvements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add 7 UX improvements to the meetup planning feature: cancel actions, inline slot picker, timezone display, suggest alternative, confirmed meeting guard, sticky meeting bar, and inbox URL routing.

**Architecture:** Backend follows Clean Architecture + CQRS via MediatR. Two new command handlers (CancelMeeting, CancelAvailability), modifications to RespondToAvailability for slot picker. Frontend is React 19 + TanStack Router + shadcn/ui + SignalR for real-time. All changes build on the existing meetup planning feature.

**Tech Stack:** ASP.NET Core 8, MediatR, EF Core + PostgreSQL, SignalR, React 19, TypeScript, TanStack Router/Query, shadcn/ui, date-fns, lucide-react

---

## File Structure

### Backend (new files)
| File | Responsibility |
|------|---------------|
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommand.cs` | Command record with MeetingId + CancellerId |
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommandHandler.cs` | Validates participant + status rules (Pending=initiator only, Accepted=either participant), sets Cancelled |
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingResponse.cs` | Response with MeetingId, ConversationId, NewStatus, participant IDs |
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommand.cs` | Command record with AvailabilityCardId + CancellerId |
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommandHandler.cs` | Validates initiator + Pending status, sets Cancelled |
| `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityResponse.cs` | Response with CardId, ConversationId, NewStatus, participant IDs |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelMeetingCommandHandlerTests.cs` | Integration tests for CancelMeeting |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelAvailabilityCommandHandlerTests.cs` | Integration tests for CancelAvailability |

### Backend (modified files)
| File | Change |
|------|--------|
| `Automotive.Marketplace.Domain/Enums/MeetingStatus.cs` | Add `Cancelled = 5` |
| `Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs` | Add `Cancelled = 3` |
| `Automotive.Marketplace.Server/Hubs/ChatHub.cs` | Add CancelMeeting + CancelAvailability hub methods, add startTime/durationMinutes to RespondToAvailability |
| `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommand.cs` | Add `StartTime` + `DurationMinutes` fields |
| `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommandHandler.cs` | Use provided StartTime/Duration for PickSlot, validate min 15 min |

### Frontend (new files)
| File | Responsibility |
|------|---------------|
| `automotive.marketplace.client/src/features/chat/utils/timezone.ts` | `getTimezoneOffsetLabel()` helper |
| `automotive.marketplace.client/src/app/routes/inbox/index.tsx` | Index route for bare /inbox (no conversation selected) |
| `automotive.marketplace.client/src/app/routes/inbox/$conversationId.tsx` | Parameterized route for /inbox/:conversationId |

### Frontend (modified files)
| File | Change |
|------|--------|
| `automotive.marketplace.client/src/features/chat/types/Meeting.ts` | Add `'Cancelled'` to MeetingStatus |
| `automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts` | Add `'Cancelled'` to AvailabilityCardStatus |
| `automotive.marketplace.client/src/features/chat/types/MeetingEventPayloads.ts` | Add MeetingCancelledPayload + AvailabilityCancelledPayload |
| `automotive.marketplace.client/src/features/chat/constants/chatHub.ts` | Add CANCEL_MEETING, CANCEL_AVAILABILITY, MEETING_CANCELLED, AVAILABILITY_CANCELLED |
| `automotive.marketplace.client/src/features/chat/api/useChatHub.ts` | Add cancelMeeting + cancelAvailability functions + event listeners |
| `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx` | Cancel button, suggest alternative popover, cancelled status, timezone |
| `automotive.marketplace.client/src/features/chat/components/AvailabilityCardComponent.tsx` | Cancel button, inline slot picker, cancelled status, timezone |
| `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx` | Local time input + timezone label |
| `automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx` | Local time input + timezone label |
| `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx` | Confirmed meeting guard (AlertDialog) |
| `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx` | Sticky bar, new cancel props, suggest-alternative wiring |
| `automotive.marketplace.client/src/app/pages/Inbox.tsx` | Accept conversationId prop, pass to ConversationList |
| `automotive.marketplace.client/src/features/chat/components/ConversationList.tsx` | Navigate on select, auto-select from URL param |
| `automotive.marketplace.client/src/app/routes/inbox.tsx` | Convert to layout route (parent for $conversationId) |

---

## Tasks

### Task 1: Add Cancelled enum values

**Files:**
- Modify: `Automotive.Marketplace.Domain/Enums/MeetingStatus.cs`
- Modify: `Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs`

- [ ] **Step 1: Add Cancelled to MeetingStatus**

```csharp
// Automotive.Marketplace.Domain/Enums/MeetingStatus.cs
namespace Automotive.Marketplace.Domain.Enums;

public enum MeetingStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Rescheduled = 3,
    Expired = 4,
    Cancelled = 5,
}
```

- [ ] **Step 2: Add Cancelled to AvailabilityCardStatus**

```csharp
// Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs
namespace Automotive.Marketplace.Domain.Enums;

public enum AvailabilityCardStatus
{
    Pending = 0,
    Responded = 1,
    Expired = 2,
    Cancelled = 3,
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -q`
Expected: Build succeeded

- [ ] **Step 4: Run all existing tests to verify no regressions**

Run: `dotnet test ./Automotive.Marketplace.sln --no-build -q`
Expected: All 114 tests pass

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Domain/Enums/MeetingStatus.cs Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs
git commit -m "feat: add Cancelled status to MeetingStatus and AvailabilityCardStatus enums"
```

---

### Task 2: CancelMeeting command, handler, and response

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingResponse.cs`
- Test: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelMeetingCommandHandlerTests.cs`

- [ ] **Step 1: Create CancelMeetingCommand**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public sealed record CancelMeetingCommand : IRequest<CancelMeetingResponse>
{
    public Guid MeetingId { get; set; }

    public Guid CancellerId { get; set; }
}
```

- [ ] **Step 2: Create CancelMeetingResponse**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public sealed record CancelMeetingResponse
{
    public Guid MeetingId { get; set; }

    public Guid ConversationId { get; set; }

    public MeetingStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid RecipientId { get; set; }
}
```

- [ ] **Step 3: Write failing tests for CancelMeetingCommandHandler**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelMeetingCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class CancelMeetingCommandHandlerTests(
    DatabaseFixture<CancelMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CancelMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CancelMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private CancelMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_InitiatorCancelsPendingMeeting_ShouldSetStatusToCancelled()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MeetingId.Should().Be(meeting.Id);
        result.ConversationId.Should().Be(conversation.Id);
        result.NewStatus.Should().Be(MeetingStatus.Cancelled);
        result.InitiatorId.Should().Be(buyer.Id);

        var saved = await context.Meetings.FindAsync(meeting.Id);
        saved!.Status.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_InitiatorCancelsAcceptedMeeting_ShouldSetStatusToCancelled()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Accepted)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_NonInitiatorCancelsPending_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = seller.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ResponderCancelsAcceptedMeeting_ShouldSucceed()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Accepted)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = seller.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Cancelled);

        var saved = await context.Meetings.FindAsync(meeting.Id);
        saved!.Status.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_MeetingAlreadyDeclined_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Declined)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("MeetingId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyCancels_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var thirdParty = Guid.NewGuid();
        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = thirdParty
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Listing listing)>
        SeedConversationAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~CancelMeetingCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: FAIL — `CancelMeetingCommandHandler` does not exist yet

- [ ] **Step 5: Implement CancelMeetingCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/CancelMeetingCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public class CancelMeetingCommandHandler(IRepository repository)
    : IRequestHandler<CancelMeetingCommand, CancelMeetingResponse>
{
    public async Task<CancelMeetingResponse> Handle(
        CancelMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync<Meeting>(
            request.MeetingId, cancellationToken);
        var conversation = meeting.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.CancellerId == conversation.BuyerId
            || request.CancellerId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only conversation participants may cancel a meeting.");

        if (meeting.Status != MeetingStatus.Pending && meeting.Status != MeetingStatus.Accepted)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId",
                    "Only pending or accepted meetings can be cancelled.")
            ]);

        // Pending meetings can only be cancelled by the initiator
        if (meeting.Status == MeetingStatus.Pending && meeting.InitiatorId != request.CancellerId)
            throw new UnauthorizedAccessException(
                "Only the meeting initiator may cancel a pending meeting.");

        meeting.Status = MeetingStatus.Cancelled;
        await repository.UpdateAsync(meeting, cancellationToken);

        var recipientId = request.CancellerId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        return new CancelMeetingResponse
        {
            MeetingId = meeting.Id,
            ConversationId = conversation.Id,
            NewStatus = MeetingStatus.Cancelled,
            InitiatorId = meeting.InitiatorId,
            RecipientId = recipientId
        };
    }
}
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~CancelMeetingCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: All 7 tests PASS

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/CancelMeeting/ Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelMeetingCommandHandlerTests.cs
git commit -m "feat: add CancelMeeting CQRS command with tests"
```

---

### Task 3: CancelAvailability command, handler, and response

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityResponse.cs`
- Test: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelAvailabilityCommandHandlerTests.cs`

- [ ] **Step 1: Create CancelAvailabilityCommand**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public sealed record CancelAvailabilityCommand : IRequest<CancelAvailabilityResponse>
{
    public Guid AvailabilityCardId { get; set; }

    public Guid CancellerId { get; set; }
}
```

- [ ] **Step 2: Create CancelAvailabilityResponse**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public sealed record CancelAvailabilityResponse
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ConversationId { get; set; }

    public AvailabilityCardStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid RecipientId { get; set; }
}
```

- [ ] **Step 3: Write failing tests for CancelAvailabilityCommandHandler**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelAvailabilityCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class CancelAvailabilityCommandHandlerTests(
    DatabaseFixture<CancelAvailabilityCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CancelAvailabilityCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CancelAvailabilityCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private CancelAvailabilityCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_InitiatorCancelsPendingCard_ShouldSetStatusToCancelled()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var command = new CancelAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            CancellerId = buyer.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.AvailabilityCardId.Should().Be(card.Id);
        result.ConversationId.Should().Be(conversation.Id);
        result.NewStatus.Should().Be(AvailabilityCardStatus.Cancelled);

        var saved = await context.AvailabilityCards.FindAsync(card.Id);
        saved!.Status.Should().Be(AvailabilityCardStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_NonInitiatorCancels_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var command = new CancelAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            CancellerId = seller.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CardAlreadyResponded_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(AvailabilityCardStatus.Responded)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var command = new CancelAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            CancellerId = buyer.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("AvailabilityCardId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyCancels_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var command = new CancelAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            CancellerId = Guid.NewGuid()
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Listing listing)>
        SeedConversationAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~CancelAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: FAIL — `CancelAvailabilityCommandHandler` does not exist yet

- [ ] **Step 5: Implement CancelAvailabilityCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/CancelAvailabilityCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public class CancelAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<CancelAvailabilityCommand, CancelAvailabilityResponse>
{
    public async Task<CancelAvailabilityResponse> Handle(
        CancelAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<AvailabilityCard>(
            request.AvailabilityCardId, cancellationToken);
        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.CancellerId == conversation.BuyerId
            || request.CancellerId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only conversation participants may cancel an availability card.");

        if (card.InitiatorId != request.CancellerId)
            throw new UnauthorizedAccessException(
                "Only the availability card initiator may cancel it.");

        if (card.Status != AvailabilityCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId",
                    "Only pending availability cards can be cancelled.")
            ]);

        card.Status = AvailabilityCardStatus.Cancelled;
        await repository.UpdateAsync(card, cancellationToken);

        var recipientId = card.InitiatorId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        return new CancelAvailabilityResponse
        {
            AvailabilityCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = AvailabilityCardStatus.Cancelled,
            InitiatorId = card.InitiatorId,
            RecipientId = recipientId
        };
    }
}
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~CancelAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: All 4 tests PASS

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/CancelAvailability/ Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelAvailabilityCommandHandlerTests.cs
git commit -m "feat: add CancelAvailability CQRS command with tests"
```

---

### Task 4: Add CancelMeeting and CancelAvailability hub methods

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

- [ ] **Step 1: Add using statements and hub methods**

Add two new methods to ChatHub. Insert after the `RespondToAvailability` method (after line 151):

```csharp
// Add to the top using section:
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

// Add these methods at the end of the ChatHub class, before the closing brace:

    public async Task CancelMeeting(Guid meetingId)
    {
        var result = await mediator.Send(new CancelMeetingCommand
        {
            MeetingId = meetingId,
            CancellerId = UserId
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("MeetingCancelled", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("MeetingCancelled", result);
    }

    public async Task CancelAvailability(Guid availabilityCardId)
    {
        var result = await mediator.Send(new CancelAvailabilityCommand
        {
            AvailabilityCardId = availabilityCardId,
            CancellerId = UserId
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("AvailabilityCancelled", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("AvailabilityCancelled", result);
    }
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -q`
Expected: Build succeeded

- [ ] **Step 3: Run all tests to verify no regressions**

Run: `dotnet test ./Automotive.Marketplace.sln -q`
Expected: All tests pass

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs
git commit -m "feat: add CancelMeeting and CancelAvailability SignalR hub methods"
```

---

### Task 5: Modify RespondToAvailability for slot picker (StartTime + DurationMinutes)

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommandHandler.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToAvailabilityCommandHandlerTests.cs`

- [ ] **Step 1: Add StartTime and DurationMinutes to command**

Add two new properties to `RespondToAvailabilityCommand`, after the `SlotId` property:

```csharp
    public DateTime? StartTime { get; set; }

    public int? DurationMinutes { get; set; }
```

- [ ] **Step 2: Write failing test for custom start time + duration on PickSlot**

Add this test to the existing `RespondToAvailabilityCommandHandlerTests` class:

```csharp
    [Fact]
    public async Task Handle_PickSlotWithCustomStartTimeAndDuration_ShouldUseProvidedValues()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var slotStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var slotEnd = slotStart.AddHours(4);
        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(seller.Id)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var slot = new AvailabilitySlotBuilder()
            .WithAvailabilityCard(card.Id)
            .WithStartTime(slotStart)
            .WithEndTime(slotEnd)
            .Build();
        await context.AddAsync(slot);
        await context.SaveChangesAsync();

        var customStart = slotStart.AddHours(1);
        var command = new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id,
            StartTime = customStart,
            DurationMinutes = 30
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.PickedSlotMeeting.Should().NotBeNull();
        result.PickedSlotMeeting!.Meeting.ProposedAt.Should().Be(customStart);
        result.PickedSlotMeeting.Meeting.DurationMinutes.Should().Be(30);
    }

    [Fact]
    public async Task Handle_PickSlotWithStartTimeOutsideRange_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var slotStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var slotEnd = slotStart.AddHours(2);
        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(seller.Id)
            .Build();
        await context.AddAsync(card);
        await context.SaveChangesAsync();

        var slot = new AvailabilitySlotBuilder()
            .WithAvailabilityCard(card.Id)
            .WithStartTime(slotStart)
            .WithEndTime(slotEnd)
            .Build();
        await context.AddAsync(slot);
        await context.SaveChangesAsync();

        var command = new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id,
            StartTime = slotStart.AddHours(-1),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("StartTime"));
    }
```

- [ ] **Step 3: Run new tests to verify they fail**

Run: `dotnet test --filter "FullyQualifiedName~RespondToAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: New tests FAIL (handler uses full slot range, doesn't validate custom range)

- [ ] **Step 4: Update handler to use StartTime + DurationMinutes when provided**

In `RespondToAvailabilityCommandHandler.cs`, replace the meeting creation in the PickSlot branch (lines 69–80 approximately). Find the lines:

```csharp
            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                InitiatorId = request.ResponderId,
                ProposedAt = slot.StartTime,
                DurationMinutes = (int)(slot.EndTime - slot.StartTime).TotalMinutes,
```

Replace with logic that uses the custom values when provided, and validates them:

```csharp
            var meetingStart = request.StartTime ?? slot.StartTime;
            var meetingDuration = request.DurationMinutes ?? (int)(slot.EndTime - slot.StartTime).TotalMinutes;

            if (meetingDuration < 15)
                throw new RequestValidationException(
                [
                    new ValidationFailure("DurationMinutes", "Meeting duration must be at least 15 minutes.")
                ]);

            if (request.StartTime.HasValue || request.DurationMinutes.HasValue)
            {
                if (meetingStart < slot.StartTime)
                    throw new RequestValidationException(
                    [
                        new ValidationFailure("StartTime", "Start time must be within the selected slot range.")
                    ]);

                var meetingEnd = meetingStart.AddMinutes(meetingDuration);
                if (meetingEnd > slot.EndTime)
                    throw new RequestValidationException(
                    [
                        new ValidationFailure("StartTime", "Meeting end time must not exceed the slot end time.")
                    ]);
            }

            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                InitiatorId = request.ResponderId,
                ProposedAt = meetingStart,
                DurationMinutes = meetingDuration,
```

Everything after `DurationMinutes` remains unchanged.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~RespondToAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: All tests PASS (including existing ones)

- [ ] **Step 6: Run full test suite**

Run: `dotnet test ./Automotive.Marketplace.sln -q`
Expected: All tests pass

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/ Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToAvailabilityCommandHandlerTests.cs
git commit -m "feat: add StartTime and DurationMinutes to RespondToAvailability PickSlot"
```

---

### Task 6: Update ChatHub — add RespondToAvailability startTime/durationMinutes params

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

- [ ] **Step 1: Update RespondToAvailability hub method signature**

Replace the `RespondToAvailability` method to accept `startTime` and `durationMinutes`:

```csharp
    public async Task RespondToAvailability(Guid availabilityCardId, string action,
        Guid? slotId = null, List<RespondToAvailabilityCommand.ShareBackSlot>? shareBackSlots = null,
        DateTime? startTime = null, int? durationMinutes = null)
    {
        var result = await mediator.Send(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = availabilityCardId,
            ResponderId = UserId,
            Action = Enum.Parse<AvailabilityResponseAction>(action, ignoreCase: true),
            SlotId = slotId,
            ShareBackSlots = shareBackSlots,
            StartTime = startTime,
            DurationMinutes = durationMinutes
        });

        var initiatorId = result.Action == AvailabilityResponseAction.PickSlot
            ? result.PickedSlotMeeting!.RecipientId
            : result.SharedBackAvailability!.RecipientId;

        await Clients.Group($"user-{UserId}").SendAsync("AvailabilityResponded", result);
        await Clients.Group($"user-{initiatorId}").SendAsync("AvailabilityResponded", result);
    }
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -q`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs
git commit -m "feat: add startTime and durationMinutes params to RespondToAvailability hub method"
```

---

### Task 7: Frontend types + hub constants + event payloads for cancel

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/types/Meeting.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/MeetingEventPayloads.ts`
- Modify: `automotive.marketplace.client/src/features/chat/constants/chatHub.ts`

- [ ] **Step 1: Add 'Cancelled' to MeetingStatus**

```typescript
// automotive.marketplace.client/src/features/chat/types/Meeting.ts
export type MeetingStatus =
  | "Pending"
  | "Accepted"
  | "Declined"
  | "Rescheduled"
  | "Expired"
  | "Cancelled";

export type Meeting = {
  id: string;
  proposedAt: string;
  durationMinutes: number;
  locationText?: string;
  locationLat?: number;
  locationLng?: number;
  status: MeetingStatus;
  expiresAt: string;
  initiatorId: string;
  parentMeetingId?: string;
};
```

- [ ] **Step 2: Add 'Cancelled' to AvailabilityCardStatus**

```typescript
// automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts
export type AvailabilityCardStatus = "Pending" | "Responded" | "Expired" | "Cancelled";

export type AvailabilitySlot = {
  id: string;
  startTime: string;
  endTime: string;
};

export type AvailabilityCard = {
  id: string;
  status: AvailabilityCardStatus;
  expiresAt: string;
  initiatorId: string;
  slots: AvailabilitySlot[];
};
```

- [ ] **Step 3: Add cancel payloads to MeetingEventPayloads.ts**

Append these types at the end of the file:

```typescript
export type MeetingCancelledPayload = {
  meetingId: string;
  conversationId: string;
  newStatus: "Cancelled";
  initiatorId: string;
  recipientId: string;
};

export type AvailabilityCancelledPayload = {
  availabilityCardId: string;
  conversationId: string;
  newStatus: "Cancelled";
  initiatorId: string;
  recipientId: string;
};
```

- [ ] **Step 4: Add hub constants**

Update `chatHub.ts` to include the 4 new constants:

```typescript
export const HUB_METHODS = {
  // Client → Server
  SEND_MESSAGE: "SendMessage",
  MAKE_OFFER: "MakeOffer",
  RESPOND_TO_OFFER: "RespondToOffer",
  PROPOSE_MEETING: "ProposeMeeting",
  RESPOND_TO_MEETING: "RespondToMeeting",
  SHARE_AVAILABILITY: "ShareAvailability",
  RESPOND_TO_AVAILABILITY: "RespondToAvailability",
  CANCEL_MEETING: "CancelMeeting",
  CANCEL_AVAILABILITY: "CancelAvailability",
  // Server → Client
  RECEIVE_MESSAGE: "ReceiveMessage",
  UPDATE_UNREAD_COUNT: "UpdateUnreadCount",
  OFFER_MADE: "OfferMade",
  OFFER_ACCEPTED: "OfferAccepted",
  OFFER_DECLINED: "OfferDeclined",
  OFFER_COUNTERED: "OfferCountered",
  OFFER_EXPIRED: "OfferExpired",
  MEETING_PROPOSED: "MeetingProposed",
  MEETING_ACCEPTED: "MeetingAccepted",
  MEETING_DECLINED: "MeetingDeclined",
  MEETING_RESCHEDULED: "MeetingRescheduled",
  MEETING_EXPIRED: "MeetingExpired",
  MEETING_CANCELLED: "MeetingCancelled",
  AVAILABILITY_SHARED: "AvailabilityShared",
  AVAILABILITY_RESPONDED: "AvailabilityResponded",
  AVAILABILITY_EXPIRED: "AvailabilityExpired",
  AVAILABILITY_CANCELLED: "AvailabilityCancelled",
} as const;
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/types/ automotive.marketplace.client/src/features/chat/constants/chatHub.ts
git commit -m "feat: add Cancelled status types and hub constants for cancel events"
```

---

### Task 8: Timezone helper utility

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/utils/timezone.ts`

- [ ] **Step 1: Create the timezone helper**

```typescript
// automotive.marketplace.client/src/features/chat/utils/timezone.ts
export function getTimezoneOffsetLabel(): string {
  const offsetMinutes = new Date().getTimezoneOffset();
  if (offsetMinutes === 0) return "UTC+0";

  const sign = offsetMinutes <= 0 ? "+" : "-";
  const absMinutes = Math.abs(offsetMinutes);
  const hours = Math.floor(absMinutes / 60);
  const minutes = absMinutes % 60;

  if (minutes === 0) return `UTC${sign}${hours}`;
  return `UTC${sign}${hours}:${String(minutes).padStart(2, "0")}`;
}
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/utils/timezone.ts
git commit -m "feat: add getTimezoneOffsetLabel helper for local timezone display"
```

---

### Task 9: Update useChatHub — cancel functions + event listeners

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

- [ ] **Step 1: Add import for new payload types**

Add to the existing MeetingEventPayloads import:

```typescript
import type {
  MeetingProposedPayload,
  MeetingStatusUpdatedPayload,
  MeetingRescheduledPayload,
  MeetingExpiredPayload,
  AvailabilitySharedPayload,
  AvailabilityRespondedPayload,
  AvailabilityExpiredPayload,
  MeetingCancelledPayload,
  AvailabilityCancelledPayload,
} from "../types/MeetingEventPayloads";
```

- [ ] **Step 2: Add MeetingCancelled event listener**

Add after the `MEETING_EXPIRED` listener block (after line 311):

```typescript
    connection.on(
      HUB_METHODS.MEETING_CANCELLED,
      (payload: MeetingCancelledPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.meeting?.id === payload.meetingId
                    ? {
                        ...m,
                        meeting: { ...m.meeting!, status: "Cancelled" as const },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );
```

- [ ] **Step 3: Add AvailabilityCancelled event listener**

Add after the `AVAILABILITY_EXPIRED` listener block (after line 441):

```typescript
    connection.on(
      HUB_METHODS.AVAILABILITY_CANCELLED,
      (payload: AvailabilityCancelledPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.availabilityCard?.id === payload.availabilityCardId
                    ? {
                        ...m,
                        availabilityCard: {
                          ...m.availabilityCard!,
                          status: "Cancelled" as const,
                        },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );
```

- [ ] **Step 4: Add cancelMeeting invoke function**

Add after the `respondToAvailability` function:

```typescript
  const cancelMeeting = useCallback(
    ({ meetingId }: { meetingId: string }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(HUB_METHODS.CANCEL_MEETING, meetingId);
    },
    [],
  );

  const cancelAvailability = useCallback(
    ({ availabilityCardId }: { availabilityCardId: string }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.CANCEL_AVAILABILITY,
        availabilityCardId,
      );
    },
    [],
  );
```

- [ ] **Step 5: Update respondToAvailability to pass startTime + durationMinutes**

Replace the `respondToAvailability` function to accept and pass the new fields:

```typescript
  const respondToAvailability = useCallback(
    ({
      availabilityCardId,
      action,
      slotId,
      shareBackSlots,
      startTime,
      durationMinutes,
    }: {
      availabilityCardId: string;
      action: "PickSlot" | "ShareBack";
      slotId?: string;
      shareBackSlots?: { startTime: string; endTime: string }[];
      startTime?: string;
      durationMinutes?: number;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.RESPOND_TO_AVAILABILITY,
        availabilityCardId,
        action,
        slotId ?? null,
        shareBackSlots ?? null,
        startTime ?? null,
        durationMinutes ?? null,
      );
    },
    [],
  );
```

- [ ] **Step 6: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds (may have unused export warnings — that's fine, components will use them in next tasks)

- [ ] **Step 7: Update return statement**

Add the new functions to the return:

```typescript
  return {
    sendMessage,
    sendOffer,
    respondToOffer,
    proposeMeeting,
    respondToMeeting,
    shareAvailability,
    respondToAvailability,
    cancelMeeting,
    cancelAvailability,
  };
```

- [ ] **Step 8: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds (may have unused export warnings — that's fine, components will use them in next tasks)

- [ ] **Step 9: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/api/useChatHub.ts
git commit -m "feat: add cancel functions and event listeners to useChatHub"
```

---

### Task 10: Update ProposeMeetingModal — timezone display + local time input

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx`

- [ ] **Step 1: Import timezone helper**

Add import at top of file:

```typescript
import { getTimezoneOffsetLabel } from "../utils/timezone";
```

- [ ] **Step 2: Add timezone constant in component body**

Add after the component's state declarations (after line 55):

```typescript
  const timezone = getTimezoneOffsetLabel();
```

- [ ] **Step 3: Fix time input to use local time instead of UTC**

Change the `defaultTime` and `defaultDate` calculations to use local time format instead of UTC ISO:

Replace lines 38-43:
```typescript
  const defaultDate = initialMeeting
    ? new Date(initialMeeting.proposedAt).toISOString().slice(0, 10)
    : "";
  const defaultTime = initialMeeting
    ? new Date(initialMeeting.proposedAt).toISOString().slice(11, 16)
    : "";
```

With:
```typescript
  const formatLocalDate = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
  const formatLocalTime = (d: Date) =>
    `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;

  const defaultDate = initialMeeting
    ? formatLocalDate(new Date(initialMeeting.proposedAt))
    : "";
  const defaultTime = initialMeeting
    ? formatLocalTime(new Date(initialMeeting.proposedAt))
    : "";
```

- [ ] **Step 4: Fix proposedAt construction to interpret as local time**

Replace line 57:
```typescript
  const proposedAt = date && time ? new Date(`${date}T${time}:00Z`) : null;
```

With:
```typescript
  const proposedAt = date && time ? new Date(`${date}T${time}:00`) : null;
```

(Removing the `Z` suffix so the browser interprets it as local time.)

- [ ] **Step 5: Update the label**

Replace `Start time (UTC)` label text:

```typescript
            <Label htmlFor="meeting-time">Start time ({timezone})</Label>
```

- [ ] **Step 6: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx
git commit -m "feat: display local timezone in ProposeMeetingModal and interpret inputs as local time"
```

---

### Task 11: Update ShareAvailabilityModal — timezone display + local time input

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx`

- [ ] **Step 1: Import timezone helper**

```typescript
import { getTimezoneOffsetLabel } from "../utils/timezone";
```

- [ ] **Step 2: Add timezone constant**

Add after line 39 (`const [slots, ...`):

```typescript
  const timezone = getTimezoneOffsetLabel();
```

- [ ] **Step 3: Fix validation to use local time**

Replace the `validSlots` filter (lines 60-65):

```typescript
  const validSlots = slots.filter((s) => {
    if (!s.date || !s.startTime || !s.endTime) return false;
    const start = new Date(`${s.date}T${s.startTime}:00`);
    const end = new Date(`${s.date}T${s.endTime}:00`);
    return start > now && end > start;
  });
```

- [ ] **Step 4: Fix handleSubmit to use local time construction**

Replace the `mapped` construction in `handleSubmit` (lines 71-74):

```typescript
    const mapped = slots.map((s) => ({
      startTime: new Date(`${s.date}T${s.startTime}:00`).toISOString(),
      endTime: new Date(`${s.date}T${s.endTime}:00`).toISOString(),
    }));
```

- [ ] **Step 5: Update labels**

Replace `From` label with `From ({timezone})` and `To` label with `To ({timezone})`:

```typescript
                <div className="space-y-1">
                  <Label className="text-xs">From ({timezone})</Label>
```

```typescript
                <div className="space-y-1">
                  <Label className="text-xs">To ({timezone})</Label>
```

- [ ] **Step 6: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx
git commit -m "feat: display local timezone in ShareAvailabilityModal and interpret inputs as local time"
```

---

### Task 12: Update MeetingCard — cancel button, suggest alternative popover, cancelled status, timezone

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx`

- [ ] **Step 1: Add imports**

Update the imports section:

```typescript
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { format } from "date-fns";
import {
  Ban,
  Calendar,
  CalendarCheck,
  CalendarClock,
  CalendarDays,
  CalendarRange,
  CalendarX,
  Clock,
  MapPin,
} from "lucide-react";
import { useState } from "react";
import type { Meeting } from "../types/Meeting";
import { getTimezoneOffsetLabel } from "../utils/timezone";
import ProposeMeetingModal from "./ProposeMeetingModal";
import ShareAvailabilityModal from "./ShareAvailabilityModal";
```

- [ ] **Step 2: Update props type**

Replace `MeetingCardProps`:

```typescript
type MeetingCardProps = {
  meeting: Meeting;
  currentUserId: string;
  onAccept: (meetingId: string) => void;
  onDecline: (meetingId: string) => void;
  onReschedule: (
    meetingId: string,
    data: {
      proposedAt: string;
      durationMinutes: number;
      locationText?: string;
      locationLat?: number;
      locationLng?: number;
    },
  ) => void;
  onCancel: (meetingId: string) => void;
  onShareAvailability: (
    meetingId: string,
    slots: { startTime: string; endTime: string }[],
  ) => void;
};
```

- [ ] **Step 3: Add Cancelled to statusConfig**

Add after the `Expired` entry:

```typescript
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Meetup Cancelled",
    icon: Ban,
    labelClass: "text-muted",
    subLabel: "Withdrawn",
    subLabelClass: "text-muted-foreground",
  },
```

- [ ] **Step 4: Update the component body**

Replace the component function to include cancel, suggest alternative, and timezone:

```typescript
const MeetingCard = ({
  meeting,
  currentUserId,
  onAccept,
  onDecline,
  onReschedule,
  onCancel,
  onShareAvailability,
}: MeetingCardProps) => {
  const [rescheduleOpen, setRescheduleOpen] = useState(false);
  const [shareAvailOpen, setShareAvailOpen] = useState(false);
  const [suggestOpen, setSuggestOpen] = useState(false);
  const config = statusConfig[meeting.status];
  const Icon = config.icon;
  const timezone = getTimezoneOffsetLabel();

  const canRespond =
    meeting.status === "Pending" && currentUserId !== meeting.initiatorId;
  const canCancel =
    (meeting.status === "Pending" || meeting.status === "Accepted") &&
    currentUserId === meeting.initiatorId;
  const proposedDate = new Date(meeting.proposedAt);
  const isMuted =
    meeting.status === "Declined" ||
    meeting.status === "Expired" ||
    meeting.status === "Rescheduled" ||
    meeting.status === "Cancelled";
```

- [ ] **Step 5: Update the time display to use timezone**

Replace the UTC time display (lines 143-153 area) with:

```typescript
              <p
                className={`text-xs ${isMuted ? "text-muted-foreground" : "text-muted-foreground"}`}
              >
                {format(proposedDate, "HH:mm")} –{" "}
                {format(
                  new Date(
                    proposedDate.getTime() + meeting.durationMinutes * 60000,
                  ),
                  "HH:mm",
                )}{" "}
                {timezone}
              </p>
```

- [ ] **Step 6: Update the action buttons section**

Replace the `{canRespond && ...}` block with:

```typescript
          {canCancel && (
            <div className="mt-3">
              <Button
                size="sm"
                variant="ghost"
                className="text-destructive hover:text-destructive h-7 text-xs"
                onClick={() => onCancel(meeting.id)}
              >
                Cancel meetup
              </Button>
            </div>
          )}

          {canRespond && (
            <div className="mt-3 flex gap-2">
              <Button
                size="sm"
                className="h-7 flex-1 text-xs"
                onClick={() => onAccept(meeting.id)}
              >
                Accept
              </Button>
              <Popover open={suggestOpen} onOpenChange={setSuggestOpen}>
                <PopoverTrigger asChild>
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 flex-1 text-xs"
                  >
                    Suggest alternative
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-52 p-1" align="start">
                  <button
                    className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm"
                    onClick={() => {
                      setSuggestOpen(false);
                      setRescheduleOpen(true);
                    }}
                  >
                    <Calendar className="mr-2 h-4 w-4" />
                    Propose a counter time
                  </button>
                  <button
                    className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm"
                    onClick={() => {
                      setSuggestOpen(false);
                      setShareAvailOpen(true);
                    }}
                  >
                    <CalendarRange className="mr-2 h-4 w-4" />
                    Share my availability
                  </button>
                </PopoverContent>
              </Popover>
              <Button
                size="sm"
                variant="ghost"
                className="text-destructive hover:text-destructive h-7 flex-1 text-xs"
                onClick={() => onDecline(meeting.id)}
              >
                Decline
              </Button>
            </div>
          )}
```

- [ ] **Step 7: Add ShareAvailabilityModal for the "share my availability" option**

After the existing `ProposeMeetingModal`, add:

```typescript
      <ShareAvailabilityModal
        open={shareAvailOpen}
        onOpenChange={setShareAvailOpen}
        onSubmit={(slots) => {
          onShareAvailability(meeting.id, slots);
          setShareAvailOpen(false);
        }}
      />
```

- [ ] **Step 8: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds (may warn about unused props in MessageThread — we'll wire them in Task 16)

- [ ] **Step 9: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx
git commit -m "feat: add cancel, suggest alternative, cancelled status, and timezone to MeetingCard"
```

---

### Task 13: Update AvailabilityCardComponent — cancel button, inline slot picker, cancelled status, timezone

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/AvailabilityCardComponent.tsx`

- [ ] **Step 1: Update imports**

```typescript
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { format } from "date-fns";
import { Ban, CalendarRange, Clock } from "lucide-react";
import { useState } from "react";
import type {
  AvailabilityCard,
  AvailabilityCardStatus,
  AvailabilitySlot,
} from "../types/AvailabilityCard";
import { getTimezoneOffsetLabel } from "../utils/timezone";
import ShareAvailabilityModal from "./ShareAvailabilityModal";
```

- [ ] **Step 2: Update props type**

Replace the `onPickSlot` callback signature:

```typescript
type AvailabilityCardComponentProps = {
  card: AvailabilityCard;
  currentUserId: string;
  onPickSlot: (
    cardId: string,
    slotId: string,
    startTime: string,
    durationMinutes: number,
  ) => void;
  onShareBack: (
    cardId: string,
    slots: { startTime: string; endTime: string }[],
  ) => void;
  onCancel: (cardId: string) => void;
};
```

- [ ] **Step 3: Add Cancelled to statusConfig**

```typescript
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    label: "Availability Cancelled",
    icon: Ban,
    labelClass: "text-muted",
  },
```

- [ ] **Step 4: Rewrite the component with inline slot picker and cancel**

```typescript
const DURATION_OPTIONS = [15, 30, 60, 90, 120];

const AvailabilityCardComponent = ({
  card,
  currentUserId,
  onPickSlot,
  onShareBack,
  onCancel,
}: AvailabilityCardComponentProps) => {
  const [shareBackOpen, setShareBackOpen] = useState(false);
  const [expandedSlotId, setExpandedSlotId] = useState<string | null>(null);
  const [pickerTime, setPickerTime] = useState("");
  const [pickerDuration, setPickerDuration] = useState(60);
  const [pickerError, setPickerError] = useState<string | null>(null);
  const config = statusConfig[card.status];
  const Icon = config.icon;
  const timezone = getTimezoneOffsetLabel();

  const canRespond =
    card.status === "Pending" && currentUserId !== card.initiatorId;
  const canCancel =
    card.status === "Pending" && currentUserId === card.initiatorId;
  const isDisabled = card.status !== "Pending";

  const handleToggleSlot = (slot: AvailabilitySlot) => {
    if (expandedSlotId === slot.id) {
      setExpandedSlotId(null);
      setPickerError(null);
    } else {
      setExpandedSlotId(slot.id);
      const start = new Date(slot.startTime);
      setPickerTime(
        `${String(start.getHours()).padStart(2, "0")}:${String(start.getMinutes()).padStart(2, "0")}`,
      );
      setPickerDuration(60);
      setPickerError(null);
    }
  };

  const handlePickSlotSubmit = (slot: AvailabilitySlot) => {
    const slotStart = new Date(slot.startTime);
    const slotEnd = new Date(slot.endTime);
    const [hours, minutes] = pickerTime.split(":").map(Number);
    const selectedStart = new Date(slotStart);
    selectedStart.setHours(hours, minutes, 0, 0);
    const selectedEnd = new Date(
      selectedStart.getTime() + pickerDuration * 60000,
    );

    if (selectedStart < slotStart) {
      setPickerError(
        `Start time must be at or after ${format(slotStart, "HH:mm")}`,
      );
      return;
    }
    if (selectedEnd > slotEnd) {
      setPickerError(
        `Meeting must end by ${format(slotEnd, "HH:mm")}`,
      );
      return;
    }

    setPickerError(null);
    setExpandedSlotId(null);
    onPickSlot(card.id, slot.id, selectedStart.toISOString(), pickerDuration);
  };

  return (
    <>
      <div
        className={`w-full max-w-[300px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
      >
        <div
          className={`${config.headerClass} flex items-center gap-2 px-4 py-2.5`}
        >
          <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
          <span
            className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}
          >
            {config.label}
          </span>
        </div>

        <div className="bg-card divide-y">
          {card.slots.map((slot) => {
            const start = new Date(slot.startTime);
            const end = new Date(slot.endTime);
            const isExpanded = expandedSlotId === slot.id;
            return (
              <div key={slot.id}>
                <div
                  className={`flex items-center justify-between px-4 py-2 ${isDisabled ? "opacity-50" : ""}`}
                >
                  <div>
                    <p className="text-sm font-medium">
                      {format(start, "EEE, MMM d")}
                    </p>
                    <p className="text-muted-foreground text-xs">
                      {format(start, "HH:mm")} – {format(end, "HH:mm")}{" "}
                      {timezone}
                    </p>
                  </div>
                  {canRespond && (
                    <Button
                      size="sm"
                      variant="outline"
                      className="h-7 text-xs"
                      onClick={() => handleToggleSlot(slot)}
                    >
                      {isExpanded ? "Close ▲" : "Propose →"}
                    </Button>
                  )}
                </div>
                {isExpanded && (
                  <div className="border-t bg-muted/30 px-4 py-2 space-y-2">
                    <div className="flex items-end gap-2">
                      <div className="flex-1 space-y-1">
                        <Label className="text-xs">Start time</Label>
                        <Input
                          type="time"
                          value={pickerTime}
                          onChange={(e) => {
                            setPickerTime(e.target.value);
                            setPickerError(null);
                          }}
                          className="h-7 text-xs"
                        />
                      </div>
                      <div className="flex-1 space-y-1">
                        <Label className="text-xs">Duration</Label>
                        <select
                          value={pickerDuration}
                          onChange={(e) => {
                            setPickerDuration(Number(e.target.value));
                            setPickerError(null);
                          }}
                          className="border-input bg-background h-7 w-full rounded-md border px-2 text-xs"
                        >
                          {DURATION_OPTIONS.map((d) => (
                            <option key={d} value={d}>
                              {d} min
                            </option>
                          ))}
                        </select>
                      </div>
                      <Button
                        size="sm"
                        className="h-7 text-xs"
                        onClick={() => handlePickSlotSubmit(slot)}
                      >
                        Propose
                      </Button>
                    </div>
                    {pickerError && (
                      <p className="text-destructive text-xs">{pickerError}</p>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {canCancel && (
          <div className="border-t px-4 py-2">
            <Button
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive h-7 text-xs"
              onClick={() => onCancel(card.id)}
            >
              Cancel availability
            </Button>
          </div>
        )}

        {canRespond && (
          <div className="border-t px-4 py-2">
            <button
              className="text-muted-foreground hover:text-foreground text-xs underline"
              onClick={() => setShareBackOpen(true)}
            >
              None of these work — share my availability instead
            </button>
          </div>
        )}
      </div>

      <ShareAvailabilityModal
        open={shareBackOpen}
        onOpenChange={setShareBackOpen}
        onSubmit={(slots) => {
          onShareBack(card.id, slots);
          setShareBackOpen(false);
        }}
      />
    </>
  );
};

export default AvailabilityCardComponent;
```

- [ ] **Step 5: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/AvailabilityCardComponent.tsx
git commit -m "feat: add cancel, inline slot picker, cancelled status, and timezone to AvailabilityCard"
```

---

### Task 14: Update ActionBar — confirmed meeting guard

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx`

- [ ] **Step 1: Add imports**

Add these imports:

```typescript
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { format } from "date-fns";
import type { Meeting } from "../types/Meeting";
```

- [ ] **Step 2: Update ActionBarProps**

Add two new props:

```typescript
type ActionBarProps = {
  currentUserId: string;
  buyerId: string;
  sellerId: string;
  listingPrice: number;
  conversationId: string;
  buyerHasEngaged: boolean;
  hasActiveOffer: boolean;
  hasActiveMeeting: boolean;
  acceptedMeeting: Meeting | null;
  onSendOffer: (amount: number) => void;
  onProposeMeeting: (data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
  onShareAvailability: (
    slots: { startTime: string; endTime: string }[],
  ) => void;
  onCancelMeeting: (meetingId: string) => void;
};
```

- [ ] **Step 3: Update the component body**

Replace the component function:

```typescript
const ActionBar = ({
  currentUserId,
  buyerId,
  sellerId,
  listingPrice,
  conversationId,
  buyerHasEngaged,
  hasActiveOffer,
  hasActiveMeeting,
  acceptedMeeting,
  onSendOffer,
  onProposeMeeting,
  onShareAvailability,
  onCancelMeeting,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [actionsPopoverOpen, setActionsPopoverOpen] = useState(false);
  const [proposeMeetingOpen, setProposeMeetingOpen] = useState(false);
  const [shareAvailabilityOpen, setShareAvailabilityOpen] = useState(false);
  const [guardAction, setGuardAction] = useState<
    "propose" | "availability" | null
  >(null);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showButtons = isBuyer || (isSeller && buyerHasEngaged);

  if (!showButtons) return null;

  const handleMeetingAction = (action: "propose" | "availability") => {
    setActionsPopoverOpen(false);
    if (acceptedMeeting) {
      setGuardAction(action);
    } else if (action === "propose") {
      setProposeMeetingOpen(true);
    } else {
      setShareAvailabilityOpen(true);
    }
  };

  const handleGuardConfirm = () => {
    if (!acceptedMeeting || !guardAction) return;
    onCancelMeeting(acceptedMeeting.id);
    if (guardAction === "propose") {
      setProposeMeetingOpen(true);
    } else {
      setShareAvailabilityOpen(true);
    }
    setGuardAction(null);
  };

  return (
    <>
      <Popover open={actionsPopoverOpen} onOpenChange={setActionsPopoverOpen}>
        <PopoverTrigger asChild>
          <Button variant="outline" size="icon" className="h-8 w-8 shrink-0">
            <Plus className="h-4 w-4" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-48 p-1" align="start">
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveOffer}
            title={
              hasActiveOffer
                ? "An offer is already pending in this conversation"
                : undefined
            }
            onClick={() => {
              setActionsPopoverOpen(false);
              setOfferModalOpen(true);
            }}
          >
            <DollarSign className="mr-2 h-4 w-4" />
            Make an Offer
          </button>
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveMeeting}
            title={
              hasActiveMeeting
                ? "A meetup negotiation is already active"
                : undefined
            }
            onClick={() => handleMeetingAction("propose")}
          >
            <Calendar className="mr-2 h-4 w-4" />
            Propose a time
          </button>
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveMeeting}
            title={
              hasActiveMeeting
                ? "A meetup negotiation is already active"
                : undefined
            }
            onClick={() => handleMeetingAction("availability")}
          >
            <Clock className="mr-2 h-4 w-4" />
            Share availability
          </button>
        </PopoverContent>
      </Popover>

      <AlertDialog
        open={guardAction !== null}
        onOpenChange={(open) => {
          if (!open) setGuardAction(null);
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancel existing meetup?</AlertDialogTitle>
            <AlertDialogDescription>
              {acceptedMeeting
                ? `You have a confirmed meetup on ${format(new Date(acceptedMeeting.proposedAt), "EEE, MMM d")} at ${format(new Date(acceptedMeeting.proposedAt), "HH:mm")}. Starting a new negotiation will cancel it.`
                : "Starting a new negotiation will cancel the existing meetup."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Keep existing</AlertDialogCancel>
            <AlertDialogAction
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              onClick={handleGuardConfirm}
            >
              Continue
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <MakeOfferModal
        open={offerModalOpen}
        onOpenChange={setOfferModalOpen}
        mode="offer"
        listingPrice={listingPrice}
        conversationId={conversationId}
        onSubmit={(amount) => {
          onSendOffer(amount);
          setOfferModalOpen(false);
        }}
      />

      <ProposeMeetingModal
        open={proposeMeetingOpen}
        onOpenChange={setProposeMeetingOpen}
        mode="propose"
        onSubmit={(data) => {
          onProposeMeeting(data);
          setProposeMeetingOpen(false);
        }}
      />

      <ShareAvailabilityModal
        open={shareAvailabilityOpen}
        onOpenChange={setShareAvailabilityOpen}
        onSubmit={(slots) => {
          onShareAvailability(slots);
          setShareAvailabilityOpen(false);
        }}
      />
    </>
  );
};

export default ActionBar;
```

- [ ] **Step 4: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: May fail due to MessageThread not passing new props yet — that's OK, we'll fix in Task 16

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ActionBar.tsx
git commit -m "feat: add confirmed meeting guard dialog to ActionBar"
```

---

### Task 15: Update MessageThread — wire all new props, cancel functions, suggest alternative

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`

- [ ] **Step 1: Update useChatHub destructuring**

Add `cancelMeeting` and `cancelAvailability` to the destructured return:

```typescript
  const {
    sendMessage,
    sendOffer,
    respondToOffer,
    proposeMeeting,
    respondToMeeting,
    shareAvailability,
    respondToAvailability,
    cancelMeeting,
    cancelAvailability,
  } = useChatHub();
```

- [ ] **Step 2: Compute acceptedMeeting**

Add after `hasActiveMeeting`:

```typescript
  const acceptedMeeting =
    messages.find(
      (m) => m.messageType === "Meeting" && m.meeting?.status === "Accepted",
    )?.meeting ?? null;
```

- [ ] **Step 3: Update MeetingCard props in the render**

Replace the `<MeetingCard ... />` JSX (approximately lines 124-141):

```typescript
                <MeetingCard
                  meeting={m.meeting}
                  currentUserId={userId}
                  onAccept={(meetingId) =>
                    respondToMeeting({ meetingId, action: "Accept" })
                  }
                  onDecline={(meetingId) =>
                    respondToMeeting({ meetingId, action: "Decline" })
                  }
                  onReschedule={(meetingId, data) =>
                    respondToMeeting({
                      meetingId,
                      action: "Reschedule",
                      rescheduleData: data,
                    })
                  }
                  onCancel={(meetingId) => cancelMeeting({ meetingId })}
                  onShareAvailability={(meetingId, slots) => {
                    respondToMeeting({ meetingId, action: "Decline" });
                    shareAvailability({
                      conversationId: conversation.id,
                      slots,
                    });
                  }}
                />
```

- [ ] **Step 4: Update AvailabilityCardComponent props**

Replace the `<AvailabilityCardComponent ... />` JSX (approximately lines 152-170):

```typescript
                <AvailabilityCardComponent
                  card={m.availabilityCard}
                  currentUserId={userId}
                  onPickSlot={(cardId, slotId, startTime, durationMinutes) =>
                    respondToAvailability({
                      availabilityCardId: cardId,
                      action: "PickSlot",
                      slotId,
                      startTime,
                      durationMinutes,
                    })
                  }
                  onShareBack={(cardId, slots) =>
                    respondToAvailability({
                      availabilityCardId: cardId,
                      action: "ShareBack",
                      shareBackSlots: slots,
                    })
                  }
                  onCancel={(cardId) =>
                    cancelAvailability({ availabilityCardId: cardId })
                  }
                />
```

- [ ] **Step 5: Update ActionBar props**

Replace the `<ActionBar ... />` JSX:

```typescript
        <ActionBar
          currentUserId={userId}
          buyerId={conversation.buyerId}
          sellerId={conversation.sellerId}
          listingPrice={conversation.listingPrice}
          conversationId={conversation.id}
          buyerHasEngaged={conversation.buyerHasEngaged}
          hasActiveOffer={hasActiveOffer}
          hasActiveMeeting={hasActiveMeeting}
          acceptedMeeting={acceptedMeeting}
          onSendOffer={(amount) =>
            sendOffer({ conversationId: conversation.id, amount })
          }
          onProposeMeeting={(data) =>
            proposeMeeting({ conversationId: conversation.id, ...data })
          }
          onShareAvailability={(slots) =>
            shareAvailability({
              conversationId: conversation.id,
              slots,
            })
          }
          onCancelMeeting={(meetingId) => cancelMeeting({ meetingId })}
        />
```

- [ ] **Step 6: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MessageThread.tsx
git commit -m "feat: wire cancel, slot picker, and suggest alternative props in MessageThread"
```

---

### Task 16: Sticky confirmed meeting bar

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`

- [ ] **Step 1: Add imports**

Add to the existing imports:

```typescript
import { format } from "date-fns";
import { CheckCircle, MapPin } from "lucide-react";
import { useCallback } from "react";
import { getTimezoneOffsetLabel } from "../utils/timezone";
```

Update the `useRef` and `useState` imports to include `useCallback`:

```typescript
import { useCallback, useEffect, useRef, useState } from "react";
```

- [ ] **Step 2: Add sticky bar state and IntersectionObserver**

Add after the `acceptedMeeting` computation:

```typescript
  const [showStickyBar, setShowStickyBar] = useState(false);
  const acceptedCardRef = useRef<HTMLDivElement>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  const acceptedCardRefCallback = useCallback(
    (node: HTMLDivElement | null) => {
      acceptedCardRef.current = node;
    },
    [],
  );

  useEffect(() => {
    const node = acceptedCardRef.current;
    const container = scrollContainerRef.current;
    if (!node || !container || !acceptedMeeting) {
      setShowStickyBar(false);
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        setShowStickyBar(!entry.isIntersecting);
      },
      { root: container, threshold: 0 },
    );

    observer.observe(node);
    return () => observer.disconnect();
  }, [acceptedMeeting]);
```

- [ ] **Step 3: Attach ref to the accepted meeting card**

In the message rendering loop where we render `MeetingCard` for meetings, wrap the accepted meeting's container div with the ref:

```typescript
          if (m.messageType === "Meeting" && m.meeting) {
            const isOwn = m.senderId === userId;
            const isAccepted = m.meeting.status === "Accepted";
            return (
              <div
                key={m.id}
                ref={isAccepted ? acceptedCardRefCallback : undefined}
                className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
              >
```

- [ ] **Step 4: Attach ref to scroll container and render sticky bar**

Replace the scroll container div (the one with `className="flex-1 space-y-2 overflow-y-auto p-4"`):

```typescript
      <div
        ref={scrollContainerRef}
        className="relative flex-1 overflow-y-auto"
      >
        {showStickyBar && acceptedMeeting && (
          <div className="bg-green-900/95 text-green-100 sticky top-0 z-10 flex items-center gap-2 px-4 py-2 text-xs backdrop-blur-sm">
            <CheckCircle className="h-3.5 w-3.5 shrink-0" />
            <span className="font-semibold">Meetup Confirmed</span>
            <span className="text-green-300">·</span>
            <span>
              {format(new Date(acceptedMeeting.proposedAt), "EEE, MMM d")} ·{" "}
              {format(new Date(acceptedMeeting.proposedAt), "HH:mm")}–
              {format(
                new Date(
                  new Date(acceptedMeeting.proposedAt).getTime() +
                    acceptedMeeting.durationMinutes * 60000,
                ),
                "HH:mm",
              )}{" "}
              {getTimezoneOffsetLabel()}
            </span>
            {acceptedMeeting.locationText && (
              <>
                <span className="text-green-300">·</span>
                <MapPin className="h-3 w-3 shrink-0" />
                <span className="truncate">{acceptedMeeting.locationText}</span>
              </>
            )}
          </div>
        )}
        <div className="space-y-2 p-4">
```

And close the container after `<div ref={bottomRef} />`:

```typescript
        <div ref={bottomRef} />
        </div>
      </div>
```

- [ ] **Step 5: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MessageThread.tsx
git commit -m "feat: add sticky confirmed meeting bar with IntersectionObserver"
```

---

### Task 17: Conversation ID in inbox URL

**Files:**
- Modify: `automotive.marketplace.client/src/app/routes/inbox.tsx`
- Create: `automotive.marketplace.client/src/app/routes/inbox/$conversationId.tsx`
- Modify: `automotive.marketplace.client/src/app/pages/Inbox.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/components/ConversationList.tsx`

- [ ] **Step 1: Convert inbox.tsx to a layout route**

Replace `automotive.marketplace.client/src/app/routes/inbox.tsx`:

```typescript
import { Outlet, createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox")({
  component: InboxLayout,
});

function InboxLayout() {
  return <Outlet />;
}
```

- [ ] **Step 2: Create inbox/index.tsx for bare /inbox**

```typescript
// automotive.marketplace.client/src/app/routes/inbox/index.tsx
import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox/")({
  component: Inbox,
});
```

- [ ] **Step 3: Create inbox/$conversationId.tsx for parameterized route**

```typescript
// automotive.marketplace.client/src/app/routes/inbox/$conversationId.tsx
import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox/$conversationId")({
  component: () => {
    const { conversationId } = Route.useParams();
    return <Inbox initialConversationId={conversationId} />;
  },
});
```

- [ ] **Step 4: Update Inbox page to accept initialConversationId prop and use navigation**

```typescript
// automotive.marketplace.client/src/app/pages/Inbox.tsx
import { ConversationList, MessageThread } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";
import { useNavigate } from "@tanstack/react-router";
import { useState } from "react";

type InboxProps = {
  initialConversationId?: string;
};

const Inbox = ({ initialConversationId }: InboxProps) => {
  const [selected, setSelected] = useState<ConversationSummary | null>(null);
  const navigate = useNavigate();

  const handleSelect = (conversation: ConversationSummary) => {
    setSelected(conversation);
    void navigate({ to: "/inbox/$conversationId", params: { conversationId: conversation.id } });
  };

  const handleInitialLoad = (conversation: ConversationSummary | null) => {
    if (selected) return;
    if (conversation) {
      setSelected(conversation);
    } else if (initialConversationId) {
      void navigate({ to: "/inbox", replace: true });
    }
  };

  return (
    <div className="flex h-[calc(100vh-64px)] overflow-hidden">
      <aside className="border-border w-72 shrink-0 overflow-hidden border-r lg:w-80">
        <div className="border-border border-b px-4 py-3">
          <h1 className="text-lg font-semibold">Messages</h1>
        </div>
        <ConversationList
          selectedId={selected?.id ?? initialConversationId ?? null}
          onSelect={handleSelect}
          initialConversationId={initialConversationId}
          onInitialLoad={handleInitialLoad}
        />
      </aside>

      <main className="flex min-w-0 flex-1 flex-col">
        {selected ? (
          <MessageThread conversation={selected} />
        ) : (
          <div className="text-muted-foreground flex h-full items-center justify-center text-sm">
            Select a conversation to start messaging.
          </div>
        )}
      </main>
    </div>
  );
};

export default Inbox;
```

- [ ] **Step 5: Update ConversationList to support initial selection**

```typescript
// automotive.marketplace.client/src/features/chat/components/ConversationList.tsx
import { useSuspenseQuery } from "@tanstack/react-query";
import { formatDistanceToNow } from "date-fns";
import { useEffect, useRef } from "react";
import { getConversationsOptions } from "../api/getConversationsOptions";
import type { ConversationSummary } from "../types/ConversationSummary";

type ConversationListProps = {
  selectedId: string | null;
  onSelect: (conversation: ConversationSummary) => void;
  initialConversationId?: string;
  onInitialLoad?: (conversation: ConversationSummary | null) => void;
};

const ConversationList = ({
  selectedId,
  onSelect,
  initialConversationId,
  onInitialLoad,
}: ConversationListProps) => {
  const { data: conversationsQuery } = useSuspenseQuery(
    getConversationsOptions(),
  );
  const conversations = conversationsQuery.data;
  const didAutoSelect = useRef(false);

  useEffect(() => {
    if (didAutoSelect.current || !initialConversationId || !onInitialLoad)
      return;
    didAutoSelect.current = true;
    const match = conversations.find((c) => c.id === initialConversationId);
    onInitialLoad(match ?? null);
  }, [conversations, initialConversationId, onInitialLoad]);

  if (conversations.length === 0) {
    return (
      <div className="text-muted-foreground flex h-full items-center justify-center p-4 text-sm">
        No conversations yet.
      </div>
    );
  }

  return (
    <ul className="divide-border divide-y overflow-y-auto">
      {conversations.map((c) => (
        <li
          key={c.id}
          onClick={() => onSelect(c)}
          className={`hover:bg-muted/50 cursor-pointer px-4 py-3 transition-colors ${
            selectedId === c.id ? "bg-muted" : ""
          }`}
        >
          <div className="flex items-start gap-3">
            {c.listingThumbnail ? (
              <img
                src={c.listingThumbnail.url}
                alt={c.listingThumbnail.altText || c.listingTitle}
                className="mt-0.5 h-10 w-14 shrink-0 rounded object-cover"
              />
            ) : (
              <div className="bg-muted mt-0.5 h-10 w-14 shrink-0 rounded" />
            )}
            <div className="min-w-0 flex-1">
              <div className="flex items-center justify-between gap-1">
                <span className="truncate text-sm font-semibold">
                  {c.listingTitle}
                </span>
                {c.unreadCount > 0 && (
                  <span className="bg-primary text-primary-foreground shrink-0 rounded-full px-1.5 py-0.5 text-[10px] font-bold">
                    {c.unreadCount}
                  </span>
                )}
              </div>
              <p className="text-muted-foreground truncate text-xs">
                {c.counterpartUsername}
              </p>
              {c.lastMessage && (
                <p className="text-muted-foreground truncate text-xs">
                  {c.lastMessage}
                </p>
              )}
            </div>
          </div>
          <p className="text-muted-foreground mt-1 text-right text-[10px]">
            {formatDistanceToNow(new Date(c.lastMessageAt), {
              addSuffix: true,
            })}
          </p>
        </li>
      ))}
    </ul>
  );
};

export default ConversationList;
```

- [ ] **Step 6: Regenerate route tree**

Run (from `automotive.marketplace.client`): `npx tsr generate`
Expected: `routeTree.gen.ts` is updated with the new routes

- [ ] **Step 7: Verify frontend builds**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds

- [ ] **Step 8: Commit**

```bash
git add automotive.marketplace.client/src/app/routes/inbox.tsx automotive.marketplace.client/src/app/routes/inbox/ automotive.marketplace.client/src/app/pages/Inbox.tsx automotive.marketplace.client/src/features/chat/components/ConversationList.tsx automotive.marketplace.client/src/routeTree.gen.ts
git commit -m "feat: add conversation ID to inbox URL with parameterized routing"
```

---

### Task 18: Final verification

**Files:** None (verification only)

- [ ] **Step 1: Run all backend tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All tests pass (original 114 + new cancel tests)

- [ ] **Step 2: Build frontend**

Run (from `automotive.marketplace.client`): `npm run build`
Expected: Build succeeds with no errors

- [ ] **Step 3: Run frontend lint**

Run (from `automotive.marketplace.client`): `npm run lint && npm run format:check`
Expected: No lint errors (may need `npm run format` to fix formatting)

- [ ] **Step 4: Commit any formatting fixes**

```bash
cd automotive.marketplace.client && npm run format
cd ..
git add -A
git commit -m "style: format code"
```

- [ ] **Step 5: Verify no leftover debug code**

Run: `grep -r "console.log\|debugger\|TODO\|FIXME\|HACK" automotive.marketplace.client/src/features/chat/ --include="*.ts" --include="*.tsx" | grep -v node_modules`
Expected: No hits (or only acceptable existing ones)
