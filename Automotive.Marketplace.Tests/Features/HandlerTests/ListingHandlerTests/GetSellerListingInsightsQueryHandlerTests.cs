using Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetSellerListingInsightsQueryHandlerTests(
    DatabaseFixture<GetSellerListingInsightsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetSellerListingInsightsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetSellerListingInsightsQueryHandlerTests> _fixture = fixture;
    private readonly ICardogApiClient _cardogClient = Substitute.For<ICardogApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetSellerListingInsightsQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>(), _cardogClient);

    private async Task<(Guid listingId, Guid sellerId)> SeedListingAsync(
        AutomotiveContext context,
        bool hasDescription = false, bool hasVin = false, bool hasColour = false,
        string makeName = "Volkswagen", string modelName = "Golf", int year = 2020)
    {
        var make = new MakeBuilder().With(m => m.Name, makeName).Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, modelName).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .WithMunicipality(municipality.Id)
            .WithYear(year)
            .Build();

        listing.Description = hasDescription
            ? "This is a well-maintained Volkswagen Golf with full service history and no accidents."
            : null;
        listing.Vin = hasVin ? "WVWZZZ1JZXW000001" : null;
        listing.Colour = hasColour ? "Blue" : null;

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return (listing.Id, seller.Id);
    }

    [Fact]
    public async Task Handle_SellerRequest_ReturnsInsights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, hasDescription: true, hasVin: true, hasColour: true);

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.MarketPosition.ListingPrice.Should().BeGreaterThan(0);
        result.ListingQuality.HasDescription.Should().BeTrue();
        result.ListingQuality.HasVin.Should().BeTrue();
        result.ListingQuality.QualityScore.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task Handle_IncompleteListingWithNoDescription_SuggestsDescription()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context);

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        result.ListingQuality.HasDescription.Should().BeFalse();
        result.ListingQuality.Suggestions.Should().Contain(s => s.Contains("description"));
    }

    [Fact]
    public async Task Handle_NonSellerRequest_ThrowsException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, _) = await SeedListingAsync(context);
        var wrongUserId = Guid.NewGuid();

        var act = async () => await handler.Handle(
            new GetSellerListingInsightsQuery { ListingId = listingId, UserId = wrongUserId },
            CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_NoCacheAndEnableVehicleScoringTrue_CallsCardogAndReturnsMarketData()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, makeName: "Toyota", modelName: "Corolla", year: 2021);

        await context.UserPreferences.AddAsync(new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = sellerId,
            EnableVehicleScoring = true,
        });
        await context.SaveChangesAsync();

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 20000m, TotalListings: 50));

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        await _cardogClient.Received(1).GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.MarketPosition.HasMarketData.Should().BeTrue();
        result.MarketPosition.MarketMedianPrice.Should().Be(20000m);
        result.MarketPosition.MarketListingCount.Should().Be(50);
    }

    [Fact]
    public async Task Handle_ValidCacheExists_DoesNotCallCardog()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, makeName: "Honda", modelName: "Civic", year: 2022);

        await context.UserPreferences.AddAsync(new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = sellerId,
            EnableVehicleScoring = true,
        });
        await context.VehicleMarketCaches.AddAsync(new VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Honda", Model = "Civic", Year = 2022,
            MedianPrice = 18000m, TotalListings = 40,
            IsFetchFailed = false,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.MarketPosition.HasMarketData.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EnableVehicleScoringFalse_DoesNotCallCardog()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, makeName: "Ford", modelName: "Focus", year: 2019);

        await context.UserPreferences.AddAsync(new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = sellerId,
            EnableVehicleScoring = false,
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.MarketPosition.HasMarketData.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CardogReturnsNull_StoresFailureSentinelAndMarketDataAbsent()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, makeName: "Kia", modelName: "Sportage", year: 2023);

        await context.UserPreferences.AddAsync(new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = sellerId,
            EnableVehicleScoring = true,
        });
        await context.SaveChangesAsync();

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogMarketResult?)null);

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        result.MarketPosition.HasMarketData.Should().BeFalse();

        var marketCache = await context.VehicleMarketCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "Kia" && c.Model == "Sportage" && c.Year == 2023);
        marketCache.Should().NotBeNull();
        marketCache!.IsFetchFailed.Should().BeTrue();
        marketCache.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Handle_IsFetchFailedCacheExists_DoesNotCallCardog()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listingId, sellerId) = await SeedListingAsync(context, makeName: "Hyundai", modelName: "Elantra", year: 2020);

        await context.UserPreferences.AddAsync(new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = sellerId,
            EnableVehicleScoring = true,
        });
        await context.VehicleMarketCaches.AddAsync(new VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Hyundai", Model = "Elantra", Year = 2020,
            MedianPrice = 0m, TotalListings = 0,
            IsFetchFailed = true,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetSellerListingInsightsQuery { ListingId = listingId, UserId = sellerId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.MarketPosition.HasMarketData.Should().BeFalse();
    }
}
