# Listing & Variant Fixes + CreateListing Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix stale frontend types, add missing backend response fields, move Year from Variant to Listing, and redesign the CreateListing form with a variant table, spec locking, and image preview.

**Architecture:** Backend-first: domain/migration changes flow into CQRS handler/mapping corrections, then frontend type fixes, then form redesign. Each backend task must compile and pass tests before moving to the next.

**Tech Stack:** ASP.NET Core 8, EF Core (PostgreSQL), AutoMapper, MediatR, React 19, TypeScript, react-hook-form, Zod, TanStack Query, Tailwind CSS.

---

## File Map

### Backend — modified
| File | Change |
|------|--------|
| `Automotive.Marketplace.Domain/Entities/Variant.cs` | Remove `Year` |
| `Automotive.Marketplace.Domain/Entities/Listing.cs` | Add `Year` |
| `Automotive.Marketplace.Infrastructure/Data/Configuration/VariantConfiguration.cs` | Update unique index (remove Year) |
| New migration file | Data-migration: move Year column |
| `Automotive.Marketplace.Application/Features/VariantFeatures/CreateVariant/CreateVariantCommand.cs` | Remove `Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/CreateVariant/CreateVariantResponse.cs` | Remove `Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommand.cs` | Remove `Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommandHandler.cs` | Remove `variant.Year = request.Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantResponse.cs` | Remove `Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelResponse.cs` | Remove `Year` |
| `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelQueryHandler.cs` | Remove `.OrderBy(v => v.Year)` |
| `Automotive.Marketplace.Application/Features/ListingFeatures/CreateListing/CreateListingCommandHandler.cs` | Set `listing.Year = request.Year`; remove Year from variant construction/dedup |
| `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs` | Add `PowerKw`, `EngineSizeMl` |
| `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs` | Year filters → `listing.Year` |
| `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs` | Add `Colour`, `Vin`, `IsSteeringWheelRight` |
| `Automotive.Marketplace.Application/Mappings/ListingMappings.cs` | Year from `src.Year`; add new fields; add Year to UpdateListingCommand→Listing map |
| `Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs` | `variant.Year` → `listing.Year` in Title string |
| `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs` | `variant.Year` → `listing.Year` in ListingTitle string |

### Backend — test builders
| File | Change |
|------|--------|
| `Automotive.Marketplace.Infrastructure/Data/Builders/VariantBuilder.cs` | Remove Year rule and `WithYear` |
| `Automotive.Marketplace.Infrastructure/Data/Builders/ListingBuilder.cs` | Add Year rule and `WithYear` |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/CreateListingCommandHandlerTests.cs` | Remove variant `Year`; assert `listing.Year` from command |
| `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetAllListingsQueryHandlerTests.cs` | Move Year from variant builder to listing builder in `SeedListingsAsync`; fix `Handle_SingleListing_ShouldReturnCorrectVariantFields` |

### Frontend — types
| File | Change |
|------|--------|
| `src/features/variantList/types/Variant.ts` | Remove `year` |
| `src/features/variantList/types/CreateVariantCommand.ts` | Remove `year` |
| `src/features/variantList/types/UpdateVariantCommand.ts` | Remove `year` |
| `src/features/listingList/types/GetAllListingsResponse.ts` | Rename stale fields; add `powerKw`, `engineSizeMl` |
| `src/features/listingDetails/types/GetListingByIdResponse.ts` | Rename stale fields; add `colour`, `vin`, `isSteeringWheelRight` |

### Frontend — admin variant form + dialogs
| File | Change |
|------|--------|
| `src/features/variantList/schemas/variantFormSchema.ts` | Remove `year` |
| `src/features/variantList/components/VariantForm.tsx` | Remove Year `FormField` |
| `src/features/variantList/components/CreateVariantDialog.tsx` | Remove `year: formData.year` from command; remove `year` from default values |
| `src/features/variantList/components/EditVariantDialog.tsx` | Remove `year: formData.year` from command |
| `src/features/variantList/components/EditVariantDialogContent.tsx` | Remove `variant.year` from title and from default values passed to VariantForm |
| `src/features/variantList/components/ViewVariantDialogContent.tsx` | Remove `Year` display row |
| `src/features/variantList/components/VariantListTable.tsx` | Remove `Year` column header and cell |

### Frontend — component field reference fixes
| File | Change |
|------|--------|
| `src/features/listingList/components/ListingCard.tsx` | Fix field references |
| `src/features/listingDetails/components/ListingDetailsContent.tsx` | Fix field references |

### Frontend — createListing redesign
| File | Change |
|------|--------|
| `src/features/createListing/schemas/createListingSchema.ts` | Make `year` required; remove from superRefine |
| `src/components/forms/VariantTable.tsx` | New component |
| `src/features/createListing/components/CreateListingForm.tsx` | Full redesign |
| `src/features/createListing/components/ImageUploadInput.tsx` | Append mode |
| `src/features/createListing/components/ImagePreview.tsx` | New component |
| `src/components/forms/select/VariantSelect.tsx` | Delete (replaced by VariantTable) |

---

## Task 1: Domain — Move Year from Variant to Listing

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/Variant.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Listing.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/VariantConfiguration.cs`

- [ ] **Step 1: Remove `Year` from Variant entity**

In `Automotive.Marketplace.Domain/Entities/Variant.cs`, remove the `Year` property so the file reads:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class Variant : BaseEntity
{
    public bool IsCustom { get; set; }

    public int DoorCount { get; set; }

    public int PowerKw { get; set; }

    public int EngineSizeMl { get; set; }

    public Guid ModelId { get; set; }

    public virtual Model Model { get; set; } = null!;

    public Guid FuelId { get; set; }

    public virtual Fuel Fuel { get; set; } = null!;

    public Guid TransmissionId { get; set; }

    public virtual Transmission Transmission { get; set; } = null!;

    public Guid BodyTypeId { get; set; }

    public virtual BodyType BodyType { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = [];
}
```

- [ ] **Step 2: Add `Year` to Listing entity**

In `Automotive.Marketplace.Domain/Entities/Listing.cs`, add `public int Year { get; set; }` after `IsSteeringWheelRight`:

```csharp
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

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public int Year { get; set; }

    public Guid DrivetrainId { get; set; }

    public virtual Drivetrain Drivetrain { get; set; } = null!;

    public Guid VariantId { get; set; }

    public virtual Variant Variant { get; set; } = null!;

    public Guid SellerId { get; set; }

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = [];

    public virtual ICollection<User> LikeUsers { get; set; } = [];
}
```

- [ ] **Step 3: Update `VariantConfiguration` to remove Year from unique index**

In `Automotive.Marketplace.Infrastructure/Data/Configuration/VariantConfiguration.cs`, change the `HasIndex` call to remove `v.Year`:

```csharp
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.HasOne(v => v.Model)
            .WithMany(m => m.Variants)
            .HasForeignKey(v => v.ModelId);

        builder.HasOne(v => v.Fuel)
            .WithMany()
            .HasForeignKey(v => v.FuelId);

        builder.HasOne(v => v.Transmission)
            .WithMany()
            .HasForeignKey(v => v.TransmissionId);

        builder.HasOne(v => v.BodyType)
            .WithMany()
            .HasForeignKey(v => v.BodyTypeId);

        builder.HasIndex(v => new { v.ModelId, v.FuelId, v.TransmissionId, v.BodyTypeId })
               .IsUnique()
               .HasFilter("\"IsCustom\" = false");
    }
}
```

- [ ] **Step 4: Build to confirm no compile errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: Build succeeded. Warnings about missing migrations are expected; errors are not.

---

## Task 2: EF Core Migration — MoveYearFromVariantToListing

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Migrations/<timestamp>_MoveYearFromVariantToListing.cs`
- Modified: `Automotive.Marketplace.Infrastructure/Migrations/AutomotiveContextModelSnapshot.cs` (auto-updated)

- [ ] **Step 1: Scaffold the migration**

```bash
dotnet ef migrations add MoveYearFromVariantToListing \
  --project Automotive.Marketplace.Infrastructure \
  --startup-project Automotive.Marketplace.Server
```

Expected: A new migration file is created under `Automotive.Marketplace.Infrastructure/Migrations/`.

- [ ] **Step 2: Open the generated migration and replace the `Up` and `Down` methods**

EF Core generates an incomplete migration. Replace both methods entirely with the correct ordered version that includes the data migration SQL.

Open the newly created file (e.g., `20260421XXXXXX_MoveYearFromVariantToListing.cs`) and replace its `Up` and `Down` methods:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add Year as nullable first (so existing rows don't violate NOT NULL)
    migrationBuilder.AddColumn<int>(
        name: "Year",
        table: "Listings",
        type: "integer",
        nullable: true);

    // 2. Copy Year from each Listing's Variant before dropping the column
    migrationBuilder.Sql(@"
        UPDATE ""Listings""
        SET ""Year"" = v.""Year""
        FROM ""Variants"" v
        WHERE ""Listings"".""VariantId"" = v.""Id""
    ");

    // 3. Make Year non-nullable now that all rows are populated
    migrationBuilder.AlterColumn<int>(
        name: "Year",
        table: "Listings",
        type: "integer",
        nullable: false,
        defaultValue: 0,
        oldClrType: typeof(int),
        oldType: "integer",
        oldNullable: true);

    // 4. Drop the old unique index that included Year
    migrationBuilder.DropIndex(
        name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
        table: "Variants");

    // 5. Drop Year from Variants (safe now that data was copied)
    migrationBuilder.DropColumn(
        name: "Year",
        table: "Variants");

    // 6. Recreate unique index without Year
    migrationBuilder.CreateIndex(
        name: "IX_Variants_ModelId_FuelId_TransmissionId_BodyTypeId",
        table: "Variants",
        columns: new[] { "ModelId", "FuelId", "TransmissionId", "BodyTypeId" },
        unique: true,
        filter: "\"IsCustom\" = false");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_Variants_ModelId_FuelId_TransmissionId_BodyTypeId",
        table: "Variants");

    migrationBuilder.AddColumn<int>(
        name: "Year",
        table: "Variants",
        type: "integer",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.Sql(@"
        UPDATE ""Variants""
        SET ""Year"" = l.""Year""
        FROM ""Listings"" l
        WHERE ""Variants"".""Id"" = l.""VariantId""
    ");

    migrationBuilder.DropColumn(
        name: "Year",
        table: "Listings");

    migrationBuilder.CreateIndex(
        name: "IX_Variants_ModelId_Year_FuelId_TransmissionId_BodyTypeId",
        table: "Variants",
        columns: new[] { "ModelId", "Year", "FuelId", "TransmissionId", "BodyTypeId" },
        unique: true,
        filter: "\"IsCustom\" = false");
}
```

- [ ] **Step 3: Build to confirm the migration compiles**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: Build succeeded.

> ⚠️ **Down migration is lossy:** A variant may back listings with multiple different years. The `Down` SQL copies a single listing's Year back to the Variant row — nondeterministically if multiple listings share the same variant. This rollback is intentionally lossy and is acceptable since rolling back this migration in production would be a manual recovery operation regardless.

---

## Task 3: Remove Year from Variant CQRS Features

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/VariantFeatures/CreateVariant/CreateVariantCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommand.cs`
- Modify: `Automotive.Marketplace.Application/Features/VariantFeatures/UpdateVariant/UpdateVariantCommandHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/VariantFeatures/GetVariantsByModel/GetVariantsByModelQueryHandler.cs`

- [ ] **Step 1: Remove `Year` from `CreateVariantCommand`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public sealed record CreateVariantCommand(
    Guid ModelId,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<CreateVariantResponse>;
```

- [ ] **Step 2: Remove `Year` from `UpdateVariantCommand`**

```csharp
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public sealed record UpdateVariantCommand(
    Guid Id,
    Guid ModelId,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<UpdateVariantResponse>;
```

- [ ] **Step 3: Remove `variant.Year = request.Year` from `UpdateVariantCommandHandler`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public class UpdateVariantCommandHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<UpdateVariantCommand, UpdateVariantResponse>
{
    public async Task<UpdateVariantResponse> Handle(UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await repository.GetByIdAsync<Variant>(request.Id, cancellationToken);

        variant.ModelId = request.ModelId;
        variant.FuelId = request.FuelId;
        variant.TransmissionId = request.TransmissionId;
        variant.BodyTypeId = request.BodyTypeId;
        variant.IsCustom = request.IsCustom;
        variant.DoorCount = request.DoorCount;
        variant.PowerKw = request.PowerKw;
        variant.EngineSizeMl = request.EngineSizeMl;

        await repository.UpdateAsync(variant, cancellationToken);

        return mapper.Map<UpdateVariantResponse>(variant);
    }
}
```

- [ ] **Step 4: Remove `Year` from `GetVariantsByModelResponse`**

```csharp
namespace Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;

public sealed record GetVariantsByModelResponse
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Guid FuelId { get; set; }
    public string FuelName { get; set; } = string.Empty;
    public Guid TransmissionId { get; set; }
    public string TransmissionName { get; set; } = string.Empty;
    public Guid BodyTypeId { get; set; }
    public string BodyTypeName { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
}
```

- [ ] **Step 5: Remove `Year` from `CreateVariantResponse`**

```csharp
namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public sealed record CreateVariantResponse
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Guid FuelId { get; set; }
    public Guid TransmissionId { get; set; }
    public Guid BodyTypeId { get; set; }
    public bool IsCustom { get; set; }
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
}
```

- [ ] **Step 6: Remove `Year` from `UpdateVariantResponse`**

```csharp
namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public sealed record UpdateVariantResponse
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Guid FuelId { get; set; }
    public Guid TransmissionId { get; set; }
    public Guid BodyTypeId { get; set; }
    public bool IsCustom { get; set; }
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
}
```

- [ ] **Step 7: Remove `.OrderBy(v => v.Year)` from `GetVariantsByModelQueryHandler`**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;

public class GetVariantsByModelQueryHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<GetVariantsByModelQuery, IEnumerable<GetVariantsByModelResponse>>
{
    public async Task<IEnumerable<GetVariantsByModelResponse>> Handle(GetVariantsByModelQuery request, CancellationToken cancellationToken)
    {
        var variants = await repository
            .AsQueryable<Variant>()
            .Where(v => v.ModelId == request.ModelId)
            .Include(v => v.Fuel)
            .Include(v => v.Transmission)
            .Include(v => v.BodyType)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetVariantsByModelResponse>>(variants);
    }
}
```

- [ ] **Step 8: Build to confirm no compile errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: Build succeeded.

---

## Task 4: Update Listing CQRS Features

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/CreateListing/CreateListingCommandHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/ListingMappings.cs`

- [ ] **Step 1: Update `CreateListingCommandHandler` — Year on Listing, remove from variant paths**

```csharp
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public class CreateListingCommandHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<CreateListingCommand, CreateListingResponse>
{
    public async Task<CreateListingResponse> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        Guid variantId;

        if (request.VariantId.HasValue)
        {
            await repository.GetByIdAsync<Variant>(request.VariantId.Value, cancellationToken);
            variantId = request.VariantId.Value;
        }
        else if (request.IsCustom)
        {
            var customVariant = new Variant
            {
                Id = Guid.NewGuid(),
                ModelId = request.ModelId,
                FuelId = request.FuelId,
                TransmissionId = request.TransmissionId,
                BodyTypeId = request.BodyTypeId,
                IsCustom = true,
                DoorCount = request.DoorCount,
                PowerKw = request.PowerKw,
                EngineSizeMl = request.EngineSizeMl,
            };
            await repository.CreateAsync(customVariant, cancellationToken);
            variantId = customVariant.Id;
        }
        else
        {
            var existing = await repository
                .AsQueryable<Variant>()
                .FirstOrDefaultAsync(v =>
                    v.ModelId == request.ModelId &&
                    v.FuelId == request.FuelId &&
                    v.TransmissionId == request.TransmissionId &&
                    v.BodyTypeId == request.BodyTypeId &&
                    !v.IsCustom,
                    cancellationToken);

            if (existing != null)
            {
                variantId = existing.Id;
            }
            else
            {
                var newVariant = new Variant
                {
                    Id = Guid.NewGuid(),
                    ModelId = request.ModelId,
                    FuelId = request.FuelId,
                    TransmissionId = request.TransmissionId,
                    BodyTypeId = request.BodyTypeId,
                    IsCustom = false,
                    DoorCount = request.DoorCount,
                    PowerKw = request.PowerKw,
                    EngineSizeMl = request.EngineSizeMl,
                };
                await repository.CreateAsync(newVariant, cancellationToken);
                variantId = newVariant.Id;
            }
        }

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            Price = request.Price,
            Mileage = request.Mileage,
            Description = request.Description,
            SellerId = request.SellerId,
            VariantId = variantId,
            DrivetrainId = request.DrivetrainId,
            IsUsed = request.IsUsed,
            City = request.City,
            Year = request.Year,
            Status = Status.Available,
        };

        await repository.CreateAsync(listing, cancellationToken);

        return mapper.Map<CreateListingResponse>(listing);
    }
}
```

- [ ] **Step 2: Add `PowerKw` and `EngineSizeMl` to `GetAllListingsResponse`**

```csharp
using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid VariantId { get; set; }
    public int Year { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string FuelName { get; set; } = string.Empty;
    public string TransmissionName { get; set; } = string.Empty;
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public ImageDto? Thumbnail { get; set; }
    public bool IsLiked { get; set; }
}
```

- [ ] **Step 3: Fix year filters in `GetAllListingsQueryHandler` to use `listing.Year`**

Change the two year filter lines (lines 36–37 of the current file) from:
```csharp
.Where(listing => request.MinYear == null || request.MinYear <= listing.Variant.Year)
.Where(listing => request.MaxYear == null || request.MaxYear >= listing.Variant.Year)
```
to:
```csharp
.Where(listing => request.MinYear == null || request.MinYear <= listing.Year)
.Where(listing => request.MaxYear == null || request.MaxYear >= listing.Year)
```

No other changes to this file.

- [ ] **Step 4: Add `Colour`, `Vin`, `IsSteeringWheelRight` to `GetListingByIdResponse`**

```csharp
namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public sealed record GetListingByIdResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid VariantId { get; set; }
    public int Year { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string FuelName { get; set; } = string.Empty;
    public string TransmissionName { get; set; } = string.Empty;
    public string BodyTypeName { get; set; } = string.Empty;
    public string DrivetrainName { get; set; } = string.Empty;
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
    public string? Colour { get; set; }
    public string? Vin { get; set; }
    public bool IsSteeringWheelRight { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public IEnumerable<Automotive.Marketplace.Application.Models.ImageDto> Images { get; set; } = [];
}
```

- [ ] **Step 5: Update `ListingMappings.cs`**

Replace the entire file with the corrected mappings:

```csharp
using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ListingMapping : Profile
{
    public ListingMapping()
    {
        CreateMap<Listing, CreateListingResponse>();

        CreateMap<Listing, GetAllListingsResponse>()
            .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.PowerKw : 0))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom((src, dest) => src.Seller != null ? src.Seller.Username ?? string.Empty : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Thumbnail, opt => opt.Ignore())
            .ForMember(dest => dest.IsLiked, opt => opt.Ignore());

        CreateMap<UpdateListingCommand, Listing>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year));

        CreateMap<Listing, GetListingByIdResponse>()
            .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.BodyTypeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.BodyType?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.DrivetrainName, opt => opt.MapFrom((src, dest) => src.Drivetrain != null ? src.Drivetrain.Name : string.Empty))
            .ForMember(dest => dest.DoorCount, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.DoorCount : 0))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.PowerKw : 0))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom((src, dest) => src.Seller != null ? src.Seller.Username ?? string.Empty : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Images, opt => opt.Ignore());
    }
}
```

- [ ] **Step 6: Build to confirm no compile errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: Build succeeded.

---

## Task 4b: Fix `variant.Year` References in Saved Listings + Chat

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/SavedListingFeatures/GetSavedListings/GetSavedListingsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs`

Both handlers build a title string using `variant.Year`. After the domain change, `Variant` no longer has `Year` — the include chain for the listing itself needs to provide it.

- [ ] **Step 1: Fix `GetSavedListingsQueryHandler.cs`**

The listing already has `like.Listing` in scope. Change:
```csharp
Title = $"{variant.Year} {variant.Model.Make.Name} {variant.Model.Name}",
```
to:
```csharp
Title = $"{listing.Year} {variant.Model.Make.Name} {variant.Model.Name}",
```

No include changes needed — `listing.Year` is a scalar on the listing entity already loaded.

- [ ] **Step 2: Fix `GetConversationsQueryHandler.cs`**

Find the line in the handler that reads `variant.Year` in the `ListingTitle` format string. The handler has a local `var listing = ...` and `var variant = listing.Variant`. Change:
```csharp
ListingTitle = $"{variant.Year} {variant.Model.Make.Name} {variant.Model.Name}",
```
to:
```csharp
ListingTitle = $"{listing.Year} {variant.Model.Make.Name} {variant.Model.Name}",
```

- [ ] **Step 3: Build to confirm no compile errors**

```bash
dotnet build ./Automotive.Marketplace.sln
```

Expected: Build succeeded.

---

## Task 5: Update Builders and Backend Tests

**Files:**
- Modify: `Automotive.Marketplace.Infrastructure/Data/Builders/VariantBuilder.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Builders/ListingBuilder.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/CreateListingCommandHandlerTests.cs`
- Modify: `Automotive.Marketplace.Tests/Features/HandlerTests/ListingHandlerTests/GetAllListingsQueryHandlerTests.cs`

- [ ] **Step 1: Remove `Year` from `VariantBuilder`**

Remove the `RuleFor(v => v.Year, ...)` line from the constructor, and remove the `WithYear` method entirely:

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class VariantBuilder
{
    private readonly Faker<Variant> _faker;

    public VariantBuilder()
    {
        _faker = new Faker<Variant>()
            .RuleFor(v => v.Id, f => f.Random.Guid())
            .RuleFor(v => v.PowerKw, f => f.Random.Int(40, 500))
            .RuleFor(v => v.EngineSizeMl, f => f.Random.Int(800, 6000))
            .RuleFor(v => v.DoorCount, f => f.Random.Int(2, 6))
            .RuleFor(v => v.IsCustom, _ => false);
    }

    public VariantBuilder WithModel(Guid modelId)
    {
        _faker.RuleFor(v => v.ModelId, modelId);
        return this;
    }

    public VariantBuilder WithFuel(Guid fuelId)
    {
        _faker.RuleFor(v => v.FuelId, fuelId);
        return this;
    }

    public VariantBuilder WithTransmission(Guid transmissionId)
    {
        _faker.RuleFor(v => v.TransmissionId, transmissionId);
        return this;
    }

    public VariantBuilder WithBodyType(Guid bodyTypeId)
    {
        _faker.RuleFor(v => v.BodyTypeId, bodyTypeId);
        return this;
    }

    public VariantBuilder With<T>(Expression<Func<Variant, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Variant Build() => _faker.Generate();

    public List<Variant> Build(int count) => _faker.Generate(count);
}
```

- [ ] **Step 2: Add `Year` to `ListingBuilder`**

Add a `RuleFor` for Year in the constructor and a `WithYear` method:

```csharp
using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ListingBuilder
{
    private readonly Faker<Listing> _faker;

    public ListingBuilder()
    {
        _faker = new Faker<Listing>()
            .RuleFor(listing => listing.Id, f => f.Random.Guid())
            .RuleFor(listing => listing.Price, f => f.Random.Decimal(100, 50000))
            .RuleFor(listing => listing.Description, f => f.Lorem.Sentences(2))
            .RuleFor(listing => listing.City, f => f.Address.City())
            .RuleFor(listing => listing.Status, Status.Available)
            .RuleFor(listing => listing.Vin, f => f.Vehicle.Vin())
            .RuleFor(listing => listing.Colour, f => f.Commerce.Color())
            .RuleFor(listing => listing.IsUsed, f => f.Random.Bool())
            .RuleFor(listing => listing.Mileage, f => f.Random.Int(10000, 400000))
            .RuleFor(listing => listing.IsSteeringWheelRight, f => f.Random.Bool())
            .RuleFor(listing => listing.Year, f => f.Random.Int(1980, 2025));
    }

    public ListingBuilder WithPrice(decimal price)
    {
        _faker.RuleFor(listing => listing.Price, price);
        return this;
    }

    public ListingBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(listing => listing.Id, listingId);
        return this;
    }

    public ListingBuilder WithSeller(Guid sellerId)
    {
        _faker.RuleFor(listing => listing.SellerId, sellerId);
        return this;
    }

    public ListingBuilder WithUsed(bool isUsed)
    {
        _faker.RuleFor(listing => listing.IsUsed, isUsed);
        return this;
    }

    public ListingBuilder WithVariant(Guid variantId)
    {
        _faker.RuleFor(listing => listing.VariantId, variantId);
        return this;
    }

    public ListingBuilder WithDrivetrain(Guid drivetrainId)
    {
        _faker.RuleFor(listing => listing.DrivetrainId, drivetrainId);
        return this;
    }

    public ListingBuilder WithYear(int year)
    {
        _faker.RuleFor(listing => listing.Year, year);
        return this;
    }

    public ListingBuilder With<T>(Expression<Func<Listing, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Listing Build() => _faker.Generate();

    public List<Listing> Build(int count) => _faker.Generate(count);
}
```

- [ ] **Step 3: Update `CreateListingCommandHandlerTests.cs`**

The tests reference `existingVariant.Year` and `.WithYear(2019)`. Update each test:

**`Handle_VariantIdProvided_ShouldUseThatVariant`** — remove `Year: existingVariant.Year` (Year is no longer a variant property); add a fixed year to the command; assert the listing has that year.

Change the command construction to use a literal year and assert listing.Year:
```csharp
var command = new CreateListingCommand(
    Price: 20000,
    Mileage: 30000,
    Description: "Existing variant car",
    SellerId: user.Id,
    VariantId: existingVariant.Id,
    ModelId: model.Id,
    Year: 2022,
    FuelId: fuel.Id,
    TransmissionId: transmission.Id,
    BodyTypeId: bodyType.Id,
    DrivetrainId: drivetrain.Id,
    IsCustom: false,
    DoorCount: 4,
    PowerKw: 100,
    EngineSizeMl: 1600,
    IsUsed: true,
    City: "Test City"
);
```

And add to the assertions:
```csharp
listings.First().Year.Should().Be(2022);
```

**`Handle_IsCustomTrue_ShouldCreateCustomVariant`** — already uses `Year: 2021` in the command; no variant `WithYear` call. Add assertion:
```csharp
listings.First().Year.Should().Be(2021);
```

**`Handle_IsCustomFalse_NoExistingVariant_ShouldCreateNewVariant`** — uses `Year: 2020`. Add assertion:
```csharp
createdListing.Year.Should().Be(command.Year);
```

**`Handle_IsCustomFalse_ExistingVariantMatch_ShouldReuseExistingVariant`** — currently uses `WithYear(2019)` on the variant builder. Remove that call since Year is no longer on Variant. Update the pre-existing variant construction:
```csharp
var preExistingVariant = new VariantBuilder()
    .WithModel(model.Id)
    .WithFuel(fuel.Id)
    .WithTransmission(transmission.Id)
    .WithBodyType(bodyType.Id)
    .With(v => v.IsCustom, false)
    .Build();
```

The command still passes `Year: 2019` — this now goes to `listing.Year`. Add assertion:
```csharp
listings.First().Year.Should().Be(2019);
```

- [ ] **Step 4: Update `GetAllListingsQueryHandlerTests.cs`**

**`SeedListingsAsync`**: The method currently calls `variantBuilder.WithYear(carYear.Value.Year)` when `carYear` is provided. Change this to use `listingBuilder.WithYear(carYear.Value.Year)` instead:

```csharp
var variantBuilder = new VariantBuilder()
    .WithModel(model.Id)
    .WithFuel(fuel.Id)
    .WithTransmission(transmission.Id)
    .WithBodyType(bodyType.Id);

var variant = variantBuilder.Build();
await context.AddAsync(variant);

var listingBuilder = new ListingBuilder()
    .WithSeller(seller.Id)
    .WithVariant(variant.Id)
    .WithDrivetrain(drivetrain.Id);

if (isCarUsed.HasValue)
{
    listingBuilder.WithUsed(isCarUsed.Value);
}

if (carYear.HasValue)
{
    listingBuilder.WithYear(carYear.Value.Year);
}

if (carPrice.HasValue)
{
    listingBuilder.WithPrice(carPrice.Value);
}

if (status.HasValue)
{
    listingBuilder.With(l => l.Status, status.Value);
}

listings.Add(listingBuilder.Build());
```

**`Handle_SingleListing_ShouldReturnCorrectVariantFields`**: The test calls `.WithYear(expectedYear)` on the variant builder and asserts `response.Year.Should().Be(expectedYear)`. The variant builder no longer has `WithYear`. Update the listing construction to set year instead:

```csharp
const int expectedYear = 2020;
// ... (make, model, fuel, etc. seeding unchanged) ...

var variant = new VariantBuilder()
    .WithModel(model.Id)
    .WithFuel(fuel.Id)
    .WithTransmission(transmission.Id)
    .WithBodyType(bodyType.Id)
    .Build();
await context.AddAsync(variant);

var seller = new UserBuilder().Build();
await context.AddAsync(seller);

var listing = new ListingBuilder()
    .WithSeller(seller.Id)
    .WithVariant(variant.Id)
    .WithDrivetrain(drivetrain.Id)
    .WithYear(expectedYear)
    .Build();
await context.AddAsync(listing);
await context.SaveChangesAsync();
```

The assertion `response.Year.Should().Be(expectedYear)` remains unchanged.

- [ ] **Step 5: Run all backend tests**

```bash
dotnet test ./Automotive.Marketplace.sln
```

Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: move Year from Variant to Listing, fix response fields, update handlers and tests

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: Frontend — Fix TypeScript Types

**Files:**
- Modify: `automotive.marketplace.client/src/features/variantList/types/Variant.ts`
- Modify: `automotive.marketplace.client/src/features/variantList/types/CreateVariantCommand.ts`
- Modify: `automotive.marketplace.client/src/features/variantList/types/UpdateVariantCommand.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/GetAllListingsResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`

- [ ] **Step 1: Remove `year` from `Variant.ts`**

```ts
export type Variant = {
  id: string;
  modelId: string;
  fuelId: string;
  fuelName: string;
  transmissionId: string;
  transmissionName: string;
  bodyTypeId: string;
  bodyTypeName: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
```

- [ ] **Step 2: Remove `year` from `CreateVariantCommand.ts`**

```ts
export type CreateVariantCommand = {
  modelId: string;
  fuelId: string;
  transmissionId: string;
  bodyTypeId: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
```

- [ ] **Step 3: Remove `year` from `UpdateVariantCommand.ts`**

```ts
export type UpdateVariantCommand = {
  id: string;
  modelId: string;
  fuelId: string;
  transmissionId: string;
  bodyTypeId: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
```

- [ ] **Step 4: Fix `GetAllListingsResponse.ts` — rename stale fields, add missing ones**

```ts
export type GetAllListingsResponse = {
  id: string;
  isUsed: boolean;
  year: number;
  makeName: string;
  modelName: string;
  mileage: number;
  price: number;
  engineSizeMl: number;
  powerKw: number;
  fuelName: string;
  transmissionName: string;
  city: string;
  description: string;
  thumbnail: Thumbnail | null;
  isLiked: boolean;
};

type Thumbnail = {
  url: string;
  altText: string;
};
```

- [ ] **Step 5: Fix `GetListingByIdResponse.ts` — rename stale fields, add missing ones**

```ts
export type GetListingByIdResponse = {
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
  images: Image[];
};

type Image = {
  url: string;
  altText: string;
};
```

---

## Task 7: Frontend — Fix Component Field References

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Fix field references in `ListingCard.tsx`**

Update the JSX that references stale field names. The card displays year, make, model, fuelType, and transmission. Change:

```tsx
// line: `${listing.year} ${listing.make} ${listing.model}`
<p className="font-sans text-xl">{`${listing.year} ${listing.makeName} ${listing.modelName}`}</p>
```

```tsx
// stat={listing.fuelType}
stat={listing.fuelName}
```

```tsx
// stat={listing.transmission}
stat={listing.transmissionName}
```

- [ ] **Step 2: Fix field references in `ListingDetailsContent.tsx`**

Change all stale references. Use find-and-replace within the file:

| Old reference | New reference |
|---|---|
| `listing.make` | `listing.makeName` |
| `listing.model` | `listing.modelName` |
| `listing.transmission` | `listing.transmissionName` |
| `listing.drivetrain` | `listing.drivetrainName` |
| `listing.fuel` | `listing.fuelName` |
| `listing.bodyType` | `listing.bodyTypeName` |
| `listing.seller` | `listing.sellerName` |

There are six occurrences total (four in JSX display, three in string interpolations / props). Apply each rename.

---

## Task 8: Frontend — Variant Admin Form Cleanup (remove Year)

**Files:**
- Modify: `automotive.marketplace.client/src/features/variantList/schemas/variantFormSchema.ts`
- Modify: `automotive.marketplace.client/src/features/variantList/components/VariantForm.tsx`

- [ ] **Step 1: Remove `year` from `variantFormSchema.ts`**

Delete the entire `year: z.coerce.number<number>()…` block (lines 12–22 of the current file). The resulting schema should have: `makeId`, `modelId`, `fuelId`, `transmissionId`, `bodyTypeId`, `doorCount`, `powerKw`, `engineSizeMl`, `isCustom`.

```ts
import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import z from "zod";

export const variantFormSchema = z.object({
  makeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a make" }),
  modelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a model" }),
  fuelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a fuel type" }),
  transmissionId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a transmission" }),
  bodyTypeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a body type" }),
  doorCount: z.coerce
    .number<number>()
    .min(VALIDATION.DOOR_COUNT.MIN, {
      error: validation.minSize({
        label: "Door count",
        size: VALIDATION.DOOR_COUNT.MIN,
      }),
    })
    .max(VALIDATION.DOOR_COUNT.MAX, {
      error: validation.maxSize({
        label: "Door count",
        size: VALIDATION.DOOR_COUNT.MAX,
      }),
    }),
  powerKw: z.coerce
    .number<number>()
    .min(VALIDATION.POWER.MIN, {
      error: validation.minSize({ label: "Power", size: VALIDATION.POWER.MIN, unit: "kW" }),
    })
    .max(VALIDATION.POWER.MAX, {
      error: validation.maxSize({ label: "Power", size: VALIDATION.POWER.MAX, unit: "kW" }),
    }),
  engineSizeMl: z.coerce
    .number<number>()
    .min(VALIDATION.ENGINE_SIZE.MIN, {
      error: validation.minSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MIN,
        unit: "ml",
      }),
    })
    .max(VALIDATION.ENGINE_SIZE.MAX, {
      error: validation.maxSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MAX,
        unit: "ml",
      }),
    }),
  isCustom: z.boolean(),
});
```

`VariantFormData` is inferred from the schema — no changes needed there.

- [ ] **Step 2: Remove the Year `FormField` from `VariantForm.tsx`**

Delete the entire `<FormField name="year" ...>` block (lines 93–105 of the current file). No other changes to this component.

- [ ] **Step 3: Update `CreateVariantDialog.tsx` — remove year**

Remove `year: formData.year` from the `createVariantAsync` call, and remove `year` from the default `variant` prop passed to `VariantForm`:

```tsx
const handleSubmit = async (formData: VariantFormData) => {
  await createVariantAsync({
    modelId: formData.modelId,
    fuelId: formData.fuelId,
    transmissionId: formData.transmissionId,
    bodyTypeId: formData.bodyTypeId,
    doorCount: formData.doorCount,
    powerKw: formData.powerKw,
    engineSizeMl: formData.engineSizeMl,
    isCustom: formData.isCustom,
  });
  setIsOpen(false);
};
```

And in the `VariantForm` default values:
```tsx
<VariantForm
  variant={{
    makeId,
    modelId,
    fuelId: "",
    transmissionId: "",
    bodyTypeId: "",
    doorCount: 4,
    powerKw: 100,
    engineSizeMl: 1600,
    isCustom: false,
  }}
  onSubmit={handleSubmit}
/>
```

- [ ] **Step 4: Update `EditVariantDialog.tsx` — remove year**

Remove `year: formData.year` from the `updateVariantAsync` call:

```tsx
const handleSubmit = async (formData: VariantFormData) => {
  await updateVariantAsync({
    id: variant.id,
    modelId: formData.modelId,
    fuelId: formData.fuelId,
    transmissionId: formData.transmissionId,
    bodyTypeId: formData.bodyTypeId,
    doorCount: formData.doorCount,
    powerKw: formData.powerKw,
    engineSizeMl: formData.engineSizeMl,
    isCustom: formData.isCustom,
  });
  setIsOpen(false);
};
```

- [ ] **Step 5: Update `EditVariantDialogContent.tsx` — remove year from title and defaults**

Remove `variant.year` from the title and from the `variant` prop defaults:

```tsx
<DialogTitle>Edit variant</DialogTitle>
```

And update the `VariantForm` default values object:
```tsx
<VariantForm
  variant={{
    makeId,
    modelId: variant.modelId,
    fuelId: variant.fuelId,
    transmissionId: variant.transmissionId,
    bodyTypeId: variant.bodyTypeId,
    doorCount: variant.doorCount,
    powerKw: variant.powerKw,
    engineSizeMl: variant.engineSizeMl,
    isCustom: variant.isCustom,
  }}
  onSubmit={onSubmit}
/>
```

- [ ] **Step 6: Update `ViewVariantDialogContent.tsx` — remove Year display**

Delete the `<p>Year: {variant.year}</p>` line.

- [ ] **Step 7: Update `VariantListTable.tsx` — remove Year column**

Delete the `<TableHead>Year</TableHead>` header and the corresponding `<TableCell>{v.year}</TableCell>` cell.

---

## Task 9: Frontend — VariantTable Component (new)

**Files:**
- Create: `automotive.marketplace.client/src/components/forms/VariantTable.tsx`

- [ ] **Step 1: Create `VariantTable.tsx`**

```tsx
import { getVariantsByModelIdOptions } from "@/features/variantList/api/getVariantsByModelIdOptions";
import type { Variant } from "@/features/variantList/types/Variant";
import { useQuery } from "@tanstack/react-query";

type VariantTableProps = {
  modelId: string;
  selectedVariantId: string;
  onSelect: (variant: Variant | null) => void;
  disabled?: boolean;
};

const VariantTable = ({
  modelId,
  selectedVariantId,
  onSelect,
  disabled,
}: VariantTableProps) => {
  const { data: variantsQuery, isPending, isError } = useQuery(
    getVariantsByModelIdOptions(modelId || undefined),
  );
  const variants = variantsQuery?.data ?? [];

  if (!modelId || disabled) return null;

  const handleRowClick = (variant: Variant) => {
    if (variant.id === selectedVariantId) {
      onSelect(null);
    } else {
      onSelect(variant);
    }
  };

  return (
    <div className="w-full overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b text-left text-muted-foreground">
            <th className="py-2 pr-4 font-medium">Fuel</th>
            <th className="py-2 pr-4 font-medium">Transmission</th>
            <th className="py-2 pr-4 font-medium">Power (kW)</th>
            <th className="py-2 pr-4 font-medium">Engine (ml)</th>
            <th className="py-2 pr-4 font-medium">Body Type</th>
            <th className="py-2 font-medium">Doors</th>
          </tr>
        </thead>
        <tbody>
          {isPending && (
            <tr>
              <td colSpan={6} className="py-3 text-center text-muted-foreground">
                Loading variants…
              </td>
            </tr>
          )}
          {isError && (
            <tr>
              <td colSpan={6} className="py-3 text-center text-destructive">
                Failed to load variants
              </td>
            </tr>
          )}
          {!isPending && !isError && variants.length === 0 && (
            <tr>
              <td colSpan={6} className="py-3 text-center text-muted-foreground">
                No variants available for this model
              </td>
            </tr>
          )}
          {variants.map((variant) => (
            <tr
              key={variant.id}
              onClick={() => handleRowClick(variant)}
              className={`cursor-pointer border-b transition-colors hover:bg-muted/50 ${
                variant.id === selectedVariantId ? "bg-muted font-medium" : ""
              }`}
            >
              <td className="py-2 pr-4">{variant.fuelName}</td>
              <td className="py-2 pr-4">{variant.transmissionName}</td>
              <td className="py-2 pr-4">{variant.powerKw}</td>
              <td className="py-2 pr-4">{variant.engineSizeMl}</td>
              <td className="py-2 pr-4">{variant.bodyTypeName}</td>
              <td className="py-2">{variant.doorCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default VariantTable;
```

---

## Task 10: Frontend — CreateListing Form Redesign

**Files:**
- Modify: `automotive.marketplace.client/src/features/createListing/schemas/createListingSchema.ts`
- Modify: `automotive.marketplace.client/src/features/createListing/components/CreateListingForm.tsx`

- [ ] **Step 1: Make `year` required in `createListingSchema.ts`**

Replace the optional `year` definition and its `superRefine` check with a required field. The full updated schema:

```ts
import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import { z } from "zod";

export const CreateListingSchema = z.object({
  price: z.coerce
    .number<number>()
    .min(VALIDATION.PRICE.MIN, {
      error: validation.minSize({ label: "Price", size: VALIDATION.PRICE.MIN, unit: "€" }),
    })
    .max(VALIDATION.PRICE.MAX, {
      error: validation.maxSize({ label: "Price", size: VALIDATION.PRICE.MAX, unit: "€" }),
    }),
  description: z
    .string()
    .max(VALIDATION.TEXT.LONG, {
      error: validation.maxLength({ label: "Description", length: VALIDATION.TEXT.LONG }),
    })
    .optional(),
  colour: z.string().optional(),
  vin: z
    .string()
    .optional()
    .refine((val) => !val || VALIDATION.VIN.REGEX.test(val), {
      message: "VIN code must have 17 characters and not include I, O, or Q",
    }),
  powerKw: z.coerce.number<number>().int().max(VALIDATION.POWER.MAX, {
    error: validation.maxSize({ label: "Engine power", size: VALIDATION.POWER.MAX, unit: "kW" }),
  }).optional(),
  engineSizeMl: z.coerce.number<number>().int().max(VALIDATION.ENGINE_SIZE.MAX, {
    error: validation.maxSize({ label: "Engine size", size: VALIDATION.ENGINE_SIZE.MAX, unit: "ml" }),
  }).optional(),
  mileage: z.coerce
    .number<number>()
    .int()
    .min(VALIDATION.MILEAGE.MIN, {
      error: validation.minSize({ label: "Mileage", size: VALIDATION.MILEAGE.MIN, unit: "km" }),
    })
    .max(VALIDATION.MILEAGE.MAX, {
      error: validation.maxSize({ label: "Mileage", size: VALIDATION.MILEAGE.MAX, unit: "km" }),
    }),
  isSteeringWheelRight: z.boolean(),
  makeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a make" }),
  modelId: z.string().optional(),
  variantId: z
    .string()
    .regex(VALIDATION.GUID.REGEX)
    .optional()
    .or(z.literal("")),
  city: z
    .string()
    .nonempty({ error: "City cannot be empty" })
    .max(VALIDATION.NAME.LONG, {
      error: validation.maxLength({ label: "City", length: VALIDATION.NAME.LONG }),
    }),
  isUsed: z.boolean(),
  isCustom: z.boolean().optional(),
  year: z.coerce
    .number<number>()
    .int()
    .min(VALIDATION.YEAR.MIN, {
      error: validation.minSize({ label: "Year", size: VALIDATION.YEAR.MIN }),
    })
    .max(new Date().getFullYear(), {
      error: validation.maxSize({ label: "Year", size: new Date().getFullYear() }),
    }),
  transmissionId: z.string().optional(),
  fuelId: z.string().optional(),
  bodyTypeId: z.string().optional(),
  drivetrainId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: "Please select a drivetrain type",
  }),
  doorCount: z.coerce.number<number>().int().max(VALIDATION.DOOR_COUNT.MAX, {
    error: validation.maxSize({ label: "Door count", size: VALIDATION.DOOR_COUNT.MAX }),
  }).optional(),
  images: z
    .array(z.instanceof(Blob, { error: "You did not upload a valid image file" }))
    .min(1, { error: "You must upload at least one image" }),
}).superRefine((data, ctx) => {
  if (!data.variantId) {
    if (!data.modelId || !VALIDATION.GUID.REGEX.test(data.modelId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["modelId"], message: "Please select a model" });
    if (!data.fuelId || !VALIDATION.GUID.REGEX.test(data.fuelId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["fuelId"], message: "Please select a fuel type" });
    if (!data.transmissionId || !VALIDATION.GUID.REGEX.test(data.transmissionId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["transmissionId"], message: "Please select a transmission" });
    if (!data.bodyTypeId || !VALIDATION.GUID.REGEX.test(data.bodyTypeId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["bodyTypeId"], message: "Please select a body type" });
    if (!data.doorCount || data.doorCount < VALIDATION.DOOR_COUNT.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["doorCount"], message: "Enter door count" });
    if (!data.powerKw || data.powerKw < VALIDATION.POWER.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["powerKw"], message: "Enter engine power" });
    if (!data.engineSizeMl || data.engineSizeMl < VALIDATION.ENGINE_SIZE.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["engineSizeMl"], message: "Enter engine size" });
  }
});
```

- [ ] **Step 2: Rewrite `CreateListingForm.tsx`**

Full replacement. Key behaviours:
- `isModified` is a local `useState<boolean>` — tracks whether "Car is modified" checkbox is checked.
- `savedVariantIdRef` stores the last selected variantId so it can be restored when isModified is unchecked.
- Spec fields are locked (disabled) when `variantId !== ""` AND `isModified === false`.
- The "Car is modified" checkbox only appears when `variantId !== ""`.
- `handleVariantSelect(variant | null)` populates spec fields and sets/clears variantId.

```tsx
import DrivetrainToggleGroup from "@/components/forms/DrivetrainToggleGroup";
import BodyTypeSelect from "@/components/forms/select/BodyTypeSelect";
import FuelSelect from "@/components/forms/select/FuelSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import VariantTable from "@/components/forms/VariantTable";
import TransmissionToggleGroup from "@/components/forms/TransmissionToggleGroup";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { VALIDATION } from "@/constants/validation";
import type { Variant } from "@/features/variantList/types/Variant";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { useCreateListing } from "../api/useCreateListing";
import { CreateListingSchema } from "../schemas/createListingSchema";
import { CreateListingFormData } from "../types/CreateListingFormData";
import ImagePreview from "./ImagePreview";
import ImageUploadInput from "./ImageUploadInput";

type CreateListingFormProps = {
  className?: string;
};

const CreateListingForm = ({ className }: CreateListingFormProps) => {
  const [isModified, setIsModified] = useState(false);
  const savedVariantIdRef = useRef<string>("");

  const form = useForm({
    resolver: zodResolver(CreateListingSchema),
    defaultValues: {
      price: 0,
      vin: "",
      powerKw: 0,
      engineSizeMl: 0,
      mileage: 0,
      isSteeringWheelRight: true,
      makeId: "",
      modelId: "",
      variantId: "",
      city: "",
      colour: "",
      isUsed: false,
      isCustom: true,
      year: undefined,
      transmissionId: "",
      fuelId: "",
      bodyTypeId: "",
      drivetrainId: "",
      doorCount: 0,
      images: [],
    },
  });

  const { mutateAsync: createListingAsync } = useCreateListing();

  const onSubmit = async (formData: CreateListingFormData) => {
    const { makeId, ...command } = formData;
    await createListingAsync({
      ...command,
      variantId: command.variantId || undefined,
    });
    form.reset();
    setIsModified(false);
    savedVariantIdRef.current = "";
  };

  const selectedMake = form.watch("makeId");
  const selectedModelId = form.watch("modelId") ?? "";
  const variantId = form.watch("variantId") ?? "";
  const images = form.watch("images") ?? [];

  const specLocked = variantId !== "" && !isModified;

  const handleVariantSelect = (variant: Variant | null) => {
    if (variant) {
      form.setValue("variantId", variant.id);
      form.setValue("isCustom", false);
      form.setValue("fuelId", variant.fuelId);
      form.setValue("transmissionId", variant.transmissionId);
      form.setValue("bodyTypeId", variant.bodyTypeId);
      form.setValue("doorCount", variant.doorCount);
      form.setValue("powerKw", variant.powerKw);
      form.setValue("engineSizeMl", variant.engineSizeMl);
      setIsModified(false);
    } else {
      form.setValue("variantId", "");
      form.setValue("isCustom", true);
      setIsModified(false);
    }
  };

  const handleMakeChange = (value: string) => {
    form.setValue("makeId", value);
    form.setValue("modelId", "");
    form.setValue("variantId", "");
    form.setValue("isCustom", true);
    setIsModified(false);
    savedVariantIdRef.current = "";
  };

  const handleModelChange = (modelId: string) => {
    form.setValue("modelId", modelId);
    form.setValue("variantId", "");
    form.setValue("isCustom", true);
    setIsModified(false);
    savedVariantIdRef.current = "";
  };

  const handleIsModifiedChange = (checked: boolean) => {
    if (checked) {
      // Save the current variantId then clear it so the backend uses the custom/isCustom path
      savedVariantIdRef.current = form.getValues("variantId") ?? "";
      form.setValue("variantId", "");
      form.setValue("isCustom", true);
      setIsModified(true);
    } else {
      form.setValue("variantId", savedVariantIdRef.current);
      form.setValue("isCustom", false);
      setIsModified(false);
    }
  };

  const handleRemoveImage = (index: number) => {
    const updated = images.filter((_, i) => i !== index) as File[];
    form.setValue("images", updated);
  };

  return (
    <div className={className}>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="grid w-full min-w-3xs grid-cols-3 gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
        >
          {/* Row 1: Make, Model, Year */}
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={(value) => {
                      field.onChange(value);
                      handleMakeChange(value);
                    }}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="modelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={handleModelChange}
                    selectedMake={selectedMake}
                    value={field.value}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={VALIDATION.YEAR.MIN} max={new Date().getFullYear()} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 2: Variant Table */}
          <div className="col-span-3">
            <FormLabel className="mb-2 block">Select a variant (optional)</FormLabel>
            <VariantTable
              modelId={selectedModelId}
              selectedVariantId={variantId}
              onSelect={handleVariantSelect}
            />
          </div>

          {/* Row 3: Spec group */}
          <div className="col-span-3 rounded-md border p-4">
            <div className="mb-4 flex items-center justify-between">
              <span className="text-sm font-medium">
                Specifications {specLocked && <span className="text-muted-foreground">(locked to variant)</span>}
              </span>
              {variantId !== "" && (
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="is-modified"
                    checked={isModified}
                    onCheckedChange={(v) => handleIsModifiedChange(!!v)}
                  />
                  <label htmlFor="is-modified" className="cursor-pointer text-sm">
                    Car is modified
                  </label>
                </div>
              )}
            </div>
            <div className="grid grid-cols-3 gap-x-6 gap-y-4">
              <FormField
                name="fuelId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Fuel type*</FormLabel>
                    <FormControl>
                      <FuelSelect
                        onValueChange={field.onChange}
                        disabled={specLocked}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="transmissionId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Transmission*</FormLabel>
                    <FormControl>
                      <TransmissionToggleGroup
                        type="single"
                        value={field.value ?? ""}
                        onValueChange={field.onChange}
                        disabled={specLocked}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="bodyTypeId"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Body type*</FormLabel>
                    <FormControl>
                      <BodyTypeSelect
                        onValueChange={field.onChange}
                        disabled={specLocked}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="doorCount"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Door count*</FormLabel>
                    <FormControl>
                      <Input type="number" disabled={specLocked} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="powerKw"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Engine power (kW)*</FormLabel>
                    <FormControl>
                      <Input type="number" min={0} disabled={specLocked} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                name="engineSizeMl"
                control={form.control}
                render={({ field }) => (
                  <FormItem className="flex flex-col justify-start">
                    <FormLabel>Engine size (ml)*</FormLabel>
                    <FormControl>
                      <Input type="number" min={0} disabled={specLocked} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>

          {/* Row 4: Drivetrain, Price, Mileage, City */}
          <FormField
            name="drivetrainId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-3 flex flex-col justify-start">
                <FormLabel>Drivetrain*</FormLabel>
                <FormControl>
                  <DrivetrainToggleGroup
                    type="single"
                    value={field.value}
                    onValueChange={field.onChange}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="price"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Car price (€)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="mileage"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Mileage (km)*</FormLabel>
                <FormControl>
                  <Input type="number" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="city"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>City*</FormLabel>
                <FormControl>
                  <Input placeholder="Kaunas" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 5: Colour, VIN */}
          <FormField
            name="colour"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Colour</FormLabel>
                <FormControl>
                  <Input placeholder="Crimson" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="vin"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>VIN</FormLabel>
                <FormControl>
                  <Input placeholder="1G1JC524417418958" type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 6: Description, Images */}
          <FormField
            name="description"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea className="max-h-96" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="images"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col justify-start">
                <FormLabel>Vehicle images*</FormLabel>
                <FormControl>
                  <ImageUploadInput field={field} />
                </FormControl>
                <ImagePreview
                  images={images as File[]}
                  onRemove={handleRemoveImage}
                />
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Row 7: Checkboxes + Submit */}
          <FormField
            name="isSteeringWheelRight"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col items-center justify-start">
                <FormLabel>Steering wheel on right</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isUsed"
            control={form.control}
            render={({ field }) => (
              <FormItem className="flex flex-col items-center justify-evenly">
                <FormLabel>Used car</FormLabel>
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="h-5 w-5"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button className="self-end" type="submit">
            Submit
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default CreateListingForm;
```

---

## Task 11: Frontend — ImageUploadInput Append Mode + ImagePreview Component

**Files:**
- Modify: `automotive.marketplace.client/src/features/createListing/components/ImageUploadInput.tsx`
- Create: `automotive.marketplace.client/src/features/createListing/components/ImagePreview.tsx`

- [ ] **Step 1: Update `ImageUploadInput.tsx` to append instead of replace**

Change `field.onChange(compressedImages)` to append to the existing value:

```tsx
import { Input } from "@/components/ui/input";
import imageCompression from "browser-image-compression";
import { ControllerRenderProps } from "react-hook-form";
import { CreateListingFormData } from "../types/CreateListingFormData";

type ImageUploadInputProps = {
  field: ControllerRenderProps<CreateListingFormData, "images">;
};

const ImageUploadInput = ({ field }: ImageUploadInputProps) => {
  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      const compressionPromises = files.map(
        async (image) =>
          await imageCompression(image, {
            maxSizeMB: 0.4,
            useWebWorker: true,
            maxWidthOrHeight: 1920,
          }),
      );

      const compressedImages = await Promise.all(compressionPromises);
      const existing = Array.isArray(field.value) ? field.value : [];
      field.onChange([...existing, ...compressedImages]);
    }
  };

  return (
    <Input type="file" multiple accept="image/*" onChange={handleImageUpload} />
  );
};

export default ImageUploadInput;
```

- [ ] **Step 2: Create `ImagePreview.tsx`**

Use `useMemo` to create URLs once per `images` change, and `useEffect` to revoke the same set on cleanup — avoiding the memory leak from calling `createObjectURL` inline in render.

```tsx
import { useEffect, useMemo } from "react";

type ImagePreviewProps = {
  images: File[];
  onRemove: (index: number) => void;
};

const ImagePreview = ({ images, onRemove }: ImagePreviewProps) => {
  const objectUrls = useMemo(
    () => images.map((file) => URL.createObjectURL(file)),
    [images],
  );

  useEffect(() => {
    return () => {
      objectUrls.forEach((url) => URL.revokeObjectURL(url));
    };
  }, [objectUrls]);

  if (images.length === 0) return null;

  return (
    <div className="mt-2 flex flex-wrap gap-2">
      {objectUrls.map((url, index) => (
        <div key={index} className="relative h-14 w-14 flex-shrink-0">
          <img
            src={url}
            alt={`Preview ${index + 1}`}
            className="h-14 w-14 rounded object-cover"
          />
          <button
            type="button"
            onClick={() => onRemove(index)}
            className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-destructive text-destructive-foreground text-xs leading-none"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
};

export default ImagePreview;
```

---

## Task 12: Delete VariantSelect and Run Frontend Lint/Build

**Files:**
- Delete: `automotive.marketplace.client/src/components/forms/select/VariantSelect.tsx`

- [ ] **Step 1: Delete `VariantSelect.tsx`**

```bash
rm automotive.marketplace.client/src/components/forms/select/VariantSelect.tsx
```

- [ ] **Step 2: Run frontend lint and type-check**

```bash
cd automotive.marketplace.client && npm run lint && npm run build
```

Expected: No type errors or lint errors.

- [ ] **Step 3: Commit all frontend changes**

```bash
cd ..
git add -A
git commit -m "feat: fix frontend types, redesign CreateListing form, add VariantTable and ImagePreview

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Self-Review Checklist

**Spec coverage:**

| Spec Section | Covered in Task |
|---|---|
| Move Year from Variant to Listing (domain + migration) | Task 1–2 |
| Variant CQRS: remove Year from CreateVariant, UpdateVariant, GetVariants | Task 3 |
| Listing CQRS: Year on listing, year filters, CreateListingCommandHandler | Task 4 |
| GetListingById: add Colour, Vin, IsSteeringWheelRight | Task 4 step 4 |
| GetAllListings: add PowerKw, EngineSizeMl | Task 4 step 2 |
| ListingMappings: Year from src.Year; new fields; UpdateListingCommand Year | Task 4 step 5 |
| Builder + test updates | Task 5 |
| Frontend Variant type: remove year | Task 6 step 1 |
| Frontend GetAllListingsResponse renames | Task 6 step 4 |
| Frontend GetListingByIdResponse renames + new fields | Task 6 step 5 |
| ListingCard field reference fixes | Task 7 step 1 |
| ListingDetailsContent field reference fixes | Task 7 step 2 |
| Variant admin form: remove Year | Task 8 |
| New VariantTable component | Task 9 |
| CreateListingForm redesign with spec locking, isModified, new layout | Task 10 |
| ImageUploadInput append mode | Task 11 step 1 |
| New ImagePreview component | Task 11 step 2 |
| Delete VariantSelect | Task 12 |
