---
name: be-cqrs-feature
description: Use when adding a new backend operation (create, get, update, delete) — a new command or query handler in the Application layer
---

# Backend CQRS Feature

## Overview

Each operation is a self-contained folder under `Automotive.Marketplace.Application/Features/<FeatureName>/<OperationName>/` containing 3–4 files.

## File Structure

```
Features/
  ListingFeatures/
    CreateListing/
      CreateListingCommand.cs         # Input: sealed record, implements IRequest<TResponse>
      CreateListingCommandHandler.cs  # Logic: IRequestHandler<TCommand, TResponse>
      CreateListingResponse.cs        # Output: sealed record
      CreateListingCommandValidator.cs # Optional: FluentValidation AbstractValidator<TCommand>
    GetAllListings/
      GetAllListingsQuery.cs
      GetAllListingsQueryHandler.cs
      GetAllListingsResponse.cs
```

**Naming:** Commands mutate state. Queries read state. Both end in `Command`/`Query`.

## Command (write operation)

```csharp
// Command — positional record preferred for small payloads
public sealed record CreateListingCommand(
    decimal Price,
    int Mileage,
    Guid SellerId
) : IRequest<CreateListingResponse>;

// Handler — inject only IRepository, IMapper, and domain services via primary constructor
public class CreateListingCommandHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<CreateListingCommand, CreateListingResponse>
{
    public async Task<CreateListingResponse> Handle(
        CreateListingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate existence of related entities via repository.GetByIdAsync<T>()
        // 2. Build and persist the new entity
        var listing = new Listing { Id = Guid.NewGuid(), Price = request.Price, ... };
        await repository.CreateAsync(listing, cancellationToken);
        // 3. Map to response
        return mapper.Map<CreateListingResponse>(listing);
    }
}
```

## Query (read operation)

```csharp
// Query — class with optional filter properties
public sealed record GetAllListingsQuery : IRequest<IEnumerable<GetAllListingsResponse>>
{
    public Guid? MakeId { get; init; }
    public int? MinYear { get; init; }
    // ...more filters
}

// Handler — use AsQueryable<T>() for filtered/included queries
public class GetAllListingsQueryHandler(IMapper mapper, IRepository repository)
    : IRequestHandler<GetAllListingsQuery, IEnumerable<GetAllListingsResponse>>
{
    public async Task<IEnumerable<GetAllListingsResponse>> Handle(
        GetAllListingsQuery request, CancellationToken cancellationToken)
    {
        var results = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant).ThenInclude(v => v.Model)
            .Where(l => request.MakeId == null || l.Variant.Model.MakeId == request.MakeId)
            .ToListAsync(cancellationToken);

        return results.Select(mapper.Map<GetAllListingsResponse>);
    }
}
```

## Key Rules

| Rule | Detail |
|------|--------|
| **Never inject `AutomotiveContext`** | Use `IRepository` in handlers; `AutomotiveContext` is for tests only |
| **Navigation props are `virtual`** | LazyLoadingProxies is enabled |
| **Throw domain exceptions** | e.g. `DbEntityNotFoundException`, `InvalidCredentialsException` — filters handle HTTP mapping |
| **Enum serialization** | `JsonStringEnumConverter` is global — enums are strings on the wire |
| **Response is a `sealed record`** | Makes it immutable and concise |
| **Validator is optional** | Add `*Validator.cs` using FluentValidation `AbstractValidator<T>` only when input validation is needed |

## AutoMapper Profile

Add mappings in `Automotive.Marketplace.Application/Mappings/<Entity>Mappings.cs`:

```csharp
public class ListingMappings : Profile
{
    public ListingMappings()
    {
        CreateMap<Listing, GetAllListingsResponse>();
        CreateMap<Listing, CreateListingResponse>();
    }
}
```

## Controller Wiring

Controllers inherit `BaseController` (`[Route("[controller]/[action]")]`). Inject `IMediator` and call `mediator.Send(command, cancellationToken)`. Use `BaseController.UserId` for the authenticated user's `Guid`.

```csharp
public class ListingController(IMediator mediator) : BaseController
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreateListingResponse>> Create(
        [FromBody] CreateListingCommand command, CancellationToken cancellationToken)
        => Ok(await mediator.Send(command, cancellationToken));
}
```
