
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Builders;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests;

public class GetAllListingsQueryHandlerTests(
    DatabaseFixture<GetAllListingsQueryHandlerTests> fixture)
        : IClassFixture<DatabaseFixture<GetAllListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllListingsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Handle_NoListingsExist_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var makeId = matchingListings.First().CarDetails.Car.Model.MakeId;

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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var modelId = matchingListings.First().CarDetails.Car.ModelId;

        var query = new GetAllListingsQuery { ModelId = modelId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByCity_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 1;
        const int otherCount = 2;

        var matchingListings = await SeedListingsAsync(context, expectedCount);
        _ = await SeedListingsAsync(context, otherCount);

        var city = matchingListings.First().City;

        var query = new GetAllListingsQuery { City = city };

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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        DateTime inRangeCarYear = DateTime.SpecifyKind(new DateTime(2005, 1, 1), DateTimeKind.Utc);
        DateTime outOfRangeCarYear = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc);

        var matchingListings = await SeedListingsAsync(context, expectedCount, carYear: inRangeCarYear);
        _ = await SeedListingsAsync(context, otherCount, carYear: outOfRangeCarYear);

        var query = new GetAllListingsQuery { YearFrom = inRangeCarYear.Year };

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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        DateTime inRangeCarYear = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc);
        DateTime outOfRangeCarYear = DateTime.SpecifyKind(new DateTime(2005, 1, 1), DateTimeKind.Utc);

        var matchingListings = await SeedListingsAsync(context, expectedCount, carYear: inRangeCarYear);
        _ = await SeedListingsAsync(context, otherCount, carYear: outOfRangeCarYear);

        var query = new GetAllListingsQuery { YearTo = inRangeCarYear.Year };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByPriceFrom_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const decimal inRangeCarPrice = 999;
        const decimal outOfRangeCarPrice = 111;

        var matchingListings = await SeedListingsAsync(context, expectedCount, carPrice: inRangeCarPrice);
        _ = await SeedListingsAsync(context, otherCount, carPrice: outOfRangeCarPrice);

        var query = new GetAllListingsQuery { PriceFrom = Decimal.ToInt32(inRangeCarPrice) };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_FilterByPriceTo_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const decimal inRangeCarPrice = 111;
        const decimal outOfRangeCarPrice = 999;

        var matchingListings = await SeedListingsAsync(context, expectedCount, carPrice: inRangeCarPrice);
        _ = await SeedListingsAsync(context, otherCount, carPrice: outOfRangeCarPrice);

        var query = new GetAllListingsQuery { PriceTo = Decimal.ToInt32(inRangeCarPrice) };

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
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 3;
        const int otherCount = 2;
        const bool isCarUsed = true;

        var matchingListings = await SeedListingsAsync(context, expectedCount, isCarUsed: isCarUsed);
        _ = await SeedListingsAsync(context, otherCount, isCarUsed: !isCarUsed);

        var makeId = matchingListings.First().CarDetails.Car.Model.MakeId;

        var query = new GetAllListingsQuery { MakeId = makeId, IsUsed = isCarUsed };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    private async static Task<List<Listing>> SeedListingsAsync(
        AutomotiveContext context,
        int count,
        bool? isCarUsed = null,
        DateTime? carYear = null,
        decimal? carPrice = null)
    {
        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();

        var carBuilder = new CarBuilder()
            .WithModel(model.Id);

        if (carYear.HasValue)
        {
            carBuilder.WithYear(carYear.Value);
        }

        var cars = carBuilder.Build(count);

        List<CarDetails> carDetails = [];
        List<Listing> listings = [];
        foreach (var car in cars)
        {
            var carDetailsBuilder = new CarDetailsBuilder()
                .WithCar(car.Id);

            if (isCarUsed.HasValue)
            {
                carDetailsBuilder.WithUsed(isCarUsed.Value);
            }

            var singleCarDetails = carDetailsBuilder.Build();

            var listingBuilder = new ListingBuilder()
                .WithSeller(seller.Id)
                .WithCarDetails(singleCarDetails.Id);

            if (carPrice.HasValue)
            {
                listingBuilder.WithPrice(carPrice.Value);
            }

            var listing = listingBuilder.Build();

            carDetails.Add(singleCarDetails);
            listings.Add(listing);
        }

        await context.AddAsync(seller);
        await context.AddAsync(make);
        await context.AddAsync(model);
        await context.AddRangeAsync(cars);
        await context.AddRangeAsync(carDetails);
        await context.AddRangeAsync(listings);
        await context.SaveChangesAsync();

        return listings;
    }
}