using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;
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

public class GetListingAiSummaryQueryHandlerTests(
    DatabaseFixture<GetListingAiSummaryQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingAiSummaryQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingAiSummaryQueryHandlerTests> _fixture = fixture;
    private readonly IOpenAiClient _openAiClient = Substitute.For<IOpenAiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingAiSummaryQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>(), _openAiClient);

    private async Task<Guid> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().With(m => m.Name, "Toyota").Build();
        var model = new ModelBuilder().WithMake(make.Id).With(m => m.Name, "Corolla").Build();
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
            .WithMunicipality(municipality.Id).WithYear(2021).WithPrice(12500m)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_NoCacheEntry_CallsOpenAiAndCachesResult()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);
        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("This Toyota Corolla represents solid value for money.");

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.IsGenerated.Should().BeTrue();
        result.Summary.Should().Contain("Toyota");
        result.FromCache.Should().BeFalse();

        var cached = await context.ListingAiSummaryCaches
            .FirstOrDefaultAsync(c => c.ListingId == listingId && c.SummaryType == "buyer");
        cached.Should().NotBeNull();
        cached!.Summary.Should().Contain("Toyota");
    }

    [Fact]
    public async Task Handle_ValidCacheEntry_ReturnsCachedSummaryWithoutCallingApi()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);

        await context.ListingAiSummaryCaches.AddAsync(new ListingAiSummaryCache
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            SummaryType = "buyer",
            ComparisonListingId = null,
            Summary = "Cached summary text.",
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.FromCache.Should().BeTrue();
        result.Summary.Should().Be("Cached summary text.");
        await _openAiClient.DidNotReceive().GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OpenAiReturnsNull_ReturnsNotGenerated()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listingId = await SeedListingAsync(context);
        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var result = await handler.Handle(new GetListingAiSummaryQuery { ListingId = listingId }, CancellationToken.None);

        result.IsGenerated.Should().BeFalse();
        result.Summary.Should().BeNull();
    }
}
