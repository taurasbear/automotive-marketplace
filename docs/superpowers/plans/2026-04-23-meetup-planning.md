# Meetup Planning Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add meetup coordination (direct time proposals and availability sharing) to the buyer–seller chat, mirroring the existing offer negotiation pattern.

**Architecture:** Two new domain entities (`Meeting`, `AvailabilityCard` + child `AvailabilitySlot`) with four CQRS command handlers invoked via SignalR hub methods. A background expiry service mirrors `OfferExpiryService`. Frontend adds four new chat components (`MeetingCard`, `AvailabilityCard`, `ProposeMeetingModal`, `ShareAvailabilityModal`) and a "Plan Meetup" dropdown in the `ActionBar`.

**Tech Stack:** ASP.NET Core 8, EF Core (PostgreSQL), MediatR CQRS, SignalR, FluentValidation, xUnit + TestContainers + Respawn + Bogus, React 19 + TypeScript, TanStack Query, shadcn/ui, date-fns, lucide-react

---

## File Structure

### New Files — Domain Layer
- `Automotive.Marketplace.Domain/Enums/MeetingStatus.cs` — Pending, Accepted, Declined, Rescheduled, Expired
- `Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs` — Pending, Responded, Expired
- `Automotive.Marketplace.Domain/Entities/Meeting.cs` — Meeting proposal entity
- `Automotive.Marketplace.Domain/Entities/AvailabilityCard.cs` — Availability card entity
- `Automotive.Marketplace.Domain/Entities/AvailabilitySlot.cs` — Time slot within an availability card

### Modified Files — Domain Layer
- `Automotive.Marketplace.Domain/Enums/MessageType.cs` — Add `Meeting = 2`, `Availability = 3`
- `Automotive.Marketplace.Domain/Entities/Message.cs` — Add `MeetingId`, `AvailabilityCardId` FKs + navigation properties

### New Files — Infrastructure Layer
- `Automotive.Marketplace.Infrastructure/Data/Configuration/MeetingConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/AvailabilityCardConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/AvailabilitySlotConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/MeetingBuilder.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/AvailabilityCardBuilder.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/AvailabilitySlotBuilder.cs`

### Modified Files — Infrastructure Layer
- `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs` — Add `MeetingId`, `AvailabilityCardId` as optional FKs
- `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — Add `Meetings`, `AvailabilityCards`, `AvailabilitySlots` DbSets

### New Files — Application Layer
- `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityResponse.cs`

### Modified Files — Application Layer
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs` — Add `MeetingData?`, `AvailabilityCardData?` nested records
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs` — Map meeting/availability data in message projection

### New Files — Server Layer
- `Automotive.Marketplace.Server/Services/MeetingExpiryService.cs`

### Modified Files — Server Layer
- `Automotive.Marketplace.Server/Hubs/ChatHub.cs` — Add 4 new hub methods
- `Automotive.Marketplace.Server/Program.cs` — Register `MeetingExpiryService`

### New Files — Tests
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/ProposeMeetingCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToMeetingCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/ShareAvailabilityCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToAvailabilityCommandHandlerTests.cs`

### New Files — Frontend
- `automotive.marketplace.client/src/features/chat/types/Meeting.ts`
- `automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts`
- `automotive.marketplace.client/src/features/chat/types/MeetingEventPayloads.ts`
- `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx`
- `automotive.marketplace.client/src/features/chat/components/AvailabilityCard.tsx`
- `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx`
- `automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx`

### Modified Files — Frontend
- `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts` — Add `Meeting`, `Availability` message types + data
- `automotive.marketplace.client/src/features/chat/types/ReceiveMessagePayload.ts` — Add meeting/availability fields
- `automotive.marketplace.client/src/features/chat/constants/chatHub.ts` — Add meeting/availability method constants
- `automotive.marketplace.client/src/features/chat/api/useChatHub.ts` — Add invoke functions + event listeners
- `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx` — Add "Plan Meetup" dropdown
- `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx` — Render meeting/availability cards

---

### Task 1: Domain Enums

**Files:**
- Create: `Automotive.Marketplace.Domain/Enums/MeetingStatus.cs`
- Create: `Automotive.Marketplace.Domain/Enums/AvailabilityCardStatus.cs`
- Modify: `Automotive.Marketplace.Domain/Enums/MessageType.cs`

- [ ] **Step 1: Create MeetingStatus enum**

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum MeetingStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Rescheduled = 3,
    Expired = 4,
}
```

- [ ] **Step 2: Create AvailabilityCardStatus enum**

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum AvailabilityCardStatus
{
    Pending = 0,
    Responded = 1,
    Expired = 2,
}
```

- [ ] **Step 3: Update MessageType enum**

Add two new values to the existing enum:

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum MessageType
{
    Text = 0,
    Offer = 1,
    Meeting = 2,
    Availability = 3,
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(domain): add MeetingStatus, AvailabilityCardStatus enums and extend MessageType

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Domain Entities

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/Meeting.cs`
- Create: `Automotive.Marketplace.Domain/Entities/AvailabilityCard.cs`
- Create: `Automotive.Marketplace.Domain/Entities/AvailabilitySlot.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Message.cs`

- [ ] **Step 1: Create Meeting entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Meeting : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public DateTime ProposedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? LocationText { get; set; }

    public decimal? LocationLat { get; set; }

    public decimal? LocationLng { get; set; }

    public MeetingStatus Status { get; set; }

    public Guid? ParentMeetingId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual Meeting? ParentMeeting { get; set; }

    public virtual ICollection<Meeting> CounterMeetings { get; set; } = [];

    public virtual Message? Message { get; set; }
}
```

- [ ] **Step 2: Create AvailabilityCard entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class AvailabilityCard : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public AvailabilityCardStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual ICollection<AvailabilitySlot> Slots { get; set; } = [];

    public virtual Message? Message { get; set; }
}
```

- [ ] **Step 3: Create AvailabilitySlot entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class AvailabilitySlot : BaseEntity
{
    public Guid AvailabilityCardId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public virtual AvailabilityCard AvailabilityCard { get; set; } = null!;
}
```

- [ ] **Step 4: Update Message entity — add MeetingId and AvailabilityCardId**

Add the following properties to the existing `Message.cs`, parallel to the existing `OfferId`/`Offer` pair:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public MessageType MessageType { get; set; }

    public Guid? OfferId { get; set; }

    public Guid? MeetingId { get; set; }

    public Guid? AvailabilityCardId { get; set; }

    public virtual Offer? Offer { get; set; }

    public virtual Meeting? Meeting { get; set; }

    public virtual AvailabilityCard? AvailabilityCard { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat(domain): add Meeting, AvailabilityCard, AvailabilitySlot entities and update Message

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: EF Core Configurations

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/MeetingConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/AvailabilityCardConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/AvailabilitySlotConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs`

- [ ] **Step 1: Create MeetingConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.Property(m => m.LocationLat)
            .HasPrecision(10, 7);

        builder.Property(m => m.LocationLng)
            .HasPrecision(10, 7);

        builder.HasOne(m => m.Conversation)
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Initiator)
            .WithMany()
            .HasForeignKey(m => m.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ParentMeeting)
            .WithMany(m => m.CounterMeetings)
            .HasForeignKey(m => m.ParentMeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Message)
            .WithOne(msg => msg.Meeting)
            .HasForeignKey<Message>(msg => msg.MeetingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

- [ ] **Step 2: Create AvailabilityCardConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class AvailabilityCardConfiguration : IEntityTypeConfiguration<AvailabilityCard>
{
    public void Configure(EntityTypeBuilder<AvailabilityCard> builder)
    {
        builder.HasOne(a => a.Conversation)
            .WithMany()
            .HasForeignKey(a => a.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Initiator)
            .WithMany()
            .HasForeignKey(a => a.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Message)
            .WithOne(msg => msg.AvailabilityCard)
            .HasForeignKey<Message>(msg => msg.AvailabilityCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

- [ ] **Step 3: Create AvailabilitySlotConfiguration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.HasOne(s => s.AvailabilityCard)
            .WithMany(a => a.Slots)
            .HasForeignKey(s => s.AvailabilityCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 4: Update MessageConfiguration — add MeetingId and AvailabilityCardId**

Add to the existing `Configure` method in `MessageConfiguration.cs`:

```csharp
builder.Property(m => m.MeetingId)
    .IsRequired(false);

builder.Property(m => m.AvailabilityCardId)
    .IsRequired(false);
```

The full updated file:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.MessageType)
            .HasDefaultValue(MessageType.Text)
            .HasSentinel(MessageType.Text);

        builder.Property(m => m.OfferId)
            .IsRequired(false);

        builder.Property(m => m.MeetingId)
            .IsRequired(false);

        builder.Property(m => m.AvailabilityCardId)
            .IsRequired(false);
    }
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat(infrastructure): add EF Core configurations for Meeting, AvailabilityCard, AvailabilitySlot

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: DbContext + Migration

**Files:**
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Add DbSets to AutomotiveContext**

Add the following properties to `AutomotiveContext.cs`, alongside the existing `Offers` DbSet:

```csharp
public DbSet<Meeting> Meetings { get; set; }

public DbSet<AvailabilityCard> AvailabilityCards { get; set; }

public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }
```

- [ ] **Step 2: Create EF migration**

Run: `dotnet ef migrations add AddMeetupEntities --project Automotive.Marketplace.Infrastructure --startup-project Automotive.Marketplace.Server`
Expected: Migration files created in `Infrastructure/Migrations/`

- [ ] **Step 3: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat(infrastructure): add Meetings, AvailabilityCards, AvailabilitySlots DbSets and migration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Bogus Builders

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/MeetingBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/AvailabilityCardBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/AvailabilitySlotBuilder.cs`

- [ ] **Step 1: Create MeetingBuilder**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MeetingBuilder
{
    private readonly Faker<Meeting> _faker;

    public MeetingBuilder()
    {
        _faker = new Faker<Meeting>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.ConversationId, f => f.Random.Guid())
            .RuleFor(m => m.InitiatorId, f => f.Random.Guid())
            .RuleFor(m => m.ProposedAt, _ => DateTime.UtcNow.AddDays(3))
            .RuleFor(m => m.DurationMinutes, 60)
            .RuleFor(m => m.LocationText, f => f.Address.FullAddress())
            .RuleFor(m => m.Status, MeetingStatus.Pending)
            .RuleFor(m => m.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(m => m.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(m => m.CreatedBy, f => f.Random.Guid().ToString());
    }

    public MeetingBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(m => m.ConversationId, conversationId);
        return this;
    }

    public MeetingBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(m => m.InitiatorId, initiatorId);
        return this;
    }

    public MeetingBuilder WithProposedAt(DateTime proposedAt)
    {
        _faker.RuleFor(m => m.ProposedAt, proposedAt);
        return this;
    }

    public MeetingBuilder WithStatus(MeetingStatus status)
    {
        _faker.RuleFor(m => m.Status, status);
        return this;
    }

    public MeetingBuilder WithParent(Guid parentMeetingId)
    {
        _faker.RuleFor(m => m.ParentMeetingId, parentMeetingId);
        return this;
    }

    public MeetingBuilder AsExpired()
    {
        _faker.RuleFor(m => m.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public MeetingBuilder With<T>(Expression<Func<Meeting, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Meeting Build() => _faker.Generate();
}
```

- [ ] **Step 2: Create AvailabilityCardBuilder**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class AvailabilityCardBuilder
{
    private readonly Faker<AvailabilityCard> _faker;

    public AvailabilityCardBuilder()
    {
        _faker = new Faker<AvailabilityCard>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.ConversationId, f => f.Random.Guid())
            .RuleFor(a => a.InitiatorId, f => f.Random.Guid())
            .RuleFor(a => a.Status, AvailabilityCardStatus.Pending)
            .RuleFor(a => a.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(a => a.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(a => a.CreatedBy, f => f.Random.Guid().ToString());
    }

    public AvailabilityCardBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(a => a.ConversationId, conversationId);
        return this;
    }

    public AvailabilityCardBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(a => a.InitiatorId, initiatorId);
        return this;
    }

    public AvailabilityCardBuilder WithStatus(AvailabilityCardStatus status)
    {
        _faker.RuleFor(a => a.Status, status);
        return this;
    }

    public AvailabilityCardBuilder AsExpired()
    {
        _faker.RuleFor(a => a.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public AvailabilityCardBuilder With<T>(Expression<Func<AvailabilityCard, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public AvailabilityCard Build() => _faker.Generate();
}
```

- [ ] **Step 3: Create AvailabilitySlotBuilder**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class AvailabilitySlotBuilder
{
    private readonly Faker<AvailabilitySlot> _faker;

    public AvailabilitySlotBuilder()
    {
        _faker = new Faker<AvailabilitySlot>()
            .RuleFor(s => s.Id, f => f.Random.Guid())
            .RuleFor(s => s.AvailabilityCardId, f => f.Random.Guid())
            .RuleFor(s => s.StartTime, _ => DateTime.UtcNow.AddDays(2))
            .RuleFor(s => s.EndTime, _ => DateTime.UtcNow.AddDays(2).AddHours(1))
            .RuleFor(s => s.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedBy, f => f.Random.Guid().ToString());
    }

    public AvailabilitySlotBuilder WithCard(Guid cardId)
    {
        _faker.RuleFor(s => s.AvailabilityCardId, cardId);
        return this;
    }

    public AvailabilitySlotBuilder WithTimes(DateTime start, DateTime end)
    {
        _faker.RuleFor(s => s.StartTime, start);
        _faker.RuleFor(s => s.EndTime, end);
        return this;
    }

    public AvailabilitySlotBuilder With<T>(Expression<Func<AvailabilitySlot, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public AvailabilitySlot Build() => _faker.Generate();
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(infrastructure): add MeetingBuilder, AvailabilityCardBuilder, AvailabilitySlotBuilder

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: ProposeMeeting Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ProposeMeeting/ProposeMeetingResponse.cs`

- [ ] **Step 1: Create ProposeMeetingCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public sealed record ProposeMeetingCommand : IRequest<ProposeMeetingResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public DateTime ProposedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? LocationText { get; set; }

    public decimal? LocationLat { get; set; }

    public decimal? LocationLng { get; set; }

    public Guid? ParentMeetingId { get; set; }
}
```

- [ ] **Step 2: Create ProposeMeetingResponse**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public sealed record ProposeMeetingResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public MeetingData Meeting { get; set; } = null!;

    public sealed record MeetingData
    {
        public Guid Id { get; set; }

        public DateTime ProposedAt { get; set; }

        public int DurationMinutes { get; set; }

        public string? LocationText { get; set; }

        public decimal? LocationLat { get; set; }

        public decimal? LocationLng { get; set; }

        public MeetingStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid InitiatorId { get; set; }

        public Guid? ParentMeetingId { get; set; }
    }
}
```

- [ ] **Step 3: Create ProposeMeetingCommandHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public class ProposeMeetingCommandHandler(IRepository repository)
    : IRequestHandler<ProposeMeetingCommand, ProposeMeetingResponse>
{
    public async Task<ProposeMeetingResponse> Handle(
        ProposeMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isSeller && conversation.BuyerId != request.InitiatorId)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may propose a meeting.");

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);

            if (!buyerHasLiked)
                throw new UnauthorizedAccessException(
                    "Seller can only propose a meeting if the buyer has liked the listing.");
        }

        if (request.ProposedAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("ProposedAt", "Proposed meeting time must be in the future.")
            ]);

        var hasActiveNegotiation = await repository.AsQueryable<Meeting>()
            .AnyAsync(m => m.ConversationId == request.ConversationId
                        && m.Status == MeetingStatus.Pending, cancellationToken)
            || await repository.AsQueryable<AvailabilityCard>()
            .AnyAsync(a => a.ConversationId == request.ConversationId
                        && a.Status == AvailabilityCardStatus.Pending, cancellationToken);

        if (hasActiveNegotiation)
            throw new RequestValidationException(
            [
                new ValidationFailure("ConversationId",
                    "There is already an active meetup negotiation in this conversation.")
            ]);

        var recipientId = isSeller ? conversation.BuyerId : listing.SellerId;
        var senderUsername = isSeller
            ? listing.Seller.Username
            : conversation.Buyer.Username;

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            ProposedAt = request.ProposedAt,
            DurationMinutes = request.DurationMinutes,
            LocationText = request.LocationText,
            LocationLat = request.LocationLat,
            LocationLng = request.LocationLng,
            Status = MeetingStatus.Pending,
            ParentMeetingId = request.ParentMeetingId,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Meeting,
            MeetingId = meeting.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(meeting, cancellationToken);
        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new ProposeMeetingResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            Meeting = new ProposeMeetingResponse.MeetingData
            {
                Id = meeting.Id,
                ProposedAt = meeting.ProposedAt,
                DurationMinutes = meeting.DurationMinutes,
                LocationText = meeting.LocationText,
                LocationLat = meeting.LocationLat,
                LocationLng = meeting.LocationLng,
                Status = meeting.Status,
                ExpiresAt = meeting.ExpiresAt,
                InitiatorId = meeting.InitiatorId,
                ParentMeetingId = meeting.ParentMeetingId
            }
        };
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(application): add ProposeMeeting command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: ProposeMeeting Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/ProposeMeetingCommandHandlerTests.cs`

- [ ] **Step 1: Write ProposeMeetingCommandHandlerTests**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
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

public class ProposeMeetingCommandHandlerTests(
    DatabaseFixture<ProposeMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ProposeMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ProposeMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private ProposeMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerProposesValidMeeting_ShouldPersistMeetingAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var proposedAt = DateTime.UtcNow.AddDays(3);
        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = proposedAt,
            DurationMinutes = 60,
            LocationText = "Central Park"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.Meeting.Id.Should().NotBeEmpty();
        result.Meeting.ProposedAt.Should().Be(proposedAt);
        result.Meeting.DurationMinutes.Should().Be(60);
        result.Meeting.LocationText.Should().Be("Central Park");
        result.Meeting.Status.Should().Be(MeetingStatus.Pending);
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().NotBe(buyer.Id);

        var savedMeeting = await context.Meetings.FindAsync(result.Meeting.Id);
        savedMeeting.Should().NotBeNull();
        savedMeeting!.Status.Should().Be(MeetingStatus.Pending);
        savedMeeting.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Meeting);
        savedMessage.MeetingId.Should().Be(result.Meeting.Id);
    }

    [Fact]
    public async Task Handle_SellerProposesWhenBuyerLiked_ShouldSucceed()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var like = new UserListingLikeBuilder()
            .WithUser(buyer.Id)
            .WithListing(listing.Id)
            .Build();
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 30
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Meeting.Id.Should().NotBeEmpty();
        result.SenderId.Should().Be(seller.Id);
        result.RecipientId.Should().Be(buyer.Id);
    }

    [Fact]
    public async Task Handle_SellerProposesWithoutBuyerLike_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PastProposedAt_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddHours(-1),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ProposedAt"));
    }

    [Fact]
    public async Task Handle_ActiveMeetingExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingMeeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(existingMeeting);
        await context.SaveChangesAsync();

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
    }

    [Fact]
    public async Task Handle_ActiveAvailabilityCardExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingCard = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(existingCard);
        await context.SaveChangesAsync();

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyInitiator_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = Guid.NewGuid(),
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 60
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

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ProposeMeetingCommandHandlerTests" ./Automotive.Marketplace.sln -v n`
Expected: All 7 tests pass

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "test: add ProposeMeetingCommandHandlerTests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: RespondToMeeting Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToMeeting/RespondToMeetingResponse.cs`

- [ ] **Step 1: Create RespondToMeetingCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public sealed record RespondToMeetingCommand : IRequest<RespondToMeetingResponse>
{
    public Guid MeetingId { get; set; }

    public Guid ResponderId { get; set; }

    public MeetingResponseAction Action { get; set; }

    public RescheduleData? Reschedule { get; set; }

    public sealed record RescheduleData
    {
        public DateTime ProposedAt { get; set; }

        public int DurationMinutes { get; set; }

        public string? LocationText { get; set; }

        public decimal? LocationLat { get; set; }

        public decimal? LocationLng { get; set; }
    }
}

public enum MeetingResponseAction
{
    Accept,
    Decline,
    Reschedule
}
```

- [ ] **Step 2: Create RespondToMeetingResponse**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public sealed record RespondToMeetingResponse
{
    public Guid MeetingId { get; set; }

    public Guid ConversationId { get; set; }

    public MeetingStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ResponderId { get; set; }

    public ProposeMeeting.ProposeMeetingResponse? RescheduledMeeting { get; set; }
}
```

- [ ] **Step 3: Create RespondToMeetingCommandHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public class RespondToMeetingCommandHandler(IRepository repository)
    : IRequestHandler<RespondToMeetingCommand, RespondToMeetingResponse>
{
    public async Task<RespondToMeetingResponse> Handle(
        RespondToMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync<Meeting>(request.MeetingId, cancellationToken);
        var conversation = meeting.Conversation;
        var listing = conversation.Listing;

        if (meeting.Status != MeetingStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId", "This meeting proposal has already been resolved.")
            ]);

        if (meeting.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own meeting proposal.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond to a meeting proposal.");

        if (meeting.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId", "This meeting proposal has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        if (request.Action == MeetingResponseAction.Accept)
        {
            meeting.Status = MeetingStatus.Accepted;
            await repository.UpdateAsync(meeting, cancellationToken);

            return new RespondToMeetingResponse
            {
                MeetingId = meeting.Id,
                ConversationId = conversation.Id,
                NewStatus = MeetingStatus.Accepted,
                InitiatorId = meeting.InitiatorId,
                ResponderId = request.ResponderId,
                RescheduledMeeting = null
            };
        }

        if (request.Action == MeetingResponseAction.Decline)
        {
            meeting.Status = MeetingStatus.Declined;
            await repository.UpdateAsync(meeting, cancellationToken);

            return new RespondToMeetingResponse
            {
                MeetingId = meeting.Id,
                ConversationId = conversation.Id,
                NewStatus = MeetingStatus.Declined,
                InitiatorId = meeting.InitiatorId,
                ResponderId = request.ResponderId,
                RescheduledMeeting = null
            };
        }

        // Reschedule
        if (request.Reschedule is null)
            throw new RequestValidationException(
            [
                new ValidationFailure("Reschedule", "Reschedule data is required when action is Reschedule.")
            ]);

        if (request.Reschedule.ProposedAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("Reschedule.ProposedAt", "Rescheduled meeting time must be in the future.")
            ]);

        meeting.Status = MeetingStatus.Rescheduled;

        var rescheduledMeeting = new Meeting
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            ProposedAt = request.Reschedule.ProposedAt,
            DurationMinutes = request.Reschedule.DurationMinutes,
            LocationText = request.Reschedule.LocationText,
            LocationLat = request.Reschedule.LocationLat,
            LocationLng = request.Reschedule.LocationLng,
            Status = MeetingStatus.Pending,
            ParentMeetingId = meeting.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        var rescheduledMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Meeting,
            MeetingId = rescheduledMeeting.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = rescheduledMessage.SentAt;

        await repository.UpdateAsync(meeting, cancellationToken);
        await repository.CreateAsync(rescheduledMeeting, cancellationToken);
        await repository.CreateAsync(rescheduledMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RespondToMeetingResponse
        {
            MeetingId = meeting.Id,
            ConversationId = conversation.Id,
            NewStatus = MeetingStatus.Rescheduled,
            InitiatorId = meeting.InitiatorId,
            ResponderId = request.ResponderId,
            RescheduledMeeting = new ProposeMeetingResponse
            {
                MessageId = rescheduledMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = rescheduledMessage.SentAt,
                RecipientId = meeting.InitiatorId,
                Meeting = new ProposeMeetingResponse.MeetingData
                {
                    Id = rescheduledMeeting.Id,
                    ProposedAt = rescheduledMeeting.ProposedAt,
                    DurationMinutes = rescheduledMeeting.DurationMinutes,
                    LocationText = rescheduledMeeting.LocationText,
                    LocationLat = rescheduledMeeting.LocationLat,
                    LocationLng = rescheduledMeeting.LocationLng,
                    Status = MeetingStatus.Pending,
                    ExpiresAt = rescheduledMeeting.ExpiresAt,
                    InitiatorId = request.ResponderId,
                    ParentMeetingId = rescheduledMeeting.ParentMeetingId
                }
            }
        };
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(application): add RespondToMeeting command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: RespondToMeeting Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToMeetingCommandHandlerTests.cs`

- [ ] **Step 1: Write RespondToMeetingCommandHandlerTests**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;
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

public class RespondToMeetingCommandHandlerTests(
    DatabaseFixture<RespondToMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_AcceptMeeting_ShouldSetStatusAccepted()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = (await context.Conversations.FindAsync(meeting.ConversationId))!.Listing.SellerId,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Accepted);
        result.RescheduledMeeting.Should().BeNull();

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Accepted);
    }

    [Fact]
    public async Task Handle_DeclineMeeting_ShouldSetStatusDeclined()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Decline
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Declined);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Declined);
    }

    [Fact]
    public async Task Handle_RescheduleMeeting_ShouldCreateNewMeetingAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var newTime = DateTime.UtcNow.AddDays(5);
        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Reschedule,
            Reschedule = new RespondToMeetingCommand.RescheduleData
            {
                ProposedAt = newTime,
                DurationMinutes = 90,
                LocationText = "New location"
            }
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Rescheduled);
        result.RescheduledMeeting.Should().NotBeNull();
        result.RescheduledMeeting!.Meeting.ProposedAt.Should().Be(newTime);
        result.RescheduledMeeting.Meeting.DurationMinutes.Should().Be(90);
        result.RescheduledMeeting.Meeting.ParentMeetingId.Should().Be(meeting.Id);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Rescheduled);

        var newMeeting = await context.Meetings.FindAsync(result.RescheduledMeeting.Meeting.Id);
        newMeeting.Should().NotBeNull();
        newMeeting!.Status.Should().Be(MeetingStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.RescheduledMeeting.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Meeting);
        newMessage.MeetingId.Should().Be(newMeeting.Id);
    }

    [Fact]
    public async Task Handle_MeetingAlreadyResolved_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        meeting.Status = MeetingStatus.Accepted;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Decline
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnMeeting_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = buyer.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_MeetingExpired_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        meeting.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_RescheduleWithPastTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Reschedule,
            Reschedule = new RespondToMeetingCommand.RescheduleData
            {
                ProposedAt = DateTime.UtcNow.AddHours(-1),
                DurationMinutes = 60
            }
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_NonParticipantResponder_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        var outsider = new UserBuilder().Build();
        await context.AddAsync(outsider);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = outsider.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Meeting meeting, Listing listing)>
        SeedPendingMeetingAsync(AutomotiveContext context, bool initiatedByBuyer)
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

        var initiatorId = initiatedByBuyer ? buyer.Id : seller.Id;
        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation, meeting);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, meeting, listing);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~RespondToMeetingCommandHandlerTests" ./Automotive.Marketplace.sln -v n`
Expected: All 8 tests pass

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "test: add RespondToMeetingCommandHandlerTests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: ShareAvailability Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ShareAvailability/ShareAvailabilityResponse.cs`

- [ ] **Step 1: Create ShareAvailabilityCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public sealed record ShareAvailabilityCommand : IRequest<ShareAvailabilityResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public List<SlotData> Slots { get; set; } = [];

    public sealed record SlotData
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
```

- [ ] **Step 2: Create ShareAvailabilityResponse**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public sealed record ShareAvailabilityResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public AvailabilityCardData AvailabilityCard { get; set; } = null!;

    public sealed record AvailabilityCardData
    {
        public Guid Id { get; set; }

        public AvailabilityCardStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid InitiatorId { get; set; }

        public List<SlotData> Slots { get; set; } = [];

        public sealed record SlotData
        {
            public Guid Id { get; set; }

            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }
        }
    }
}
```

- [ ] **Step 3: Create ShareAvailabilityCommandHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public class ShareAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<ShareAvailabilityCommand, ShareAvailabilityResponse>
{
    public async Task<ShareAvailabilityResponse> Handle(
        ShareAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isSeller && conversation.BuyerId != request.InitiatorId)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may share availability.");

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);

            if (!buyerHasLiked)
                throw new UnauthorizedAccessException(
                    "Seller can only share availability if the buyer has liked the listing.");
        }

        if (request.Slots.Count == 0)
            throw new RequestValidationException(
            [
                new ValidationFailure("Slots", "At least one time slot is required.")
            ]);

        foreach (var slot in request.Slots)
        {
            if (slot.StartTime <= DateTime.UtcNow)
                throw new RequestValidationException(
                [
                    new ValidationFailure("Slots.StartTime", "All slot start times must be in the future.")
                ]);

            if (slot.EndTime <= slot.StartTime)
                throw new RequestValidationException(
                [
                    new ValidationFailure("Slots.EndTime", "Slot end time must be after start time.")
                ]);
        }

        var hasActiveNegotiation = await repository.AsQueryable<Meeting>()
            .AnyAsync(m => m.ConversationId == request.ConversationId
                        && m.Status == MeetingStatus.Pending, cancellationToken)
            || await repository.AsQueryable<AvailabilityCard>()
            .AnyAsync(a => a.ConversationId == request.ConversationId
                        && a.Status == AvailabilityCardStatus.Pending, cancellationToken);

        if (hasActiveNegotiation)
            throw new RequestValidationException(
            [
                new ValidationFailure("ConversationId",
                    "There is already an active meetup negotiation in this conversation.")
            ]);

        var recipientId = isSeller ? conversation.BuyerId : listing.SellerId;
        var senderUsername = isSeller
            ? listing.Seller.Username
            : conversation.Buyer.Username;

        var card = new AvailabilityCard
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Status = AvailabilityCardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        await repository.CreateAsync(card, cancellationToken);

        var slotEntities = new List<AvailabilitySlot>();
        foreach (var slot in request.Slots)
        {
            var slotEntity = new AvailabilitySlot
            {
                Id = Guid.NewGuid(),
                AvailabilityCardId = card.Id,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.InitiatorId.ToString()
            };
            await repository.CreateAsync(slotEntity, cancellationToken);
            slotEntities.Add(slotEntity);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Availability,
            AvailabilityCardId = card.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new ShareAvailabilityResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            AvailabilityCard = new ShareAvailabilityResponse.AvailabilityCardData
            {
                Id = card.Id,
                Status = card.Status,
                ExpiresAt = card.ExpiresAt,
                InitiatorId = card.InitiatorId,
                Slots = slotEntities.Select(s => new ShareAvailabilityResponse.AvailabilityCardData.SlotData
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            }
        };
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(application): add ShareAvailability command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: ShareAvailability Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/ShareAvailabilityCommandHandlerTests.cs`

- [ ] **Step 1: Write ShareAvailabilityCommandHandlerTests**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class ShareAvailabilityCommandHandlerTests(
    DatabaseFixture<ShareAvailabilityCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ShareAvailabilityCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ShareAvailabilityCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private ShareAvailabilityCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerSharesValidSlots_ShouldPersistCardSlotsAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var start1 = DateTime.UtcNow.AddDays(2);
        var end1 = start1.AddHours(2);
        var start2 = DateTime.UtcNow.AddDays(3);
        var end2 = start2.AddHours(1);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData { StartTime = start1, EndTime = end1 },
                new ShareAvailabilityCommand.SlotData { StartTime = start2, EndTime = end2 }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.AvailabilityCard.Id.Should().NotBeEmpty();
        result.AvailabilityCard.Status.Should().Be(AvailabilityCardStatus.Pending);
        result.AvailabilityCard.Slots.Should().HaveCount(2);
        result.SenderId.Should().Be(buyer.Id);

        var savedCard = await context.AvailabilityCards.FindAsync(result.AvailabilityCard.Id);
        savedCard.Should().NotBeNull();
        savedCard!.Status.Should().Be(AvailabilityCardStatus.Pending);

        var savedSlots = await context.AvailabilitySlots
            .Where(s => s.AvailabilityCardId == savedCard.Id)
            .ToListAsync();
        savedSlots.Should().HaveCount(2);

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Availability);
        savedMessage.AvailabilityCardId.Should().Be(savedCard.Id);
    }

    [Fact]
    public async Task Handle_NoSlots_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots = []
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots"));
    }

    [Fact]
    public async Task Handle_PastSlotStartTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(1)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots.StartTime"));
    }

    [Fact]
    public async Task Handle_EndTimeBeforeStartTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var start = DateTime.UtcNow.AddDays(2);
        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = start,
                    EndTime = start.AddMinutes(-30)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots.EndTime"));
    }

    [Fact]
    public async Task Handle_ActiveMeetingNegotiationExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingMeeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(existingMeeting);
        await context.SaveChangesAsync();

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(1)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
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

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~ShareAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln -v n`
Expected: All 5 tests pass

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "test: add ShareAvailabilityCommandHandlerTests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 12: RespondToAvailability Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToAvailability/RespondToAvailabilityResponse.cs`

- [ ] **Step 1: Create RespondToAvailabilityCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public sealed record RespondToAvailabilityCommand : IRequest<RespondToAvailabilityResponse>
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ResponderId { get; set; }

    public AvailabilityResponseAction Action { get; set; }

    public Guid? SlotId { get; set; }

    public List<ShareBackSlot>? ShareBackSlots { get; set; }

    public sealed record ShareBackSlot
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}

public enum AvailabilityResponseAction
{
    PickSlot,
    ShareBack
}
```

- [ ] **Step 2: Create RespondToAvailabilityResponse**

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public sealed record RespondToAvailabilityResponse
{
    public Guid AvailabilityCardId { get; set; }

    public Guid ConversationId { get; set; }

    public AvailabilityResponseAction Action { get; set; }

    public ProposeMeetingResponse? PickedSlotMeeting { get; set; }

    public ShareAvailabilityResponse? SharedBackAvailability { get; set; }
}
```

- [ ] **Step 3: Create RespondToAvailabilityCommandHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public class RespondToAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<RespondToAvailabilityCommand, RespondToAvailabilityResponse>
{
    public async Task<RespondToAvailabilityResponse> Handle(
        RespondToAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<AvailabilityCard>(
            request.AvailabilityCardId, cancellationToken);
        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (card.Status != AvailabilityCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId", "This availability card has already been responded to.")
            ]);

        if (card.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own availability card.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond to an availability card.");

        if (card.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId", "This availability card has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        var recipientId = card.InitiatorId;

        if (request.Action == AvailabilityResponseAction.PickSlot)
        {
            if (request.SlotId is null)
                throw new RequestValidationException(
                [
                    new ValidationFailure("SlotId", "SlotId is required when action is PickSlot.")
                ]);

            var slot = card.Slots.FirstOrDefault(s => s.Id == request.SlotId.Value)
                ?? throw new RequestValidationException(
                [
                    new ValidationFailure("SlotId", "The specified slot does not belong to this availability card.")
                ]);

            card.Status = AvailabilityCardStatus.Responded;
            await repository.UpdateAsync(card, cancellationToken);

            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                InitiatorId = request.ResponderId,
                ProposedAt = slot.StartTime,
                DurationMinutes = (int)(slot.EndTime - slot.StartTime).TotalMinutes,
                Status = MeetingStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                Content = string.Empty,
                MessageType = MessageType.Meeting,
                MeetingId = meeting.Id,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };

            conversation.LastMessageAt = message.SentAt;

            await repository.CreateAsync(meeting, cancellationToken);
            await repository.CreateAsync(message, cancellationToken);
            await repository.UpdateAsync(conversation, cancellationToken);

            return new RespondToAvailabilityResponse
            {
                AvailabilityCardId = card.Id,
                ConversationId = conversation.Id,
                Action = AvailabilityResponseAction.PickSlot,
                PickedSlotMeeting = new ProposeMeetingResponse
                {
                    MessageId = message.Id,
                    ConversationId = conversation.Id,
                    SenderId = request.ResponderId,
                    SenderUsername = responderUsername,
                    SentAt = message.SentAt,
                    RecipientId = recipientId,
                    Meeting = new ProposeMeetingResponse.MeetingData
                    {
                        Id = meeting.Id,
                        ProposedAt = meeting.ProposedAt,
                        DurationMinutes = meeting.DurationMinutes,
                        LocationText = meeting.LocationText,
                        LocationLat = meeting.LocationLat,
                        LocationLng = meeting.LocationLng,
                        Status = meeting.Status,
                        ExpiresAt = meeting.ExpiresAt,
                        InitiatorId = meeting.InitiatorId,
                        ParentMeetingId = meeting.ParentMeetingId
                    }
                },
                SharedBackAvailability = null
            };
        }

        // ShareBack
        if (request.ShareBackSlots is null || request.ShareBackSlots.Count == 0)
            throw new RequestValidationException(
            [
                new ValidationFailure("ShareBackSlots", "At least one time slot is required for ShareBack.")
            ]);

        foreach (var slot in request.ShareBackSlots)
        {
            if (slot.StartTime <= DateTime.UtcNow)
                throw new RequestValidationException(
                [
                    new ValidationFailure("ShareBackSlots.StartTime", "All slot start times must be in the future.")
                ]);

            if (slot.EndTime <= slot.StartTime)
                throw new RequestValidationException(
                [
                    new ValidationFailure("ShareBackSlots.EndTime", "Slot end time must be after start time.")
                ]);
        }

        card.Status = AvailabilityCardStatus.Responded;
        await repository.UpdateAsync(card, cancellationToken);

        var newCard = new AvailabilityCard
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            Status = AvailabilityCardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        await repository.CreateAsync(newCard, cancellationToken);

        var slotEntities = new List<AvailabilitySlot>();
        foreach (var slot in request.ShareBackSlots)
        {
            var slotEntity = new AvailabilitySlot
            {
                Id = Guid.NewGuid(),
                AvailabilityCardId = newCard.Id,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };
            await repository.CreateAsync(slotEntity, cancellationToken);
            slotEntities.Add(slotEntity);
        }

        var shareBackMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Availability,
            AvailabilityCardId = newCard.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = shareBackMessage.SentAt;

        await repository.CreateAsync(shareBackMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RespondToAvailabilityResponse
        {
            AvailabilityCardId = card.Id,
            ConversationId = conversation.Id,
            Action = AvailabilityResponseAction.ShareBack,
            PickedSlotMeeting = null,
            SharedBackAvailability = new ShareAvailabilityResponse
            {
                MessageId = shareBackMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = shareBackMessage.SentAt,
                RecipientId = recipientId,
                AvailabilityCard = new ShareAvailabilityResponse.AvailabilityCardData
                {
                    Id = newCard.Id,
                    Status = newCard.Status,
                    ExpiresAt = newCard.ExpiresAt,
                    InitiatorId = newCard.InitiatorId,
                    Slots = slotEntities.Select(s => new ShareAvailabilityResponse.AvailabilityCardData.SlotData
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToList()
                }
            }
        };
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(application): add RespondToAvailability command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: RespondToAvailability Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToAvailabilityCommandHandlerTests.cs`

- [ ] **Step 1: Write RespondToAvailabilityCommandHandlerTests**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class RespondToAvailabilityCommandHandlerTests(
    DatabaseFixture<RespondToAvailabilityCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToAvailabilityCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToAvailabilityCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToAvailabilityCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_PickSlot_ShouldCreateMeetingAndMarkCardResponded()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        result.Action.Should().Be(AvailabilityResponseAction.PickSlot);
        result.PickedSlotMeeting.Should().NotBeNull();
        result.PickedSlotMeeting!.Meeting.ProposedAt.Should().Be(slot.StartTime);
        result.PickedSlotMeeting.Meeting.Status.Should().Be(MeetingStatus.Pending);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(AvailabilityCardStatus.Responded);

        var newMeeting = await context.Meetings.FindAsync(result.PickedSlotMeeting.Meeting.Id);
        newMeeting.Should().NotBeNull();
        newMeeting!.Status.Should().Be(MeetingStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.PickedSlotMeeting.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Meeting);
    }

    [Fact]
    public async Task Handle_ShareBack_ShouldCreateNewCardAndMarkOldCardResponded()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var newStart = DateTime.UtcNow.AddDays(4);
        var result = await handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.ShareBack,
            ShareBackSlots =
            [
                new RespondToAvailabilityCommand.ShareBackSlot
                {
                    StartTime = newStart,
                    EndTime = newStart.AddHours(2)
                }
            ]
        }, CancellationToken.None);

        result.Action.Should().Be(AvailabilityResponseAction.ShareBack);
        result.SharedBackAvailability.Should().NotBeNull();
        result.SharedBackAvailability!.AvailabilityCard.Slots.Should().HaveCount(1);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(AvailabilityCardStatus.Responded);

        var newCard = await context.AvailabilityCards.FindAsync(result.SharedBackAvailability.AvailabilityCard.Id);
        newCard.Should().NotBeNull();
        newCard!.Status.Should().Be(AvailabilityCardStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.SharedBackAvailability.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Availability);
    }

    [Fact]
    public async Task Handle_CardAlreadyResponded_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);
        card.Status = AvailabilityCardStatus.Responded;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnCard_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NonParticipantResponder_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);
        var outsider = new UserBuilder().Build();
        await context.AddAsync(outsider);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = outsider.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PickSlotWithInvalidSlotId_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, AvailabilityCard card, AvailabilitySlot slot)>
        SeedPendingAvailabilityAsync(AutomotiveContext context, bool initiatedByBuyer)
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

        var initiatorId = initiatedByBuyer ? buyer.Id : seller.Id;
        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .Build();

        var slot = new AvailabilitySlotBuilder()
            .WithCard(card.Id)
            .WithTimes(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1))
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation, card, slot);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card, slot);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test --filter "FullyQualifiedName~RespondToAvailabilityCommandHandlerTests" ./Automotive.Marketplace.sln -v n`
Expected: All 6 tests pass

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "test: add RespondToAvailabilityCommandHandlerTests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 14: Update GetMessages Handler + Response

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs`

- [ ] **Step 1: Update GetMessagesResponse — add MeetingData and AvailabilityCardData nested records**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public sealed record GetMessagesResponse
{
    public Guid ConversationId { get; set; }

    public List<Message> Messages { get; set; } = [];

    public sealed record Message
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public MessageType MessageType { get; set; }

        public OfferData? Offer { get; set; }

        public MeetingData? Meeting { get; set; }

        public AvailabilityCardData? AvailabilityCard { get; set; }

        public sealed record OfferData
        {
            public Guid Id { get; set; }

            public decimal Amount { get; set; }

            public decimal ListingPrice { get; set; }

            public decimal PercentageOff { get; set; }

            public OfferStatus Status { get; set; }

            public DateTime ExpiresAt { get; set; }

            public Guid InitiatorId { get; set; }

            public Guid? ParentOfferId { get; set; }
        }

        public sealed record MeetingData
        {
            public Guid Id { get; set; }

            public DateTime ProposedAt { get; set; }

            public int DurationMinutes { get; set; }

            public string? LocationText { get; set; }

            public decimal? LocationLat { get; set; }

            public decimal? LocationLng { get; set; }

            public MeetingStatus Status { get; set; }

            public DateTime ExpiresAt { get; set; }

            public Guid InitiatorId { get; set; }

            public Guid? ParentMeetingId { get; set; }
        }

        public sealed record AvailabilityCardData
        {
            public Guid Id { get; set; }

            public AvailabilityCardStatus Status { get; set; }

            public DateTime ExpiresAt { get; set; }

            public Guid InitiatorId { get; set; }

            public List<SlotData> Slots { get; set; } = [];

            public sealed record SlotData
            {
                public Guid Id { get; set; }

                public DateTime StartTime { get; set; }

                public DateTime EndTime { get; set; }
            }
        }
    }
}
```

- [ ] **Step 2: Update GetMessagesQueryHandler — map Meeting and AvailabilityCard data**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesQueryHandler(IRepository repository)
    : IRequestHandler<GetMessagesQuery, GetMessagesResponse>
{
    public async Task<GetMessagesResponse> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var isBuyer = conversation.BuyerId == request.UserId;
        var isSeller = conversation.Listing.SellerId == request.UserId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException(
                "You are not a participant in this conversation.");

        var listingPrice = conversation.Listing.Price;

        var messages = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new GetMessagesResponse.Message
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.Username,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                MessageType = m.MessageType,
                Offer = m.Offer is null ? null : new GetMessagesResponse.Message.OfferData
                {
                    Id = m.Offer.Id,
                    Amount = m.Offer.Amount,
                    ListingPrice = listingPrice,
                    PercentageOff = Math.Round(
                        (listingPrice - m.Offer.Amount) / listingPrice * 100, 2),
                    Status = m.Offer.Status,
                    ExpiresAt = m.Offer.ExpiresAt,
                    InitiatorId = m.Offer.InitiatorId,
                    ParentOfferId = m.Offer.ParentOfferId
                },
                Meeting = m.Meeting is null ? null : new GetMessagesResponse.Message.MeetingData
                {
                    Id = m.Meeting.Id,
                    ProposedAt = m.Meeting.ProposedAt,
                    DurationMinutes = m.Meeting.DurationMinutes,
                    LocationText = m.Meeting.LocationText,
                    LocationLat = m.Meeting.LocationLat,
                    LocationLng = m.Meeting.LocationLng,
                    Status = m.Meeting.Status,
                    ExpiresAt = m.Meeting.ExpiresAt,
                    InitiatorId = m.Meeting.InitiatorId,
                    ParentMeetingId = m.Meeting.ParentMeetingId
                },
                AvailabilityCard = m.AvailabilityCard is null ? null : new GetMessagesResponse.Message.AvailabilityCardData
                {
                    Id = m.AvailabilityCard.Id,
                    Status = m.AvailabilityCard.Status,
                    ExpiresAt = m.AvailabilityCard.ExpiresAt,
                    InitiatorId = m.AvailabilityCard.InitiatorId,
                    Slots = m.AvailabilityCard.Slots.Select(s => new GetMessagesResponse.Message.AvailabilityCardData.SlotData
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToList()
                }
            })
            .ToList();

        return new GetMessagesResponse
        {
            ConversationId = request.ConversationId,
            Messages = messages
        };
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat(application): update GetMessages to include Meeting and AvailabilityCard data

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 15: ChatHub Additions

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

- [ ] **Step 1: Add meeting and availability hub methods to ChatHub**

Add the following methods to the existing `ChatHub` class. Add the required `using` statements at the top of the file:

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;
```

New hub methods:

```csharp
public async Task ProposeMeeting(
    Guid conversationId, DateTime proposedAt, int durationMinutes,
    string? locationText = null, decimal? locationLat = null, decimal? locationLng = null)
{
    var result = await mediator.Send(new ProposeMeetingCommand
    {
        ConversationId = conversationId,
        InitiatorId = UserId,
        ProposedAt = proposedAt,
        DurationMinutes = durationMinutes,
        LocationText = locationText,
        LocationLat = locationLat,
        LocationLng = locationLng
    });

    await Clients.Group($"user-{UserId}").SendAsync("MeetingProposed", result);
    await Clients.Group($"user-{result.RecipientId}").SendAsync("MeetingProposed", result);
}

public async Task RespondToMeeting(Guid meetingId, string action,
    RespondToMeetingCommand.RescheduleData? rescheduleData = null)
{
    var result = await mediator.Send(new RespondToMeetingCommand
    {
        MeetingId = meetingId,
        ResponderId = UserId,
        Action = Enum.Parse<MeetingResponseAction>(action, ignoreCase: true),
        Reschedule = rescheduleData
    });

    var eventName = result.NewStatus switch
    {
        Domain.Enums.MeetingStatus.Accepted => "MeetingAccepted",
        Domain.Enums.MeetingStatus.Declined => "MeetingDeclined",
        Domain.Enums.MeetingStatus.Rescheduled => "MeetingRescheduled",
        _ => throw new InvalidOperationException($"Unexpected meeting status: {result.NewStatus}")
    };

    await Clients.Group($"user-{result.InitiatorId}").SendAsync(eventName, result);
    await Clients.Group($"user-{result.ResponderId}").SendAsync(eventName, result);
}

public async Task ShareAvailability(Guid conversationId,
    List<ShareAvailabilityCommand.SlotData> slots)
{
    var result = await mediator.Send(new ShareAvailabilityCommand
    {
        ConversationId = conversationId,
        InitiatorId = UserId,
        Slots = slots
    });

    await Clients.Group($"user-{UserId}").SendAsync("AvailabilityShared", result);
    await Clients.Group($"user-{result.RecipientId}").SendAsync("AvailabilityShared", result);
}

public async Task RespondToAvailability(Guid availabilityCardId, string action,
    Guid? slotId = null, List<RespondToAvailabilityCommand.ShareBackSlot>? shareBackSlots = null)
{
    var result = await mediator.Send(new RespondToAvailabilityCommand
    {
        AvailabilityCardId = availabilityCardId,
        ResponderId = UserId,
        Action = Enum.Parse<AvailabilityResponseAction>(action, ignoreCase: true),
        SlotId = slotId,
        ShareBackSlots = shareBackSlots
    });

    var initiatorId = result.Action == AvailabilityResponseAction.PickSlot
        ? result.PickedSlotMeeting!.RecipientId
        : result.SharedBackAvailability!.RecipientId;

    await Clients.Group($"user-{UserId}").SendAsync("AvailabilityResponded", result);
    await Clients.Group($"user-{initiatorId}").SendAsync("AvailabilityResponded", result);
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat(server): add meeting and availability hub methods to ChatHub

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 16: MeetingExpiryService

**Files:**
- Create: `Automotive.Marketplace.Server/Services/MeetingExpiryService.cs`
- Modify: `Automotive.Marketplace.Server/Program.cs` (register service)

- [ ] **Step 1: Create MeetingExpiryService**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Server.Services;

public class MeetingExpiryService(
    IServiceScopeFactory scopeFactory,
    IHubContext<ChatHub> hubContext,
    ILogger<MeetingExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            await ExpireMeetingsAsync(stoppingToken);
            await ExpireAvailabilityCardsAsync(stoppingToken);
        }
    }

    private async Task ExpireMeetingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredMeetings = await repository.AsQueryable<Meeting>()
                .Where(m => m.Status == MeetingStatus.Pending && m.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var meeting in expiredMeetings)
            {
                meeting.Status = MeetingStatus.Expired;
                await repository.UpdateAsync(meeting, cancellationToken);

                var conversation = meeting.Conversation;
                var recipientId = meeting.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    meetingId = meeting.Id,
                    conversationId = meeting.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{meeting.InitiatorId}")
                    .SendAsync("MeetingExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("MeetingExpired", payload, cancellationToken);
            }

            if (expiredMeetings.Count > 0)
                logger.LogInformation("Expired {Count} pending meetings.", expiredMeetings.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring meetings.");
        }
    }

    private async Task ExpireAvailabilityCardsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredCards = await repository.AsQueryable<AvailabilityCard>()
                .Where(a => a.Status == AvailabilityCardStatus.Pending && a.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var card in expiredCards)
            {
                card.Status = AvailabilityCardStatus.Expired;
                await repository.UpdateAsync(card, cancellationToken);

                var conversation = card.Conversation;
                var recipientId = card.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    availabilityCardId = card.Id,
                    conversationId = card.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{card.InitiatorId}")
                    .SendAsync("AvailabilityExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("AvailabilityExpired", payload, cancellationToken);
            }

            if (expiredCards.Count > 0)
                logger.LogInformation("Expired {Count} pending availability cards.", expiredCards.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring availability cards.");
        }
    }
}
```

- [ ] **Step 2: Register MeetingExpiryService in Program.cs**

Add to the service registration section, alongside the existing `OfferExpiryService` registration:

```csharp
builder.Services.AddHostedService<MeetingExpiryService>();
```

- [ ] **Step 3: Verify build**

Run: `dotnet build ./Automotive.Marketplace.sln --no-restore -v q`
Expected: Build succeeded

- [ ] **Step 4: Run all backend tests**

Run: `dotnet test ./Automotive.Marketplace.sln -v n`
Expected: All tests pass (existing + new)

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(server): add MeetingExpiryService for meeting and availability card expiration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 17: Frontend Types

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/types/Meeting.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/AvailabilityCard.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/MeetingEventPayloads.ts`

- [ ] **Step 1: Create Meeting.ts**

```typescript
export type MeetingStatus = 'Pending' | 'Accepted' | 'Declined' | 'Rescheduled' | 'Expired';

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

- [ ] **Step 2: Create AvailabilityCard.ts**

```typescript
export type AvailabilityCardStatus = 'Pending' | 'Responded' | 'Expired';

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

- [ ] **Step 3: Create MeetingEventPayloads.ts**

```typescript
import type { Meeting } from './Meeting';
import type { AvailabilityCard } from './AvailabilityCard';

export type MeetingProposedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  meeting: Meeting;
};

export type MeetingStatusUpdatedPayload = {
  meetingId: string;
  conversationId: string;
  newStatus: 'Accepted' | 'Declined';
  initiatorId: string;
  responderId: string;
  rescheduledMeeting: null;
};

export type MeetingRescheduledPayload = {
  meetingId: string;
  conversationId: string;
  newStatus: 'Rescheduled';
  initiatorId: string;
  responderId: string;
  rescheduledMeeting: MeetingProposedPayload;
};

export type MeetingExpiredPayload = {
  meetingId: string;
  conversationId: string;
};

export type AvailabilitySharedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  availabilityCard: AvailabilityCard;
};

export type AvailabilityRespondedPayload = {
  availabilityCardId: string;
  conversationId: string;
  action: 'PickSlot' | 'ShareBack';
  pickedSlotMeeting: MeetingProposedPayload | null;
  sharedBackAvailability: AvailabilitySharedPayload | null;
};

export type AvailabilityExpiredPayload = {
  availabilityCardId: string;
  conversationId: string;
};
```

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat(client): add Meeting, AvailabilityCard, and MeetingEventPayloads types

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 18: Update Frontend Message Types

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/ReceiveMessagePayload.ts`

- [ ] **Step 1: Update GetMessagesResponse.ts**

```typescript
import type { Offer } from './Offer';
import type { Meeting } from './Meeting';
import type { AvailabilityCard } from './AvailabilityCard';

export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};

export type Message = {
  id: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: 'Text' | 'Offer' | 'Meeting' | 'Availability';
  offer?: Offer;
  meeting?: Meeting;
  availabilityCard?: AvailabilityCard;
};
```

- [ ] **Step 2: Update ReceiveMessagePayload.ts**

```typescript
import type { Offer } from './Offer';
import type { Meeting } from './Meeting';
import type { AvailabilityCard } from './AvailabilityCard';

export type ReceiveMessagePayload = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: 'Text' | 'Offer' | 'Meeting' | 'Availability';
  offer?: Offer;
  meeting?: Meeting;
  availabilityCard?: AvailabilityCard;
};
```

- [ ] **Step 3: Run lint check**

Run (from `automotive.marketplace.client/`): `npm run lint && npm run format:check`
Expected: No errors

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat(client): update Message types to include Meeting and AvailabilityCard

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 19: chatHub Constants + useChatHub Additions

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/constants/chatHub.ts`
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

- [ ] **Step 1: Update chatHub.ts constants**

```typescript
export const HUB_METHODS = {
  // Client → Server
  SEND_MESSAGE: 'SendMessage',
  MAKE_OFFER: 'MakeOffer',
  RESPOND_TO_OFFER: 'RespondToOffer',
  PROPOSE_MEETING: 'ProposeMeeting',
  RESPOND_TO_MEETING: 'RespondToMeeting',
  SHARE_AVAILABILITY: 'ShareAvailability',
  RESPOND_TO_AVAILABILITY: 'RespondToAvailability',
  // Server → Client
  RECEIVE_MESSAGE: 'ReceiveMessage',
  UPDATE_UNREAD_COUNT: 'UpdateUnreadCount',
  OFFER_MADE: 'OfferMade',
  OFFER_ACCEPTED: 'OfferAccepted',
  OFFER_DECLINED: 'OfferDeclined',
  OFFER_COUNTERED: 'OfferCountered',
  OFFER_EXPIRED: 'OfferExpired',
  MEETING_PROPOSED: 'MeetingProposed',
  MEETING_ACCEPTED: 'MeetingAccepted',
  MEETING_DECLINED: 'MeetingDeclined',
  MEETING_RESCHEDULED: 'MeetingRescheduled',
  MEETING_EXPIRED: 'MeetingExpired',
  AVAILABILITY_SHARED: 'AvailabilityShared',
  AVAILABILITY_RESPONDED: 'AvailabilityResponded',
  AVAILABILITY_EXPIRED: 'AvailabilityExpired',
} as const;
```

- [ ] **Step 2: Add meeting/availability event listeners to useChatHub.ts**

Add the following imports at the top of `useChatHub.ts`:

```typescript
import type {
  MeetingProposedPayload,
  MeetingStatusUpdatedPayload,
  MeetingRescheduledPayload,
  MeetingExpiredPayload,
  AvailabilitySharedPayload,
  AvailabilityRespondedPayload,
  AvailabilityExpiredPayload,
} from '../types/MeetingEventPayloads';
```

Add the following event listeners inside the `useEffect`, after the existing offer event listeners and before `connectionRef.current = connection;`:

```typescript
connection.on(HUB_METHODS.MEETING_PROPOSED, (payload: MeetingProposedPayload) => {
  queryClient.setQueryData<{ data: GetMessagesResponse }>(
    chatKeys.messages(payload.conversationId),
    (old) => {
      if (!old) return old;
      const newMessage: Message = {
        id: payload.messageId,
        senderId: payload.senderId,
        senderUsername: payload.senderUsername,
        content: '',
        sentAt: payload.sentAt,
        isRead: false,
        messageType: 'Meeting',
        meeting: payload.meeting,
      };
      return {
        ...old,
        data: { ...old.data, messages: [...old.data.messages, newMessage] },
      };
    },
  );
  void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
});

const handleMeetingStatusUpdate = (payload: MeetingStatusUpdatedPayload) => {
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
              ? { ...m, meeting: { ...m.meeting!, status: payload.newStatus } }
              : m,
          ),
        },
      };
    },
  );
};

connection.on(HUB_METHODS.MEETING_ACCEPTED, handleMeetingStatusUpdate);
connection.on(HUB_METHODS.MEETING_DECLINED, handleMeetingStatusUpdate);

connection.on(HUB_METHODS.MEETING_RESCHEDULED, (payload: MeetingRescheduledPayload) => {
  queryClient.setQueryData<{ data: GetMessagesResponse }>(
    chatKeys.messages(payload.conversationId),
    (old) => {
      if (!old) return old;
      const updatedMessages = old.data.messages.map((m) =>
        m.meeting?.id === payload.meetingId
          ? { ...m, meeting: { ...m.meeting!, status: 'Rescheduled' as const } }
          : m,
      );
      const rescheduledMessage: Message = {
        id: payload.rescheduledMeeting.messageId,
        senderId: payload.rescheduledMeeting.senderId,
        senderUsername: payload.rescheduledMeeting.senderUsername,
        content: '',
        sentAt: payload.rescheduledMeeting.sentAt,
        isRead: false,
        messageType: 'Meeting',
        meeting: payload.rescheduledMeeting.meeting,
      };
      return {
        ...old,
        data: { ...old.data, messages: [...updatedMessages, rescheduledMessage] },
      };
    },
  );
  void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
});

connection.on(HUB_METHODS.MEETING_EXPIRED, (payload: MeetingExpiredPayload) => {
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
              ? { ...m, meeting: { ...m.meeting!, status: 'Expired' as const } }
              : m,
          ),
        },
      };
    },
  );
});

connection.on(HUB_METHODS.AVAILABILITY_SHARED, (payload: AvailabilitySharedPayload) => {
  queryClient.setQueryData<{ data: GetMessagesResponse }>(
    chatKeys.messages(payload.conversationId),
    (old) => {
      if (!old) return old;
      const newMessage: Message = {
        id: payload.messageId,
        senderId: payload.senderId,
        senderUsername: payload.senderUsername,
        content: '',
        sentAt: payload.sentAt,
        isRead: false,
        messageType: 'Availability',
        availabilityCard: payload.availabilityCard,
      };
      return {
        ...old,
        data: { ...old.data, messages: [...old.data.messages, newMessage] },
      };
    },
  );
  void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
});

connection.on(HUB_METHODS.AVAILABILITY_RESPONDED, (payload: AvailabilityRespondedPayload) => {
  queryClient.setQueryData<{ data: GetMessagesResponse }>(
    chatKeys.messages(payload.conversationId),
    (old) => {
      if (!old) return old;
      const updatedMessages = old.data.messages.map((m) =>
        m.availabilityCard?.id === payload.availabilityCardId
          ? { ...m, availabilityCard: { ...m.availabilityCard!, status: 'Responded' as const } }
          : m,
      );
      if (payload.action === 'PickSlot' && payload.pickedSlotMeeting) {
        const meetingMessage: Message = {
          id: payload.pickedSlotMeeting.messageId,
          senderId: payload.pickedSlotMeeting.senderId,
          senderUsername: payload.pickedSlotMeeting.senderUsername,
          content: '',
          sentAt: payload.pickedSlotMeeting.sentAt,
          isRead: false,
          messageType: 'Meeting',
          meeting: payload.pickedSlotMeeting.meeting,
        };
        return {
          ...old,
          data: { ...old.data, messages: [...updatedMessages, meetingMessage] },
        };
      }
      if (payload.action === 'ShareBack' && payload.sharedBackAvailability) {
        const availMessage: Message = {
          id: payload.sharedBackAvailability.messageId,
          senderId: payload.sharedBackAvailability.senderId,
          senderUsername: payload.sharedBackAvailability.senderUsername,
          content: '',
          sentAt: payload.sharedBackAvailability.sentAt,
          isRead: false,
          messageType: 'Availability',
          availabilityCard: payload.sharedBackAvailability.availabilityCard,
        };
        return {
          ...old,
          data: { ...old.data, messages: [...updatedMessages, availMessage] },
        };
      }
      return { ...old, data: { ...old.data, messages: updatedMessages } };
    },
  );
  void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
});

connection.on(HUB_METHODS.AVAILABILITY_EXPIRED, (payload: AvailabilityExpiredPayload) => {
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
              ? { ...m, availabilityCard: { ...m.availabilityCard!, status: 'Expired' as const } }
              : m,
          ),
        },
      };
    },
  );
});
```

- [ ] **Step 3: Add meeting/availability invoke functions to useChatHub.ts**

Add the following functions inside the `useChatHub` hook, before the `return` statement:

```typescript
const proposeMeeting = useCallback(
  ({
    conversationId,
    proposedAt,
    durationMinutes,
    locationText,
    locationLat,
    locationLng,
  }: {
    conversationId: string;
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => {
    if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected. Please wait and try again.');
    }
    void connectionRef.current.invoke(
      HUB_METHODS.PROPOSE_MEETING,
      conversationId,
      proposedAt,
      durationMinutes,
      locationText ?? null,
      locationLat ?? null,
      locationLng ?? null,
    );
  },
  [],
);

const respondToMeeting = useCallback(
  ({
    meetingId,
    action,
    rescheduleData,
  }: {
    meetingId: string;
    action: 'Accept' | 'Decline' | 'Reschedule';
    rescheduleData?: {
      proposedAt: string;
      durationMinutes: number;
      locationText?: string;
      locationLat?: number;
      locationLng?: number;
    };
  }) => {
    if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected. Please wait and try again.');
    }
    void connectionRef.current.invoke(
      HUB_METHODS.RESPOND_TO_MEETING,
      meetingId,
      action,
      rescheduleData ?? null,
    );
  },
  [],
);

const shareAvailability = useCallback(
  ({
    conversationId,
    slots,
  }: {
    conversationId: string;
    slots: { startTime: string; endTime: string }[];
  }) => {
    if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected. Please wait and try again.');
    }
    void connectionRef.current.invoke(
      HUB_METHODS.SHARE_AVAILABILITY,
      conversationId,
      slots,
    );
  },
  [],
);

const respondToAvailability = useCallback(
  ({
    availabilityCardId,
    action,
    slotId,
    shareBackSlots,
  }: {
    availabilityCardId: string;
    action: 'PickSlot' | 'ShareBack';
    slotId?: string;
    shareBackSlots?: { startTime: string; endTime: string }[];
  }) => {
    if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected. Please wait and try again.');
    }
    void connectionRef.current.invoke(
      HUB_METHODS.RESPOND_TO_AVAILABILITY,
      availabilityCardId,
      action,
      slotId ?? null,
      shareBackSlots ?? null,
    );
  },
  [],
);
```

Update the return statement:

```typescript
return {
  sendMessage,
  sendOffer,
  respondToOffer,
  proposeMeeting,
  respondToMeeting,
  shareAvailability,
  respondToAvailability,
};
```

- [ ] **Step 4: Run lint check**

Run (from `automotive.marketplace.client/`): `npm run lint && npm run format:check`
Expected: No errors

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(client): add meeting/availability SignalR constants and hub methods

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 20: ProposeMeetingModal Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/ProposeMeetingModal.tsx`

- [ ] **Step 1: Create ProposeMeetingModal.tsx**

```tsx
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useState } from 'react';
import type { Meeting } from '../types/Meeting';

type ProposeMeetingModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: 'propose' | 'reschedule';
  initialMeeting?: Meeting;
  onSubmit: (data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
};

const DURATION_PRESETS = [30, 60, 90, 120];

const ProposeMeetingModal = ({
  open,
  onOpenChange,
  mode,
  initialMeeting,
  onSubmit,
}: ProposeMeetingModalProps) => {
  const now = new Date();
  const defaultDate = initialMeeting
    ? new Date(initialMeeting.proposedAt).toISOString().slice(0, 10)
    : '';
  const defaultTime = initialMeeting
    ? new Date(initialMeeting.proposedAt).toISOString().slice(11, 16)
    : '';

  const [date, setDate] = useState(defaultDate);
  const [time, setTime] = useState(defaultTime);
  const [duration, setDuration] = useState(initialMeeting?.durationMinutes ?? 60);
  const [locationText, setLocationText] = useState(initialMeeting?.locationText ?? '');
  const [showCoords, setShowCoords] = useState(false);
  const [lat, setLat] = useState(initialMeeting?.locationLat?.toString() ?? '');
  const [lng, setLng] = useState(initialMeeting?.locationLng?.toString() ?? '');

  const proposedAt = date && time ? new Date(`${date}T${time}:00Z`) : null;
  const isInFuture = proposedAt ? proposedAt > now : false;
  const isValid = !!date && !!time && isInFuture && duration > 0;

  const handleSubmit = () => {
    if (!isValid || !proposedAt) return;
    onSubmit({
      proposedAt: proposedAt.toISOString(),
      durationMinutes: duration,
      locationText: locationText || undefined,
      locationLat: lat ? parseFloat(lat) : undefined,
      locationLng: lng ? parseFloat(lng) : undefined,
    });
    onOpenChange(false);
  };

  const handleUseMyLocation = () => {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setLat(pos.coords.latitude.toFixed(7));
        setLng(pos.coords.longitude.toFixed(7));
        setShowCoords(true);
      },
      () => setShowCoords(true),
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>
            {mode === 'reschedule' ? 'Reschedule Meeting' : 'Propose a Meetup'}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="space-y-1.5">
            <Label htmlFor="meeting-date">Date</Label>
            <Input
              id="meeting-date"
              type="date"
              value={date}
              min={now.toISOString().slice(0, 10)}
              onChange={(e) => setDate(e.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="meeting-time">Start time (UTC)</Label>
            <Input
              id="meeting-time"
              type="time"
              value={time}
              onChange={(e) => setTime(e.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label>Duration</Label>
            <div className="flex gap-2">
              {DURATION_PRESETS.map((d) => (
                <Button
                  key={d}
                  type="button"
                  size="sm"
                  variant={duration === d ? 'default' : 'outline'}
                  className="h-7 text-xs"
                  onClick={() => setDuration(d)}
                >
                  {d} min
                </Button>
              ))}
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="meeting-location">Location (optional)</Label>
            <Input
              id="meeting-location"
              placeholder="e.g. Central Park, 5th Ave entrance"
              value={locationText}
              onChange={(e) => setLocationText(e.target.value)}
            />
          </div>

          {!showCoords && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="text-xs"
              onClick={handleUseMyLocation}
            >
              📍 Set pin (optional)
            </Button>
          )}

          {showCoords && (
            <div className="grid grid-cols-2 gap-2">
              <div className="space-y-1">
                <Label htmlFor="lat" className="text-xs">Latitude</Label>
                <Input
                  id="lat"
                  type="number"
                  step="any"
                  value={lat}
                  onChange={(e) => setLat(e.target.value)}
                  placeholder="e.g. 40.7829"
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="lng" className="text-xs">Longitude</Label>
                <Input
                  id="lng"
                  type="number"
                  step="any"
                  value={lng}
                  onChange={(e) => setLng(e.target.value)}
                  placeholder="e.g. -73.9654"
                />
              </div>
            </div>
          )}

          {date && time && !isInFuture && (
            <p className="text-destructive text-xs">
              Meeting time must be in the future.
            </p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!isValid}>
              {mode === 'reschedule' ? 'Send Reschedule' : 'Propose Meetup'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ProposeMeetingModal;
```

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "feat(client): add ProposeMeetingModal component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 21: ShareAvailabilityModal Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/ShareAvailabilityModal.tsx`

- [ ] **Step 1: Create ShareAvailabilityModal.tsx**

```tsx
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Trash2 } from 'lucide-react';
import { useState } from 'react';

type SlotEntry = {
  key: number;
  date: string;
  startTime: string;
  endTime: string;
};

type ShareAvailabilityModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (slots: { startTime: string; endTime: string }[]) => void;
};

let nextKey = 0;
const createEmptySlot = (): SlotEntry => ({
  key: nextKey++,
  date: '',
  startTime: '',
  endTime: '',
});

const ShareAvailabilityModal = ({
  open,
  onOpenChange,
  onSubmit,
}: ShareAvailabilityModalProps) => {
  const [slots, setSlots] = useState<SlotEntry[]>([createEmptySlot()]);
  const now = new Date();

  const updateSlot = (key: number, field: keyof Omit<SlotEntry, 'key'>, value: string) => {
    setSlots((prev) =>
      prev.map((s) => (s.key === key ? { ...s, [field]: value } : s)),
    );
  };

  const removeSlot = (key: number) => {
    setSlots((prev) => prev.filter((s) => s.key !== key));
  };

  const addSlot = () => {
    setSlots((prev) => [...prev, createEmptySlot()]);
  };

  const validSlots = slots.filter((s) => {
    if (!s.date || !s.startTime || !s.endTime) return false;
    const start = new Date(`${s.date}T${s.startTime}:00Z`);
    const end = new Date(`${s.date}T${s.endTime}:00Z`);
    return start > now && end > start;
  });

  const isValid = validSlots.length > 0 && validSlots.length === slots.length;

  const handleSubmit = () => {
    if (!isValid) return;
    const mapped = slots.map((s) => ({
      startTime: new Date(`${s.date}T${s.startTime}:00Z`).toISOString(),
      endTime: new Date(`${s.date}T${s.endTime}:00Z`).toISOString(),
    }));
    onSubmit(mapped);
    onOpenChange(false);
    setSlots([createEmptySlot()]);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Share Your Availability</DialogTitle>
        </DialogHeader>

        <div className="max-h-80 space-y-3 overflow-y-auto py-2">
          {slots.map((slot, i) => (
            <div key={slot.key} className="bg-muted/50 relative rounded-lg p-3">
              <div className="mb-1 flex items-center justify-between">
                <span className="text-muted-foreground text-xs font-medium">
                  Slot {i + 1}
                </span>
                {slots.length > 1 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="text-destructive h-6 w-6 p-0"
                    onClick={() => removeSlot(slot.key)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                )}
              </div>
              <div className="grid grid-cols-3 gap-2">
                <div className="space-y-1">
                  <Label className="text-xs">Date</Label>
                  <Input
                    type="date"
                    value={slot.date}
                    min={now.toISOString().slice(0, 10)}
                    onChange={(e) => updateSlot(slot.key, 'date', e.target.value)}
                    className="h-8 text-xs"
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">From</Label>
                  <Input
                    type="time"
                    value={slot.startTime}
                    onChange={(e) => updateSlot(slot.key, 'startTime', e.target.value)}
                    className="h-8 text-xs"
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">To</Label>
                  <Input
                    type="time"
                    value={slot.endTime}
                    onChange={(e) => updateSlot(slot.key, 'endTime', e.target.value)}
                    className="h-8 text-xs"
                  />
                </div>
              </div>
            </div>
          ))}
        </div>

        <Button
          type="button"
          variant="outline"
          size="sm"
          className="w-full text-xs"
          onClick={addSlot}
        >
          + Add another slot
        </Button>

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!isValid}>
            Share Availability
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ShareAvailabilityModal;
```

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "feat(client): add ShareAvailabilityModal component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 22: MeetingCard Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/MeetingCard.tsx`

- [ ] **Step 1: Create MeetingCard.tsx**

```tsx
import { Button } from '@/components/ui/button';
import { format } from 'date-fns';
import { CalendarCheck, CalendarX, Clock, CalendarDays, CalendarClock } from 'lucide-react';
import { useState } from 'react';
import type { Meeting } from '../types/Meeting';
import ProposeMeetingModal from './ProposeMeetingModal';

type MeetingCardProps = {
  meeting: Meeting;
  currentUserId: string;
  onAccept: (meetingId: string) => void;
  onDecline: (meetingId: string) => void;
  onReschedule: (meetingId: string, data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
};

const statusConfig = {
  Pending: {
    headerClass: 'bg-[#1e3a5f]',
    borderClass: 'border-blue-300 dark:border-blue-800',
    label: 'Meetup Proposed',
    icon: CalendarDays,
    labelClass: 'text-blue-200',
    subLabel: 'Awaiting response',
    subLabelClass: 'text-blue-400',
  },
  Accepted: {
    headerClass: 'bg-green-900',
    borderClass: 'border-green-300 dark:border-green-800',
    label: 'Meetup Confirmed',
    icon: CalendarCheck,
    labelClass: 'text-green-200',
    subLabel: 'See you there!',
    subLabelClass: 'text-green-400',
  },
  Declined: {
    headerClass: 'bg-red-900',
    borderClass: 'border-red-300 dark:border-red-800',
    label: 'Meetup Declined',
    icon: CalendarX,
    labelClass: 'text-red-200',
    subLabel: 'Not happening',
    subLabelClass: 'text-red-400',
  },
  Rescheduled: {
    headerClass: 'bg-violet-900',
    borderClass: 'border-violet-300 dark:border-violet-800',
    label: 'Reschedule Proposed',
    icon: CalendarClock,
    labelClass: 'text-violet-200',
    subLabel: 'Superseded',
    subLabelClass: 'text-violet-400',
  },
  Expired: {
    headerClass: 'bg-muted-foreground/60',
    borderClass: 'border-border',
    label: 'Meetup Expired',
    icon: Clock,
    labelClass: 'text-muted',
    subLabel: 'No response in time',
    subLabelClass: 'text-muted-foreground',
  },
} as const;

const MeetingCard = ({
  meeting,
  currentUserId,
  onAccept,
  onDecline,
  onReschedule,
}: MeetingCardProps) => {
  const [rescheduleOpen, setRescheduleOpen] = useState(false);
  const config = statusConfig[meeting.status];
  const Icon = config.icon;

  const canRespond = meeting.status === 'Pending' && currentUserId !== meeting.initiatorId;
  const proposedDate = new Date(meeting.proposedAt);
  const isMuted = meeting.status === 'Declined' || meeting.status === 'Expired' || meeting.status === 'Rescheduled';

  return (
    <>
      <div className={`w-full max-w-[280px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}>
        <div className={`${config.headerClass} flex items-center justify-between px-4 py-2.5`}>
          <div className="flex items-center gap-2">
            <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
            <span className={`text-xs font-semibold uppercase tracking-wider ${config.labelClass}`}>
              {config.label}
            </span>
          </div>
          <span className={`text-xs ${config.subLabelClass}`}>{config.subLabel}</span>
        </div>

        <div className="bg-card px-4 py-3">
          <div className="mb-2 flex items-center gap-3">
            <div className={`flex h-12 w-12 flex-col items-center justify-center rounded-lg ${isMuted ? 'bg-muted' : 'bg-primary/10'}`}>
              <span className={`text-lg font-bold leading-none ${isMuted ? 'text-muted-foreground' : 'text-primary'}`}>
                {format(proposedDate, 'd')}
              </span>
              <span className={`text-[10px] uppercase ${isMuted ? 'text-muted-foreground' : 'text-primary'}`}>
                {format(proposedDate, 'MMM')}
              </span>
            </div>
            <div>
              <p className={`text-sm font-semibold ${isMuted ? 'text-muted-foreground line-through' : ''}`}>
                {format(proposedDate, 'EEEE')}
              </p>
              <p className={`text-xs ${isMuted ? 'text-muted-foreground' : 'text-muted-foreground'}`}>
                {format(proposedDate, 'HH:mm')} – {format(new Date(proposedDate.getTime() + meeting.durationMinutes * 60000), 'HH:mm')} UTC
              </p>
              <p className="text-muted-foreground text-[10px]">
                {meeting.durationMinutes} min
              </p>
            </div>
          </div>

          {meeting.locationText && (
            <p className={`text-xs ${isMuted ? 'text-muted-foreground' : ''}`}>
              📍 {meeting.locationText}
            </p>
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
              <Button
                size="sm"
                variant="outline"
                className="h-7 flex-1 text-xs"
                onClick={() => setRescheduleOpen(true)}
              >
                Reschedule
              </Button>
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
        </div>
      </div>

      <ProposeMeetingModal
        open={rescheduleOpen}
        onOpenChange={setRescheduleOpen}
        mode="reschedule"
        initialMeeting={meeting}
        onSubmit={(data) => {
          onReschedule(meeting.id, data);
          setRescheduleOpen(false);
        }}
      />
    </>
  );
};

export default MeetingCard;
```

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "feat(client): add MeetingCard component with status variants

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 23: AvailabilityCard Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/AvailabilityCardComponent.tsx`

Note: Named `AvailabilityCardComponent.tsx` to avoid collision with the `AvailabilityCard` type.

- [ ] **Step 1: Create AvailabilityCardComponent.tsx**

```tsx
import { Button } from '@/components/ui/button';
import { format } from 'date-fns';
import { CalendarRange, Clock } from 'lucide-react';
import { useState } from 'react';
import type { AvailabilityCard, AvailabilityCardStatus } from '../types/AvailabilityCard';
import ShareAvailabilityModal from './ShareAvailabilityModal';

type AvailabilityCardComponentProps = {
  card: AvailabilityCard;
  currentUserId: string;
  onPickSlot: (cardId: string, slotId: string) => void;
  onShareBack: (cardId: string, slots: { startTime: string; endTime: string }[]) => void;
};

const statusConfig: Record<AvailabilityCardStatus, {
  headerClass: string;
  borderClass: string;
  label: string;
  icon: typeof CalendarRange;
  labelClass: string;
}> = {
  Pending: {
    headerClass: 'bg-purple-900',
    borderClass: 'border-purple-300 dark:border-purple-800',
    label: 'Availability Shared',
    icon: CalendarRange,
    labelClass: 'text-purple-200',
  },
  Responded: {
    headerClass: 'bg-purple-900/60',
    borderClass: 'border-border',
    label: 'Responded',
    icon: CalendarRange,
    labelClass: 'text-purple-300',
  },
  Expired: {
    headerClass: 'bg-muted-foreground/60',
    borderClass: 'border-border',
    label: 'Availability Expired',
    icon: Clock,
    labelClass: 'text-muted',
  },
};

const AvailabilityCardComponent = ({
  card,
  currentUserId,
  onPickSlot,
  onShareBack,
}: AvailabilityCardComponentProps) => {
  const [shareBackOpen, setShareBackOpen] = useState(false);
  const config = statusConfig[card.status];
  const Icon = config.icon;

  const canRespond = card.status === 'Pending' && currentUserId !== card.initiatorId;
  const isDisabled = card.status !== 'Pending';

  return (
    <>
      <div className={`w-full max-w-[300px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}>
        <div className={`${config.headerClass} flex items-center gap-2 px-4 py-2.5`}>
          <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
          <span className={`text-xs font-semibold uppercase tracking-wider ${config.labelClass}`}>
            {config.label}
          </span>
        </div>

        <div className="bg-card divide-y">
          {card.slots.map((slot) => {
            const start = new Date(slot.startTime);
            const end = new Date(slot.endTime);
            return (
              <div
                key={slot.id}
                className={`flex items-center justify-between px-4 py-2 ${isDisabled ? 'opacity-50' : ''}`}
              >
                <div>
                  <p className="text-sm font-medium">
                    {format(start, 'EEE, MMM d')}
                  </p>
                  <p className="text-muted-foreground text-xs">
                    {format(start, 'HH:mm')} – {format(end, 'HH:mm')} UTC
                  </p>
                </div>
                {canRespond && (
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 text-xs"
                    onClick={() => onPickSlot(card.id, slot.id)}
                  >
                    Propose →
                  </Button>
                )}
              </div>
            );
          })}
        </div>

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

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "feat(client): add AvailabilityCardComponent with slot picking and share-back

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 24: ActionBar + MessageThread Updates

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`

- [ ] **Step 1: Update ActionBar.tsx — add "Plan Meetup" dropdown**

```tsx
import { Button } from '@/components/ui/button';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { useState } from 'react';
import MakeOfferModal from './MakeOfferModal';
import ProposeMeetingModal from './ProposeMeetingModal';
import ShareAvailabilityModal from './ShareAvailabilityModal';

type ActionBarProps = {
  currentUserId: string;
  buyerId: string;
  sellerId: string;
  listingPrice: number;
  conversationId: string;
  buyerHasLiked: boolean;
  hasActiveOffer: boolean;
  hasActiveMeeting: boolean;
  onSendOffer: (amount: number) => void;
  onProposeMeeting: (data: {
    proposedAt: string;
    durationMinutes: number;
    locationText?: string;
    locationLat?: number;
    locationLng?: number;
  }) => void;
  onShareAvailability: (slots: { startTime: string; endTime: string }[]) => void;
};

const ActionBar = ({
  currentUserId,
  buyerId,
  sellerId,
  listingPrice,
  conversationId,
  buyerHasLiked,
  hasActiveOffer,
  hasActiveMeeting,
  onSendOffer,
  onProposeMeeting,
  onShareAvailability,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [meetupPopoverOpen, setMeetupPopoverOpen] = useState(false);
  const [proposeMeetingOpen, setProposeMeetingOpen] = useState(false);
  const [shareAvailabilityOpen, setShareAvailabilityOpen] = useState(false);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showButtons = isBuyer || (isSeller && buyerHasLiked);

  if (!showButtons) return null;

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        disabled={hasActiveOffer}
        onClick={() => setOfferModalOpen(true)}
        title={hasActiveOffer ? 'An offer is already pending in this conversation' : undefined}
        className="shrink-0"
      >
        Make an Offer
      </Button>

      <Popover open={meetupPopoverOpen} onOpenChange={setMeetupPopoverOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={hasActiveMeeting}
            title={hasActiveMeeting ? 'A meetup negotiation is already active' : undefined}
            className="shrink-0"
          >
            Plan Meetup 📅 ▾
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-48 p-1" align="start">
          <button
            className="hover:bg-muted w-full rounded-md px-3 py-2 text-left text-sm"
            onClick={() => {
              setMeetupPopoverOpen(false);
              setProposeMeetingOpen(true);
            }}
          >
            🗓️ Propose a time
          </button>
          <button
            className="hover:bg-muted w-full rounded-md px-3 py-2 text-left text-sm"
            onClick={() => {
              setMeetupPopoverOpen(false);
              setShareAvailabilityOpen(true);
            }}
          >
            ⏰ Share availability
          </button>
        </PopoverContent>
      </Popover>

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

- [ ] **Step 2: Update MessageThread.tsx — render meeting and availability cards**

```tsx
import { Button } from "@/components/ui/button";
import { useAppSelector } from "@/hooks/redux";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { getMessagesOptions } from "../api/getMessagesOptions";
import { useChatHub } from "../api/useChatHub";
import { useMarkMessagesRead } from "../api/useMarkMessagesRead";
import type { ConversationSummary } from "../types/ConversationSummary";
import ActionBar from "./ActionBar";
import AvailabilityCardComponent from "./AvailabilityCardComponent";
import ListingCard from "./ListingCard";
import MeetingCard from "./MeetingCard";
import OfferCard from "./OfferCard";

type MessageThreadProps = {
  conversation: ConversationSummary;
  showListingCard?: boolean;
};

const MessageThread = ({
  conversation,
  showListingCard = true,
}: MessageThreadProps) => {
  const userId = useAppSelector((s) => s.auth.userId) ?? "";
  const { data: messagesQuery } = useSuspenseQuery(
    getMessagesOptions({ conversationId: conversation.id }),
  );
  const messages = messagesQuery.data.messages;
  const {
    sendMessage,
    sendOffer,
    respondToOffer,
    proposeMeeting,
    respondToMeeting,
    shareAvailability,
    respondToAvailability,
  } = useChatHub();
  const { mutate: markRead } = useMarkMessagesRead();
  const [input, setInput] = useState("");
  const [sendError, setSendError] = useState<string | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length]);

  useEffect(() => {
    markRead(conversation.id);
  }, [messages.length, conversation.id, markRead]);

  const hasActiveOffer = messages.some(
    (m) => m.messageType === "Offer" && m.offer?.status === "Pending",
  );

  const hasActiveMeeting = messages.some(
    (m) =>
      (m.messageType === "Meeting" && m.meeting?.status === "Pending") ||
      (m.messageType === "Availability" &&
        m.availabilityCard?.status === "Pending"),
  );

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    try {
      sendMessage({ conversationId: conversation.id, content: trimmed });
      setInput("");
      setSendError(null);
    } catch (err) {
      setSendError(
        err instanceof Error ? err.message : "Failed to send message.",
      );
    }
  };

  return (
    <div className="flex h-full flex-col">
      {showListingCard && (
        <ListingCard
          listingId={conversation.listingId}
          listingTitle={conversation.listingTitle}
          listingPrice={conversation.listingPrice}
          listingThumbnail={conversation.listingThumbnail}
        />
      )}
      <div className="flex-1 space-y-2 overflow-y-auto p-4">
        {messages.map((m) => {
          if (m.messageType === "Offer" && m.offer) {
            const isOwn = m.senderId === userId;
            return (
              <div
                key={m.id}
                className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
              >
                <OfferCard
                  offer={m.offer}
                  currentUserId={userId}
                  listingPrice={conversation.listingPrice}
                  onAccept={(offerId) =>
                    respondToOffer({ offerId, action: "Accept" })
                  }
                  onDecline={(offerId) =>
                    respondToOffer({ offerId, action: "Decline" })
                  }
                  onCounter={(offerId, amount) =>
                    respondToOffer({
                      offerId,
                      action: "Counter",
                      counterAmount: amount,
                    })
                  }
                />
              </div>
            );
          }

          if (m.messageType === "Meeting" && m.meeting) {
            const isOwn = m.senderId === userId;
            return (
              <div
                key={m.id}
                className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
              >
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
                />
              </div>
            );
          }

          if (m.messageType === "Availability" && m.availabilityCard) {
            const isOwn = m.senderId === userId;
            return (
              <div
                key={m.id}
                className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
              >
                <AvailabilityCardComponent
                  card={m.availabilityCard}
                  currentUserId={userId}
                  onPickSlot={(cardId, slotId) =>
                    respondToAvailability({
                      availabilityCardId: cardId,
                      action: "PickSlot",
                      slotId,
                    })
                  }
                  onShareBack={(cardId, slots) =>
                    respondToAvailability({
                      availabilityCardId: cardId,
                      action: "ShareBack",
                      shareBackSlots: slots,
                    })
                  }
                />
              </div>
            );
          }

          const isOwn = m.senderId === userId;
          return (
            <div
              key={m.id}
              className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`max-w-[75%] rounded-2xl px-3 py-2 text-sm break-words ${
                  isOwn
                    ? "bg-primary text-primary-foreground rounded-br-sm"
                    : "bg-muted rounded-bl-sm"
                }`}
              >
                {m.content}
              </div>
            </div>
          );
        })}
        <div ref={bottomRef} />
      </div>
      {sendError && (
        <p className="text-destructive px-3 pb-1 text-xs">{sendError}</p>
      )}
      <div className="border-border flex items-center gap-2 border-t p-3">
        <ActionBar
          currentUserId={userId}
          buyerId={conversation.buyerId}
          sellerId={conversation.sellerId}
          listingPrice={conversation.listingPrice}
          conversationId={conversation.id}
          buyerHasLiked={conversation.buyerHasLiked}
          hasActiveOffer={hasActiveOffer}
          hasActiveMeeting={hasActiveMeeting}
          onSendOffer={(amount) =>
            sendOffer({ conversationId: conversation.id, amount })
          }
          onProposeMeeting={(data) =>
            proposeMeeting({ conversationId: conversation.id, ...data })
          }
          onShareAvailability={(slots) =>
            shareAvailability({ conversationId: conversation.id, slots })
          }
        />
        <input
          className="border-input bg-background focus:ring-ring flex-1 rounded-full border px-4 py-2 text-sm focus:ring-2 focus:outline-none"
          placeholder={`Message ${conversation.counterpartUsername}...`}
          value={input}
          onChange={(e) => {
            setInput(e.target.value);
            setSendError(null);
          }}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <Button size="sm" onClick={handleSend} disabled={!input.trim()}>
          Send
        </Button>
      </div>
    </div>
  );
};

export default MessageThread;
```

- [ ] **Step 3: Run lint and format check**

Run (from `automotive.marketplace.client/`): `npm run lint && npm run format:check`
Expected: No errors (fix formatting if needed with `npm run format`)

- [ ] **Step 4: Run frontend build**

Run (from `automotive.marketplace.client/`): `npm run build`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(client): update ActionBar with Plan Meetup dropdown and MessageThread with meeting/availability rendering

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 25: Final Verification

- [ ] **Step 1: Run full backend build and tests**

Run: `dotnet build ./Automotive.Marketplace.sln && dotnet test ./Automotive.Marketplace.sln -v n`
Expected: Build succeeded, all tests pass

- [ ] **Step 2: Run full frontend build**

Run (from `automotive.marketplace.client/`): `npm run lint && npm run format:check && npm run build`
Expected: All pass

- [ ] **Step 3: Final commit (if any formatting fixes)**

```bash
git add -A && git commit -m "chore: formatting fixes

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
