using Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
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

    private async Task<(Guid listingId, Guid sellerId)> SeedListingAsync(AutomotiveContext context, bool hasDescription = false, bool hasVin = false, bool hasColour = false)
    {
        var make = new MakeBuilder().With(m => m.Name, "Volkswagen").Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Golf").Build();
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
}
