using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingScoreQueryHandlerTests(
    DatabaseFixture<GetListingScoreQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingScoreQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingScoreQueryHandlerTests> _fixture = fixture;
    private readonly ICardogApiClient _cardogClient = Substitute.For<ICardogApiClient>();
    private readonly IFuelEconomyApiClient _fuelEconomyClient = Substitute.For<IFuelEconomyApiClient>();
    private readonly INhtsaApiClient _nhtsaClient = Substitute.For<INhtsaApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingScoreQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        return new GetListingScoreQueryHandler(repository, _cardogClient, _fuelEconomyClient, _nhtsaClient, scopeFactory);
    }

    private async Task<Guid> SeedListingAsync(AutomotiveContext context, string makeName = "Honda", string modelName = "Accord", int year = 2020, decimal price = 15000m, int mileage = 80000)
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
            .WithYear(year).WithPrice(price).WithMileage(mileage)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_AllCardogDataAvailable_ReturnsFullScore()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, price: 15000m);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 18000m, TotalListings: 60));
        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogEfficiencyResult(LitersPer100Km: 7.5, KWhPer100Km: null));
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogReliabilityResult(RecallCount: 1, ComplaintCrashes: 0, ComplaintInjuries: 0));

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeFalse();
        result.OverallScore.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100);
        result.Value.Status.Should().Be("scored");
        result.Efficiency.Status.Should().Be("scored");
        result.Reliability.Status.Should().Be("scored");
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_CardogReturnsNull_MissingFactorsReturned()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogMarketResult?)null);
        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogEfficiencyResult?)null);
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogReliabilityResult?)null);

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_CacheHit_DoesNotCallCardogApi()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Toyota", modelName: "Camry", year: 2021);

        // Pre-seed cache entries
        await context.VehicleMarketCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            MedianPrice = 22000m, TotalListings = 45,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23),
        });
        await context.VehicleEfficiencyCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleEfficiencyCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            LitersPer100Km = 8.5, KWhPer100Km = null,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.VehicleReliabilityCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleReliabilityCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            RecallCount = 2, ComplaintCrashes = 1, ComplaintInjuries = 0,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(6),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.DidNotReceive().GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.DidNotReceive().GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.HasMissingFactors.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ListingNotFound_ThrowsNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = async () => await handler.Handle(
            new GetListingScoreQuery { ListingId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_CacheMiss_PersistsDataToCache()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "BMW", modelName: "3 Series", year: 2019);

        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogEfficiencyResult(LitersPer100Km: 6.5, KWhPer100Km: null));
        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 25000m, TotalListings: 30));
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogReliabilityResult(RecallCount: 0, ComplaintCrashes: 0, ComplaintInjuries: 0));

        await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        var efficiencyCache = await context.VehicleEfficiencyCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);
        var marketCache = await context.VehicleMarketCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);
        var reliabilityCache = await context.VehicleReliabilityCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);

        efficiencyCache.Should().NotBeNull();
        marketCache.Should().NotBeNull();
        reliabilityCache.Should().NotBeNull();
        efficiencyCache!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        marketCache!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        reliabilityCache!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ExpiredCache_CallsApiAndUpdatesCache()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Ford", modelName: "Focus", year: 2018);

        var expiredAt = DateTime.UtcNow.AddDays(-1);
        await context.VehicleEfficiencyCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleEfficiencyCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            LitersPer100Km = 9.0, KWhPer100Km = null,
            FetchedAt = expiredAt,
            ExpiresAt = expiredAt,
        });
        await context.VehicleMarketCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            MedianPrice = 10000m, TotalListings = 15,
            FetchedAt = expiredAt,
            ExpiresAt = expiredAt,
        });
        await context.VehicleReliabilityCaches.AddAsync(new Automotive.Marketplace.Domain.Entities.VehicleReliabilityCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            RecallCount = 3, ComplaintCrashes = 1, ComplaintInjuries = 2,
            FetchedAt = expiredAt,
            ExpiresAt = expiredAt,
        });
        await context.SaveChangesAsync();

        _cardogClient.GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogEfficiencyResult(LitersPer100Km: 8.0, KWhPer100Km: null));
        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 11000m, TotalListings: 20));
        _cardogClient.GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogReliabilityResult(RecallCount: 2, ComplaintCrashes: 0, ComplaintInjuries: 1));

        await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _cardogClient.Received().GetEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.Received().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.Received().GetReliabilityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());

        var updatedEfficiency = await context.VehicleEfficiencyCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "Ford" && c.Model == "Focus" && c.Year == 2018);
        updatedEfficiency!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
