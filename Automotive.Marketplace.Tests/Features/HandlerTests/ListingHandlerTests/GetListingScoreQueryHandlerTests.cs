using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
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

public class GetListingScoreQueryHandlerTests(
    DatabaseFixture<GetListingScoreQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingScoreQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingScoreQueryHandlerTests> _fixture = fixture;
    private readonly ICardogApiClient _cardogClient = Substitute.For<ICardogApiClient>();
    private readonly INhtsaApiClient _nhtsaClient = Substitute.For<INhtsaApiClient>();
    private readonly IFuelEconomyApiClient _fuelEconomyClient = Substitute.For<IFuelEconomyApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingScoreQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        return new GetListingScoreQueryHandler(repository, _cardogClient, _nhtsaClient, _fuelEconomyClient, scopeFactory);
    }

    private async Task<Guid> SeedListingAsync(AutomotiveContext context, string makeName = "Honda",
        string modelName = "Accord", int year = 2020, decimal price = 15000m, int mileage = 80000)
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
    public async Task Handle_AllDataAvailable_ReturnsFullScore()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, price: 15000m);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 18000m, TotalListings: 60));
        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new FuelEconomyEfficiencyResult(LitersPer100Km: 7.5, KWhPer100Km: null));
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaRecallsResult(1));
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaComplaintsResult(50));
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaSafetyRatingResult(4));

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeFalse();
        result.OverallScore.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100);
        result.Value.Status.Should().Be("scored");
        result.Efficiency.Status.Should().Be("scored");
        result.Reliability.Status.Should().Be("scored");
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_AllClientsReturnNull_MissingFactorsReturned()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogMarketResult?)null);
        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((FuelEconomyEfficiencyResult?)null);
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaRecallsResult?)null);
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaComplaintsResult?)null);
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaSafetyRatingResult?)null);

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
        result.Mileage.Status.Should().Be("scored");
    }

    [Fact]
    public async Task Handle_CacheHit_DoesNotCallAnyExternalApi()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Toyota", modelName: "Camry", year: 2021);

        await context.VehicleMarketCaches.AddAsync(new VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            MedianPrice = 22000m, TotalListings = 45,
            IsFetchFailed = false,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23),
        });
        await context.VehicleEfficiencyCaches.AddAsync(new VehicleEfficiencyCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            LitersPer100Km = 8.5, KWhPer100Km = null,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(89),
        });
        await context.VehicleReliabilityCaches.AddAsync(new VehicleReliabilityCache
        {
            Id = Guid.NewGuid(),
            Make = "Toyota", Model = "Camry", Year = 2021,
            RecallCount = 2, ComplaintCount = 80, OverallSafetyRating = 4,
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _fuelEconomyClient.DidNotReceive().GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _nhtsaClient.DidNotReceive().GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.HasMissingFactors.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CardogMarketFails_StoresFailureSentinelAndReturnsMissingFactor()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Kia", modelName: "Sportage", year: 2022);

        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CardogMarketResult?)null);
        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((FuelEconomyEfficiencyResult?)null);
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaRecallsResult?)null);
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaComplaintsResult?)null);
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaSafetyRatingResult?)null);

        await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        var marketCache = await context.VehicleMarketCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "Kia" && c.Model == "Sportage" && c.Year == 2022);
        marketCache.Should().NotBeNull();
        marketCache!.IsFetchFailed.Should().BeTrue();
        marketCache.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Handle_CardogFailureSentinelNotExpired_DoesNotCallCardogAgain()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Mazda", modelName: "CX-5", year: 2021);

        await context.VehicleMarketCaches.AddAsync(new VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Mazda", Model = "CX-5", Year = 2021,
            MedianPrice = 0, TotalListings = 0,
            IsFetchFailed = true,
            FetchedAt = DateTime.UtcNow.AddMinutes(-30),
            ExpiresAt = DateTime.UtcNow.AddMinutes(90),
        });
        await context.SaveChangesAsync();

        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((FuelEconomyEfficiencyResult?)null);
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaRecallsResult?)null);
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaComplaintsResult?)null);
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaSafetyRatingResult?)null);

        var result = await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _cardogClient.DidNotReceive().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        result.Value.Status.Should().Be("missing");
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
    public async Task Handle_CacheMiss_PersistsAllDataToCache()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "BMW", modelName: "3 Series", year: 2019);

        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new FuelEconomyEfficiencyResult(LitersPer100Km: 6.5, KWhPer100Km: null));
        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 25000m, TotalListings: 30));
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaRecallsResult(0));
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaComplaintsResult(10));
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaSafetyRatingResult(5));

        await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        var efficiencyCache = await context.VehicleEfficiencyCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);
        var marketCache = await context.VehicleMarketCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);
        var reliabilityCache = await context.VehicleReliabilityCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "BMW" && c.Model == "3 Series" && c.Year == 2019);

        efficiencyCache.Should().NotBeNull();
        efficiencyCache!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        marketCache.Should().NotBeNull();
        marketCache!.IsFetchFailed.Should().BeFalse();
        marketCache.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        reliabilityCache.Should().NotBeNull();
        reliabilityCache!.RecallCount.Should().Be(0);
        reliabilityCache.ComplaintCount.Should().Be(10);
        reliabilityCache.OverallSafetyRating.Should().Be(5);
        reliabilityCache.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ExpiredCache_CallsApisAndUpdatesCache()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context, makeName: "Ford", modelName: "Focus", year: 2018);

        var expiredAt = DateTime.UtcNow.AddDays(-1);
        await context.VehicleEfficiencyCaches.AddAsync(new VehicleEfficiencyCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            LitersPer100Km = 9.0, KWhPer100Km = null,
            FetchedAt = expiredAt, ExpiresAt = expiredAt,
        });
        await context.VehicleMarketCaches.AddAsync(new VehicleMarketCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            MedianPrice = 10000m, TotalListings = 15, IsFetchFailed = false,
            FetchedAt = expiredAt, ExpiresAt = expiredAt,
        });
        await context.VehicleReliabilityCaches.AddAsync(new VehicleReliabilityCache
        {
            Id = Guid.NewGuid(),
            Make = "Ford", Model = "Focus", Year = 2018,
            RecallCount = 3, ComplaintCount = 40, OverallSafetyRating = 3,
            FetchedAt = expiredAt, ExpiresAt = expiredAt,
        });
        await context.SaveChangesAsync();

        _fuelEconomyClient.GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new FuelEconomyEfficiencyResult(LitersPer100Km: 8.0, KWhPer100Km: null));
        _cardogClient.GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CardogMarketResult(MedianPrice: 11000m, TotalListings: 20));
        _nhtsaClient.GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaRecallsResult(2));
        _nhtsaClient.GetComplaintsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new NhtsaComplaintsResult(25));
        _nhtsaClient.GetSafetyRatingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((NhtsaSafetyRatingResult?)null);

        await handler.Handle(new GetListingScoreQuery { ListingId = listingId }, CancellationToken.None);

        await _fuelEconomyClient.Received().GetFuelEfficiencyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _cardogClient.Received().GetMarketOverviewAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _nhtsaClient.Received().GetRecallsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());

        var updatedEfficiency = await context.VehicleEfficiencyCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Make == "Ford" && c.Model == "Focus" && c.Year == 2018);
        updatedEfficiency!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
