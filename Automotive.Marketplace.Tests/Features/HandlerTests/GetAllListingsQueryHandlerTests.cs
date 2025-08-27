
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
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
    public async Task Handle_NoFilter_ShouldReturnAllListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        const int expectedCount = 5;
        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder()
            .WithMake(make.Id)
            .Build();

        await context.AddAsync(seller);
        await context.AddAsync(make);
        await context.AddAsync(model);

        var cars = new CarBuilder()
            .WithModel(model.Id)
            .Build(expectedCount);

        foreach (var car in cars)
        {
            var carDetails = new CarDetailsBuilder()
            .WithCar(car.Id)
            .Build();

            var listings = new ListingBuilder()
                .WithSeller(seller.Id)
                .WithCarDetails(carDetails.Id)
                .Build();

            await context.AddAsync(car);
            await context.AddAsync(carDetails);
            await context.AddRangeAsync(listings);
        }

        await context.SaveChangesAsync();

        var query = new GetAllListingsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count().Should().Be(expectedCount);
    }

    [Fact]
    public async Task Handle_WithMakeIdFilter_ShouldReturnFilteredListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetAllListingsQueryHandler>();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.SaveChangesAsync();

        var query = new GetAllListingsQuery { MakeId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}