# Personalization Quiz Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `UserPreferences` table storing per-user score weights, expose `GET /UserPreferences/Get` and `PUT /UserPreferences/Upsert` endpoints, extend the score engine to apply personalized weights for authenticated users, and build a 3-step quiz modal with entry points in the score card, settings page, and comparison banner.

**Architecture:** `UserPreferences` stores one row per user (four weights summing to 1.0). `GetListingScoreQuery` is extended with an optional `UserId`; the handler loads preferences when present. The quiz modal computes initial weight suggestions from scenario + priority selections, then allows fine-tuning with sliders before saving.

**Prerequisite:** Plan 1 (`2026-04-26-cardog-score-engine.md`) must be completed first. `UserPreferences` also stores `AutoGenerateAiSummary` (needed by Plan 3), so this entity must be complete before Plan 3 begins.

**Tech Stack:** .NET 8, EF Core, MediatR; React 19, TypeScript, TanStack Query, shadcn/ui Dialog/Slider, Lucide React.

---

## File Map

**New files — Backend:**
- `Automotive.Marketplace.Domain/Entities/UserPreferences.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs`
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQuery.cs`
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs`
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs`
- `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs`
- `Automotive.Marketplace.Server/Controllers/UserPreferencesController.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/GetUserPreferencesQueryHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesCommandHandlerTests.cs`

**Modified files — Backend:**
- `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — add `DbSet<UserPreferences>`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQuery.cs` — add `UserId?`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs` — add `IsPersonalized`
- `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs` — apply user weights when UserId present
- `Automotive.Marketplace.Server/Controllers/ListingController.cs` — pass `UserId` to GetScore query

**New files — Frontend:**
- `automotive.marketplace.client/src/features/userPreferences/types/UserPreferencesResponse.ts`
- `automotive.marketplace.client/src/features/userPreferences/types/UpsertUserPreferencesCommand.ts`
- `automotive.marketplace.client/src/features/userPreferences/api/getUserPreferencesOptions.ts`
- `automotive.marketplace.client/src/features/userPreferences/api/useUpsertUserPreferences.ts`
- `automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx`
- `automotive.marketplace.client/src/features/userPreferences/index.ts`
- `automotive.marketplace.client/src/api/queryKeys/userPreferencesKeys.ts`

**Modified files — Frontend:**
- `automotive.marketplace.client/src/constants/endpoints.ts` — add `USER_PREFERENCES` section
- `automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx` — add quiz entry point + personalized label
- `automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx` — add quiz entry point
- `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts` — add `isPersonalized`

---

### Task 1: UserPreferences domain entity + EF config + migration

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/UserPreferences.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create UserPreferences entity**

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }

    public double ValueWeight { get; set; } = 0.30;
    public double EfficiencyWeight { get; set; } = 0.25;
    public double ReliabilityWeight { get; set; } = 0.25;
    public double MileageWeight { get; set; } = 0.20;

    public bool AutoGenerateAiSummary { get; set; }

    public virtual User User { get; set; } = null!;
}
```

- [ ] **Step 2: Create EF configuration**

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 3: Add DbSet to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add:
```csharp
public DbSet<UserPreferences> UserPreferences { get; set; }
```

- [ ] **Step 4: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 5: Create and apply migration**

```bash
dotnet ef migrations add AddUserPreferences \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server

dotnet ef database update \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/Entities/UserPreferences.cs \
        Automotive.Marketplace.Infrastructure/Data/Configuration/UserPreferencesConfiguration.cs \
        Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs \
        Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat: add UserPreferences entity, EF config, and migration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 2: GetUserPreferences query + handler + tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/GetUserPreferences/GetUserPreferencesQueryHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/GetUserPreferencesQueryHandlerTests.cs`

Returns the current user's preferences. Returns default weights when no preferences record exists yet.

- [ ] **Step 1: Create query and response**

```csharp
// GetUserPreferencesQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesQuery : IRequest<GetUserPreferencesResponse>
{
    public Guid UserId { get; set; }
}
```

```csharp
// GetUserPreferencesResponse.cs
namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesResponse
{
    public double ValueWeight { get; init; }
    public double EfficiencyWeight { get; init; }
    public double ReliabilityWeight { get; init; }
    public double MileageWeight { get; init; }
    public bool AutoGenerateAiSummary { get; init; }
    public bool HasPreferences { get; init; }
}
```

- [ ] **Step 2: Write the failing tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/GetUserPreferencesQueryHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.UserPreferencesHandlerTests;

public class GetUserPreferencesQueryHandlerTests(
    DatabaseFixture<GetUserPreferencesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetUserPreferencesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetUserPreferencesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetUserPreferencesQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoPreferencesExist_ReturnsDefaultWeights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(new GetUserPreferencesQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.HasPreferences.Should().BeFalse();
        result.ValueWeight.Should().Be(0.30);
        result.EfficiencyWeight.Should().Be(0.25);
        result.ReliabilityWeight.Should().Be(0.25);
        result.MileageWeight.Should().Be(0.20);
        result.AutoGenerateAiSummary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PreferencesExist_ReturnsStoredWeights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        var prefs = new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ValueWeight = 0.40,
            EfficiencyWeight = 0.30,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = true,
        };
        await context.AddAsync(prefs);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetUserPreferencesQuery { UserId = user.Id }, CancellationToken.None);

        result.HasPreferences.Should().BeTrue();
        result.ValueWeight.Should().Be(0.40);
        result.EfficiencyWeight.Should().Be(0.30);
        result.ReliabilityWeight.Should().Be(0.20);
        result.MileageWeight.Should().Be(0.10);
        result.AutoGenerateAiSummary.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetUserPreferencesQueryHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors about `GetUserPreferencesQueryHandler` not existing.

- [ ] **Step 4: Create handler**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IRepository repository)
    : IRequestHandler<GetUserPreferencesQuery, GetUserPreferencesResponse>
{
    public async Task<GetUserPreferencesResponse> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (prefs is null)
        {
            return new GetUserPreferencesResponse
            {
                ValueWeight = 0.30,
                EfficiencyWeight = 0.25,
                ReliabilityWeight = 0.25,
                MileageWeight = 0.20,
                AutoGenerateAiSummary = false,
                HasPreferences = false,
            };
        }

        return new GetUserPreferencesResponse
        {
            ValueWeight = prefs.ValueWeight,
            EfficiencyWeight = prefs.EfficiencyWeight,
            ReliabilityWeight = prefs.ReliabilityWeight,
            MileageWeight = prefs.MileageWeight,
            AutoGenerateAiSummary = prefs.AutoGenerateAiSummary,
            HasPreferences = true,
        };
    }
}
```

- [ ] **Step 5: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetUserPreferencesQueryHandlerTests" -q
```
Expected: Both tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/UserPreferencesFeatures/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/GetUserPreferencesQueryHandlerTests.cs
git commit -m "feat: add GetUserPreferencesQueryHandler — returns stored or default weights

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 3: UpsertUserPreferences command + handler + tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/UpsertUserPreferencesCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesCommandHandlerTests.cs`

Creates a new row if none exists; updates the existing row if found. Enforces that the four weights must sum to approximately 1.0 (within ±0.01).

- [ ] **Step 1: Create command**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommand : IRequest
{
    public Guid UserId { get; set; }
    public double ValueWeight { get; set; }
    public double EfficiencyWeight { get; set; }
    public double ReliabilityWeight { get; set; }
    public double MileageWeight { get; set; }
    public bool AutoGenerateAiSummary { get; set; }
}
```

- [ ] **Step 2: Write the failing tests**

Create `Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesCommandHandlerTests.cs`:

```csharp
using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.UserPreferencesHandlerTests;

public class UpsertUserPreferencesCommandHandlerTests(
    DatabaseFixture<UpsertUserPreferencesCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpsertUserPreferencesCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpsertUserPreferencesCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private UpsertUserPreferencesCommandHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingPreferences_CreatesNewRow()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.40,
            EfficiencyWeight = 0.30,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = true,
        }, CancellationToken.None);

        var prefs = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
        prefs.Should().NotBeNull();
        prefs!.ValueWeight.Should().Be(0.40);
        prefs.AutoGenerateAiSummary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingPreferences_UpdatesRow()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        var existing = new UserPreferences
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            ValueWeight = 0.30, EfficiencyWeight = 0.25,
            ReliabilityWeight = 0.25, MileageWeight = 0.20,
        };
        await context.AddAsync(existing);
        await context.SaveChangesAsync();

        await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.50,
            EfficiencyWeight = 0.20,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = false,
        }, CancellationToken.None);

        var updated = await context.UserPreferences.FirstAsync(p => p.UserId == user.Id);
        updated.ValueWeight.Should().Be(0.50);
        updated.MileageWeight.Should().Be(0.10);
    }

    [Fact]
    public async Task Handle_InvalidWeightSum_ThrowsValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        var act = async () => await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.50,
            EfficiencyWeight = 0.50,
            ReliabilityWeight = 0.50,
            MileageWeight = 0.50,
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 3: Run to verify they fail**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~UpsertUserPreferencesCommandHandlerTests" --no-build 2>&1 | tail -5
```
Expected: compile errors.

- [ ] **Step 4: Create handler**

```csharp
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommandHandler(IRepository repository)
    : IRequestHandler<UpsertUserPreferencesCommand>
{
    public async Task Handle(UpsertUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var total = request.ValueWeight + request.EfficiencyWeight + request.ReliabilityWeight + request.MileageWeight;
        if (Math.Abs(total - 1.0) > 0.01)
            throw new ArgumentException("Score weights must sum to 1.0");

        var existing = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (existing != null)
        {
            existing.ValueWeight = request.ValueWeight;
            existing.EfficiencyWeight = request.EfficiencyWeight;
            existing.ReliabilityWeight = request.ReliabilityWeight;
            existing.MileageWeight = request.MileageWeight;
            existing.AutoGenerateAiSummary = request.AutoGenerateAiSummary;
            await repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await repository.CreateAsync(new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ValueWeight = request.ValueWeight,
                EfficiencyWeight = request.EfficiencyWeight,
                ReliabilityWeight = request.ReliabilityWeight,
                MileageWeight = request.MileageWeight,
                AutoGenerateAiSummary = request.AutoGenerateAiSummary,
            }, cancellationToken);
        }
    }
}
```

Note: The `RequestValidationException` in `Automotive.Marketplace.Application.Common.Exceptions` requires FluentValidation failures; use `ArgumentException` for simple guard clauses like this. Update the test's `ThrowAsync<Exception>()` to `ThrowAsync<ArgumentException>()` for specificity.

- [ ] **Step 5: Build and run tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~UpsertUserPreferencesCommandHandlerTests" -q
```
Expected: All 3 tests PASS.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/UserPreferencesFeatures/UpsertUserPreferences/ \
        Automotive.Marketplace.Tests/Features/HandlerTests/UserPreferencesHandlerTests/UpsertUserPreferencesCommandHandlerTests.cs
git commit -m "feat: add UpsertUserPreferencesCommandHandler with weight-sum validation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 4: UserPreferencesController

**Files:**
- Create: `Automotive.Marketplace.Server/Controllers/UserPreferencesController.cs`

Routes: `GET /UserPreferences/Get` (authenticated) and `PUT /UserPreferences/Upsert` (authenticated).

- [ ] **Step 1: Create controller**

```csharp
using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;
using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class UserPreferencesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [Protect(Permission.ViewListings)]
    public async Task<ActionResult<GetUserPreferencesResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPreferencesQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Protect(Permission.ViewListings)]
    public async Task<ActionResult> Upsert(
        [FromBody] UpsertUserPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = UserId }, cancellationToken);
        return NoContent();
    }
}
```

Note: `Permission.ViewListings` is the most permissive permission in the enum (any authenticated user with a standard role should have it). The `[Protect]` attribute ensures authentication + permission check. The `UserId` base controller property returns `Guid.Empty` for unauthenticated users, so an unauthenticated call would set `UserId = Guid.Empty` which would not match any user — this is safe.

Note also that `UpsertUserPreferencesCommand` is a `class`, not a `record`, so the `with` expression won't work. Set the property directly:
```csharp
command.UserId = UserId;
await mediator.Send(command, cancellationToken);
```

- [ ] **Step 2: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/UserPreferencesController.cs
git commit -m "feat: add UserPreferencesController with GET and PUT endpoints

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: Extend score engine to apply user weights

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQuery.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/GetListingScoreQueryHandler.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`

- [ ] **Step 1: Add UserId to query**

In `GetListingScoreQuery.cs`, add the optional property:
```csharp
public Guid? UserId { get; set; }
```

- [ ] **Step 2: Add IsPersonalized to response**

In `GetListingScoreResponse.cs`, add:
```csharp
public bool IsPersonalized { get; init; }
```

- [ ] **Step 3: Update handler to load user weights**

In `GetListingScoreQueryHandler.cs`, at the start of the `Handle` method (before calling `ListingScoreCalculator.Calculate`), add user preference lookup:

```csharp
ScoreWeights? weights = null;
bool isPersonalized = false;

if (request.UserId.HasValue)
{
    var prefs = await repository.AsQueryable<UserPreferences>()
        .FirstOrDefaultAsync(p => p.UserId == request.UserId.Value, cancellationToken);

    if (prefs != null)
    {
        weights = new ScoreWeights(prefs.ValueWeight, prefs.EfficiencyWeight, prefs.ReliabilityWeight, prefs.MileageWeight);
        isPersonalized = true;
    }
}
```

Also add `using Automotive.Marketplace.Domain.Entities;` to the handler's usings if not already present (for `UserPreferences`).

Update the `ListingScoreCalculator.Calculate` call to pass `weights`:
```csharp
var response = ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, market, efficiency, reliability, weights);
return response with { IsPersonalized = isPersonalized };
```

Note: `GetListingScoreResponse` is a `class`, so `with` expression won't work. Instead, create the result and set `IsPersonalized` separately. The cleanest approach is to modify `ListingScoreCalculator.Calculate` to accept an extra `bool isPersonalized` parameter and include it in the response. **Alternatively**, just create a helper to clone the response:

```csharp
var scoreResult = ListingScoreCalculator.Calculate(listing.Price, year, listing.Mileage, market, efficiency, reliability, weights);
return new GetListingScoreResponse
{
    OverallScore = scoreResult.OverallScore,
    Value = scoreResult.Value,
    Efficiency = scoreResult.Efficiency,
    Reliability = scoreResult.Reliability,
    Mileage = scoreResult.Mileage,
    HasMissingFactors = scoreResult.HasMissingFactors,
    MissingFactors = scoreResult.MissingFactors,
    IsPersonalized = isPersonalized,
};
```

- [ ] **Step 4: Pass UserId in controller**

In `ListingController.cs`, modify the `GetScore` action to forward the authenticated user's ID:

```csharp
[HttpGet]
public async Task<ActionResult<GetListingScoreResponse>> GetScore(
    [FromQuery] GetListingScoreQuery query,
    CancellationToken cancellationToken)
{
    query.UserId = UserId != Guid.Empty ? UserId : null;
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

- [ ] **Step 5: Build and run score tests**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q && \
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~GetListingScoreQueryHandlerTests" -q
```
Expected: All existing tests still PASS (UserId being null is the same as the previous behavior).

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ListingFeatures/GetListingScore/ \
        Automotive.Marketplace.Server/Controllers/ListingController.cs
git commit -m "feat: apply personalized weights in score engine for authenticated users

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: Frontend API layer for preferences

**Files:**
- Create: `automotive.marketplace.client/src/features/userPreferences/types/UserPreferencesResponse.ts`
- Create: `automotive.marketplace.client/src/features/userPreferences/types/UpsertUserPreferencesCommand.ts`
- Create: `automotive.marketplace.client/src/features/userPreferences/api/getUserPreferencesOptions.ts`
- Create: `automotive.marketplace.client/src/features/userPreferences/api/useUpsertUserPreferences.ts`
- Create: `automotive.marketplace.client/src/features/userPreferences/index.ts`
- Create: `automotive.marketplace.client/src/api/queryKeys/userPreferencesKeys.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts`

- [ ] **Step 1: Create types**

```typescript
// UserPreferencesResponse.ts
export type UserPreferencesResponse = {
  valueWeight: number;
  efficiencyWeight: number;
  reliabilityWeight: number;
  mileageWeight: number;
  autoGenerateAiSummary: boolean;
  hasPreferences: boolean;
};
```

```typescript
// UpsertUserPreferencesCommand.ts
export type UpsertUserPreferencesCommand = {
  valueWeight: number;
  efficiencyWeight: number;
  reliabilityWeight: number;
  mileageWeight: number;
  autoGenerateAiSummary: boolean;
};
```

- [ ] **Step 2: Add endpoint constants**

In `automotive.marketplace.client/src/constants/endpoints.ts`, add a new `USER_PREFERENCES` section:
```typescript
USER_PREFERENCES: {
  GET: "/UserPreferences/Get",
  UPSERT: "/UserPreferences/Upsert",
},
```

- [ ] **Step 3: Add isPersonalized to GetListingScoreResponse**

In `automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts`, add:
```typescript
isPersonalized: boolean;
```
to the `GetListingScoreResponse` type.

- [ ] **Step 4: Create query key**

Create `automotive.marketplace.client/src/api/queryKeys/userPreferencesKeys.ts`:
```typescript
export const userPreferencesKeys = {
  current: () => ["userPreferences"],
};
```

- [ ] **Step 5: Create query options**

```typescript
// getUserPreferencesOptions.ts
import { userPreferencesKeys } from "@/api/queryKeys/userPreferencesKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { UserPreferencesResponse } from "../types/UserPreferencesResponse";

export const getUserPreferencesOptions = queryOptions({
  queryKey: userPreferencesKeys.current(),
  queryFn: () => axiosClient.get<UserPreferencesResponse>(ENDPOINTS.USER_PREFERENCES.GET),
});
```

- [ ] **Step 6: Create mutation hook**

```typescript
// useUpsertUserPreferences.ts
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { userPreferencesKeys } from "@/api/queryKeys/userPreferencesKeys";
import { listingKeys } from "@/api/queryKeys/listingKeys";
import type { UpsertUserPreferencesCommand } from "../types/UpsertUserPreferencesCommand";

const upsertUserPreferences = (body: UpsertUserPreferencesCommand) =>
  axiosClient.put(ENDPOINTS.USER_PREFERENCES.UPSERT, body);

export const useUpsertUserPreferences = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: upsertUserPreferences,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userPreferencesKeys.current() });
      // Invalidate all score queries so they re-fetch with new weights
      queryClient.invalidateQueries({ queryKey: listingKeys.all() });
    },
    meta: {
      successMessage: "toasts:preferences.saved",
      errorMessage: "toasts:preferences.saveError",
    },
  });
};
```

- [ ] **Step 7: Create feature index**

```typescript
// index.ts
export { getUserPreferencesOptions } from "./api/getUserPreferencesOptions";
export { useUpsertUserPreferences } from "./api/useUpsertUserPreferences";
export { QuizModal } from "./components/QuizModal";
export type { UserPreferencesResponse } from "./types/UserPreferencesResponse";
export type { UpsertUserPreferencesCommand } from "./types/UpsertUserPreferencesCommand";
```

- [ ] **Step 8: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 9: Commit**

```bash
git add automotive.marketplace.client/src/features/userPreferences/ \
        automotive.marketplace.client/src/api/queryKeys/userPreferencesKeys.ts \
        automotive.marketplace.client/src/constants/endpoints.ts \
        automotive.marketplace.client/src/features/listingDetails/types/GetListingScoreResponse.ts
git commit -m "feat: add frontend API layer for user preferences

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: QuizModal component

**Files:**
- Create: `automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx`

The modal has 3 steps:
1. **Driving Style** — 3 scenario cards (user selects one):
   - `Car` icon: "City Commuter" — values efficiency & reliability more
   - `Gauge` icon: "Highway Driver" — values efficiency & value for money more
   - `Compass` icon: "Mixed Use" — balanced weights
2. **Priorities** — 4 toggle-able cards (user picks 1–4, ranked by click order):
   - `BadgeDollarSign`: "Value for Money"
   - `Leaf`: "Eco-Friendly"
   - `ShieldCheck`: "Dependability"
   - `TrendingDown`: "Low Mileage"
3. **Review & Adjust** — 4 labeled sliders (0–100, steps of 5). A live "Total" indicator shows the sum; sliders auto-normalize when total exceeds 100.

On Step 3 "Save":
- Normalize the 4 slider values to fractions summing to 1.0
- Call `useUpsertUserPreferences` mutation
- Close modal on success

Initial slider values on Step 3 are computed from Steps 1 + 2 using this mapping:
- Base weights: `{ value: 25, efficiency: 25, reliability: 25, mileage: 25 }` (equal start)
- Step 1 adjustments add/subtract points from this base:
  - City Commuter: `efficiency +15, reliability +10, value -15, mileage -10`
  - Highway Driver: `efficiency +15, value +10, reliability -15, mileage -10`
  - Mixed Use: no change (stays at 25/25/25/25)
- Step 2 adds 10 points to each selected priority (in selection order; first selected gets 15, second 10, third 5, fourth 0)
- Clamp each weight to [5, 60], then normalize to sum to 100

- [ ] **Step 1: Create QuizModal**

```tsx
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import {
  Car,
  Gauge,
  Compass,
  BadgeDollarSign,
  Leaf,
  ShieldCheck,
  TrendingDown,
} from "lucide-react";
import { useState } from "react";
import { useUpsertUserPreferences } from "../api/useUpsertUserPreferences";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialWeights?: {
    valueWeight: number;
    efficiencyWeight: number;
    reliabilityWeight: number;
    mileageWeight: number;
  };
};

type DrivingStyle = "city" | "highway" | "mixed";
type Priority = "value" | "efficiency" | "reliability" | "mileage";

const DRIVING_STYLES = [
  { id: "city" as DrivingStyle, label: "City Commuter", icon: Car, description: "Mostly urban stop-and-go driving" },
  { id: "highway" as DrivingStyle, label: "Highway Driver", icon: Gauge, description: "Long-distance highway cruising" },
  { id: "mixed" as DrivingStyle, label: "Mixed Use", icon: Compass, description: "A bit of everything" },
];

const PRIORITIES = [
  { id: "value" as Priority, label: "Value for Money", icon: BadgeDollarSign },
  { id: "efficiency" as Priority, label: "Eco-Friendly", icon: Leaf },
  { id: "reliability" as Priority, label: "Dependability", icon: ShieldCheck },
  { id: "mileage" as Priority, label: "Low Mileage", icon: TrendingDown },
];

const STYLE_ADJUSTMENTS: Record<DrivingStyle, Record<Priority, number>> = {
  city: { efficiency: 15, reliability: 10, value: -15, mileage: -10 },
  highway: { efficiency: 15, value: 10, reliability: -15, mileage: -10 },
  mixed: { efficiency: 0, value: 0, reliability: 0, mileage: 0 },
};

const PRIORITY_BONUSES = [15, 10, 5, 0];

function computeSliders(style: DrivingStyle, priorities: Priority[]): Record<Priority, number> {
  const base: Record<Priority, number> = { value: 25, efficiency: 25, reliability: 25, mileage: 25 };
  const adj = STYLE_ADJUSTMENTS[style];
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.max(5, Math.min(60, base[k] + (adj[k] ?? 0)));
  });
  priorities.forEach((p, i) => {
    base[p] = Math.min(60, base[p] + (PRIORITY_BONUSES[i] ?? 0));
  });
  const total = Object.values(base).reduce((a, b) => a + b, 0);
  (Object.keys(base) as Priority[]).forEach((k) => {
    base[k] = Math.round((base[k] / total) * 100);
  });
  return base;
}

function normalizeSliders(sliders: Record<Priority, number>): Record<Priority, number> {
  const total = Object.values(sliders).reduce((a, b) => a + b, 0);
  if (total === 0) return { value: 25, efficiency: 25, reliability: 25, mileage: 25 };
  const result = { ...sliders };
  (Object.keys(result) as Priority[]).forEach((k) => {
    result[k] = Math.round((result[k] / total) * 100);
  });
  return result;
}

export function QuizModal({ open, onOpenChange, initialWeights }: Props) {
  const [step, setStep] = useState(0);
  const [drivingStyle, setDrivingStyle] = useState<DrivingStyle>("mixed");
  const [priorities, setPriorities] = useState<Priority[]>([]);
  const [sliders, setSliders] = useState<Record<Priority, number>>(() => {
    if (initialWeights) {
      return {
        value: Math.round(initialWeights.valueWeight * 100),
        efficiency: Math.round(initialWeights.efficiencyWeight * 100),
        reliability: Math.round(initialWeights.reliabilityWeight * 100),
        mileage: Math.round(initialWeights.mileageWeight * 100),
      };
    }
    return { value: 25, efficiency: 25, reliability: 25, mileage: 25 };
  });

  const { mutateAsync: upsert, isPending } = useUpsertUserPreferences();

  const handleStyleSelect = (style: DrivingStyle) => setDrivingStyle(style);

  const handlePriorityToggle = (priority: Priority) => {
    setPriorities((prev) =>
      prev.includes(priority) ? prev.filter((p) => p !== priority) : [...prev, priority],
    );
  };

  const handleNextToStep2 = () => setStep(1);

  const handleNextToStep3 = () => {
    setSliders(computeSliders(drivingStyle, priorities));
    setStep(2);
  };

  const handleSliderChange = (key: Priority, value: number) => {
    setSliders((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    const normalized = normalizeSliders(sliders);
    const total = Object.values(normalized).reduce((a, b) => a + b, 0);
    const fraction = (v: number) => Math.round((v / total) * 1000) / 1000;
    await upsert({
      valueWeight: fraction(normalized.value),
      efficiencyWeight: fraction(normalized.efficiency),
      reliabilityWeight: fraction(normalized.reliability),
      mileageWeight: fraction(normalized.mileage),
      autoGenerateAiSummary: false,
    });
    onOpenChange(false);
    setStep(0);
  };

  const sliderTotal = Object.values(sliders).reduce((a, b) => a + b, 0);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>
            {step === 0 && "What describes your driving style?"}
            {step === 1 && "What matters most to you?"}
            {step === 2 && "Review your priorities"}
          </DialogTitle>
        </DialogHeader>

        {step === 0 && (
          <div className="grid grid-cols-3 gap-3">
            {DRIVING_STYLES.map(({ id, label, icon: Icon, description }) => (
              <button
                key={id}
                onClick={() => handleStyleSelect(id)}
                className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                  drivingStyle === id
                    ? "border-primary bg-primary/5"
                    : "border-border hover:border-muted-foreground"
                }`}
              >
                <Icon className="h-6 w-6" />
                <span className="text-sm font-medium">{label}</span>
                <span className="text-muted-foreground text-xs">{description}</span>
              </button>
            ))}
          </div>
        )}

        {step === 1 && (
          <div className="grid grid-cols-2 gap-3">
            {PRIORITIES.map(({ id, label, icon: Icon }) => {
              const rank = priorities.indexOf(id);
              const isSelected = rank !== -1;
              return (
                <button
                  key={id}
                  onClick={() => handlePriorityToggle(id)}
                  className={`flex flex-col items-center gap-2 rounded-lg border p-3 text-center transition-colors ${
                    isSelected
                      ? "border-primary bg-primary/5"
                      : "border-border hover:border-muted-foreground"
                  }`}
                >
                  <Icon className="h-5 w-5" />
                  <span className="text-sm font-medium">{label}</span>
                  {isSelected && (
                    <span className="bg-primary text-primary-foreground flex h-5 w-5 items-center justify-center rounded-full text-xs">
                      {rank + 1}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        )}

        {step === 2 && (
          <div className="space-y-4">
            <p className="text-muted-foreground text-sm">
              Drag the sliders to fine-tune. Total: {sliderTotal}%
              {sliderTotal !== 100 && (
                <span className="ml-1 text-orange-500">(will be normalized to 100%)</span>
              )}
            </p>
            {PRIORITIES.map(({ id, label }) => (
              <div key={id} className="space-y-1">
                <div className="flex justify-between text-sm">
                  <span>{label}</span>
                  <span className="font-medium">{sliders[id]}%</span>
                </div>
                <Slider
                  min={5}
                  max={60}
                  step={5}
                  value={[sliders[id]]}
                  onValueChange={([v]) => handleSliderChange(id, v)}
                />
              </div>
            ))}
          </div>
        )}

        <DialogFooter className="gap-2">
          {step > 0 && (
            <Button variant="outline" onClick={() => setStep(step - 1)}>
              Back
            </Button>
          )}
          {step < 2 && (
            <Button onClick={step === 0 ? handleNextToStep2 : handleNextToStep3}>
              Next
            </Button>
          )}
          {step === 2 && (
            <Button onClick={handleSave} disabled={isPending}>
              {isPending ? "Saving..." : "Save preferences"}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
```

Note: The `Slider` component from shadcn/ui must be installed. Check `automotive.marketplace.client/src/components/ui/slider.tsx`. If it doesn't exist, install it:
```bash
cd automotive.marketplace.client && npx shadcn@latest add slider
```

- [ ] **Step 2: Build**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/userPreferences/components/QuizModal.tsx
git commit -m "feat: add QuizModal with 3-step personalization flow

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Wire quiz entry points — ScoreCard + CompareScoreBanner

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx`
- Modify: `automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx`

- [ ] **Step 1: Update ScoreCard**

Add to ScoreCard's imports:
```tsx
import { SlidersHorizontal } from "lucide-react";
import { QuizModal } from "@/features/userPreferences";
import { getUserPreferencesOptions } from "@/features/userPreferences";
import { useAppSelector } from "@/hooks/redux";
```

Add inside the component function:
```tsx
const [quizOpen, setQuizOpen] = useState(false);
const { userId } = useAppSelector((state) => state.auth);
const { data: prefsData } = useQuery(getUserPreferencesOptions);
const isAuthenticated = !!userId;
const prefs = prefsData?.data;
```

Change the "Un-personalized" label to be dynamic:
```tsx
<p className="text-muted-foreground text-xs">
  {score.isPersonalized ? "Personalized" : "Un-personalized"}
</p>
```

Add a personalization button next to the score label (only for authenticated users):
```tsx
{isAuthenticated && (
  <button
    onClick={() => setQuizOpen(true)}
    className="text-muted-foreground hover:text-foreground ml-auto"
    aria-label="Personalize score weights"
  >
    <SlidersHorizontal className="h-4 w-4" />
  </button>
)}
```

Add the QuizModal at the end of the JSX (before closing div):
```tsx
<QuizModal
  open={quizOpen}
  onOpenChange={setQuizOpen}
  initialWeights={prefs?.hasPreferences ? {
    valueWeight: prefs.valueWeight,
    efficiencyWeight: prefs.efficiencyWeight,
    reliabilityWeight: prefs.reliabilityWeight,
    mileageWeight: prefs.mileageWeight,
  } : undefined}
/>
```

- [ ] **Step 2: Update CompareScoreBanner**

Add similar quiz entry point to `CompareScoreBanner`. Add a "Personalize" button row above the score columns:

Add to imports:
```tsx
import { SlidersHorizontal } from "lucide-react";
import { QuizModal } from "@/features/userPreferences";
import { getUserPreferencesOptions } from "@/features/userPreferences";
import { useQuery } from "@tanstack/react-query";
import { useAppSelector } from "@/hooks/redux";
```

Add inside `CompareScoreBanner`:
```tsx
const [quizOpen, setQuizOpen] = useState(false);
const { userId } = useAppSelector((state) => state.auth);
const { data: prefsData } = useQuery(getUserPreferencesOptions);
const isAuthenticated = !!userId;
const prefs = prefsData?.data;
```

Add a row above the scores grid:
```tsx
{isAuthenticated && (
  <div className="mb-3 flex items-center justify-between">
    <span className="text-muted-foreground text-sm">Vehicle Score</span>
    <button
      onClick={() => setQuizOpen(true)}
      className="text-muted-foreground hover:text-foreground flex items-center gap-1 text-xs"
    >
      <SlidersHorizontal className="h-3.5 w-3.5" />
      Personalize
    </button>
  </div>
)}

<QuizModal
  open={quizOpen}
  onOpenChange={setQuizOpen}
  initialWeights={prefs?.hasPreferences ? {
    valueWeight: prefs.valueWeight,
    efficiencyWeight: prefs.efficiencyWeight,
    reliabilityWeight: prefs.reliabilityWeight,
    mileageWeight: prefs.mileageWeight,
  } : undefined}
/>
```

- [ ] **Step 3: Build and lint**

```bash
cd automotive.marketplace.client && npm run build && npm run lint 2>&1 | tail -15
```
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/listingDetails/components/ScoreCard.tsx \
        automotive.marketplace.client/src/features/compareListings/components/CompareScoreBanner.tsx
git commit -m "feat: add quiz entry points to ScoreCard and CompareScoreBanner

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: Run full test suite and verify

- [ ] **Step 1: Run all backend tests**

```bash
dotnet test ./Automotive.Marketplace.sln -q 2>&1 | tail -20
```
Expected: All tests pass.

- [ ] **Step 2: Build frontend**

```bash
cd automotive.marketplace.client && npm run build && npm run lint 2>&1 | tail -10
```
Expected: no errors.

- [ ] **Step 3: Final commit**

```bash
git add -A
git commit -m "chore: Plan 2 complete — personalization quiz and user preferences

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
