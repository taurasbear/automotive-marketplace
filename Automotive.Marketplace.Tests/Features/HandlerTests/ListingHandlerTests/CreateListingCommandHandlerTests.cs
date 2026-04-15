using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
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

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class CreateListingCommandHandlerTests(
    DatabaseFixture<CreateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    private CreateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new CreateListingCommandHandler(repository, mapper);
    }

    // Path 1: VariantId provided → uses existing variant directly
    [Fact]
    public async Task Handle_VariantIdProvided_ShouldUseThatVariant()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, model, fuel, transmission, bodyType, drivetrain, user) = await SeedRequiredEntitiesAsync(context);

        var existingVariant = new VariantBuilder()
            .WithModel(model.Id)
            .WithFuel(fuel.Id)
            .WithTransmission(transmission.Id)
            .WithBodyType(bodyType.Id)
            .Build();

        await context.AddAsync(existingVariant);
        await context.SaveChangesAsync();

        var command = new CreateListingCommand(
            Price: 20000,
            Mileage: 30000,
            Description: "Existing variant car",
            SellerId: user.Id,
            VariantId: existingVariant.Id,
            ModelId: model.Id,
            Year: existingVariant.Year,
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

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var listings = await context.Set<Listing>().ToListAsync();
        var variants = await context.Set<Variant>().ToListAsync();

        listings.Should().HaveCount(1);
        variants.Should().HaveCount(1, "no new Variant should have been created");

        listings.First().VariantId.Should().Be(existingVariant.Id);
        response.VariantId.Should().Be(existingVariant.Id);
    }

    // Path 2: IsCustom = true → always creates a new Variant
    [Fact]
    public async Task Handle_IsCustomTrue_ShouldCreateCustomVariant()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, model, fuel, transmission, bodyType, drivetrain, user) = await SeedRequiredEntitiesAsync(context);

        var command = new CreateListingCommand(
            Price: 25000,
            Mileage: 10000,
            Description: "Custom car",
            SellerId: user.Id,
            VariantId: null,
            ModelId: model.Id,
            Year: 2021,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            DrivetrainId: drivetrain.Id,
            IsCustom: true,
            DoorCount: 4,
            PowerKw: 150,
            EngineSizeMl: 3000,
            IsUsed: false,
            City: "Another city"
        );

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var listings = await context.Set<Listing>().ToListAsync();
        var variants = await context.Set<Variant>().ToListAsync();

        listings.Should().HaveCount(1);
        variants.Should().HaveCount(1);

        var createdVariant = variants.First();
        createdVariant.IsCustom.Should().BeTrue();

        listings.First().VariantId.Should().Be(createdVariant.Id);
        response.VariantId.Should().Be(createdVariant.Id);
    }

    // Path 3a: IsCustom = false, no existing match → creates new Variant
    [Fact]
    public async Task Handle_IsCustomFalse_NoExistingVariant_ShouldCreateNewVariant()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, model, fuel, transmission, bodyType, drivetrain, user) = await SeedRequiredEntitiesAsync(context);

        var command = new CreateListingCommand(
            Price: 15000,
            Mileage: 50000,
            Description: "A great car",
            SellerId: user.Id,
            VariantId: null,
            ModelId: model.Id,
            Year: 2020,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            DrivetrainId: drivetrain.Id,
            IsCustom: false,
            DoorCount: 5,
            PowerKw: 110,
            EngineSizeMl: 2000,
            IsUsed: true,
            City: "Super cool city"
        );

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var listings = await context.Set<Listing>().ToListAsync();
        var variants = await context.Set<Variant>().ToListAsync();

        listings.Should().HaveCount(1);
        variants.Should().HaveCount(1);

        var createdVariant = variants.First();
        createdVariant.IsCustom.Should().BeFalse();

        var createdListing = listings.First();
        createdListing.Price.Should().Be(command.Price);
        createdListing.Mileage.Should().Be(command.Mileage);
        createdListing.Description.Should().Be(command.Description);
        createdListing.SellerId.Should().Be(command.SellerId);
        createdListing.City.Should().Be(command.City);
        createdListing.IsUsed.Should().Be(command.IsUsed);
        createdListing.Status.Should().Be(Status.Available);
        createdListing.VariantId.Should().Be(createdVariant.Id);
        response.VariantId.Should().Be(createdVariant.Id);
    }

    // Path 3b: IsCustom = false, existing match found → reuses existing Variant
    [Fact]
    public async Task Handle_IsCustomFalse_ExistingVariantMatch_ShouldReuseExistingVariant()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, model, fuel, transmission, bodyType, drivetrain, user) = await SeedRequiredEntitiesAsync(context);

        var preExistingVariant = new VariantBuilder()
            .WithModel(model.Id)
            .WithYear(2019)
            .WithFuel(fuel.Id)
            .WithTransmission(transmission.Id)
            .WithBodyType(bodyType.Id)
            .With(v => v.IsCustom, false)
            .Build();

        await context.AddAsync(preExistingVariant);
        await context.SaveChangesAsync();

        var command = new CreateListingCommand(
            Price: 12000,
            Mileage: 80000,
            Description: "Same combo car",
            SellerId: user.Id,
            VariantId: null,
            ModelId: model.Id,
            Year: 2019,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            DrivetrainId: drivetrain.Id,
            IsCustom: false,
            DoorCount: 4,
            PowerKw: 90,
            EngineSizeMl: 1400,
            IsUsed: true,
            City: "Old city"
        );

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var listings = await context.Set<Listing>().ToListAsync();
        var variants = await context.Set<Variant>().ToListAsync();

        listings.Should().HaveCount(1);
        variants.Should().HaveCount(1, "existing Variant should have been reused, not a new one created");

        listings.First().VariantId.Should().Be(preExistingVariant.Id);
        response.VariantId.Should().Be(preExistingVariant.Id);
    }

    [Fact]
    public async Task Handle_ModelDoesNotExist_ShouldThrowException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = _fixture.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, _, fuel, transmission, bodyType, drivetrain, user) = await SeedRequiredEntitiesAsync(context);

        var command = new CreateListingCommand(
            Price: 15000,
            Mileage: 50000,
            Description: "A great car",
            SellerId: user.Id,
            VariantId: null,
            ModelId: Guid.NewGuid(),
            Year: 2020,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            DrivetrainId: drivetrain.Id,
            IsCustom: false,
            DoorCount: 5,
            PowerKw: 110,
            EngineSizeMl: 2000,
            IsUsed: true,
            City: "Test city"
        );

        // Act
        var handleDelegate = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await handleDelegate.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = _fixture.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var (_, model, fuel, transmission, bodyType, drivetrain, _) = await SeedRequiredEntitiesAsync(context);

        var command = new CreateListingCommand(
            Price: 15000,
            Mileage: 50000,
            Description: "A great car",
            SellerId: Guid.NewGuid(),
            VariantId: null,
            ModelId: model.Id,
            Year: 2020,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            DrivetrainId: drivetrain.Id,
            IsCustom: false,
            DoorCount: 5,
            PowerKw: 110,
            EngineSizeMl: 2000,
            IsUsed: true,
            City: "Test city"
        );

        // Act
        var handleDelegate = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await handleDelegate.Should().ThrowAsync<DbUpdateException>();
    }

    private static async Task<(Make make, Model model, Fuel fuel, Transmission transmission, BodyType bodyType, Drivetrain drivetrain, User user)>
        SeedRequiredEntitiesAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var user = new UserBuilder().Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, user);
        await context.SaveChangesAsync();

        return (make, model, fuel, transmission, bodyType, drivetrain, user);
    }
}