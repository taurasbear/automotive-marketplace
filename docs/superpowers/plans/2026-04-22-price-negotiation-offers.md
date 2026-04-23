# Price Negotiation (Offers) Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a full offer/counter-offer negotiation flow to the existing chat, where offers appear as status-aware cards in the message thread.

**Architecture:** Every offer action creates both an `Offer` record and a linked `Message` of type `Offer`, so offers sort naturally in the chronological message feed without any separate merge logic. The ChatHub dispatches CQRS commands and broadcasts five new SignalR events (`OfferMade`, `OfferAccepted`, `OfferDeclined`, `OfferCountered`, `OfferExpired`). A `BackgroundService` in the Server project runs every 15 minutes to auto-expire pending offers after 48 hours.

**Tech Stack:** C# / .NET 8, EF Core (Npgsql), MediatR, FluentValidation, xUnit + TestContainers + Bogus + FluentAssertions, SignalR, React 19 + TypeScript, TanStack Query, shadcn/ui, Tailwind CSS, Lucide React

---

## File Map

### New files

| File | Responsibility |
|---|---|
| `Automotive.Marketplace.Domain/Enums/OfferStatus.cs` | `Pending`, `Accepted`, `Declined`, `Countered`, `Expired` |
| `Automotive.Marketplace.Domain/Enums/MessageType.cs` | `Text`, `Offer` |
| `Automotive.Marketplace.Domain/Entities/Offer.cs` | Offer entity with self-referencing counter-offer FK |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/OfferConfiguration.cs` | EF fluent config for Offer |
| `Automotive.Marketplace.Infrastructure/Data/Builders/OfferBuilder.cs` | Bogus builder for tests |
| `Automotive.Marketplace.Infrastructure/Data/Builders/UserListingLikeBuilder.cs` | Bogus builder for tests |
| `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferCommand.cs` | CQRS command |
| `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferCommandHandler.cs` | Handler logic |
| `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferResponse.cs` | Response / SignalR payload |
| `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferCommand.cs` | CQRS command |
| `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferCommandHandler.cs` | Handler logic |
| `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferResponse.cs` | Response / SignalR payload |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/MakeOfferCommandHandlerTests.cs` | Integration tests |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToOfferCommandHandlerTests.cs` | Integration tests |
| `Automotive.Marketplace.Server/Services/OfferExpiryService.cs` | BackgroundService; expires pending offers every 15 min |
| `automotive.marketplace.client/src/features/chat/types/Offer.ts` | `Offer` and `OfferStatus` TS types |
| `automotive.marketplace.client/src/features/chat/types/OfferEventPayloads.ts` | Types for all 5 offer SignalR event payloads |
| `automotive.marketplace.client/src/features/chat/components/OfferCard.tsx` | Renders status-aware offer card in the chat thread |
| `automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx` | Modal dialog for making/countering an offer |

### Modified files

| File | What changes |
|---|---|
| `Automotive.Marketplace.Domain/Entities/Message.cs` | Add `MessageType`, `OfferId?`, navigation `Offer?` |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs` | Add OfferId FK + Offer relationship |
| `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` | Add `DbSet<Offer>` |
| `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs` | Add `MessageType` and `OfferData` nested record to `Message` |
| `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs` | Manual mapping (remove AutoMapper); include offer data |
| `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ConversationSummaryResponse.cs` | Add `BuyerId`, `SellerId`, `BuyerHasLiked` |
| `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs` | Populate new fields |
| `Automotive.Marketplace.Application/Mappings/ChatMappings.cs` | Remove now-obsolete `Message` mapping |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetMessagesQueryHandlerTests.cs` | Add offer message test |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetConversationsQueryHandlerTests.cs` | Add `BuyerHasLiked` test |
| `Automotive.Marketplace.Server/Hubs/ChatHub.cs` | Add `MakeOffer`, `RespondToOffer` hub methods |
| `Automotive.Marketplace.Server/Program.cs` | Register `OfferExpiryService` as hosted service |
| `automotive.marketplace.client/src/features/chat/constants/chatHub.ts` | Add 5 new server-to-client event names + 2 new client-to-server methods |
| `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts` | Add `messageType`, `offer?` to inline `Message` type |
| `automotive.marketplace.client/src/features/chat/types/ReceiveMessagePayload.ts` | Add `messageType`, `offer?` |
| `automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts` | Add `buyerId`, `sellerId`, `buyerHasLiked` |
| `automotive.marketplace.client/src/features/chat/api/useChatHub.ts` | Add `sendOffer`, `respondToOffer`; handle 5 offer events |
| `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx` | Props-driven; enable "Make an Offer" button |
| `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx` | Render `OfferCard` for offer-type messages; pass props to `ActionBar` |
| `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx` | Add `buyerId`, `sellerId`, `buyerHasLiked` to manual `chatConversation` construction |

---

## Task 1: Domain enums and entities

**Files:**
- Create: `Automotive.Marketplace.Domain/Enums/OfferStatus.cs`
- Create: `Automotive.Marketplace.Domain/Enums/MessageType.cs`
- Create: `Automotive.Marketplace.Domain/Entities/Offer.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Message.cs`

- [ ] **Step 1: Create `OfferStatus.cs`**

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum OfferStatus
{
    Pending,
    Accepted,
    Declined,
    Countered,
    Expired
}
```

- [ ] **Step 2: Create `MessageType.cs`**

```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum MessageType
{
    Text,
    Offer
}
```

- [ ] **Step 3: Create `Offer.cs`**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Offer : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public decimal Amount { get; set; }

    public OfferStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public Guid? ParentOfferId { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual Offer? ParentOffer { get; set; }

    public virtual ICollection<Offer> CounterOffers { get; set; } = [];

    public virtual Message? Message { get; set; }
}
```

- [ ] **Step 4: Update `Message.cs` — add `MessageType`, `OfferId?`, and `Offer?` navigation**

Add these three properties to the existing `Message` class (below the `IsRead` property):

```csharp
public MessageType MessageType { get; set; }

public Guid? OfferId { get; set; }

public virtual Offer? Offer { get; set; }
```

Also add the using at the top:
```csharp
using Automotive.Marketplace.Domain.Enums;
```

- [ ] **Step 5: Verify build**

```bash
cd /path/to/repo && dotnet build Automotive.Marketplace.Domain/Automotive.Marketplace.Domain.csproj --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/
git commit -m "feat: add Offer entity, OfferStatus and MessageType enums, extend Message"
```

---

## Task 2: EF Core configuration and migration

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/OfferConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create `OfferConfiguration.cs`**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.Property(o => o.Amount)
            .HasPrecision(18, 2);

        builder.HasOne(o => o.Conversation)
            .WithMany()
            .HasForeignKey(o => o.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Initiator)
            .WithMany()
            .HasForeignKey(o => o.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ParentOffer)
            .WithMany(o => o.CounterOffers)
            .HasForeignKey(o => o.ParentOfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Message)
            .WithOne(m => m.Offer)
            .HasForeignKey<Message>(m => m.OfferId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

- [ ] **Step 2: Update `MessageConfiguration.cs` — add `MessageType` default and `OfferId` property**

Add inside the `Configure` method, after the existing `HasOne` calls:

```csharp
builder.Property(m => m.MessageType)
    .HasDefaultValue(MessageType.Text);

builder.Property(m => m.OfferId)
    .IsRequired(false);
```

Add the using at the top of the file:
```csharp
using Automotive.Marketplace.Domain.Enums;
```

- [ ] **Step 3: Add `DbSet<Offer>` to `AutomotiveContext.cs`**

Add after the existing `DbSet<Message> Messages`:

```csharp
public DbSet<Offer> Offers { get; set; }
```

- [ ] **Step 4: Add migration**

```bash
cd /path/to/repo
dotnet ef migrations add AddOfferEntities \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected: A new migration file is created under `Automotive.Marketplace.Infrastructure/Migrations/`.

- [ ] **Step 5: Verify the migration looks correct**

Open the generated migration file and confirm it:
- Creates an `Offers` table with `Id`, `ConversationId`, `InitiatorId`, `Amount`, `Status`, `ExpiresAt`, `ParentOfferId`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- Adds `MessageType` column to `Messages` with a default value of `0`
- Adds `OfferId` nullable column to `Messages`
- Adds FK from `Messages.OfferId` → `Offers.Id`

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/
git commit -m "feat: EF Core config for Offer entity and AddOfferEntities migration"
```

---

## Task 3: Test builders

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/OfferBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/UserListingLikeBuilder.cs`

- [ ] **Step 1: Create `OfferBuilder.cs`**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class OfferBuilder
{
    private readonly Faker<Offer> _faker;

    public OfferBuilder()
    {
        _faker = new Faker<Offer>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.ConversationId, f => f.Random.Guid())
            .RuleFor(o => o.InitiatorId, f => f.Random.Guid())
            .RuleFor(o => o.Amount, f => f.Random.Decimal(1000, 10000))
            .RuleFor(o => o.Status, OfferStatus.Pending)
            .RuleFor(o => o.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(o => o.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(o => o.CreatedBy, f => f.Random.Guid().ToString());
    }

    public OfferBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(o => o.ConversationId, conversationId);
        return this;
    }

    public OfferBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(o => o.InitiatorId, initiatorId);
        return this;
    }

    public OfferBuilder WithAmount(decimal amount)
    {
        _faker.RuleFor(o => o.Amount, amount);
        return this;
    }

    public OfferBuilder WithStatus(OfferStatus status)
    {
        _faker.RuleFor(o => o.Status, status);
        return this;
    }

    public OfferBuilder AsExpired()
    {
        _faker.RuleFor(o => o.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public OfferBuilder WithParent(Guid parentOfferId)
    {
        _faker.RuleFor(o => o.ParentOfferId, parentOfferId);
        return this;
    }

    public Offer Build() => _faker.Generate();
}
```

- [ ] **Step 2: Create `UserListingLikeBuilder.cs`**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class UserListingLikeBuilder
{
    private readonly Faker<UserListingLike> _faker;

    public UserListingLikeBuilder()
    {
        _faker = new Faker<UserListingLike>()
            .RuleFor(l => l.Id, f => f.Random.Guid())
            .RuleFor(l => l.UserId, f => f.Random.Guid())
            .RuleFor(l => l.ListingId, f => f.Random.Guid())
            .RuleFor(l => l.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(l => l.CreatedBy, f => f.Random.Guid().ToString());
    }

    public UserListingLikeBuilder WithUser(Guid userId)
    {
        _faker.RuleFor(l => l.UserId, userId);
        return this;
    }

    public UserListingLikeBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(l => l.ListingId, listingId);
        return this;
    }

    public UserListingLike Build() => _faker.Generate();
}
```

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Data/Builders/
git commit -m "feat: add OfferBuilder and UserListingLikeBuilder test helpers"
```

---

## Task 4: MakeOffer feature (TDD)

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/MakeOfferCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/MakeOfferCommandHandlerTests.cs`

- [ ] **Step 1: Create `MakeOfferResponse.cs`**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public sealed record MakeOfferResponse
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public OfferData Offer { get; set; } = null!;

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
}
```

- [ ] **Step 2: Create `MakeOfferCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public sealed record MakeOfferCommand : IRequest<MakeOfferResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public decimal Amount { get; set; }
}
```

- [ ] **Step 3: Write the failing tests**

Create `MakeOfferCommandHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
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

public class MakeOfferCommandHandlerTests(
    DatabaseFixture<MakeOfferCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<MakeOfferCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<MakeOfferCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private MakeOfferCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerMakesValidOffer_ShouldPersistOfferAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var offerAmount = listing.Price * 0.8m;
        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = offerAmount
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.Offer.Id.Should().NotBeEmpty();
        result.Offer.Amount.Should().Be(offerAmount);
        result.Offer.Status.Should().Be(OfferStatus.Pending);
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().NotBe(buyer.Id);

        var savedOffer = await context.Offers.FindAsync(result.Offer.Id);
        savedOffer.Should().NotBeNull();
        savedOffer!.Status.Should().Be(OfferStatus.Pending);
        savedOffer.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Offer);
        savedMessage.OfferId.Should().Be(result.Offer.Id);
    }

    [Fact]
    public async Task Handle_SellerMakesOfferWhenBuyerHasLiked_ShouldSucceed()
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

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            Amount = listing.Price * 0.95m
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Offer.Id.Should().NotBeEmpty();
        result.Offer.Status.Should().Be(OfferStatus.Pending);
    }

    [Fact]
    public async Task Handle_SellerMakesOfferWithoutBuyerLike_ShouldThrowUnauthorizedException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            Amount = listing.Price * 0.9m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AmountBelowMinimum_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price / 4  // less than 1/3
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_AmountAboveListingPrice_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price + 1
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_ListingNotAvailable_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);
        listing.Status = Status.OnHold;
        await context.SaveChangesAsync();

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price * 0.8m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_ActiveOfferAlreadyExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var existingOffer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithAmount(listing.Price * 0.8m)
            .Build();
        await context.AddAsync(existingOffer);
        await context.SaveChangesAsync();

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price * 0.7m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
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

- [ ] **Step 4: Run tests to confirm they fail (compilation error expected — handler doesn't exist yet)**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~MakeOfferCommandHandlerTests" --no-build 2>&1 | tail -5
```

Expected: Build error — `MakeOfferCommandHandler` not found.

- [ ] **Step 5: Create `MakeOfferCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public class MakeOfferCommandHandler(IRepository repository)
    : IRequestHandler<MakeOfferCommand, MakeOfferResponse>
{
    public async Task<MakeOfferResponse> Handle(
        MakeOfferCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);

            if (!buyerHasLiked)
                throw new UnauthorizedAccessException(
                    "Seller can only make an offer if the buyer has liked the listing.");
        }

        if (listing.Status != Status.Available)
            throw new RequestValidationException(
            [
                new ValidationFailure("ListingId", "Offers can only be made on available listings.")
            ]);

        var hasActiveOffer = await repository.AsQueryable<Offer>()
            .AnyAsync(o => o.ConversationId == request.ConversationId
                        && o.Status == OfferStatus.Pending, cancellationToken);

        if (hasActiveOffer)
            throw new RequestValidationException(
            [
                new ValidationFailure("ConversationId", "There is already a pending offer in this conversation.")
            ]);

        if (request.Amount < listing.Price / 3)
            throw new RequestValidationException(
            [
                new ValidationFailure("Amount",
                    $"Offer must be at least {listing.Price / 3:C} (one third of the asking price).")
            ]);

        if (request.Amount > listing.Price)
            throw new RequestValidationException(
            [
                new ValidationFailure("Amount", "Offer cannot exceed the listing price.")
            ]);

        var recipientId = isSeller ? conversation.BuyerId : listing.SellerId;
        var senderUsername = isSeller
            ? listing.Seller.Username
            : conversation.Buyer.Username;

        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Amount = request.Amount,
            Status = OfferStatus.Pending,
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
            MessageType = MessageType.Offer,
            OfferId = offer.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(offer, cancellationToken);
        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var percentageOff = Math.Round((listing.Price - offer.Amount) / listing.Price * 100, 2);

        return new MakeOfferResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            Offer = new MakeOfferResponse.OfferData
            {
                Id = offer.Id,
                Amount = offer.Amount,
                ListingPrice = listing.Price,
                PercentageOff = percentageOff,
                Status = offer.Status,
                ExpiresAt = offer.ExpiresAt,
                InitiatorId = offer.InitiatorId,
                ParentOfferId = offer.ParentOfferId
            }
        };
    }
}
```

- [ ] **Step 6: Run the tests**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~MakeOfferCommandHandlerTests" -v n 2>&1 | tail -20
```

Expected: All 6 tests pass.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/MakeOffer/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/MakeOfferCommandHandlerTests.cs
git commit -m "feat: add MakeOfferCommand handler with TDD tests"
```

---

## Task 5: RespondToOffer feature (TDD)

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/RespondToOfferCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToOfferCommandHandlerTests.cs`

- [ ] **Step 1: Create `RespondToOfferResponse.cs`**

```csharp
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public sealed record RespondToOfferResponse
{
    public Guid OfferId { get; set; }

    public Guid ConversationId { get; set; }

    public OfferStatus NewStatus { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ResponderId { get; set; }

    public MakeOffer.MakeOfferResponse? CounterOffer { get; set; }
}
```

- [ ] **Step 2: Create `RespondToOfferCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public sealed record RespondToOfferCommand : IRequest<RespondToOfferResponse>
{
    public Guid OfferId { get; set; }

    public Guid ResponderId { get; set; }

    public OfferResponseAction Action { get; set; }

    public decimal? CounterAmount { get; set; }
}

public enum OfferResponseAction
{
    Accept,
    Decline,
    Counter
}
```

- [ ] **Step 3: Write the failing tests**

Create `RespondToOfferCommandHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;
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

public class RespondToOfferCommandHandlerTests(
    DatabaseFixture<RespondToOfferCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToOfferCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToOfferCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToOfferCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_AcceptOffer_ShouldSetStatusAcceptedAndListingOnHold()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        result.NewStatus.Should().Be(OfferStatus.Accepted);
        result.CounterOffer.Should().BeNull();

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Accepted);

        await context.Entry(listing).ReloadAsync();
        listing.Status.Should().Be(Status.OnHold);
    }

    [Fact]
    public async Task Handle_DeclineOffer_ShouldSetStatusDeclinedAndListingStaysAvailable()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Decline
        }, CancellationToken.None);

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Declined);

        await context.Entry(listing).ReloadAsync();
        listing.Status.Should().Be(Status.Available);
    }

    [Fact]
    public async Task Handle_CounterOffer_ShouldCreateNewOfferAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var counterAmount = listing.Price * 0.9m;
        var result = await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Counter,
            CounterAmount = counterAmount
        }, CancellationToken.None);

        result.NewStatus.Should().Be(OfferStatus.Countered);
        result.CounterOffer.Should().NotBeNull();
        result.CounterOffer!.Offer.Amount.Should().Be(counterAmount);
        result.CounterOffer.Offer.ParentOfferId.Should().Be(offer.Id);

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Countered);

        var newOffer = await context.Offers.FindAsync(result.CounterOffer.Offer.Id);
        newOffer.Should().NotBeNull();
        newOffer!.Status.Should().Be(OfferStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.CounterOffer.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Offer);
        newMessage.OfferId.Should().Be(newOffer.Id);
    }

    [Fact]
    public async Task Handle_OfferAlreadyResolved_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);
        offer.Status = OfferStatus.Accepted;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Decline
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnOffer_ShouldThrowUnauthorizedException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, offer, _) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = buyer.Id,  // buyer made the offer; can't respond to own offer
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_OfferExpired_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);
        offer.ExpiresAt = DateTime.UtcNow.AddHours(-1);  // already expired
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_CounterAmountBelowMinimum_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Counter,
            CounterAmount = listing.Price / 4  // below 1/3 floor
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Offer offer, Listing listing)>
        SeedPendingOfferAsync(AutomotiveContext context, bool initiatedByBuyer)
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
        var offer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .WithAmount(listing.Price * 0.8m)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation, offer);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, offer, listing);
    }
}
```

- [ ] **Step 4: Run tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RespondToOfferCommandHandlerTests" --no-build 2>&1 | tail -5
```

Expected: Build error — `RespondToOfferCommandHandler` not found.

- [ ] **Step 5: Create `RespondToOfferCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public class RespondToOfferCommandHandler(IRepository repository)
    : IRequestHandler<RespondToOfferCommand, RespondToOfferResponse>
{
    public async Task<RespondToOfferResponse> Handle(
        RespondToOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await repository.GetByIdAsync<Offer>(request.OfferId, cancellationToken);
        var conversation = offer.Conversation;
        var listing = conversation.Listing;

        if (offer.Status != OfferStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("OfferId", "This offer has already been resolved.")
            ]);

        if (offer.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own offer.");

        if (offer.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("OfferId", "This offer has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        if (request.Action == OfferResponseAction.Accept)
        {
            offer.Status = OfferStatus.Accepted;
            listing.Status = Status.OnHold;

            await repository.UpdateAsync(offer, cancellationToken);
            await repository.UpdateAsync(listing, cancellationToken);

            return new RespondToOfferResponse
            {
                OfferId = offer.Id,
                ConversationId = conversation.Id,
                NewStatus = OfferStatus.Accepted,
                InitiatorId = offer.InitiatorId,
                ResponderId = request.ResponderId,
                CounterOffer = null
            };
        }

        if (request.Action == OfferResponseAction.Decline)
        {
            offer.Status = OfferStatus.Declined;
            await repository.UpdateAsync(offer, cancellationToken);

            return new RespondToOfferResponse
            {
                OfferId = offer.Id,
                ConversationId = conversation.Id,
                NewStatus = OfferStatus.Declined,
                InitiatorId = offer.InitiatorId,
                ResponderId = request.ResponderId,
                CounterOffer = null
            };
        }

        // Counter
        var counterAmount = request.CounterAmount!.Value;

        if (counterAmount < listing.Price / 3)
            throw new RequestValidationException(
            [
                new ValidationFailure("CounterAmount",
                    $"Counter offer must be at least {listing.Price / 3:C} (one third of the asking price).")
            ]);

        if (counterAmount > listing.Price)
            throw new RequestValidationException(
            [
                new ValidationFailure("CounterAmount", "Counter offer cannot exceed the listing price.")
            ]);

        offer.Status = OfferStatus.Countered;

        var counterOffer = new Offer
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            Amount = counterAmount,
            Status = OfferStatus.Pending,
            ParentOfferId = offer.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        var counterMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Offer,
            OfferId = counterOffer.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = counterMessage.SentAt;

        await repository.UpdateAsync(offer, cancellationToken);
        await repository.CreateAsync(counterOffer, cancellationToken);
        await repository.CreateAsync(counterMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var percentageOff = Math.Round(
            (listing.Price - counterOffer.Amount) / listing.Price * 100, 2);

        return new RespondToOfferResponse
        {
            OfferId = offer.Id,
            ConversationId = conversation.Id,
            NewStatus = OfferStatus.Countered,
            InitiatorId = offer.InitiatorId,
            ResponderId = request.ResponderId,
            CounterOffer = new MakeOfferResponse
            {
                MessageId = counterMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = counterMessage.SentAt,
                RecipientId = offer.InitiatorId,
                Offer = new MakeOfferResponse.OfferData
                {
                    Id = counterOffer.Id,
                    Amount = counterOffer.Amount,
                    ListingPrice = listing.Price,
                    PercentageOff = percentageOff,
                    Status = OfferStatus.Pending,
                    ExpiresAt = counterOffer.ExpiresAt,
                    InitiatorId = request.ResponderId,
                    ParentOfferId = counterOffer.ParentOfferId
                }
            }
        };
    }
}
```

- [ ] **Step 6: Run the tests**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RespondToOfferCommandHandlerTests" -v n 2>&1 | tail -20
```

Expected: All 7 tests pass.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/RespondToOffer/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToOfferCommandHandlerTests.cs
git commit -m "feat: add RespondToOfferCommand handler with TDD tests"
```

---

## Task 6: GetMessages — include offer data in response

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/ChatMappings.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetMessagesQueryHandlerTests.cs`

- [ ] **Step 1: Update `GetMessagesResponse.cs` — add `MessageType` and `OfferData` to nested `Message`**

Replace the existing file content with:

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
    }
}
```

- [ ] **Step 2: Update `ChatMappings.cs` — remove obsolete mapping (handler will use manual mapping)**

Replace the entire file with:

```csharp
using AutoMapper;

namespace Automotive.Marketplace.Application.Mappings;

public class ChatMappings : Profile
{
    public ChatMappings() { }
}
```

- [ ] **Step 3: Write failing test** — add to `GetMessagesQueryHandlerTests.cs`

Open the existing file and add this test method inside the class:

```csharp
[Fact]
public async Task Handle_MessageWithOffer_ShouldIncludeOfferData()
{
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var handler = new GetMessagesQueryHandler(
        scope.ServiceProvider.GetRequiredService<IRepository>());
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var (buyer, seller, conversation) = await SeedConversationAsync(context);

    var offer = new OfferBuilder()
        .WithConversation(conversation.Id)
        .WithInitiator(buyer.Id)
        .WithAmount(12000m)
        .Build();

    var offerMessage = new MessageBuilder()
        .WithConversation(conversation.Id)
        .WithSender(buyer.Id)
        .With(m => m.MessageType, MessageType.Offer)
        .With(m => m.OfferId, offer.Id)
        .With(m => m.Content, string.Empty)
        .Build();

    await context.AddRangeAsync(offer, offerMessage);
    await context.SaveChangesAsync();

    var result = await handler.Handle(new GetMessagesQuery
    {
        ConversationId = conversation.Id,
        UserId = buyer.Id
    }, CancellationToken.None);

    var offerMsg = result.Messages.Single(m => m.MessageType == MessageType.Offer);
    offerMsg.Offer.Should().NotBeNull();
    offerMsg.Offer!.Amount.Should().Be(12000m);
    offerMsg.Offer.ListingPrice.Should().BeGreaterThan(0);
    offerMsg.Offer.PercentageOff.Should().BeGreaterThan(0);
    offerMsg.Offer.Status.Should().Be(OfferStatus.Pending);
    offerMsg.Offer.InitiatorId.Should().Be(buyer.Id);
}
```

Also add the required usings at the top of the test file:
```csharp
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
```

- [ ] **Step 4: Run the test to confirm it fails**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~GetMessagesQueryHandlerTests.Handle_MessageWithOffer" -v n 2>&1 | tail -10
```

Expected: FAIL — `GetMessagesQueryHandler` doesn't yet return offer data.

- [ ] **Step 5: Update `GetMessagesQueryHandler.cs` — switch to manual mapping, include offer**

Replace the entire file with:

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
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

- [ ] **Step 6: Run all GetMessages tests**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~GetMessagesQueryHandlerTests" -v n 2>&1 | tail -15
```

Expected: All tests pass (existing tests + new offer test).

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/ \
        Automotive.Marketplace.Application/Mappings/ChatMappings.cs \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetMessagesQueryHandlerTests.cs
git commit -m "feat: extend GetMessages response with MessageType and OfferData"
```

---

## Task 7: GetConversations — add BuyerId, SellerId, BuyerHasLiked

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ConversationSummaryResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetConversationsQueryHandlerTests.cs`

- [ ] **Step 1: Update `ConversationSummaryResponse.cs`**

Add three new properties to the existing record (at the end, before the closing brace):

```csharp
public Guid BuyerId { get; set; }

public Guid SellerId { get; set; }

public bool BuyerHasLiked { get; set; }
```

- [ ] **Step 2: Write failing test** — add to `GetConversationsQueryHandlerTests.cs`

`DatabaseFixture` does not register `IImageStorageService`, so use NSubstitute to create a stub that returns `null` for the presigned URL (no images in test data). Add these two test methods inside the class:

```csharp
[Fact]
public async Task Handle_BuyerHasLikedListing_ShouldReturnBuyerHasLikedTrue()
{
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var imageService = NSubstitute.Substitute.For<Automotive.Marketplace.Application.Interfaces.Services.IImageStorageService>();
    var handler = new GetConversationsQueryHandler(
        scope.ServiceProvider.GetRequiredService<IRepository>(),
        imageService);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var (buyer, seller, conversation, listing) = await SeedAsync(context);

    var like = new UserListingLikeBuilder()
        .WithUser(buyer.Id)
        .WithListing(listing.Id)
        .Build();
    await context.AddAsync(like);
    await context.SaveChangesAsync();

    var result = (await handler.Handle(
        new GetConversationsQuery { UserId = seller.Id },
        CancellationToken.None)).ToList();

    result.Should().ContainSingle();
    result[0].BuyerHasLiked.Should().BeTrue();
    result[0].BuyerId.Should().Be(buyer.Id);
    result[0].SellerId.Should().Be(seller.Id);
}

[Fact]
public async Task Handle_BuyerHasNotLikedListing_ShouldReturnBuyerHasLikedFalse()
{
    await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
    var imageService = NSubstitute.Substitute.For<Automotive.Marketplace.Application.Interfaces.Services.IImageStorageService>();
    var handler = new GetConversationsQueryHandler(
        scope.ServiceProvider.GetRequiredService<IRepository>(),
        imageService);
    var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

    var (buyer, seller, conversation, listing) = await SeedAsync(context);

    var result = (await handler.Handle(
        new GetConversationsQuery { UserId = seller.Id },
        CancellationToken.None)).ToList();

    result.Should().ContainSingle();
    result[0].BuyerHasLiked.Should().BeFalse();
}
```

Also add the required using at the top:
```csharp
using Automotive.Marketplace.Infrastructure.Data.Builders;
```

The existing test file likely has a `SeedAsync` helper — if it only returns `(buyer, seller, conversation)` you need to update it to also return `listing`. Check the existing helper and add `listing` to the return tuple and the seeding logic if not already present. The helper should look like:

```csharp
private static async Task<(User buyer, User seller, Conversation conversation, Listing listing)>
    SeedAsync(AutomotiveContext context)
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
        .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
    var conversation = new ConversationBuilder()
        .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

    await context.AddRangeAsync(seller, buyer, make, model, fuel,
        transmission, bodyType, drivetrain, variant, listing, conversation);
    await context.SaveChangesAsync();

    return (buyer, seller, conversation, listing);
}
```

Update existing tests that used the old `(buyer, seller, conversation)` tuple to use `(buyer, seller, conversation, _)`.

- [ ] **Step 3: Run new tests to confirm they fail**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~GetConversationsQueryHandlerTests.Handle_BuyerHasLiked" -v n 2>&1 | tail -10
```

Expected: FAIL — `BuyerHasLiked` not populated yet.

- [ ] **Step 4: Update `GetConversationsQueryHandler.cs`**

In the `foreach` loop, add the `BuyerHasLiked` query and populate the new response fields. Add this before `result.Add(...)`:

```csharp
var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
    .AnyAsync(l => l.UserId == conversation.BuyerId
                && l.ListingId == listing.Id, cancellationToken);
```

Then update `result.Add(new ConversationSummaryResponse { ... })` to include:

```csharp
BuyerId = conversation.BuyerId,
SellerId = listing.SellerId,
BuyerHasLiked = buyerHasLiked,
```

Also add the using at the top of the handler file:
```csharp
using Automotive.Marketplace.Domain.Entities;
```
(only if not already present — it likely is via lazy loading)

- [ ] **Step 5: Run all GetConversations tests**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~GetConversationsQueryHandlerTests" -v n 2>&1 | tail -15
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/GetConversationsQueryHandlerTests.cs
git commit -m "feat: add BuyerId, SellerId, BuyerHasLiked to GetConversations response"
```

---

## Task 8: OfferExpiryService and registration

**Files:**
- Create: `Automotive.Marketplace.Server/Services/OfferExpiryService.cs`
- Modify: `Automotive.Marketplace.Server/Program.cs`

- [ ] **Step 1: Create `OfferExpiryService.cs`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Server.Services;

public class OfferExpiryService(
    IServiceScopeFactory scopeFactory,
    IHubContext<ChatHub> hubContext,
    ILogger<OfferExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            await ExpireOffersAsync(stoppingToken);
        }
    }

    private async Task ExpireOffersAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredOffers = await repository.AsQueryable<Offer>()
                .Where(o => o.Status == OfferStatus.Pending && o.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var offer in expiredOffers)
            {
                offer.Status = OfferStatus.Expired;
                await repository.UpdateAsync(offer, cancellationToken);

                var conversation = offer.Conversation;
                var recipientId = offer.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    offerId = offer.Id,
                    conversationId = offer.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{offer.InitiatorId}")
                    .SendAsync("OfferExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("OfferExpired", payload, cancellationToken);
            }

            if (expiredOffers.Count > 0)
                logger.LogInformation("Expired {Count} pending offers.", expiredOffers.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring offers.");
        }
    }
}
```

- [ ] **Step 2: Register in `Program.cs`**

Add after `builder.Services.AddSignalR();`:

```csharp
builder.Services.AddHostedService<Automotive.Marketplace.Server.Services.OfferExpiryService>();
```

- [ ] **Step 3: Verify build**

```bash
dotnet build Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Server/Services/OfferExpiryService.cs \
        Automotive.Marketplace.Server/Program.cs
git commit -m "feat: add OfferExpiryService background service to auto-expire pending offers"
```

---

## Task 9: ChatHub extensions

**Files:**
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`

- [ ] **Step 1: Update `ChatHub.cs` — add `MakeOffer` and `RespondToOffer` hub methods**

Replace the entire file with:

```csharp
using System.Security.Claims;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;
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

    public async Task MakeOffer(Guid conversationId, decimal amount)
    {
        var result = await mediator.Send(new MakeOfferCommand
        {
            ConversationId = conversationId,
            InitiatorId = UserId,
            Amount = amount
        });

        await Clients.Group($"user-{UserId}").SendAsync("OfferMade", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("OfferMade", result);
    }

    public async Task RespondToOffer(Guid offerId, string action, decimal? counterAmount = null)
    {
        var result = await mediator.Send(new RespondToOfferCommand
        {
            OfferId = offerId,
            ResponderId = UserId,
            Action = Enum.Parse<OfferResponseAction>(action, ignoreCase: true),
            CounterAmount = counterAmount
        });

        var eventName = result.NewStatus switch
        {
            Domain.Enums.OfferStatus.Accepted => "OfferAccepted",
            Domain.Enums.OfferStatus.Declined => "OfferDeclined",
            Domain.Enums.OfferStatus.Countered => "OfferCountered",
            _ => throw new InvalidOperationException($"Unexpected offer status: {result.NewStatus}")
        };

        await Clients.Group($"user-{result.InitiatorId}").SendAsync(eventName, result);
        await Clients.Group($"user-{result.ResponderId}").SendAsync(eventName, result);
    }
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Run the full backend test suite**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj -v n 2>&1 | tail -20
```

Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Server/Hubs/ChatHub.cs
git commit -m "feat: extend ChatHub with MakeOffer and RespondToOffer hub methods"
```

---

## Task 10: Frontend types

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/types/Offer.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/OfferEventPayloads.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/ReceiveMessagePayload.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts`
- Modify: `automotive.marketplace.client/src/features/chat/constants/chatHub.ts`

- [ ] **Step 1: Create `types/Offer.ts`**

```typescript
export type OfferStatus = 'Pending' | 'Accepted' | 'Declined' | 'Countered' | 'Expired';

export type Offer = {
  id: string;
  amount: number;
  listingPrice: number;
  percentageOff: number;
  status: OfferStatus;
  expiresAt: string;
  initiatorId: string;
  parentOfferId?: string;
};
```

- [ ] **Step 2: Create `types/OfferEventPayloads.ts`**

```typescript
import type { Offer } from './Offer';

// Payload for "OfferMade" and the counterOffer within "OfferCountered"
export type OfferMadePayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  offer: Offer;
};

// Payload for "OfferAccepted" and "OfferDeclined"
export type OfferStatusUpdatedPayload = {
  offerId: string;
  conversationId: string;
  newStatus: 'Accepted' | 'Declined';
  initiatorId: string;
  responderId: string;
  counterOffer: null;
};

// Payload for "OfferCountered"
export type OfferCounteredPayload = {
  offerId: string;
  conversationId: string;
  newStatus: 'Countered';
  initiatorId: string;
  responderId: string;
  counterOffer: OfferMadePayload;
};

// Payload for "OfferExpired" (from background service)
export type OfferExpiredPayload = {
  offerId: string;
  conversationId: string;
};
```

- [ ] **Step 3: Update `types/GetMessagesResponse.ts`**

Replace the file contents with:

```typescript
import type { Offer } from './Offer';

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
  messageType: 'Text' | 'Offer';
  offer?: Offer;
};
```

- [ ] **Step 4: Update `types/ReceiveMessagePayload.ts`**

```typescript
import type { Offer } from './Offer';

export type ReceiveMessagePayload = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: 'Text' | 'Offer';
  offer?: Offer;
};
```

- [ ] **Step 5: Update `types/ConversationSummary.ts`**

```typescript
export type ConversationSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingThumbnail: { url: string; altText: string } | null;
  listingPrice: number;
  counterpartId: string;
  counterpartUsername: string;
  lastMessage: string | null;
  lastMessageAt: string;
  unreadCount: number;
  buyerId: string;
  sellerId: string;
  buyerHasLiked: boolean;
};
```

- [ ] **Step 6: Update `constants/chatHub.ts`**

```typescript
export const HUB_METHODS = {
  // Client → Server
  SEND_MESSAGE: 'SendMessage',
  MAKE_OFFER: 'MakeOffer',
  RESPOND_TO_OFFER: 'RespondToOffer',
  // Server → Client
  RECEIVE_MESSAGE: 'ReceiveMessage',
  UPDATE_UNREAD_COUNT: 'UpdateUnreadCount',
  OFFER_MADE: 'OfferMade',
  OFFER_ACCEPTED: 'OfferAccepted',
  OFFER_DECLINED: 'OfferDeclined',
  OFFER_COUNTERED: 'OfferCountered',
  OFFER_EXPIRED: 'OfferExpired',
} as const;
```

- [ ] **Step 7: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit 2>&1 | head -30
```

Expected: No errors related to the new types (there may be downstream errors — note them; they'll be resolved in later tasks).

- [ ] **Step 8: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/types/ \
        automotive.marketplace.client/src/features/chat/constants/
git commit -m "feat: add Offer types and extend chat types for offer negotiation"
```

---

## Task 11: useChatHub extensions

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

- [ ] **Step 1: Replace `useChatHub.ts` with the extended version**

```typescript
import { chatKeys } from '@/api/queryKeys/chatKeys';
import { selectAccessToken } from '@/features/auth';
import { useAppSelector } from '@/hooks/redux';
import queryClient from '@/lib/tanstack-query/queryClient';
import * as signalR from '@microsoft/signalr';
import type { AxiosResponse } from 'axios';
import { useCallback, useEffect, useRef } from 'react';
import { HUB_METHODS } from '../constants/chatHub';
import type { GetUnreadCountResponse } from '../api/getUnreadCountOptions';
import type { GetMessagesResponse, Message } from '../types/GetMessagesResponse';
import type { ReceiveMessagePayload } from '../types/ReceiveMessagePayload';
import type {
  OfferCounteredPayload,
  OfferExpiredPayload,
  OfferMadePayload,
  OfferStatusUpdatedPayload,
} from '../types/OfferEventPayloads';

const apiBase =
  (import.meta.env.VITE_APP_API_URL as string) ||
  'https://api.automotive-marketplace.taurasbear.me';
const hubUrl = import.meta.env.PROD ? `${apiBase}/hubs/chat` : '/api/hubs/chat';

const connectionRef = { current: null as signalR.HubConnection | null };

export const useChatHub = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const isOwner = useRef(false);

  useEffect(() => {
    if (!accessToken) return;
    if (connectionRef.current) return;

    isOwner.current = true;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on(HUB_METHODS.RECEIVE_MESSAGE, (message: ReceiveMessagePayload) => {
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
      void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    });

    connection.on(HUB_METHODS.UPDATE_UNREAD_COUNT, (count: number) => {
      queryClient.setQueryData<AxiosResponse<GetUnreadCountResponse>>(
        chatKeys.unreadCount(),
        (old) => {
          if (!old) return old;
          return { ...old, data: { unreadCount: count } };
        },
      );
    });

    connection.on(HUB_METHODS.OFFER_MADE, (payload: OfferMadePayload) => {
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
            messageType: 'Offer',
            offer: payload.offer,
          };
          return {
            ...old,
            data: { ...old.data, messages: [...old.data.messages, newMessage] },
          };
        },
      );
      void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    });

    const handleOfferStatusUpdate = (
      payload: OfferStatusUpdatedPayload,
    ) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: {
              ...old.data,
              messages: old.data.messages.map((m) =>
                m.offer?.id === payload.offerId
                  ? { ...m, offer: { ...m.offer!, status: payload.newStatus } }
                  : m,
              ),
            },
          };
        },
      );
    };

    connection.on(HUB_METHODS.OFFER_ACCEPTED, handleOfferStatusUpdate);
    connection.on(HUB_METHODS.OFFER_DECLINED, handleOfferStatusUpdate);

    connection.on(HUB_METHODS.OFFER_COUNTERED, (payload: OfferCounteredPayload) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          const updatedMessages = old.data.messages.map((m) =>
            m.offer?.id === payload.offerId
              ? { ...m, offer: { ...m.offer!, status: 'Countered' as const } }
              : m,
          );
          const counterMessage: Message = {
            id: payload.counterOffer.messageId,
            senderId: payload.counterOffer.senderId,
            senderUsername: payload.counterOffer.senderUsername,
            content: '',
            sentAt: payload.counterOffer.sentAt,
            isRead: false,
            messageType: 'Offer',
            offer: payload.counterOffer.offer,
          };
          return {
            ...old,
            data: { ...old.data, messages: [...updatedMessages, counterMessage] },
          };
        },
      );
      void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    });

    connection.on(HUB_METHODS.OFFER_EXPIRED, (payload: OfferExpiredPayload) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: {
              ...old.data,
              messages: old.data.messages.map((m) =>
                m.offer?.id === payload.offerId
                  ? { ...m, offer: { ...m.offer!, status: 'Expired' as const } }
                  : m,
              ),
            },
          };
        },
      );
    });

    connectionRef.current = connection;
    connection.start().catch(console.error);

    return () => {
      if (isOwner.current) {
        void connection.stop();
        connectionRef.current = null;
      }
    };
  }, [accessToken]);

  const sendMessage = useCallback(
    ({ conversationId, content }: { conversationId: string; content: string }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
        throw new Error('Not connected. Please wait and try again.');
      }
      void connectionRef.current.invoke(HUB_METHODS.SEND_MESSAGE, conversationId, content);
    },
    [],
  );

  const sendOffer = useCallback(
    ({ conversationId, amount }: { conversationId: string; amount: number }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
        throw new Error('Not connected. Please wait and try again.');
      }
      void connectionRef.current.invoke(HUB_METHODS.MAKE_OFFER, conversationId, amount);
    },
    [],
  );

  const respondToOffer = useCallback(
    ({
      offerId,
      action,
      counterAmount,
    }: {
      offerId: string;
      action: 'Accept' | 'Decline' | 'Counter';
      counterAmount?: number;
    }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
        throw new Error('Not connected. Please wait and try again.');
      }
      void connectionRef.current.invoke(
        HUB_METHODS.RESPOND_TO_OFFER,
        offerId,
        action,
        counterAmount ?? null,
      );
    },
    [],
  );

  return { sendMessage, sendOffer, respondToOffer };
};
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit 2>&1 | head -20
```

Expected: No errors in `useChatHub.ts`. Some downstream errors in components still using old `ActionBar` props are expected — they'll be fixed in later tasks.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/api/useChatHub.ts
git commit -m "feat: extend useChatHub with sendOffer, respondToOffer and 5 offer event handlers"
```

---

## Task 12: OfferCard component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/OfferCard.tsx`

- [ ] **Step 1: Create `OfferCard.tsx`**

```tsx
import { Button } from '@/components/ui/button';
import { BadgeCheck, BadgeX, Clock, HandCoins, Undo2 } from 'lucide-react';
import { useState } from 'react';
import type { Offer } from '../types/Offer';
import MakeOfferModal from './MakeOfferModal';

type OfferCardProps = {
  offer: Offer;
  currentUserId: string;
  listingPrice: number;
  onAccept: (offerId: string) => void;
  onDecline: (offerId: string) => void;
  onCounter: (offerId: string, amount: number) => void;
};

const statusConfig = {
  Pending: {
    headerClass: 'bg-slate-900',
    borderClass: 'border-border',
    label: 'Offer',
    icon: HandCoins,
    labelClass: 'text-slate-200',
    subLabel: 'Pending response',
    subLabelClass: 'text-slate-400',
    priceClass: 'text-foreground',
    badgeClass: 'bg-red-100 text-red-600 dark:bg-red-950 dark:text-red-400',
  },
  Accepted: {
    headerClass: 'bg-green-900',
    borderClass: 'border-green-300 dark:border-green-800',
    label: 'Offer Accepted',
    icon: BadgeCheck,
    labelClass: 'text-green-200',
    subLabel: 'Listing is now on hold',
    subLabelClass: 'text-green-400',
    priceClass: 'text-green-600 dark:text-green-400',
    badgeClass: 'bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-400',
  },
  Declined: {
    headerClass: 'bg-red-900',
    borderClass: 'border-red-300 dark:border-red-800',
    label: 'Offer Declined',
    icon: BadgeX,
    labelClass: 'text-red-200',
    subLabel: 'No deal reached',
    subLabelClass: 'text-red-400',
    priceClass: 'text-muted-foreground line-through',
    badgeClass: 'bg-muted text-muted-foreground',
  },
  Countered: {
    headerClass: 'bg-violet-900',
    borderClass: 'border-violet-300 dark:border-violet-800',
    label: 'Counter-Offer',
    icon: Undo2,
    labelClass: 'text-violet-200',
    subLabel: 'Awaiting response',
    subLabelClass: 'text-violet-400',
    priceClass: 'text-foreground',
    badgeClass: 'bg-violet-100 text-violet-700 dark:bg-violet-950 dark:text-violet-400',
  },
  Expired: {
    headerClass: 'bg-muted-foreground/60',
    borderClass: 'border-border',
    label: 'Offer Expired',
    icon: Clock,
    labelClass: 'text-muted',
    subLabel: 'No response within 48 hours',
    subLabelClass: 'text-muted-foreground',
    priceClass: 'text-muted-foreground line-through',
    badgeClass: 'bg-muted text-muted-foreground',
  },
} as const;

const OfferCard = ({
  offer,
  currentUserId,
  listingPrice,
  onAccept,
  onDecline,
  onCounter,
}: OfferCardProps) => {
  const [counterModalOpen, setCounterModalOpen] = useState(false);
  const config = statusConfig[offer.status];
  const Icon = config.icon;

  const canRespond = offer.status === 'Pending' && currentUserId !== offer.initiatorId;

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
          <div className="mb-1 flex items-baseline gap-2">
            <span className={`text-xl font-bold ${config.priceClass}`}>
              €{offer.amount.toLocaleString()}
            </span>
            {offer.status !== 'Declined' && offer.status !== 'Expired' && (
              <span className="text-muted-foreground text-xs line-through">
                €{listingPrice.toLocaleString()}
              </span>
            )}
            <span className={`rounded-full px-1.5 py-0.5 text-[10px] font-semibold ${config.badgeClass}`}>
              −{offer.percentageOff}%
            </span>
          </div>

          {canRespond && (
            <div className="mt-3 flex gap-2">
              <Button
                size="sm"
                className="h-7 flex-1 text-xs"
                onClick={() => onAccept(offer.id)}
              >
                Accept
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-7 flex-1 text-xs"
                onClick={() => setCounterModalOpen(true)}
              >
                Counter
              </Button>
              <Button
                size="sm"
                variant="ghost"
                className="text-destructive hover:text-destructive h-7 flex-1 text-xs"
                onClick={() => onDecline(offer.id)}
              >
                Decline
              </Button>
            </div>
          )}
        </div>
      </div>

      <MakeOfferModal
        open={counterModalOpen}
        onOpenChange={setCounterModalOpen}
        mode="counter"
        listingPrice={listingPrice}
        initialAmount={offer.amount}
        offerId={offer.id}
        onSubmit={(amount) => {
          onCounter(offer.id, amount);
          setCounterModalOpen(false);
        }}
      />
    </>
  );
};

export default OfferCard;
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/OfferCard.tsx
git commit -m "feat: add OfferCard component for offer cards in chat thread"
```

---

## Task 13: MakeOfferModal component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx`

- [ ] **Step 1: Create `MakeOfferModal.tsx`**

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

type MakeOfferModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: 'offer' | 'counter';
  listingPrice: number;
  conversationId?: string; // required when mode === 'offer'
  offerId?: string;        // required when mode === 'counter'
  initialAmount?: number;
  onSubmit: (amount: number) => void;
};

const MakeOfferModal = ({
  open,
  onOpenChange,
  mode,
  listingPrice,
  initialAmount,
  onSubmit,
}: MakeOfferModalProps) => {
  const [rawValue, setRawValue] = useState(
    initialAmount !== undefined ? String(Math.round(initialAmount)) : '',
  );

  const amount = parseFloat(rawValue);
  const minAmount = listingPrice / 3;
  const isValidNumber = !isNaN(amount) && amount > 0;
  const isTooLow = isValidNumber && amount < minAmount;
  const isTooHigh = isValidNumber && amount > listingPrice;
  const isValid = isValidNumber && !isTooLow && !isTooHigh;

  const percentageOff =
    isValidNumber && amount <= listingPrice
      ? Math.round(((listingPrice - amount) / listingPrice) * 100)
      : null;

  const handleSubmit = () => {
    if (!isValid) return;
    onSubmit(amount);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{mode === 'counter' ? 'Counter Offer' : 'Make an Offer'}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="bg-muted flex justify-between rounded-lg px-4 py-3 text-sm">
            <div>
              <p className="text-muted-foreground text-xs">Listed price</p>
              <p className="font-semibold">€{listingPrice.toLocaleString()}</p>
            </div>
            {percentageOff !== null && percentageOff > 0 && (
              <div className="text-right">
                <p className="text-muted-foreground text-xs">Discount</p>
                <p className="text-destructive font-semibold">−{percentageOff}%</p>
              </div>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="offer-amount">Your offer (€)</Label>
            <Input
              id="offer-amount"
              type="number"
              min={Math.ceil(minAmount)}
              max={listingPrice}
              value={rawValue}
              onChange={(e) => setRawValue(e.target.value)}
              placeholder={`${Math.ceil(minAmount)} – ${listingPrice}`}
            />
            {isTooLow && (
              <p className="text-destructive text-xs">
                Minimum offer is €{Math.ceil(minAmount).toLocaleString()} (⅓ of asking price).
              </p>
            )}
            {isTooHigh && (
              <p className="text-destructive text-xs">
                Offer cannot exceed the listing price of €{listingPrice.toLocaleString()}.
              </p>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} disabled={!isValid}>
              {mode === 'counter' ? 'Send Counter' : 'Send Offer'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default MakeOfferModal;
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MakeOfferModal.tsx
git commit -m "feat: add MakeOfferModal with live discount preview and validation"
```

---

## Task 14: ActionBar update

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx`

- [ ] **Step 1: Replace `ActionBar.tsx`**

```tsx
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import MakeOfferModal from './MakeOfferModal';

type ActionBarProps = {
  currentUserId: string;
  buyerId: string;
  sellerId: string;
  listingPrice: number;
  conversationId: string;
  buyerHasLiked: boolean;
  hasActiveOffer: boolean;
  onSendOffer: (amount: number) => void;
};

const ActionBar = ({
  currentUserId,
  buyerId,
  sellerId,
  listingPrice,
  conversationId,
  buyerHasLiked,
  hasActiveOffer,
  onSendOffer,
}: ActionBarProps) => {
  const [offerModalOpen, setOfferModalOpen] = useState(false);

  const isBuyer = currentUserId === buyerId;
  const isSeller = currentUserId === sellerId;
  const showOfferButton = isBuyer || (isSeller && buyerHasLiked);

  return (
    <>
      <div className="border-border flex items-center gap-2 border-b px-3 py-2">
        {showOfferButton && (
          <Button
            variant="outline"
            size="sm"
            disabled={hasActiveOffer}
            onClick={() => setOfferModalOpen(true)}
            title={hasActiveOffer ? 'An offer is already pending in this conversation' : undefined}
          >
            Make an Offer
          </Button>
        )}
      </div>

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
    </>
  );
};

export default ActionBar;
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ActionBar.tsx
git commit -m "feat: enable ActionBar Make an Offer button with offer modal"
```

---

## Task 15: MessageThread update + ListingDetailsContent fix

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Replace `MessageThread.tsx`**

```tsx
import { useAppSelector } from '@/hooks/redux';
import { useSuspenseQuery } from '@tanstack/react-query';
import { useEffect, useRef, useState } from 'react';
import { getMessagesOptions } from '../api/getMessagesOptions';
import { useChatHub } from '../api/useChatHub';
import { useMarkMessagesRead } from '../api/useMarkMessagesRead';
import type { ConversationSummary } from '../types/ConversationSummary';
import ActionBar from './ActionBar';
import ListingCard from './ListingCard';
import OfferCard from './OfferCard';
import { Button } from '@/components/ui/button';

type MessageThreadProps = {
  conversation: ConversationSummary;
  showListingCard?: boolean;
};

const MessageThread = ({
  conversation,
  showListingCard = true,
}: MessageThreadProps) => {
  const userId = useAppSelector((s) => s.auth.userId) ?? '';
  const { data: messagesQuery } = useSuspenseQuery(
    getMessagesOptions({ conversationId: conversation.id }),
  );
  const messages = messagesQuery.data.messages;
  const { sendMessage, sendOffer, respondToOffer } = useChatHub();
  const { mutate: markRead } = useMarkMessagesRead();
  const [input, setInput] = useState('');
  const [sendError, setSendError] = useState<string | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages.length]);

  useEffect(() => {
    markRead(conversation.id);
  }, [messages.length, conversation.id, markRead]);

  const hasActiveOffer = messages.some(
    (m) => m.messageType === 'Offer' && m.offer?.status === 'Pending',
  );

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    try {
      sendMessage({ conversationId: conversation.id, content: trimmed });
      setInput('');
      setSendError(null);
    } catch (err) {
      setSendError(err instanceof Error ? err.message : 'Failed to send message.');
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
      <ActionBar
        currentUserId={userId}
        buyerId={conversation.buyerId}
        sellerId={conversation.sellerId}
        listingPrice={conversation.listingPrice}
        conversationId={conversation.id}
        buyerHasLiked={conversation.buyerHasLiked}
        hasActiveOffer={hasActiveOffer}
        onSendOffer={(amount) => sendOffer({ conversationId: conversation.id, amount })}
      />
      <div className="flex-1 space-y-2 overflow-y-auto p-4">
        {messages.map((m) => {
          if (m.messageType === 'Offer' && m.offer) {
            const isOwn = m.senderId === userId;
            return (
              <div
                key={m.id}
                className={`flex ${isOwn ? 'justify-end' : 'justify-start'}`}
              >
                <OfferCard
                  offer={m.offer}
                  currentUserId={userId}
                  listingPrice={conversation.listingPrice}
                  onAccept={(offerId) =>
                    respondToOffer({ offerId, action: 'Accept' })
                  }
                  onDecline={(offerId) =>
                    respondToOffer({ offerId, action: 'Decline' })
                  }
                  onCounter={(offerId, amount) =>
                    respondToOffer({ offerId, action: 'Counter', counterAmount: amount })
                  }
                />
              </div>
            );
          }

          const isOwn = m.senderId === userId;
          return (
            <div
              key={m.id}
              className={`flex ${isOwn ? 'justify-end' : 'justify-start'}`}
            >
              <div
                className={`max-w-[75%] rounded-2xl px-3 py-2 text-sm break-words ${
                  isOwn
                    ? 'bg-primary text-primary-foreground rounded-br-sm'
                    : 'bg-muted rounded-bl-sm'
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
        <input
          className="border-input bg-background focus:ring-ring flex-1 rounded-full border px-4 py-2 text-sm focus:ring-2 focus:outline-none"
          placeholder={`Message ${conversation.counterpartUsername}...`}
          value={input}
          onChange={(e) => {
            setInput(e.target.value);
            setSendError(null);
          }}
          onKeyDown={(e) => e.key === 'Enter' && handleSend()}
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

- [ ] **Step 2: Update `ListingDetailsContent.tsx` — add `buyerId`, `sellerId`, `buyerHasLiked` to chatConversation**

Find the `setChatConversation({...})` call (around line 40-50) and add the three new fields:

```typescript
setChatConversation({
  id: res.data.conversationId,
  listingId: id,
  listingTitle: `${listing.year} ${listing.makeName} ${listing.modelName}`,
  listingThumbnail: listing.images[0] ?? null,
  listingPrice: listing.price,
  counterpartId: listing.sellerId,
  counterpartUsername: listing.sellerName,
  lastMessage: null,
  lastMessageAt: new Date().toISOString(),
  unreadCount: 0,
  buyerId: userId ?? '',       // current user is always the buyer in this context
  sellerId: listing.sellerId,
  buyerHasLiked: false,        // buyer's offer button doesn't depend on this field
});
```

- [ ] **Step 3: Verify TypeScript compiles clean**

```bash
cd automotive.marketplace.client && npx tsc --noEmit 2>&1 | head -20
```

Expected: 0 errors.

- [ ] **Step 4: Run frontend lint**

```bash
cd automotive.marketplace.client && npm run lint 2>&1 | tail -10
```

Expected: No errors.

- [ ] **Step 5: Build frontend**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```

Expected: Build succeeded.

- [ ] **Step 6: Run the full backend test suite one final time**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj -v n 2>&1 | tail -15
```

Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MessageThread.tsx \
        automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: wire OfferCard and ActionBar into MessageThread; fix ListingDetailsContent conversation shape"
```

---

## Done

The offer negotiation feature is fully implemented. Verify end-to-end by:

1. Starting the app (`docker compose up -d`)
2. Opening two browser sessions (buyer and seller accounts)
3. Buyer opens a chat from a listing → "Make an Offer" button is active → submit an offer
4. Seller sees the offer card in inbox with Accept / Counter / Decline buttons
5. Accepting sets the listing to OnHold; declining allows a new offer; countering creates a new card
6. Seller can initiate from inbox only if the buyer has liked the listing (button is hidden otherwise)
