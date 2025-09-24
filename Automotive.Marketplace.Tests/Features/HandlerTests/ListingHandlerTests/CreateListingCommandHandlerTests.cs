using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class CreateListingCommandHandlerTests(
    DatabaseFixture<CreateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateListingCommandHandlerTests> _fixture = fixture;

    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    private CreateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var handler = new CreateListingCommandHandler(mapper, repository, _imageStorageService);
        return handler;
    }

    [Fact]
    public async Task Handle_ValidCreateListingCommandWithImages_ShouldCreateListing()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        _imageStorageService
            .UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>())
            .Returns(new ImageUploadResult
            {
                BucketName = "test-bucket",
                ObjectKey = "test-object-key",
                FileSize = 1024
            });

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();

        var user = new UserBuilder().Build();

        await context.AddAsync(make);
        await context.AddAsync(model);
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        var mockImage = Substitute.For<IFormFile>();
        mockImage.FileName.Returns("image1.jpg");
        mockImage.ContentType.Returns("image/jpeg");

        var command = new CreateListingCommand
        {
            ModelId = model.Id,
            Price = 15000,
            Description = "A great car",
            Colour = "Blue",
            Vin = "1234567890ABCDEFG",
            Power = 110,
            EngineSize = 2000,
            Mileage = 50000,
            IsSteeringWheelRight = false,
            City = "Super cool city",
            IsUsed = true,
            Year = 2020,
            Transmission = Transmission.Automatic,
            Fuel = Fuel.Petrol,
            DoorCount = 5,
            BodyType = BodyType.Sedan,
            Drivetrain = Drivetrain.AWD,
            UserId = user.Id,
            Images = [mockImage]
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        const int expectedListingCount = 1;
        const int expectedCarCount = 1;
        const int expectedImageCount = 1;

        var listings = await context.Set<Listing>().ToListAsync();
        var cars = await context.Set<Car>().ToListAsync();

        listings.Count.Should().Be(expectedListingCount);
        cars.Count.Should().Be(expectedCarCount);

        var createdListing = listings.FirstOrDefault();

        createdListing.Should().NotBeNull();
        createdListing!.Price.Should().Be(command.Price);
        createdListing.Description.Should().Be(command.Description);
        createdListing.Colour.Should().Be(command.Colour);
        createdListing.Vin.Should().Be(command.Vin);
        createdListing.Power.Should().Be(command.Power);
        createdListing.EngineSize.Should().Be(command.EngineSize);
        createdListing.Mileage.Should().Be(command.Mileage);
        createdListing.IsSteeringWheelRight.Should().Be(command.IsSteeringWheelRight);
        createdListing.City.Should().Be(command.City);
        createdListing.IsUsed.Should().Be(command.IsUsed);
        createdListing.SellerId.Should().Be(command.UserId);
        createdListing.Images.Count.Should().Be(expectedImageCount);

        var createdCar = createdListing.Car;
        createdCar.Should().NotBeNull();
        createdCar.ModelId.Should().Be(command.ModelId);
        createdCar.Year.Year.Should().Be(command.Year);
        createdCar.Transmission.Should().Be(command.Transmission);
        createdCar.Fuel.Should().Be(command.Fuel);
        createdCar.DoorCount.Should().Be(command.DoorCount);
        createdCar.BodyType.Should().Be(command.BodyType);
        createdCar.Drivetrain.Should().Be(command.Drivetrain);

        await _imageStorageService
            .Received(1)
            .UploadImageAsync(
                mockImage,
                Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ValidCreateListingCommandWithoutImages_ShouldCreateListing()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();

        var user = new UserBuilder().Build();

        await context.AddAsync(make);
        await context.AddAsync(model);
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        var command = new CreateListingCommand
        {
            ModelId = model.Id,
            Price = 15000,
            Description = "A great car",
            Colour = "Blue",
            Vin = "1234567890ABCDEFG",
            Power = 110,
            EngineSize = 2000,
            Mileage = 50000,
            IsSteeringWheelRight = false,
            City = "Super cool city",
            IsUsed = true,
            Year = 2020,
            Transmission = Transmission.Automatic,
            Fuel = Fuel.Petrol,
            DoorCount = 5,
            BodyType = BodyType.Sedan,
            Drivetrain = Drivetrain.AWD,
            UserId = user.Id,
            Images = []
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        const int expectedCount = 1;
        var listings = await context.Set<Listing>().ToListAsync();
        var cars = await context.Set<Car>().ToListAsync();

        listings.Count.Should().Be(expectedCount);
        cars.Count.Should().Be(expectedCount);

        var createdListing = listings.FirstOrDefault();

        createdListing.Should().NotBeNull();
        createdListing!.Price.Should().Be(command.Price);
        createdListing.Description.Should().Be(command.Description);
        createdListing.Colour.Should().Be(command.Colour);
        createdListing.Vin.Should().Be(command.Vin);
        createdListing.Power.Should().Be(command.Power);
        createdListing.EngineSize.Should().Be(command.EngineSize);
        createdListing.Mileage.Should().Be(command.Mileage);
        createdListing.IsSteeringWheelRight.Should().Be(command.IsSteeringWheelRight);
        createdListing.City.Should().Be(command.City);
        createdListing.IsUsed.Should().Be(command.IsUsed);
        createdListing.SellerId.Should().Be(command.UserId);
        createdListing.Images.Should().BeNullOrEmpty();

        var createdCar = createdListing.Car;
        createdCar.Should().NotBeNull();
        createdCar.ModelId.Should().Be(command.ModelId);
        createdCar.Year.Year.Should().Be(command.Year);
        createdCar.Transmission.Should().Be(command.Transmission);
        createdCar.Fuel.Should().Be(command.Fuel);
        createdCar.DoorCount.Should().Be(command.DoorCount);
        createdCar.BodyType.Should().Be(command.BodyType);
        createdCar.Drivetrain.Should().Be(command.Drivetrain);

        await _imageStorageService
            .Received(0)
            .UploadImageAsync(
                Arg.Any<IFormFile>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ModelDoesNotExist_ShouldThrowException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = _fixture.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var user = new UserBuilder().Build();

        await context.AddAsync(user);
        await context.SaveChangesAsync();

        var command = new CreateListingCommand
        {
            ModelId = Guid.NewGuid(),
            Price = 15000,
            Description = "A great car",
            Colour = "Blue",
            Vin = "1234567890ABCDEFG",
            Power = 110,
            EngineSize = 2000,
            Mileage = 50000,
            IsSteeringWheelRight = false,
            City = "Super cool city",
            IsUsed = true,
            Year = 2020,
            Transmission = Transmission.Automatic,
            Fuel = Fuel.Petrol,
            DoorCount = 5,
            BodyType = BodyType.Sedan,
            Drivetrain = Drivetrain.AWD,
            UserId = user.Id
        };

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

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();

        await context.AddAsync(make);
        await context.AddAsync(model);
        await context.SaveChangesAsync();

        var command = new CreateListingCommand
        {
            ModelId = model.Id,
            Price = 15000,
            Description = "A great car",
            Colour = "Blue",
            Vin = "1234567890ABCDEFG",
            Power = 110,
            EngineSize = 2000,
            Mileage = 50000,
            IsSteeringWheelRight = false,
            City = "Super cool city",
            IsUsed = true,
            Year = 2020,
            Transmission = Transmission.Automatic,
            Fuel = Fuel.Petrol,
            DoorCount = 5,
            BodyType = BodyType.Sedan,
            Drivetrain = Drivetrain.AWD,
            UserId = Guid.NewGuid(),
        };

        // Act
        var handleDelegate = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await handleDelegate.Should().ThrowAsync<DbUpdateException>();
    }
}