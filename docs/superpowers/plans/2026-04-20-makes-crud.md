# Makes CRUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full CRUD (Create, Read, Update, Delete) for the `Make` entity, including BE CQRS handlers, controller endpoints, AutoMapper mappings, permission guards, handler integration tests, and a `makeList` React feature with an AlertDialog delete confirmation.

**Architecture:** Mirror the existing Models CRUD pattern exactly. New BE handlers go under `Application/Features/MakeFeatures/`. The new `makeList` FE feature lives at `src/features/makeList/` and reuses the global `getAllMakesOptions` (shared with `MakeSelect`). Tests use TestContainers + Respawn via `DatabaseFixture<T>`.

**Tech Stack:** ASP.NET Core 8, MediatR, AutoMapper, EF Core, xUnit, TestContainers, Respawn, Bogus, FluentAssertions; React 19, TypeScript, TanStack Query, Axios, Zod, react-hook-form, shadcn/ui

---

## Task 1: Add Make Permissions to Domain

**Files:**
- Modify: `Automotive.Marketplace.Domain/Enums/Permission.cs`

- [ ] **Step 1: Add the three new permission values**

Replace the existing content of `Permission.cs` with:
```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum Permission
{
    CreateListings,
    ManageListings,
    ViewListings,
    ViewModels,
    CreateModels,
    ManageModels,
    ViewVariants,
    CreateVariants,
    ManageVariants,
    ViewMakes,
    CreateMakes,
    ManageMakes
}
```

- [ ] **Step 2: Build to confirm no compilation errors**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Domain/Enums/Permission.cs
git commit -m "feat: add ViewMakes, CreateMakes, ManageMakes permissions

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: GetAllMakesResponse — Add Audit Fields

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/MakeFeatures/GetAllMakes/GetAllMakesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/MakeMappings.cs`

- [ ] **Step 1: Add audit fields to `GetAllMakesResponse`**

Replace the file content:
```csharp
namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;

public sealed record GetAllMakesResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Update `MakeMappings.cs` to map audit fields (AutoMapper maps by convention so no explicit members needed, but keep it explicit for clarity)**

Replace the file content:
```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>();
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Features/MakeFeatures/GetAllMakes/GetAllMakesResponse.cs \
        Automotive.Marketplace.Application/Mappings/MakeMappings.cs
git commit -m "feat: add audit fields to GetAllMakesResponse

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: GetMakeById — Query, Handler, Response

**Files:**
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/GetMakeById/GetMakeByIdQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/GetMakeById/GetMakeByIdResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/GetMakeById/GetMakeByIdQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/MakeMappings.cs`

- [ ] **Step 1: Create `GetMakeByIdQuery.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;

public sealed record GetMakeByIdQuery : IRequest<GetMakeByIdResponse>
{
    public Guid Id { get; set; }
}
```

- [ ] **Step 2: Create `GetMakeByIdResponse.cs`**

```csharp
namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;

public sealed record GetMakeByIdResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Create `GetMakeByIdQueryHandler.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;

public class GetMakeByIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetMakeByIdQuery, GetMakeByIdResponse>
{
    public async Task<GetMakeByIdResponse> Handle(GetMakeByIdQuery request, CancellationToken cancellationToken)
    {
        var make = await repository.GetByIdAsync<Make>(request.Id, cancellationToken);
        return mapper.Map<GetMakeByIdResponse>(make);
    }
}
```

- [ ] **Step 4: Add `Make → GetMakeByIdResponse` mapping in `MakeMappings.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>();
        CreateMap<Make, GetMakeByIdResponse>();
    }
}
```

- [ ] **Step 5: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/MakeFeatures/GetMakeById/ \
        Automotive.Marketplace.Application/Mappings/MakeMappings.cs
git commit -m "feat: add GetMakeById query handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: CreateMake — Command and Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/CreateMake/CreateMakeCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/CreateMake/CreateMakeCommandHandler.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/MakeMappings.cs`

- [ ] **Step 1: Create `CreateMakeCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;

public sealed record CreateMakeCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create `CreateMakeCommandHandler.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;

public class CreateMakeCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<CreateMakeCommand>
{
    public async Task Handle(CreateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = mapper.Map<Make>(request);
        await repository.CreateAsync(make, cancellationToken);
    }
}
```

- [ ] **Step 3: Add `CreateMakeCommand → Make` mapping in `MakeMappings.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>();
        CreateMap<Make, GetMakeByIdResponse>();
        CreateMap<CreateMakeCommand, Make>();
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/MakeFeatures/CreateMake/ \
        Automotive.Marketplace.Application/Mappings/MakeMappings.cs
git commit -m "feat: add CreateMake command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: UpdateMake — Command and Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/UpdateMake/UpdateMakeCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/UpdateMake/UpdateMakeCommandHandler.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/MakeMappings.cs`

- [ ] **Step 1: Create `UpdateMakeCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;

public sealed record UpdateMakeCommand : IRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create `UpdateMakeCommandHandler.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;

public class UpdateMakeCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<UpdateMakeCommand>
{
    public async Task Handle(UpdateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = mapper.Map<Make>(request);
        await repository.UpdateAsync(make, cancellationToken);
    }
}
```

- [ ] **Step 3: Add `UpdateMakeCommand → Make` mapping in `MakeMappings.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>();
        CreateMap<Make, GetMakeByIdResponse>();
        CreateMap<CreateMakeCommand, Make>();
        CreateMap<UpdateMakeCommand, Make>();
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/MakeFeatures/UpdateMake/ \
        Automotive.Marketplace.Application/Mappings/MakeMappings.cs
git commit -m "feat: add UpdateMake command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: DeleteMake — Command and Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/DeleteMake/DeleteMakeCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/MakeFeatures/DeleteMake/DeleteMakeCommandHandler.cs`

- [ ] **Step 1: Create `DeleteMakeCommand.cs`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;

public sealed record DeleteMakeCommand : IRequest
{
    public Guid Id { get; set; }
}
```

- [ ] **Step 2: Create `DeleteMakeCommandHandler.cs`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;

public class DeleteMakeCommandHandler(IRepository repository) : IRequestHandler<DeleteMakeCommand>
{
    public async Task Handle(DeleteMakeCommand request, CancellationToken cancellationToken)
    {
        var make = await repository.GetByIdAsync<Make>(request.Id, cancellationToken);
        await repository.DeleteAsync(make, cancellationToken);
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Automotive.Marketplace.Application/Features/MakeFeatures/DeleteMake/
git commit -m "feat: add DeleteMake command handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 7: Update MakeController

**Files:**
- Modify: `Automotive.Marketplace.Server/Controllers/MakeController.cs`

- [ ] **Step 1: Replace `MakeController.cs` with full CRUD endpoints**

```csharp
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class MakeController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMakesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllMakesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.ViewMakes)]
    public async Task<ActionResult<GetMakeByIdResponse>> GetById([FromQuery] GetMakeByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageMakes, Permission.CreateMakes)]
    public async Task<ActionResult> Create([FromBody] CreateMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Created();
    }

    [HttpPut]
    [Protect(Permission.ManageMakes)]
    public async Task<ActionResult> Update([FromBody] UpdateMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [Protect(Permission.ManageMakes)]
    public async Task<ActionResult> Delete([FromQuery] DeleteMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build ./Automotive.Marketplace.sln --no-restore -q
```
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add Automotive.Marketplace.Server/Controllers/MakeController.cs
git commit -m "feat: add GetById, Create, Update, Delete endpoints to MakeController

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 8: Handler Integration Tests

**Files:**
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/GetAllMakesQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/GetMakeByIdQueryHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/CreateMakeCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/UpdateMakeCommandHandlerTests.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/DeleteMakeCommandHandlerTests.cs`

- [ ] **Step 1: Create `GetAllMakesQueryHandlerTests.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class GetAllMakesQueryHandlerTests(
    DatabaseFixture<GetAllMakesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllMakesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllMakesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetAllMakesQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetAllMakesQueryHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_WithMakes_ShouldReturnAllMakesOrderedByName()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var makes = new MakeBuilder().Build(3);
        await context.AddRangeAsync(makes);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);
        var list = result.ToList();

        // Assert
        list.Should().HaveCount(3);
        list.Should().BeInAscendingOrder(m => m.Name);
    }

    [Fact]
    public async Task Handle_WithNoMakes_ShouldReturnEmptyCollection()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        // Act
        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAuditFields()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder()
            .With(m => m.CreatedBy, "admin")
            .Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);
        var returned = result.Single();

        // Assert
        returned.CreatedBy.Should().Be("admin");
        returned.CreatedAt.Should().NotBe(default);
    }
}
```

- [ ] **Step 2: Create `GetMakeByIdQueryHandlerTests.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class GetMakeByIdQueryHandlerTests(
    DatabaseFixture<GetMakeByIdQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetMakeByIdQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetMakeByIdQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetMakeByIdQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetMakeByIdQueryHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldReturnCorrectMake()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var query = new GetMakeByIdQuery { Id = make.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(make.Id);
        result.Name.Should().Be(make.Name);
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldThrowException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var query = new GetMakeByIdQuery { Id = Guid.NewGuid() };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 3: Create `CreateMakeCommandHandlerTests.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class CreateMakeCommandHandlerTests(
    DatabaseFixture<CreateMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private CreateMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new CreateMakeCommandHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistMake()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var command = new CreateMakeCommand { Name = "Toyota" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var makes = await context.Set<Make>().ToListAsync();
        makes.Should().HaveCount(1);
        makes.First().Name.Should().Be("Toyota");
    }
}
```

- [ ] **Step 4: Create `UpdateMakeCommandHandlerTests.cs`**

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class UpdateMakeCommandHandlerTests(
    DatabaseFixture<UpdateMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private UpdateMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new UpdateMakeCommandHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldUpdateName()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().With(m => m.Name, "OldName").Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var command = new UpdateMakeCommand { Id = make.Id, Name = "NewName" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await context.Set<Make>().FirstAsync();
        updated.Name.Should().Be("NewName");
    }
}
```

- [ ] **Step 5: Create `DeleteMakeCommandHandlerTests.cs`**

```csharp
using Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class DeleteMakeCommandHandlerTests(
    DatabaseFixture<DeleteMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private DeleteMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new DeleteMakeCommandHandler(repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldRemoveMake()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var command = new DeleteMakeCommand { Id = make.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var makes = await context.Set<Make>().ToListAsync();
        makes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldThrowException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var command = new DeleteMakeCommand { Id = Guid.NewGuid() };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
```

- [ ] **Step 6: Run the new Make handler tests**

```bash
dotnet test ./Automotive.Marketplace.sln --filter "FullyQualifiedName~MakeHandlerTests" -v normal
```
Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Tests/Features/HandlerTests/MakeHandlerTests/
git commit -m "test: add Make handler integration tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 9: Frontend — Update Shared Files

**Files:**
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/types/make/GetAllMakesResponse.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/makeKeys.ts`

- [ ] **Step 1: Extend `ENDPOINTS.MAKE` in `endpoints.ts`**

Replace the `MAKE` block:
```ts
  MAKE: {
    GET_ALL: "/Make/GetAll",
    GET_BY_ID: "/Make/GetById",
    CREATE: "/Make/Create",
    UPDATE: "/Make/Update",
    DELETE: "/Make/Delete",
  },
```

- [ ] **Step 2: Add audit fields to `GetAllMakesResponse.ts`**

```ts
export type GetAllMakesResponse = {
  id: string;
  name: string;
  createdAt: string;
  modifiedAt: string | null;
  createdBy: string;
  modifiedBy: string;
};
```

- [ ] **Step 3: Add `byId` key to `makeKeys.ts`**

```ts
export const makeKeys = {
  all: () => ["make"],
  byId: (id: string) => ["make", id],
};
```

- [ ] **Step 4: Run FE lint/typecheck**

```bash
cd automotive.marketplace.client && npm run lint && npm run build 2>&1 | tail -20
```
Expected: No errors.

- [ ] **Step 5: Commit**

```bash
cd .. && git add automotive.marketplace.client/src/constants/endpoints.ts \
               automotive.marketplace.client/src/types/make/GetAllMakesResponse.ts \
               automotive.marketplace.client/src/api/queryKeys/makeKeys.ts
git commit -m "feat: extend Make endpoints, queryKeys, and GetAllMakesResponse type

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 10: Frontend — makeList Types and Schema

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/types/MakeFormData.ts`
- Create: `automotive.marketplace.client/src/features/makeList/types/CreateMakeCommand.ts`
- Create: `automotive.marketplace.client/src/features/makeList/types/UpdateMakeCommand.ts`
- Create: `automotive.marketplace.client/src/features/makeList/types/DeleteMakeCommand.ts`
- Create: `automotive.marketplace.client/src/features/makeList/types/GetMakeByIdQuery.ts`
- Create: `automotive.marketplace.client/src/features/makeList/types/GetMakeByIdResponse.ts`
- Create: `automotive.marketplace.client/src/features/makeList/schemas/makeFormSchema.ts`

- [ ] **Step 1: Create `makeFormSchema.ts`**

```ts
import z from "zod";

export const makeFormSchema = z.object({
  name: z.string().min(1, "Name is required"),
});
```

- [ ] **Step 2: Create `MakeFormData.ts`**

```ts
import z from "zod";
import { makeFormSchema } from "../schemas/makeFormSchema";

export type MakeFormData = z.infer<typeof makeFormSchema>;
```

- [ ] **Step 3: Create `CreateMakeCommand.ts`**

```ts
export type CreateMakeCommand = {
  name: string;
};
```

- [ ] **Step 4: Create `UpdateMakeCommand.ts`**

```ts
export type UpdateMakeCommand = {
  id: string;
  name: string;
};
```

- [ ] **Step 5: Create `DeleteMakeCommand.ts`**

```ts
export type DeleteMakeCommand = {
  id: string;
};
```

- [ ] **Step 6: Create `GetMakeByIdQuery.ts`**

```ts
export type GetMakeByIdQuery = {
  id: string;
};
```

- [ ] **Step 7: Create `GetMakeByIdResponse.ts`**

```ts
export type GetMakeByIdResponse = {
  id: string;
  name: string;
  createdAt: string;
  modifiedAt: string | null;
  createdBy: string;
  modifiedBy: string;
};
```

- [ ] **Step 8: Commit**

```bash
git add automotive.marketplace.client/src/features/makeList/
git commit -m "feat: add makeList feature types and schema

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 11: Frontend — makeList API Hooks

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/api/getMakeByIdOptions.ts`
- Create: `automotive.marketplace.client/src/features/makeList/api/useCreateMake.ts`
- Create: `automotive.marketplace.client/src/features/makeList/api/useUpdateMake.ts`
- Create: `automotive.marketplace.client/src/features/makeList/api/useDeleteMake.ts`

- [ ] **Step 1: Create `getMakeByIdOptions.ts`**

```ts
import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetMakeByIdQuery } from "../types/GetMakeByIdQuery";
import { GetMakeByIdResponse } from "../types/GetMakeByIdResponse";

const getMakeById = (query: GetMakeByIdQuery) =>
  axiosClient.get<GetMakeByIdResponse>(ENDPOINTS.MAKE.GET_BY_ID, {
    params: query,
  });

export const getMakeByIdOptions = (query: GetMakeByIdQuery) =>
  queryOptions({
    queryKey: makeKeys.byId(query.id),
    queryFn: () => getMakeById(query),
  });
```

- [ ] **Step 2: Create `useCreateMake.ts`**

```ts
import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateMakeCommand } from "../types/CreateMakeCommand";

const createMake = (body: CreateMakeCommand) =>
  axiosClient.post<void>(ENDPOINTS.MAKE.CREATE, body);

export const useCreateMake = () =>
  useMutation({
    mutationFn: createMake,
    meta: {
      successMessage: "Successfully created make!",
      errorMessage: "Sorry, we couldn't create your make",
      invalidatesQuery: makeKeys.all(),
    },
  });
```

- [ ] **Step 3: Create `useUpdateMake.ts`**

```ts
import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateMakeCommand } from "../types/UpdateMakeCommand";

const updateMake = (body: UpdateMakeCommand) =>
  axiosClient.put<void>(ENDPOINTS.MAKE.UPDATE, body);

export const useUpdateMake = () =>
  useMutation({
    mutationFn: updateMake,
    meta: {
      successMessage: "Successfully updated make!",
      errorMessage: "Sorry, we couldn't update your make",
      invalidatesQuery: makeKeys.all(),
    },
  });
```

- [ ] **Step 4: Create `useDeleteMake.ts`**

```ts
import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteMakeCommand } from "../types/DeleteMakeCommand";

const deleteMake = (query: DeleteMakeCommand) =>
  axiosClient.delete<void>(ENDPOINTS.MAKE.DELETE, { params: query });

export const useDeleteMake = () =>
  useMutation({
    mutationFn: deleteMake,
    meta: {
      successMessage: "Successfully deleted make!",
      errorMessage: "Sorry, we had trouble deleting your make",
      invalidatesQuery: makeKeys.all(),
    },
  });
```

- [ ] **Step 5: Commit**

```bash
git add automotive.marketplace.client/src/features/makeList/api/
git commit -m "feat: add makeList API hooks

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 12: Frontend — MakeForm Component

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/components/MakeForm.tsx`

- [ ] **Step 1: Create `MakeForm.tsx`**

```tsx
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { makeFormSchema } from "../schemas/makeFormSchema";
import { MakeFormData } from "../types/MakeFormData";

type MakeFormProps = {
  make: MakeFormData;
  onSubmit: (formData: MakeFormData) => Promise<void>;
  className?: string;
};

const MakeForm = ({ make, onSubmit, className }: MakeFormProps) => {
  const form = useForm({
    defaultValues: { name: make.name },
    resolver: zodResolver(makeFormSchema),
  });

  const handleSubmit = async (formData: MakeFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
        >
          <FormField
            name="name"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Make name</FormLabel>
                <FormControl>
                  <Input type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit">Confirm</Button>
        </form>
      </Form>
    </div>
  );
};

export default MakeForm;
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/makeList/components/MakeForm.tsx
git commit -m "feat: add MakeForm component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 13: Frontend — Edit Dialogs

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/components/EditMakeDialogContent.tsx`
- Create: `automotive.marketplace.client/src/features/makeList/components/EditMakeDialog.tsx`

- [ ] **Step 1: Create `EditMakeDialogContent.tsx`**

```tsx
import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getMakeByIdOptions } from "../api/getMakeByIdOptions";
import { MakeFormData } from "../types/MakeFormData";
import MakeForm from "./MakeForm";

type EditMakeDialogContentProps = {
  id: string;
  onSubmit: (formData: MakeFormData) => Promise<void>;
};

const EditMakeDialogContent = ({ id, onSubmit }: EditMakeDialogContentProps) => {
  const { data: makeQuery } = useSuspenseQuery(getMakeByIdOptions({ id }));
  const make = makeQuery.data;

  return (
    <div>
      <DialogHeader>
        <DialogTitle>Edit {make.name}</DialogTitle>
      </DialogHeader>
      <MakeForm make={{ name: make.name }} onSubmit={onSubmit} />
    </div>
  );
};

export default EditMakeDialogContent;
```

- [ ] **Step 2: Create `EditMakeDialog.tsx`**

```tsx
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useUpdateMake } from "../api/useUpdateMake";
import { MakeFormData } from "../types/MakeFormData";
import EditMakeDialogContent from "./EditMakeDialogContent";

type EditMakeDialogProps = {
  id: string;
};

const EditMakeDialog = ({ id }: EditMakeDialogProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const { mutateAsync: updateMakeAsync } = useUpdateMake();

  const handleSubmit = async (formData: MakeFormData) => {
    await updateMakeAsync({ ...formData, id });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <EditMakeDialogContent id={id} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default EditMakeDialog;
```

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/makeList/components/EditMakeDialogContent.tsx \
        automotive.marketplace.client/src/features/makeList/components/EditMakeDialog.tsx
git commit -m "feat: add EditMakeDialog components

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 14: Frontend — CreateMakeDialog

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/components/CreateMakeDialog.tsx`

- [ ] **Step 1: Create `CreateMakeDialog.tsx`**

```tsx
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useCreateMake } from "../api/useCreateMake";
import { MakeFormData } from "../types/MakeFormData";
import MakeForm from "./MakeForm";

const CreateMakeDialog = () => {
  const [isOpen, setIsOpen] = useState<boolean>();
  const { mutateAsync: createMakeAsync } = useCreateMake();

  const handleSubmit = async (formData: MakeFormData) => {
    await createMakeAsync({ ...formData });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button>Add make</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create new make</DialogTitle>
        </DialogHeader>
        <MakeForm make={{ name: "" }} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default CreateMakeDialog;
```

- [ ] **Step 2: Commit**

```bash
git add automotive.marketplace.client/src/features/makeList/components/CreateMakeDialog.tsx
git commit -m "feat: add CreateMakeDialog component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 15: Frontend — MakeListTable with AlertDialog

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/components/MakeListTable.tsx`

- [ ] **Step 1: Install the shadcn `alert-dialog` component**

```bash
cd automotive.marketplace.client && npx shadcn@latest add alert-dialog --yes
```
Expected: `automotive.marketplace.client/src/components/ui/alert-dialog.tsx` is created.

- [ ] **Step 2: Create `MakeListTable.tsx`**

```tsx
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
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getAllMakesOptions } from "@/api/make/getAllMakesOptions";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { useDeleteMake } from "../api/useDeleteMake";
import EditMakeDialog from "./EditMakeDialog";

type MakeListTableProps = {
  className?: string;
};

const MakeListTable = ({ className }: MakeListTableProps) => {
  const { data: makesQuery } = useQuery(getAllMakesOptions);
  const makes = makesQuery?.data || [];

  const { mutateAsync: deleteMakeAsync } = useDeleteMake();

  const handleDelete = async (id: string) => {
    await deleteMakeAsync({ id });
  };

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>A list of makes</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Created by</TableHead>
            <TableHead>Created at</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {makes.map((m) => (
            <TableRow key={m.id}>
              <TableCell>{m.name}</TableCell>
              <TableCell>{m.createdBy}</TableCell>
              <TableCell>{new Date(m.createdAt).toLocaleDateString()}</TableCell>
              <TableCell className="flex gap-1">
                <EditMakeDialog id={m.id} />
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button variant="secondary">
                      <Trash />
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Delete {m.name}?</AlertDialogTitle>
                      <AlertDialogDescription>
                        This action cannot be undone.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction onClick={() => handleDelete(m.id)}>
                        Delete
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default MakeListTable;
```

- [ ] **Step 4: Run lint**

```bash
npm run lint 2>&1 | tail -20
```
Expected: No errors.

- [ ] **Step 5: Commit**

```bash
cd .. && git add automotive.marketplace.client/src/components/ui/alert-dialog.tsx \
               automotive.marketplace.client/src/features/makeList/components/MakeListTable.tsx
git commit -m "feat: add MakeListTable with AlertDialog delete confirmation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 16: Frontend — Feature Index + Final Build

**Files:**
- Create: `automotive.marketplace.client/src/features/makeList/index.ts`

- [ ] **Step 1: Create `index.ts`**

```ts
export { default as CreateMakeDialog } from "./components/CreateMakeDialog";
export { default as MakeListTable } from "./components/MakeListTable";
```

- [ ] **Step 3: Run the full frontend build to verify no type errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -30
```
Expected: Build completes without TypeScript errors.

- [ ] **Step 3: Run lint**

```bash
npm run lint 2>&1 | tail -20
```
Expected: No errors.

- [ ] **Step 4: Commit**

```bash
cd .. && git add automotive.marketplace.client/src/features/makeList/index.ts
git commit -m "feat: add makeList feature index

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 17: Run Full Test Suite

- [ ] **Step 1: Run all backend tests**

```bash
dotnet test ./Automotive.Marketplace.sln -v normal 2>&1 | tail -30
```
Expected: All tests pass with no failures.

- [ ] **Step 2: Final commit if needed**

If any minor fixes were made, commit them:
```bash
git add -A && git commit -m "fix: address any post-test-run issues

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
