using Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class SearchListingsQueryHandlerTests(
    DatabaseFixture<SearchListingsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SearchListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<SearchListingsQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private SearchListingsQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new SearchListingsQueryHandler(repository, _imageStorageService);
    }

    [Fact]
    public async Task Handle_NoResults_ReturnsEmptyList()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(
            new SearchListingsQuery { Q = "nonexistentxyz123" }, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MatchesMakeName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().With(m => m.Name, "Toyota").Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "toyota" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.MakeName == "Toyota");
    }

    [Fact]
    public async Task Handle_MatchesModelName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Camry").Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "camry" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.ModelName == "Camry");
    }

    [Fact]
    public async Task Handle_MatchesYear_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .WithYear(2020)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "2020" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.Year == 2020);
    }

    [Fact]
    public async Task Handle_MatchesSellerName_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().With(u => u.Username, "johndoe").Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new SearchListingsQuery { Q = "johndoe" }, CancellationToken.None);

        result.Should().ContainSingle(r => r.SellerName == "johndoe");
    }

    [Fact]
    public async Task Handle_MatchesListingId_ReturnsListing()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = Guid.NewGuid();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .WithListing(listingId)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new SearchListingsQuery { Q = listingId.ToString() }, CancellationToken.None);

        result.Should().ContainSingle(r => r.Id == listingId);
    }

    [Fact]
    public async Task Handle_LimitRespected_ReturnsOnlyLimitCount()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().With(m => m.Name, "Honda").Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        await context.AddRangeAsync(make, model, transmission, bodyType, drivetrain);

        for (int i = 0; i < 5; i++)
        {
            var fuel = new FuelBuilder().Build();
            var variant = new VariantBuilder()
                .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
                .Build();
            var seller = new UserBuilder().Build();
            var listing = new ListingBuilder()
                .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
                .Build();
            await context.AddRangeAsync(fuel, variant, seller, listing);
        }
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new SearchListingsQuery { Q = "Honda", Limit = 3 }, CancellationToken.None);

        result.Count().Should().Be(3);
    }
}
