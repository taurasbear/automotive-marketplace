# Listing Comparison Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `/compare?a={id}&b={id}` page with a 3-column comparison table, difference highlighting, and a search modal on the listing detail page to pick the second listing.

**Architecture:** Two new backend CQRS query handlers (`SearchListingsQueryHandler`, `GetListingComparisonQueryHandler`) exposed as `GET /Listing/Search` and `GET /Listing/Compare`. Frontend adds a `compareListings` feature folder with API hooks, comparison table/row components, a search modal, a floating diff-toggle button, and a `/compare` TanStack Router file-based route. `GetListingByIdResponse` is extended with `id` and `status` fields needed for the comparison table.

**Tech Stack:** ASP.NET Core 8 · MediatR · EF Core · AutoMapper (backend) · React 19 · TypeScript · TanStack Query v5 · TanStack Router · Radix UI Dialog · Tailwind CSS · Vitest + React Testing Library (frontend)

---

## File Structure

**New/Modified Backend Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonQueryHandler.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/SearchListingsQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonQueryHandlerTests.cs`

**New/Modified Frontend Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonResponse.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/types/SearchListingsResponse.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/types/diff.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/utils/computeDiff.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonOptions.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/api/searchListingsOptions.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareRow.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareHeader.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareSearchModal.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/DiffToggleFab.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/index.ts`
- Create: `automotive.marketplace.client/src/app/routes/compare.tsx`
- Create: `automotive.marketplace.client/src/app/pages/Compare.tsx`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/utils/computeDiff.test.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.test.tsx`
- Create: `automotive.marketplace.client/src/app/pages/Compare.test.tsx`

---

### Task 1: SearchListings — CQRS classes and handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/SearchListingsQueryHandler.cs`

- [ ] **Step 1: Create SearchListingsQuery.cs**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public sealed record SearchListingsQuery : IRequest<IEnumerable<SearchListingsResponse>>
{
    public string Q { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}
```

- [ ] **Step 2: Create SearchListingsResponse.cs**

```csharp
namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public sealed record SearchListingsResponse
{
    public Guid Id { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string City { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string? FirstImageUrl { get; set; }
}
```

- [ ] **Step 3: Create SearchListingsQueryHandler.cs**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public class SearchListingsQueryHandler(
    IRepository repository,
    IImageStorageService imageStorageService)
    : IRequestHandler<SearchListingsQuery, IEnumerable<SearchListingsResponse>>
{
    public async Task<IEnumerable<SearchListingsResponse>> Handle(
        SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var q = request.Q.Trim().ToLower();
        var isYear = q.Length == 4 && int.TryParse(q, out var yearValue);

        var listings = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Where(l =>
                l.Variant.Model.Make.Name.ToLower().Contains(q) ||
                l.Variant.Model.Name.ToLower().Contains(q) ||
                (isYear && l.Year == yearValue) ||
                l.Seller.Username.ToLower().Contains(q) ||
                l.Id.ToString() == request.Q.Trim())
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var result = new List<SearchListingsResponse>();
        foreach (var listing in listings)
        {
            var response = new SearchListingsResponse
            {
                Id = listing.Id,
                MakeName = listing.Variant?.Model?.Make?.Name ?? string.Empty,
                ModelName = listing.Variant?.Model?.Name ?? string.Empty,
                Year = listing.Year,
                Price = listing.Price,
                Mileage = listing.Mileage,
                City = listing.City,
                SellerName = listing.Seller?.Username ?? string.Empty,
            };

            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage != null)
            {
                response = response with
                {
                    FirstImageUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey)
                };
            }

            result.Add(response);
        }

        return result;
    }
}
```

- [ ] **Step 4: Build the solution**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -10
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/SearchListings/
git commit -m "feat: add SearchListings CQRS classes and handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: GetListingComparison — CQRS classes and handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/GetListingComparisonQueryHandler.cs`

- [ ] **Step 1: Create GetListingComparisonQuery.cs**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public sealed record GetListingComparisonQuery : IRequest<GetListingComparisonResponse>
{
    public Guid ListingAId { get; set; }
    public Guid ListingBId { get; set; }
}
```

- [ ] **Step 2: Create GetListingComparisonResponse.cs**

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public sealed record GetListingComparisonResponse
{
    public GetListingByIdResponse ListingA { get; set; } = null!;
    public GetListingByIdResponse ListingB { get; set; } = null!;
}
```

- [ ] **Step 3: Create GetListingComparisonQueryHandler.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public class GetListingComparisonQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService)
    : IRequestHandler<GetListingComparisonQuery, GetListingComparisonResponse>
{
    public async Task<GetListingComparisonResponse> Handle(
        GetListingComparisonQuery request, CancellationToken cancellationToken)
    {
        var listingA = await FetchListingAsync(request.ListingAId, cancellationToken);
        var listingB = await FetchListingAsync(request.ListingBId, cancellationToken);

        return new GetListingComparisonResponse { ListingA = listingA, ListingB = listingB };
    }

    private async Task<GetListingByIdResponse> FetchListingAsync(Guid id, CancellationToken cancellationToken)
    {
        var listing = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Fuel)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Transmission)
            .Include(l => l.Variant)
                .ThenInclude(v => v.BodyType)
            .Include(l => l.Drivetrain)
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), id);

        var response = mapper.Map<GetListingByIdResponse>(listing);

        var images = new List<ImageDto>();
        foreach (var image in listing.Images)
        {
            images.Add(new ImageDto
            {
                Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                AltText = image.AltText
            });
        }
        response.Images = images;

        return response;
    }
}
```

- [ ] **Step 4: Build the solution**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -10
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingComparison/
git commit -m "feat: add GetListingComparison CQRS classes and handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Controller endpoints

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Add usings at the top of ListingController.cs**

Add after the existing using statements:

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;
using Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;
```

- [ ] **Step 2: Append two action methods to the controller body (after the Update action)**

```csharp
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SearchListingsResponse>>> Search(
        [FromQuery] SearchListingsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetListingComparisonResponse>> Compare(
        [FromQuery] Guid a,
        [FromQuery] Guid b,
        CancellationToken cancellationToken)
    {
        var query = new GetListingComparisonQuery { ListingAId = a, ListingBId = b };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
```

These expose `GET /Listing/Search?q=…&limit=…` and `GET /Listing/Compare?a=…&b=…`.
The Compare action takes `a` and `b` as direct params (matching the spec URL `?a={guid}&b={guid}`) and maps them into `GetListingComparisonQuery`.

- [ ] **Step 3: Build the solution**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -10
```

Expected: `Build succeeded` with 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat: expose Search and Compare listing endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Backend tests — SearchListingsQueryHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/SearchListingsQueryHandlerTests.cs`

- [ ] **Step 1: Create SearchListingsQueryHandlerTests.cs**

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class SearchListingsQueryHandlerTests(
    DatabaseFixture<SearchListingsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SearchListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<SearchListingsQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private SearchListingsQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new SearchListingsQueryHandler(repository, _imageStorageService);
    }

    [Fact]
    public async Task Handle_NoResults_ReturnsEmptyList()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(
            new SearchListingsQuery { Q = "nonexistentxyz123" }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MatchesMakeName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().With(m => m.Name, "Toyota").Build();
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

        var result = await handler.Handle(new SearchListingsQuery { Q = "toyota" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.MakeName == "Toyota");
    }

    [Fact]
    public async Task Handle_MatchesModelName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Camry").Build();
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

        var result = await handler.Handle(new SearchListingsQuery { Q = "camry" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.ModelName == "Camry");
    }

    [Fact]
    public async Task Handle_MatchesYear_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

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
            .WithYear(2020)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "2020" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.Year == 2020);
    }

    [Fact]
    public async Task Handle_MatchesSellerName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().With(u => u.Username, "johndoe").Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "johndoe" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.SellerName == "johndoe");
    }

    [Fact]
    public async Task Handle_MatchesListingId_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = Guid.NewGuid();
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
            .WithListing(listingId)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new SearchListingsQuery { Q = listingId.ToString() }, CancellationToken.None);

        result.Should().ContainSingle(r => r.Id == listingId);
    }

    [Fact]
    public async Task Handle_LimitRespected_ReturnsOnlyLimitCount()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().With(m => m.Name, "Honda").Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        await context.AddRangeAsync(make, model, transmission, bodyType, drivetrain);

        for (int i = 0; i < 5; i++)
        {
            var fuel = new FuelBuilder().Build();
            var variant = new VariantBuilder()
                .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
                .Build();
            var seller = new UserBuilder().Build();
            var listing = new ListingBuilder()
                .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
                .Build();
            await context.AddRangeAsync(fuel, variant, seller, listing);
        }
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new SearchListingsQuery { Q = "Honda", Limit = 3 }, CancellationToken.None);

        result.Count().Should().Be(3);
    }
}
```

- [ ] **Step 2: Run SearchListings tests**

```bash
dotnet test --filter "FullyQualifiedName~SearchListingsQueryHandlerTests" ./Automotive.Marketplace.sln 2>&1 | tail -20
```

Expected: All 7 tests pass.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/SearchListingsQueryHandlerTests.cs
git commit -m "test: add SearchListingsQueryHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Backend tests — GetListingComparisonQueryHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonQueryHandlerTests.cs`

- [ ] **Step 1: Create GetListingComparisonQueryHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingComparisonQueryHandlerTests(
    DatabaseFixture<GetListingComparisonQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingComparisonQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingComparisonQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingComparisonQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingComparisonQueryHandler(mapper, repository, _imageStorageService);
    }

    private async Task<(Guid listingId, Guid variantId)> SeedListingAsync(AutomotiveContext context)
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
        return (listing.Id, variant.Id);
    }

    [Fact]
    public async Task Handle_BothListingsExist_ReturnsBothInResponse()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idA, _) = await SeedListingAsync(context);
        var (idB, _) = await SeedListingAsync(context);

        var query = new GetListingComparisonQuery { ListingAId = idA, ListingBId = idB };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ListingA.Should().NotBeNull();
        result.ListingB.Should().NotBeNull();
        result.ListingA.Id.Should().Be(idA);
        result.ListingB.Id.Should().Be(idB);
    }

    [Fact]
    public async Task Handle_ListingAMissing_Throws()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idB, _) = await SeedListingAsync(context);
        var missingId = Guid.NewGuid();

        var query = new GetListingComparisonQuery { ListingAId = missingId, ListingBId = idB };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ListingBMissing_Throws()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idA, _) = await SeedListingAsync(context);
        var missingId = Guid.NewGuid();

        var query = new GetListingComparisonQuery { ListingAId = idA, ListingBId = missingId };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_SameIdForBoth_ReturnsSameListingTwice()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (id, _) = await SeedListingAsync(context);

        var query = new GetListingComparisonQuery { ListingAId = id, ListingBId = id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ListingA.Id.Should().Be(id);
        result.ListingB.Id.Should().Be(id);
    }
}
```

**Note:** The `GetListingByIdResponse` does not include `Id` yet in the mapping — `ListingMappings.cs` maps it from `Listing` but the response class has `Guid Id { get; set; }`. The mapping profile uses `CreateMap<Listing, GetListingByIdResponse>()` which auto-maps `Id` by convention. Verify the test assertion `result.ListingA.Id.Should().Be(idA)` passes; if `Id` is not mapped, add `.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))` to `ListingMappings.cs`.

- [ ] **Step 2: Run GetListingComparison tests**

```bash
dotnet test --filter "FullyQualifiedName~GetListingComparisonQueryHandlerTests" ./Automotive.Marketplace.sln 2>&1 | tail -20
```

Expected: All 4 tests pass.

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetListingComparisonQueryHandlerTests.cs
git commit -m "test: add GetListingComparisonQueryHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Extend GetListingByIdResponse + update endpoints and query keys

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/listingKeys.ts`

- [ ] **Step 1: Add `id` and `status` to GetListingByIdResponse.ts**

Replace the file contents with:

```typescript
export type GetListingByIdResponse = {
  id: string;
  makeName: string;
  modelName: string;
  price: number;
  description?: string;
  colour?: string;
  vin?: string;
  powerKw: number;
  engineSizeMl: number;
  mileage: number;
  isSteeringWheelRight: boolean;
  city: string;
  isUsed: boolean;
  year: number;
  transmissionName: string;
  fuelName: string;
  doorCount: number;
  bodyTypeName: string;
  drivetrainName: string;
  sellerName: string;
  sellerId: string;
  status: string;
  images: Image[];
};

type Image = {
  url: string;
  altText: string;
};
```

- [ ] **Step 2: Add SEARCH and COMPARE to endpoints.ts**

In `LISTING` object, add after `UPDATE`:

```typescript
    SEARCH: "/Listing/Search",
    COMPARE: "/Listing/Compare",
```

- [ ] **Step 3: Add `comparison` and `search` keys to listingKeys.ts**

Replace the file contents with:

```typescript
import { GetAllListingsQuery } from "@/features/listingList";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
  byId: (id: string) => [...listingKeys.all(), id],
  comparison: (a: string, b: string) => [...listingKeys.all(), "comparison", a, b],
  search: (q: string) => [...listingKeys.all(), "search", q],
};
```

- [ ] **Step 4: Build the frontend to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -20
```

Expected: Build succeeds with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add \
  automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts \
  automotive.marketplace.client/src/constants/endpoints.ts \
  automotive.marketplace.client/src/api/queryKeys/listingKeys.ts
git commit -m "feat: extend GetListingByIdResponse and add comparison endpoints/keys

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Frontend test setup (Vitest)

**Files:**
- Modify: `automotive.marketplace.client/package.json`
- Modify: `automotive.marketplace.client/vite.config.ts`
- Create: `automotive.marketplace.client/src/test/setup.ts`

- [ ] **Step 1: Install test dependencies**

```bash
cd automotive.marketplace.client && npm install -D vitest @testing-library/react @testing-library/user-event @testing-library/jest-dom jsdom @vitejs/plugin-react
```

- [ ] **Step 2: Add test script to package.json**

In `package.json`, inside the `"scripts"` object, add:

```json
"test": "vitest run",
"test:watch": "vitest"
```

- [ ] **Step 3: Add test config to vite.config.ts**

Add `/// <reference types="vitest" />` at the top of the file (line 1), then inside `defineConfig(({ mode }) => { return { ... } })`, add a `test` key at the same level as `plugins`:

```typescript
    test: {
      globals: true,
      environment: "jsdom",
      setupFiles: ["./src/test/setup.ts"],
    },
```

- [ ] **Step 4: Create src/test/setup.ts**

```typescript
import "@testing-library/jest-dom";
```

- [ ] **Step 5: Verify test runner works**

```bash
cd automotive.marketplace.client && npm test 2>&1 | tail -10
```

Expected: `No test files found` (or 0 tests run) — confirms vitest is configured and can run.

- [ ] **Step 6: Commit**

```bash
git add \
  automotive.marketplace.client/package.json \
  automotive.marketplace.client/package-lock.json \
  automotive.marketplace.client/vite.config.ts \
  automotive.marketplace.client/src/test/setup.ts
git commit -m "chore: add vitest + React Testing Library test setup

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: computeDiff — types, failing tests, implementation

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/types/diff.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/utils/computeDiff.test.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/utils/computeDiff.ts`

- [ ] **Step 1: Create diff.ts**

```typescript
export type DiffResult = "equal" | "a-better" | "b-better" | "different";
export type DiffMap = Record<string, DiffResult>;
```

- [ ] **Step 2: Create computeDiff.test.ts (failing — computeDiff.ts does not exist yet)**

```typescript
import { describe, it, expect } from "vitest";
import { computeDiff } from "./computeDiff";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

const base: GetListingByIdResponse = {
  id: "a",
  makeName: "Toyota",
  modelName: "Camry",
  price: 15000,
  powerKw: 120,
  engineSizeMl: 1998,
  mileage: 50000,
  isSteeringWheelRight: false,
  city: "Vilnius",
  isUsed: true,
  year: 2020,
  transmissionName: "Automatic",
  fuelName: "Petrol",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "FWD",
  sellerName: "John",
  sellerId: "s1",
  status: "Available",
  images: [],
};

describe("computeDiff", () => {
  it("returns equal for all fields when listings are identical", () => {
    const diff = computeDiff(base, { ...base });
    expect(diff.makeName).toBe("equal");
    expect(diff.price).toBe("equal");
    expect(diff.powerKw).toBe("equal");
    expect(diff.mileage).toBe("equal");
  });

  it("returns a-better when A has higher powerKw (higher-is-better)", () => {
    const b = { ...base, powerKw: 80 };
    const diff = computeDiff(base, b);
    expect(diff.powerKw).toBe("a-better");
  });

  it("returns b-better when B has higher powerKw (higher-is-better)", () => {
    const b = { ...base, powerKw: 200 };
    const diff = computeDiff(base, b);
    expect(diff.powerKw).toBe("b-better");
  });

  it("returns a-better when A has higher year (higher-is-better)", () => {
    const b = { ...base, year: 2015 };
    const diff = computeDiff(base, b);
    expect(diff.year).toBe("a-better");
  });

  it("returns b-better when B has higher year (higher-is-better)", () => {
    const b = { ...base, year: 2024 };
    const diff = computeDiff(base, b);
    expect(diff.year).toBe("b-better");
  });

  it("returns a-better when A has lower mileage (lower-is-better)", () => {
    const b = { ...base, mileage: 150000 };
    const diff = computeDiff(base, b);
    expect(diff.mileage).toBe("a-better");
  });

  it("returns b-better when B has lower mileage (lower-is-better)", () => {
    const b = { ...base, mileage: 10000 };
    const diff = computeDiff(base, b);
    expect(diff.mileage).toBe("b-better");
  });

  it("returns a-better when A has lower price (lower-is-better)", () => {
    const b = { ...base, price: 30000 };
    const diff = computeDiff(base, b);
    expect(diff.price).toBe("a-better");
  });

  it("returns b-better when B has lower price (lower-is-better)", () => {
    const b = { ...base, price: 5000 };
    const diff = computeDiff(base, b);
    expect(diff.price).toBe("b-better");
  });

  it("returns different for string field when values differ", () => {
    const b = { ...base, makeName: "Honda" };
    const diff = computeDiff(base, b);
    expect(diff.makeName).toBe("different");
  });

  it("returns equal for string field when values are the same", () => {
    const diff = computeDiff(base, { ...base });
    expect(diff.makeName).toBe("equal");
  });

  it("returns different for engineSizeMl when values differ (no direction)", () => {
    const b = { ...base, engineSizeMl: 2500 };
    const diff = computeDiff(base, b);
    expect(diff.engineSizeMl).toBe("different");
  });

  it("does not include images array in diff map", () => {
    const b = { ...base, images: [{ url: "x", altText: "y" }] };
    const diff = computeDiff(base, b);
    expect(diff.images).toBeUndefined();
  });
});
```

- [ ] **Step 3: Run tests — verify they fail**

```bash
cd automotive.marketplace.client && npm test -- src/features/compareListings/utils/computeDiff.test.ts 2>&1 | tail -15
```

Expected: FAIL — module not found for `./computeDiff`.

- [ ] **Step 4: Create computeDiff.ts**

```typescript
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap, DiffResult } from "../types/diff";

const HIGHER_IS_BETTER = new Set<keyof GetListingByIdResponse>(["powerKw", "year"]);
const LOWER_IS_BETTER = new Set<keyof GetListingByIdResponse>(["mileage", "price"]);

export function computeDiff(
  a: GetListingByIdResponse,
  b: GetListingByIdResponse,
): DiffMap {
  const result: DiffMap = {};

  for (const field of HIGHER_IS_BETTER) {
    const aVal = a[field] as number;
    const bVal = b[field] as number;
    result[field] = aVal === bVal ? "equal" : aVal > bVal ? "a-better" : "b-better";
  }

  for (const field of LOWER_IS_BETTER) {
    const aVal = a[field] as number;
    const bVal = b[field] as number;
    result[field] = aVal === bVal ? "equal" : aVal < bVal ? "a-better" : "b-better";
  }

  const allFields = Object.keys(a) as (keyof GetListingByIdResponse)[];
  for (const field of allFields) {
    if (field in result) continue;
    const aVal = a[field];
    const bVal = b[field];
    if (Array.isArray(aVal) || Array.isArray(bVal)) continue;
    const diff: DiffResult = String(aVal) === String(bVal) ? "equal" : "different";
    result[field] = diff;
  }

  return result;
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
cd automotive.marketplace.client && npm test -- src/features/compareListings/utils/computeDiff.test.ts 2>&1 | tail -10
```

Expected: All 13 tests pass.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/
git commit -m "feat: add computeDiff utility with unit tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: compareListings types and API options

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/types/GetListingComparisonResponse.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/types/SearchListingsResponse.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/api/getListingComparisonOptions.ts`
- Create: `automotive.marketplace.client/src/features/compareListings/api/searchListingsOptions.ts`

- [ ] **Step 1: Create GetListingComparisonResponse.ts**

```typescript
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

export type GetListingComparisonResponse = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
};
```

- [ ] **Step 2: Create SearchListingsResponse.ts**

```typescript
export type SearchListingsResponse = {
  id: string;
  makeName: string;
  modelName: string;
  year: number;
  price: number;
  mileage: number;
  city: string;
  sellerName: string;
  firstImageUrl?: string;
};
```

- [ ] **Step 3: Create getListingComparisonOptions.ts**

```typescript
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonResponse } from "../types/GetListingComparisonResponse";

const getListingComparison = (a: string, b: string) =>
  axiosClient.get<GetListingComparisonResponse>(ENDPOINTS.LISTING.COMPARE, {
    params: { a, b },
  });

export const getListingComparisonOptions = (a: string, b: string) =>
  queryOptions({
    queryKey: listingKeys.comparison(a, b),
    queryFn: () => getListingComparison(a, b),
  });
```

- [ ] **Step 4: Create searchListingsOptions.ts**

```typescript
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { SearchListingsResponse } from "../types/SearchListingsResponse";

const searchListings = (q: string) =>
  axiosClient.get<SearchListingsResponse[]>(ENDPOINTS.LISTING.SEARCH, {
    params: { q },
  });

export const searchListingsOptions = (q: string) =>
  queryOptions({
    queryKey: listingKeys.search(q),
    queryFn: () => searchListings(q),
    enabled: q.trim().length > 0,
  });
```

- [ ] **Step 5: Build to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```

Expected: Build succeeds with 0 errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/
git commit -m "feat: add compareListings types and API options

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: CompareRow and CompareTable — failing tests, then implementation

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.test.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareRow.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareTable.tsx`

- [ ] **Step 1: Create CompareTable.test.tsx (failing — components don't exist yet)**

```typescript
import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { CompareTable } from "./CompareTable";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap } from "../types/diff";

const listingA: GetListingByIdResponse = {
  id: "a1",
  makeName: "Toyota",
  modelName: "Camry",
  price: 15000,
  powerKw: 120,
  engineSizeMl: 1998,
  mileage: 50000,
  isSteeringWheelRight: false,
  city: "Vilnius",
  isUsed: true,
  year: 2020,
  transmissionName: "Automatic",
  fuelName: "Petrol",
  doorCount: 4,
  bodyTypeName: "Sedan",
  drivetrainName: "FWD",
  sellerName: "John",
  sellerId: "s1",
  status: "Available",
  images: [],
};

const listingB: GetListingByIdResponse = {
  ...listingA,
  id: "b1",
  makeName: "Honda",
  powerKw: 100,
  mileage: 80000,
};

const allEqualDiff: DiffMap = {
  makeName: "equal",
  modelName: "equal",
  bodyTypeName: "equal",
  year: "equal",
  isUsed: "equal",
  mileage: "equal",
  city: "equal",
  powerKw: "equal",
  engineSizeMl: "equal",
  fuelName: "equal",
  transmissionName: "equal",
  drivetrainName: "equal",
  price: "equal",
  status: "equal",
  sellerName: "equal",
};

const mixedDiff: DiffMap = {
  ...allEqualDiff,
  makeName: "different",
  powerKw: "a-better",
  mileage: "a-better",
};

describe("CompareTable", () => {
  it("renders all section headings when diffOnly is false", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={allEqualDiff}
        diffOnly={false}
      />,
    );
    expect(screen.getByText("Basic Info")).toBeInTheDocument();
    expect(screen.getByText("Engine & Performance")).toBeInTheDocument();
    expect(screen.getByText("Listing Details")).toBeInTheDocument();
  });

  it("renders row labels for all spec fields when diffOnly is false", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={allEqualDiff}
        diffOnly={false}
      />,
    );
    expect(screen.getByText("Make")).toBeInTheDocument();
    expect(screen.getByText("Power (kW)")).toBeInTheDocument();
    expect(screen.getByText("Price")).toBeInTheDocument();
  });

  it("hides rows where diff is equal when diffOnly is true", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={true}
      />,
    );
    expect(screen.queryByText("Model")).not.toBeInTheDocument();
    expect(screen.getByText("Make")).toBeInTheDocument();
    expect(screen.getByText("Power (kW)")).toBeInTheDocument();
    expect(screen.getByText("Mileage")).toBeInTheDocument();
  });

  it("applies green class to the better cell for a-better numeric field", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={false}
      />,
    );
    // powerKw: a-better — A cell (120 kW) should have green class
    const cells = screen.getAllByText(/120/);
    const betterCell = cells[0].closest("td");
    expect(betterCell?.className).toMatch(/text-green/);
  });

  it("applies orange class to the worse cell for a-better numeric field", () => {
    render(
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={mixedDiff}
        diffOnly={false}
      />,
    );
    // powerKw: a-better — B cell (100 kW) should have orange class
    const cells = screen.getAllByText(/100/);
    const worseCell = cells[0].closest("td");
    expect(worseCell?.className).toMatch(/text-orange/);
  });
});
```

- [ ] **Step 2: Run tests — verify they fail**

```bash
cd automotive.marketplace.client && npm test -- src/features/compareListings/components/CompareTable.test.tsx 2>&1 | tail -10
```

Expected: FAIL — module not found.

- [ ] **Step 3: Create CompareRow.tsx**

```typescript
import type { DiffResult } from "../types/diff";

type CompareRowProps = {
  label: string;
  valueA: string;
  valueB: string;
  diff: DiffResult;
};

const getCellClass = (side: "a" | "b", diff: DiffResult): string => {
  if (diff === "a-better")
    return side === "a" ? "text-green-600 font-semibold" : "text-orange-600";
  if (diff === "b-better")
    return side === "b" ? "text-green-600 font-semibold" : "text-orange-600";
  if (diff === "different") return "bg-amber-50 dark:bg-amber-950/20";
  return "";
};

const getArrow = (side: "a" | "b", diff: DiffResult): string => {
  if (diff === "a-better") return side === "a" ? " ↑" : " ↓";
  if (diff === "b-better") return side === "b" ? " ↑" : " ↓";
  return "";
};

const getRowStyle = (diff: DiffResult): React.CSSProperties =>
  diff !== "equal" ? { backgroundColor: "rgba(249,115,22,0.05)" } : {};

export const CompareRow = ({ label, valueA, valueB, diff }: CompareRowProps) => (
  <tr
    style={getRowStyle(diff)}
    className="divide-x divide-border border-b border-border"
  >
    <td className="px-4 py-3 text-sm font-medium text-muted-foreground">{label}</td>
    <td className={`px-4 py-3 text-sm text-center ${getCellClass("a", diff)}`}>
      {valueA}
      {getArrow("a", diff)}
    </td>
    <td className={`px-4 py-3 text-sm text-center ${getCellClass("b", diff)}`}>
      {valueB}
      {getArrow("b", diff)}
    </td>
  </tr>
);
```

- [ ] **Step 4: Create CompareTable.tsx**

```typescript
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap } from "../types/diff";
import { CompareRow } from "./CompareRow";

type RowSpec = {
  field: keyof GetListingByIdResponse;
  label: string;
  format?: (value: GetListingByIdResponse[keyof GetListingByIdResponse]) => string;
};

type SectionSpec = {
  label: string;
  rows: RowSpec[];
};

const TABLE_SECTIONS: SectionSpec[] = [
  {
    label: "Basic Info",
    rows: [
      { field: "makeName", label: "Make" },
      { field: "modelName", label: "Model" },
      { field: "bodyTypeName", label: "Body Type" },
      { field: "year", label: "Year" },
      { field: "isUsed", label: "Condition", format: (v) => (v ? "Used" : "New") },
      {
        field: "mileage",
        label: "Mileage",
        format: (v) => `${(v as number).toLocaleString()} km`,
      },
      { field: "city", label: "City" },
    ],
  },
  {
    label: "Engine & Performance",
    rows: [
      { field: "powerKw", label: "Power (kW)" },
      { field: "engineSizeMl", label: "Engine Size (ml)" },
      { field: "fuelName", label: "Fuel Type" },
      { field: "transmissionName", label: "Transmission" },
      { field: "drivetrainName", label: "Drivetrain" },
    ],
  },
  {
    label: "Listing Details",
    rows: [
      {
        field: "price",
        label: "Price",
        format: (v) => `${(v as number).toFixed(0)} €`,
      },
      { field: "status", label: "Status" },
      { field: "sellerName", label: "Seller" },
    ],
  },
];

type CompareTableProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
  diffMap: DiffMap;
  diffOnly: boolean;
};

export const CompareTable = ({
  listingA,
  listingB,
  diffMap,
  diffOnly,
}: CompareTableProps) => (
  <div className="mt-4 overflow-x-auto">
    <table className="w-full border-collapse">
      <tbody>
        {TABLE_SECTIONS.flatMap((section) => {
          const visibleRows = diffOnly
            ? section.rows.filter((r) => diffMap[r.field] !== "equal")
            : section.rows;
          if (visibleRows.length === 0) return [];
          return [
            <tr key={`section-${section.label}`} className="bg-muted">
              <td colSpan={3} className="px-4 py-2 text-sm font-semibold">
                {section.label}
              </td>
            </tr>,
            ...visibleRows.map((row) => {
              const valA = listingA[row.field];
              const valB = listingB[row.field];
              const displayA = row.format
                ? row.format(valA)
                : String(valA ?? "—");
              const displayB = row.format
                ? row.format(valB)
                : String(valB ?? "—");
              return (
                <CompareRow
                  key={row.field}
                  label={row.label}
                  valueA={displayA}
                  valueB={displayB}
                  diff={diffMap[row.field] ?? "equal"}
                />
              );
            }),
          ];
        })}
      </tbody>
    </table>
  </div>
);
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
cd automotive.marketplace.client && npm test -- src/features/compareListings/components/CompareTable.test.tsx 2>&1 | tail -10
```

Expected: All 5 tests pass.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/
git commit -m "feat: add CompareRow and CompareTable components with tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: CompareHeader, DiffToggleFab, CompareSearchModal

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareHeader.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/DiffToggleFab.tsx`
- Create: `automotive.marketplace.client/src/features/compareListings/components/CompareSearchModal.tsx`

- [ ] **Step 1: Create CompareHeader.tsx**

```typescript
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

type CompareHeaderProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
};

const ListingCard = ({ listing }: { listing: GetListingByIdResponse }) => (
  <div className="text-center">
    <img
      src={
        listing.images[0]?.url ??
        "https://placehold.co/200x150?text=No+Image"
      }
      alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
      className="mx-auto h-32 w-48 rounded object-cover"
    />
    <p className="mt-2 font-semibold">
      {listing.year} {listing.makeName} {listing.modelName}
    </p>
    <p className="text-primary font-bold">{listing.price.toFixed(0)} €</p>
    <p className="text-sm text-muted-foreground">{listing.city}</p>
  </div>
);

export const CompareHeader = ({ listingA, listingB }: CompareHeaderProps) => (
  <div className="sticky top-0 z-10 mb-4 grid grid-cols-3 rounded-lg border bg-background p-4 shadow-sm">
    <div className="flex items-center">
      <span className="text-sm font-semibold text-muted-foreground">
        Specification
      </span>
    </div>
    <ListingCard listing={listingA} />
    <ListingCard listing={listingB} />
  </div>
);
```

- [ ] **Step 2: Create DiffToggleFab.tsx**

```typescript
import { Button } from "@/components/ui/button";

type DiffToggleFabProps = {
  active: boolean;
  onToggle: () => void;
};

export const DiffToggleFab = ({ active, onToggle }: DiffToggleFabProps) => (
  <Button
    onClick={onToggle}
    variant={active ? "default" : "outline"}
    className="fixed bottom-6 right-6 z-20 shadow-lg"
  >
    {active ? "Show All" : "Diff Only"}
  </Button>
);
```

- [ ] **Step 3: Create CompareSearchModal.tsx**

```typescript
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { router } from "@/lib/router";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { searchListingsOptions } from "../api/searchListingsOptions";

type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  currentListingId: string;
};

export const CompareSearchModal = ({
  open,
  onClose,
  currentListingId,
}: CompareSearchModalProps) => {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedQuery(query), 300);
    return () => clearTimeout(timer);
  }, [query]);

  const { data } = useQuery(searchListingsOptions(debouncedQuery));
  const results = (data?.data ?? []).filter((r) => r.id !== currentListingId);

  const handleCompare = (selectedId: string) => {
    onClose();
    router.navigate({
      to: "/compare",
      search: { a: currentListingId, b: selectedId },
    });
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Compare with another listing</DialogTitle>
        </DialogHeader>
        <Input
          placeholder="Search by make, model, year, seller…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoFocus
        />
        <div className="mt-4 max-h-96 space-y-2 overflow-y-auto">
          {results.map((listing) => (
            <div
              key={listing.id}
              className="flex items-center gap-3 rounded-lg border p-3"
            >
              <img
                src={
                  listing.firstImageUrl ??
                  "https://placehold.co/80x60?text=No+Image"
                }
                alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
                className="h-14 w-20 rounded object-cover"
              />
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">
                  {listing.year} {listing.makeName} {listing.modelName}
                </p>
                <p className="text-muted-foreground text-sm">
                  {listing.price.toFixed(0)} € ·{" "}
                  {listing.mileage.toLocaleString()} km · {listing.city}
                </p>
                <p className="text-muted-foreground text-sm">
                  {listing.sellerName}
                </p>
              </div>
              <Button size="sm" onClick={() => handleCompare(listing.id)}>
                Compare
              </Button>
            </div>
          ))}
          {debouncedQuery && results.length === 0 && (
            <p className="text-muted-foreground py-4 text-center">
              No results found
            </p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
```

- [ ] **Step 4: Build to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```

Expected: Build succeeds with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/compareListings/components/
git commit -m "feat: add CompareHeader, DiffToggleFab, CompareSearchModal components

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 12: compareListings index, route, and Compare page

**Files:**
- Create: `automotive.marketplace.client/src/features/compareListings/index.ts`
- Create: `automotive.marketplace.client/src/app/routes/compare.tsx`
- Create: `automotive.marketplace.client/src/app/pages/Compare.tsx`

- [ ] **Step 1: Create compareListings/index.ts**

```typescript
export { CompareHeader } from "./components/CompareHeader";
export { CompareTable } from "./components/CompareTable";
export { CompareSearchModal } from "./components/CompareSearchModal";
export { DiffToggleFab } from "./components/DiffToggleFab";
```

- [ ] **Step 2: Create src/app/routes/compare.tsx**

```typescript
import { createFileRoute, redirect } from "@tanstack/react-router";
import Compare from "../pages/Compare";

const UUID_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const Route = createFileRoute("/compare")({
  validateSearch: (search) => {
    const a = search["a"];
    const b = search["b"];
    if (
      typeof a !== "string" ||
      typeof b !== "string" ||
      !UUID_REGEX.test(a) ||
      !UUID_REGEX.test(b)
    ) {
      throw redirect({ to: "/" });
    }
    return { a, b };
  },
  component: Compare,
});
```

- [ ] **Step 3: Create src/app/pages/Compare.tsx**

```typescript
import { getListingComparisonOptions } from "@/features/compareListings/api/getListingComparisonOptions";
import {
  CompareHeader,
  CompareTable,
  DiffToggleFab,
} from "@/features/compareListings";
import { computeDiff } from "@/features/compareListings/utils/computeDiff";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Route } from "@/app/routes/compare";

const Compare = () => {
  const { a, b } = Route.useSearch();
  const [diffOnly, setDiffOnly] = useState(false);

  const { data: response, isLoading, isError } = useQuery(
    getListingComparisonOptions(a, b),
  );

  if (isLoading) {
    return (
      <div className="py-8">
        <div className="mb-4 h-48 animate-pulse rounded-lg bg-muted" />
        <div className="space-y-2">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="h-10 animate-pulse rounded bg-muted" />
          ))}
        </div>
      </div>
    );
  }

  if (isError || !response) {
    return (
      <div className="py-16 text-center">
        <p className="text-muted-foreground">
          One or more listings could not be found.
        </p>
      </div>
    );
  }

  const { listingA, listingB } = response.data;
  const diffMap = computeDiff(listingA, listingB);

  return (
    <div className="py-8">
      <CompareHeader listingA={listingA} listingB={listingB} />
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={diffMap}
        diffOnly={diffOnly}
      />
      <DiffToggleFab active={diffOnly} onToggle={() => setDiffOnly((d) => !d)} />
    </div>
  );
};

export default Compare;
```

- [ ] **Step 4: Build to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```

Expected: Build succeeds with 0 errors. TanStack Router's code gen will regenerate the route tree to include the new `/compare` route.

- [ ] **Step 5: Commit**

```bash
git add \
  automotive.marketplace.client/src/features/compareListings/index.ts \
  automotive.marketplace.client/src/app/routes/compare.tsx \
  automotive.marketplace.client/src/app/pages/Compare.tsx \
  automotive.marketplace.client/src/routeTree.gen.ts
git commit -m "feat: add compare route and page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: Wire up "Compare" button to ListingDetailsContent

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Add import for CompareSearchModal**

After the existing imports at the top of `ListingDetailsContent.tsx`, add:

```typescript
import { CompareSearchModal } from "@/features/compareListings";
```

- [ ] **Step 2: Add compareModalOpen state**

After the existing `const [chatConversation, ...` state declaration, add:

```typescript
const [compareModalOpen, setCompareModalOpen] = useState(false);
```

- [ ] **Step 3: Add Compare button after the Contact Seller button**

After the closing `}` of the `{userId && !isSeller && (...)}` block (the Contact Seller button), add:

```typescript
              <Button
                variant="outline"
                className="mt-2 w-full"
                onClick={() => setCompareModalOpen(true)}
              >
                Compare with another listing
              </Button>
```

- [ ] **Step 4: Add CompareSearchModal at the bottom of the JSX (after the ChatPanel)**

After `{chatConversation && (<ChatPanel ... />)}`, add:

```typescript
      <CompareSearchModal
        open={compareModalOpen}
        onClose={() => setCompareModalOpen(false)}
        currentListingId={id}
      />
```

- [ ] **Step 5: Build to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```

Expected: Build succeeds with 0 errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx
git commit -m "feat: add Compare button to listing detail page

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 14: Compare page integration test

**Files:**
- Create: `automotive.marketplace.client/src/app/pages/Compare.test.tsx`

- [ ] **Step 1: Create Compare.test.tsx**

```typescript
import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import Compare from "./Compare";
import type { GetListingComparisonResponse } from "@/features/compareListings/types/GetListingComparisonResponse";

// Mock the route to supply search params
vi.mock("@/app/routes/compare", () => ({
  Route: {
    useSearch: () => ({ a: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", b: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" }),
  },
}));

const mockComparison: GetListingComparisonResponse = {
  listingA: {
    id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    makeName: "Toyota",
    modelName: "Camry",
    price: 15000,
    powerKw: 120,
    engineSizeMl: 1998,
    mileage: 50000,
    isSteeringWheelRight: false,
    city: "Vilnius",
    isUsed: true,
    year: 2020,
    transmissionName: "Automatic",
    fuelName: "Petrol",
    doorCount: 4,
    bodyTypeName: "Sedan",
    drivetrainName: "FWD",
    sellerName: "John",
    sellerId: "s1",
    status: "Available",
    images: [],
  },
  listingB: {
    id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    makeName: "Honda",
    modelName: "Civic",
    price: 12000,
    powerKw: 100,
    engineSizeMl: 1500,
    mileage: 80000,
    isSteeringWheelRight: false,
    city: "Kaunas",
    isUsed: true,
    year: 2018,
    transmissionName: "Manual",
    fuelName: "Petrol",
    doorCount: 4,
    bodyTypeName: "Sedan",
    drivetrainName: "FWD",
    sellerName: "Jane",
    sellerId: "s2",
    status: "Available",
    images: [],
  },
};

// Mock the API options so useQuery returns our fixture
vi.mock("@/features/compareListings/api/getListingComparisonOptions", () => ({
  getListingComparisonOptions: (_a: string, _b: string) => ({
    queryKey: ["listing", "comparison", _a, _b],
    queryFn: async () => ({ data: mockComparison }),
  }),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("Compare page", () => {
  it("renders comparison table with both listing names after data loads", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getByText("Toyota")).toBeInTheDocument();
      expect(screen.getByText("Honda")).toBeInTheDocument();
    });
  });

  it("renders all three table sections", async () => {
    render(<Compare />, { wrapper: createWrapper() });

    await waitFor(() => {
      expect(screen.getByText("Basic Info")).toBeInTheDocument();
      expect(screen.getByText("Engine & Performance")).toBeInTheDocument();
      expect(screen.getByText("Listing Details")).toBeInTheDocument();
    });
  });

  it("shows error message when query errors", async () => {
    vi.mock("@/features/compareListings/api/getListingComparisonOptions", () => ({
      getListingComparisonOptions: () => ({
        queryKey: ["listing", "comparison", "err"],
        queryFn: async () => { throw new Error("404"); },
      }),
    }));

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    render(
      <QueryClientProvider client={queryClient}>
        <Compare />
      </QueryClientProvider>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(/one or more listings could not be found/i),
      ).toBeInTheDocument();
    });
  });
});
```

- [ ] **Step 2: Run all frontend tests**

```bash
cd automotive.marketplace.client && npm test 2>&1 | tail -20
```

Expected: All tests pass (computeDiff × 13, CompareTable × 5, Compare × 3 = 21 total).

- [ ] **Step 3: Run full backend test suite**

```bash
dotnet test ./Automotive.Marketplace.sln 2>&1 | tail -20
```

Expected: All tests pass.

- [ ] **Step 4: Final build check**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
dotnet build ./Automotive.Marketplace.sln --no-restore 2>&1 | tail -10
```

Expected: Both succeed with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/app/pages/Compare.test.tsx
git commit -m "test: add Compare page integration test

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
