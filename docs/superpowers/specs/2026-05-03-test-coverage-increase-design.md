# Test Coverage Increase — Design Spec

## Problem

Current backend test coverage is uneven:

| Layer | Current | Target |
|-------|---------|--------|
| Application | 71% | 80%+ |
| Domain | 84.1% | maintain |
| Infrastructure | 2.5% | 30%+ |
| Server | 0% | 30%+ |

The project has 219 passing tests (45 handler integration tests + 1 unit test on backend, 5 frontend tests). 28 CQRS handlers have zero test coverage. Server and Infrastructure layers have no dedicated tests — their coverage comes only from indirect execution through handler integration tests.

## Approach

Layer-by-layer, following existing patterns:

1. Application layer — integration tests for 28 untested handlers + unit tests for validators
2. Server layer — unit tests for exception filters, middleware, background services
3. Infrastructure layer — unit tests for domain services and API clients

## Test Patterns

### Integration Tests (Handlers)

Follow the existing `DatabaseFixture<T>` pattern:

```csharp
public class XxxCommandHandlerTests(
    DatabaseFixture<XxxCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<XxxCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private XxxCommandHandler CreateHandler(IServiceScope scope)
    {
        // Get real IMapper, IRepository from DI
        // Mock external services (ITokenService, IS3ImageStorageService, etc.) with NSubstitute
        return new XxxCommandHandler(...);
    }

    [Fact]
    public async Task Handle_ValidInput_ProducesExpectedResult()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        // Seed prerequisite data directly via context
        // Call handler.Handle(command, CancellationToken.None)
        // Assert response + DB state via FluentAssertions
    }
}
```

Key: real PostgreSQL (TestContainers), real EF Core, real AutoMapper, mocked external services.

### Unit Tests (Validators, Filters, Services)

Standard xUnit + FluentAssertions, no database:

```csharp
public class XxxTests
{
    [Fact]
    public void Method_Scenario_ExpectedBehavior()
    {
        // Arrange — create instance, mock dependencies if needed
        // Act — call the method
        // Assert — verify with FluentAssertions
    }
}
```

### Mocked HttpClient (API Clients)

Shared `MockHttpMessageHandler` helper for all API client tests:

```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;

    public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseContent = responseContent;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_responseContent, Encoding.UTF8, "application/json"),
        });
    }

    // Track the last request for assertions
    public HttpRequestMessage? LastRequest { get; private set; }
}
```

## Phase 1: Application Layer (71% → 80%+)

### 1.1 Handler Integration Tests (28 handlers)

#### Auth Feature (3 handlers)

| Handler | Test Cases |
|---------|-----------|
| **LoginUserCommandHandler** | Valid credentials → returns tokens; Invalid email → throws UserNotFoundException; Wrong password → throws InvalidCredentialsException |
| **LogoutUserCommandHandler** | Valid user → revokes refresh token in DB; Missing token → throws MissingRefreshTokenException |
| **RefreshTokenCommandHandler** | Valid refresh token → returns new access + refresh tokens; Expired token → throws InvalidRefreshTokenException; Revoked token → throws InvalidRefreshTokenException |

Mocked: `IPasswordHasher`, `ITokenService`

#### Listing Feature (5 handlers)

| Handler | Test Cases |
|---------|-----------|
| **DeleteListingCommandHandler** | Owner deletes own listing → soft delete; Non-owner → throws forbidden; Non-existent listing → throws not found |
| **GetListingByIdQueryHandler** | Existing listing → returns full response with images, defects; Non-existent → throws not found |
| **UpdateListingCommandHandler** | Owner updates listing → DB reflects changes; Non-owner → forbidden |
| **UpdateListingStatusCommandHandler** | Owner changes status → DB updated; Invalid transition → error |
| **ReactivateListingCommandHandler** | Expired listing → reactivated with new expiry |

Mocked: `IS3ImageStorageService`

#### Model Feature (6 handlers)

| Handler | Test Cases |
|---------|-----------|
| **CreateModelCommandHandler** | Valid model with existing make → created in DB |
| **DeleteModelCommandHandler** | Existing model → deleted; Non-existent → not found |
| **GetAllModelsQueryHandler** | Returns all models |
| **GetModelByIdQueryHandler** | Existing → returns; Non-existent → not found |
| **GetModelsByMakeIdQueryHandler** | Returns models filtered by make |
| **UpdateModelCommandHandler** | Valid update → DB reflects changes |

No external mocks needed.

#### Variant Feature (4 handlers)

| Handler | Test Cases |
|---------|-----------|
| **CreateVariantCommandHandler** | Valid variant with model → created |
| **DeleteVariantCommandHandler** | Existing → deleted; Non-existent → not found |
| **GetVariantsByModelQueryHandler** | Returns variants filtered by model |
| **UpdateVariantCommandHandler** | Valid update → DB reflects changes |

No external mocks needed.

#### Chat Feature (7 handlers)

| Handler | Test Cases |
|---------|-----------|
| **CancelOfferCommandHandler** | Pending offer by sender → cancelled; Already responded → error |
| **ExportContractPdfQueryHandler** | Valid contract → returns PDF bytes |
| **GetContractCardQueryHandler** | Active contract → returns card data |
| **GetOrCreateConversationAsSellerCommandHandler** | New conversation → created; Existing → returned |
| **GetUnreadCountQueryHandler** | Returns correct unread count per user |
| **GetUserContractProfileQueryHandler** | Existing profile → returned; No profile → empty/defaults |
| **UpdateUserContractProfileCommandHandler** | Valid data → profile updated in DB |

Mocked: `IContractPdfService`

#### Defect Feature (5 handlers)

| Handler | Test Cases |
|---------|-----------|
| **AddListingDefectCommandHandler** | Valid defect → created linked to listing |
| **AddDefectImageCommandHandler** | Valid image → linked to defect |
| **GetDefectCategoriesQueryHandler** | Returns all categories |
| **RemoveDefectImageCommandHandler** | Existing image → removed |
| **RemoveListingDefectCommandHandler** | Existing defect → removed |

Mocked: `IS3ImageStorageService`

#### Lookup Features (4 handlers)

| Handler | Test Cases |
|---------|-----------|
| **GetAllBodyTypesQueryHandler** | Returns all body types with translations |
| **GetAllDrivetrainsQueryHandler** | Returns all drivetrains |
| **GetAllFuelsQueryHandler** | Returns all fuels |
| **GetAllTransmissionsQueryHandler** | Returns all transmissions |

No mocks needed. Seed data, query, assert.

#### Dashboard Feature (1 handler)

| Handler | Test Cases |
|---------|-----------|
| **GetDashboardSummaryQueryHandler** | Returns summary stats (listing counts, user counts, etc.) |

### 1.2 Validator Unit Tests

Test directory: `Features/UnitTests/ValidatorTests/`

| Validator | Test Cases |
|-----------|-----------|
| **RegisterUserCommandValidator** | Valid input passes; Empty email fails; Short password fails; Empty username fails |
| **LoginUserCommandValidator** | Valid input passes; Empty email fails; Empty password fails |
| All other 0% validators | Valid input passes; Each rule violation produces correct error |

Pattern: instantiate validator, call `TestValidateAsync(command)`, assert `IsValid` and error messages.

### 1.3 ValidationBehavior Unit Test

Test the MediatR pipeline behavior:
- Command with validation errors → throws `RequestValidationException`
- Command without validator → passes through
- Valid command → passes through

## Phase 2: Server Layer (0% → 30%+)

### 2.1 Exception Filter Unit Tests

Test directory: `Features/UnitTests/FilterTests/`

| Filter | Test Cases |
|--------|-----------|
| **ValidationExceptionFilter** | RequestValidationException → 400 with grouped errors |
| **NotFoundExceptionFilter** | DbEntityNotFoundException → 404 with message |
| **UnauthorizedExceptionFilter** | Unauthorized exception → 401 |
| **ForbiddenExceptionFilter** | Forbidden exception → 403 |
| **AuthorizationFilter** | User with required permission → passes; User without → 403; No user claims → 401 |

Pattern: create filter, build mock `ActionExecutedContext`/`AuthorizationFilterContext`, invoke, assert `Result`.

### 2.2 GlobalExceptionHandler Unit Test

Test directory: `Features/UnitTests/MiddlewareTests/`

- Unhandled exception → 500 ProblemDetails with correct structure
- ConflictException → 409

### 2.3 Background Service Unit Tests

Test directory: `Features/UnitTests/ServiceTests/`

| Service | Test Cases |
|---------|-----------|
| **OfferExpiryService** | Expired pending offers → status set to Expired; Non-expired offers → untouched |
| **MeetingExpiryService** | Expired pending meetings → status set to Expired; Non-expired → untouched |

Mock: `IServiceScopeFactory` → `IServiceScope` → `AutomotiveContext` (or `IRepository`). Verify the correct entities are updated.

## Phase 3: Infrastructure Layer (2.5% → 30%+)

### 3.1 Domain Service Unit Tests

Test directory: `Features/UnitTests/InfrastructureTests/`

| Service | Test Cases |
|---------|-----------|
| **PasswordHasher** | Hash returns non-null; Verify with correct password → true; Verify with wrong password → false; Hash is not plaintext |
| **TokenService** | GenerateAccessToken includes user ID claim, email claim, permission claims, correct expiry; GenerateRefreshTokenEntity creates token with correct expiry and IsRevoked=false |
| **ContractPdfService** | Generate with valid contract data → returns non-empty byte array; All required fields appear in output |

### 3.2 API Client Unit Tests (Mocked HttpClient)

Test directory: `Features/UnitTests/ApiClientTests/`

Shared `MockHttpMessageHandler` in `Infrastructure/` test helpers.

| Client | Test Cases |
|--------|-----------|
| **CardogApiClient** | Valid VIN → returns decoded data; 404 → returns null/throws; Constructs correct URL |
| **OpenAiClient** | Valid prompt → returns summary text; API error → throws/handles gracefully; Constructs correct request body |
| **NhtsaApiClient** | Valid response → correctly mapped to domain model; Empty results → handles gracefully |
| **FuelEconomyApiClient** | Valid response → parsed correctly; Missing data → null/defaults |
| **VpicVehicleDataApiClient** | Valid response → mapped; Handles pagination if applicable |
| **LithuanianMunicipalityApiClient** | Valid response → list of municipalities; Empty → empty list |
| **S3ImageStorageService** | GeneratePresignedUrl → valid URL with correct bucket/key; Delete → sends correct request |

## File Organization

```
Automotive.Marketplace.Tests/
├── Infrastructure/
│   ├── DatabaseContainer.cs          (existing)
│   ├── DatabaseFixture.cs            (existing)
│   └── MockHttpMessageHandler.cs     (NEW — shared test helper)
├── Features/
│   ├── HandlerTests/
│   │   ├── AuthHandlerTests/
│   │   │   ├── RegisterUserCommandHandlerTests.cs   (existing)
│   │   │   ├── LoginUserCommandHandlerTests.cs      (NEW)
│   │   │   ├── LogoutUserCommandHandlerTests.cs     (NEW)
│   │   │   └── RefreshTokenCommandHandlerTests.cs   (NEW)
│   │   ├── ListingHandlerTests/
│   │   │   ├── ... (existing)
│   │   │   ├── DeleteListingCommandHandlerTests.cs  (NEW)
│   │   │   ├── GetListingByIdQueryHandlerTests.cs   (NEW)
│   │   │   ├── UpdateListingCommandHandlerTests.cs  (NEW)
│   │   │   ├── UpdateListingStatusCommandHandlerTests.cs (NEW)
│   │   │   └── ReactivateListingCommandHandlerTests.cs   (NEW)
│   │   ├── ModelHandlerTests/                       (NEW directory)
│   │   │   ├── CreateModelCommandHandlerTests.cs
│   │   │   ├── DeleteModelCommandHandlerTests.cs
│   │   │   ├── GetAllModelsQueryHandlerTests.cs
│   │   │   ├── GetModelByIdQueryHandlerTests.cs
│   │   │   ├── GetModelsByMakeIdQueryHandlerTests.cs
│   │   │   └── UpdateModelCommandHandlerTests.cs
│   │   ├── VariantHandlerTests/                     (NEW directory)
│   │   │   ├── CreateVariantCommandHandlerTests.cs
│   │   │   ├── DeleteVariantCommandHandlerTests.cs
│   │   │   ├── GetVariantsByModelQueryHandlerTests.cs
│   │   │   └── UpdateVariantCommandHandlerTests.cs
│   │   ├── ChatHandlerTests/
│   │   │   ├── ... (existing)
│   │   │   ├── CancelOfferCommandHandlerTests.cs    (NEW)
│   │   │   ├── ExportContractPdfQueryHandlerTests.cs (NEW)
│   │   │   ├── GetContractCardQueryHandlerTests.cs  (NEW)
│   │   │   ├── GetOrCreateConversationAsSellerCommandHandlerTests.cs (NEW)
│   │   │   ├── GetUnreadCountQueryHandlerTests.cs   (NEW)
│   │   │   ├── GetUserContractProfileQueryHandlerTests.cs (NEW)
│   │   │   └── UpdateUserContractProfileCommandHandlerTests.cs (NEW)
│   │   ├── DefectHandlerTests/                      (NEW directory)
│   │   │   ├── AddDefectImageCommandHandlerTests.cs
│   │   │   ├── AddListingDefectCommandHandlerTests.cs
│   │   │   ├── GetDefectCategoriesQueryHandlerTests.cs
│   │   │   ├── RemoveDefectImageCommandHandlerTests.cs
│   │   │   └── RemoveListingDefectCommandHandlerTests.cs
│   │   ├── LookupHandlerTests/                      (NEW directory)
│   │   │   ├── GetAllBodyTypesQueryHandlerTests.cs
│   │   │   ├── GetAllDrivetrainsQueryHandlerTests.cs
│   │   │   ├── GetAllFuelsQueryHandlerTests.cs
│   │   │   └── GetAllTransmissionsQueryHandlerTests.cs
│   │   └── DashboardHandlerTests/                   (NEW directory)
│   │       └── GetDashboardSummaryQueryHandlerTests.cs
│   └── UnitTests/
│       ├── ListingScoreCalculatorTests.cs           (existing)
│       ├── ValidatorTests/                          (NEW directory)
│       │   ├── RegisterUserCommandValidatorTests.cs
│       │   ├── LoginUserCommandValidatorTests.cs
│       │   └── ... (one per validator)
│       ├── FilterTests/                             (NEW directory)
│       │   ├── ValidationExceptionFilterTests.cs
│       │   ├── NotFoundExceptionFilterTests.cs
│       │   ├── UnauthorizedExceptionFilterTests.cs
│       │   ├── ForbiddenExceptionFilterTests.cs
│       │   └── AuthorizationFilterTests.cs
│       ├── MiddlewareTests/                         (NEW directory)
│       │   └── GlobalExceptionHandlerTests.cs
│       ├── ServiceTests/                            (NEW directory)
│       │   ├── OfferExpiryServiceTests.cs
│       │   └── MeetingExpiryServiceTests.cs
│       ├── InfrastructureTests/                     (NEW directory)
│       │   ├── PasswordHasherTests.cs
│       │   ├── TokenServiceTests.cs
│       │   └── ContractPdfServiceTests.cs
│       └── ApiClientTests/                          (NEW directory)
│           ├── CardogApiClientTests.cs
│           ├── OpenAiClientTests.cs
│           ├── NhtsaApiClientTests.cs
│           ├── FuelEconomyApiClientTests.cs
│           ├── VpicVehicleDataApiClientTests.cs
│           ├── LithuanianMunicipalityApiClientTests.cs
│           └── S3ImageStorageServiceTests.cs
```

## Estimated Test Count

| Category | New Tests (approx) |
|----------|-------------------|
| Handler integration tests | ~70-90 (2-3 per handler × 28 handlers) |
| Validator unit tests | ~20-30 |
| Filter unit tests | ~10-15 |
| Middleware unit tests | ~3-5 |
| Background service unit tests | ~6-8 |
| Infrastructure service unit tests | ~10-15 |
| API client unit tests | ~20-25 |
| **Total new tests** | **~140-190** |

Combined with existing 219 tests → **~360-410 total tests**.

## Success Criteria

- All existing 219 tests continue to pass
- Application line coverage ≥ 80%
- Server line coverage ≥ 30%
- Infrastructure line coverage ≥ 30%
- `dotnet test` passes with zero failures
- No changes to production code
