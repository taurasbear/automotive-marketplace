# My Listings, Image Gallery & Defect Marking — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add owner-facing listing management with inline editing, multi-image galleries across the app, and a defect marking system with photo attachments.

**Architecture:** Backend extends existing Clean Architecture + CQRS patterns. New `DefectCategory` enum entity (with translations) + `ListingDefect` join entity + extended `Image` entity. Frontend adds reusable gallery components, a new `myListings` feature module, inline editing with floating save, and a `DefectSelector` form component. Navbar gets reorganized with a user dropdown menu.

**Tech Stack:** ASP.NET Core 8, EF Core, MediatR, AutoMapper, PostgreSQL, MinIO (S3), React 19, TanStack Router (file-based), TanStack Query, shadcn/ui, react-i18next, Tailwind CSS.

**Spec:** `docs/superpowers/specs/2025-07-21-my-listings-gallery-defects-design.md`

---

## File Structure

### New Backend Files
```
Automotive.Marketplace.Domain/Entities/
  DefectCategory.cs
  DefectCategoryTranslation.cs
  ListingDefect.cs

Automotive.Marketplace.Infrastructure/Data/Configuration/
  DefectCategoryTranslationConfiguration.cs
  ListingDefectConfiguration.cs

Automotive.Marketplace.Infrastructure/Data/Seeders/
  DefectCategorySeeder.cs

Automotive.Marketplace.Application/Features/DefectFeatures/
  GetDefectCategories/
    GetDefectCategoriesQuery.cs
    GetDefectCategoriesQueryHandler.cs
    GetDefectCategoriesResponse.cs
    GetDefectCategoryTranslationResponse.cs
  AddListingDefect/
    AddListingDefectCommand.cs
    AddListingDefectCommandHandler.cs
  RemoveListingDefect/
    RemoveListingDefectCommand.cs
    RemoveListingDefectCommandHandler.cs
  AddDefectImage/
    AddDefectImageCommand.cs
    AddDefectImageCommandHandler.cs
  RemoveDefectImage/
    RemoveDefectImageCommand.cs
    RemoveDefectImageCommandHandler.cs

Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/
  GetMyListingsQuery.cs
  GetMyListingsQueryHandler.cs
  GetMyListingsResponse.cs

Automotive.Marketplace.Application/Mappings/
  DefectCategoryMappings.cs

Automotive.Marketplace.Application/Models/
  ListingDefectDto.cs

Automotive.Marketplace.Server/Controllers/
  DefectController.cs
```

### New Frontend Files
```
automotive.marketplace.client/src/
  components/
    layout/header/UserMenu.tsx
    gallery/ImageHoverGallery.tsx
    gallery/ImageArrowGallery.tsx
    forms/DefectSelector.tsx
  features/myListings/
    api/
      getMyListingsOptions.ts
      useDeleteMyListing.ts
    components/
      MyListingsPage.tsx
      MyListingCard.tsx
      MyListingDetail.tsx
      EditableField.tsx
    types/
      GetMyListingsResponse.ts
  api/
    defect/
      getDefectCategoriesOptions.ts
      useAddListingDefect.ts
      useRemoveListingDefect.ts
      useAddDefectImage.ts
      useRemoveDefectImage.ts
    queryKeys/
      defectKeys.ts
      myListingKeys.ts
  app/routes/
    my-listings.tsx
    my-listings/$id.tsx
  lib/i18n/locales/en/myListings.json
  lib/i18n/locales/lt/myListings.json
```

### Modified Backend Files
```
Automotive.Marketplace.Domain/Entities/Image.cs — add ListingDefectId? FK
Automotive.Marketplace.Domain/Entities/Listing.cs — add Defects navigation
Automotive.Marketplace.Infrastructure/Data/Configuration/ImageConfiguration.cs — add defect relationship
Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs — add DbSets
Automotive.Marketplace.Infrastructure/ServiceExtensions.cs — register DefectCategorySeeder
Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs — add defects
Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs — include defects
Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs — add images[], defectCount, imageCount
Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs — map all images + counts
Automotive.Marketplace.Application/Mappings/ListingMappings.cs — add new mappings
Automotive.Marketplace.Server/Controllers/ListingController.cs — add GetMy endpoint
```

### Modified Frontend Files
```
automotive.marketplace.client/src/
  components/layout/header/Header.tsx — replace logout/admin links with UserMenu
  features/listingList/components/ListingCard.tsx — use ImageHoverGallery
  features/listingList/types/GetAllListingsResponse.ts — add images[], defectCount, imageCount
  features/listingDetails/components/ListingDetailsContent.tsx — use ImageArrowGallery + defects
  features/listingDetails/types/GetListingByIdResponse.ts — add defects
  features/createListing/components/CreateListingForm.tsx — add DefectSelector
  features/createListing/schemas/createListingSchema.ts — add defects to schema
  constants/endpoints.ts — add DEFECT + LISTING.GET_MY endpoints
  lib/i18n/i18n.ts — register myListings namespace
  lib/i18n/locales/en/common.json — add userMenu keys
  lib/i18n/locales/lt/common.json — add userMenu keys
```

---

## Task 1: DefectCategory Domain Entities

**Files:**
- Create: `Automotive.Marketplace.Domain/Entities/DefectCategory.cs`
- Create: `Automotive.Marketplace.Domain/Entities/DefectCategoryTranslation.cs`
- Create: `Automotive.Marketplace.Domain/Entities/ListingDefect.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Image.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/Listing.cs`

- [ ] **Step 1: Create DefectCategory entity**

```csharp
// Automotive.Marketplace.Domain/Entities/DefectCategory.cs
namespace Automotive.Marketplace.Domain.Entities;

public class DefectCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<DefectCategoryTranslation> Translations { get; set; } = [];
}
```

- [ ] **Step 2: Create DefectCategoryTranslation entity**

```csharp
// Automotive.Marketplace.Domain/Entities/DefectCategoryTranslation.cs
namespace Automotive.Marketplace.Domain.Entities;

public class DefectCategoryTranslation : BaseEntity
{
    public Guid DefectCategoryId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual DefectCategory DefectCategory { get; set; } = null!;
}
```

- [ ] **Step 3: Create ListingDefect entity**

```csharp
// Automotive.Marketplace.Domain/Entities/ListingDefect.cs
namespace Automotive.Marketplace.Domain.Entities;

public class ListingDefect : BaseEntity
{
    public Guid ListingId { get; set; }

    public Guid? DefectCategoryId { get; set; }

    public string? CustomName { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual DefectCategory? DefectCategory { get; set; }

    public virtual ICollection<Image> Images { get; set; } = [];
}
```

- [ ] **Step 4: Extend Image entity with ListingDefectId**

Add to `Automotive.Marketplace.Domain/Entities/Image.cs`:

```csharp
public class Image : BaseEntity
{
    // ... existing properties ...

    public Guid? ListingDefectId { get; set; }

    public virtual ListingDefect? ListingDefect { get; set; }
}
```

- [ ] **Step 5: Extend Listing entity with Defects navigation**

Add to `Automotive.Marketplace.Domain/Entities/Listing.cs`:

```csharp
public class Listing : BaseEntity
{
    // ... existing properties ...

    public virtual ICollection<ListingDefect> Defects { get; set; } = [];
}
```

- [ ] **Step 6: Build to verify compilation**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 7: Commit**

```bash
git add -A && git commit -m "feat(domain): add DefectCategory, ListingDefect entities and extend Image/Listing

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 2: EF Core Configuration + Seeder + Migration

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/DefectCategoryTranslationConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ListingDefectConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Seeders/DefectCategorySeeder.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/ImageConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`
- Modify: `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs`

- [ ] **Step 1: Create DefectCategoryTranslationConfiguration**

Follow `FuelTranslationConfiguration` pattern exactly:

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/DefectCategoryTranslationConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class DefectCategoryTranslationConfiguration : IEntityTypeConfiguration<DefectCategoryTranslation>
{
    public void Configure(EntityTypeBuilder<DefectCategoryTranslation> builder)
    {
        builder.HasOne(t => t.DefectCategory)
            .WithMany(dc => dc.Translations)
            .HasForeignKey(t => t.DefectCategoryId);

        builder.HasIndex(t => new { t.DefectCategoryId, t.LanguageCode }).IsUnique();
    }
}
```

- [ ] **Step 2: Create ListingDefectConfiguration**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/ListingDefectConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingDefectConfiguration : IEntityTypeConfiguration<ListingDefect>
{
    public void Configure(EntityTypeBuilder<ListingDefect> builder)
    {
        builder.HasOne(ld => ld.Listing)
            .WithMany(l => l.Defects)
            .HasForeignKey(ld => ld.ListingId);

        builder.HasOne(ld => ld.DefectCategory)
            .WithMany()
            .HasForeignKey(ld => ld.DefectCategoryId)
            .IsRequired(false);
    }
}
```

- [ ] **Step 3: Update ImageConfiguration to add defect relationship**

Modify `Automotive.Marketplace.Infrastructure/Data/Configuration/ImageConfiguration.cs`:

```csharp
public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasOne(image => image.Listing)
            .WithMany(listing => listing.Images)
            .HasForeignKey(image => image.ListingId);

        builder.HasOne(image => image.ListingDefect)
            .WithMany(ld => ld.Images)
            .HasForeignKey(image => image.ListingDefectId)
            .IsRequired(false);
    }
}
```

- [ ] **Step 4: Add DbSets to AutomotiveContext**

Add to `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`:

```csharp
public DbSet<DefectCategory> DefectCategories { get; set; }
public DbSet<DefectCategoryTranslation> DefectCategoryTranslations { get; set; }
public DbSet<ListingDefect> ListingDefects { get; set; }
```

- [ ] **Step 5: Create DefectCategorySeeder**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Seeders/DefectCategorySeeder.cs
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class DefectCategorySeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> Categories =
    [
        (Guid.Parse("dd000001-0000-0000-0000-000000000001"), "Scratch", "Įbrėžimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000002"), "Dent", "Įlenkimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000003"), "Rust", "Rūdys"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000004"), "Paint Damage", "Dažų pažeidimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000005"), "Crack", "Įtrūkimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000006"), "Corrosion", "Korozija"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000007"), "Stain", "Dėmė"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000008"), "Mechanical Wear", "Mechaninis nusidėvėjimas"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<DefectCategory>().AnyAsync(cancellationToken))
            return;

        var entities = new List<DefectCategory>();
        foreach (var (id, en, lt) in Categories)
        {
            entities.Add(new DefectCategory
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new DefectCategoryTranslation { Id = Guid.NewGuid(), DefectCategoryId = id, LanguageCode = "en", Name = en },
                    new DefectCategoryTranslation { Id = Guid.NewGuid(), DefectCategoryId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 6: Register seeder in ServiceExtensions**

Add to `Automotive.Marketplace.Infrastructure/ServiceExtensions.cs` after the DrivetrainSeeder line:

```csharp
services.AddScoped<IDevelopmentSeeder, DefectCategorySeeder>();
```

- [ ] **Step 7: Create EF Core migration**

Run: `dotnet ef migrations add AddDefectCategoryAndListingDefect --project Automotive.Marketplace.Infrastructure --startup-project Automotive.Marketplace.Server`

If the above command doesn't work due to dotnet-ef not being installed, the implementer should check if there's a different migration approach in the project (e.g., auto-migration on startup). Check `Program.cs` for migration/seeding logic.

- [ ] **Step 8: Build to verify**

Run: `dotnet build ./Automotive.Marketplace.sln`
Expected: Build succeeded

- [ ] **Step 9: Commit**

```bash
git add -A && git commit -m "feat(infra): add DefectCategory config, seeder, and EF migration

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 3: GetDefectCategories Query Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/GetDefectCategories/GetDefectCategoriesQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/GetDefectCategories/GetDefectCategoriesResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/GetDefectCategories/GetDefectCategoryTranslationResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/GetDefectCategories/GetDefectCategoriesQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Mappings/DefectCategoryMappings.cs`

Follow the `GetAllFuels` pattern exactly.

- [ ] **Step 1: Create query, response, handler, and mappings**

```csharp
// GetDefectCategoriesQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoriesQuery : IRequest<IEnumerable<GetDefectCategoriesResponse>>;
```

```csharp
// GetDefectCategoriesResponse.cs
namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoriesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<GetDefectCategoryTranslationResponse> Translations { get; set; } = [];
}
```

```csharp
// GetDefectCategoryTranslationResponse.cs
namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoryTranslationResponse
{
    public Guid Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// GetDefectCategoriesQueryHandler.cs
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public class GetDefectCategoriesQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetDefectCategoriesQuery, IEnumerable<GetDefectCategoriesResponse>>
{
    public async Task<IEnumerable<GetDefectCategoriesResponse>> Handle(GetDefectCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository
            .AsQueryable<DefectCategory>()
            .Include(dc => dc.Translations)
            .OrderBy(dc => dc.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetDefectCategoriesResponse>>(categories);
    }
}
```

```csharp
// Automotive.Marketplace.Application/Mappings/DefectCategoryMappings.cs
using AutoMapper;
using Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class DefectCategoryMappings : Profile
{
    public DefectCategoryMappings()
    {
        CreateMap<DefectCategoryTranslation, GetDefectCategoryTranslationResponse>();
        CreateMap<DefectCategory, GetDefectCategoriesResponse>();
    }
}
```

- [ ] **Step 2: Build and commit**

Run: `dotnet build ./Automotive.Marketplace.sln`

```bash
git add -A && git commit -m "feat(app): add GetDefectCategories query handler

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 4: Defect CRUD Command Handlers

**Files:**
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/AddListingDefect/AddListingDefectCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/AddListingDefect/AddListingDefectCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/RemoveListingDefect/RemoveListingDefectCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/RemoveListingDefect/RemoveListingDefectCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/AddDefectImage/AddDefectImageCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/AddDefectImage/AddDefectImageCommandHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/RemoveDefectImage/RemoveDefectImageCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/DefectFeatures/RemoveDefectImage/RemoveDefectImageCommandHandler.cs`

- [ ] **Step 1: Create AddListingDefect command and handler**

```csharp
// AddListingDefectCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;

public sealed record AddListingDefectCommand : IRequest<Guid>
{
    public Guid ListingId { get; set; }
    public Guid? DefectCategoryId { get; set; }
    public string? CustomName { get; set; }
}
```

```csharp
// AddListingDefectCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;

public class AddListingDefectCommandHandler(IRepository repository) : IRequestHandler<AddListingDefectCommand, Guid>
{
    public async Task<Guid> Handle(AddListingDefectCommand request, CancellationToken cancellationToken)
    {
        await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);

        if (request.DefectCategoryId.HasValue)
            await repository.GetByIdAsync<DefectCategory>(request.DefectCategoryId.Value, cancellationToken);

        var defect = new ListingDefect
        {
            Id = Guid.NewGuid(),
            ListingId = request.ListingId,
            DefectCategoryId = request.DefectCategoryId,
            CustomName = request.CustomName,
        };

        await repository.CreateAsync(defect, cancellationToken);
        return defect.Id;
    }
}
```

- [ ] **Step 2: Create RemoveListingDefect command and handler**

```csharp
// RemoveListingDefectCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;

public sealed record RemoveListingDefectCommand : IRequest
{
    public Guid Id { get; set; }
}
```

```csharp
// RemoveListingDefectCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;

public class RemoveListingDefectCommandHandler(
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<RemoveListingDefectCommand>
{
    public async Task Handle(RemoveListingDefectCommand request, CancellationToken cancellationToken)
    {
        var defect = await repository
            .AsQueryable<ListingDefect>()
            .Include(ld => ld.Images)
            .FirstOrDefaultAsync(ld => ld.Id == request.Id, cancellationToken)
            ?? throw new Application.Common.Exceptions.DbEntityNotFoundException(nameof(ListingDefect), request.Id);

        foreach (var image in defect.Images)
        {
            await imageStorageService.DeleteImageAsync(image.ObjectKey);
            await repository.DeleteAsync(image, cancellationToken);
        }

        await repository.DeleteAsync(defect, cancellationToken);
    }
}
```

**Note:** The handler calls `imageStorageService.DeleteImageAsync`. Check if this method exists on `IImageStorageService`. If not, add it — it should call `DeleteObjectAsync` on the S3 client. The implementer should check the interface and add the method if missing.

- [ ] **Step 3: Create AddDefectImage command and handler**

```csharp
// AddDefectImageCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;

public sealed record AddDefectImageCommand : IRequest<Guid>
{
    public Guid ListingDefectId { get; set; }
    public IFormFile Image { get; set; } = null!;
}
```

```csharp
// AddDefectImageCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;

public class AddDefectImageCommandHandler(
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<AddDefectImageCommand, Guid>
{
    private const int MaxImagesPerDefect = 3;

    public async Task<Guid> Handle(AddDefectImageCommand request, CancellationToken cancellationToken)
    {
        var defect = await repository
            .AsQueryable<ListingDefect>()
            .Include(ld => ld.Images)
            .FirstOrDefaultAsync(ld => ld.Id == request.ListingDefectId, cancellationToken)
            ?? throw new Application.Common.Exceptions.DbEntityNotFoundException(nameof(ListingDefect), request.ListingDefectId);

        if (defect.Images.Count >= MaxImagesPerDefect)
            throw new InvalidOperationException($"A defect can have at most {MaxImagesPerDefect} images.");

        var uniqueFileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
        var uploadResult = await imageStorageService.UploadImageAsync(request.Image, uniqueFileName);

        var image = new Image
        {
            Id = Guid.NewGuid(),
            ListingId = defect.ListingId,
            ListingDefectId = defect.Id,
            BucketName = uploadResult.BucketName,
            ObjectKey = uploadResult.ObjectKey,
            OriginalFileName = request.Image.FileName,
            ContentType = request.Image.ContentType,
            FileSize = uploadResult.FileSize,
            AltText = defect.CustomName ?? "Defect image",
        };

        await repository.CreateAsync(image, cancellationToken);
        return image.Id;
    }
}
```

- [ ] **Step 4: Create RemoveDefectImage command and handler**

```csharp
// RemoveDefectImageCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;

public sealed record RemoveDefectImageCommand : IRequest
{
    public Guid Id { get; set; }
}
```

```csharp
// RemoveDefectImageCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;

public class RemoveDefectImageCommandHandler(
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<RemoveDefectImageCommand>
{
    public async Task Handle(RemoveDefectImageCommand request, CancellationToken cancellationToken)
    {
        var image = await repository.GetByIdAsync<Image>(request.Id, cancellationToken);
        await imageStorageService.DeleteImageAsync(image.ObjectKey);
        await repository.DeleteAsync(image, cancellationToken);
    }
}
```

- [ ] **Step 5: Build and commit**

Run: `dotnet build ./Automotive.Marketplace.sln`

```bash
git add -A && git commit -m "feat(app): add defect CRUD command handlers (add/remove defect and images)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 5: GetMyListings Query + Extend GetListingById + Extend GetAllListings

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ListingFeatures/GetMyListings/GetMyListingsQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Models/ListingDefectDto.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetAllListings/GetAllListingsQueryHandler.cs`
- Modify: `Automotive.Marketplace.Application/Mappings/ListingMappings.cs`

- [ ] **Step 1: Create ListingDefectDto**

```csharp
// Automotive.Marketplace.Application/Models/ListingDefectDto.cs
namespace Automotive.Marketplace.Application.Models;

public sealed class ListingDefectDto
{
    public Guid Id { get; set; }
    public Guid? DefectCategoryId { get; set; }
    public string? DefectCategoryName { get; set; }
    public string? CustomName { get; set; }
    public IEnumerable<ImageDto> Images { get; set; } = [];
}
```

- [ ] **Step 2: Create GetMyListings query, response, handler**

```csharp
// GetMyListingsQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public sealed record GetMyListingsQuery : IRequest<IEnumerable<GetMyListingsResponse>>
{
    public Guid SellerId { get; set; }
}
```

```csharp
// GetMyListingsResponse.cs
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
    public int ImageCount { get; set; }
    public int DefectCount { get; set; }
}
```

```csharp
// GetMyListingsQueryHandler.cs
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
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
            .Include(l => l.Images)
            .Include(l => l.Defects)
            .Where(l => l.SellerId == request.SellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = new List<GetMyListingsResponse>();
        foreach (var listing in listings)
        {
            var mapped = mapper.Map<GetMyListingsResponse>(listing);
            mapped.ImageCount = listing.Images.Count;
            mapped.DefectCount = listing.Defects.Count;

            var firstImage = listing.Images.Where(i => i.ListingDefectId == null).FirstOrDefault();
            if (firstImage != null)
            {
                mapped.Thumbnail = new Application.Models.ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey),
                    AltText = firstImage.AltText,
                };
            }
            response.Add(mapped);
        }

        return response;
    }
}
```

- [ ] **Step 3: Add GetMyListingsResponse mapping to ListingMappings.cs**

Add to `Automotive.Marketplace.Application/Mappings/ListingMappings.cs` constructor:

```csharp
CreateMap<Listing, GetMyListingsResponse>()
    .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.Thumbnail, opt => opt.Ignore())
    .ForMember(dest => dest.ImageCount, opt => opt.Ignore())
    .ForMember(dest => dest.DefectCount, opt => opt.Ignore());
```

Also add the using statement for GetMyListings namespace.

- [ ] **Step 4: Extend GetListingByIdResponse with defects**

Add to `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdResponse.cs`:

```csharp
public IEnumerable<Automotive.Marketplace.Application.Models.ListingDefectDto> Defects { get; set; } = [];
```

- [ ] **Step 5: Extend GetListingByIdQueryHandler to include defects**

In `GetListingByIdQueryHandler.cs`, add `.Include(l => l.Defects).ThenInclude(d => d.DefectCategory)` and `.Include(l => l.Defects).ThenInclude(d => d.Images)` to the query chain.

After the existing image mapping loop, add defect mapping:

```csharp
var defects = new List<Automotive.Marketplace.Application.Models.ListingDefectDto>();
foreach (var defect in listing.Defects)
{
    var defectImages = new List<Automotive.Marketplace.Application.Models.ImageDto>();
    foreach (var img in defect.Images)
    {
        defectImages.Add(new Automotive.Marketplace.Application.Models.ImageDto
        {
            Url = await imageStorageService.GetPresignedUrlAsync(img.ObjectKey),
            AltText = img.AltText,
        });
    }
    defects.Add(new Automotive.Marketplace.Application.Models.ListingDefectDto
    {
        Id = defect.Id,
        DefectCategoryId = defect.DefectCategoryId,
        DefectCategoryName = defect.DefectCategory?.Name,
        CustomName = defect.CustomName,
        Images = defectImages,
    });
}
response.Defects = defects;
```

Also update the ListingMappings to ignore Defects: `.ForMember(dest => dest.Defects, opt => opt.Ignore())` on the `GetListingByIdResponse` mapping.

- [ ] **Step 6: Extend GetAllListingsResponse with images array + counts**

Add to `GetAllListingsResponse.cs`:

```csharp
public IEnumerable<Automotive.Marketplace.Application.Models.ImageDto> Images { get; set; } = [];
public int ImageCount { get; set; }
public int DefectCount { get; set; }
```

- [ ] **Step 7: Update GetAllListingsQueryHandler to map all images + counts**

In `GetAllListingsQueryHandler.cs`, add `.Include(l => l.Defects)` to the query chain.

In the foreach loop, after existing thumbnail mapping, add:

```csharp
// Map all listing images (non-defect) for hover gallery
var allImages = new List<Automotive.Marketplace.Application.Models.ImageDto>();
foreach (var img in listing.Images.Where(i => i.ListingDefectId == null))
{
    allImages.Add(new Automotive.Marketplace.Application.Models.ImageDto
    {
        Url = await imageStorageService.GetPresignedUrlAsync(img.ObjectKey),
        AltText = img.AltText,
    });
}
mappedListing.Images = allImages;
mappedListing.ImageCount = listing.Images.Count;
mappedListing.DefectCount = listing.Defects.Count;
```

Also update ListingMappings to ignore these new fields:
```csharp
.ForMember(dest => dest.Images, opt => opt.Ignore())
.ForMember(dest => dest.ImageCount, opt => opt.Ignore())
.ForMember(dest => dest.DefectCount, opt => opt.Ignore());
```

- [ ] **Step 8: Build and commit**

Run: `dotnet build ./Automotive.Marketplace.sln`

```bash
git add -A && git commit -m "feat(app): add GetMyListings query and extend listing responses with defects and image arrays

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 6: DefectController + Extend ListingController

**Files:**
- Create: `Automotive.Marketplace.Server/Controllers/DefectController.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ListingController.cs`
- Modify: `Automotive.Marketplace.Application/Features/ListingFeatures/GetListingById/GetListingByIdQueryHandler.cs` — filter listing images to exclude defect images

- [ ] **Step 1: Create DefectController**

```csharp
// Automotive.Marketplace.Server/Controllers/DefectController.cs
using Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;
using Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;
using Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;
using Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;
using Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class DefectController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetDefectCategoriesResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDefectCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult<Guid>> Add(
        [FromBody] AddListingDefectCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> Remove(
        [FromQuery] RemoveListingDefectCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult<Guid>> AddImage(
        [FromForm] AddDefectImageCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> RemoveImage(
        [FromQuery] RemoveDefectImageCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

- [ ] **Step 2: Add GetMy endpoint to ListingController**

Add to `Automotive.Marketplace.Server/Controllers/ListingController.cs`:

```csharp
[HttpGet]
[Protect(Permission.ManageListings)]
public async Task<ActionResult<IEnumerable<GetMyListingsResponse>>> GetMy(
    CancellationToken cancellationToken)
{
    var query = new GetMyListingsQuery { SellerId = UserId };
    var result = await mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

Add the using statement for `GetMyListings` namespace.

- [ ] **Step 3: Fix GetListingById image filtering**

In `GetListingByIdQueryHandler.cs`, update the image mapping loop to only include non-defect images in the main `Images` list:

```csharp
foreach (var image in listing.Images.Where(i => i.ListingDefectId == null))
```

Defect images are already included via the defect DTOs.

- [ ] **Step 4: Build and test backend**

Run: `dotnet build ./Automotive.Marketplace.sln`
Run: `dotnet test ./Automotive.Marketplace.sln` (if tests exist)

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(server): add DefectController and GetMy listing endpoint

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 7: Frontend — Endpoints, Types, API Hooks

**Files:**
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Create: `automotive.marketplace.client/src/api/queryKeys/defectKeys.ts`
- Create: `automotive.marketplace.client/src/api/queryKeys/myListingKeys.ts`
- Create: `automotive.marketplace.client/src/api/defect/getDefectCategoriesOptions.ts`
- Create: `automotive.marketplace.client/src/api/defect/useAddListingDefect.ts`
- Create: `automotive.marketplace.client/src/api/defect/useRemoveListingDefect.ts`
- Create: `automotive.marketplace.client/src/api/defect/useAddDefectImage.ts`
- Create: `automotive.marketplace.client/src/api/defect/useRemoveDefectImage.ts`
- Create: `automotive.marketplace.client/src/features/myListings/api/getMyListingsOptions.ts`
- Create: `automotive.marketplace.client/src/features/myListings/api/useDeleteMyListing.ts`
- Create: `automotive.marketplace.client/src/features/myListings/types/GetMyListingsResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingList/types/GetAllListingsResponse.ts`
- Modify: `automotive.marketplace.client/src/features/listingDetails/types/GetListingByIdResponse.ts`

- [ ] **Step 1: Add endpoints**

Add to `endpoints.ts`:

```typescript
DEFECT: {
  GET_CATEGORIES: "/Defect/GetCategories",
  ADD: "/Defect/Add",
  REMOVE: "/Defect/Remove",
  ADD_IMAGE: "/Defect/AddImage",
  REMOVE_IMAGE: "/Defect/RemoveImage",
},
```

Add to the `LISTING` object:
```typescript
GET_MY: "/Listing/GetMy",
```

- [ ] **Step 2: Create query keys**

```typescript
// src/api/queryKeys/defectKeys.ts
export const defectKeys = {
  all: () => ["defects"] as const,
  categories: () => [...defectKeys.all(), "categories"] as const,
};
```

```typescript
// src/api/queryKeys/myListingKeys.ts
export const myListingKeys = {
  all: () => ["myListings"] as const,
};
```

- [ ] **Step 3: Create types**

```typescript
// src/features/myListings/types/GetMyListingsResponse.ts
type Thumbnail = {
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
  thumbnail: Thumbnail | null;
  imageCount: number;
  defectCount: number;
};
```

Update `GetAllListingsResponse.ts` — add:
```typescript
images: { url: string; altText: string }[];
imageCount: number;
defectCount: number;
```

Update `GetListingByIdResponse.ts` — add:
```typescript
defects: ListingDefectDto[];
```

And add the type:
```typescript
export type ListingDefectDto = {
  id: string;
  defectCategoryId?: string;
  defectCategoryName?: string;
  customName?: string;
  images: { url: string; altText: string }[];
};
```

- [ ] **Step 4: Create all API query options and mutation hooks**

Follow the existing patterns (`getListingByIdOptions`, `useDeleteListing`, `getAllFuelsOptions`).

Create `getDefectCategoriesOptions.ts`, `useAddListingDefect.ts`, `useRemoveListingDefect.ts`, `useAddDefectImage.ts`, `useRemoveDefectImage.ts`, `getMyListingsOptions.ts`, `useDeleteMyListing.ts`.

Key patterns:
- Query options use `queryOptions()` from TanStack Query
- Mutations use `useMutation()` with `meta.successMessage`, `meta.errorMessage`, and `meta.invalidatesQuery`
- File uploads use `FormData` and `axiosClient.post` with `{ headers: { 'Content-Type': 'multipart/form-data' } }`

- [ ] **Step 5: Build frontend**

Run: `cd automotive.marketplace.client && npm run build`

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat(fe): add defect and my-listings API endpoints, types, and hooks

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 8: Frontend — UserMenu + Navbar Reorganization

**Files:**
- Create: `automotive.marketplace.client/src/components/layout/header/UserMenu.tsx`
- Modify: `automotive.marketplace.client/src/components/layout/header/Header.tsx`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/common.json` — add userMenu keys
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/common.json` — add userMenu keys

- [ ] **Step 1: Add translation keys to common.json (EN)**

Add to `en/common.json`:
```json
"userMenu": {
  "myListings": "My Listings",
  "makes": "Makes",
  "models": "Models",
  "variants": "Variants",
  "profileSettings": "Profile Settings",
  "logOut": "Log out",
  "sectionMyListings": "My Listings",
  "sectionAdmin": "Admin",
  "sectionAccount": "Account",
  "comingSoon": "Coming soon"
}
```

Add same keys to `lt/common.json` with Lithuanian translations:
```json
"userMenu": {
  "myListings": "Mano skelbimai",
  "makes": "Markės",
  "models": "Modeliai",
  "variants": "Variantai",
  "profileSettings": "Profilio nustatymai",
  "logOut": "Atsijungti",
  "sectionMyListings": "Mano skelbimai",
  "sectionAdmin": "Administravimas",
  "sectionAccount": "Paskyra",
  "comingSoon": "Greitai"
}
```

- [ ] **Step 2: Create UserMenu component**

Create `src/components/layout/header/UserMenu.tsx` using shadcn `DropdownMenu`. The component should:
- Show a circle avatar with the user's first initial (derive from username stored somewhere, or just use a generic user icon)
- Use `DropdownMenuLabel` for section headers
- Use `DropdownMenuSeparator` between sections
- Show "My Listings" section with link to `/my-listings`
- Show "Admin" section (Makes, Models, Variants links) only if `permissions.includes(PERMISSIONS.ViewMakes)` (or appropriate permission)
- Show "Account" section with disabled "Profile Settings" and red "Log out"
- Log out should dispatch `clearCredentials` and call the logout API
- Use `useTranslation("common")` for all labels
- Use `Link` from TanStack Router for navigation

- [ ] **Step 3: Update Header.tsx**

Remove:
- The inline admin links (Makes, Models, Variants buttons with permission checks)
- The `LogoutButton` / `RegisterButton` conditional

Add:
- `<UserMenu />` where the logout button was (only when `userId` is truthy)
- Keep `RegisterButton` for logged-out users

The header should now be: Logo | Sell Your Car | Inbox | Saved | LanguageSwitcher | ThemeToggle | UserMenu (or RegisterButton)

- [ ] **Step 4: Build and verify**

Run: `cd automotive.marketplace.client && npm run build`

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat(fe): add UserMenu dropdown and reorganize navbar

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 9: Frontend — Image Gallery Components

**Files:**
- Create: `automotive.marketplace.client/src/components/gallery/ImageHoverGallery.tsx`
- Create: `automotive.marketplace.client/src/components/gallery/ImageArrowGallery.tsx`

- [ ] **Step 1: Create ImageHoverGallery (card hover-swipe)**

Create `src/components/gallery/ImageHoverGallery.tsx`:

```typescript
type ImageHoverGalleryProps = {
  images: { url: string; altText: string }[];
  fallbackUrl: string;
  className?: string;
};
```

Implementation:
- Track `activeIndex` state (default 0)
- `onMouseMove` handler: calculate zone index from `(e.clientX - rect.left) / rect.width * images.length`
- `onMouseLeave`: reset to index 0
- Render the image at `activeIndex`
- Show dot indicators at the bottom (absolute positioned): one dot per image, active dot is white, others are translucent
- If `images` is empty, show fallback image
- Dots only show on hover (use group/group-hover Tailwind classes)

- [ ] **Step 2: Create ImageArrowGallery (detail page gallery)**

Create `src/components/gallery/ImageArrowGallery.tsx`:

```typescript
type GalleryImage = {
  url: string;
  altText: string;
  defectName?: string;
};

type ImageArrowGalleryProps = {
  images: GalleryImage[];
  className?: string;
};
```

Implementation:
- Track `activeIndex` state
- Left/right arrow buttons (absolute positioned, semi-transparent circles with `‹` `›`)
- Counter in top-right: "N / M"
- Main image area with aspect-video
- Thumbnail strip below: horizontal scroll, each ~72px wide
  - Active thumbnail has `ring-2 ring-primary` border
  - Defect thumbnails have `ring-2 ring-amber-500` border + small label below
  - Put a thin separator `<div>` between regular and defect thumbnails
- When viewing a defect image, show an info bar below: amber background, defect name
- Click thumbnail to jump to that index
- Arrow keys cycle (wrap around)

- [ ] **Step 3: Build and commit**

Run: `cd automotive.marketplace.client && npm run build`

```bash
git add -A && git commit -m "feat(fe): add ImageHoverGallery and ImageArrowGallery components

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 10: Frontend — Integrate Galleries into ListingCard + ListingDetailsContent

**Files:**
- Modify: `automotive.marketplace.client/src/features/listingList/components/ListingCard.tsx`
- Modify: `automotive.marketplace.client/src/features/listingDetails/components/ListingDetailsContent.tsx`

- [ ] **Step 1: Update ListingCard to use ImageHoverGallery**

Replace the `<img>` tag in `ListingCard.tsx` with `<ImageHoverGallery>`:

```tsx
<ImageHoverGallery
  images={listing.images}
  fallbackUrl="https://placehold.co/800x600?text=No+Image"
  className="aspect-[4/3]"
/>
```

Keep the like button overlay positioned on top of the gallery.

- [ ] **Step 2: Update ListingDetailsContent to use ImageArrowGallery**

Replace the `<img>` tag in `ListingDetailsContent.tsx` with `<ImageArrowGallery>`.

Build the gallery images array by combining listing images + defect images:

```tsx
const galleryImages: GalleryImage[] = [
  ...listing.images.map(img => ({ url: img.url, altText: img.altText })),
  ...listing.defects.flatMap(defect =>
    defect.images.map(img => ({
      url: img.url,
      altText: img.altText,
      defectName: defect.customName ?? defect.defectCategoryName ?? "Defect",
    }))
  ),
];
```

Also add a "Defects" section below the description if `listing.defects.length > 0`, showing defect pills with names and photo counts.

- [ ] **Step 3: Build and commit**

Run: `cd automotive.marketplace.client && npm run build`

```bash
git add -A && git commit -m "feat(fe): integrate image galleries into ListingCard and ListingDetailsContent

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 11: Frontend — DefectSelector Component

**Files:**
- Create: `automotive.marketplace.client/src/components/forms/DefectSelector.tsx`

- [ ] **Step 1: Create DefectSelector component**

This is a reusable form component used in both CreateListingForm and MyListingDetail.

**Two modes:**
1. **Form mode** (for create listing): manages local state, returns selected defects + files to parent
2. **API mode** (for inline edit): calls defect API directly for an existing listing

```typescript
// Form mode props
type DefectSelectorFormProps = {
  mode: "form";
  selectedDefects: FormDefect[];
  onDefectsChange: (defects: FormDefect[]) => void;
};

type FormDefect = {
  defectCategoryId?: string;
  customName?: string;
  images: Blob[];
};

// API mode props
type DefectSelectorApiProps = {
  mode: "api";
  listingId: string;
  existingDefects: ListingDefectDto[];
};

type DefectSelectorProps = DefectSelectorFormProps | DefectSelectorApiProps;
```

Implementation:
- Fetch defect categories with `useQuery(getDefectCategoriesOptions)`
- Display `getTranslatedName` for each category
- 2-column grid of checkboxes (toggling adds/removes from selected list)
- Custom defect input + "Add" button
- Selected defects section below with:
  - Defect name (amber left border)
  - Photo count "N / 3"
  - Thumbnail previews with remove buttons
  - "+" button to add photos (up to 3 per defect)
- In "form" mode: manage state via `onDefectsChange` callback
- In "api" mode: call `useAddListingDefect`, `useRemoveListingDefect`, `useAddDefectImage`, `useRemoveDefectImage` mutations directly

- [ ] **Step 2: Build and commit**

Run: `cd automotive.marketplace.client && npm run build`

```bash
git add -A && git commit -m "feat(fe): add DefectSelector reusable form component

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 12: Frontend — My Listings Page + Route

**Files:**
- Create: `automotive.marketplace.client/src/features/myListings/components/MyListingsPage.tsx`
- Create: `automotive.marketplace.client/src/features/myListings/components/MyListingCard.tsx`
- Create: `automotive.marketplace.client/src/app/routes/my-listings.tsx`
- Create: `automotive.marketplace.client/src/lib/i18n/locales/en/myListings.json`
- Create: `automotive.marketplace.client/src/lib/i18n/locales/lt/myListings.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/i18n.ts` — register myListings namespace

- [ ] **Step 1: Create i18n translations**

```json
// en/myListings.json
{
  "page": {
    "title": "My Listings",
    "createListing": "Create Listing",
    "emptyState": "You haven't created any listings yet.",
    "createFirst": "Create your first listing!"
  },
  "card": {
    "active": "Active",
    "sold": "Sold",
    "defects": "{{count}} defects",
    "edit": "Edit",
    "delete": "Delete"
  },
  "detail": {
    "backToMyListings": "Back to My Listings",
    "deleteListing": "Delete Listing",
    "unsavedChanges": "{{count}} unsaved changes",
    "discard": "Discard",
    "saveChanges": "Save Changes",
    "defects": "Defects",
    "addDefect": "Add defect",
    "deleteConfirmTitle": "Delete listing?",
    "deleteConfirmDescription": "This action cannot be undone. This will permanently delete your listing.",
    "cancel": "Cancel",
    "confirm": "Delete"
  },
  "fields": {
    "price": "Price",
    "city": "City",
    "mileage": "Mileage",
    "description": "Description",
    "colour": "Colour",
    "vin": "VIN",
    "isUsed": "Condition",
    "steeringWheel": "Steering Wheel",
    "used": "Used",
    "new": "New",
    "left": "Left",
    "right": "Right"
  }
}
```

Create corresponding `lt/myListings.json` with Lithuanian translations.

- [ ] **Step 2: Register namespace in i18n.ts**

Import the new JSON files and add `myListings: myListingsEn` and `myListings: myListingsLt` to the resources.

- [ ] **Step 3: Create MyListingCard component**

Shows a single listing in a horizontal card layout (single column). Displays:
- Thumbnail
- Title (Year Make Model)
- City, mileage, price
- Status badge
- Defect count badge (if > 0)
- Image count
- Edit button → navigates to `/my-listings/${id}`
- Delete button → confirmation dialog → API call

Sold listings appear with reduced opacity and no action buttons.

- [ ] **Step 4: Create MyListingsPage**

Uses `useQuery(getMyListingsOptions)` to fetch data. Shows:
- Page header with title + "Create Listing" button (links to `/listing/create`)
- List of `MyListingCard` components (single column)
- Empty state when no listings

- [ ] **Step 5: Create route file**

```typescript
// src/app/routes/my-listings.tsx
import { createFileRoute } from "@tanstack/react-router";
import MyListingsPage from "@/features/myListings/components/MyListingsPage";

export const Route = createFileRoute("/my-listings")({
  component: MyListingsPage,
});
```

- [ ] **Step 6: Build, lint, format, commit**

```bash
cd automotive.marketplace.client && npm run build && npx prettier --write src/
git add -A && git commit -m "feat(fe): add My Listings page with listing cards and route

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 13: Frontend — Inline Edit Detail View

**Files:**
- Create: `automotive.marketplace.client/src/features/myListings/components/MyListingDetail.tsx`
- Create: `automotive.marketplace.client/src/features/myListings/components/EditableField.tsx`
- Create: `automotive.marketplace.client/src/app/routes/my-listings/$id.tsx`

- [ ] **Step 1: Create EditableField component**

A generic inline-editable field component:

```typescript
type EditableFieldProps = {
  label: string;
  value: string | number | boolean;
  displayValue?: string;
  type: "text" | "number" | "textarea" | "toggle";
  toggleLabels?: { on: string; off: string };
  onConfirm: (newValue: string | number | boolean) => void;
};
```

Implementation:
- Read mode: shows label, display value, pencil icon button
- Edit mode: shows label, appropriate input, confirm (✓) and cancel (✗) buttons
- For "toggle" type: shows a switch/toggle between two states
- Does NOT save to API — just calls `onConfirm` with the new value

- [ ] **Step 2: Create MyListingDetail component**

The main inline edit view. Uses `useSuspenseQuery(getListingByIdOptions({ id }))` to get listing data.

Layout:
- Back link → `/my-listings`
- `<ImageArrowGallery>` with combined listing + defect images
- Title area with status badge + delete button
- List of `<EditableField>` components for each editable field
- Tracks pending changes in local state: `Record<string, any>`
- Floating save bar appears when `Object.keys(pendingChanges).length > 0`
- "Save Changes" calls `useUpdateListing` mutation with only changed fields
- "Discard" clears pending changes
- `<DefectSelector mode="api">` at the bottom

Editable fields: Price, City, Mileage, Description, Colour, VIN, IsUsed (toggle), IsSteeringWheelRight (toggle)

Non-editable (display only): Make, Model, Fuel, Transmission, Body Type, Drivetrain, Doors, Power, Engine Size

Delete: confirmation dialog using shadcn `AlertDialog` → calls delete mutation → navigates to `/my-listings`

- [ ] **Step 3: Create route file**

```typescript
// src/app/routes/my-listings/$id.tsx
import { createFileRoute } from "@tanstack/react-router";
import MyListingDetail from "@/features/myListings/components/MyListingDetail";

export const Route = createFileRoute("/my-listings/$id")({
  component: () => {
    const { id } = Route.useParams();
    return <MyListingDetail id={id} />;
  },
});
```

- [ ] **Step 4: Build, lint, format, commit**

```bash
cd automotive.marketplace.client && npm run build && npx prettier --write src/
git add -A && git commit -m "feat(fe): add inline edit detail view for My Listings

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 14: Frontend — Integrate DefectSelector into CreateListingForm

**Files:**
- Modify: `automotive.marketplace.client/src/features/createListing/components/CreateListingForm.tsx`
- Modify: `automotive.marketplace.client/src/features/createListing/schemas/createListingSchema.ts`

- [ ] **Step 1: Extend create listing schema with optional defects**

The `CreateListingSchema` doesn't need to validate individual defect fields — just accept an array. Add to the schema:

```typescript
defects: z.array(z.object({
  defectCategoryId: z.string().optional(),
  customName: z.string().optional(),
  images: z.array(z.instanceof(Blob)).max(3),
})).optional().default([]),
```

- [ ] **Step 2: Add DefectSelector to CreateListingForm**

After the image upload section and before the submit button, add:

```tsx
<DefectSelector
  mode="form"
  selectedDefects={form.watch("defects") ?? []}
  onDefectsChange={(defects) => form.setValue("defects", defects)}
/>
```

- [ ] **Step 3: Update form submission to include defects**

The create listing mutation needs to send defects as part of the FormData. For each defect, append the category ID (or custom name) and its images. The backend `CreateListingCommand` will need to accept these — if the current command doesn't support it, the implementer should extend it or use separate API calls after creation (call AddListingDefect + AddDefectImage for each defect after the listing is created).

The simpler approach: after `createListing` succeeds and returns the listing ID, loop through defects and call `addListingDefect` + `addDefectImage` for each. This uses the APIs from Task 7.

- [ ] **Step 4: Build, lint, format, commit**

```bash
cd automotive.marketplace.client && npm run build && npx prettier --write src/
git add -A && git commit -m "feat(fe): integrate DefectSelector into CreateListingForm

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

## Task 15: i18n + Final Polish + Verification

**Files:**
- Various i18n JSON files
- Various component files for final polish

- [ ] **Step 1: Ensure all new strings are translated**

Check all new components for hardcoded strings. Ensure everything uses `useTranslation()` with appropriate namespace keys. Add any missing keys to both EN and LT JSON files.

- [ ] **Step 2: Run full build**

```bash
cd automotive.marketplace.client && npm run build
```

- [ ] **Step 3: Run lint and format**

```bash
cd automotive.marketplace.client && npm run lint && npm run format:check
```

Fix any issues.

- [ ] **Step 4: Build backend**

```bash
dotnet build ./Automotive.Marketplace.sln
dotnet test ./Automotive.Marketplace.sln
```

- [ ] **Step 5: Final commit**

```bash
git add -A && git commit -m "feat(i18n): translate all new strings and final polish

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
