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

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingScoreQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingScoreQueryHandler(repository, _cardogClient);
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

        await act.Should().ThrowAsync<Exception>();
    }
}
