using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetAllListingsQueryHandlerTests(
    DatabaseFixture<GetAllListingsQueryHandlerTests> fixture)
        : IClassFixture<DatabaseFixture<GetAllListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllListingsQueryHandlerTests> _fixture = fixture;

    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    private GetAllListingsQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var handler = new GetAllListingsQueryHandler(mapper, repository, _imageStorageService);
        return handler;
    }

    [Fact]
    public async Task Handle_NoListingsExist_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var query = new GetAllListingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoFilter_ShouldReturnAllListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 5;
        await SeedListingsAsync(context, 5);

        var query = new GetAllListingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByMakeId_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var makeId = matchingListings.First().Variant.Model.MakeId;

        var query = new GetAllListingsQuery { MakeId = makeId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByModelId_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var modelId = matchingListings.First().Variant.ModelId;

        var query = new GetAllListingsQuery { Models = [modelId] };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByMunicipalityId_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 1;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var municipalityId = matchingListings.First().MunicipalityId;

        var query = new GetAllListingsQuery { MunicipalityId = municipalityId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByIsUsed_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const bool isCarUsed = true;

        var matchingListings = await SeedListingsAsync(context, expectedCount, isCarUsed: isCarUsed);
        _ = await SeedListingsAsync(context, otherCount, isCarUsed: !isCarUsed);

        var query = new GetAllListingsQuery { IsUsed = isCarUsed };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByFromYear_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        DateTime inRangeCarYear = DateTime.SpecifyKind(new DateTime(2005, 1, 1), DateTimeKind.Utc);
        DateTime outOfRangeCarYear = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc);

        var matchingListings = await SeedListingsAsync(context, expectedCount, carYear: inRangeCarYear);
        _ = await SeedListingsAsync(context, otherCount, carYear: outOfRangeCarYear);

        var query = new GetAllListingsQuery { MinYear = inRangeCarYear.Year };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByToYear_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        DateTime inRangeCarYear = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc);
        DateTime outOfRangeCarYear = DateTime.SpecifyKind(new DateTime(2005, 1, 1), DateTimeKind.Utc);

        var matchingListings = await SeedListingsAsync(context, expectedCount, carYear: inRangeCarYear);
        _ = await SeedListingsAsync(context, otherCount, carYear: outOfRangeCarYear);

        var query = new GetAllListingsQuery { MaxYear = inRangeCarYear.Year };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByMinPrice_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const decimal inRangeCarPrice = 999;
        const decimal outOfRangeCarPrice = 111;

        var matchingListings = await SeedListingsAsync(context, expectedCount, carPrice: inRangeCarPrice);
        _ = await SeedListingsAsync(context, otherCount, carPrice: outOfRangeCarPrice);

        var query = new GetAllListingsQuery { MinPrice = Decimal.ToInt32(inRangeCarPrice) };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByMaxPrice_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const decimal inRangeCarPrice = 111;
        const decimal outOfRangeCarPrice = 999;

        var matchingListings = await SeedListingsAsync(context, expectedCount, carPrice: inRangeCarPrice);
        _ = await SeedListingsAsync(context, otherCount, carPrice: outOfRangeCarPrice);

        var query = new GetAllListingsQuery { MaxPrice = Decimal.ToInt32(inRangeCarPrice) };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByMakeIdAndIsUsed_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const bool isCarUsed = true;

        var matchingListings = await SeedListingsAsync(context, expectedCount, isCarUsed: isCarUsed);
        _ = await SeedListingsAsync(context, otherCount, isCarUsed: !isCarUsed);

        var makeId = matchingListings.First().Variant.Model.MakeId;

        var query = new GetAllListingsQuery { MakeId = makeId, IsUsed = isCarUsed };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_SingleListing_ShouldReturnCorrectVariantFields()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedYear = 2020;
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain);

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

        var query = new GetAllListingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var response = result.Single();
        response.VariantId.Should().Be(variant.Id);
        response.Year.Should().Be(expectedYear);
        response.MakeName.Should().Be(make.Name);
        response.ModelName.Should().Be(model.Name);
        response.FuelName.Should().Be(fuel.Name);
        response.TransmissionName.Should().Be(transmission.Name);
    }

    [Theory]
    [InlineData(Status.Removed)]
    [InlineData(Status.Bought)]
    [InlineData(Status.OnHold)]
    public async Task Handle_NonAvailableListings_ShouldNotBeReturned(Status nonAvailableStatus)
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        const int availableCount = 2;
        const int nonAvailableCount = 3;
        await SeedListingsAsync(context, availableCount);
        await SeedListingsAsync(context, nonAvailableCount, status: nonAvailableStatus);

        var query = new GetAllListingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(availableCount);
        result.Should().OnlyContain(r => r.Status == nameof(Status.Available));
    }

    [Fact]
    public async Task Handle_WithMatchingUserId_ShouldReturnIsLikedTrue()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listings = await SeedListingsAsync(context, 1);
        var listing = listings.First();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var like = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ListingId = listing.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id.ToString()
        };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var query = new GetAllListingsQuery { UserId = user.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonMatchingUserId_ShouldReturnIsLikedFalse()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listings = await SeedListingsAsync(context, 1);
        var listing = listings.First();

        var userA = new UserBuilder().Build();
        var userB = new UserBuilder().Build();
        await context.AddRangeAsync(userA, userB);

        var like = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = userA.Id,
            ListingId = listing.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userA.Id.ToString()
        };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var query = new GetAllListingsQuery { UserId = userB.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNullUserId_ShouldReturnIsLikedFalse()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listings = await SeedListingsAsync(context, 1);
        var listing = listings.First();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var like = new UserListingLike
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ListingId = listing.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id.ToString()
        };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var query = new GetAllListingsQuery { UserId = null };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().IsLiked.Should().BeFalse();
    }

    private async static Task<List<Listing>> SeedListingsAsync(
        AutomotiveContext context,
        int count,
        bool? isCarUsed = null,
        DateTime? carYear = null,
        decimal? carPrice = null,
        Status? status = null)
    {
        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();

        await context.AddRangeAsync(seller, make, model, transmission, bodyType, drivetrain, municipality);

        List<Listing> listings = [];
        for (int i = 0; i < count; i++)
        {
            var fuel = new FuelBuilder().Build();
            await context.AddAsync(fuel);

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
                .WithDrivetrain(drivetrain.Id)
                .WithMunicipality(municipality.Id);

            if (carYear.HasValue)
            {
                listingBuilder.WithYear(carYear.Value.Year);
            }

            if (isCarUsed.HasValue)
            {
                listingBuilder.WithUsed(isCarUsed.Value);
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
        }

        await context.AddRangeAsync(listings);
        await context.SaveChangesAsync();

        return listings;
    }
}