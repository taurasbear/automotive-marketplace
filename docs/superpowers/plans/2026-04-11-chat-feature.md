# Chat Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add real-time buyer↔seller chat tied to individual listings, using SignalR for live delivery and PostgreSQL for message persistence.

**Architecture:** SignalR `ChatHub` handles real-time message delivery; a `ChatController` handles REST for history/conversation management. The Application layer is clean of SignalR concerns — the hub broadcasts after calling MediatR handlers. The frontend uses `@microsoft/signalr` in a `useChatHub` hook initialised in the root layout, with TanStack Query caching message history.

**Tech Stack:** .NET 8 SignalR, MediatR, AutoMapper, EF Core 9 (Npgsql), xUnit + FluentAssertions + NSubstitute + Testcontainers (backend tests); React 19, TanStack Query, TanStack Router, Redux Toolkit, `@microsoft/signalr`, Tailwind CSS, Radix UI (frontend).

**Spec:** `docs/superpowers/specs/2026-04-11-chat-design.md`

---

## Part 1 — Backend

### Task 1: Domain Entities

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/Conversation.cs`
- Create: `Automotive.Marketplace.Domain/Entities/Message.cs`

- [ ] **Step 1: Create `Conversation.cs`**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid ListingId { get; set; }

    public Guid BuyerId { get; set; }

    public DateTime LastMessageAt { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = [];
}
```

- [ ] **Step 2: Create `Message.cs`**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
```

- [ ] **Step 3: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Domain/Entities/Conversation.cs \
        Automotive.Marketplace.Domain/Entities/Message.cs
git commit -m "feat: add Conversation and Message domain entities

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: EF Core Configuration + DbSets

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ConversationConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create `ConversationConfiguration.cs`**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasOne(c => c.Listing)
            .WithMany()
            .HasForeignKey(c => c.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Buyer)
            .WithMany()
            .HasForeignKey(c => c.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.BuyerId, c.ListingId }).IsUnique();
    }
}
```

- [ ] **Step 2: Create `MessageConfiguration.cs`**

```csharp
using Automotive.Marketplace.Domain.Entities;
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
    }
}
```

- [ ] **Step 3: Add DbSets to `AutomotiveContext.cs`**

Add these two properties after the existing DbSets:

```csharp
public DbSet<Conversation> Conversations { get; set; }

public DbSet<Message> Messages { get; set; }
```

- [ ] **Step 4: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Infrastructure/Data/Configuration/ConversationConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs
git commit -m "feat: add EF Core configuration for Conversation and Message

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: EF Core Migration

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Migrations/<timestamp>_AddChatEntities.cs` (auto-generated)

- [ ] **Step 1: Add migration**

Run from the repo root (where the `.sln` lives):

```bash
dotnet ef migrations add AddChatEntities \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected: migration file created in `Automotive.Marketplace.Infrastructure/Migrations/`.

- [ ] **Step 2: Verify the migration looks correct**

Open the generated migration file. Confirm it creates:
- `Conversations` table with columns `Id`, `ListingId`, `BuyerId`, `LastMessageAt`, `CreatedAt`, `ModifiedAt`, `CreatedBy`, `ModifiedBy`
- `Messages` table with columns `Id`, `ConversationId`, `SenderId`, `Content`, `SentAt`, `IsRead`, `CreatedAt`, `ModifiedAt`, `CreatedBy`, `ModifiedBy`
- Unique index on `Conversations(BuyerId, ListingId)`
- FK `Conversations.ListingId → Listings.Id` (cascade delete)
- FK `Conversations.BuyerId → Users.Id` (restrict)
- FK `Messages.ConversationId → Conversations.Id` (cascade delete)
- FK `Messages.SenderId → Users.Id` (restrict)

- [ ] **Step 3: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat: add EF migration for chat entities

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Test Builders

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/ConversationBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/MessageBuilder.cs`

- [ ] **Step 1: Create `ConversationBuilder.cs`**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ConversationBuilder
{
    private readonly Faker<Conversation> _faker;

    public ConversationBuilder()
    {
        _faker = new Faker<Conversation>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(c => c.LastMessageAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(c => c.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ConversationBuilder WithBuyer(Guid buyerId)
    {
        _faker.RuleFor(c => c.BuyerId, buyerId);
        return this;
    }

    public ConversationBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(c => c.ListingId, listingId);
        return this;
    }

    public ConversationBuilder With<T>(Expression<Func<Conversation, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Conversation Build() => _faker.Generate();
}
```

- [ ] **Step 2: Create `MessageBuilder.cs`**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MessageBuilder
{
    private readonly Faker<Message> _faker;

    public MessageBuilder()
    {
        _faker = new Faker<Message>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.Content, f => f.Lorem.Sentence())
            .RuleFor(m => m.SentAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(m => m.IsRead, false)
            .RuleFor(m => m.CreatedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(m => m.CreatedBy, f => f.Random.Guid().ToString());
    }

    public MessageBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(m => m.ConversationId, conversationId);
        return this;
    }

    public MessageBuilder WithSender(Guid senderId)
    {
        _faker.RuleFor(m => m.SenderId, senderId);
        return this;
    }

    public MessageBuilder WithIsRead(bool isRead)
    {
        _faker.RuleFor(m => m.IsRead, isRead);
        return this;
    }

    public MessageBuilder With<T>(Expression<Func<Message, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Message Build() => _faker.Generate();

    public List<Message> Build(int count) => _faker.Generate(count);
}
```

- [ ] **Step 3: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Infrastructure/Data/Builders/ConversationBuilder.cs \
        Automotive.Marketplace.Infrastructure/Data/Builders/MessageBuilder.cs
git commit -m "feat: add test builders for Conversation and Message

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: GetOrCreateConversation Feature

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetOrCreateConversation/GetOrCreateConversationCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetOrCreateConversation/GetOrCreateConversationResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetOrCreateConversation/GetOrCreateConversationCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetOrCreateConversationCommandHandlerTests.cs`

- [ ] **Step 1: Create `GetOrCreateConversationCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;

public class GetOrCreateConversationCommand : IRequest<GetOrCreateConversationResponse>
{
    public Guid BuyerId { get; set; }

    public Guid ListingId { get; set; }
}
```

- [ ] **Step 2: Create `GetOrCreateConversationResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;

public class GetOrCreateConversationResponse
{
    public Guid ConversationId { get; set; }
}
```

- [ ] **Step 3: Write the failing test**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetOrCreateConversationCommandHandlerTests(
    DatabaseFixture<GetOrCreateConversationCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetOrCreateConversationCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetOrCreateConversationCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetOrCreateConversationCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NewConversation_ShouldCreateAndReturnConversationId()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedBuyerAndListingAsync(context);

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = buyer.Id,
            ListingId = listing.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ConversationId.Should().NotBeEmpty();
        var saved = await context.Conversations.FindAsync(result.ConversationId);
        saved.Should().NotBeNull();
        saved!.BuyerId.Should().Be(buyer.Id);
        saved.ListingId.Should().Be(listing.Id);
    }

    [Fact]
    public async Task Handle_ExistingConversation_ShouldReturnExistingConversationId()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedBuyerAndListingAsync(context);
        var existing = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();
        await context.AddAsync(existing);
        await context.SaveChangesAsync();

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = buyer.Id,
            ListingId = listing.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ConversationId.Should().Be(existing.Id);
        var count = await context.Conversations.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_BuyerIsTheSeller_ShouldThrowRequestValidationException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();

        await context.AddRangeAsync(seller, make, model, car, listing);
        await context.SaveChangesAsync();

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = seller.Id,
            ListingId = listing.Id
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RequestValidationException>();
    }

    private static async Task<(User buyer, Listing listing)> SeedBuyerAndListingAsync(
        AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, car, listing);
        await context.SaveChangesAsync();

        return (buyer, listing);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetOrCreateConversationCommandHandlerTests"
```

Expected: compilation error (`GetOrCreateConversationCommandHandler` not found).

- [ ] **Step 5: Create `GetOrCreateConversationCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;

public class GetOrCreateConversationCommandHandler(IRepository repository)
    : IRequestHandler<GetOrCreateConversationCommand, GetOrCreateConversationResponse>
{
    public async Task<GetOrCreateConversationResponse> Handle(
        GetOrCreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);

        if (listing.SellerId == request.BuyerId)
        {
            throw new RequestValidationException(
            [
                new ValidationFailure("ListingId", "You cannot start a conversation about your own listing.")
            ]);
        }

        var existing = await repository.AsQueryable<Conversation>()
            .FirstOrDefaultAsync(
                c => c.BuyerId == request.BuyerId && c.ListingId == request.ListingId,
                cancellationToken);

        if (existing is not null)
            return new GetOrCreateConversationResponse { ConversationId = existing.Id };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BuyerId = request.BuyerId,
            ListingId = request.ListingId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            CreatedBy = request.BuyerId.ToString()
        };

        await repository.CreateAsync(conversation, cancellationToken);
        return new GetOrCreateConversationResponse { ConversationId = conversation.Id };
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetOrCreateConversationCommandHandlerTests"
```

Expected: 3 tests pass.

- [ ] **Step 7: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ChatFeatures/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/
git commit -m "feat: add GetOrCreateConversation command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: SendMessage Feature

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SendMessage/SendMessageCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SendMessage/SendMessageResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SendMessage/SendMessageCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SendMessageCommandHandlerTests.cs`

- [ ] **Step 1: Create `SendMessageCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public class SendMessageCommand : IRequest<SendMessageResponse>
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create `SendMessageResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public class SendMessageResponse
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public int RecipientUnreadCount { get; set; }
}
```

- [ ] **Step 3: Write the failing test**

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class SendMessageCommandHandlerTests(
    DatabaseFixture<SendMessageCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SendMessageCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<SendMessageCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private SendMessageCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistMessageAndUpdateLastMessageAt()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation) = await SeedConversationAsync(context);

        var command = new SendMessageCommand
        {
            ConversationId = conversation.Id,
            SenderId = buyer.Id,
            Content = "Is this still available?"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Content.Should().Be("Is this still available?");
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().Be(seller.Id);

        var savedMessage = await context.Messages.FindAsync(result.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.ConversationId.Should().Be(conversation.Id);

        var updatedConversation = await context.Conversations.FindAsync(conversation.Id);
        updatedConversation!.LastMessageAt.Should().BeCloseTo(result.SentAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_UnreadMessage_ShouldIncrementRecipientUnreadCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationAsync(context);

        // Act
        var result = await handler.Handle(new SendMessageCommand
        {
            ConversationId = conversation.Id,
            SenderId = buyer.Id,
            Content = "Hello"
        }, CancellationToken.None);

        // Assert
        result.RecipientUnreadCount.Should().Be(1);
    }

    private static async Task<(User buyer, User seller, Conversation conversation)> SeedConversationAsync(
        AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, car, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.SendMessageCommandHandlerTests"
```

Expected: compilation error.

- [ ] **Step 5: Create `SendMessageCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public class SendMessageCommandHandler(IRepository repository)
    : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    public async Task<SendMessageResponse> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var recipientId = conversation.BuyerId == request.SenderId
            ? conversation.Listing.SellerId
            : conversation.BuyerId;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.SenderId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var recipientUnreadCount = await repository.AsQueryable<Message>()
            .Where(m => !m.IsRead
                && m.SenderId != recipientId
                && (m.Conversation.BuyerId == recipientId
                    || m.Conversation.Listing.SellerId == recipientId))
            .CountAsync(cancellationToken);

        return new SendMessageResponse
        {
            Id = message.Id,
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderUsername = conversation.Buyer.Id == request.SenderId
                ? conversation.Buyer.Username
                : conversation.Listing.Seller.Username,
            Content = request.Content,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            RecipientUnreadCount = recipientUnreadCount
        };
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.SendMessageCommandHandlerTests"
```

Expected: 2 tests pass.

- [ ] **Step 7: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ChatFeatures/SendMessage/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SendMessageCommandHandlerTests.cs
git commit -m "feat: add SendMessage command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: GetConversations Feature

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ConversationSummaryResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetConversationsQueryHandlerTests.cs`

- [ ] **Step 1: Create `GetConversationsQuery.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public class GetConversationsQuery : IRequest<IEnumerable<ConversationSummaryResponse>>
{
    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create `ConversationSummaryResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public class ConversationSummaryResponse
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    public string ListingTitle { get; set; } = string.Empty;

    public string? ListingThumbnailUrl { get; set; }

    public decimal ListingPrice { get; set; }

    public Guid CounterpartId { get; set; }

    public string CounterpartUsername { get; set; } = string.Empty;

    public string? LastMessage { get; set; }

    public DateTime LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}
```

- [ ] **Step 3: Write the failing test**

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetConversationsQueryHandlerTests(
    DatabaseFixture<GetConversationsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetConversationsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetConversationsQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetConversationsQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(), _imageStorageService);

    [Fact]
    public async Task Handle_BuyerWithConversations_ShouldReturnConversationsAsBuyer()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationAsync(context);

        // Act
        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = buyer.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_SellerWithConversations_ShouldReturnConversationsAsSeller()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation) = await SeedConversationAsync(context);

        // Act
        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = seller.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_NoConversations_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        // Act
        var result = await handler.Handle(
            new GetConversationsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    private static async Task<(User buyer, User seller, Conversation conversation)> SeedConversationAsync(
        AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, car, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetConversationsQueryHandlerTests"
```

Expected: compilation error.

- [ ] **Step 5: Create `GetConversationsQueryHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public class GetConversationsQueryHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<GetConversationsQuery, IEnumerable<ConversationSummaryResponse>>
{
    public async Task<IEnumerable<ConversationSummaryResponse>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await repository.AsQueryable<Conversation>()
            .Where(c => c.BuyerId == request.UserId || c.Listing.SellerId == request.UserId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);

        var result = new List<ConversationSummaryResponse>();

        foreach (var conversation in conversations)
        {
            var listing = conversation.Listing;
            var car = listing.Car;
            var counterpart = conversation.BuyerId == request.UserId
                ? listing.Seller
                : conversation.Buyer;

            var lastMessage = conversation.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = conversation.Messages
                .Count(m => m.SenderId != request.UserId && !m.IsRead);

            string? thumbnailUrl = null;
            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage is not null)
                thumbnailUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey);

            result.Add(new ConversationSummaryResponse
            {
                Id = conversation.Id,
                ListingId = listing.Id,
                ListingTitle = $"{car.Year.Year} {car.Model.Make.Name} {car.Model.Name}",
                ListingThumbnailUrl = thumbnailUrl,
                ListingPrice = listing.Price,
                CounterpartId = counterpart.Id,
                CounterpartUsername = counterpart.Username,
                LastMessage = lastMessage?.Content,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount
            });
        }

        return result;
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetConversationsQueryHandlerTests"
```

Expected: 3 tests pass.

- [ ] **Step 7: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetConversationsQueryHandlerTests.cs
git commit -m "feat: add GetConversations query handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: GetMessages Feature

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Mappings/ChatMappings.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetMessagesQueryHandlerTests.cs`

- [ ] **Step 1: Create `GetMessagesQuery.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesQuery : IRequest<GetMessagesResponse>
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create `GetMessagesResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesResponse
{
    public Guid ConversationId { get; set; }

    public List<MessageDto> Messages { get; set; } = [];

    public class MessageDto
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }
}
```

- [ ] **Step 3: Write the failing test**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetMessagesQueryHandlerTests(
    DatabaseFixture<GetMessagesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetMessagesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetMessagesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetMessagesQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(),
               scope.ServiceProvider.GetRequiredService<IMapper>());

    [Fact]
    public async Task Handle_ParticipantRequest_ShouldReturnMessagesOrderedBySentAt()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationWithMessagesAsync(context);

        // Act
        var result = await handler.Handle(
            new GetMessagesQuery { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        result.Messages.Should().HaveCount(2);
        result.Messages.Should().BeInAscendingOrder(m => m.SentAt);
    }

    [Fact]
    public async Task Handle_NonParticipantRequest_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation) = await SeedConversationWithMessagesAsync(context);

        // Act
        var act = async () => await handler.Handle(
            new GetMessagesQuery { ConversationId = conversation.Id, UserId = Guid.NewGuid() },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation)>
        SeedConversationWithMessagesAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        var msg1 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.SentAt, DateTime.UtcNow.AddMinutes(-5))
            .Build();
        var msg2 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .With(m => m.SentAt, DateTime.UtcNow)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, car, listing,
            conversation, msg1, msg2);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetMessagesQueryHandlerTests"
```

Expected: compilation error.

- [ ] **Step 5: Create `ChatMappings.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ChatMappings : Profile
{
    public ChatMappings()
    {
        CreateMap<Message, GetMessagesResponse.MessageDto>()
            .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.Sender.Username));
    }
}
```

- [ ] **Step 6: Create `GetMessagesQueryHandler.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesQueryHandler(IRepository repository, IMapper mapper)
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

        var messages = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => mapper.Map<GetMessagesResponse.MessageDto>(m))
            .ToList();

        return new GetMessagesResponse
        {
            ConversationId = request.ConversationId,
            Messages = messages
        };
    }
}
```

- [ ] **Step 7: Run tests to confirm they pass**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.GetMessagesQueryHandlerTests"
```

Expected: 2 tests pass.

- [ ] **Step 8: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/ \
        Automotive.Marketplace.Application/Mappings/ChatMappings.cs \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetMessagesQueryHandlerTests.cs
git commit -m "feat: add GetMessages query handler and ChatMappings

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: MarkMessagesRead Feature

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MarkMessagesRead/MarkMessagesReadCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MarkMessagesRead/MarkMessagesReadResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MarkMessagesRead/MarkMessagesReadCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/MarkMessagesReadCommandHandlerTests.cs`

- [ ] **Step 1: Create `MarkMessagesReadCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public class MarkMessagesReadCommand : IRequest<MarkMessagesReadResponse>
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create `MarkMessagesReadResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public class MarkMessagesReadResponse
{
    public int TotalUnreadCount { get; set; }
}
```

- [ ] **Step 3: Write the failing test**

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class MarkMessagesReadCommandHandlerTests(
    DatabaseFixture<MarkMessagesReadCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<MarkMessagesReadCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<MarkMessagesReadCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private MarkMessagesReadCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_UnreadMessages_ShouldMarkThemAsRead()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation) = await SeedConversationWithUnreadMessagesAsync(context);

        // Act
        await handler.Handle(
            new MarkMessagesReadCommand { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        var unread = await context.Messages
            .Where(m => m.ConversationId == conversation.Id
                && m.SenderId == seller.Id && !m.IsRead)
            .CountAsync();
        unread.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AfterMarkingRead_ShouldReturnZeroTotalUnreadCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationWithUnreadMessagesAsync(context);

        // Act
        var result = await handler.Handle(
            new MarkMessagesReadCommand { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        result.TotalUnreadCount.Should().Be(0);
    }

    private static async Task<(User buyer, User seller, Conversation conversation)>
        SeedConversationWithUnreadMessagesAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var car = new CarBuilder().WithModel(model.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithCar(car.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        var msg1 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();
        var msg2 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, car, listing,
            conversation, msg1, msg2);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests \
  --filter "ChatHandlerTests.MarkMessagesReadCommandHandlerTests"
```

Expected: compilation error.

- [ ] **Step 5: Create `MarkMessagesReadCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public class MarkMessagesReadCommandHandler(IRepository repository)
    : IRequestHandler<MarkMessagesReadCommand, MarkMessagesReadResponse>
{
    public async Task<MarkMessagesReadResponse> Handle(
        MarkMessagesReadCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var unreadMessages = conversation.Messages
            .Where(m => m.SenderId != request.UserId && !m.IsRead)
            .ToList();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            await repository.UpdateAsync(message, cancellationToken);
        }

        var totalUnreadCount = await repository.AsQueryable<Message>()
            .Where(m => !m.IsRead
                && m.SenderId != request.UserId
                && (m.Conversation.BuyerId == request.UserId
                    || m.Conversation.Listing.SellerId == request.UserId))
            .CountAsync(cancellationToken);

        return new MarkMessagesReadResponse { TotalUnreadCount = totalUnreadCount };
    }
}
```

- [ ] **Step 6: Run all tests**

```bash
dotnet test Automotive.Marketplace.Tests
```

Expected: all tests pass.

- [ ] **Step 7: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ChatFeatures/MarkMessagesRead/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/MarkMessagesReadCommandHandlerTests.cs
git commit -m "feat: add MarkMessagesRead command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: ChatHub + Program.cs SignalR Registration

**Files:**
- Create: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`
- Modify: `Automotive.Marketplace.Server/Program.cs`

- [ ] **Step 1: Create `ChatHub.cs`**

```csharp
using System.Security.Claims;
using Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Automotive.Marketplace.Server.Hubs;

[Authorize]
public class ChatHub(IMediator mediator) : Hub
{
    private Guid UserId =>
        Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{UserId}");
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var result = await mediator.Send(new SendMessageCommand
        {
            ConversationId = conversationId,
            SenderId = UserId,
            Content = content
        });

        await Clients.Group($"user-{UserId}").SendAsync("ReceiveMessage", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("ReceiveMessage", result);
        await Clients.Group($"user-{result.RecipientId}")
            .SendAsync("UpdateUnreadCount", result.RecipientUnreadCount);
    }
}
```

- [ ] **Step 2: Add SignalR registration to `Program.cs`**

After `builder.Services.AddLogging(...)`, add:

```csharp
builder.Services.AddSignalR();
```

- [ ] **Step 3: Configure JWT to accept token from query string in `Program.cs`**

Replace the existing `.AddJwtBearer(options => { ... })` block with the version that adds `OnMessageReceived`:

```csharp
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});
```

- [ ] **Step 4: Map the hub in `Program.cs`**

Add this before `app.MapFallbackToFile("/index.html")`:

```csharp
app.MapHub<ChatHub>("/hubs/chat");
```

Add the using at the top of `Program.cs`:

```csharp
using Automotive.Marketplace.Server.Hubs;
```

- [ ] **Step 5: Build to confirm no errors**

```bash
dotnet build Automotive.Marketplace.Server
```

Expected: build succeeds.

- [ ] **Step 6: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs \
        Automotive.Marketplace.Server/Program.cs
git commit -m "feat: add ChatHub and configure SignalR in Program.cs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: ChatController

**Files:**
- Create: `Automotive.Marketplace.Server/Controllers/ChatController.cs`

- [ ] **Step 1: Create `ChatController.cs`**

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;
using Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;
using Automotive.Marketplace.Server.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Automotive.Marketplace.Server.Controllers;

[Authorize]
public class ChatController(IMediator mediator, IHubContext<ChatHub> hubContext) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<GetOrCreateConversationResponse>> GetOrCreateConversation(
        [FromBody] GetOrCreateConversationCommand command,
        CancellationToken cancellationToken)
    {
        command.BuyerId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationSummaryResponse>>> GetConversations(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetConversationsQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetMessagesResponse>> GetMessages(
        [FromQuery] GetMessagesQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId;
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> MarkMessagesRead(
        [FromBody] MarkMessagesReadCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        await hubContext.Clients
            .Group($"user-{UserId}")
            .SendAsync("UpdateUnreadCount", result.TotalUnreadCount, cancellationToken);
        return NoContent();
    }
}
```

- [ ] **Step 2: Build and run all tests**

```bash
dotnet build Automotive.Marketplace.Server && dotnet test Automotive.Marketplace.Tests
```

Expected: build succeeds, all tests pass.

- [ ] **Step 3: Commit**

```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Server/Controllers/ChatController.cs
git commit -m "feat: add ChatController REST endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Appendix A: Expose `SellerId` in `GetListingByIdResponse`

This small backend change is required before Task 19 (frontend listing details integration).

**Files to change:**

1. `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs` — add:
   ```csharp
   public Guid SellerId { get; set; }
   ```

2. `Automotive.Marketplace.Application/Mappings/ListingMappings.cs` — in the `Listing → GetListingByIdResponse` mapping, add:
   ```csharp
   .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId))
   ```

3. `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts` — add:
   ```ts
   sellerId: string;
   ```

Commit:
```bash
dotnet format Automotive.Marketplace.sln
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs \
        Automotive.Marketplace.Application/Mappings/ListingMappings.cs \
        automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts
git commit -m "feat: expose sellerId in GetListingByIdResponse

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Part 2 — Frontend

### Task 12: Install SignalR Client + Shared Infrastructure

**Files:**
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Create: `automotive.marketplace.client/src/api/queryKeys/chatKeys.ts`

- [ ] **Step 1: Install `@microsoft/signalr`**

```bash
cd automotive.marketplace.client && npm install @microsoft/signalr
```

Expected: `@microsoft/signalr` appears in `package.json` dependencies.

- [ ] **Step 2: Add `CHAT` to `endpoints.ts`**

Add inside the `ENDPOINTS` object after `ENUM`:

```ts
CHAT: {
  GET_OR_CREATE_CONVERSATION: "/Chat/GetOrCreateConversation",
  GET_CONVERSATIONS: "/Chat/GetConversations",
  GET_MESSAGES: "/Chat/GetMessages",
  MARK_MESSAGES_READ: "/Chat/MarkMessagesRead",
},
```

- [ ] **Step 3: Create `chatKeys.ts`**

```ts
export const chatKeys = {
  all: () => ["chat"] as const,
  conversations: () => [...chatKeys.all(), "conversations"] as const,
  messages: (conversationId: string) =>
    [...chatKeys.all(), "messages", conversationId] as const,
};
```

- [ ] **Step 4: Commit**

```bash
npm run format && npm run lint
git add src/constants/endpoints.ts src/api/queryKeys/chatKeys.ts \
        package.json package-lock.json
git commit -m "feat: install @microsoft/signalr and add chat constants

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: Chat Types

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/Message.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/GetOrCreateConversationCommand.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/GetOrCreateConversationResponse.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/GetMessagesQuery.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts`

- [ ] **Step 1: Create all type files**

`ConversationSummary.ts`:
```ts
export type ConversationSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingThumbnailUrl: string | null;
  listingPrice: number;
  counterpartId: string;
  counterpartUsername: string;
  lastMessage: string | null;
  lastMessageAt: string;
  unreadCount: number;
};
```

`Message.ts`:
```ts
export type Message = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
};
```

`GetOrCreateConversationCommand.ts`:
```ts
export type GetOrCreateConversationCommand = {
  listingId: string;
};
```

`GetOrCreateConversationResponse.ts`:
```ts
export type GetOrCreateConversationResponse = {
  conversationId: string;
};
```

`GetMessagesQuery.ts`:
```ts
export type GetMessagesQuery = {
  conversationId: string;
};
```

`GetMessagesResponse.ts`:
```ts
import type { Message } from "./Message";

export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};
```

- [ ] **Step 2: Commit**

```bash
npm run format && npm run lint
git add src/features/chat/types/
git commit -m "feat: add chat TypeScript types

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 14: Chat API Layer

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/api/getConversationsOptions.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/getMessagesOptions.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/useGetOrCreateConversation.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/useMarkMessagesRead.ts`

- [ ] **Step 1: Create `getConversationsOptions.ts`**

```ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { ConversationSummary } from "../types/ConversationSummary";

const getConversations = () =>
  axiosClient.get<ConversationSummary[]>(ENDPOINTS.CHAT.GET_CONVERSATIONS);

export const getConversationsOptions = () =>
  queryOptions({
    queryKey: chatKeys.conversations(),
    queryFn: () => getConversations(),
  });
```

- [ ] **Step 2: Create `getMessagesOptions.ts`**

```ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetMessagesQuery } from "../types/GetMessagesQuery";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";

const getMessages = (query: GetMessagesQuery) =>
  axiosClient.get<GetMessagesResponse>(ENDPOINTS.CHAT.GET_MESSAGES, {
    params: query,
  });

export const getMessagesOptions = (query: GetMessagesQuery) =>
  queryOptions({
    queryKey: chatKeys.messages(query.conversationId),
    queryFn: () => getMessages(query),
  });
```

- [ ] **Step 3: Create `useGetOrCreateConversation.ts`**

```ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import type { GetOrCreateConversationCommand } from "../types/GetOrCreateConversationCommand";
import type { GetOrCreateConversationResponse } from "../types/GetOrCreateConversationResponse";

const getOrCreateConversation = (command: GetOrCreateConversationCommand) =>
  axiosClient.post<GetOrCreateConversationResponse>(
    ENDPOINTS.CHAT.GET_OR_CREATE_CONVERSATION,
    command,
  );

export const useGetOrCreateConversation = () =>
  useMutation({
    mutationFn: getOrCreateConversation,
    meta: {
      errorMessage: "Could not open conversation. Please try again.",
      invalidatesQuery: chatKeys.conversations(),
    },
  });
```

- [ ] **Step 4: Create `useMarkMessagesRead.ts`**

```ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";

const markMessagesRead = (conversationId: string) =>
  axiosClient.put<void>(ENDPOINTS.CHAT.MARK_MESSAGES_READ, { conversationId });

export const useMarkMessagesRead = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: markMessagesRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    },
  });
};
```

- [ ] **Step 5: Commit**

```bash
npm run format && npm run lint
git add src/features/chat/api/getConversationsOptions.ts \
        src/features/chat/api/getMessagesOptions.ts \
        src/features/chat/api/useGetOrCreateConversation.ts \
        src/features/chat/api/useMarkMessagesRead.ts
git commit -m "feat: add chat API query options and mutations

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 15: useChatHub Hook + Redux Chat Slice

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/state/chatSlice.ts`
- Modify: `automotive.marketplace.client/src/lib/redux/store.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

- [ ] **Step 1: Create `chatSlice.ts`**

```ts
import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "../../../lib/redux/store";

type ChatState = {
  unreadCount: number;
};

const initialState: ChatState = {
  unreadCount: 0,
};

const chatSlice = createSlice({
  name: "chat",
  initialState,
  reducers: {
    setUnreadCount: (state, action: PayloadAction<number>) => {
      state.unreadCount = action.payload;
    },
  },
});

export const selectUnreadCount = (state: RootState) => state.chat.unreadCount;
export const { setUnreadCount } = chatSlice.actions;
export default chatSlice.reducer;
```

- [ ] **Step 2: Register `chatReducer` in the Redux store**

Open `src/lib/redux/store.ts`. Add the import and register the reducer. Find the `configureStore` call and add `chat: chatReducer` to the `reducer` object:

```ts
import chatReducer from "@/features/chat/state/chatSlice";

// In configureStore({ reducer: { ..., chat: chatReducer } })
```

- [ ] **Step 3: Create `useChatHub.ts`**

```ts
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef } from "react";
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { selectAccessToken } from "@/features/auth/state/authSlice";
import { useAppDispatch, useAppSelector } from "@/hooks/redux";
import { setUnreadCount } from "../state/chatSlice";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";
import type { Message } from "../types/Message";

const connectionRef = { current: null as signalR.HubConnection | null };

export const useChatHub = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const dispatch = useAppDispatch();
  const queryClient = useQueryClient();
  const isOwner = useRef(false);

  useEffect(() => {
    if (!accessToken) return;
    if (connectionRef.current) return;

    isOwner.current = true;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveMessage", (message: Message) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(message.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: { ...old.data, messages: [...old.data.messages, message] },
          };
        },
      );
      queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    });

    connection.on("UpdateUnreadCount", (count: number) => {
      dispatch(setUnreadCount(count));
    });

    connectionRef.current = connection;
    connection.start().catch(console.error);

    return () => {
      if (isOwner.current) {
        connection.stop();
        connectionRef.current = null;
      }
    };
  }, [accessToken, dispatch, queryClient]);

  const sendMessage = useCallback(
    ({
      conversationId,
      content,
    }: {
      conversationId: string;
      content: string;
    }) => {
      connectionRef.current?.invoke("SendMessage", conversationId, content);
    },
    [],
  );

  return { sendMessage };
};
```

> The module-level `connectionRef` ensures only one connection exists regardless of how many components call `useChatHub()`. The `isOwner` ref prevents the cleanup from stopping a connection that another instance created.

- [ ] **Step 4: Commit**

```bash
npm run format && npm run lint
git add src/features/chat/state/chatSlice.ts \
        src/features/chat/api/useChatHub.ts \
        src/lib/redux/store.ts
git commit -m "feat: add chat Redux slice and useChatHub SignalR hook

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 16: Chat UI Components

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/UnreadBadge.tsx`
- Create: `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx`
- Create: `automotive.marketplace.client/src/features/chat/components/ListingCard.tsx`
- Create: `automotive.marketplace.client/src/features/chat/components/ConversationList.tsx`
- Create: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`
- Create: `automotive.marketplace.client/src/features/chat/components/ChatPanel.tsx`

- [ ] **Step 1: Create `UnreadBadge.tsx`**

```tsx
import { useAppSelector } from "@/hooks/redux";
import { selectUnreadCount } from "../state/chatSlice";

const UnreadBadge = () => {
  const count = useAppSelector(selectUnreadCount);
  if (count === 0) return null;
  return (
    <span className="bg-destructive text-destructive-foreground absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full text-[10px] font-bold">
      {count > 99 ? "99+" : count}
    </span>
  );
};

export default UnreadBadge;
```

- [ ] **Step 2: Create `ActionBar.tsx`**

```tsx
import { Button } from "@/components/ui/button";

const ActionBar = () => (
  <div className="border-border flex items-center gap-2 border-b px-3 py-2">
    <Button variant="outline" size="sm" disabled>
      Make an Offer
    </Button>
    <Button variant="outline" size="sm" disabled>
      More ▾
    </Button>
    <span className="text-muted-foreground ml-auto text-xs">
      More actions coming soon
    </span>
  </div>
);

export default ActionBar;
```

- [ ] **Step 3: Create `ListingCard.tsx`**

```tsx
import { Link } from "@tanstack/react-router";

type ListingCardProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingThumbnailUrl: string | null;
};

const ListingCard = ({
  listingId,
  listingTitle,
  listingPrice,
  listingThumbnailUrl,
}: ListingCardProps) => (
  <div className="border-border bg-muted/40 flex items-center gap-3 border-b px-4 py-2">
    {listingThumbnailUrl && (
      <img
        src={listingThumbnailUrl}
        alt={listingTitle}
        className="h-10 w-14 rounded object-cover"
      />
    )}
    <div className="min-w-0 flex-1">
      <p className="truncate text-sm font-semibold">{listingTitle}</p>
      <p className="text-muted-foreground text-xs">
        {listingPrice.toLocaleString()} €
      </p>
    </div>
    <Link
      to="/listing/$id"
      params={{ id: listingId }}
      className="text-primary shrink-0 text-xs hover:underline"
    >
      View listing
    </Link>
  </div>
);

export default ListingCard;
```

- [ ] **Step 4: Create `ConversationList.tsx`**

```tsx
import { useSuspenseQuery } from "@tanstack/react-query";
import { formatDistanceToNow } from "date-fns";
import { getConversationsOptions } from "../api/getConversationsOptions";
import type { ConversationSummary } from "../types/ConversationSummary";

type ConversationListProps = {
  selectedId: string | null;
  onSelect: (conversation: ConversationSummary) => void;
};

const ConversationList = ({ selectedId, onSelect }: ConversationListProps) => {
  const { data: conversationsQuery } = useSuspenseQuery(getConversationsOptions());
  const conversations = conversationsQuery.data;

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
            {c.listingThumbnailUrl ? (
              <img
                src={c.listingThumbnailUrl}
                alt={c.listingTitle}
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
            {formatDistanceToNow(new Date(c.lastMessageAt), { addSuffix: true })}
          </p>
        </li>
      ))}
    </ul>
  );
};

export default ConversationList;
```

- [ ] **Step 5: Create `MessageThread.tsx`**

```tsx
import { Button } from "@/components/ui/button";
import { useAppSelector } from "@/hooks/redux";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { getMessagesOptions } from "../api/getMessagesOptions";
import { useMarkMessagesRead } from "../api/useMarkMessagesRead";
import { useChatHub } from "../api/useChatHub";
import type { ConversationSummary } from "../types/ConversationSummary";
import ActionBar from "./ActionBar";
import ListingCard from "./ListingCard";

type MessageThreadProps = {
  conversation: ConversationSummary;
  showListingCard?: boolean;
};

const MessageThread = ({
  conversation,
  showListingCard = true,
}: MessageThreadProps) => {
  const userId = useAppSelector((s) => s.auth.userId);
  const { data: messagesQuery } = useSuspenseQuery(
    getMessagesOptions({ conversationId: conversation.id }),
  );
  const messages = messagesQuery.data.messages;
  const { sendMessage } = useChatHub();
  const { mutate: markRead } = useMarkMessagesRead();
  const [input, setInput] = useState("");
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length]);

  useEffect(() => {
    markRead(conversation.id);
  }, [conversation.id, markRead]);

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    sendMessage({ conversationId: conversation.id, content: trimmed });
    setInput("");
  };

  return (
    <div className="flex h-full flex-col">
      {showListingCard && (
        <ListingCard
          listingId={conversation.listingId}
          listingTitle={conversation.listingTitle}
          listingPrice={conversation.listingPrice}
          listingThumbnailUrl={conversation.listingThumbnailUrl}
        />
      )}
      <ActionBar />
      <div className="flex-1 space-y-2 overflow-y-auto p-4">
        {messages.map((m) => {
          const isOwn = m.senderId === userId;
          return (
            <div
              key={m.id}
              className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`max-w-[75%] rounded-2xl px-3 py-2 text-sm ${
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
      <div className="border-border flex items-center gap-2 border-t p-3">
        <input
          className="border-input bg-background focus:ring-ring flex-1 rounded-full border px-4 py-2 text-sm focus:outline-none focus:ring-2"
          placeholder={`Message ${conversation.counterpartUsername}...`}
          value={input}
          onChange={(e) => setInput(e.target.value)}
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

- [ ] **Step 6: Create `ChatPanel.tsx`**

```tsx
import { useEffect } from "react";
import type { ConversationSummary } from "../types/ConversationSummary";
import MessageThread from "./MessageThread";

type ChatPanelProps = {
  conversation: ConversationSummary;
  onClose: () => void;
};

const ChatPanel = ({ conversation, onClose }: ChatPanelProps) => {
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onClose]);

  return (
    <div className="border-border bg-card fixed inset-y-0 right-0 z-50 flex w-80 flex-col border-l shadow-xl lg:w-96">
      <div className="border-border flex items-center gap-3 border-b px-4 py-3">
        <p className="flex-1 text-sm font-semibold">
          {conversation.counterpartUsername}
        </p>
        <button
          onClick={onClose}
          className="text-muted-foreground hover:text-foreground text-lg leading-none"
          aria-label="Close chat"
        >
          ✕
        </button>
      </div>
      <div className="flex-1 overflow-hidden">
        <MessageThread conversation={conversation} showListingCard={false} />
      </div>
    </div>
  );
};

export default ChatPanel;
```

- [ ] **Step 7: Commit**

```bash
npm run format && npm run lint
git add src/features/chat/components/
git commit -m "feat: add chat UI components

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 17: InboxPage + Route

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/pages/InboxPage.tsx`
- Create: `automotive.marketplace.client/src/app/pages/Inbox.tsx`
- Create: `automotive.marketplace.client/src/app/routes/inbox.tsx`
- Create: `automotive.marketplace.client/src/features/chat/index.ts`

- [ ] **Step 1: Create `InboxPage.tsx`**

```tsx
import { useState } from "react";
import ConversationList from "../components/ConversationList";
import MessageThread from "../components/MessageThread";
import type { ConversationSummary } from "../types/ConversationSummary";

const InboxPage = () => {
  const [selected, setSelected] = useState<ConversationSummary | null>(null);

  return (
    <div className="flex h-[calc(100vh-64px)] overflow-hidden">
      <aside className="border-border w-72 shrink-0 overflow-hidden border-r lg:w-80">
        <div className="border-border border-b px-4 py-3">
          <h1 className="text-lg font-semibold">Messages</h1>
        </div>
        <ConversationList
          selectedId={selected?.id ?? null}
          onSelect={setSelected}
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

export default InboxPage;
```

- [ ] **Step 2: Create `src/app/pages/Inbox.tsx`**

```tsx
import { InboxPage } from "@/features/chat";

const Inbox = () => <InboxPage />;

export default Inbox;
```

- [ ] **Step 3: Create `src/app/routes/inbox.tsx`**

```tsx
import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox")({
  component: Inbox,
});
```

- [ ] **Step 4: Create `features/chat/index.ts`**

```ts
export { default as ChatPanel } from "./components/ChatPanel";
export { default as UnreadBadge } from "./components/UnreadBadge";
export { default as InboxPage } from "./pages/InboxPage";
export { useChatHub } from "./api/useChatHub";
export { useGetOrCreateConversation } from "./api/useGetOrCreateConversation";
export type { ConversationSummary } from "./types/ConversationSummary";
```

- [ ] **Step 5: Verify route is detected**

```bash
npm run dev
```

Open `http://localhost:5173/inbox` — the page should render without errors. TanStack Router auto-regenerates `routeTree.gen.ts`.

- [ ] **Step 6: Commit**

```bash
npm run format && npm run lint
git add src/features/chat/pages/ src/app/pages/Inbox.tsx \
        src/app/routes/inbox.tsx src/features/chat/index.ts \
        src/routeTree.gen.ts
git commit -m "feat: add InboxPage and /inbox route

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 18: Update Header (Inbox Link + UnreadBadge)

**Files:**
- Modify: `automotive.marketplace.client/src/components/layout/header/Header.tsx`

- [ ] **Step 1: Add inbox link with `UnreadBadge` to `Header.tsx`**

Add import:
```tsx
import { UnreadBadge } from "@/features/chat";
```

Inside the existing nav links area, after the "Sell your car" link and only when `userId` is truthy, add:

```tsx
{userId && (
  <Link to="/inbox">
    <Button variant="link" className="relative">
      Inbox
      <UnreadBadge />
    </Button>
  </Link>
)}
```

- [ ] **Step 2: Build to confirm no TypeScript errors**

```bash
npm run format && npm run lint && npm run build
```

Expected: build succeeds.

- [ ] **Step 3: Commit**

```bash
npm run format && npm run lint
git add src/components/layout/header/Header.tsx
git commit -m "feat: add inbox link and unread badge to header

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 19: Update ListingDetailsContent (Contact Seller + ChatPanel)

Complete Appendix A first (add `sellerId` to `GetListingByIdResponse`).

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Add state, handler, and imports**

Add at the top of the file:

```tsx
import { useState } from "react";
import { ChatPanel, useGetOrCreateConversation } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";
```

Inside `ListingDetailsContent`, after `const { permissions }` line, add:

```tsx
const [chatConversation, setChatConversation] = useState<ConversationSummary | null>(null);
const { mutateAsync: getOrCreateConversation } = useGetOrCreateConversation();

const { userId } = useAppSelector((state) => state.auth);
const isSeller = listing.sellerId === userId;

const handleContactSeller = async () => {
  const res = await getOrCreateConversation({ listingId: id });
  setChatConversation({
    id: res.data.conversationId,
    listingId: id,
    listingTitle: `${listing.year} ${listing.make} ${listing.model}`,
    listingThumbnailUrl: listing.images[0]?.url ?? null,
    listingPrice: listing.price,
    counterpartId: listing.sellerId,
    counterpartUsername: listing.seller,
    lastMessage: null,
    lastMessageAt: new Date().toISOString(),
    unreadCount: 0,
  });
};
```

- [ ] **Step 2: Add "Contact Seller" button in JSX**

Inside the price/details card (after the badge section), add:

```tsx
{userId && !isSeller && (
  <Button className="mt-4 w-full" onClick={handleContactSeller}>
    Contact Seller
  </Button>
)}
```

- [ ] **Step 3: Render `ChatPanel` at end of return**

Just before the final closing `</div>` of the component's return, add:

```tsx
{chatConversation && (
  <ChatPanel
    conversation={chatConversation}
    onClose={() => setChatConversation(null)}
  />
)}
```

- [ ] **Step 4: Build and verify**

```bash
npm run format && npm run lint && npm run build
```

Expected: no TypeScript errors.

- [ ] **Step 5: Commit**

```bash
npm run format && npm run lint
git add src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: add Contact Seller button and ChatPanel to listing details

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 20: Wire useChatHub into Root Layout

**Files:**
- Modify: `automotive.marketplace.client/src/app/routes/__root.tsx`

- [ ] **Step 1: Call `useChatHub` in the root layout component**

Add import:
```tsx
import { useChatHub } from "@/features/chat";
```

Extract the anonymous component into a named function and call the hook:

```tsx
const RootLayout = () => {
  useChatHub();
  return (
    <>
      <Header />
      <div className="mx-8 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
    </>
  );
};

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
});
```

- [ ] **Step 2: Final build and all tests**

```bash
npm run build
cd .. && dotnet test Automotive.Marketplace.Tests
```

Expected: frontend build succeeds, all backend tests pass.

- [ ] **Step 3: Final commit**

```bash
npm run format && npm run lint
git add src/app/routes/__root.tsx
git commit -m "feat: wire useChatHub into root layout

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
