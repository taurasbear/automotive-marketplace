---
name: be-handler-tests
description: Use when writing integration tests for a CQRS handler in Automotive.Marketplace.Tests
---

# Backend Handler Integration Tests

## Overview

Tests use a real PostgreSQL container (via TestContainers). Each test **class** gets its own dedicated database. State is reset between **methods** using Respawn — no container restart needed.

## Test Class Structure

```csharp
// Must: IClassFixture<DatabaseFixture<TSelf>> + IAsyncLifetime
public class CreateListingCommandHandlerTests(
    DatabaseFixture<CreateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateListingCommandHandlerTests> _fixture = fixture;

    // Mock external services only — IRepository is the real thing
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    // REQUIRED: reset DB state after every test
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    // Factory method — keeps handler construction in one place
    private MyHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new MyHandler(repository, mapper, _imageStorageService);
    }

    [Fact]
    public async Task Handle_HappyPath_ShouldCreateEntity()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        // Seed via Builders, then save with context directly
        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var command = new MyCommand(...);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert with FluentAssertions
        result.Should().NotBeNull();
        var saved = await context.Set<MyEntity>().ToListAsync();
        saved.Should().HaveCount(1);
    }
}
```

## Builders

Builders live in `Automotive.Marketplace.Infrastructure/Data/Builders/`. They use Bogus and follow a fluent `.WithXxx()` pattern:

```csharp
var make  = new MakeBuilder().Build();
var model = new ModelBuilder().WithMake(make.Id).Build();
var user  = new UserBuilder().Build();

// Override any property with the generic With()
var variant = new VariantBuilder()
    .WithModel(model.Id)
    .WithFuel(fuel.Id)
    .With(v => v.IsCustom, false)
    .Build();

await context.AddRangeAsync(make, model, user, variant);
await context.SaveChangesAsync();
```

**Always add a Builder** for any new entity so future tests can use it.

## Seed Helper Pattern

For tests requiring many related entities, extract a private seed method:

```csharp
private static async Task<(Make make, Model model, User user)>
    SeedRequiredEntitiesAsync(AutomotiveContext context)
{
    var make  = new MakeBuilder().Build();
    var model = new ModelBuilder().WithMake(make.Id).Build();
    var user  = new UserBuilder().Build();
    await context.AddRangeAsync(make, model, user);
    await context.SaveChangesAsync();
    return (make, model, user);
}
```

## Quick Reference

| Concern | How |
|---------|-----|
| Real DB | `DatabaseFixture<T>` — one container, one DB per class |
| State reset | `_fixture.ResetDatabaseAsync()` in `DisposeAsync` |
| Seed data | Builders + `context.AddRangeAsync` + `context.SaveChangesAsync` |
| Mock services | `NSubstitute.Substitute.For<IMyService>()` |
| Assertions | FluentAssertions (`.Should().Be()`, `.Should().HaveCount()`) |
| Async throws | `Func<Task> act = async () => await handler.Handle(...)` + `.Should().ThrowAsync<T>()` |
| Run one class | `dotnet test --filter "FullyQualifiedName~MyHandlerTests"` |

## Common Mistakes

- **Forgetting `DisposeAsync`** → state bleeds between tests, causing false failures
- **Injecting `AutomotiveContext` into handler** → use `IRepository` in handlers; `AutomotiveContext` is for test seeding/assertions only
- **Not using `CreateAsyncScope`** → causes scoping errors with `Transient` DbContext lifetime
