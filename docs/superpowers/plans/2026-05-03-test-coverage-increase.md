# Test Coverage Increase — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Increase test coverage from Application 71% → 80%+, Server 0% → 30%+, Infrastructure 2.5% → 30%+ by writing integration and unit tests following existing patterns.

**Architecture:** Layer-by-layer testing — Phase 1 covers 28 untested Application CQRS handlers (integration tests using DatabaseFixture + TestContainers), Phase 2 covers Server exception filters, middleware, and background services (unit tests with NSubstitute mocks), Phase 3 covers Infrastructure domain services and API clients (unit tests with MockHttpMessageHandler).

**Tech Stack:** xUnit, FluentAssertions, NSubstitute, TestContainers.PostgreSql, Respawn, Bogus builders, AutoMapper

---

## File Structure

### Phase 1 — Application Layer Integration Tests

| Status | File | Responsibility |
|--------|------|---------------|
| NEW | `Tests/Features/HandlerTests/AuthHandlerTests/LoginUserCommandHandlerTests.cs` | Login happy path + invalid email + wrong password |
| NEW | `Tests/Features/HandlerTests/AuthHandlerTests/LogoutUserCommandHandlerTests.cs` | Logout with valid token + expired/revoked token |
| NEW | `Tests/Features/HandlerTests/AuthHandlerTests/RefreshTokenCommandHandlerTests.cs` | Refresh happy path + expired + revoked |
| NEW | `Tests/Features/HandlerTests/ListingHandlerTests/DeleteListingCommandHandlerTests.cs` | Owner delete + non-owner forbidden |
| NEW | `Tests/Features/HandlerTests/ListingHandlerTests/GetListingByIdQueryHandlerTests.cs` | Existing listing + not found |
| NEW | `Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingCommandHandlerTests.cs` | Owner update + non-owner forbidden |
| NEW | `Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingStatusCommandHandlerTests.cs` | Valid status change + invalid status + non-owner |
| NEW | `Tests/Features/HandlerTests/ListingHandlerTests/ReactivateListingCommandHandlerTests.cs` | OnHold → Available + not OnHold error + active contract error |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/CreateModelCommandHandlerTests.cs` | Creates model in DB |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/DeleteModelCommandHandlerTests.cs` | Deletes existing model |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/GetAllModelsQueryHandlerTests.cs` | Returns all sorted |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/GetModelByIdQueryHandlerTests.cs` | Existing + not found |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/GetModelsByMakeIdQueryHandlerTests.cs` | Filtered by make + OnlyWithListings filter |
| NEW | `Tests/Features/HandlerTests/ModelHandlerTests/UpdateModelCommandHandlerTests.cs` | Updates model |
| NEW | `Tests/Features/HandlerTests/VariantHandlerTests/CreateVariantCommandHandlerTests.cs` | Creates variant |
| NEW | `Tests/Features/HandlerTests/VariantHandlerTests/DeleteVariantCommandHandlerTests.cs` | Deletes variant |
| NEW | `Tests/Features/HandlerTests/VariantHandlerTests/GetVariantsByModelQueryHandlerTests.cs` | Filtered by model |
| NEW | `Tests/Features/HandlerTests/VariantHandlerTests/UpdateVariantCommandHandlerTests.cs` | Updates variant |
| NEW | `Tests/Features/HandlerTests/ChatHandlerTests/CancelOfferCommandHandlerTests.cs` | Cancel pending offer |
| NEW | `Tests/Features/HandlerTests/ChatHandlerTests/GetUnreadCountQueryHandlerTests.cs` | Correct unread count |
| NEW | `Tests/Features/HandlerTests/ChatHandlerTests/GetUserContractProfileQueryHandlerTests.cs` | Existing profile + no profile |
| NEW | `Tests/Features/HandlerTests/ChatHandlerTests/UpdateUserContractProfileCommandHandlerTests.cs` | Upserts profile |
| NEW | `Tests/Features/HandlerTests/DefectHandlerTests/AddListingDefectCommandHandlerTests.cs` | Adds defect |
| NEW | `Tests/Features/HandlerTests/DefectHandlerTests/AddDefectImageCommandHandlerTests.cs` | Adds defect image |
| NEW | `Tests/Features/HandlerTests/DefectHandlerTests/GetDefectCategoriesQueryHandlerTests.cs` | Returns categories |
| NEW | `Tests/Features/HandlerTests/DefectHandlerTests/RemoveDefectImageCommandHandlerTests.cs` | Removes image |
| NEW | `Tests/Features/HandlerTests/DefectHandlerTests/RemoveListingDefectCommandHandlerTests.cs` | Removes defect |
| NEW | `Tests/Features/HandlerTests/LookupHandlerTests/GetAllBodyTypesQueryHandlerTests.cs` | Returns body types |
| NEW | `Tests/Features/HandlerTests/LookupHandlerTests/GetAllDrivetrainsQueryHandlerTests.cs` | Returns drivetrains |
| NEW | `Tests/Features/HandlerTests/LookupHandlerTests/GetAllFuelsQueryHandlerTests.cs` | Returns fuels |
| NEW | `Tests/Features/HandlerTests/LookupHandlerTests/GetAllTransmissionsQueryHandlerTests.cs` | Returns transmissions |
| NEW | `Tests/Features/HandlerTests/DashboardHandlerTests/GetDashboardSummaryQueryHandlerTests.cs` | Returns summary |

### Phase 2 — Server Layer Unit Tests

| Status | File | Responsibility |
|--------|------|---------------|
| NEW | `Tests/Features/UnitTests/FilterTests/ValidationExceptionFilterTests.cs` | RequestValidationException → 400 |
| NEW | `Tests/Features/UnitTests/FilterTests/NotFoundExceptionFilterTests.cs` | DbEntityNotFoundException → 404 |
| NEW | `Tests/Features/UnitTests/FilterTests/UnauthorizedExceptionFilterTests.cs` | Auth exceptions → 401 |
| NEW | `Tests/Features/UnitTests/FilterTests/ForbiddenExceptionFilterTests.cs` | UnauthorizedAccessException → 403 |
| NEW | `Tests/Features/UnitTests/FilterTests/AuthorizationFilterTests.cs` | Permission checks → pass/401/403 |
| NEW | `Tests/Features/UnitTests/MiddlewareTests/GlobalExceptionHandlerTests.cs` | Unhandled → ProblemDetails |
| NEW | `Tests/Features/UnitTests/ServiceTests/PasswordHasherTests.cs` | Hash + verify |
| NEW | `Tests/Features/UnitTests/ServiceTests/TokenServiceTests.cs` | AccessToken + RefreshToken claims |

### Phase 3 — Infrastructure Layer Unit Tests

| Status | File | Responsibility |
|--------|------|---------------|
| NEW | `Tests/Infrastructure/MockHttpMessageHandler.cs` | Shared test helper for API client tests |
| NEW | `Tests/Features/UnitTests/ApiClientTests/CardogApiClientTests.cs` | Market overview → result or null |
| NEW | `Tests/Features/UnitTests/ApiClientTests/OpenAiClientTests.cs` | Prompt → response or null |
| NEW | `Tests/Features/UnitTests/ApiClientTests/NhtsaApiClientTests.cs` | Recalls + complaints + safety |
| NEW | `Tests/Features/UnitTests/ApiClientTests/FuelEconomyApiClientTests.cs` | Fuel efficiency parsing |
| NEW | `Tests/Features/UnitTests/ApiClientTests/VpicVehicleDataApiClientTests.cs` | Makes + models |
| NEW | `Tests/Features/UnitTests/ApiClientTests/LithuanianMunicipalityApiClientTests.cs` | Municipality list |

---

## Phase 1: Application Layer Handler Integration Tests

### Task 1: Auth — LoginUserCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/LoginUserCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class LoginUserCommandHandlerTests(
    DatabaseFixture<LoginUserCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<LoginUserCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<LoginUserCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private LoginUserCommandHandler CreateHandler(
        IServiceScope scope,
        IPasswordHasher? passwordHasher = null,
        ITokenService? tokenService = null)
    {
        var ph = passwordHasher ?? Substitute.For<IPasswordHasher>();
        var ts = tokenService ?? Substitute.For<ITokenService>();

        return new LoginUserCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            ph,
            ts,
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    private static async Task<User> SeedUserAsync(AutomotiveContext context, string email = "test@example.com")
    {
        var user = new UserBuilder()
            .With(u => u.Email, email)
            .With(u => u.HashedPassword, "hashed_pw")
            .Build();

        user.UserPermissions =
        [
            new UserPermission { Permission = Permission.ViewListings },
            new UserPermission { Permission = Permission.CreateListings },
        ];

        await context.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndPermissions()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var user = await SeedUserAsync(context);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        tokenService.GenerateRefreshTokenEntity(Arg.Any<User>()).Returns(callInfo =>
        {
            var u = callInfo.Arg<User>();
            return new RefreshToken
            {
                Token = "refresh_token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                User = u,
            };
        });

        var handler = CreateHandler(scope, passwordHasher, tokenService);

        var result = await handler.Handle(new LoginUserCommand
        {
            Email = user.Email,
            Password = "correct_password",
        }, CancellationToken.None);

        result.FreshAccessToken.Should().Be("access_token");
        result.FreshRefreshToken.Should().Be("refresh_token");
        result.UserId.Should().Be(user.Id);
        result.Permissions.Should().Contain(Permission.ViewListings);
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ThrowsUserNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(new LoginUserCommand
        {
            Email = "nobody@example.com",
            Password = "password",
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentialsException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        await SeedUserAsync(context);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var handler = CreateHandler(scope, passwordHasher);

        var act = () => handler.Handle(new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "wrong_password",
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~LoginUserCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/LoginUserCommandHandlerTests.cs
git commit -m "test: add LoginUserCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: Auth — LogoutUserCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/LogoutUserCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using Automotive.Marketplace.Application.Features.AuthFeatures.LogoutUser;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class LogoutUserCommandHandlerTests(
    DatabaseFixture<LogoutUserCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<LogoutUserCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<LogoutUserCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static LogoutUserCommandHandler CreateHandler(IServiceScope scope)
    {
        return new LogoutUserCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesRefreshToken()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = "valid_token",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        await handler.Handle(new LogoutUserCommand { RefreshToken = "valid_token" }, CancellationToken.None);

        var updated = await context.Set<RefreshToken>().AsNoTracking()
            .FirstAsync(rt => rt.Token == "valid_token");
        updated.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExpiredToken_DoesNotRevoke()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = "expired_token",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        await handler.Handle(new LogoutUserCommand { RefreshToken = "expired_token" }, CancellationToken.None);

        var notRevoked = await context.Set<RefreshToken>().AsNoTracking()
            .FirstAsync(rt => rt.Token == "expired_token");
        notRevoked.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistentToken_CompletesWithoutError()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(new LogoutUserCommand { RefreshToken = "nonexistent" }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~LogoutUserCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/LogoutUserCommandHandlerTests.cs
git commit -m "test: add LogoutUserCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: Auth — RefreshTokenCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/RefreshTokenCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class RefreshTokenCommandHandlerTests(
    DatabaseFixture<RefreshTokenCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RefreshTokenCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RefreshTokenCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RefreshTokenCommandHandler CreateHandler(IServiceScope scope, ITokenService? tokenService = null)
    {
        var ts = tokenService ?? Substitute.For<ITokenService>();
        ts.GenerateAccessToken(Arg.Any<User>()).Returns("new_access_token");
        ts.GenerateRefreshTokenEntity(Arg.Any<User>()).Returns(callInfo =>
        {
            var u = callInfo.Arg<User>();
            return new Domain.Entities.RefreshToken
            {
                Token = "new_refresh_token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                User = u,
            };
        });

        return new RefreshTokenCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            ts,
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        user.UserPermissions = [new UserPermission { Permission = Permission.ViewListings }];
        await context.AddAsync(user);

        var token = new Domain.Entities.RefreshToken
        {
            Token = "current_token",
            ExpiryDate = DateTime.UtcNow.AddDays(5),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(token);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(
            new RefreshTokenCommand { RefreshToken = "current_token" },
            CancellationToken.None);

        result.FreshAccessToken.Should().Be("new_access_token");
        result.FreshRefreshToken.Should().Be("new_refresh_token");
        result.Permissions.Should().Contain(Permission.ViewListings);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var token = new Domain.Entities.RefreshToken
        {
            Token = "expired_token",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(token);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(
            new RefreshTokenCommand { RefreshToken = "expired_token" },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsInvalidRefreshTokenException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var token = new Domain.Entities.RefreshToken
        {
            Token = "revoked_token",
            ExpiryDate = DateTime.UtcNow.AddDays(5),
            IsRevoked = true,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(token);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(
            new RefreshTokenCommand { RefreshToken = "revoked_token" },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~RefreshTokenCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/AuthHandlerTests/RefreshTokenCommandHandlerTests.cs
git commit -m "test: add RefreshTokenCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: Listing — DeleteListingCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/DeleteListingCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class DeleteListingCommandHandlerTests(
    DatabaseFixture<DeleteListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new DeleteListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    private static async Task<(Listing listing, User seller)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return (listing, seller);
    }

    [Fact]
    public async Task Handle_OwnerDeletes_RemovesListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        await handler.Handle(new DeleteListingCommand
        {
            Id = listing.Id,
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        var deleted = await context.Set<Listing>().AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listing.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, _) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new DeleteListingCommand
        {
            Id = listing.Id,
            CurrentUserId = Guid.NewGuid(),
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AdminPermission_DeletesAnyListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, _) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        await handler.Handle(new DeleteListingCommand
        {
            Id = listing.Id,
            CurrentUserId = Guid.NewGuid(),
            Permissions = [Permission.ManageListings.ToString()],
        }, CancellationToken.None);

        var deleted = await context.Set<Listing>().AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listing.Id);
        deleted.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~DeleteListingCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/DeleteListingCommandHandlerTests.cs
git commit -m "test: add DeleteListingCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Listing — UpdateListingCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class UpdateListingCommandHandlerTests(
    DatabaseFixture<UpdateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    private static async Task<(Listing listing, User seller)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id)
            .WithPrice(10000m).Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return (listing, seller);
    }

    [Fact]
    public async Task Handle_OwnerUpdates_UpdatesListingInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        await handler.Handle(new UpdateListingCommand
        {
            Id = listing.Id,
            Price = 15000m,
            Mileage = 50000,
            Description = "Updated description",
            Year = listing.Year,
            MunicipalityId = listing.MunicipalityId,
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listing.Id);
        updated.Price.Should().Be(15000m);
        updated.Mileage.Should().Be(50000);
        updated.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, _) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new UpdateListingCommand
        {
            Id = listing.Id,
            Price = 20000m,
            Year = listing.Year,
            MunicipalityId = listing.MunicipalityId,
            CurrentUserId = Guid.NewGuid(),
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~UpdateListingCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 2 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingCommandHandlerTests.cs
git commit -m "test: add UpdateListingCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Listing — UpdateListingStatusCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingStatusCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListingStatus;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class UpdateListingStatusCommandHandlerTests(
    DatabaseFixture<UpdateListingStatusCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateListingStatusCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateListingStatusCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateListingStatusCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateListingStatusCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    private static async Task<(Listing listing, User seller)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return (listing, seller);
    }

    [Fact]
    public async Task Handle_OwnerChangesStatus_UpdatesInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        await handler.Handle(new UpdateListingStatusCommand
        {
            Id = listing.Id,
            Status = nameof(Status.OnHold),
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listing.Id);
        updated.Status.Should().Be(Status.OnHold);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsArgumentException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new UpdateListingStatusCommand
        {
            Id = listing.Id,
            Status = "NotAValidStatus",
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, _) = await SeedListingAsync(context);

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new UpdateListingStatusCommand
        {
            Id = listing.Id,
            Status = nameof(Status.Sold),
            CurrentUserId = Guid.NewGuid(),
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~UpdateListingStatusCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/UpdateListingStatusCommandHandlerTests.cs
git commit -m "test: add UpdateListingStatusCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: Listing — ReactivateListingCommandHandlerTests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/ReactivateListingCommandHandlerTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
using Automotive.Marketplace.Application.Features.ListingFeatures.ReactivateListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class ReactivateListingCommandHandlerTests(
    DatabaseFixture<ReactivateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ReactivateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ReactivateListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static ReactivateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new ReactivateListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    private static async Task<(Listing listing, User seller, User buyer)> SeedOnHoldListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id)
            .With(l => l.Status, Status.OnHold).Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, buyer, municipality, listing);
        await context.SaveChangesAsync();
        return (listing, seller, buyer);
    }

    [Fact]
    public async Task Handle_OnHoldListing_ReactivatesToAvailable()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller, _) = await SeedOnHoldListingAsync(context);

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new ReactivateListingCommand
        {
            ListingId = listing.Id,
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        result.ListingId.Should().Be(listing.Id);

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listing.Id);
        updated.Status.Should().Be(Status.Available);
    }

    [Fact]
    public async Task Handle_NotOnHoldListing_ThrowsInvalidOperationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, seller, _) = await SeedOnHoldListingAsync(context);

        listing.Status = Status.Available;
        context.Update(listing);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new ReactivateListingCommand
        {
            ListingId = listing.Id,
            CurrentUserId = seller.Id,
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*OnHold*");
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listing, _, _) = await SeedOnHoldListingAsync(context);

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(new ReactivateListingCommand
        {
            ListingId = listing.Id,
            CurrentUserId = Guid.NewGuid(),
            Permissions = [],
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~ReactivateListingCommandHandlerTests" ./Automotive.Marketplace.sln`
Expected: 3 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/ReactivateListingCommandHandlerTests.cs
git commit -m "test: add ReactivateListingCommandHandler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Model — All 6 Model Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/CreateModelCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/DeleteModelCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/GetAllModelsQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/GetModelByIdQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/GetModelsByMakeIdQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/UpdateModelCommandHandlerTests.cs`

- [ ] **Step 1: Create CreateModelCommandHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class CreateModelCommandHandlerTests(
    DatabaseFixture<CreateModelCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateModelCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateModelCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static CreateModelCommandHandler CreateHandler(IServiceScope scope)
    {
        return new CreateModelCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesModelInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        await handler.Handle(new CreateModelCommand
        {
            Name = "Corolla",
            MakeId = make.Id,
        }, CancellationToken.None);

        var model = await context.Set<Model>().AsNoTracking()
            .FirstOrDefaultAsync(m => m.Name == "Corolla");
        model.Should().NotBeNull();
        model!.MakeId.Should().Be(make.Id);
    }
}
```

- [ ] **Step 2: Create DeleteModelCommandHandlerTests.cs**

```csharp
using Automotive.Marketplace.Application.Features.ModelFeatures.DeleteModel;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class DeleteModelCommandHandlerTests(
    DatabaseFixture<DeleteModelCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteModelCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteModelCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteModelCommandHandler CreateHandler(IServiceScope scope)
    {
        return new DeleteModelCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ExistingModel_DeletesFromDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        await context.AddRangeAsync(make, model);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        await handler.Handle(new DeleteModelCommand { Id = model.Id }, CancellationToken.None);

        var deleted = await context.Set<Model>().AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == model.Id);
        deleted.Should().BeNull();
    }
}
```

- [ ] **Step 3: Create GetAllModelsQueryHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class GetAllModelsQueryHandlerTests(
    DatabaseFixture<GetAllModelsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllModelsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllModelsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetAllModelsQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetAllModelsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ModelsExist_ReturnsSortedByName()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var modelB = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Camry").Build();
        var modelA = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Accord").Build();
        await context.AddRangeAsync(make, modelB, modelA);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllModelsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(m => m.Name);
    }

    [Fact]
    public async Task Handle_NoModels_ReturnsEmpty()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllModelsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
```

- [ ] **Step 4: Create GetModelByIdQueryHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class GetModelByIdQueryHandlerTests(
    DatabaseFixture<GetModelByIdQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetModelByIdQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetModelByIdQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetModelByIdQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetModelByIdQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ExistingModel_ReturnsModel()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Civic").Build();
        await context.AddRangeAsync(make, model);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetModelByIdQuery { Id = model.Id }, CancellationToken.None);

        result.Name.Should().Be("Civic");
        result.MakeId.Should().Be(make.Id);
    }

    [Fact]
    public async Task Handle_NonExistentModel_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();

        var handler = CreateHandler(scope);
        var act = () => handler.Handle(
            new GetModelByIdQuery { Id = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
```

- [ ] **Step 5: Create GetModelsByMakeIdQueryHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class GetModelsByMakeIdQueryHandlerTests(
    DatabaseFixture<GetModelsByMakeIdQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetModelsByMakeIdQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetModelsByMakeIdQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetModelsByMakeIdQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetModelsByMakeIdQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ModelsForMake_ReturnsFilteredAndSorted()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var toyota = new MakeBuilder().With(m => m.Name, "Toyota").Build();
        var honda = new MakeBuilder().With(m => m.Name, "Honda").Build();
        var camry = new ModelBuilder().WithMake(toyota.Id).With(m => m.Name, "Camry").Build();
        var corolla = new ModelBuilder().WithMake(toyota.Id).With(m => m.Name, "Corolla").Build();
        var civic = new ModelBuilder().WithMake(honda.Id).With(m => m.Name, "Civic").Build();
        await context.AddRangeAsync(toyota, honda, camry, corolla, civic);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(
            new GetModelsByMakeIdQuery { MakeId = toyota.Id },
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(m => m.Name);
        result.Should().OnlyContain(m => m.Name == "Camry" || m.Name == "Corolla");
    }
}
```

- [ ] **Step 6: Create UpdateModelCommandHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class UpdateModelCommandHandlerTests(
    DatabaseFixture<UpdateModelCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateModelCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateModelCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateModelCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateModelCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesModelInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "OldName").Build();
        await context.AddRangeAsync(make, model);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        await handler.Handle(new UpdateModelCommand
        {
            Id = model.Id,
            Name = "NewName",
            MakeId = make.Id,
        }, CancellationToken.None);

        var updated = await context.Set<Model>().AsNoTracking()
            .FirstAsync(m => m.Id == model.Id);
        updated.Name.Should().Be("NewName");
    }
}
```

- [ ] **Step 7: Run all model tests**

Run: `dotnet test --filter "FullyQualifiedName~ModelHandlerTests" ./Automotive.Marketplace.sln`
Expected: 8 tests PASS

- [ ] **Step 8: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ModelHandlerTests/
git commit -m "test: add all 6 Model handler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: Variant — All 4 Variant Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/VariantHandlerTests/CreateVariantCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/VariantHandlerTests/DeleteVariantCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/VariantHandlerTests/GetVariantsByModelQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/VariantHandlerTests/UpdateVariantCommandHandlerTests.cs`

- [ ] **Step 1: Create CreateVariantCommandHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VariantHandlerTests;

public class CreateVariantCommandHandlerTests(
    DatabaseFixture<CreateVariantCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateVariantCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateVariantCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static CreateVariantCommandHandler CreateHandler(IServiceScope scope)
    {
        return new CreateVariantCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesVariantInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new CreateVariantCommand(
            ModelId: model.Id,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            IsCustom: false,
            DoorCount: 4,
            PowerKw: 150,
            EngineSizeMl: 2000
        ), CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        var variant = await context.Set<Variant>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == result.Id);
        variant.Should().NotBeNull();
        variant!.PowerKw.Should().Be(150);
    }
}
```

- [ ] **Step 2: Create DeleteVariantCommandHandlerTests.cs**

```csharp
using Automotive.Marketplace.Application.Features.VariantFeatures.DeleteVariant;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VariantHandlerTests;

public class DeleteVariantCommandHandlerTests(
    DatabaseFixture<DeleteVariantCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteVariantCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteVariantCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteVariantCommandHandler CreateHandler(IServiceScope scope)
    {
        return new DeleteVariantCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ExistingVariant_DeletesFromDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, variant);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        await handler.Handle(new DeleteVariantCommand(variant.Id), CancellationToken.None);

        var deleted = await context.Set<Variant>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == variant.Id);
        deleted.Should().BeNull();
    }
}
```

- [ ] **Step 3: Create GetVariantsByModelQueryHandlerTests.cs**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VariantHandlerTests;

public class GetVariantsByModelQueryHandlerTests(
    DatabaseFixture<GetVariantsByModelQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetVariantsByModelQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetVariantsByModelQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetVariantsByModelQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetVariantsByModelQueryHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    [Fact]
    public async Task Handle_VariantsForModel_ReturnsFiltered()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model1 = new ModelBuilder().WithMake(make.Id).Build();
        var model2 = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var v1 = new VariantBuilder()
            .WithModel(model1.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var v2 = new VariantBuilder()
            .WithModel(model1.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var v3 = new VariantBuilder()
            .WithModel(model2.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        await context.AddRangeAsync(make, model1, model2, fuel, transmission, bodyType, v1, v2, v3);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(
            new GetVariantsByModelQuery(model1.Id),
            CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
```

- [ ] **Step 4: Create UpdateVariantCommandHandlerTests.cs**

The UpdateVariant handler needs to be verified. Let me read it first to get the exact signature:

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VariantHandlerTests;

public class UpdateVariantCommandHandlerTests(
    DatabaseFixture<UpdateVariantCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateVariantCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateVariantCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateVariantCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateVariantCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesVariantInDb()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .With(v => v.PowerKw, 100).Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, variant);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        await handler.Handle(new UpdateVariantCommand
        {
            Id = variant.Id,
            ModelId = model.Id,
            FuelId = fuel.Id,
            TransmissionId = transmission.Id,
            BodyTypeId = bodyType.Id,
            PowerKw = 200,
            EngineSizeMl = variant.EngineSizeMl,
            DoorCount = variant.DoorCount,
            IsCustom = false,
        }, CancellationToken.None);

        var updated = await context.Set<Variant>().AsNoTracking()
            .FirstAsync(v => v.Id == variant.Id);
        updated.PowerKw.Should().Be(200);
    }
}
```

NOTE: The `UpdateVariantCommand` class must be checked for exact property names. Read `Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommand.cs` before implementing and adjust property names accordingly.

- [ ] **Step 5: Run all variant tests**

Run: `dotnet test --filter "FullyQualifiedName~VariantHandlerTests" ./Automotive.Marketplace.sln`
Expected: 4 tests PASS

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/VariantHandlerTests/
git commit -m "test: add all 4 Variant handler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: Lookup — All 4 Lookup Handler Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/LookupHandlerTests/GetAllBodyTypesQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/LookupHandlerTests/GetAllDrivetrainsQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/LookupHandlerTests/GetAllFuelsQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/LookupHandlerTests/GetAllTransmissionsQueryHandlerTests.cs`

- [ ] **Step 1: Create all 4 lookup test files**

Each follows the same pattern — seed lookup entities, query, assert returned list. The handler for each takes `IMapper` + `IRepository`.

**GetAllBodyTypesQueryHandlerTests.cs:**
```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.LookupFeatures.GetAllBodyTypes;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.LookupHandlerTests;

public class GetAllBodyTypesQueryHandlerTests(
    DatabaseFixture<GetAllBodyTypesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllBodyTypesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllBodyTypesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetAllBodyTypesQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetAllBodyTypesQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_BodyTypesExist_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var sedan = new BodyTypeBuilder().Build();
        var suv = new BodyTypeBuilder().Build();
        await context.AddRangeAsync(sedan, suv);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllBodyTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
```

**GetAllDrivetrainsQueryHandlerTests.cs:**
```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.LookupFeatures.GetAllDrivetrains;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.LookupHandlerTests;

public class GetAllDrivetrainsQueryHandlerTests(
    DatabaseFixture<GetAllDrivetrainsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllDrivetrainsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllDrivetrainsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetAllDrivetrainsQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetAllDrivetrainsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_DrivetrainsExist_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var fwd = new DrivetrainBuilder().Build();
        var awd = new DrivetrainBuilder().Build();
        await context.AddRangeAsync(fwd, awd);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllDrivetrainsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
```

**GetAllFuelsQueryHandlerTests.cs:**
```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.LookupFeatures.GetAllFuels;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.LookupHandlerTests;

public class GetAllFuelsQueryHandlerTests(
    DatabaseFixture<GetAllFuelsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllFuelsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllFuelsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetAllFuelsQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetAllFuelsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_FuelsExist_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var petrol = new FuelBuilder().Build();
        var diesel = new FuelBuilder().Build();
        await context.AddRangeAsync(petrol, diesel);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllFuelsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
```

**GetAllTransmissionsQueryHandlerTests.cs:**
```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.LookupFeatures.GetAllTransmissions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.LookupHandlerTests;

public class GetAllTransmissionsQueryHandlerTests(
    DatabaseFixture<GetAllTransmissionsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllTransmissionsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllTransmissionsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetAllTransmissionsQueryHandler CreateHandler(IServiceScope scope)
    {
        return new GetAllTransmissionsQueryHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_TransmissionsExist_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var auto = new TransmissionBuilder().Build();
        var manual = new TransmissionBuilder().Build();
        await context.AddRangeAsync(auto, manual);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);
        var result = await handler.Handle(new GetAllTransmissionsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
```

- [ ] **Step 2: Run all lookup tests**

Run: `dotnet test --filter "FullyQualifiedName~LookupHandlerTests" ./Automotive.Marketplace.sln`
Expected: 4 tests PASS

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/LookupHandlerTests/
git commit -m "test: add all 4 Lookup handler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 11: Remaining Handler Tests (Chat, Defect, Dashboard)

These handlers need specific entity relationships. The implementing agent should read the handler source code directly before writing tests.

**Chat handlers to test:**
- `CancelOfferCommandHandler` — read `Application/Features/ChatFeatures/CancelOffer/` then write test
- `GetUnreadCountQueryHandler` — read `Application/Features/ChatFeatures/GetUnreadCount/` then write test
- `GetUserContractProfileQueryHandler` — read `Application/Features/ChatFeatures/GetUserContractProfile/` then write test
- `UpdateUserContractProfileCommandHandler` — read `Application/Features/ChatFeatures/UpdateUserContractProfile/` then write test

**Defect handlers to test:**
- `AddListingDefectCommandHandler` — read `Application/Features/DefectFeatures/AddListingDefect/` then write test
- `AddDefectImageCommandHandler` — read `Application/Features/DefectFeatures/AddDefectImage/` then write test
- `GetDefectCategoriesQueryHandler` — read `Application/Features/DefectFeatures/GetDefectCategories/` then write test
- `RemoveDefectImageCommandHandler` — read `Application/Features/DefectFeatures/RemoveDefectImage/` then write test
- `RemoveListingDefectCommandHandler` — read `Application/Features/DefectFeatures/RemoveListingDefect/` then write test

**Dashboard handler to test:**
- `GetDashboardSummaryQueryHandler` — read `Application/Features/DashboardFeatures/GetDashboardSummary/` then write test

Follow the same pattern as Tasks 1-10: DatabaseFixture, CreateHandler, seed data, assert.

- [ ] **Step 1: Read each handler's source files before writing tests**
- [ ] **Step 2: Write each test file following the pattern from Tasks 1-10**
- [ ] **Step 3: Run tests** — `dotnet test --filter "FullyQualifiedName~ChatHandlerTests|DefectHandlerTests|DashboardHandlerTests" ./Automotive.Marketplace.sln`
- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/ \
       Automotive.Marketplace.Tests/Features/HandlerTests/DefectHandlerTests/ \
       Automotive.Marketplace.Tests/Features/HandlerTests/DashboardHandlerTests/
git commit -m "test: add Chat, Defect, Dashboard handler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Phase 2: Server Layer Unit Tests

### Task 12: Exception Filter Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/FilterTests/ValidationExceptionFilterTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/FilterTests/NotFoundExceptionFilterTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/FilterTests/UnauthorizedExceptionFilterTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/FilterTests/ForbiddenExceptionFilterTests.cs`

- [ ] **Step 1: Create ValidationExceptionFilterTests.cs**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Server.Filters;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Automotive.Marketplace.Tests.Features.UnitTests.FilterTests;

public class ValidationExceptionFilterTests
{
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    [Fact]
    public void OnException_RequestValidationException_Returns400WithErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email must be valid"),
            new("Password", "Password is required"),
        };
        var exception = new RequestValidationException(failures);
        var context = CreateExceptionContext(exception);
        var filter = new ValidationExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("Validation");
        errorResponse.Messages.Should().ContainKey("Email");
        errorResponse.Messages!["Email"].Should().HaveCount(2);
    }

    [Fact]
    public void OnException_OtherException_DoesNotHandle()
    {
        var context = CreateExceptionContext(new InvalidOperationException("test"));
        var filter = new ValidationExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }
}
```

- [ ] **Step 2: Create NotFoundExceptionFilterTests.cs**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Server.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Automotive.Marketplace.Tests.Features.UnitTests.FilterTests;

public class NotFoundExceptionFilterTests
{
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    [Fact]
    public void OnException_DbEntityNotFoundException_Returns404()
    {
        var exception = new DbEntityNotFoundException("Listing", Guid.NewGuid());
        var context = CreateExceptionContext(exception);
        var filter = new NotFoundExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<NotFoundObjectResult>();
        var result = (NotFoundObjectResult)context.Result!;
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("NotFound");
        errorResponse.Message.Should().Contain("listing");
    }
}
```

- [ ] **Step 3: Create UnauthorizedExceptionFilterTests.cs**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Server.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Automotive.Marketplace.Tests.Features.UnitTests.FilterTests;

public class UnauthorizedExceptionFilterTests
{
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    [Fact]
    public void OnException_InvalidCredentialsException_Returns401()
    {
        var context = CreateExceptionContext(new InvalidCredentialsException());
        var filter = new UnauthorizedExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = (UnauthorizedObjectResult)context.Result!;
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("Authentication");
    }

    [Fact]
    public void OnException_MissingRefreshTokenException_Returns401()
    {
        var context = CreateExceptionContext(new MissingRefreshTokenException());
        var filter = new UnauthorizedExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var result = (UnauthorizedObjectResult)context.Result!;
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("MissingToken");
    }

    [Fact]
    public void OnException_InvalidRefreshTokenException_Returns401()
    {
        var context = CreateExceptionContext(new InvalidRefreshTokenException());
        var filter = new UnauthorizedExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var result = (UnauthorizedObjectResult)context.Result!;
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("InvalidToken");
    }
}
```

- [ ] **Step 4: Create ForbiddenExceptionFilterTests.cs**

```csharp
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Server.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Automotive.Marketplace.Tests.Features.UnitTests.FilterTests;

public class ForbiddenExceptionFilterTests
{
    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    [Fact]
    public void OnException_UnauthorizedAccessException_Returns403()
    {
        var context = CreateExceptionContext(new UnauthorizedAccessException("No access"));
        var filter = new ForbiddenExceptionFilter();

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var errorResponse = result.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Type.Should().Be("Forbidden");
    }
}
```

- [ ] **Step 5: Run all filter tests**

Run: `dotnet test --filter "FullyQualifiedName~FilterTests" ./Automotive.Marketplace.sln`
Expected: 7 tests PASS

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/UnitTests/FilterTests/
git commit -m "test: add exception filter unit tests (Server layer)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 13: Infrastructure Service Unit Tests (PasswordHasher + TokenService)

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ServiceTests/PasswordHasherTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ServiceTests/TokenServiceTests.cs`

- [ ] **Step 1: Create PasswordHasherTests.cs**

```csharp
using Automotive.Marketplace.Infrastructure.Services;
using FluentAssertions;

namespace Automotive.Marketplace.Tests.Features.UnitTests.ServiceTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ValidPassword_ReturnsNonNullHash()
    {
        var hash = _sut.Hash("MyPassword123!");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_Password_IsNotPlaintext()
    {
        var hash = _sut.Hash("MyPassword123!");

        hash.Should().NotBe("MyPassword123!");
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var password = "SecurePassword!";
        var hash = _sut.Hash(password);

        var result = _sut.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("CorrectPassword");

        var result = _sut.Verify("WrongPassword", hash);

        result.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Create TokenServiceTests.cs**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Automotive.Marketplace.Tests.Features.UnitTests.ServiceTests;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
            })
            .Build();

        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsJwtWithClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            HashedPassword = "hash",
            UserPermissions =
            [
                new UserPermission { Permission = Permission.ViewListings },
                new UserPermission { Permission = Permission.CreateListings },
            ],
        };

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();

        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public void GenerateRefreshTokenEntity_ValidUser_ReturnsEntityWithCorrectExpiry()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            HashedPassword = "hash",
        };

        var entity = _sut.GenerateRefreshTokenEntity(user);

        entity.Token.Should().NotBeNullOrEmpty();
        entity.IsRevoked.Should().BeFalse();
        entity.User.Should().Be(user);
        entity.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(5));
    }
}
```

- [ ] **Step 3: Run service tests**

Run: `dotnet test --filter "FullyQualifiedName~ServiceTests" ./Automotive.Marketplace.sln`
Expected: 7 tests PASS

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/UnitTests/ServiceTests/
git commit -m "test: add PasswordHasher and TokenService unit tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Phase 3: Infrastructure Layer — API Client Tests

### Task 14: MockHttpMessageHandler + API Client Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Infrastructure/MockHttpMessageHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/CardogApiClientTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/OpenAiClientTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/NhtsaApiClientTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/LithuanianMunicipalityApiClientTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/VpicVehicleDataApiClientTests.cs`

- [ ] **Step 1: Create MockHttpMessageHandler.cs**

```csharp
using System.Net;
using System.Text;

namespace Automotive.Marketplace.Tests.Infrastructure;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;

    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = responseContent;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_responseContent, Encoding.UTF8, "application/json"),
        });
    }
}
```

- [ ] **Step 2: Create API client test files**

Each API client test follows the pattern: create MockHttpMessageHandler with canned response → construct `HttpClient` with it → construct API client → call method → assert result.

The implementing agent should:
1. Read each API client's source code in `Infrastructure/Services/`
2. Understand the exact JSON response structure expected
3. Write tests with real JSON fixtures

Example pattern for CardogApiClient:
```csharp
using System.Net;
using Automotive.Marketplace.Infrastructure.Services;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Automotive.Marketplace.Tests.Features.UnitTests.ApiClientTests;

public class CardogApiClientTests
{
    [Fact]
    public async Task GetMarketOverviewAsync_ValidResponse_ReturnsResult()
    {
        var json = """{"median_price": 18000, "total_listings": 60}""";
        var handler = new MockHttpMessageHandler(json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var client = new CardogApiClient(httpClient, NullLogger<CardogApiClient>.Instance);

        var result = await client.GetMarketOverviewAsync("Toyota", "Camry", 2020, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MedianPrice.Should().Be(18000);
        result.TotalListings.Should().Be(60);
    }

    [Fact]
    public async Task GetMarketOverviewAsync_ApiError_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var client = new CardogApiClient(httpClient, NullLogger<CardogApiClient>.Instance);

        var result = await client.GetMarketOverviewAsync("Toyota", "Camry", 2020, CancellationToken.None);

        result.Should().BeNull();
    }
}
```

NOTE: The JSON payloads above are approximations. The implementing agent MUST read the actual API client source code to verify the exact JSON structure and deserialization logic.

- [ ] **Step 3: Run all API client tests**

Run: `dotnet test --filter "FullyQualifiedName~ApiClientTests" ./Automotive.Marketplace.sln`
Expected: All tests PASS

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Tests/Infrastructure/MockHttpMessageHandler.cs \
       Automotive.Marketplace.Tests/Features/UnitTests/ApiClientTests/
git commit -m "test: add MockHttpMessageHandler and API client unit tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Final Verification

### Task 15: Run Full Test Suite + Coverage

- [ ] **Step 1: Run all tests**

Run: `dotnet test ./Automotive.Marketplace.sln`
Expected: All tests PASS (219 existing + ~80-100 new = ~300-320 total)

- [ ] **Step 2: Run coverage analysis**

```bash
dotnet test ./Automotive.Marketplace.sln --collect:"XPlat Code Coverage" --results-directory TestResults
~/.dotnet/tools/reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:TestResults/report -reporttypes:TextSummary
cat TestResults/report/Summary.txt
```

Expected: Application ≥ 80%, Server ≥ 30%, Infrastructure ≥ 30%

- [ ] **Step 3: Cleanup**

```bash
rm -rf TestResults
```

- [ ] **Step 4: Final commit if any fixes were needed**

```bash
git add -A
git commit -m "test: finalize test coverage increase

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
