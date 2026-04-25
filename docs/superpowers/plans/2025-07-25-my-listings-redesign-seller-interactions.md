# My Listings Redesign & Seller Interaction Dashboard — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign `MyListingCard` to match the search result card layout and add a lazy-loaded buyer engagement panel showing who liked/messaged each listing, with a slide-over ChatPanel for quick replies.

**Architecture:** Backend extends `GetMyListings` response with `Images[]`, `LikeCount`, `ConversationCount`, and adds a new `GetListingEngagements` CQRS query (controller action `/Listing/GetEngagements?id=...`). Frontend rewrites `MyListingCard` to use `ImageHoverGallery` + spec badges + seller controls, adds a new `ListingBuyerPanel` component with Tabs (Conversations/Likes), and wires a `ChatPanel` slide-over at the `MyListingsPage` level.

**Tech Stack:** .NET 8 / MediatR / EF Core / AutoMapper (backend); React 19 / TanStack Query v5 / TanStack Router / shadcn/ui / Lucide icons / react-i18next (frontend)

---

## Task 1: Extend Listing entity + GetMyListings with images and counts

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/Listing.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/ListingMappings.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs`

- [ ] **Step 1: Add nav props to Listing entity**

In `Automotive.Marketplace.Domain/Entities/Listing.cs`, add two inverse navigation properties at the bottom of the class (before the closing brace). These expose the existing FK relationships as collections — no schema migration is required since the FK columns already exist in `UserListingLike.ListingId` and `Conversation.ListingId`.

```csharp
// Automotive.Marketplace.Domain/Entities/Listing.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Domain.Entities;

public class Listing : BaseEntity
{
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public Status Status { get; set; }
    public string? Description { get; set; }
    public string? Vin { get; set; }
    public string? Colour { get; set; }
    public bool IsUsed { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public bool IsSteeringWheelRight { get; set; }
    public Guid DrivetrainId { get; set; }
    public virtual Drivetrain Drivetrain { get; set; } = null!;
    public Guid VariantId { get; set; }
    public virtual Variant Variant { get; set; } = null!;
    public Guid SellerId { get; set; }
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<Image> Images { get; set; } = [];
    public virtual ICollection<ListingDefect> Defects { get; set; } = [];
    public virtual ICollection<User> LikeUsers { get; set; } = [];
    public virtual ICollection<UserListingLike> Likes { get; set; } = [];       // NEW
    public virtual ICollection<Conversation> Conversations { get; set; } = [];  // NEW
}
```

- [ ] **Step 2: Add three new fields to GetMyListingsResponse**

```csharp
// Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs
using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public sealed record GetMyListingsResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public bool IsUsed { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Year { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public ImageDto? Thumbnail { get; set; }
    public IEnumerable<ImageDto> Images { get; set; } = [];  // NEW
    public int ImageCount { get; set; }
    public int DefectCount { get; set; }
    public string FuelName { get; set; } = string.Empty;
    public string TransmissionName { get; set; } = string.Empty;
    public int EngineSizeMl { get; set; }
    public int PowerKw { get; set; }
    public int LikeCount { get; set; }          // NEW
    public int ConversationCount { get; set; }  // NEW
}
```

- [ ] **Step 3: Update AutoMapper mapping to ignore the three new manually-set fields**

In `Automotive.Marketplace.Application/Mappings/ListingMappings.cs`, find the `CreateMap<Listing, GetMyListingsResponse>()` chain and add three `.ForMember(...Ignore())` calls:

```csharp
// In ListingMappings.cs, update the GetMyListingsResponse mapping block:
CreateMap<Listing, GetMyListingsResponse>()
    .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.PowerKw : 0))
    .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.Thumbnail, opt => opt.Ignore())
    .ForMember(dest => dest.ImageCount, opt => opt.Ignore())
    .ForMember(dest => dest.DefectCount, opt => opt.Ignore())
    .ForMember(dest => dest.Images, opt => opt.Ignore())          // NEW
    .ForMember(dest => dest.LikeCount, opt => opt.Ignore())       // NEW
    .ForMember(dest => dest.ConversationCount, opt => opt.Ignore()); // NEW
```

- [ ] **Step 4: Update GetMyListingsQueryHandler to include nav props and populate new fields**

Replace the entire handler file:

```csharp
// Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs
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
            .Include(l => l.Likes)          // NEW
            .Include(l => l.Conversations)  // NEW
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        List<GetMyListingsResponse> response = [];
        foreach (var listing in listings)
        {
            var mappedListing = mapper.Map<GetMyListingsResponse>(listing);

            var nonDefectImages = listing.Images.Where(i => i.ListingDefectId == null).ToList();

            // Thumbnail
            var firstImage = nonDefectImages.FirstOrDefault();
            if (firstImage != null)
            {
                mappedListing.Thumbnail = new ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey),
                    AltText = firstImage.AltText
                };
            }

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

            // Counts
            mappedListing.ImageCount = listing.Images.Count;
            mappedListing.DefectCount = listing.Defects.Count;
            mappedListing.LikeCount = listing.Likes.Count;           // NEW
            mappedListing.ConversationCount = listing.Conversations.Count; // NEW

            response.Add(mappedListing);
        }

        return response;
    }
}
```

- [ ] **Step 5: Build and verify no errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/Listing.cs \
        Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs \
        Automotive.Marketplace.Application/Mappings/ListingMappings.cs \
        Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs
git commit -m "feat: extend GetMyListings response with images, like count, and conversation count

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: Create GetListingEngagements CQRS feature + controller endpoint

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Create GetListingEngagementsQuery**

```csharp
// Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public sealed record GetListingEngagementsQuery : IRequest<GetListingEngagementsResponse>
{
    public Guid ListingId { get; set; }
    public Guid CurrentUserId { get; set; }
}
```

- [ ] **Step 2: Create GetListingEngagementsResponse with nested types**

`Conversation` and `Liker` are only used in this response, so they are nested inside it (per the `be-query-response-classes` skill pattern).

```csharp
// Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsResponse.cs
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public sealed record GetListingEngagementsResponse
{
    public IEnumerable<Conversation> Conversations { get; set; } = [];
    public IEnumerable<Liker> Likers { get; set; } = [];

    public sealed record Conversation
    {
        public Guid ConversationId { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerUsername { get; set; } = string.Empty;
        public string LastMessageType { get; set; } = string.Empty;
        public DateTime LastInteractionAt { get; set; }
    }

    public sealed record Liker
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime LikedAt { get; set; }
    }
}
```

- [ ] **Step 3: Create GetListingEngagementsQueryHandler**

The handler:
1. Loads the listing to verify existence and check `SellerId == CurrentUserId`
2. Loads all conversations for the listing (with last message type + buyer info)
3. Loads "pure likers" — users in `UserListingLike` who have **no** conversation for this listing

```csharp
// Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/GetListingEngagementsQueryHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public class GetListingEngagementsQueryHandler(IRepository repository)
    : IRequestHandler<GetListingEngagementsQuery, GetListingEngagementsResponse>
{
    public async Task<GetListingEngagementsResponse> Handle(
        GetListingEngagementsQuery request,
        CancellationToken cancellationToken)
    {
        var listing = await repository
            .AsQueryable<Listing>()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), request.ListingId);

        if (listing.SellerId != request.CurrentUserId)
            throw new UnauthorizedAccessException("You are not the seller of this listing.");

        var conversations = await repository
            .AsQueryable<Conversation>()
            .Include(c => c.Messages)
            .Include(c => c.Buyer)
            .Where(c => c.ListingId == request.ListingId)
            .ToListAsync(cancellationToken);

        var buyerIds = conversations.Select(c => c.BuyerId).ToHashSet();

        var likers = await repository
            .AsQueryable<UserListingLike>()
            .Include(l => l.User)
            .Where(l => l.ListingId == request.ListingId && !buyerIds.Contains(l.UserId))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var conversationDtos = conversations
            .Select(c =>
            {
                var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                return new GetListingEngagementsResponse.Conversation
                {
                    ConversationId = c.Id,
                    BuyerId = c.BuyerId,
                    BuyerUsername = c.Buyer.Username,
                    LastMessageType = lastMsg?.MessageType.ToString() ?? "Text",
                    LastInteractionAt = c.LastMessageAt,
                };
            })
            .OrderByDescending(c => c.LastInteractionAt)
            .ToList();

        var likerDtos = likers
            .Select(l => new GetListingEngagementsResponse.Liker
            {
                UserId = l.UserId,
                Username = l.User.Username,
                LikedAt = l.CreatedAt,
            })
            .ToList();

        return new GetListingEngagementsResponse
        {
            Conversations = conversationDtos,
            Likers = likerDtos,
        };
    }
}
```

- [ ] **Step 4: Add GetEngagements action to ListingController**

The controller follows `[Route("[controller]/[action]")]` convention (from `BaseController`), so this becomes `/Listing/GetEngagements`. Add at the bottom of `ListingController.cs`:

```csharp
// Add this using at the top of ListingController.cs:
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

// Add this action inside the ListingController class:
[HttpGet]
[Protect(Permission.ManageListings)]
public async Task<ActionResult<GetListingEngagementsResponse>> GetEngagements(
    [FromQuery] Guid id,
    CancellationToken cancellationToken)
{
    var query = new GetListingEngagementsQuery { ListingId = id, CurrentUserId = UserId };
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

- [ ] **Step 5: Build and verify no errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingEngagements/ \
        Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat: add GetListingEngagements CQRS handler and controller endpoint

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: Backend integration tests for GetListingEngagementsQueryHandler

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs`

- [ ] **Step 1: Create the test class with seeding helpers**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingEngagementsQueryHandlerTests(
    DatabaseFixture<GetListingEngagementsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingEngagementsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingEngagementsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingEngagementsQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingEngagementsQueryHandler(repository);
    }

    private async Task<(Guid listingId, Guid sellerId)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();
        return (listing.Id, seller.Id);
    }

    [Fact]
    public async Task Handle_ListingDoesNotExist_ShouldThrowDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var query = new GetListingEngagementsQuery
        {
            ListingId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
        };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_CurrentUserIsNotSeller_ShouldThrowUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, _) = await SeedListingAsync(context);

        var query = new GetListingEngagementsQuery
        {
            ListingId = listingId,
            CurrentUserId = Guid.NewGuid(), // wrong user
        };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NoEngagements_ShouldReturnEmptyCollections()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().BeEmpty();
        result.Likers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithConversation_ShouldReturnConversationWithCorrectLastMessageType()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var buyer = new UserBuilder().Build();
        var conversation = new ConversationBuilder().WithListing(listingId).WithBuyer(buyer.Id).Build();
        var textMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Text)
            .With(m => m.SentAt, DateTime.UtcNow.AddMinutes(-10))
            .Build();
        var offerMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Offer)
            .With(m => m.SentAt, DateTime.UtcNow)
            .Build();

        await context.AddRangeAsync(buyer, conversation, textMessage, offerMessage);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().HaveCount(1);
        var c = result.Conversations.First();
        c.BuyerId.Should().Be(buyer.Id);
        c.BuyerUsername.Should().Be(buyer.Username);
        c.LastMessageType.Should().Be("Offer"); // most recent message wins
    }

    [Fact]
    public async Task Handle_WithLikerWhoHasNoConversation_ShouldAppearInLikers()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var liker = new UserBuilder().Build();
        var like = new UserListingLikeBuilder().WithListing(listingId).WithUser(liker.Id).Build();

        await context.AddRangeAsync(liker, like);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Likers.Should().HaveCount(1);
        result.Likers.First().UserId.Should().Be(liker.Id);
        result.Likers.First().Username.Should().Be(liker.Username);
        result.Conversations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWhoLikedAndMessaged_ShouldAppearOnlyInConversations()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var buyer = new UserBuilder().Build();
        var conversation = new ConversationBuilder().WithListing(listingId).WithBuyer(buyer.Id).Build();
        var message = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Text)
            .Build();
        var like = new UserListingLikeBuilder().WithListing(listingId).WithUser(buyer.Id).Build();

        await context.AddRangeAsync(buyer, conversation, message, like);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().HaveCount(1);
        result.Likers.Should().BeEmpty(); // buyer is excluded from likers because they have a conversation
    }
}
```

- [ ] **Step 2: Run the tests**

```bash
dotnet test --filter "FullyQualifiedName~GetListingEngagementsQueryHandlerTests" ./Automotive.Marketplace.sln
```

Expected: All 5 tests pass.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingEngagementsQueryHandlerTests.cs
git commit -m "test: add integration tests for GetListingEngagementsQueryHandler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: Frontend types, endpoints, query options, and i18n

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/types/GetMyListingsResponse.ts`
- Create: `automotive.marketplace.client/src/features/myListings/types/GetListingEngagementsResponse.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/myListingKeys.ts`
- Create: `automotive.marketplace.client/src/features/myListings/api/getListingEngagementsOptions.ts`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`

- [ ] **Step 1: Update GetMyListingsResponse TypeScript type**

```typescript
// automotive.marketplace.client/src/features/myListings/types/GetMyListingsResponse.ts
type Image = {
  url: string;
  altText: string;
};

export type GetMyListingsResponse = {
  id: string;
  price: number;
  mileage: number;
  isUsed: boolean;
  city: string;
  status: string;
  year: number;
  makeName: string;
  modelName: string;
  thumbnail: Image | null;
  images: Image[];       // NEW
  imageCount: number;
  defectCount: number;
  fuelName: string;
  transmissionName: string;
  engineSizeMl: number;
  powerKw: number;
  likeCount: number;          // NEW
  conversationCount: number;  // NEW
};
```

- [ ] **Step 2: Create GetListingEngagementsResponse TypeScript types**

```typescript
// automotive.marketplace.client/src/features/myListings/types/GetListingEngagementsResponse.ts
export type ListingConversationEngagement = {
  conversationId: string;
  buyerId: string;
  buyerUsername: string;
  lastMessageType: "Text" | "Offer" | "Meeting" | "Availability";
  lastInteractionAt: string;
};

export type ListingLikerEngagement = {
  userId: string;
  username: string;
  likedAt: string;
};

export type GetListingEngagementsResponse = {
  conversations: ListingConversationEngagement[];
  likers: ListingLikerEngagement[];
};
```

- [ ] **Step 3: Add GET_ENGAGEMENTS to endpoints constant**

In `automotive.marketplace.client/src/constants/endpoints.ts`, add `GET_ENGAGEMENTS` to the `LISTING` block:

```typescript
LISTING: {
  GET_BY_ID: "/Listing/GetById",
  GET_ALL: "/Listing/GetAll",
  GET_MY: "/Listing/GetMy",
  GET_ENGAGEMENTS: "/Listing/GetEngagements",  // NEW
  CREATE: "/Listing/Create",
  DELETE: "/Listing/Delete",
  UPDATE: "/Listing/Update",
  SEARCH: "/Listing/Search",
  COMPARE: "/Listing/Compare",
},
```

- [ ] **Step 4: Add engagements key to myListingKeys**

```typescript
// automotive.marketplace.client/src/api/queryKeys/myListingKeys.ts
export const myListingKeys = {
  all: () => ["myListings"] as const,
  engagements: (listingId: string) =>
    ["myListings", "engagements", listingId] as const,
};
```

- [ ] **Step 5: Create getListingEngagementsOptions**

```typescript
// automotive.marketplace.client/src/features/myListings/api/getListingEngagementsOptions.ts
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetListingEngagementsResponse } from "../types/GetListingEngagementsResponse";

const getListingEngagements = (listingId: string) =>
  axiosClient.get<GetListingEngagementsResponse>(ENDPOINTS.LISTING.GET_ENGAGEMENTS, {
    params: { id: listingId },
  });

export const getListingEngagementsOptions = (listingId: string) =>
  queryOptions({
    queryKey: myListingKeys.engagements(listingId),
    queryFn: () => getListingEngagements(listingId),
  });
```

- [ ] **Step 6: Add buyerPanel keys to en/myListings.json**

Add the `"buyerPanel"` key at the end of the existing JSON object (before the closing `}`):

```json
{
  "page": { "...existing..." },
  "card": { "...existing..." },
  "detail": { "...existing..." },
  "fields": { "...existing..." },
  "vehicleInfo": { "...existing..." },
  "buyerPanel": {
    "buyers": "Buyers",
    "conversations": "Conversations",
    "likes": "Likes",
    "moreConversations": "more conversations",
    "moreLikes": "more likes",
    "chat": "Chat",
    "interactionTypes": {
      "offer": "Price offer",
      "meeting": "Meeting proposal",
      "availability": "Availability",
      "message": "Message",
      "liked": "Liked"
    }
  }
}
```

- [ ] **Step 7: Add buyerPanel keys to lt/myListings.json**

```json
{
  "...existing keys...",
  "buyerPanel": {
    "buyers": "Pirkėjai",
    "conversations": "Pokalbiai",
    "likes": "Patiktukai",
    "moreConversations": "daugiau pokalbių",
    "moreLikes": "daugiau patiktukų",
    "chat": "Rašyti",
    "interactionTypes": {
      "offer": "Kainos pasiūlymas",
      "meeting": "Susitikimo pasiūlymas",
      "availability": "Laisvas laikas",
      "message": "Žinutė",
      "liked": "Patiko"
    }
  }
}
```

- [ ] **Step 8: Install Tabs shadcn component (needed for Task 5)**

```bash
cd automotive.marketplace.client
npx shadcn@latest add tabs
```

Expected: Creates `src/components/ui/tabs.tsx`.

- [ ] **Step 9: Build and verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build
```

Expected: Build succeeds with no errors.

- [ ] **Step 10: Commit**

```bash
git add \
  src/features/myListings/types/GetMyListingsResponse.ts \
  src/features/myListings/types/GetListingEngagementsResponse.ts \
  src/features/myListings/api/getListingEngagementsOptions.ts \
  src/constants/endpoints.ts \
  src/api/queryKeys/myListingKeys.ts \
  src/lib/i18n/locales/en/myListings.json \
  src/lib/i18n/locales/lt/myListings.json \
  src/components/ui/tabs.tsx
git commit -m "feat: add engagement types, query options, i18n keys, and Tabs component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: Create ListingBuyerPanel component

**Files:**
- Create: `automotive.marketplace.client/src/features/myListings/components/ListingBuyerPanel.tsx`

- [ ] **Step 1: Create the component**

```typescript
// automotive.marketplace.client/src/features/myListings/components/ListingBuyerPanel.tsx
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Link } from "@tanstack/react-router";
import {
  Calendar,
  Clock,
  DollarSign,
  Heart,
  MessageSquare,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useGetOrCreateConversation } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";

import { getListingEngagementsOptions } from "../api/getListingEngagementsOptions";
import type {
  ListingConversationEngagement,
  ListingLikerEngagement,
} from "../types/GetListingEngagementsResponse";

type ListingBuyerPanelProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingThumbnail: { url: string; altText: string } | null;
  sellerId: string;
  onStartChat: (conversation: ConversationSummary) => void;
};

/** Deterministic color from a UUID string for avatar backgrounds */
function stringToColor(str: string): string {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  const h = Math.abs(hash) % 360;
  return `hsl(${h}, 60%, 45%)`;
}

function timeAgo(dateStr: string): string {
  const diffMs = Date.now() - new Date(dateStr).getTime();
  const diffHours = Math.floor(diffMs / 3_600_000);
  if (diffHours < 1) return "< 1h";
  if (diffHours < 24) return `${diffHours}h`;
  return `${Math.floor(diffHours / 24)}d`;
}

function InteractionBadge({ type }: { type: string }) {
  const { t } = useTranslation("myListings");
  const config: Record<string, { icon: React.ReactNode; className: string; label: string }> = {
    Offer: {
      icon: <DollarSign className="h-3 w-3" />,
      className: "bg-red-100 text-red-700",
      label: t("buyerPanel.interactionTypes.offer"),
    },
    Meeting: {
      icon: <Calendar className="h-3 w-3" />,
      className: "bg-green-100 text-green-700",
      label: t("buyerPanel.interactionTypes.meeting"),
    },
    Availability: {
      icon: <Clock className="h-3 w-3" />,
      className: "bg-blue-100 text-blue-700",
      label: t("buyerPanel.interactionTypes.availability"),
    },
    Text: {
      icon: <MessageSquare className="h-3 w-3" />,
      className: "bg-gray-100 text-gray-700",
      label: t("buyerPanel.interactionTypes.message"),
    },
  };
  const { icon, className, label } = config[type] ?? config.Text;
  return (
    <span className={`flex items-center gap-1 rounded px-2 py-0.5 text-xs ${className}`}>
      {icon}
      {label}
    </span>
  );
}

function AvatarInitial({ id, name }: { id: string; name: string }) {
  return (
    <div
      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-medium text-white"
      style={{ backgroundColor: stringToColor(id) }}
    >
      {name.slice(0, 1).toUpperCase()}
    </div>
  );
}

export default function ListingBuyerPanel({
  listingId,
  listingTitle,
  listingPrice,
  listingThumbnail,
  sellerId,
  onStartChat,
}: ListingBuyerPanelProps) {
  const { t } = useTranslation("myListings");
  const { mutateAsync: getOrCreate } = useGetOrCreateConversation();
  const engagementsQuery = useQuery(getListingEngagementsOptions(listingId));

  const conversations = engagementsQuery.data?.data.conversations ?? [];
  const likers = engagementsQuery.data?.data.likers ?? [];

  const handleOpenConversation = (engagement: ListingConversationEngagement) => {
    onStartChat({
      id: engagement.conversationId,
      listingId,
      listingTitle,
      listingThumbnail,
      listingPrice,
      counterpartId: engagement.buyerId,
      counterpartUsername: engagement.buyerUsername,
      lastMessage: null,
      lastMessageAt: engagement.lastInteractionAt,
      unreadCount: 0,
      buyerId: engagement.buyerId,
      sellerId,
      buyerHasEngaged: true,
    });
  };

  const handleOpenLikerChat = async (liker: ListingLikerEngagement) => {
    const res = await getOrCreate({ listingId });
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
  };

  if (engagementsQuery.isPending) {
    return (
      <div className="border-border border-t px-4 py-4">
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </div>
    );
  }

  const shownConversations = conversations.slice(0, 5);
  const extraConversations = Math.max(0, conversations.length - 5);
  const shownLikers = likers.slice(0, 5);
  const extraLikers = Math.max(0, likers.length - 5);

  return (
    <div className="border-border border-t px-4 py-4">
      <Tabs defaultValue="conversations">
        <TabsList className="mb-3">
          <TabsTrigger value="conversations">
            {t("buyerPanel.conversations")} ({conversations.length})
          </TabsTrigger>
          <TabsTrigger value="likes">
            {t("buyerPanel.likes")} ({likers.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="conversations">
          {shownConversations.length === 0 ? (
            <p className="text-muted-foreground py-2 text-sm">
              {t("buyerPanel.conversations")} (0)
            </p>
          ) : (
            <div className="space-y-1">
              {shownConversations.map((c) => (
                <div key={c.conversationId} className="flex items-center gap-3 py-2">
                  <AvatarInitial id={c.buyerId} name={c.buyerUsername} />
                  <span className="flex-1 truncate text-sm font-medium">{c.buyerUsername}</span>
                  <InteractionBadge type={c.lastMessageType} />
                  <span className="text-muted-foreground shrink-0 text-xs">{timeAgo(c.lastInteractionAt)}</span>
                  <Button size="sm" variant="outline" onClick={() => handleOpenConversation(c)}>
                    {t("buyerPanel.chat")}
                  </Button>
                </div>
              ))}
              {extraConversations > 0 && (
                <Link to="/inbox" className="text-primary block pt-1 text-sm hover:underline">
                  + {extraConversations} {t("buyerPanel.moreConversations")} →
                </Link>
              )}
            </div>
          )}
        </TabsContent>

        <TabsContent value="likes">
          {shownLikers.length === 0 ? (
            <p className="text-muted-foreground py-2 text-sm">
              {t("buyerPanel.likes")} (0)
            </p>
          ) : (
            <div className="space-y-1">
              {shownLikers.map((l) => (
                <div key={l.userId} className="flex items-center gap-3 py-2">
                  <AvatarInitial id={l.userId} name={l.username} />
                  <span className="flex-1 truncate text-sm font-medium">{l.username}</span>
                  <span className="flex items-center gap-1 rounded bg-amber-100 px-2 py-0.5 text-xs text-amber-700">
                    <Heart className="h-3 w-3" />
                    {t("buyerPanel.interactionTypes.liked")}
                  </span>
                  <span className="text-muted-foreground shrink-0 text-xs">{timeAgo(l.likedAt)}</span>
                  <Button size="sm" variant="outline" onClick={() => handleOpenLikerChat(l)}>
                    {t("buyerPanel.chat")}
                  </Button>
                </div>
              ))}
              {extraLikers > 0 && (
                <Link to="/inbox" className="text-primary block pt-1 text-sm hover:underline">
                  + {extraLikers} {t("buyerPanel.moreLikes")} →
                </Link>
              )}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
```

- [ ] **Step 2: Build and verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add src/features/myListings/components/ListingBuyerPanel.tsx
git commit -m "feat: add ListingBuyerPanel component with conversations and likes tabs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: Rewrite MyListingCard and update MyListingsPage

**Files:**
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx` (full rewrite)
- Modify: `automotive.marketplace.client/src/features/myListings/components/MyListingsPage.tsx`

- [ ] **Step 1: Rewrite MyListingCard**

The new card mirrors `ListingCard` from `src/features/listingList/components/ListingCard.tsx` (2-column grid layout) and adds seller controls and a buyer engagement toggle. Import `ListingCardBadge` from `@/features/listingList/components/ListingCardBadge`.

```typescript
// automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx
import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import {
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  Heart,
  MessageSquare,
  Pencil,
  Trash2,
} from "lucide-react";
import { IoLocationOutline } from "react-icons/io5";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { PiEngine } from "react-icons/pi";
import { TbManualGearbox } from "react-icons/tb";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import ImageHoverGallery from "@/components/gallery/ImageHoverGallery";
import ListingCardBadge from "@/features/listingList/components/ListingCardBadge";
import type { ConversationSummary } from "@/features/chat";
import { useAppSelector } from "@/hooks/redux";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";

import { GetMyListingsResponse } from "../types/GetMyListingsResponse";
import { useDeleteMyListing } from "../api/useDeleteMyListing";
import ListingBuyerPanel from "./ListingBuyerPanel";

type MyListingCardProps = {
  listing: GetMyListingsResponse;
  onStartChat: (conversation: ConversationSummary) => void;
};

function StatusBadge({ status }: { status: string }) {
  const { t } = useTranslation("myListings");
  const isSold = status === "Sold";
  const isActive = status === "Active" || status === "Approved";
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
      {isSold ? t("card.sold") : isActive ? t("card.active") : status}
    </span>
  );
}

export default function MyListingCard({
  listing,
  onStartChat,
}: MyListingCardProps) {
  const { t } = useTranslation("myListings");
  const { userId } = useAppSelector((state) => state.auth);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [panelOpen, setPanelOpen] = useState(false);
  const deleteListingMutation = useDeleteMyListing();

  const isSold = listing.status === "Sold";

  const handleDelete = () => {
    deleteListingMutation.mutate(
      { id: listing.id },
      { onSuccess: () => setDeleteDialogOpen(false) },
    );
  };

  return (
    <div className={isSold ? "opacity-50" : ""}>
      <div className="bg-card border-border grid w-full gap-8 border-1 md:grid-cols-2">
        {/* Left — Image gallery */}
        <div className="relative flex flex-shrink-0 py-5">
          <ImageHoverGallery
            images={listing.images}
            className="aspect-[4/3] w-full"
          />
          {/* Status badge — top-left overlay */}
          <div className="absolute top-7 left-2">
            <StatusBadge status={listing.status} />
          </div>
          {/* Image count — top-right overlay */}
          {listing.imageCount > 1 && (
            <div className="absolute top-7 right-2 rounded bg-black/60 px-1.5 py-0.5 text-xs text-white">
              {listing.imageCount}
            </div>
          )}
        </div>

        {/* Right — Details */}
        <div className="flex min-w-0 flex-grow flex-col justify-between gap-3 pt-4 pr-4 pb-2">
          <div className="truncate">
            <p className="truncate font-sans text-xs">
              {listing.isUsed ? t("fields.used") : t("fields.new")}
            </p>
            <p className="font-sans text-xl">
              {listing.year} {listing.makeName} {listing.modelName}
            </p>
            <p className="font-sans text-xs">
              {formatNumber(listing.mileage)} km
            </p>
            <p className="font-sans text-3xl font-bold">
              {formatCurrency(listing.price)} €
            </p>
          </div>

          {/* Spec badges */}
          <div className="grid grid-cols-2 gap-x-0 gap-y-4">
            <div className="flex justify-self-start">
              <ListingCardBadge
                Icon={<PiEngine className="h-8 w-8" />}
                title={t("vehicleInfo.engineSizeMl").replace(":", "")}
                stat={`${listing.engineSizeMl / 1000} l ${listing.powerKw} kW`}
              />
            </div>
            <div className="flex justify-self-end">
              <ListingCardBadge
                Icon={<MdOutlineLocalGasStation className="h-8 w-8" />}
                title={t("vehicleInfo.fuel").replace(":", "")}
                stat={listing.fuelName}
              />
            </div>
            <div className="flex justify-self-start">
              <ListingCardBadge
                Icon={<TbManualGearbox className="h-8 w-8" />}
                title={t("vehicleInfo.transmission").replace(":", "")}
                stat={listing.transmissionName}
              />
            </div>
            <div className="flex justify-self-end">
              <ListingCardBadge
                Icon={<IoLocationOutline className="h-8 w-8" />}
                title={t("vehicleInfo.make").replace(":", "")}
                stat={listing.city}
              />
            </div>
          </div>

          {/* Bottom row — defect badge, spacer, buyer panel toggle, edit/delete */}
          <div className="flex flex-wrap items-center gap-2">
            {listing.defectCount > 0 && (
              <Badge
                variant="outline"
                className="border-amber-200 bg-amber-50 text-amber-700"
              >
                <AlertTriangle className="mr-1 h-3 w-3" />
                {t("card.defects", { count: listing.defectCount })}
              </Badge>
            )}

            <div className="flex-1" />

            {/* Buyer panel toggle — only when not sold */}
            {!isSold && (
              <button
                type="button"
                onClick={() => setPanelOpen((o) => !o)}
                className="text-muted-foreground hover:text-foreground flex items-center gap-1 text-sm"
              >
                <Heart className="h-4 w-4" />
                <span>{listing.likeCount}</span>
                <MessageSquare className="ml-1 h-4 w-4" />
                <span>{listing.conversationCount}</span>
                <span className="ml-1">{t("buyerPanel.buyers")}</span>
                {panelOpen ? (
                  <ChevronUp className="h-4 w-4" />
                ) : (
                  <ChevronDown className="h-4 w-4" />
                )}
              </button>
            )}

            {/* Edit / Delete — only when not sold */}
            {!isSold && (
              <>
                <Button asChild size="sm" variant="outline">
                  <Link to="/my-listings/$id" params={{ id: listing.id }}>
                    <Pencil className="mr-1 h-4 w-4" />
                    {t("card.edit")}
                  </Link>
                </Button>

                <AlertDialog
                  open={deleteDialogOpen}
                  onOpenChange={setDeleteDialogOpen}
                >
                  <AlertDialogTrigger asChild>
                    <Button
                      size="sm"
                      variant="outline"
                      className="text-red-600 hover:text-red-700"
                    >
                      <Trash2 className="mr-1 h-4 w-4" />
                      {t("card.delete")}
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>
                        {t("detail.deleteConfirmTitle")}
                      </AlertDialogTitle>
                      <AlertDialogDescription>
                        {t("detail.deleteConfirmDescription")}
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>{t("detail.cancel")}</AlertDialogCancel>
                      <AlertDialogAction
                        onClick={handleDelete}
                        className="bg-red-600 hover:bg-red-700"
                        disabled={deleteListingMutation.isPending}
                      >
                        {deleteListingMutation.isPending ? "..." : t("detail.confirm")}
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Buyer panel — expands below the card */}
      {panelOpen && !isSold && (
        <ListingBuyerPanel
          listingId={listing.id}
          listingTitle={`${listing.year} ${listing.makeName} ${listing.modelName}`}
          listingPrice={listing.price}
          listingThumbnail={listing.thumbnail}
          sellerId={userId ?? ""}
          onStartChat={onStartChat}
        />
      )}
    </div>
  );
}
```

> **Note:** The `ListingCardBadge` titles above reuse existing `vehicleInfo.*` i18n keys as a close approximation. If the badge labels look odd, swap them for inline strings matching the search result card's labels (`"Engine"`, `"Fuel"`, `"Gearbox"`, `"Location"`) — whichever reads better is fine.

- [ ] **Step 2: Update MyListingsPage to add ChatPanel and onStartChat state**

Replace the full file:

```typescript
// automotive.marketplace.client/src/features/myListings/components/MyListingsPage.tsx
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { ChatPanel } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";

import { getMyListingsOptions } from "../api/getMyListingsOptions";
import MyListingCard from "./MyListingCard";

export default function MyListingsPage() {
  const { t } = useTranslation("myListings");
  const myListingsQuery = useQuery(getMyListingsOptions);

  const { data, isLoading, isError } = myListingsQuery;
  const listings = data?.data ?? [];

  const [activeChatConversation, setActiveChatConversation] =
    useState<ConversationSummary | null>(null);

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8 flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-10 w-32" />
        </div>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="rounded-lg border p-4">
              <div className="flex gap-4">
                <Skeleton className="h-24 w-32 shrink-0 rounded-lg" />
                <div className="flex-1 space-y-3">
                  <Skeleton className="h-6 w-64" />
                  <Skeleton className="h-4 w-48" />
                  <div className="flex gap-2">
                    <Skeleton className="h-6 w-16" />
                    <Skeleton className="h-6 w-20" />
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center text-red-600">{t("page.loadError")}</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">{t("page.title")}</h1>
        <Button asChild>
          <Link to="/listing/create">
            <Plus className="mr-2 h-4 w-4" />
            {t("page.createListing")}
          </Link>
        </Button>
      </div>

      {/* Listings */}
      {!listings || listings.length === 0 ? (
        <div className="py-12 text-center">
          <div className="mx-auto max-w-md">
            <h2 className="mb-2 text-xl font-medium text-gray-900">
              {t("page.emptyState")}
            </h2>
            <p className="mb-6 text-gray-600">{t("page.createFirst")}</p>
            <Button asChild>
              <Link to="/listing/create">
                <Plus className="mr-2 h-4 w-4" />
                {t("page.createListing")}
              </Link>
            </Button>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {listings.map((listing) => (
            <MyListingCard
              key={listing.id}
              listing={listing}
              onStartChat={setActiveChatConversation}
            />
          ))}
        </div>
      )}

      {/* Chat slide-over — same pattern as ListingDetailsContent */}
      {activeChatConversation && (
        <ChatPanel
          conversation={activeChatConversation}
          onClose={() => setActiveChatConversation(null)}
        />
      )}
    </div>
  );
}
```

- [ ] **Step 3: Build and verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build
```

Expected: Build succeeds.

- [ ] **Step 4: Lint**

```bash
cd automotive.marketplace.client && npm run lint
```

Expected: No errors (warnings acceptable).

- [ ] **Step 5: Commit**

```bash
git add \
  src/features/myListings/components/MyListingCard.tsx \
  src/features/myListings/components/MyListingsPage.tsx
git commit -m "feat: rewrite MyListingCard with hover gallery and seller controls, add ChatPanel to MyListingsPage

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Quick Reference

### Routing

All backend endpoints use `[Route("[controller]/[action]")]` convention:
- GetMyListings → `GET /Listing/GetMy`
- GetListingEngagements → `GET /Listing/GetEngagements?id={guid}`

### Key Patterns

- `queryOptions` factory: `export const getXOptions = (param: string) => queryOptions({ queryKey: [...], queryFn: ... })`
- Axios response wrapping: `useQuery(getXOptions(id)).data` is `AxiosResponse<T>`, so actual data is at `.data.data`
- `ConversationSummary` type imported from `@/features/chat`
- `useGetOrCreateConversation` is a mutation (not a query) — call `mutateAsync({ listingId })` to get `{ data: { conversationId: string } }`
- `ChatPanel` props: `conversation: ConversationSummary`, `onClose: () => void`
- Nested C# response types accessed as `GetListingEngagementsResponse.Conversation` and `GetListingEngagementsResponse.Liker`
