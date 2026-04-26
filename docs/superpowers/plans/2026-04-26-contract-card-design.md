# Contract Card Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Buyer–Seller Contract Card feature — a collaborative multi-step workflow in the chat interface that pre-fills a Lithuanian VKTI vehicle purchase-sale form and generates a compliant PDF.

**Architecture:** Follows the existing chat card pattern (AvailabilityCard, Meeting, Offer). New domain entities (`ContractCard`, `ContractSellerSubmission`, `ContractBuyerSubmission`) are persisted via EF Core. CQRS handlers in the Application layer drive all state transitions. SignalR broadcasts `ContractCardUpdated` events; the frontend calls `GetContractCard` (REST) on receipt to refresh UI. PDF is generated on-demand via QuestPDF in the Infrastructure layer.

**Tech Stack:** ASP.NET Core 8, EF Core 9 (PostgreSQL), MediatR, QuestPDF, SignalR, React 19, TypeScript, TanStack Query, i18next, Lucide React, shadcn/ui

---

## File Map

**New files — Backend:**
- `Automotive.Marketplace.Domain/Entities/ContractCard.cs`
- `Automotive.Marketplace.Domain/Entities/ContractSellerSubmission.cs`
- `Automotive.Marketplace.Domain/Entities/ContractBuyerSubmission.cs`
- `Automotive.Marketplace.Domain/Enums/ContractCardStatus.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractCardConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractSellerSubmissionConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractBuyerSubmissionConfiguration.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/ContractCardBuilder.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/ContractSellerSubmissionBuilder.cs`
- `Automotive.Marketplace.Infrastructure/Data/Builders/ContractBuyerSubmissionBuilder.cs`
- `Automotive.Marketplace.Application/Interfaces/Services/IContractPdfService.cs`
- `Automotive.Marketplace.Infrastructure/Services/ContractPdfService.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommandHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQuery.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQuery.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQuery.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQueryHandler.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileResponse.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommand.cs`
- `Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommandHandler.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RequestContractCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToContractCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelContractCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractSellerFormCommandHandlerTests.cs`
- `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractBuyerFormCommandHandlerTests.cs`

**New files — Frontend:**
- `automotive.marketplace.client/src/features/chat/types/ContractCard.ts`
- `automotive.marketplace.client/src/features/chat/types/ContractEventPayloads.ts`
- `automotive.marketplace.client/src/features/chat/api/getContractCardOptions.ts`
- `automotive.marketplace.client/src/features/chat/api/getUserContractProfileOptions.ts`
- `automotive.marketplace.client/src/features/chat/api/useUpdateUserContractProfile.ts`
- `automotive.marketplace.client/src/features/chat/components/ContractCard.tsx`
- `automotive.marketplace.client/src/features/chat/components/ContractFormDialog.tsx`

**Modified files — Backend:**
- `Automotive.Marketplace.Domain/Entities/Message.cs` — add `ContractCardId Guid?` + navigation
- `Automotive.Marketplace.Domain/Enums/MessageType.cs` — add `Contract = 4`
- `Automotive.Marketplace.Domain/Entities/User.cs` — add `PhoneNumber`, `PersonalIdCode`, `Address`
- `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs` — add 3 new DbSets
- `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs` — add ContractCard relation
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs` — add `ContractCardData`
- `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs` — map ContractCard
- `Automotive.Marketplace.Server/Hubs/ChatHub.cs` — add 4 hub methods
- `Automotive.Marketplace.Server/Controllers/ChatController.cs` — add 4 REST endpoints
- `Automotive.Marketplace.Infrastructure/Infrastructure.csproj` — add QuestPDF package

**Modified files — Frontend:**
- `automotive.marketplace.client/src/features/chat/constants/chatHub.ts` — add contract hub method constants
- `automotive.marketplace.client/src/features/chat/api/useChatHub.ts` — add ContractCardUpdated handler
- `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts` — add `ContractCard` type + `"Contract"` message type
- `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx` — add 4th menu item
- `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx` — render ContractCard messages
- i18n `lt/chat.json` + `en/chat.json` — all new strings

---

### Task 1: Domain Entities and Enum

**Files:**
- Create: `Automotive.Marketplace.Domain/Enums/ContractCardStatus.cs`
- Create: `Automotive.Marketplace.Domain/Entities/ContractCard.cs`
- Create: `Automotive.Marketplace.Domain/Entities/ContractSellerSubmission.cs`
- Create: `Automotive.Marketplace.Domain/Entities/ContractBuyerSubmission.cs`

- [ ] **Step 1: Create ContractCardStatus enum**

```csharp
// Automotive.Marketplace.Domain/Enums/ContractCardStatus.cs
namespace Automotive.Marketplace.Domain.Enums;

public enum ContractCardStatus
{
    Pending = 0,
    Active = 1,
    SellerSubmitted = 2,
    BuyerSubmitted = 3,
    Complete = 4,
    Declined = 5,
    Cancelled = 6,
}
```

- [ ] **Step 2: Create ContractCard entity**

```csharp
// Automotive.Marketplace.Domain/Entities/ContractCard.cs
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class ContractCard : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public ContractCardStatus Status { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual ContractSellerSubmission? SellerSubmission { get; set; }

    public virtual ContractBuyerSubmission? BuyerSubmission { get; set; }

    public virtual Message? Message { get; set; }
}
```

- [ ] **Step 3: Create ContractSellerSubmission entity**

```csharp
// Automotive.Marketplace.Domain/Entities/ContractSellerSubmission.cs
namespace Automotive.Marketplace.Domain.Entities;

public class ContractSellerSubmission : BaseEntity
{
    public Guid ContractCardId { get; set; }

    // Vehicle
    public string? SdkCode { get; set; }
    public string Make { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public string? RegistrationCertificate { get; set; }
    public bool TechnicalInspectionValid { get; set; }
    public bool WasDamaged { get; set; }
    public bool? DamageKnown { get; set; }
    public bool DefectBrakes { get; set; }
    public bool DefectSafety { get; set; }
    public bool DefectSteering { get; set; }
    public bool DefectExhaust { get; set; }
    public bool DefectLighting { get; set; }
    public string? DefectDetails { get; set; }
    public decimal? Price { get; set; }

    // Seller personal
    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = "Lietuva";
    public DateTime SubmittedAt { get; set; }

    public virtual ContractCard ContractCard { get; set; } = null!;
}
```

- [ ] **Step 4: Create ContractBuyerSubmission entity**

```csharp
// Automotive.Marketplace.Domain/Entities/ContractBuyerSubmission.cs
namespace Automotive.Marketplace.Domain.Entities;

public class ContractBuyerSubmission : BaseEntity
{
    public Guid ContractCardId { get; set; }

    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public virtual ContractCard ContractCard { get; set; } = null!;
}
```

- [ ] **Step 5: Build to verify**

```bash
cd /path/to/repo
dotnet build Automotive.Marketplace.Domain/Automotive.Marketplace.Domain.csproj --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Domain/
git commit -m "feat(domain): add ContractCard entities and ContractCardStatus enum"
```

---

### Task 2: Update Existing Domain Entities

**Files:**
- Modify: `Automotive.Marketplace.Domain/Entities/Message.cs`
- Modify: `Automotive.Marketplace.Domain/Enums/MessageType.cs`
- Modify: `Automotive.Marketplace.Domain/Entities/User.cs`

- [ ] **Step 1: Add `Contract = 4` to MessageType enum**

In `Automotive.Marketplace.Domain/Enums/MessageType.cs`, add after `Availability = 3`:

```csharp
    Contract = 4,
```

Final file:
```csharp
namespace Automotive.Marketplace.Domain.Enums;

public enum MessageType
{
    Text = 0,
    Offer = 1,
    Meeting = 2,
    Availability = 3,
    Contract = 4,
}
```

- [ ] **Step 2: Add `ContractCardId` and navigation to Message**

Replace the body of `Automotive.Marketplace.Domain/Entities/Message.cs` with:

```csharp
namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public MessageType MessageType { get; set; }

    public Guid? OfferId { get; set; }

    public Guid? MeetingId { get; set; }

    public Guid? AvailabilityCardId { get; set; }

    public Guid? ContractCardId { get; set; }

    public virtual Offer? Offer { get; set; }

    public virtual Meeting? Meeting { get; set; }

    public virtual AvailabilityCard? AvailabilityCard { get; set; }

    public virtual ContractCard? ContractCard { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
```

- [ ] **Step 3: Add nullable contract profile fields to User**

In `Automotive.Marketplace.Domain/Entities/User.cs`, add three nullable properties after `HashedPassword`:

```csharp
    public string? PhoneNumber { get; set; }

    public string? PersonalIdCode { get; set; }

    public string? Address { get; set; }
```

Final User.cs:
```csharp
namespace Automotive.Marketplace.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? PersonalIdCode { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Listing> LikedListings { get; set; } = [];

    public virtual ICollection<Listing> Listings { get; set; } = [];

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = [];
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build Automotive.Marketplace.Domain/Automotive.Marketplace.Domain.csproj --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Domain/
git commit -m "feat(domain): extend Message, MessageType, and User for contract feature"
```

---

### Task 3: EF Core Configuration and DbContext

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractCardConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractSellerSubmissionConfiguration.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Configuration/ContractBuyerSubmissionConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs`
- Modify: `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`

- [ ] **Step 1: Create ContractCardConfiguration**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/ContractCardConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractCardConfiguration : IEntityTypeConfiguration<ContractCard>
{
    public void Configure(EntityTypeBuilder<ContractCard> builder)
    {
        builder.HasOne(c => c.Conversation)
            .WithMany()
            .HasForeignKey(c => c.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Initiator)
            .WithMany()
            .HasForeignKey(c => c.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Message)
            .WithOne(m => m.ContractCard)
            .HasForeignKey<Message>(m => m.ContractCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SellerSubmission)
            .WithOne(s => s.ContractCard)
            .HasForeignKey<ContractSellerSubmission>(s => s.ContractCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.BuyerSubmission)
            .WithOne(b => b.ContractCard)
            .HasForeignKey<ContractBuyerSubmission>(b => b.ContractCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 2: Create ContractSellerSubmissionConfiguration**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/ContractSellerSubmissionConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractSellerSubmissionConfiguration : IEntityTypeConfiguration<ContractSellerSubmission>
{
    public void Configure(EntityTypeBuilder<ContractSellerSubmission> builder)
    {
        builder.Property(s => s.Price).HasPrecision(18, 2);
        builder.Property(s => s.Make).HasMaxLength(100);
        builder.Property(s => s.CommercialName).HasMaxLength(200);
        builder.Property(s => s.RegistrationNumber).HasMaxLength(20);
        builder.Property(s => s.PersonalIdCode).HasMaxLength(50);
        builder.Property(s => s.FullName).HasMaxLength(200);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.Country).HasMaxLength(100);
    }
}
```

- [ ] **Step 3: Create ContractBuyerSubmissionConfiguration**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Configuration/ContractBuyerSubmissionConfiguration.cs
using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractBuyerSubmissionConfiguration : IEntityTypeConfiguration<ContractBuyerSubmission>
{
    public void Configure(EntityTypeBuilder<ContractBuyerSubmission> builder)
    {
        builder.Property(b => b.PersonalIdCode).HasMaxLength(50);
        builder.Property(b => b.FullName).HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.Email).HasMaxLength(200);
        builder.Property(b => b.Address).HasMaxLength(500);
    }
}
```

- [ ] **Step 4: Add ContractCard relation to MessageConfiguration**

Open `Automotive.Marketplace.Infrastructure/Data/Configuration/MessageConfiguration.cs` and add inside `Configure`:

```csharp
        builder.HasOne(m => m.ContractCard)
            .WithOne(c => c.Message)
            .HasForeignKey<Message>(m => m.ContractCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
```

(Note: The inverse side (`ContractCard.Message`) is configured in `ContractCardConfiguration`. Only one side needs the FK — keep it on `Message`. Remove this from MessageConfiguration if it conflicts — EF resolves from both; pick one side. The FK is already declared in ContractCardConfiguration. Skip adding to MessageConfiguration — ContractCardConfiguration is the canonical side.)

Actually: In EF, for one-to-one, configure from ONE side only. Since ContractCardConfiguration already configures `HasOne(c => c.Message).WithOne(m => m.ContractCard).HasForeignKey<Message>(m => m.ContractCardId)`, no change to MessageConfiguration is needed. Skip this step.

- [ ] **Step 5: Add DbSets to AutomotiveContext**

In `Automotive.Marketplace.Infrastructure/Data/DatabaseContext/AutomotiveContext.cs`, add after `public DbSet<AvailabilitySlots> AvailabilitySlots`:

```csharp
    public DbSet<ContractCard> ContractCards { get; set; }

    public DbSet<ContractSellerSubmission> ContractSellerSubmissions { get; set; }

    public DbSet<ContractBuyerSubmission> ContractBuyerSubmissions { get; set; }
```

- [ ] **Step 6: Build Infrastructure**

```bash
dotnet build Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj --no-restore -q
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/
git commit -m "feat(infra): add EF Core configurations and DbSets for contract entities"
```

---

### Task 4: EF Migration

**Files:**
- Auto-generated migration in `Automotive.Marketplace.Infrastructure/Migrations/`

- [ ] **Step 1: Add EF migration**

```bash
cd Automotive.Marketplace.Infrastructure
dotnet ef migrations add AddContractCardEntities \
  --startup-project ../Automotive.Marketplace.Server \
  --output-dir Migrations
```

Expected: Done. To undo this action, use 'dotnet ef migrations remove'

- [ ] **Step 2: Verify migration snapshot includes new tables**

```bash
grep -l "ContractCard\|ContractSellerSubmission\|ContractBuyerSubmission" Migrations/*.cs
```

Expected: Lists the new migration file and snapshot.

- [ ] **Step 3: Apply migration to local dev DB**

```bash
dotnet ef database update \
  --startup-project ../Automotive.Marketplace.Server
```

Expected: Migrations applied, done.

- [ ] **Step 4: Commit**

```bash
cd ..
git add Automotive.Marketplace.Infrastructure/Migrations/
git commit -m "feat(infra): add EF migration for contract card entities"
```

---

### Task 5: Test Builders

**Files:**
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/ContractCardBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/ContractSellerSubmissionBuilder.cs`
- Create: `Automotive.Marketplace.Infrastructure/Data/Builders/ContractBuyerSubmissionBuilder.cs`

- [ ] **Step 1: Create ContractCardBuilder**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Builders/ContractCardBuilder.cs
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractCardBuilder
{
    private readonly Faker<ContractCard> _faker;

    public ContractCardBuilder()
    {
        _faker = new Faker<ContractCard>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.ConversationId, f => f.Random.Guid())
            .RuleFor(c => c.InitiatorId, f => f.Random.Guid())
            .RuleFor(c => c.Status, ContractCardStatus.Pending)
            .RuleFor(c => c.AcceptedAt, _ => null)
            .RuleFor(c => c.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(c => c.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractCardBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(c => c.ConversationId, conversationId);
        return this;
    }

    public ContractCardBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(c => c.InitiatorId, initiatorId);
        return this;
    }

    public ContractCardBuilder WithStatus(ContractCardStatus status)
    {
        _faker.RuleFor(c => c.Status, status);
        return this;
    }

    public ContractCard Build() => _faker.Generate();
}
```

- [ ] **Step 2: Create ContractSellerSubmissionBuilder**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Builders/ContractSellerSubmissionBuilder.cs
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractSellerSubmissionBuilder
{
    private readonly Faker<ContractSellerSubmission> _faker;

    public ContractSellerSubmissionBuilder()
    {
        _faker = new Faker<ContractSellerSubmission>()
            .RuleFor(s => s.Id, f => f.Random.Guid())
            .RuleFor(s => s.ContractCardId, f => f.Random.Guid())
            .RuleFor(s => s.Make, f => f.Vehicle.Manufacturer())
            .RuleFor(s => s.CommercialName, f => f.Vehicle.Model())
            .RuleFor(s => s.RegistrationNumber, f => f.Random.AlphaNumeric(6).ToUpper())
            .RuleFor(s => s.Mileage, f => f.Random.Int(1000, 300000))
            .RuleFor(s => s.Vin, f => f.Vehicle.Vin())
            .RuleFor(s => s.TechnicalInspectionValid, true)
            .RuleFor(s => s.WasDamaged, false)
            .RuleFor(s => s.DamageKnown, _ => null)
            .RuleFor(s => s.DefectBrakes, false)
            .RuleFor(s => s.DefectSafety, false)
            .RuleFor(s => s.DefectSteering, false)
            .RuleFor(s => s.DefectExhaust, false)
            .RuleFor(s => s.DefectLighting, false)
            .RuleFor(s => s.Price, f => f.Random.Decimal(2000, 80000))
            .RuleFor(s => s.PersonalIdCode, f => f.Random.AlphaNumeric(11))
            .RuleFor(s => s.FullName, f => f.Name.FullName())
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.Address, f => f.Address.FullAddress())
            .RuleFor(s => s.Country, "Lietuva")
            .RuleFor(s => s.SubmittedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractSellerSubmissionBuilder WithContractCard(Guid contractCardId)
    {
        _faker.RuleFor(s => s.ContractCardId, contractCardId);
        return this;
    }

    public ContractSellerSubmission Build() => _faker.Generate();
}
```

- [ ] **Step 3: Create ContractBuyerSubmissionBuilder**

```csharp
// Automotive.Marketplace.Infrastructure/Data/Builders/ContractBuyerSubmissionBuilder.cs
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractBuyerSubmissionBuilder
{
    private readonly Faker<ContractBuyerSubmission> _faker;

    public ContractBuyerSubmissionBuilder()
    {
        _faker = new Faker<ContractBuyerSubmission>()
            .RuleFor(b => b.Id, f => f.Random.Guid())
            .RuleFor(b => b.ContractCardId, f => f.Random.Guid())
            .RuleFor(b => b.PersonalIdCode, f => f.Random.AlphaNumeric(11))
            .RuleFor(b => b.FullName, f => f.Name.FullName())
            .RuleFor(b => b.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(b => b.Email, f => f.Internet.Email())
            .RuleFor(b => b.Address, f => f.Address.FullAddress())
            .RuleFor(b => b.SubmittedAt, _ => DateTime.UtcNow)
            .RuleFor(b => b.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(b => b.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractBuyerSubmissionBuilder WithContractCard(Guid contractCardId)
    {
        _faker.RuleFor(b => b.ContractCardId, contractCardId);
        return this;
    }

    public ContractBuyerSubmission Build() => _faker.Generate();
}
```

- [ ] **Step 4: Build to verify builders compile**

```bash
dotnet build Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj --no-restore -q
```

Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/Data/Builders/
git commit -m "feat(infra): add test builders for contract card entities"
```
---

### Task 6: RequestContractCommand — Handler and Tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RequestContractCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RequestContractCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class RequestContractCommandHandlerTests(
    DatabaseFixture<RequestContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RequestContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private RequestContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ValidRequest_CreatesContractCardPendingAndMessage()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.ContractCard.Status.Should().Be(ContractCardStatus.Pending);
        result.ContractCard.InitiatorId.Should().Be(buyer.Id);

        var savedCard = await context.ContractCards.FindAsync(result.ContractCard.Id);
        savedCard.Should().NotBeNull();
        savedCard!.Status.Should().Be(ContractCardStatus.Pending);

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Contract);
        savedMessage.ContractCardId.Should().Be(savedCard.Id);
    }

    [Fact]
    public async Task Handle_ActiveContractAlreadyExists_ThrowsConflict()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingCard = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(ContractCardStatus.Active)
            .Build();
        await context.AddAsync(existingCard);
        await context.SaveChangesAsync();

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_NonParticipant_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, _) = await SeedConversationAsync(context);
        var stranger = new UserBuilder().Build();
        await context.AddAsync(stranger);
        await context.SaveChangesAsync();

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = stranger.Id,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Listing listing)>
        SeedConversationAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
```

- [ ] **Step 2: Run test — verify it fails (type not found)**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RequestContractCommandHandlerTests" --no-build -q 2>&1 | head -20
```

Expected: Build error — `RequestContractCommand` not found.

- [ ] **Step 3: Create RequestContractCommand and Response**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public sealed record RequestContractCommand : IRequest<RequestContractResponse>
{
    public Guid ConversationId { get; set; }
    public Guid InitiatorId { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public sealed record RequestContractResponse
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public Guid RecipientId { get; set; }
    public ContractCardData ContractCard { get; set; } = null!;

    public sealed record ContractCardData
    {
        public Guid Id { get; set; }
        public ContractCardStatus Status { get; set; }
        public Guid InitiatorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

- [ ] **Step 4: Create RequestContractCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RequestContract/RequestContractCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public class RequestContractCommandHandler(IRepository repository)
    : IRequestHandler<RequestContractCommand, RequestContractResponse>
{
    private static readonly ContractCardStatus[] ActiveStatuses =
    [
        ContractCardStatus.Pending,
        ContractCardStatus.Active,
        ContractCardStatus.SellerSubmitted,
        ContractCardStatus.BuyerSubmitted,
    ];

    public async Task<RequestContractResponse> Handle(
        RequestContractCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isBuyer = conversation.BuyerId == request.InitiatorId;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may request a contract.");

        var hasActiveContract = await repository.AsQueryable<ContractCard>()
            .AnyAsync(c => c.ConversationId == request.ConversationId
                        && ActiveStatuses.Contains(c.Status), cancellationToken);

        if (hasActiveContract)
            throw new ConflictException("An active contract already exists in this conversation.");

        var recipientId = isBuyer ? listing.SellerId : conversation.BuyerId;
        var senderUsername = isBuyer
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        var card = new ContractCard
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Status = ContractCardStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString(),
        };

        await repository.CreateAsync(card, cancellationToken);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Contract,
            ContractCardId = card.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString(),
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RequestContractResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            ContractCard = new RequestContractResponse.ContractCardData
            {
                Id = card.Id,
                Status = card.Status,
                InitiatorId = card.InitiatorId,
                CreatedAt = card.CreatedAt,
            },
        };
    }
}
```

Note: `ConflictException` must exist in `Automotive.Marketplace.Application.Common.Exceptions`. Check if it already exists; if not, create it:

```csharp
// Automotive.Marketplace.Application/Common/Exceptions/ConflictException.cs
namespace Automotive.Marketplace.Application.Common.Exceptions;

public class ConflictException(string message) : Exception(message);
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RequestContractCommandHandlerTests" -q
```

Expected: 3 passed.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Tests/
git commit -m "feat(app): add RequestContractCommand handler and tests"
```

---

### Task 7: RespondToContractCommand — Handler and Tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToContractCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/RespondToContractCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class RespondToContractCommandHandlerTests(
    DatabaseFixture<RespondToContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private RespondToContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_RecipientAccepts_TransitionsToActive()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: buyer =>  buyer.Id);
        // buyer initiated, seller responds
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Accept,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Active);
        var savedCard = await context.ContractCards.FindAsync(card.Id);
        savedCard!.Status.Should().Be(ContractCardStatus.Active);
        savedCard.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_RecipientDeclines_TransitionsToDeclined()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Decline,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Declined);
    }

    [Fact]
    public async Task Handle_InitiatorResponds_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = buyer.Id, // buyer is initiator — cannot respond
            Action = ContractResponseAction.Accept,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CardNotPending_ThrowsValidationException()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        card.Status = ContractCardStatus.Active;
        context.Update(card);
        await context.SaveChangesAsync();

        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Accept,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ContractCardId"));
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedPendingCardAsync(AutomotiveContext context, Func<User, Guid> initiatorId)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();
        var card = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId(buyer))
            .WithStatus(ContractCardStatus.Pending)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
```

- [ ] **Step 2: Run test — verify it fails (type not found)**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RespondToContractCommandHandlerTests" --no-build -q 2>&1 | head -10
```

Expected: Build error — `RespondToContractCommand` not found.

- [ ] **Step 3: Create ContractResponseAction enum and Command/Response**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/ContractResponseAction.cs
namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public enum ContractResponseAction
{
    Accept,
    Decline,
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public sealed record RespondToContractCommand : IRequest<RespondToContractResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid ResponderId { get; set; }
    public ContractResponseAction Action { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public sealed record RespondToContractResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public Guid ResponderId { get; set; }
    public Guid InitiatorId { get; set; }
}
```

- [ ] **Step 4: Create RespondToContractCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/RespondToContract/RespondToContractCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public class RespondToContractCommandHandler(IRepository repository)
    : IRequestHandler<RespondToContractCommand, RespondToContractResponse>
{
    public async Task<RespondToContractResponse> Handle(
        RespondToContractCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (card.Status != ContractCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "This contract card has already been responded to.")
            ]);

        if (card.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException(
                "You cannot respond to your own contract request.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond.");

        card.Status = request.Action == ContractResponseAction.Accept
            ? ContractCardStatus.Active
            : ContractCardStatus.Declined;

        if (request.Action == ContractResponseAction.Accept)
            card.AcceptedAt = DateTime.UtcNow;

        await repository.UpdateAsync(card, cancellationToken);

        return new RespondToContractResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            ResponderId = request.ResponderId,
            InitiatorId = card.InitiatorId,
        };
    }
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~RespondToContractCommandHandlerTests" -q
```

Expected: 4 passed.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Tests/
git commit -m "feat(app): add RespondToContractCommand handler and tests"
```

---

### Task 8: CancelContractCommand — Handler and Tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelContractCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/CancelContractCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class CancelContractCommandHandlerTests(
    DatabaseFixture<CancelContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CancelContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private CancelContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_InitiatorCancelsPending_TransitionsToCancelled()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedCardAsync(context, ContractCardStatus.Pending, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = buyer.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Cancelled);
        var saved = await context.ContractCards.FindAsync(card.Id);
        saved!.Status.Should().Be(ContractCardStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_CancelActiveCard_ThrowsValidationException()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedCardAsync(context, ContractCardStatus.Active, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = buyer.Id,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ContractCardId"));
    }

    [Fact]
    public async Task Handle_NonInitiatorCancels_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedCardAsync(context, ContractCardStatus.Pending, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = seller.Id, // seller is not the initiator
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedCardAsync(AutomotiveContext context, ContractCardStatus status, bool initiatorIsBuyer)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();
        var card = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorIsBuyer ? buyer.Id : seller.Id)
            .WithStatus(status)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
```

- [ ] **Step 2: Run test to confirm it fails**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~CancelContractCommandHandlerTests" --no-build -q 2>&1 | head -10
```

Expected: Build error.

- [ ] **Step 3: Create CancelContractCommand and Response**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public sealed record CancelContractCommand : IRequest<CancelContractResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public sealed record CancelContractResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid RecipientId { get; set; }
}
```

- [ ] **Step 4: Create CancelContractCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/CancelContract/CancelContractCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public class CancelContractCommandHandler(IRepository repository)
    : IRequestHandler<CancelContractCommand, CancelContractResponse>
{
    public async Task<CancelContractResponse> Handle(
        CancelContractCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (card.InitiatorId != request.RequesterId)
            throw new UnauthorizedAccessException(
                "Only the initiator may cancel a contract request.");

        if (card.Status != ContractCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "Only Pending contract requests can be cancelled.")
            ]);

        var recipientId = card.InitiatorId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        card.Status = ContractCardStatus.Cancelled;
        await repository.UpdateAsync(card, cancellationToken);

        return new CancelContractResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            InitiatorId = card.InitiatorId,
            RecipientId = recipientId,
        };
    }
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~CancelContractCommandHandlerTests" -q
```

Expected: 3 passed.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Tests/
git commit -m "feat(app): add CancelContractCommand handler and tests"
```

---

### Task 9: SubmitContractSellerFormCommand — Handler and Tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractSellerFormCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractSellerFormCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class SubmitContractSellerFormCommandHandlerTests(
    DatabaseFixture<SubmitContractSellerFormCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SubmitContractSellerFormCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private SubmitContractSellerFormCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static SubmitContractSellerFormCommand BuildSellerCommand(Guid contractCardId, Guid sellerId)
        => new()
        {
            ContractCardId = contractCardId,
            SellerId = sellerId,
            Make = "Toyota",
            CommercialName = "Corolla",
            RegistrationNumber = "ABC123",
            Mileage = 50000,
            Vin = "JT2BF22K1W0037674",
            TechnicalInspectionValid = true,
            WasDamaged = false,
            DamageKnown = null,
            DefectBrakes = false,
            DefectSafety = false,
            DefectSteering = false,
            DefectExhaust = false,
            DefectLighting = false,
            Price = 12000m,
            PersonalIdCode = "38901011234",
            FullName = "Jonas Jonaitis",
            Phone = "+37060000001",
            Email = "seller@example.com",
            Address = "Gedimino pr. 1, Vilnius",
            Country = "Lietuva",
            UpdateProfile = false,
        };

    [Fact]
    public async Task Handle_SellerSubmitsOnActivCard_TransitionsToSellerSubmitted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);

        var command = BuildSellerCommand(card.Id, seller.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.SellerSubmitted);

        var savedCard = await context.ContractCards.FindAsync(card.Id);
        savedCard!.Status.Should().Be(ContractCardStatus.SellerSubmitted);

        var savedSubmission = await context.ContractSellerSubmissions
            .FirstOrDefaultAsync(s => s.ContractCardId == card.Id);
        savedSubmission.Should().NotBeNull();
        savedSubmission!.Make.Should().Be("Toyota");
    }

    [Fact]
    public async Task Handle_SellerSubmitsWhenBuyerAlreadySubmitted_TransitionsToComplete()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, _, card) = await SeedActiveCardAsync(context);

        // Seed buyer submission first
        card.Status = ContractCardStatus.BuyerSubmitted;
        var buyerSub = new ContractBuyerSubmissionBuilder()
            .WithContractCard(card.Id)
            .Build();
        context.Update(card);
        await context.AddAsync(buyerSub);
        await context.SaveChangesAsync();

        var command = BuildSellerCommand(card.Id, seller.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Complete);
    }

    [Fact]
    public async Task Handle_BuyerAttemptsSellerForm_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedActiveCardAsync(context);
        var command = BuildSellerCommand(card.Id, buyer.Id); // buyer not allowed

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CardNotActive_ThrowsValidationException()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);
        card.Status = ContractCardStatus.Pending;
        context.Update(card);
        await context.SaveChangesAsync();

        var command = BuildSellerCommand(card.Id, seller.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ContractCardId"));
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedActiveCardAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();
        var card = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(ContractCardStatus.Active)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
```

- [ ] **Step 2: Run test to confirm it fails**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~SubmitContractSellerFormCommandHandlerTests" --no-build -q 2>&1 | head -10
```

Expected: Build error.

- [ ] **Step 3: Create SubmitContractSellerFormCommand and Response**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;

public sealed record SubmitContractSellerFormCommand : IRequest<SubmitContractSellerFormResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid SellerId { get; set; }

    // Vehicle
    public string? SdkCode { get; set; }
    public string Make { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public string? RegistrationCertificate { get; set; }
    public bool TechnicalInspectionValid { get; set; }
    public bool WasDamaged { get; set; }
    public bool? DamageKnown { get; set; }
    public bool DefectBrakes { get; set; }
    public bool DefectSafety { get; set; }
    public bool DefectSteering { get; set; }
    public bool DefectExhaust { get; set; }
    public bool DefectLighting { get; set; }
    public string? DefectDetails { get; set; }
    public decimal? Price { get; set; }

    // Seller personal
    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = "Lietuva";

    public bool UpdateProfile { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;

public sealed record SubmitContractSellerFormResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public DateTime SellerSubmittedAt { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
}
```

- [ ] **Step 4: Create SubmitContractSellerFormCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractSellerForm/SubmitContractSellerFormCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;

public class SubmitContractSellerFormCommandHandler(IRepository repository)
    : IRequestHandler<SubmitContractSellerFormCommand, SubmitContractSellerFormResponse>
{
    private static readonly ContractCardStatus[] SubmittableStatuses =
    [
        ContractCardStatus.Active,
        ContractCardStatus.BuyerSubmitted,
    ];

    public async Task<SubmitContractSellerFormResponse> Handle(
        SubmitContractSellerFormCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (listing.SellerId != request.SellerId)
            throw new UnauthorizedAccessException(
                "Only the seller of this conversation may submit the seller form.");

        if (!SubmittableStatuses.Contains(card.Status))
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "The seller form can only be submitted when the contract is Active or BuyerSubmitted.")
            ]);

        var submission = new ContractSellerSubmission
        {
            Id = Guid.NewGuid(),
            ContractCardId = card.Id,
            SdkCode = request.SdkCode,
            Make = request.Make,
            CommercialName = request.CommercialName,
            RegistrationNumber = request.RegistrationNumber,
            Mileage = request.Mileage,
            Vin = request.Vin,
            RegistrationCertificate = request.RegistrationCertificate,
            TechnicalInspectionValid = request.TechnicalInspectionValid,
            WasDamaged = request.WasDamaged,
            DamageKnown = request.WasDamaged ? request.DamageKnown : null,
            DefectBrakes = request.DefectBrakes,
            DefectSafety = request.DefectSafety,
            DefectSteering = request.DefectSteering,
            DefectExhaust = request.DefectExhaust,
            DefectLighting = request.DefectLighting,
            DefectDetails = request.DefectDetails,
            Price = request.Price,
            PersonalIdCode = request.PersonalIdCode,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Country = request.Country,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.SellerId.ToString(),
        };

        await repository.CreateAsync(submission, cancellationToken);

        card.Status = card.Status == ContractCardStatus.BuyerSubmitted
            ? ContractCardStatus.Complete
            : ContractCardStatus.SellerSubmitted;

        await repository.UpdateAsync(card, cancellationToken);

        if (request.UpdateProfile)
        {
            var seller = await repository.GetByIdAsync<User>(request.SellerId, cancellationToken);
            seller.PhoneNumber = request.Phone;
            seller.PersonalIdCode = request.PersonalIdCode;
            seller.Address = request.Address;
            try { await repository.UpdateAsync(seller, cancellationToken); }
            catch { /* profile save is non-critical */ }
        }

        return new SubmitContractSellerFormResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            SellerSubmittedAt = submission.SubmittedAt,
            InitiatorId = card.InitiatorId,
            BuyerId = conversation.BuyerId,
            SellerId = listing.SellerId,
        };
    }
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~SubmitContractSellerFormCommandHandlerTests" -q
```

Expected: 4 passed.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Tests/
git commit -m "feat(app): add SubmitContractSellerFormCommand handler and tests"
```

---

### Task 10: SubmitContractBuyerFormCommand — Handler and Tests

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommandHandler.cs`
- Create: `Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractBuyerFormCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// Automotive.Marketplace.Tests/Features/HandlerTests/ChatHandlerTests/SubmitContractBuyerFormCommandHandlerTests.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class SubmitContractBuyerFormCommandHandlerTests(
    DatabaseFixture<SubmitContractBuyerFormCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SubmitContractBuyerFormCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private SubmitContractBuyerFormCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static SubmitContractBuyerFormCommand BuildBuyerCommand(Guid contractCardId, Guid buyerId)
        => new()
        {
            ContractCardId = contractCardId,
            BuyerId = buyerId,
            PersonalIdCode = "49001011234",
            FullName = "Petras Petraitis",
            Phone = "+37060000002",
            Email = "buyer@example.com",
            Address = "Laisvės al. 5, Kaunas",
            UpdateProfile = false,
        };

    [Fact]
    public async Task Handle_BuyerSubmitsOnActiveCard_TransitionsToBuyerSubmitted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedActiveCardAsync(context);

        var command = BuildBuyerCommand(card.Id, buyer.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.BuyerSubmitted);

        var savedCard = await context.ContractCards.FindAsync(card.Id);
        savedCard!.Status.Should().Be(ContractCardStatus.BuyerSubmitted);

        var savedSub = await context.ContractBuyerSubmissions
            .FirstOrDefaultAsync(b => b.ContractCardId == card.Id);
        savedSub.Should().NotBeNull();
        savedSub!.FullName.Should().Be("Petras Petraitis");
    }

    [Fact]
    public async Task Handle_BuyerSubmitsWhenSellerAlreadySubmitted_TransitionsToComplete()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, _, card) = await SeedActiveCardAsync(context);

        card.Status = ContractCardStatus.SellerSubmitted;
        var sellerSub = new ContractSellerSubmissionBuilder()
            .WithContractCard(card.Id)
            .Build();
        context.Update(card);
        await context.AddAsync(sellerSub);
        await context.SaveChangesAsync();

        var command = BuildBuyerCommand(card.Id, buyer.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Complete);
    }

    [Fact]
    public async Task Handle_SellerAttemptsBuyerForm_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);
        var command = BuildBuyerCommand(card.Id, seller.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedActiveCardAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();
        var card = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(ContractCardStatus.Active)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
```

- [ ] **Step 2: Run test to confirm it fails**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~SubmitContractBuyerFormCommandHandlerTests" --no-build -q 2>&1 | head -10
```

- [ ] **Step 3: Create SubmitContractBuyerFormCommand and Response**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public sealed record SubmitContractBuyerFormCommand : IRequest<SubmitContractBuyerFormResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid BuyerId { get; set; }

    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public bool UpdateProfile { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public sealed record SubmitContractBuyerFormResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public DateTime BuyerSubmittedAt { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
}
```

- [ ] **Step 4: Create SubmitContractBuyerFormCommandHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/SubmitContractBuyerForm/SubmitContractBuyerFormCommandHandler.cs
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public class SubmitContractBuyerFormCommandHandler(IRepository repository)
    : IRequestHandler<SubmitContractBuyerFormCommand, SubmitContractBuyerFormResponse>
{
    private static readonly ContractCardStatus[] SubmittableStatuses =
    [
        ContractCardStatus.Active,
        ContractCardStatus.SellerSubmitted,
    ];

    public async Task<SubmitContractBuyerFormResponse> Handle(
        SubmitContractBuyerFormCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (conversation.BuyerId != request.BuyerId)
            throw new UnauthorizedAccessException(
                "Only the buyer of this conversation may submit the buyer form.");

        if (!SubmittableStatuses.Contains(card.Status))
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "The buyer form can only be submitted when the contract is Active or SellerSubmitted.")
            ]);

        var submission = new ContractBuyerSubmission
        {
            Id = Guid.NewGuid(),
            ContractCardId = card.Id,
            PersonalIdCode = request.PersonalIdCode,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.BuyerId.ToString(),
        };

        await repository.CreateAsync(submission, cancellationToken);

        card.Status = card.Status == ContractCardStatus.SellerSubmitted
            ? ContractCardStatus.Complete
            : ContractCardStatus.BuyerSubmitted;

        await repository.UpdateAsync(card, cancellationToken);

        if (request.UpdateProfile)
        {
            var buyer = await repository.GetByIdAsync<User>(request.BuyerId, cancellationToken);
            buyer.PhoneNumber = request.Phone;
            buyer.PersonalIdCode = request.PersonalIdCode;
            buyer.Address = request.Address;
            try { await repository.UpdateAsync(buyer, cancellationToken); }
            catch { /* non-critical */ }
        }

        return new SubmitContractBuyerFormResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            BuyerSubmittedAt = submission.SubmittedAt,
            InitiatorId = card.InitiatorId,
            BuyerId = conversation.BuyerId,
            SellerId = listing.SellerId,
        };
    }
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~SubmitContractBuyerFormCommandHandlerTests" -q
```

Expected: 3 passed.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Tests/
git commit -m "feat(app): add SubmitContractBuyerFormCommand handler and tests"
```
---

### Task 11: GetContractCardQuery — Handler

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQueryHandler.cs`

- [ ] **Step 1: Create GetContractCardQuery**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public sealed record GetContractCardQuery : IRequest<GetContractCardResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
```

- [ ] **Step 2: Create GetContractCardResponse**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public sealed record GetContractCardResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid InitiatorId { get; set; }
    public ContractCardStatus Status { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SellerSubmittedAt { get; set; }
    public DateTime? BuyerSubmittedAt { get; set; }
}
```

- [ ] **Step 3: Create GetContractCardQueryHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/GetContractCardQueryHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public class GetContractCardQueryHandler(IRepository repository)
    : IRequestHandler<GetContractCardQuery, GetContractCardResponse>
{
    public async Task<GetContractCardResponse> Handle(
        GetContractCardQuery request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.RequesterId == conversation.BuyerId
            || request.RequesterId == listing.SellerId;

        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may view the contract card.");

        return new GetContractCardResponse
        {
            Id = card.Id,
            ConversationId = card.ConversationId,
            InitiatorId = card.InitiatorId,
            Status = card.Status,
            AcceptedAt = card.AcceptedAt,
            CreatedAt = card.CreatedAt,
            SellerSubmittedAt = card.SellerSubmission?.SubmittedAt,
            BuyerSubmittedAt = card.BuyerSubmission?.SubmittedAt,
        };
    }
}
```

- [ ] **Step 4: Build to verify**

```bash
dotnet build Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj --no-restore -q
```

Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetContractCard/
git commit -m "feat(app): add GetContractCardQuery handler"
```

---

### Task 12: GetUserContractProfile and UpdateUserContractProfile

**Files:**
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileResponse.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQueryHandler.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommand.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommandHandler.cs`

- [ ] **Step 1: Create GetUserContractProfileQuery**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public sealed record GetUserContractProfileQuery : IRequest<GetUserContractProfileResponse>
{
    public Guid UserId { get; set; }
}
```

- [ ] **Step 2: Create GetUserContractProfileResponse**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileResponse.cs
namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public sealed record GetUserContractProfileResponse
{
    public string? PhoneNumber { get; set; }
    public string? PersonalIdCode { get; set; }
    public string? Address { get; set; }
}
```

- [ ] **Step 3: Create GetUserContractProfileQueryHandler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/GetUserContractProfileQueryHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public class GetUserContractProfileQueryHandler(IRepository repository)
    : IRequestHandler<GetUserContractProfileQuery, GetUserContractProfileResponse>
{
    public async Task<GetUserContractProfileResponse> Handle(
        GetUserContractProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync<User>(request.UserId, cancellationToken);

        return new GetUserContractProfileResponse
        {
            PhoneNumber = user.PhoneNumber,
            PersonalIdCode = user.PersonalIdCode,
            Address = user.Address,
        };
    }
}
```

- [ ] **Step 4: Create UpdateUserContractProfileCommand and Handler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommand.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;

public sealed record UpdateUserContractProfileCommand : IRequest
{
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PersonalIdCode { get; set; }
    public string? Address { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/UpdateUserContractProfileCommandHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;

public class UpdateUserContractProfileCommandHandler(IRepository repository)
    : IRequestHandler<UpdateUserContractProfileCommand>
{
    public async Task Handle(
        UpdateUserContractProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync<User>(request.UserId, cancellationToken);
        user.PhoneNumber = request.PhoneNumber;
        user.PersonalIdCode = request.PersonalIdCode;
        user.Address = request.Address;
        await repository.UpdateAsync(user, cancellationToken);
    }
}
```

- [ ] **Step 5: Build to verify**

```bash
dotnet build Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj --no-restore -q
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetUserContractProfile/ \
        Automotive.Marketplace.Application/Features/ChatFeatures/UpdateUserContractProfile/
git commit -m "feat(app): add GetUserContractProfile and UpdateUserContractProfile handlers"
```

---

### Task 13: ContractPdfService — QuestPDF Integration

**Files:**
- Modify: `Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj`
- Create: `Automotive.Marketplace.Application/Interfaces/Services/IContractPdfService.cs`
- Create: `Automotive.Marketplace.Infrastructure/Services/ContractPdfService.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQuery.cs`
- Create: `Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQueryHandler.cs`

- [ ] **Step 1: Add QuestPDF NuGet package to Infrastructure**

```bash
cd Automotive.Marketplace.Infrastructure
dotnet add package QuestPDF --version 2025.4.0
cd ..
```

Expected: PackageReference added.

- [ ] **Step 2: Create IContractPdfService interface**

```csharp
// Automotive.Marketplace.Application/Interfaces/Services/IContractPdfService.cs
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IContractPdfService
{
    byte[] Generate(ContractCard card, ContractSellerSubmission seller, ContractBuyerSubmission buyer);
}
```

- [ ] **Step 3: Create ContractPdfService**

```csharp
// Automotive.Marketplace.Infrastructure/Services/ContractPdfService.cs
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Automotive.Marketplace.Infrastructure.Services;

public class ContractPdfService : IContractPdfService
{
    public byte[] Generate(
        ContractCard card,
        ContractSellerSubmission seller,
        ContractBuyerSubmission buyer)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("TRANSPORTO PRIEMONIŲ PIRKIMO–PARDAVIMO SUTARTIS")
                        .Bold().FontSize(12).AlignCenter();
                    col.Item().Text($"Sudaryta: {card.AcceptedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd")}")
                        .AlignCenter();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    // Vehicle section
                    col.Item().Text("TRANSPORTO PRIEMONĖS DUOMENYS").Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        void Row(string label, string value)
                        {
                            table.Cell().Text(label);
                            table.Cell().Text(value);
                        }

                        Row("Markė:", seller.Make);
                        Row("Komercinis pavadinimas:", seller.CommercialName);
                        Row("Valstybinis numeris:", seller.RegistrationNumber);
                        Row("Rida (km):", seller.Mileage.ToString("N0"));
                        Row("VIN kodas:", seller.Vin ?? "–");
                        Row("TP registracijos liudijimas:", seller.RegistrationCertificate ?? "–");
                        Row("SDK kodas:", seller.SdkCode ?? "–");
                        Row("Techninė apžiūra galioja:", seller.TechnicalInspectionValid ? "Taip" : "Ne");
                        Row("Buvo sugadinta:", seller.WasDamaged ? "Taip" : "Ne");
                        if (seller.WasDamaged)
                            Row("Žala žinoma:", seller.DamageKnown == true ? "Taip" : "Ne");
                        Row("Kaina (EUR):", seller.Price.HasValue ? seller.Price.Value.ToString("N2") : "–");
                    });

                    // Defects
                    var defects = new List<string>();
                    if (seller.DefectBrakes) defects.Add("Stabdžių sistema");
                    if (seller.DefectSafety) defects.Add("Saugos sistemos");
                    if (seller.DefectSteering) defects.Add("Vairo ir pakabos sistema");
                    if (seller.DefectExhaust) defects.Add("Išmetimo sistema");
                    if (seller.DefectLighting) defects.Add("Apšvietimo sistema");

                    col.Item().Text($"Defektai: {(defects.Count > 0 ? string.Join(", ", defects) : "Nėra")}");
                    if (!string.IsNullOrWhiteSpace(seller.DefectDetails))
                        col.Item().Text($"Papildoma informacija: {seller.DefectDetails}");

                    col.Item().LineHorizontal(0.5f);

                    // Seller / Buyer columns
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(s =>
                        {
                            s.Item().Text("PARDAVĖJAS").Bold();
                            s.Item().Text($"Vardas, pavardė: {seller.FullName}");
                            s.Item().Text($"Asmens / įmonės kodas: {seller.PersonalIdCode}");
                            s.Item().Text($"Adresas: {seller.Address}");
                            s.Item().Text($"Šalis: {seller.Country}");
                            s.Item().Text($"Telefonas: {seller.Phone}");
                            s.Item().Text($"El. paštas: {seller.Email}");
                            s.Item().Text($"Pateikta: {seller.SubmittedAt:yyyy-MM-dd HH:mm}");
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Column(b =>
                        {
                            b.Item().Text("PIRKĖJAS").Bold();
                            b.Item().Text($"Vardas, pavardė: {buyer.FullName}");
                            b.Item().Text($"Asmens / įmonės kodas: {buyer.PersonalIdCode}");
                            b.Item().Text($"Adresas: {buyer.Address}");
                            b.Item().Text($"Telefonas: {buyer.Phone}");
                            b.Item().Text($"El. paštas: {buyer.Email}");
                            b.Item().Text($"Pateikta: {buyer.SubmittedAt:yyyy-MM-dd HH:mm}");
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    // Signatures
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(s =>
                        {
                            s.Item().Text("Pardavėjo parašas: ________________________");
                            s.Item().Text($"Data: {seller.SubmittedAt:yyyy-MM-dd}");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Column(b =>
                        {
                            b.Item().Text("Pirkėjo parašas: ________________________");
                            b.Item().Text($"Data: {buyer.SubmittedAt:yyyy-MM-dd}");
                        });
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Puslapis ");
                        t.CurrentPageNumber();
                        t.Span(" iš ");
                        t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }
}
```

- [ ] **Step 4: Register IContractPdfService in DI**

Find the infrastructure DI registration file (typically `Automotive.Marketplace.Infrastructure/DependencyInjection.cs` or `InfrastructureServiceRegistration.cs`). Add:

```csharp
services.AddScoped<IContractPdfService, ContractPdfService>();
```

To find the right file:
```bash
grep -rl "AddScoped\|AddSingleton\|AddTransient" Automotive.Marketplace.Infrastructure/ --include="*.cs" | grep -v "obj\|bin"
```

- [ ] **Step 5: Create ExportContractPdfQuery and Handler**

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQuery.cs
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;

public sealed record ExportContractPdfQuery : IRequest<byte[]>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
```

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ExportContractPdfQueryHandler.cs
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;

public class ExportContractPdfQueryHandler(IRepository repository, IContractPdfService pdfService)
    : IRequestHandler<ExportContractPdfQuery, byte[]>
{
    public async Task<byte[]> Handle(
        ExportContractPdfQuery request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.RequesterId == conversation.BuyerId
            || request.RequesterId == listing.SellerId;

        if (!isParticipant)
            throw new UnauthorizedAccessException("Access denied.");

        if (card.Status != ContractCardStatus.Complete)
            throw new UnauthorizedAccessException(
                "PDF is only available after both parties have submitted.");

        var seller = card.SellerSubmission
            ?? throw new InvalidOperationException("Seller submission missing for complete contract.");
        var buyer = card.BuyerSubmission
            ?? throw new InvalidOperationException("Buyer submission missing for complete contract.");

        return pdfService.Generate(card, seller, buyer);
    }
}
```

- [ ] **Step 6: Build Infrastructure and Application**

```bash
dotnet build Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj --no-restore -q
dotnet build Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj --no-restore -q
```

Expected: Both succeed.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Infrastructure/ Automotive.Marketplace.Application/Features/ChatFeatures/ExportContractPdf/ \
        Automotive.Marketplace.Application/Interfaces/Services/IContractPdfService.cs
git commit -m "feat(infra): add ContractPdfService (QuestPDF) and ExportContractPdfQuery"
```

---

### Task 14: Extend GetMessages for ContractCard + SignalR Hub Methods

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs`
- Modify: `Automotive.Marketplace.Server/Hubs/ChatHub.cs`
- Modify: `Automotive.Marketplace.Server/Controllers/ChatController.cs`

- [ ] **Step 1: Add ContractCardData to GetMessagesResponse**

In `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs`, add `ContractCardData?` to the `Message` record and add the `ContractCardData` nested record:

```csharp
// Add property to Message record (after AvailabilityCard property):
        public ContractCardData? ContractCard { get; set; }

// Add new nested record (after AvailabilityCardData record):
        public sealed record ContractCardData
        {
            public Guid Id { get; set; }

            public ContractCardStatus Status { get; set; }

            public Guid InitiatorId { get; set; }

            public DateTime? AcceptedAt { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime? SellerSubmittedAt { get; set; }

            public DateTime? BuyerSubmittedAt { get; set; }
        }
```

Full updated file:

```csharp
// Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesResponse.cs
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public sealed record GetMessagesResponse
{
    public Guid ConversationId { get; set; }

    public List<Message> Messages { get; set; } = [];

    public sealed record Message
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public MessageType MessageType { get; set; }

        public OfferData? Offer { get; set; }

        public MeetingData? Meeting { get; set; }

        public AvailabilityCardData? AvailabilityCard { get; set; }

        public ContractCardData? ContractCard { get; set; }

        public sealed record OfferData
        {
            public Guid Id { get; set; }
            public decimal Amount { get; set; }
            public decimal ListingPrice { get; set; }
            public decimal PercentageOff { get; set; }
            public OfferStatus Status { get; set; }
            public DateTime ExpiresAt { get; set; }
            public Guid InitiatorId { get; set; }
            public Guid? ParentOfferId { get; set; }
        }

        public sealed record MeetingData
        {
            public Guid Id { get; set; }
            public DateTime ProposedAt { get; set; }
            public int DurationMinutes { get; set; }
            public string? LocationText { get; set; }
            public decimal? LocationLat { get; set; }
            public decimal? LocationLng { get; set; }
            public MeetingStatus Status { get; set; }
            public DateTime ExpiresAt { get; set; }
            public Guid InitiatorId { get; set; }
            public Guid? ParentMeetingId { get; set; }
        }

        public sealed record AvailabilityCardData
        {
            public Guid Id { get; set; }
            public AvailabilityCardStatus Status { get; set; }
            public DateTime ExpiresAt { get; set; }
            public Guid InitiatorId { get; set; }
            public List<SlotData> Slots { get; set; } = [];

            public sealed record SlotData
            {
                public Guid Id { get; set; }
                public DateTime StartTime { get; set; }
                public DateTime EndTime { get; set; }
            }
        }

        public sealed record ContractCardData
        {
            public Guid Id { get; set; }
            public ContractCardStatus Status { get; set; }
            public Guid InitiatorId { get; set; }
            public DateTime? AcceptedAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? SellerSubmittedAt { get; set; }
            public DateTime? BuyerSubmittedAt { get; set; }
        }
    }
}
```

- [ ] **Step 2: Map ContractCard in GetMessagesQueryHandler**

In `Automotive.Marketplace.Application/Features/ChatFeatures/GetMessages/GetMessagesQueryHandler.cs`, add the `ContractCard` mapping in the `.Select()` projection (after `AvailabilityCard = ...`):

```csharp
                ContractCard = m.ContractCard is null ? null : new GetMessagesResponse.Message.ContractCardData
                {
                    Id = m.ContractCard.Id,
                    Status = m.ContractCard.Status,
                    InitiatorId = m.ContractCard.InitiatorId,
                    AcceptedAt = m.ContractCard.AcceptedAt,
                    CreatedAt = m.ContractCard.CreatedAt,
                    SellerSubmittedAt = m.ContractCard.SellerSubmission != null
                        ? m.ContractCard.SellerSubmission.SubmittedAt
                        : null,
                    BuyerSubmittedAt = m.ContractCard.BuyerSubmission != null
                        ? m.ContractCard.BuyerSubmission.SubmittedAt
                        : null,
                },
```

- [ ] **Step 3: Add Hub methods to ChatHub.cs**

In `Automotive.Marketplace.Server/Hubs/ChatHub.cs`, add the following using statements at the top:

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;
```

Then add the following hub methods inside the `ChatHub` class:

```csharp
    public async Task RequestContract(Guid conversationId)
    {
        var result = await mediator.Send(new RequestContractCommand
        {
            ConversationId = conversationId,
            InitiatorId = UserId,
        });

        await Clients.Group($"user-{UserId}").SendAsync("ContractRequested", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("ContractRequested", result);
    }

    public async Task RespondToContract(Guid contractCardId, string action)
    {
        var result = await mediator.Send(new RespondToContractCommand
        {
            ContractCardId = contractCardId,
            ResponderId = UserId,
            Action = Enum.Parse<ContractResponseAction>(action, ignoreCase: true),
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("ContractStatusUpdated", result);
        await Clients.Group($"user-{result.ResponderId}").SendAsync("ContractStatusUpdated", result);
    }

    public async Task CancelContract(Guid contractCardId)
    {
        var result = await mediator.Send(new CancelContractCommand
        {
            ContractCardId = contractCardId,
            RequesterId = UserId,
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("ContractStatusUpdated", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("ContractStatusUpdated", result);
    }

    public async Task SubmitContractSellerForm(Guid contractCardId, SubmitContractSellerFormCommand formData)
    {
        formData.ContractCardId = contractCardId;
        formData.SellerId = UserId;
        var result = await mediator.Send(formData);

        await Clients.Group($"user-{result.BuyerId}").SendAsync("ContractStatusUpdated", result);
        await Clients.Group($"user-{result.SellerId}").SendAsync("ContractStatusUpdated", result);
    }

    public async Task SubmitContractBuyerForm(Guid contractCardId, SubmitContractBuyerFormCommand formData)
    {
        formData.ContractCardId = contractCardId;
        formData.BuyerId = UserId;
        var result = await mediator.Send(formData);

        await Clients.Group($"user-{result.BuyerId}").SendAsync("ContractStatusUpdated", result);
        await Clients.Group($"user-{result.SellerId}").SendAsync("ContractStatusUpdated", result);
    }
```

- [ ] **Step 4: Add REST endpoints to ChatController**

In `Automotive.Marketplace.Server/Controllers/ChatController.cs`, add using statements:

```csharp
using Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;
using Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;
using Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;
```

Then add the following endpoints inside the controller class:

```csharp
    [HttpGet]
    public async Task<ActionResult<GetContractCardResponse>> GetContractCard(
        [FromQuery] Guid contractCardId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetContractCardQuery { ContractCardId = contractCardId, RequesterId = UserId },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> ExportContractPdf(
        [FromQuery] Guid contractCardId,
        CancellationToken cancellationToken)
    {
        var bytes = await mediator.Send(
            new ExportContractPdfQuery { ContractCardId = contractCardId, RequesterId = UserId },
            cancellationToken);
        return File(bytes, "application/pdf", "contract.pdf");
    }

    [HttpGet]
    public async Task<ActionResult<GetUserContractProfileResponse>> GetUserContractProfile(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetUserContractProfileQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserContractProfile(
        [FromBody] UpdateUserContractProfileCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
```

- [ ] **Step 5: Build Server**

```bash
dotnet build Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj --no-restore -q
```

Expected: Build succeeded.

- [ ] **Step 6: Run full test suite to check for regressions**

```bash
dotnet test Automotive.Marketplace.sln -q
```

Expected: All existing tests pass, new handler tests pass.

- [ ] **Step 7: Commit**

```bash
git add Automotive.Marketplace.Application/ Automotive.Marketplace.Server/
git commit -m "feat(server): add hub methods, REST endpoints, and extend GetMessages for contract card"
```
---

### Task 15: Extend ConversationSummary with Listing Pre-fill Fields

**Files:**
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ConversationSummaryResponse.cs`
- Modify: `Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/GetConversationsQueryHandler.cs`
- Modify: `automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts`

- [ ] **Step 1: Add listing pre-fill fields to ConversationSummaryResponse**

In `ConversationSummaryResponse.cs`, add after `ListingPrice`:

```csharp
    public string ListingMake { get; set; } = string.Empty;

    public string ListingCommercialName { get; set; } = string.Empty;

    public string? ListingVin { get; set; }

    public int ListingMileage { get; set; }
```

- [ ] **Step 2: Populate new fields in GetConversationsQueryHandler**

In `GetConversationsQueryHandler.cs`, inside `result.Add(new ConversationSummaryResponse { ... })`, add after `ListingPrice = listing.Price,`:

```csharp
                ListingMake = variant.Model.Make.Name,
                ListingCommercialName = !string.IsNullOrEmpty(variant.Name) ? variant.Name : variant.Model.Name,
                ListingVin = listing.Vin,
                ListingMileage = listing.Mileage,
```

- [ ] **Step 3: Update frontend ConversationSummary type**

In `automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts`, add after `listingPrice`:

```ts
  listingMake: string;
  listingCommercialName: string;
  listingVin: string | null;
  listingMileage: number;
```

Final file:
```ts
export type ConversationSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingThumbnail: { url: string; altText: string } | null;
  listingPrice: number;
  listingMake: string;
  listingCommercialName: string;
  listingVin: string | null;
  listingMileage: number;
  counterpartId: string;
  counterpartUsername: string;
  lastMessage: string | null;
  lastMessageAt: string;
  unreadCount: number;
  buyerId: string;
  sellerId: string;
  buyerHasEngaged: boolean;
};
```

- [ ] **Step 4: Build backend and run existing GetConversations tests**

```bash
dotnet build Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj --no-restore -q
dotnet test Automotive.Marketplace.Tests/Automotive.Marketplace.Tests.csproj \
  --filter "FullyQualifiedName~GetConversationsQueryHandlerTests" -q
```

Expected: Build succeeded. Tests pass.

- [ ] **Step 5: Commit**

```bash
git add Automotive.Marketplace.Application/Features/ChatFeatures/GetConversations/ \
        automotive.marketplace.client/src/features/chat/types/ConversationSummary.ts
git commit -m "feat: extend ConversationSummary with listing pre-fill fields for contract form"
```

---

### Task 16: Frontend Types, Constants, and Endpoints

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/types/ContractCard.ts`
- Create: `automotive.marketplace.client/src/features/chat/types/ContractEventPayloads.ts`
- Modify: `automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts`
- Modify: `automotive.marketplace.client/src/features/chat/constants/chatHub.ts`
- Modify: `automotive.marketplace.client/src/constants/endpoints.ts`
- Modify: `automotive.marketplace.client/src/api/queryKeys/chatKeys.ts`

- [ ] **Step 1: Create ContractCard type**

```ts
// automotive.marketplace.client/src/features/chat/types/ContractCard.ts
export type ContractCardStatus =
  | "Pending"
  | "Active"
  | "SellerSubmitted"
  | "BuyerSubmitted"
  | "Complete"
  | "Declined"
  | "Cancelled";

export type ContractCard = {
  id: string;
  status: ContractCardStatus;
  initiatorId: string;
  acceptedAt: string | null;
  createdAt: string;
  sellerSubmittedAt: string | null;
  buyerSubmittedAt: string | null;
};
```

- [ ] **Step 2: Create ContractEventPayloads type**

```ts
// automotive.marketplace.client/src/features/chat/types/ContractEventPayloads.ts
import type { ContractCardStatus } from "./ContractCard";

export type ContractRequestedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  contractCard: {
    id: string;
    status: ContractCardStatus;
    initiatorId: string;
    createdAt: string;
  };
};

export type ContractStatusUpdatedPayload = {
  contractCardId: string;
  conversationId: string;
  newStatus: ContractCardStatus;
  sellerSubmittedAt?: string;
  buyerSubmittedAt?: string;
};
```

- [ ] **Step 3: Update GetMessagesResponse.ts**

Replace the file content:

```ts
// automotive.marketplace.client/src/features/chat/types/GetMessagesResponse.ts
import type { Offer } from "./Offer";
import type { Meeting } from "./Meeting";
import type { AvailabilityCard } from "./AvailabilityCard";
import type { ContractCard } from "./ContractCard";

export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};

export type Message = {
  id: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: "Text" | "Offer" | "Meeting" | "Availability" | "Contract";
  offer?: Offer;
  meeting?: Meeting;
  availabilityCard?: AvailabilityCard;
  contractCard?: ContractCard;
};
```

- [ ] **Step 4: Add contract constants to chatHub.ts**

In `automotive.marketplace.client/src/features/chat/constants/chatHub.ts`, add the following to the `HUB_METHODS` object:

```ts
  // Contract card — Client → Server
  REQUEST_CONTRACT: "RequestContract",
  RESPOND_TO_CONTRACT: "RespondToContract",
  CANCEL_CONTRACT: "CancelContract",
  SUBMIT_CONTRACT_SELLER_FORM: "SubmitContractSellerForm",
  SUBMIT_CONTRACT_BUYER_FORM: "SubmitContractBuyerForm",
  // Contract card — Server → Client
  CONTRACT_REQUESTED: "ContractRequested",
  CONTRACT_STATUS_UPDATED: "ContractStatusUpdated",
```

- [ ] **Step 5: Add contract endpoints to endpoints.ts**

In `automotive.marketplace.client/src/constants/endpoints.ts`, inside the `CHAT` object, add:

```ts
    GET_CONTRACT_CARD: "/Chat/GetContractCard",
    EXPORT_CONTRACT_PDF: "/Chat/ExportContractPdf",
    GET_USER_CONTRACT_PROFILE: "/Chat/GetUserContractProfile",
    UPDATE_USER_CONTRACT_PROFILE: "/Chat/UpdateUserContractProfile",
```

- [ ] **Step 6: Add contractCard key to chatKeys.ts**

In `automotive.marketplace.client/src/api/queryKeys/chatKeys.ts`:

```ts
export const chatKeys = {
  all: () => ["chat"] as const,
  conversations: () => [...chatKeys.all(), "conversations"] as const,
  messages: (conversationId: string) =>
    [...chatKeys.all(), "messages", conversationId] as const,
  unreadCount: () => [...chatKeys.all(), "unreadCount"] as const,
  contractCard: (contractCardId: string) =>
    [...chatKeys.all(), "contractCard", contractCardId] as const,
  userContractProfile: () => [...chatKeys.all(), "userContractProfile"] as const,
};
```

- [ ] **Step 7: Build frontend to verify types**

```bash
cd automotive.marketplace.client
npm run build 2>&1 | tail -20
cd ..
```

Expected: Build succeeds (or only pre-existing errors, none from new types).

- [ ] **Step 8: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/types/ \
        automotive.marketplace.client/src/features/chat/constants/ \
        automotive.marketplace.client/src/constants/endpoints.ts \
        automotive.marketplace.client/src/api/queryKeys/chatKeys.ts
git commit -m "feat(fe): add ContractCard types, hub constants, and API endpoints"
```

---

### Task 17: Frontend API — Options and Mutations

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/api/getContractCardOptions.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/getUserContractProfileOptions.ts`
- Create: `automotive.marketplace.client/src/features/chat/api/useUpdateUserContractProfile.ts`

- [ ] **Step 1: Create getContractCardOptions**

```ts
// automotive.marketplace.client/src/features/chat/api/getContractCardOptions.ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { ContractCard } from "../types/ContractCard";

const getContractCard = (contractCardId: string) =>
  axiosClient.get<ContractCard>(ENDPOINTS.CHAT.GET_CONTRACT_CARD, {
    params: { contractCardId },
  });

export const getContractCardOptions = (contractCardId: string) =>
  queryOptions({
    queryKey: chatKeys.contractCard(contractCardId),
    queryFn: () => getContractCard(contractCardId),
    enabled: !!contractCardId,
  });
```

- [ ] **Step 2: Create getUserContractProfileOptions**

```ts
// automotive.marketplace.client/src/features/chat/api/getUserContractProfileOptions.ts
import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";

export type UserContractProfile = {
  phoneNumber: string | null;
  personalIdCode: string | null;
  address: string | null;
};

const getUserContractProfile = () =>
  axiosClient.get<UserContractProfile>(ENDPOINTS.CHAT.GET_USER_CONTRACT_PROFILE);

export const getUserContractProfileOptions = () =>
  queryOptions({
    queryKey: chatKeys.userContractProfile(),
    queryFn: getUserContractProfile,
  });
```

- [ ] **Step 3: Create useUpdateUserContractProfile**

```ts
// automotive.marketplace.client/src/features/chat/api/useUpdateUserContractProfile.ts
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type UpdateProfilePayload = {
  phoneNumber?: string | null;
  personalIdCode?: string | null;
  address?: string | null;
};

const updateUserContractProfile = (payload: UpdateProfilePayload) =>
  axiosClient.put(ENDPOINTS.CHAT.UPDATE_USER_CONTRACT_PROFILE, payload);

export const useUpdateUserContractProfile = () =>
  useMutation({ mutationFn: updateUserContractProfile });
```

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/api/getContractCardOptions.ts \
        automotive.marketplace.client/src/features/chat/api/getUserContractProfileOptions.ts \
        automotive.marketplace.client/src/features/chat/api/useUpdateUserContractProfile.ts
git commit -m "feat(fe): add contract card API options and profile mutation"
```

---

### Task 18: Extend useChatHub with Contract Methods

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/api/useChatHub.ts`

- [ ] **Step 1: Add contract type imports**

At the top of `useChatHub.ts`, add imports:

```ts
import type {
  ContractRequestedPayload,
  ContractStatusUpdatedPayload,
} from "../types/ContractEventPayloads";
```

- [ ] **Step 2: Register ContractRequested event handler**

Inside the `useEffect` (after the last `connection.on` call before `connectionRef.current = connection`), add:

```ts
    connection.on(
      HUB_METHODS.CONTRACT_REQUESTED,
      (payload: ContractRequestedPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const newMessage: Message = {
              id: payload.messageId,
              senderId: payload.senderId,
              senderUsername: payload.senderUsername,
              content: "",
              sentAt: payload.sentAt,
              isRead: false,
              messageType: "Contract",
              contractCard: {
                id: payload.contractCard.id,
                status: payload.contractCard.status,
                initiatorId: payload.contractCard.initiatorId,
                acceptedAt: null,
                createdAt: payload.contractCard.createdAt,
                sellerSubmittedAt: null,
                buyerSubmittedAt: null,
              },
            };
            return {
              ...old,
              data: {
                ...old.data,
                messages: [...old.data.messages, newMessage],
              },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(
      HUB_METHODS.CONTRACT_STATUS_UPDATED,
      (payload: ContractStatusUpdatedPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.contractCard?.id === payload.contractCardId
                    ? {
                        ...m,
                        contractCard: {
                          ...m.contractCard,
                          status: payload.newStatus,
                          sellerSubmittedAt:
                            payload.sellerSubmittedAt ?? m.contractCard.sellerSubmittedAt,
                          buyerSubmittedAt:
                            payload.buyerSubmittedAt ?? m.contractCard.buyerSubmittedAt,
                        },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );
```

- [ ] **Step 3: Add contract invoke functions (in the return block)**

After the existing invoke functions (`sendMessage`, `sendOffer`, ..., `cancelAvailability`), add:

```ts
  const requestContract = useCallback(
    ({ conversationId }: { conversationId: string }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected)
        throw new Error("Not connected.");
      void connectionRef.current.invoke(HUB_METHODS.REQUEST_CONTRACT, conversationId);
    },
    [],
  );

  const respondToContract = useCallback(
    ({ contractCardId, action }: { contractCardId: string; action: "Accept" | "Decline" }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected)
        throw new Error("Not connected.");
      void connectionRef.current.invoke(HUB_METHODS.RESPOND_TO_CONTRACT, contractCardId, action);
    },
    [],
  );

  const cancelContract = useCallback(
    ({ contractCardId }: { contractCardId: string }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected)
        throw new Error("Not connected.");
      void connectionRef.current.invoke(HUB_METHODS.CANCEL_CONTRACT, contractCardId);
    },
    [],
  );

  const submitContractSellerForm = useCallback(
    ({ contractCardId, formData }: { contractCardId: string; formData: object }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected)
        throw new Error("Not connected.");
      void connectionRef.current.invoke(
        HUB_METHODS.SUBMIT_CONTRACT_SELLER_FORM,
        contractCardId,
        formData,
      );
    },
    [],
  );

  const submitContractBuyerForm = useCallback(
    ({ contractCardId, formData }: { contractCardId: string; formData: object }) => {
      if (connectionRef.current?.state !== signalR.HubConnectionState.Connected)
        throw new Error("Not connected.");
      void connectionRef.current.invoke(
        HUB_METHODS.SUBMIT_CONTRACT_BUYER_FORM,
        contractCardId,
        formData,
      );
    },
    [],
  );
```

- [ ] **Step 4: Add new functions to the return statement**

In the `return { ... }` at the end of `useChatHub`, add:

```ts
    requestContract,
    respondToContract,
    cancelContract,
    submitContractSellerForm,
    submitContractBuyerForm,
```

- [ ] **Step 5: Build frontend to verify**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -20 && cd ..
```

Expected: No new errors.

- [ ] **Step 6: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/api/useChatHub.ts
git commit -m "feat(fe): add contract hub methods and event handlers to useChatHub"
```

---

### Task 19: ContractCard.tsx Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/ContractCard.tsx`

- [ ] **Step 1: Create ContractCard.tsx**

```tsx
// automotive.marketplace.client/src/features/chat/components/ContractCard.tsx
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Ban, CheckCircle, Clock, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import { format } from "date-fns";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import type { ContractCard, ContractCardStatus } from "../types/ContractCard";

type ContractCardProps = {
  card: ContractCard;
  currentUserId: string;
  isSeller: boolean;
  onAccept: (cardId: string) => void;
  onDecline: (cardId: string) => void;
  onCancel: (cardId: string) => void;
  onFillOut: (cardId: string) => void;
  onViewSubmitted: (cardId: string) => void;
  onExportPdf: (cardId: string) => void;
};

type StatusConfig = {
  headerClass: string;
  borderClass: string;
  labelKey: string;
  icon: React.ElementType;
  labelClass: string;
};

const statusConfig: Record<ContractCardStatus, StatusConfig> = {
  Pending: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.pending",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  Active: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  SellerSubmitted: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  BuyerSubmitted: {
    headerClass: "bg-sky-900",
    borderClass: "border-sky-300 dark:border-sky-800",
    labelKey: "contractCard.statusLabels.active",
    icon: FileText,
    labelClass: "text-sky-200",
  },
  Complete: {
    headerClass: "bg-green-900",
    borderClass: "border-green-300 dark:border-green-800",
    labelKey: "contractCard.statusLabels.complete",
    icon: CheckCircle,
    labelClass: "text-green-200",
  },
  Declined: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "contractCard.statusLabels.declined",
    icon: Ban,
    labelClass: "text-muted",
  },
  Cancelled: {
    headerClass: "bg-muted-foreground/60",
    borderClass: "border-border",
    labelKey: "contractCard.statusLabels.cancelled",
    icon: Ban,
    labelClass: "text-muted",
  },
};

const ContractCardComponent = ({
  card,
  currentUserId,
  isSeller,
  onAccept,
  onDecline,
  onCancel,
  onFillOut,
  onViewSubmitted,
  onExportPdf,
}: ContractCardProps) => {
  const { t } = useTranslation("chat");
  const locale = useDateLocale();
  const config = statusConfig[card.status];
  const Icon = config.icon;

  const isInitiator = card.initiatorId === currentUserId;
  const isRecipient = !isInitiator;

  const sellerSubmitted = !!card.sellerSubmittedAt;
  const buyerSubmitted = !!card.buyerSubmittedAt;

  const callerHasSubmitted = isSeller ? sellerSubmitted : buyerSubmitted;
  const canFillOut =
    (card.status === "Active" ||
      (card.status === "SellerSubmitted" && !isSeller) ||
      (card.status === "BuyerSubmitted" && isSeller)) &&
    !callerHasSubmitted;

  return (
    <div
      className={`w-full max-w-[300px] overflow-hidden rounded-xl border ${config.borderClass} shadow-sm`}
    >
      {/* Header */}
      <div className={`${config.headerClass} flex items-center gap-2 px-4 py-2.5`}>
        <Icon className={`h-3.5 w-3.5 ${config.labelClass}`} />
        <span className={`text-xs font-semibold tracking-wider uppercase ${config.labelClass}`}>
          {t(config.labelKey)}
        </span>
      </div>

      {/* Body */}
      <div className="bg-card px-4 py-3 space-y-3">
        {/* Pending — recipient sees accept/decline */}
        {card.status === "Pending" && isRecipient && (
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{t("contractCard.recipientPendingMessage")}</p>
            <div className="flex gap-2">
              <Button size="sm" className="flex-1" onClick={() => onAccept(card.id)}>
                {t("contractCard.accept")}
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="flex-1"
                onClick={() => onDecline(card.id)}
              >
                {t("contractCard.decline")}
              </Button>
            </div>
          </div>
        )}

        {/* Pending — initiator waits */}
        {card.status === "Pending" && isInitiator && (
          <div className="space-y-2">
            <div className="flex items-center gap-2 text-muted-foreground">
              <Clock className="h-3.5 w-3.5" />
              <span className="text-xs">{t("contractCard.waitingForResponse")}</span>
            </div>
            <Button
              size="sm"
              variant="ghost"
              className="text-destructive hover:text-destructive h-7 text-xs w-full"
              onClick={() => onCancel(card.id)}
            >
              {t("contractCard.cancel")}
            </Button>
          </div>
        )}

        {/* Active / SellerSubmitted / BuyerSubmitted — show badges */}
        {["Active", "SellerSubmitted", "BuyerSubmitted"].includes(card.status) && (
          <div className="space-y-2">
            <div className="flex gap-2">
              <Badge
                variant={sellerSubmitted ? "default" : "secondary"}
                className={sellerSubmitted ? "bg-green-600 text-white" : "bg-yellow-600/20 text-yellow-700 dark:text-yellow-400"}
              >
                {t("contractCard.seller")}: {sellerSubmitted ? t("contractCard.submitted") : t("contractCard.pending")}
              </Badge>
              <Badge
                variant={buyerSubmitted ? "default" : "secondary"}
                className={buyerSubmitted ? "bg-green-600 text-white" : "bg-yellow-600/20 text-yellow-700 dark:text-yellow-400"}
              >
                {t("contractCard.buyer")}: {buyerSubmitted ? t("contractCard.submitted") : t("contractCard.pending")}
              </Badge>
            </div>
            {canFillOut && (
              <Button size="sm" className="w-full" onClick={() => onFillOut(card.id)}>
                {t("contractCard.fillOut")}
              </Button>
            )}
            {callerHasSubmitted && (
              <Button size="sm" variant="outline" className="w-full" onClick={() => onViewSubmitted(card.id)}>
                {t("contractCard.viewSubmittedData")} ·{" "}
                {isSeller
                  ? format(new Date(card.sellerSubmittedAt!), "MMM d", { locale })
                  : format(new Date(card.buyerSubmittedAt!), "MMM d", { locale })}
              </Button>
            )}
          </div>
        )}

        {/* Complete */}
        {card.status === "Complete" && (
          <div className="space-y-2">
            <div className="flex gap-2">
              <Badge className="bg-green-600 text-white">
                {t("contractCard.seller")}: {t("contractCard.submitted")}
              </Badge>
              <Badge className="bg-green-600 text-white">
                {t("contractCard.buyer")}: {t("contractCard.submitted")}
              </Badge>
            </div>
            <Button size="sm" className="w-full" onClick={() => onExportPdf(card.id)}>
              <FileText className="mr-2 h-4 w-4" />
              {t("contractCard.exportPdf")}
            </Button>
          </div>
        )}

        {/* Declined / Cancelled */}
        {(card.status === "Declined" || card.status === "Cancelled") && (
          <p className="text-xs text-muted-foreground">
            {card.status === "Declined"
              ? t("contractCard.declinedMessage")
              : t("contractCard.cancelledMessage")}
          </p>
        )}
      </div>
    </div>
  );
};

export default ContractCardComponent;
```

- [ ] **Step 2: Build to verify no TypeScript errors**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | grep "error TS" | head -20 && cd ..
```

Expected: No new TS errors.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ContractCard.tsx
git commit -m "feat(fe): add ContractCard component"
```

---

### Task 20: ContractFormDialog.tsx Component

**Files:**
- Create: `automotive.marketplace.client/src/features/chat/components/ContractFormDialog.tsx`

- [ ] **Step 1: Create ContractFormDialog.tsx**

```tsx
// automotive.marketplace.client/src/features/chat/components/ContractFormDialog.tsx
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Textarea } from "@/components/ui/textarea";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { format } from "date-fns";
import { useDateLocale } from "@/lib/i18n/dateLocale";
import { useQuery } from "@tanstack/react-query";
import { getUserContractProfileOptions } from "../api/getUserContractProfileOptions";
import { useAppSelector } from "@/hooks/redux";

type VehicleDefaults = {
  make: string;
  commercialName: string;
  vin: string | null;
  mileage: number;
  price?: number | null;
};

type ContractFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  contractCardId: string;
  isSeller: boolean;
  isReadOnly?: boolean;
  submittedAt?: string | null;
  vehicleDefaults: VehicleDefaults;
  userEmail: string;
  onSubmitSeller: (
    cardId: string,
    data: SellerFormData,
    updateProfile: boolean,
  ) => void;
  onSubmitBuyer: (
    cardId: string,
    data: BuyerFormData,
    updateProfile: boolean,
  ) => void;
};

export type SellerFormData = {
  sdkCode?: string;
  make: string;
  commercialName: string;
  registrationNumber: string;
  mileage: number;
  vin?: string;
  registrationCertificate?: string;
  technicalInspectionValid: boolean;
  wasDamaged: boolean;
  damageKnown?: boolean;
  defectBrakes: boolean;
  defectSafety: boolean;
  defectSteering: boolean;
  defectExhaust: boolean;
  defectLighting: boolean;
  defectDetails?: string;
  price?: number;
  personalIdCode: string;
  fullName: string;
  phone: string;
  email: string;
  address: string;
  country: string;
};

export type BuyerFormData = {
  personalIdCode: string;
  fullName: string;
  phone: string;
  email: string;
  address: string;
};

const ContractFormDialog = ({
  open,
  onOpenChange,
  contractCardId,
  isSeller,
  isReadOnly = false,
  submittedAt,
  vehicleDefaults,
  userEmail,
  onSubmitSeller,
  onSubmitBuyer,
}: ContractFormDialogProps) => {
  const { t } = useTranslation("chat");
  const locale = useDateLocale();
  const userId = useAppSelector((s) => s.auth.userId) ?? "";

  const { data: profileData } = useQuery(getUserContractProfileOptions());
  const profile = profileData?.data;

  // Step management: seller sees steps 1 and 2, buyer starts at step 2
  const [step, setStep] = useState<1 | 2>(isSeller ? 1 : 2);
  const [updateProfile, setUpdateProfile] = useState(false);

  // Step 1 — Vehicle fields
  const [make, setMake] = useState(vehicleDefaults.make);
  const [commercialName, setCommercialName] = useState(vehicleDefaults.commercialName);
  const [vin, setVin] = useState(vehicleDefaults.vin ?? "");
  const [mileage, setMileage] = useState(String(vehicleDefaults.mileage));
  const [price, setPrice] = useState(vehicleDefaults.price ? String(vehicleDefaults.price) : "");
  const [registrationNumber, setRegistrationNumber] = useState("");
  const [sdkCode, setSdkCode] = useState("");
  const [registrationCertificate, setRegistrationCertificate] = useState("");
  const [technicalInspectionValid, setTechnicalInspectionValid] = useState(true);
  const [wasDamaged, setWasDamaged] = useState(false);
  const [damageKnown, setDamageKnown] = useState<boolean | undefined>(undefined);
  const [defectBrakes, setDefectBrakes] = useState(false);
  const [defectSafety, setDefectSafety] = useState(false);
  const [defectSteering, setDefectSteering] = useState(false);
  const [defectExhaust, setDefectExhaust] = useState(false);
  const [defectLighting, setDefectLighting] = useState(false);
  const [defectDetails, setDefectDetails] = useState("");

  // Step 2 — Personal fields
  const [personalIdCode, setPersonalIdCode] = useState(profile?.personalIdCode ?? "");
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState(profile?.phoneNumber ?? "");
  const [address, setAddress] = useState(profile?.address ?? "");
  const [country, setCountry] = useState("Lietuva");

  const disabled = isReadOnly;

  const handleSubmit = () => {
    if (isSeller) {
      onSubmitSeller(
        contractCardId,
        {
          sdkCode: sdkCode || undefined,
          make,
          commercialName,
          registrationNumber,
          mileage: Number(mileage),
          vin: vin || undefined,
          registrationCertificate: registrationCertificate || undefined,
          technicalInspectionValid,
          wasDamaged,
          damageKnown: wasDamaged ? damageKnown : undefined,
          defectBrakes,
          defectSafety,
          defectSteering,
          defectExhaust,
          defectLighting,
          defectDetails: defectDetails || undefined,
          price: price ? Number(price) : undefined,
          personalIdCode,
          fullName,
          phone,
          email: userEmail,
          address,
          country,
        },
        updateProfile,
      );
    } else {
      onSubmitBuyer(
        contractCardId,
        { personalIdCode, fullName, phone, email: userEmail, address },
        updateProfile,
      );
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-[560px]">
        <DialogHeader>
          <DialogTitle>
            {isReadOnly
              ? t("contractForm.titleReadOnly", {
                  date: submittedAt
                    ? format(new Date(submittedAt), "MMM d, yyyy", { locale })
                    : "",
                })
              : t("contractForm.title")}
          </DialogTitle>
        </DialogHeader>

        {/* Step indicator (seller only) */}
        {isSeller && !isReadOnly && (
          <div className="flex items-center gap-2 text-xs text-muted-foreground mb-2">
            <span className={step === 1 ? "font-semibold text-foreground" : ""}>
              {t("contractForm.step1")}
            </span>
            <span>→</span>
            <span className={step === 2 ? "font-semibold text-foreground" : ""}>
              {t("contractForm.step2")}
            </span>
          </div>
        )}

        {/* Step 1 — Vehicle */}
        {step === 1 && isSeller && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label className="text-xs">{t("contractForm.make")}</Label>
                <Input
                  value={make}
                  onChange={(e) => setMake(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.commercialName")}</Label>
                <Input
                  value={commercialName}
                  onChange={(e) => setCommercialName(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.vin")}</Label>
                <Input
                  value={vin}
                  onChange={(e) => setVin(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.mileage")}</Label>
                <Input
                  type="number"
                  value={mileage}
                  onChange={(e) => setMileage(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.price")}</Label>
                <Input
                  type="number"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.registrationNumber")}</Label>
                <Input
                  value={registrationNumber}
                  onChange={(e) => setRegistrationNumber(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.sdkCode")}</Label>
                <Input
                  value={sdkCode}
                  onChange={(e) => setSdkCode(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.registrationCertificate")}</Label>
                <Input
                  value={registrationCertificate}
                  onChange={(e) => setRegistrationCertificate(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
            </div>

            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Checkbox
                  id="techInspection"
                  checked={technicalInspectionValid}
                  onCheckedChange={(v) => setTechnicalInspectionValid(!!v)}
                  disabled={disabled}
                />
                <Label htmlFor="techInspection" className="text-sm">
                  {t("contractForm.technicalInspectionValid")}
                </Label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id="wasDamaged"
                  checked={wasDamaged}
                  onCheckedChange={(v) => {
                    setWasDamaged(!!v);
                    if (!v) setDamageKnown(undefined);
                  }}
                  disabled={disabled}
                />
                <Label htmlFor="wasDamaged" className="text-sm">
                  {t("contractForm.wasDamaged")}
                </Label>
              </div>
              {wasDamaged && (
                <div className="ml-6 flex items-center gap-2">
                  <Checkbox
                    id="damageKnown"
                    checked={damageKnown === true}
                    onCheckedChange={(v) => setDamageKnown(!!v)}
                    disabled={disabled}
                  />
                  <Label htmlFor="damageKnown" className="text-sm">
                    {t("contractForm.damageKnown")}
                  </Label>
                </div>
              )}
            </div>

            <div className="space-y-1">
              <Label className="text-xs">{t("contractForm.defects")}</Label>
              <div className="grid grid-cols-2 gap-1">
                {(
                  [
                    ["defectBrakes", defectBrakes, setDefectBrakes],
                    ["defectSafety", defectSafety, setDefectSafety],
                    ["defectSteering", defectSteering, setDefectSteering],
                    ["defectExhaust", defectExhaust, setDefectExhaust],
                    ["defectLighting", defectLighting, setDefectLighting],
                  ] as const
                ).map(([key, val, setter]) => (
                  <div key={key} className="flex items-center gap-2">
                    <Checkbox
                      id={key}
                      checked={val}
                      onCheckedChange={(v) => setter(!!v)}
                      disabled={disabled}
                    />
                    <Label htmlFor={key} className="text-sm">
                      {t(`contractForm.${key}`)}
                    </Label>
                  </div>
                ))}
              </div>
            </div>

            <div>
              <Label className="text-xs">{t("contractForm.defectDetails")}</Label>
              <Textarea
                value={defectDetails}
                onChange={(e) => setDefectDetails(e.target.value)}
                disabled={disabled}
                className="text-sm min-h-[60px]"
              />
            </div>

            {!isReadOnly && (
              <Button className="w-full" onClick={() => setStep(2)}>
                {t("contractForm.next")}
              </Button>
            )}
          </div>
        )}

        {/* Step 2 — Personal */}
        {step === 2 && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <Label className="text-xs">{t("contractForm.email")}</Label>
                <Input value={userEmail} disabled className="h-8 text-sm opacity-60" />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.fullName")}</Label>
                <Input
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.phone")}</Label>
                <Input
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.personalIdCode")}</Label>
                <Input
                  value={personalIdCode}
                  onChange={(e) => setPersonalIdCode(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                  placeholder={t("contractForm.personalIdCodePlaceholder")}
                />
              </div>
              <div>
                <Label className="text-xs">{t("contractForm.country")}</Label>
                <Input
                  value={country}
                  onChange={(e) => setCountry(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
              <div className="col-span-2">
                <Label className="text-xs">{t("contractForm.address")}</Label>
                <Input
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  disabled={disabled}
                  className="h-8 text-sm"
                />
              </div>
            </div>

            {!isReadOnly && (
              <>
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="updateProfile"
                    checked={updateProfile}
                    onCheckedChange={(v) => setUpdateProfile(!!v)}
                  />
                  <Label htmlFor="updateProfile" className="text-sm">
                    {t("contractForm.rememberForNextTime")}
                  </Label>
                </div>

                <div className="flex gap-2">
                  {isSeller && (
                    <Button variant="outline" className="flex-1" onClick={() => setStep(1)}>
                      {t("contractForm.back")}
                    </Button>
                  )}
                  <Button className="flex-1" onClick={handleSubmit}>
                    {t("contractForm.submit")}
                  </Button>
                </div>
              </>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default ContractFormDialog;
```

- [ ] **Step 2: Build to verify**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | grep "error TS" | head -20 && cd ..
```

Expected: No new TS errors.

- [ ] **Step 3: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/ContractFormDialog.tsx
git commit -m "feat(fe): add ContractFormDialog 2-step wizard component"
```

---

### Task 21: Wire Up MessageThread and ActionBar

**Files:**
- Modify: `automotive.marketplace.client/src/features/chat/components/MessageThread.tsx`
- Modify: `automotive.marketplace.client/src/features/chat/components/ActionBar.tsx`

- [ ] **Step 1: Update ActionBar to add contract menu item**

In `ActionBar.tsx`:

1. Add import: `import { FileText } from "lucide-react";` (already has `Plus`, add `FileText`)
2. Add `hasActiveContract` prop to `ActionBarProps`:

```ts
  hasActiveContract: boolean;
  onRequestContract: () => void;
```

3. In the JSX popover content, add the 4th button after "Share availability":

```tsx
          <button
            className="hover:bg-muted flex w-full items-center rounded-md px-3 py-2 text-left text-sm disabled:cursor-not-allowed disabled:opacity-50"
            disabled={hasActiveContract}
            title={
              hasActiveContract ? t("actionBar.contractAlreadyActive") : undefined
            }
            onClick={() => {
              setActionsPopoverOpen(false);
              onRequestContract();
            }}
          >
            <FileText className="mr-2 h-4 w-4" />
            {t("actionBar.requestContract")}
          </button>
```

- [ ] **Step 2: Update MessageThread to render ContractCard messages**

In `MessageThread.tsx`:

1. Add imports:

```tsx
import ContractCardComponent from "./ContractCard";
import ContractFormDialog from "./ContractFormDialog";
```

2. Add `hasActiveContract` computation (after `hasActiveMeeting`):

```tsx
  const hasActiveContract = messages.some(
    (m) =>
      m.messageType === "Contract" &&
      m.contractCard &&
      ["Pending", "Active", "SellerSubmitted", "BuyerSubmitted"].includes(
        m.contractCard.status,
      ),
  );
```

3. Add state for the form dialog:

```tsx
  const [contractFormOpen, setContractFormOpen] = useState(false);
  const [contractFormCardId, setContractFormCardId] = useState<string>("");
  const [contractFormReadOnly, setContractFormReadOnly] = useState(false);
  const [contractFormSubmittedAt, setContractFormSubmittedAt] = useState<string | null>(null);
```

4. Destructure new hub methods:

```tsx
  const {
    // ... existing ...
    requestContract,
    respondToContract,
    cancelContract,
    submitContractSellerForm,
    submitContractBuyerForm,
  } = useChatHub();
```

5. Add PDF export handler:

```tsx
  const handleExportPdf = (cardId: string) => {
    const url = `${import.meta.env.VITE_APP_API_URL || ""}/api/Chat/ExportContractPdf?contractCardId=${cardId}`;
    window.open(url, "_blank");
  };
```

6. Inside the messages map, add the Contract case before the plain text fallback:

```tsx
            if (m.messageType === "Contract" && m.contractCard) {
              const isOwn = m.senderId === userId;
              const isSeller = userId === conversation.sellerId;
              return (
                <div
                  key={m.id}
                  className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                >
                  <ContractCardComponent
                    card={m.contractCard}
                    currentUserId={userId}
                    isSeller={isSeller}
                    onAccept={(cardId) =>
                      respondToContract({ contractCardId: cardId, action: "Accept" })
                    }
                    onDecline={(cardId) =>
                      respondToContract({ contractCardId: cardId, action: "Decline" })
                    }
                    onCancel={(cardId) =>
                      cancelContract({ contractCardId: cardId })
                    }
                    onFillOut={(cardId) => {
                      setContractFormCardId(cardId);
                      setContractFormReadOnly(false);
                      setContractFormSubmittedAt(null);
                      setContractFormOpen(true);
                    }}
                    onViewSubmitted={(cardId) => {
                      setContractFormCardId(cardId);
                      setContractFormReadOnly(true);
                      setContractFormSubmittedAt(
                        isSeller
                          ? m.contractCard!.sellerSubmittedAt
                          : m.contractCard!.buyerSubmittedAt,
                      );
                      setContractFormOpen(true);
                    }}
                    onExportPdf={handleExportPdf}
                  />
                </div>
              );
            }
```

7. Add ContractFormDialog after the message list `</div>`:

```tsx
        <ContractFormDialog
          open={contractFormOpen}
          onOpenChange={setContractFormOpen}
          contractCardId={contractFormCardId}
          isSeller={userId === conversation.sellerId}
          isReadOnly={contractFormReadOnly}
          submittedAt={contractFormSubmittedAt}
          vehicleDefaults={{
            make: conversation.listingMake,
            commercialName: conversation.listingCommercialName,
            vin: conversation.listingVin,
            mileage: conversation.listingMileage,
            price: messages.find(
              (m) => m.messageType === "Offer" && m.offer?.status === "Accepted",
            )?.offer?.amount ?? null,
          }}
          userEmail={/* from Redux store */ ""}
          onSubmitSeller={(cardId, data, updateProfile) =>
            submitContractSellerForm({ contractCardId: cardId, formData: { ...data, updateProfile } })
          }
          onSubmitBuyer={(cardId, data, updateProfile) =>
            submitContractBuyerForm({ contractCardId: cardId, formData: { ...data, updateProfile } })
          }
        />
```

Note: Replace `""` with actual user email. Add a `selectUserEmail` selector in auth Redux slice if not already present, or use `useAppSelector((s) => s.auth.email)`.

8. Add `hasActiveContract` and `onRequestContract` to `<ActionBar>`:

```tsx
          hasActiveContract={hasActiveContract}
          onRequestContract={() =>
            requestContract({ conversationId: conversation.id })
          }
```

- [ ] **Step 3: Build frontend**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | grep "error TS" | head -30 && cd ..
```

Expected: No new errors.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/features/chat/components/MessageThread.tsx \
        automotive.marketplace.client/src/features/chat/components/ActionBar.tsx
git commit -m "feat(fe): wire up ContractCard in MessageThread and ActionBar"
```

---

### Task 22: i18n Strings

**Files:**
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/en/chat.json`
- Modify: `automotive.marketplace.client/src/lib/i18n/locales/lt/chat.json`

- [ ] **Step 1: Add English strings**

In `en/chat.json`, add the following top-level keys:

```json
  "actionBar": {
    "...": "... (existing keys)",
    "contractAlreadyActive": "A contract request is already active in this conversation",
    "requestContract": "Purchase-Sale Agreement"
  },
  "contractCard": {
    "statusLabels": {
      "pending": "Contract Requested",
      "active": "Contract Agreement",
      "complete": "Contract Complete"
    },
    "declined": "Declined",
    "cancelled": "Cancelled",
    "declinedMessage": "The contract request was declined.",
    "cancelledMessage": "The contract request was cancelled.",
    "recipientPendingMessage": "A purchase-sale contract has been requested.",
    "waitingForResponse": "Waiting for response...",
    "accept": "Accept",
    "decline": "Decline",
    "cancel": "Cancel request",
    "seller": "Seller",
    "buyer": "Buyer",
    "submitted": "Submitted",
    "pending": "Pending",
    "fillOut": "Fill out",
    "viewSubmittedData": "Submitted",
    "exportPdf": "Export PDF"
  },
  "contractForm": {
    "title": "Purchase-Sale Agreement",
    "titleReadOnly": "Submitted on {{date}}",
    "step1": "Vehicle details",
    "step2": "Personal info",
    "make": "Make",
    "commercialName": "Commercial name",
    "vin": "VIN",
    "mileage": "Mileage (km)",
    "price": "Price (EUR)",
    "registrationNumber": "Registration number",
    "sdkCode": "SDK code (Transporto priemonės savininko deklaravimo kodas)",
    "registrationCertificate": "Registration certificate (series & no.)",
    "technicalInspectionValid": "Technical inspection valid",
    "wasDamaged": "Vehicle was damaged",
    "damageKnown": "Damage / repairs are known to buyer",
    "defects": "Technical defects",
    "defectBrakes": "Brake system",
    "defectSafety": "Safety systems",
    "defectSteering": "Steering & suspension",
    "defectExhaust": "Exhaust system",
    "defectLighting": "Lighting system",
    "defectDetails": "Defect / incident details",
    "email": "Email (from account)",
    "fullName": "Full name",
    "phone": "Phone",
    "personalIdCode": "Personal ID / company code",
    "personalIdCodePlaceholder": "e.g. 38901011234",
    "country": "Country",
    "address": "Address",
    "rememberForNextTime": "Remember phone, ID code, and address for next time",
    "next": "Next",
    "back": "Back",
    "submit": "Submit"
  }
```

- [ ] **Step 2: Add Lithuanian strings**

In `lt/chat.json`, add:

```json
  "actionBar": {
    "...": "... (existing keys)",
    "contractAlreadyActive": "Šiame pokalbyje jau yra aktyvi pirkimo-pardavimo sutarties užklausa",
    "requestContract": "Pirkimo–pardavimo sutartis"
  },
  "contractCard": {
    "statusLabels": {
      "pending": "Sutarties užklausa",
      "active": "Pirkimo–pardavimo sutartis",
      "complete": "Sutartis užbaigta"
    },
    "declined": "Atmesta",
    "cancelled": "Atšaukta",
    "declinedMessage": "Sutarties užklausa buvo atmesta.",
    "cancelledMessage": "Sutarties užklausa buvo atšaukta.",
    "recipientPendingMessage": "Pateikta pirkimo–pardavimo sutarties užklausa.",
    "waitingForResponse": "Laukiama atsakymo...",
    "accept": "Priimti",
    "decline": "Atmesti",
    "cancel": "Atšaukti užklausą",
    "seller": "Pardavėjas",
    "buyer": "Pirkėjas",
    "submitted": "Pateikta",
    "pending": "Laukiama",
    "fillOut": "Pildyti",
    "viewSubmittedData": "Pateikta",
    "exportPdf": "Eksportuoti PDF"
  },
  "contractForm": {
    "title": "Pirkimo–pardavimo sutartis",
    "titleReadOnly": "Pateikta {{date}}",
    "step1": "Transporto priemonės duomenys",
    "step2": "Asmeninė informacija",
    "make": "Markė",
    "commercialName": "Komercinis pavadinimas",
    "vin": "VIN kodas",
    "mileage": "Rida (km)",
    "price": "Kaina (EUR)",
    "registrationNumber": "Valstybinis numeris",
    "sdkCode": "Transporto priemonės savininko deklaravimo kodas (SDK)",
    "registrationCertificate": "Registracijos liudijimas (serija ir nr.)",
    "technicalInspectionValid": "Techninė apžiūra galiojanti",
    "wasDamaged": "Transporto priemonė buvo sugadinta",
    "damageKnown": "Žala / remontas žinomas pirkėjui",
    "defects": "Techniniai defektai",
    "defectBrakes": "Stabdžių sistema",
    "defectSafety": "Saugos sistemos",
    "defectSteering": "Vairo ir pakabos sistema",
    "defectExhaust": "Išmetimo sistema",
    "defectLighting": "Apšvietimo sistema",
    "defectDetails": "Defektų / incidentų aprašymas",
    "email": "El. paštas (iš paskyros)",
    "fullName": "Vardas, pavardė",
    "phone": "Telefonas",
    "personalIdCode": "Asmens kodas / įmonės kodas",
    "personalIdCodePlaceholder": "pvz. 38901011234",
    "country": "Šalis",
    "address": "Adresas",
    "rememberForNextTime": "Įsiminti telefoną, asmens kodą ir adresą kitam kartui",
    "next": "Toliau",
    "back": "Atgal",
    "submit": "Pateikti"
  }
```

- [ ] **Step 3: Build frontend to verify no missing key warnings**

```bash
cd automotive.marketplace.client && npm run build 2>&1 | tail -10 && cd ..
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add automotive.marketplace.client/src/lib/i18n/locales/
git commit -m "feat(fe): add i18n strings for contract card feature (en + lt)"
```

---

### Task 23: Final Build Verification and Full Test Run

**Files:** none (verification only)

- [ ] **Step 1: Run full backend test suite**

```bash
dotnet test Automotive.Marketplace.sln -q
```

Expected: All tests pass.

- [ ] **Step 2: Run frontend lint and build**

```bash
cd automotive.marketplace.client
npm run lint && npm run build
cd ..
```

Expected: No lint errors, build succeeds.

- [ ] **Step 3: Start dev server and smoke test contract flow**

```bash
docker compose --env-file .env up -d
cd automotive.marketplace.client && npm run dev &
```

Manual smoke test steps:
1. Log in as user A (buyer) and user B (seller) in two browser windows
2. Open a conversation between them
3. Click `+` → "Pirkimo–pardavimo sutartis" — verify ContractCard appears (Pending)
4. As seller: Accept → verify card transitions to Active with two "Pending" badges
5. As seller: Click "Fill out" → verify 2-step dialog opens with listing pre-fill
6. Submit seller form → verify badge turns green, card status = SellerSubmitted
7. As buyer: Click "Fill out" → verify step 2 only (personal info)
8. Submit buyer form → verify both badges green, "Export PDF" button appears
9. Click "Export PDF" → verify PDF downloads with correct data

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: complete Buyer-Seller Contract Card feature

Adds collaborative contract card workflow to the chat interface:
- New domain entities: ContractCard, ContractSellerSubmission, ContractBuyerSubmission
- ContractCardStatus enum (Pending → Active → SellerSubmitted/BuyerSubmitted → Complete)
- CQRS handlers: RequestContract, RespondToContract, CancelContract, SubmitSellerForm, SubmitBuyerForm
- GetContractCard, ExportContractPdf, GetUserContractProfile, UpdateUserContractProfile handlers
- QuestPDF-based ContractPdfService for on-demand VKTI form PDF generation
- SignalR hub methods and ContractRequested/ContractStatusUpdated events
- Frontend: ContractCard component, ContractFormDialog 2-step wizard
- ActionBar 4th menu item; MessageThread contract message rendering
- Full i18n (en + lt) with official VKTI field names in Lithuanian

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```
