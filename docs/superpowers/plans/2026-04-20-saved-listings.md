# Saved Listings (Likes + Notes) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Saved Listings hub where users can like/unlike listings and attach private notes, with heart icon on listing cards and a dedicated `/saved` page.

**Architecture:** Backend uses CQRS via MediatR with four new handlers (ToggleLike, GetSavedListings, UpsertNote, DeleteNote) plus a modification to GetAllListings to include `isLiked`. A new `UserListingNote` entity is added. Frontend adds a new `savedListings` feature folder with TanStack Query hooks, a `/saved` route, and modifies the existing ListingCard to show a heart icon.

**Tech Stack:** ASP.NET Core 8, MediatR, EF Core (PostgreSQL), FluentValidation, xUnit + TestContainers + Respawn, React 19, TanStack Query, TanStack Router, Tailwind CSS, react-icons.

---

## File Structure

### Backend — New Files

| File | Responsibility |
|------|---------------|
| `Domain/Entities/UserListingNote.cs` | Note entity |
| `Infrastructure/Data/Configuration/UserListingNoteConfiguration.cs` | EF config (unique constraint) |
| `Infrastructure/Data/Builders/UserListingNoteBuilder.cs` | Bogus test builder |
| `Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommand.cs` | Command record |
| `Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeResponse.cs` | Response record |
| `Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommandHandler.cs` | Handler |
| `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQuery.cs` | Query record |
| `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsResponse.cs` | Response record |
| `Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs` | Handler |
| `Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommand.cs` | Command record |
| `Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommandHandler.cs` | Handler |
| `Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommandValidator.cs` | FluentValidation |
| `Application/Features/SavedListingFeatures/DeleteListingNote/DeleteListingNoteCommand.cs` | Command record |
| `Application/Features/SavedListingFeatures/DeleteListingNote/DeleteListingNoteCommandHandler.cs` | Handler |
| `Server/Controllers/SavedListingController.cs` | API controller |
| `Tests/Features/HandlerTests/SavedListingHandlerTests/ToggleLikeCommandHandlerTests.cs` | Tests |
| `Tests/Features/HandlerTests/SavedListingHandlerTests/GetSavedListingsQueryHandlerTests.cs` | Tests |
| `Tests/Features/HandlerTests/SavedListingHandlerTests/UpsertListingNoteCommandHandlerTests.cs` | Tests |
| `Tests/Features/HandlerTests/SavedListingHandlerTests/DeleteListingNoteCommandHandlerTests.cs` | Tests |

### Backend — Modified Files

| File | Change |
|------|--------|
| `Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` | Add `DbSet<UserListingNote>` |
| `Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs` | Add `IsLiked` field |
| `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs` | Populate `IsLiked` from `LikeUsers` |
| `Application/Features/ListingFeatures/GetAllListings/GetAllListingsQuery.cs` | Add `UserId` property |

### Frontend — New Files

| File | Responsibility |
|------|---------------|
| `src/api/queryKeys/savedListingKeys.ts` | Query keys |
| `src/features/savedListings/types/SavedListing.ts` | Response type |
| `src/features/savedListings/types/ToggleLikeResponse.ts` | Toggle response type |
| `src/features/savedListings/api/getSavedListingsOptions.ts` | Query options |
| `src/features/savedListings/api/useToggleLike.ts` | Toggle mutation |
| `src/features/savedListings/api/useUpsertListingNote.ts` | Upsert note mutation |
| `src/features/savedListings/api/useDeleteListingNote.ts` | Delete note mutation |
| `src/features/savedListings/components/SavedListingsPage.tsx` | Main page |
| `src/features/savedListings/components/SavedListingRow.tsx` | Row component |
| `src/features/savedListings/components/NoteEditor.tsx` | Inline note editor |
| `src/features/savedListings/components/PropertyMentionPicker.tsx` | Property chip picker |
| `src/features/savedListings/index.ts` | Public exports |
| `src/app/routes/saved.tsx` | Route definition |
| `src/app/pages/SavedListings.tsx` | Page component |

### Frontend — Modified Files

| File | Change |
|------|--------|
| `src/features/listingList/components/ListingCard.tsx` | Add heart icon overlay |
| `src/features/listingList/types/GetAllListingsResponse.ts` | Add `isLiked` field |
| `src/components/layout/header/Header.tsx` | Add Saved nav link |
| `src/constants/endpoints.ts` | Add SAVED_LISTING endpoints |

---

## Task 1: Domain Entity — UserListingNote

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/UserListingNote.cs`

- [ ] **Step 1: Create the UserListingNote entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class UserListingNote : BaseEntity
{
    public Guid ListingId { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Build solution to verify compilation**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/UserListingNote.cs
git commit -m "feat(domain): add UserListingNote entity

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: EF Configuration + DbContext + Migration

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/UserListingNoteConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create EF configuration with unique constraint and cascade delete**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserListingNoteConfiguration : IEntityTypeConfiguration<UserListingNote>
{
    public void Configure(EntityTypeBuilder<UserListingNote> builder)
    {
        builder.HasIndex(note => new { note.UserId, note.ListingId })
            .IsUnique();

        builder.HasOne(note => note.Listing)
            .WithMany()
            .HasForeignKey(note => note.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(note => note.User)
            .WithMany()
            .HasForeignKey(note => note.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 2: Add DbSet to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add after the `Messages` DbSet:

```csharp
public DbSet<UserListingNote> UserListingNotes { get; set; }
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 4: Generate EF migration**

Run from repo root:
```bash
dotnet ef migrations add AddUserListingNote --project Automotive.Marketplace.Infrastructure --startup-project Automotive.Marketplace.Server
```
Expected: Migration file created in `Automotive.Marketplace.Infrastructure/Migrations/`

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/
git commit -m "feat(infra): add UserListingNote EF config, DbSet, and migration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: Test Builder — UserListingNoteBuilder

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/UserListingNoteBuilder.cs`

- [ ] **Step 1: Create the builder**

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class UserListingNoteBuilder
{
    private readonly Faker<UserListingNote> _faker;

    public UserListingNoteBuilder()
    {
        _faker = new Faker<UserListingNote>()
            .RuleFor(note => note.Id, f => f.Random.Guid())
            .RuleFor(note => note.Content, f => f.Lorem.Sentence());
    }

    public UserListingNoteBuilder WithUser(Guid userId)
    {
        _faker.RuleFor(note => note.UserId, userId);
        return this;
    }

    public UserListingNoteBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(note => note.ListingId, listingId);
        return this;
    }

    public UserListingNoteBuilder WithContent(string content)
    {
        _faker.RuleFor(note => note.Content, content);
        return this;
    }

    public UserListingNoteBuilder With<T>(Expression<Func<UserListingNote, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public UserListingNote Build() => _faker.Generate();
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Data/Builders/UserListingNoteBuilder.cs
git commit -m "feat(infra): add UserListingNoteBuilder for tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: ToggleLikeCommand Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/ToggleLikeCommandHandler.cs`

- [ ] **Step 1: Create ToggleLikeCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public sealed record ToggleLikeCommand : IRequest<ToggleLikeResponse>
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create ToggleLikeResponse**

```csharp
namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public sealed record ToggleLikeResponse
{
    public bool IsLiked { get; set; }
}
```

- [ ] **Step 3: Create ToggleLikeCommandHandler**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public class ToggleLikeCommandHandler(IRepository repository)
    : IRequestHandler<ToggleLikeCommand, ToggleLikeResponse>
{
    public async Task<ToggleLikeResponse> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var existingLike = await repository
            .AsQueryable<UserListingLike>()
            .FirstOrDefaultAsync(
                like => like.UserId == request.UserId && like.ListingId == request.ListingId,
                cancellationToken);

        if (existingLike is not null)
        {
            var existingNote = await repository
                .AsQueryable<UserListingNote>()
                .FirstOrDefaultAsync(
                    note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                    cancellationToken);

            if (existingNote is not null)
            {
                await repository.DeleteAsync(existingNote, cancellationToken);
            }

            await repository.DeleteAsync(existingLike, cancellationToken);
            return new ToggleLikeResponse { IsLiked = false };
        }

        var newLike = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ListingId = request.ListingId
        };

        await repository.CreateAsync(newLike, cancellationToken);
        return new ToggleLikeResponse { IsLiked = true };
    }
}
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/SavedListingFeatures/ToggleLike/
git commit -m "feat(app): add ToggleLikeCommand handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: ToggleLike Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/ToggleLikeCommandHandlerTests.cs`

- [ ] **Step 1: Write the test class with all test cases**

```csharp
using Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class ToggleLikeCommandHandlerTests(
    DatabaseFixture<ToggleLikeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ToggleLikeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ToggleLikeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static ToggleLikeCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingLike_ShouldCreateLikeAndReturnIsLikedTrue()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeTrue();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExistingLike_ShouldDeleteLikeAndReturnIsLikedFalse()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingLikeWithNote_ShouldDeleteBothLikeAndNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };
        var note = new UserListingNote
        {
            Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, Content = "Great car!"
        };
        await context.AddRangeAsync(like, note);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().BeNull();
    }

    private static async Task<(User user, Listing listing)> SeedUserAndListingAsync(AutomotiveContext context)
    {
        var user = new UserBuilder().Build();
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
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
```

- [ ] **Step 2: Run the tests**

Run: `dotnet test --filter "FullyQualifiedName~ToggleLikeCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests passed

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/
git commit -m "test: add ToggleLikeCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: GetSavedListings Query Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs`

- [ ] **Step 1: Create GetSavedListingsQuery**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public sealed record GetSavedListingsQuery : IRequest<IEnumerable<GetSavedListingsResponse>>
{
    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create GetSavedListingsResponse**

```csharp
using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public sealed record GetSavedListingsResponse
{
    public Guid ListingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string City { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public string FuelName { get; set; } = string.Empty;

    public string TransmissionName { get; set; } = string.Empty;

    public ImageDto? Thumbnail { get; set; }

    public string? NoteContent { get; set; }
}
```

- [ ] **Step 3: Create GetSavedListingsQueryHandler**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public class GetSavedListingsQueryHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<GetSavedListingsQuery, IEnumerable<GetSavedListingsResponse>>
{
    public async Task<IEnumerable<GetSavedListingsResponse>> Handle(
        GetSavedListingsQuery request,
        CancellationToken cancellationToken)
    {
        var likes = await repository
            .AsQueryable<UserListingLike>()
            .Where(like => like.UserId == request.UserId)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Model)
                        .ThenInclude(model => model.Make)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Fuel)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Variant)
                    .ThenInclude(variant => variant.Transmission)
            .Include(like => like.Listing)
                .ThenInclude(listing => listing.Images)
            .ToListAsync(cancellationToken);

        var listingIds = likes.Select(l => l.ListingId).ToList();

        var notes = await repository
            .AsQueryable<UserListingNote>()
            .Where(note => note.UserId == request.UserId && listingIds.Contains(note.ListingId))
            .ToDictionaryAsync(note => note.ListingId, note => note.Content, cancellationToken);

        var result = new List<GetSavedListingsResponse>();

        foreach (var like in likes)
        {
            var listing = like.Listing;
            var variant = listing.Variant;

            string? thumbnailUrl = null;
            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage is not null)
            {
                thumbnailUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey);
            }

            result.Add(new GetSavedListingsResponse
            {
                ListingId = listing.Id,
                Title = $"{variant.Year} {variant.Model.Make.Name} {variant.Model.Name}",
                Price = listing.Price,
                City = listing.City,
                Mileage = listing.Mileage,
                FuelName = variant.Fuel.Name,
                TransmissionName = variant.Transmission.Name,
                Thumbnail = thumbnailUrl is not null
                    ? new Application.Models.ImageDto { Url = thumbnailUrl, AltText = firstImage!.AltText }
                    : null,
                NoteContent = notes.GetValueOrDefault(listing.Id)
            });
        }

        return result;
    }
}
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/
git commit -m "feat(app): add GetSavedListings query handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 7: GetSavedListings Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/GetSavedListingsQueryHandlerTests.cs`

- [ ] **Step 1: Write the test class**

```csharp
using Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;
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

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class GetSavedListingsQueryHandlerTests(
    DatabaseFixture<GetSavedListingsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetSavedListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetSavedListingsQueryHandlerTests> _fixture = fixture;

    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetSavedListingsQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(), _imageStorageService);

    [Fact]
    public async Task Handle_UserWithLikedListings_ShouldReturnListingsWithDetails()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing, _) = await SeedLikeWithNoteAsync(context, noteContent: null);

        // Act
        var result = (await handler.Handle(
            new GetSavedListingsQuery { UserId = user.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ListingId.Should().Be(listing.Id);
        result[0].Price.Should().Be(listing.Price);
        result[0].City.Should().Be(listing.City);
        result[0].Mileage.Should().Be(listing.Mileage);
        result[0].NoteContent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserWithLikedListingAndNote_ShouldReturnNoteContent()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing, _) = await SeedLikeWithNoteAsync(context, noteContent: "Check this one!");

        // Act
        var result = (await handler.Handle(
            new GetSavedListingsQuery { UserId = user.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].NoteContent.Should().Be("Check this one!");
    }

    [Fact]
    public async Task Handle_UserWithNoLikes_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        // Act
        var result = await handler.Handle(
            new GetSavedListingsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    private static async Task<(User user, Listing listing, UserListingLike like)> SeedLikeWithNoteAsync(
        AutomotiveContext context, string? noteContent)
    {
        var user = new UserBuilder().Build();
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
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like);

        if (noteContent is not null)
        {
            var note = new UserListingNote
            {
                Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, Content = noteContent
            };
            await context.AddAsync(note);
        }

        await context.SaveChangesAsync();
        return (user, listing, like);
    }
}
```

- [ ] **Step 2: Run the tests**

Run: `dotnet test --filter "FullyQualifiedName~GetSavedListingsQueryHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests passed

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/GetSavedListingsQueryHandlerTests.cs
git commit -m "test: add GetSavedListingsQueryHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 8: UpsertListingNote Command Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/UpsertListingNote/UpsertListingNoteCommandValidator.cs`

- [ ] **Step 1: Create UpsertListingNoteCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public sealed record UpsertListingNoteCommand : IRequest
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create UpsertListingNoteCommandValidator**

```csharp
using FluentValidation;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public sealed class UpsertListingNoteCommandValidator : AbstractValidator<UpsertListingNoteCommand>
{
    public UpsertListingNoteCommandValidator()
    {
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(command => command.ListingId)
            .NotEmpty();
    }
}
```

- [ ] **Step 3: Create UpsertListingNoteCommandHandler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public class UpsertListingNoteCommandHandler(IRepository repository)
    : IRequestHandler<UpsertListingNoteCommand>
{
    public async Task Handle(UpsertListingNoteCommand request, CancellationToken cancellationToken)
    {
        var likeExists = await repository
            .AsQueryable<UserListingLike>()
            .AnyAsync(
                like => like.UserId == request.UserId && like.ListingId == request.ListingId,
                cancellationToken);

        if (!likeExists)
        {
            throw new DbEntityNotFoundException(nameof(UserListingLike), request.ListingId);
        }

        var existingNote = await repository
            .AsQueryable<UserListingNote>()
            .FirstOrDefaultAsync(
                note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                cancellationToken);

        if (existingNote is not null)
        {
            existingNote.Content = request.Content;
            await repository.UpdateAsync(existingNote, cancellationToken);
        }
        else
        {
            var newNote = new UserListingNote
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ListingId = request.ListingId,
                Content = request.Content
            };
            await repository.CreateAsync(newNote, cancellationToken);
        }
    }
}
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/SavedListingFeatures/UpsertListingNote/
git commit -m "feat(app): add UpsertListingNote command handler with validation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 9: UpsertListingNote Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/UpsertListingNoteCommandHandlerTests.cs`

- [ ] **Step 1: Write the test class**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class UpsertListingNoteCommandHandlerTests(
    DatabaseFixture<UpsertListingNoteCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpsertListingNoteCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpsertListingNoteCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpsertListingNoteCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingNote_ShouldCreateNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAsync(context);

        var command = new UpsertListingNoteCommand
        {
            UserId = user.Id, ListingId = listing.Id, Content = "Nice car!"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().NotBeNull();
        noteInDb!.Content.Should().Be("Nice car!");
    }

    [Fact]
    public async Task Handle_ExistingNote_ShouldUpdateContent()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAsync(context);
        var note = new UserListingNote
        {
            Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, Content = "Old note"
        };
        await context.AddAsync(note);
        await context.SaveChangesAsync();

        var command = new UpsertListingNoteCommand
        {
            UserId = user.Id, ListingId = listing.Id, Content = "Updated note"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb!.Content.Should().Be("Updated note");
    }

    [Fact]
    public async Task Handle_NoLikeExists_ShouldThrowNotFoundException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var command = new UpsertListingNoteCommand
        {
            UserId = Guid.NewGuid(), ListingId = Guid.NewGuid(), Content = "Orphan note"
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    private static async Task<(User user, Listing listing)> SeedUserWithLikeAsync(AutomotiveContext context)
    {
        var user = new UserBuilder().Build();
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
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
```

- [ ] **Step 2: Run the tests**

Run: `dotnet test --filter "FullyQualifiedName~UpsertListingNoteCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests passed

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/UpsertListingNoteCommandHandlerTests.cs
git commit -m "test: add UpsertListingNoteCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 10: DeleteListingNote Command Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/DeleteListingNote/DeleteListingNoteCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/SavedListingFeatures/DeleteListingNote/DeleteListingNoteCommandHandler.cs`

- [ ] **Step 1: Create DeleteListingNoteCommand**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;

public sealed record DeleteListingNoteCommand : IRequest
{
    public Guid ListingId { get; set; }

    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create DeleteListingNoteCommandHandler**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;

public class DeleteListingNoteCommandHandler(IRepository repository)
    : IRequestHandler<DeleteListingNoteCommand>
{
    public async Task Handle(DeleteListingNoteCommand request, CancellationToken cancellationToken)
    {
        var existingNote = await repository
            .AsQueryable<UserListingNote>()
            .FirstOrDefaultAsync(
                note => note.UserId == request.UserId && note.ListingId == request.ListingId,
                cancellationToken);

        if (existingNote is not null)
        {
            await repository.DeleteAsync(existingNote, cancellationToken);
        }
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Features/SavedListingFeatures/DeleteListingNote/
git commit -m "feat(app): add DeleteListingNote command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 11: DeleteListingNote Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/DeleteListingNoteCommandHandlerTests.cs`

- [ ] **Step 1: Write the test class**

```csharp
using Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class DeleteListingNoteCommandHandlerTests(
    DatabaseFixture<DeleteListingNoteCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteListingNoteCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteListingNoteCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteListingNoteCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ExistingNote_ShouldDeleteNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAndNoteAsync(context);

        var command = new DeleteListingNoteCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoExistingNote_ShouldNotThrow()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var command = new DeleteListingNoteCommand { UserId = Guid.NewGuid(), ListingId = Guid.NewGuid() };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_DeleteNote_ShouldNotDeleteLike()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAndNoteAsync(context);

        var command = new DeleteListingNoteCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().NotBeNull();
    }

    private static async Task<(User user, Listing listing)> SeedUserWithLikeAndNoteAsync(AutomotiveContext context)
    {
        var user = new UserBuilder().Build();
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
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };
        var note = new UserListingNote
        {
            Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, Content = "Some note"
        };

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like, note);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
```

- [ ] **Step 2: Run the tests**

Run: `dotnet test --filter "FullyQualifiedName~DeleteListingNoteCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests passed

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/SavedListingHandlerTests/DeleteListingNoteCommandHandlerTests.cs
git commit -m "test: add DeleteListingNoteCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 12: SavedListing Controller

**Files:**
- Create: `Automotive.Marketplace.Server/Controllers/SavedListingController.cs`

- [ ] **Step 1: Create the controller**

```csharp
using Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[Authorize]
public class SavedListingController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<ToggleLikeResponse>> ToggleLike(
        [FromBody] ToggleLikeCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetSavedListingsResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetSavedListingsQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpsertNote(
        [FromBody] UpsertListingNoteCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteNote(
        [FromQuery] DeleteListingNoteCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/SavedListingController.cs
git commit -m "feat(server): add SavedListingController with all endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 13: Add IsLiked to GetAllListings

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`

- [ ] **Step 1: Add IsLiked to GetAllListingsResponse**

In `GetAllListingsResponse.cs`, add after the `Thumbnail` property:

```csharp
    public bool IsLiked { get; set; }
```

- [ ] **Step 2: Add UserId to GetAllListingsQuery**

In `GetAllListingsQuery.cs`, add a property:

```csharp
    public Guid? UserId { get; set; }
```

- [ ] **Step 3: Modify GetAllListingsQueryHandler to populate IsLiked**

In `GetAllListingsQueryHandler.cs`, after the existing `.Include(l => l.Images)` line, add:

```csharp
            .Include(l => l.LikeUsers)
```

Then inside the `foreach` loop, after setting `Thumbnail`, add:

```csharp
            if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            {
                mappedListing.IsLiked = listing.LikeUsers.Any(u => u.Id == request.UserId.Value);
            }
```

- [ ] **Step 4: Update the ListingController to pass UserId**

In `Automotive.Marketplace.Server/Controllers/ListingController.cs`, modify the `GetAll` action to pass UserId:

Change from:
```csharp
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllListingsResponse>>> GetAll(
        [FromQuery] GetAllListingsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
```

To:
```csharp
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllListingsResponse>>> GetAll(
        [FromQuery] GetAllListingsQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId != Guid.Empty ? UserId : null;
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
```

- [ ] **Step 5: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 6: Run existing listing handler tests to verify no regression**

Run: `dotnet test --filter "FullyQualifiedName~ListingHandlerTests" ./Automotive.Marketplace.sln`
Expected: All existing tests pass

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/ Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat(app): add IsLiked field to GetAllListings response

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 14: Frontend — Endpoints, Query Keys, Types

**Files:**
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Create: `automotive.marketplace.client/src/api/queryKeys/savedListingKeys.ts`
- Create: `automotive.marketplace.client/src/features/savedListings/types/SavedListing.ts`
- Create: `automotive.marketplace.client/src/features/savedListings/types/ToggleLikeResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/GetAllListingsResponse.ts`

- [ ] **Step 1: Add SAVED_LISTING endpoints**

In `src/constants/endpoints.ts`, add a new section before the closing `} as const`:

```typescript
  SAVED_LISTING: {
    TOGGLE_LIKE: "/SavedListing/ToggleLike",
    GET_ALL: "/SavedListing/GetAll",
    UPSERT_NOTE: "/SavedListing/UpsertNote",
    DELETE_NOTE: "/SavedListing/DeleteNote",
  },
```

- [ ] **Step 2: Create savedListingKeys**

```typescript
export const savedListingKeys = {
  all: () => ["savedListing"] as const,
  list: () => [...savedListingKeys.all(), "list"] as const,
};
```

- [ ] **Step 3: Create SavedListing type**

```typescript
export type SavedListing = {
  listingId: string;
  title: string;
  price: number;
  city: string;
  mileage: number;
  fuelName: string;
  transmissionName: string;
  thumbnail: { url: string; altText: string } | null;
  noteContent: string | null;
};
```

- [ ] **Step 4: Create ToggleLikeResponse type**

```typescript
export type ToggleLikeResponse = {
  isLiked: boolean;
};
```

- [ ] **Step 5: Add isLiked to GetAllListingsResponse**

In `src/features/listingList/types/GetAllListingsResponse.ts`, add after `thumbnail`:

```typescript
  isLiked: boolean;
```

- [ ] **Step 6: Commit**

```bash
cd automotive.marketplace.client
git add src/constants/endpoints.ts src/api/queryKeys/savedListingKeys.ts src/features/savedListings/types/ src/features/listingList/types/GetAllListingsResponse.ts
git commit -m "feat(fe): add saved listing endpoints, query keys, and types

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 15: Frontend — API Hooks (Mutations + Query)

**Files:**
- Create: `automotive.marketplace.client/src/features/savedListings/api/getSavedListingsOptions.ts`
- Create: `automotive.marketplace.client/src/features/savedListings/api/useToggleLike.ts`
- Create: `automotive.marketplace.client/src/features/savedListings/api/useUpsertListingNote.ts`
- Create: `automotive.marketplace.client/src/features/savedListings/api/useDeleteListingNote.ts`

- [ ] **Step 1: Create getSavedListingsOptions**

```typescript
import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { SavedListing } from "../types/SavedListing";

const getSavedListings = () =>
  axiosClient.get<SavedListing[]>(ENDPOINTS.SAVED_LISTING.GET_ALL);

export const getSavedListingsOptions = () =>
  queryOptions({
    queryKey: savedListingKeys.list(),
    queryFn: () => getSavedListings(),
  });
```

- [ ] **Step 2: Create useToggleLike**

```typescript
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ToggleLikeResponse } from "../types/ToggleLikeResponse";

type ToggleLikeCommand = {
  listingId: string;
};

const toggleLike = (command: ToggleLikeCommand) =>
  axiosClient.post<ToggleLikeResponse>(
    ENDPOINTS.SAVED_LISTING.TOGGLE_LIKE,
    command,
  );

export const useToggleLike = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: toggleLike,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: savedListingKeys.all() });
      void queryClient.invalidateQueries({ queryKey: listingKeys.all() });
    },
    meta: {
      errorMessage: "Could not update like. Please try again.",
    },
  });
};
```

- [ ] **Step 3: Create useUpsertListingNote**

```typescript
import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type UpsertListingNoteCommand = {
  listingId: string;
  content: string;
};

const upsertListingNote = (command: UpsertListingNoteCommand) =>
  axiosClient.put<void>(ENDPOINTS.SAVED_LISTING.UPSERT_NOTE, command);

export const useUpsertListingNote = () =>
  useMutation({
    mutationFn: upsertListingNote,
    meta: {
      errorMessage: "Could not save note. Please try again.",
      invalidatesQuery: savedListingKeys.list(),
    },
  });
```

- [ ] **Step 4: Create useDeleteListingNote**

```typescript
import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type DeleteListingNoteCommand = {
  listingId: string;
};

const deleteListingNote = (command: DeleteListingNoteCommand) =>
  axiosClient.delete<void>(ENDPOINTS.SAVED_LISTING.DELETE_NOTE, {
    params: command,
  });

export const useDeleteListingNote = () =>
  useMutation({
    mutationFn: deleteListingNote,
    meta: {
      errorMessage: "Could not delete note. Please try again.",
      invalidatesQuery: savedListingKeys.list(),
    },
  });
```

- [ ] **Step 5: Commit**

```bash
git add src/features/savedListings/api/
git commit -m "feat(fe): add saved listing API hooks (query + mutations)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 16: Frontend — Heart Icon on ListingCard

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx`

- [ ] **Step 1: Add heart icon overlay to ListingCard**

Replace the full content of `ListingCard.tsx`:

```tsx
import { Button } from "@/components/ui/button";
import { selectAccessToken } from "@/features/auth";
import { useToggleLike } from "@/features/savedListings/api/useToggleLike";
import { useAppSelector } from "@/hooks/redux";
import { router } from "@/lib/router";
import { IoLocationOutline } from "react-icons/io5";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { PiEngine } from "react-icons/pi";
import { TbManualGearbox } from "react-icons/tb";
import { IoHeartOutline, IoHeart } from "react-icons/io5";
import { GetAllListingsResponse } from "../types/GetAllListingsResponse";
import ListingCardBadge from "./ListingCardBadge";

interface ListingCardProps {
  listing: GetAllListingsResponse;
}

const ListingCard = ({ listing }: ListingCardProps) => {
  const accessToken = useAppSelector(selectAccessToken);
  const toggleLike = useToggleLike();

  const handleClick = async () => {
    await router.navigate({ to: "/listing/$id", params: { id: listing.id } });
  };

  const handleLikeClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    toggleLike.mutate({ listingId: listing.id });
  };

  return (
    <div className="bg-card border-border grid w-full gap-8 border-1 md:grid-cols-2">
      <div className="group relative flex flex-shrink-0 py-5">
        <img
          className="aspect-[4/3] object-cover"
          alt={listing.thumbnail?.altText || "Listing image"}
          src={
            listing.thumbnail
              ? listing.thumbnail.url
              : "https://imgs.search.brave.com/_avFlFDyXU8SS34ve__STsLcC6LfrFsy76XnfAbI4Vo/rs:fit:860:0:0:0/g:ce/aHR0cHM6Ly9tZWRp/YS5nZXR0eWltYWdl/cy5jb20vaWQvNDU5/NDQ1ODUxL3Bob3Rv/L3RveW90YS1wcml1/cy5qcGc_cz02MTJ4/NjEyJnc9MCZrPTIw/JmM9OGRDdF9lSGxP/YzhMcUxEQllYME42/N0FpZFNNd2lRT0ZT/LVhzMUxYcnBjQT0"
          }
        />
        {accessToken && (
          <button
            onClick={handleLikeClick}
            className={`absolute top-7 left-2 flex h-9 w-9 items-center justify-center rounded-full transition-opacity ${
              listing.isLiked
                ? "bg-red-500 opacity-100"
                : "bg-black/50 opacity-0 group-hover:opacity-100"
            }`}
          >
            {listing.isLiked ? (
              <IoHeart className="h-5 w-5 text-white" />
            ) : (
              <IoHeartOutline className="h-5 w-5 text-white" />
            )}
          </button>
        )}
      </div>
      <div className="flex min-w-0 flex-grow flex-col justify-between pt-4 pr-4 pb-2">
        <div className="truncate">
          <p className="truncate font-sans text-xs">
            {listing.isUsed ? "Used" : "New"}
          </p>
          <p className="font-sans text-xl">{`${listing.year} ${listing.make} ${listing.model}`}</p>
          <p className="font-sans text-xs">{listing.mileage} km</p>
          <p className="font-sans text-3xl font-bold">
            {listing.price.toFixed(0)} €
          </p>
        </div>
        <div className="justify-items-stretched grid grid-cols-2 gap-x-0 gap-y-4">
          <div className="flex justify-self-start">
            <ListingCardBadge
              Icon={<PiEngine className="h-8 w-8" />}
              title={"Engine"}
              stat={`${listing.engineSizeMl / 1000} l ${listing.powerKw} kW`}
            />
          </div>
          <div className="flex justify-self-end">
            <ListingCardBadge
              Icon={<MdOutlineLocalGasStation className="h-8 w-8" />}
              title={"Fuel Type"}
              stat={listing.fuelType}
            />
          </div>
          <div className="flex justify-self-start">
            <ListingCardBadge
              Icon={<TbManualGearbox className="h-8 w-8" />}
              title={"Gear Box"}
              stat={listing.transmission}
            />
          </div>
          <div className="flex justify-self-end">
            <ListingCardBadge
              Icon={<IoLocationOutline className="h-8 w-8" />}
              title={"Location"}
              stat={listing.city}
            />
          </div>
        </div>
        <div className="flex justify-end">
          <Button
            className="h-full max-h-12 rounded-3xl text-xl font-bold"
            onClick={handleClick}
          >
            Check out
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ListingCard;
```

- [ ] **Step 2: Run lint/build**

Run: `npm run lint && npm run build`
Expected: No errors

- [ ] **Step 3: Commit**

```bash
git add src/features/listingList/components/ListingCard.tsx
git commit -m "feat(fe): add heart icon overlay to ListingCard

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 17: Frontend — SavedListings Page Components

**Files:**
- Create: `automotive.marketplace.client/src/features/savedListings/components/NoteEditor.tsx`
- Create: `automotive.marketplace.client/src/features/savedListings/components/PropertyMentionPicker.tsx`
- Create: `automotive.marketplace.client/src/features/savedListings/components/SavedListingRow.tsx`
- Create: `automotive.marketplace.client/src/features/savedListings/components/SavedListingsPage.tsx`

- [ ] **Step 1: Create PropertyMentionPicker**

```tsx
import type { SavedListing } from "../types/SavedListing";

interface PropertyMentionPickerProps {
  listing: SavedListing;
  onSelect: (chip: string) => void;
  onClose: () => void;
}

const PROPERTY_FIELDS: { key: keyof SavedListing; label: string; format: (value: unknown) => string }[] = [
  { key: "mileage", label: "Mileage", format: (v) => `${(v as number).toLocaleString()} km` },
  { key: "price", label: "Price", format: (v) => `${(v as number).toLocaleString()} €` },
  { key: "fuelName", label: "Fuel", format: (v) => v as string },
  { key: "transmissionName", label: "Transmission", format: (v) => v as string },
  { key: "city", label: "City", format: (v) => v as string },
];

const PropertyMentionPicker = ({ listing, onSelect, onClose }: PropertyMentionPickerProps) => {
  return (
    <div className="border-border bg-card absolute z-10 mt-1 rounded border shadow-lg">
      <ul className="py-1">
        {PROPERTY_FIELDS.map(({ key, label, format }) => (
          <li key={key}>
            <button
              className="hover:bg-muted w-full px-3 py-1.5 text-left text-sm"
              onClick={() => {
                onSelect(`📌 ${label} · ${format(listing[key])}`);
                onClose();
              }}
            >
              <span className="font-medium">{label}</span>
              <span className="text-muted-foreground ml-2">{format(listing[key])}</span>
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default PropertyMentionPicker;
```

- [ ] **Step 2: Create NoteEditor**

```tsx
import { useRef, useState } from "react";
import { useUpsertListingNote } from "../api/useUpsertListingNote";
import { useDeleteListingNote } from "../api/useDeleteListingNote";
import type { SavedListing } from "../types/SavedListing";
import PropertyMentionPicker from "./PropertyMentionPicker";

interface NoteEditorProps {
  listing: SavedListing;
  isExpanded: boolean;
}

const NoteEditor = ({ listing, isExpanded }: NoteEditorProps) => {
  const [text, setText] = useState(listing.noteContent ?? "");
  const [isEditing, setIsEditing] = useState(false);
  const [showPicker, setShowPicker] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const upsertNote = useUpsertListingNote();
  const deleteNote = useDeleteListingNote();

  const handleBlur = () => {
    setIsEditing(false);
    setShowPicker(false);

    const trimmed = text.trim();
    if (trimmed === "" && listing.noteContent) {
      deleteNote.mutate({ listingId: listing.listingId });
    } else if (trimmed !== "" && trimmed !== listing.noteContent) {
      upsertNote.mutate({ listingId: listing.listingId, content: trimmed });
    }
  };

  const handleChipInsert = (chip: string) => {
    const textarea = textareaRef.current;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const newText = text.slice(0, start) + chip + text.slice(end);
    setText(newText);

    setTimeout(() => {
      textarea.selectionStart = start + chip.length;
      textarea.selectionEnd = start + chip.length;
      textarea.focus();
    }, 0);
  };

  if (!isExpanded && !listing.noteContent) return null;

  if (!isExpanded && listing.noteContent) {
    return (
      <p className="text-muted-foreground truncate text-sm">
        {listing.noteContent}
      </p>
    );
  }

  return (
    <div className="relative mt-2">
      {isEditing ? (
        <div className="border-l-2 border-red-500 pl-3">
          <div className="relative">
            <textarea
              ref={textareaRef}
              className="bg-transparent w-full resize-none text-sm outline-none"
              value={text}
              onChange={(e) => setText(e.target.value)}
              onBlur={handleBlur}
              rows={3}
              autoFocus
            />
            <button
              className="text-muted-foreground hover:text-foreground absolute right-0 bottom-0 text-lg"
              onMouseDown={(e) => {
                e.preventDefault();
                setShowPicker(!showPicker);
              }}
            >
              +
            </button>
          </div>
          {showPicker && (
            <PropertyMentionPicker
              listing={listing}
              onSelect={handleChipInsert}
              onClose={() => setShowPicker(false)}
            />
          )}
        </div>
      ) : (
        <div
          className="cursor-pointer border-l-2 border-red-500 pl-3"
          onClick={() => setIsEditing(true)}
        >
          {listing.noteContent ? (
            <p className="text-sm">{listing.noteContent}</p>
          ) : (
            <p className="text-muted-foreground text-sm italic">
              Click to add a note…
            </p>
          )}
        </div>
      )}
    </div>
  );
};

export default NoteEditor;
```

- [ ] **Step 3: Create SavedListingRow**

```tsx
import { useState } from "react";
import { IoHeart } from "react-icons/io5";
import { useToggleLike } from "../api/useToggleLike";
import type { SavedListing } from "../types/SavedListing";
import NoteEditor from "./NoteEditor";

interface SavedListingRowProps {
  listing: SavedListing;
}

const SavedListingRow = ({ listing }: SavedListingRowProps) => {
  const [isHovered, setIsHovered] = useState(false);
  const toggleLike = useToggleLike();

  const handleUnlike = () => {
    toggleLike.mutate({ listingId: listing.listingId });
  };

  return (
    <div
      className="border-border hover:bg-muted/50 flex gap-4 border-b p-4 transition-colors"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Thumbnail */}
      <div className="h-20 w-28 flex-shrink-0 overflow-hidden rounded">
        {listing.thumbnail ? (
          <img
            src={listing.thumbnail.url}
            alt={listing.thumbnail.altText}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="bg-muted flex h-full w-full items-center justify-center text-xs">
            No image
          </div>
        )}
      </div>

      {/* Content */}
      <div className="flex min-w-0 flex-1 flex-col">
        <div className="flex items-start justify-between">
          <div className="min-w-0">
            <p className="truncate font-medium">{listing.title}</p>
            <p className="text-muted-foreground text-sm">
              {listing.price.toLocaleString()} € · {listing.city} ·{" "}
              {listing.mileage.toLocaleString()} km · {listing.fuelName} ·{" "}
              {listing.transmissionName}
            </p>
          </div>
          <button
            onClick={handleUnlike}
            className="ml-2 flex-shrink-0 text-red-500 transition-opacity hover:opacity-70"
            title="Remove from saved"
          >
            <IoHeart className="h-5 w-5" />
          </button>
        </div>

        <NoteEditor listing={listing} isExpanded={isHovered} />
      </div>
    </div>
  );
};

export default SavedListingRow;
```

- [ ] **Step 4: Create SavedListingsPage**

```tsx
import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { useQuery } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { getSavedListingsOptions } from "../api/getSavedListingsOptions";
import SavedListingRow from "./SavedListingRow";

const SavedListingsPage = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const { data: savedQuery } = useQuery({
    ...getSavedListingsOptions(),
    enabled: !!accessToken,
  });

  const listings = savedQuery?.data ?? [];

  if (listings.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <p className="text-muted-foreground text-lg">
          You haven't saved any listings yet.
        </p>
        <Link
          to="/listings"
          className="mt-4 text-sm text-red-500 underline hover:text-red-600"
        >
          Browse listings
        </Link>
      </div>
    );
  }

  return (
    <div className="py-6">
      <h1 className="mb-4 text-2xl font-bold">Saved Listings</h1>
      <div className="border-border divide-border divide-y rounded border">
        {listings.map((listing) => (
          <SavedListingRow key={listing.listingId} listing={listing} />
        ))}
      </div>
    </div>
  );
};

export default SavedListingsPage;
```

- [ ] **Step 5: Run lint/build**

Run: `npm run lint && npm run build`
Expected: No errors

- [ ] **Step 6: Commit**

```bash
git add src/features/savedListings/components/
git commit -m "feat(fe): add SavedListings page components with note editor

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 18: Frontend — Route, Page, Feature Index, Header Link

**Files:**
- Create: `automotive.marketplace.client/src/features/savedListings/index.ts`
- Create: `automotive.marketplace.client/src/app/pages/SavedListings.tsx`
- Create: `automotive.marketplace.client/src/app/routes/saved.tsx`
- Modify: `automotive.marketplace.client/src/components/layout/header/Header.tsx`

- [ ] **Step 1: Create feature index.ts**

```typescript
export { default as SavedListingsPage } from "./components/SavedListingsPage";
export { useToggleLike } from "./api/useToggleLike";
export { getSavedListingsOptions } from "./api/getSavedListingsOptions";
```

- [ ] **Step 2: Create page component**

```tsx
import { SavedListingsPage } from "@/features/savedListings";

const SavedListings = () => {
  return <SavedListingsPage />;
};

export default SavedListings;
```

- [ ] **Step 3: Create route file**

```tsx
import SavedListings from "@/app/pages/SavedListings";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { store } from "@/lib/redux/store";

export const Route = createFileRoute("/saved")({
  beforeLoad: () => {
    const { auth } = store.getState();
    if (!auth.userId) {
      throw redirect({ to: "/login" });
    }
  },
  component: SavedListings,
});
```

- [ ] **Step 4: Add Saved link to Header**

In `src/components/layout/header/Header.tsx`, add after the inbox Link block (after the `)}` that closes the `{userId && (` inbox section):

```tsx
          {userId && (
            <Link to="/saved">
              <Button variant="link">Saved</Button>
            </Link>
          )}
```

- [ ] **Step 5: Run lint/build to verify**

Run: `npm run lint && npm run build`
Expected: No errors (TanStack Router auto-generates route tree)

- [ ] **Step 6: Commit**

```bash
git add src/features/savedListings/index.ts src/app/pages/SavedListings.tsx src/app/routes/saved.tsx src/components/layout/header/Header.tsx
git commit -m "feat(fe): add /saved route, page, feature exports, and header link

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 19: Final Verification

- [ ] **Step 1: Run all backend tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All tests pass (existing + 12 new)

- [ ] **Step 2: Run frontend lint + build**

Run: `cd automotive.marketplace.client && npm run lint && npm run build`
Expected: No errors

- [ ] **Step 3: Verify backend builds clean**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded with no warnings from new code

---
