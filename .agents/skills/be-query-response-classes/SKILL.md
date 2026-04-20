---
name: be-query-response-classes
description: Use when creating Query and Response classes for backend CQRS handlers—defines naming conventions, nesting patterns, and structure for no-DTO responses with nested types
---

# Query and Response Classes

## Overview

Automotive.Marketplace uses a strict naming convention for CQRS Query and Response classes: **Query** and **Response** only—never DTO, Request, Result, or Output naming. Nested types live inside Response classes as inner classes, not as separate DTOs.

**Core principle:** Responses are self-contained. Related data types nest inside the response class that needs them.

## When to Use

Use this when:
- Creating a new Query handler in the Application layer
- Defining a Response for that handler
- Structuring complex response data with nested types (images, metadata, related objects)

Do NOT use separate DTO files for types only used in a single Response.

## Naming Convention

### Query Classes

Format: `Get[Subject][ByCondition]Query`

Examples:
- `GetAllListingsQuery`
- `GetListingByIdQuery`
- `GetMakesByNameQuery`
- `GetListingsByLocationQuery`

**Structure:**
```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllListingsQuery : IRequest<IEnumerable<GetAllListingsResponse>>
{
    // Query parameters (filters, pagination)
    public int? Page { get; set; }
    public string? City { get; set; }
    public Guid? MakeId { get; set; }
}
```

**Use sealed records** for immutability and performance. Include filter parameters as public properties.

### Response Classes

Format: `Get[Subject][ByCondition]Response`

Examples:
- `GetAllListingsResponse`
- `GetListingByIdResponse`
- `GetMakesByNameResponse`
- `GetListingsByLocationResponse`

**Structure:**
```csharp
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // Nested types go INSIDE the response, not as separate files
    public IEnumerable<Image> Images { get; set; } = [];
    
    public sealed record Image
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}
```

Use sealed records. Include default empty values for strings and collections.

## Nested Types Pattern

### When to Create Nested Classes

If a type is **only used inside one Response**, nest it inside that Response class as an inner class.

✅ **CORRECT: Nest related types**
```csharp
public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public IEnumerable<Image> Images { get; set; } = [];
    public Seller SellerInfo { get; set; } = null!;
    
    // Only used in GetAllListingsResponse
    public sealed record Image
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
    
    // Only used in GetAllListingsResponse
    public sealed record Seller
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
```

❌ **WRONG: Create separate DTO files for response-only types**
```csharp
// ImageDto.cs (separate file) - DON'T DO THIS
public sealed record ImageDto { ... }

// SellerDto.cs (separate file) - DON'T DO THIS
public sealed record SellerDto { ... }
```

### When to Use Separate Files

Use a separate file if the type is **shared across multiple Responses** (e.g., pagination metadata used by all query responses). Example: `PaginationMetadata.cs`

```csharp
// Shared across responses - appropriate for separate file
public sealed record PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
```

## Accessing Nested Types

Nested types use dot notation:

```csharp
// In handler or test
var response = new GetAllListingsResponse
{
    Images = new[]
    {
        new GetAllListingsResponse.Image { Url = "...", AltText = "..." }
    }
};

// Usage
foreach (var image in response.Images)
{
    Console.WriteLine(image.Url);  // GetAllListingsResponse.Image
}
```

## Directory Structure

Place Query, Response, and Handler in the same feature folder:

```
Features/ListingFeatures/GetAllListings/
├── GetAllListingsQuery.cs
├── GetAllListingsResponse.cs
└── GetAllListingsQueryHandler.cs
```

Each in its own file for clarity.

## Common Pattern

### Single Response Object
```csharp
public sealed record GetListingByIdResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public IEnumerable<Image> Images { get; set; } = [];
    
    public sealed record Image
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}
```

Handler returns: `IRequest<GetListingByIdResponse>`

### Collection Response
```csharp
public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public IEnumerable<Image> Images { get; set; } = [];
    
    public sealed record Image
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}
```

Handler returns: `IRequest<IEnumerable<GetAllListingsResponse>>`

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Using `ListingDto`, `GetListingResult` naming | Use `Query` and `Response` only |
| Creating separate `ImageDto.cs` for response-only types | Nest inside `GetAllListingsResponse` |
| Creating DTOs for data used in one response | Nest as inner class in that response |
| Using `class` instead of `sealed record` | Use sealed records for immutability |
| Not providing default empty values | Add `= string.Empty` and `= []` |
| Deep nesting (3+ levels) | Consider if all levels are response-only; if not, extract |

## No-DTO Rule

**DO NOT create a DTO for data that appears in only one Response.**

The "DTO" pattern in some architectures creates files like `ImageDto.cs`, `ContactDto.cs`, etc. that exist only to be used inside one response. This is unnecessary indirection.

Instead:
- Nest simple types inside their Response
- Use one file per Response
- Keep related data together

**Result:** Fewer files, clearer ownership, easier refactoring.
