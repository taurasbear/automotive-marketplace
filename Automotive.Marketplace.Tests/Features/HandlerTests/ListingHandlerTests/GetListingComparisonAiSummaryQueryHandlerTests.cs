using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;
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

public class GetListingComparisonAiSummaryQueryHandlerTests(
    DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingComparisonAiSummaryQueryHandlerTests> _fixture = fixture;
    private readonly IOpenAiClient _openAiClient = Substitute.For<IOpenAiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingComparisonAiSummaryQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>(), _openAiClient);

    private async Task<Guid> SeedListingAsync(AutomotiveContext context, string makeName, string modelName)
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
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, municipality, listing);
        await context.SaveChangesAsync();
        return listing.Id;
    }

    [Fact]
    public async Task Handle_NoCacheEntry_CallsOpenAiAndCaches()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var idA = await SeedListingAsync(context, "Honda", "Civic");
        var idB = await SeedListingAsync(context, "Toyota", "Corolla");

        _openAiClient.GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("The Toyota Corolla is the better choice due to reliability.");

        var result = await handler.Handle(
            new GetListingComparisonAiSummaryQuery { ListingAId = idA, ListingBId = idB },
            CancellationToken.None);

        result.IsGenerated.Should().BeTrue();
        result.Summary.Should().NotBeNullOrEmpty();
        result.FromCache.Should().BeFalse();

        var keyId = idA < idB ? idA : idB;
        var compId = idA < idB ? idB : idA;
        var cached = await context.ListingAiSummaryCaches
            .FirstOrDefaultAsync(c => c.ListingId == keyId && c.ComparisonListingId == compId && c.SummaryType == "comparison");
        cached.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReversedOrder_ReturnsSameCachedResult()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var idA = await SeedListingAsync(context, "Honda", "Civic");
        var idB = await SeedListingAsync(context, "Toyota", "Corolla");

        var keyId = idA < idB ? idA : idB;
        var compId = idA < idB ? idB : idA;

        await context.ListingAiSummaryCaches.AddAsync(new ListingAiSummaryCache
        {
            Id = Guid.NewGuid(),
            ListingId = keyId,
            ComparisonListingId = compId,
            SummaryType = "comparison",
            Summary = "Corolla is better.",
            GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(29),
        });
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new GetListingComparisonAiSummaryQuery { ListingAId = idB, ListingBId = idA },
            CancellationToken.None);

        result.FromCache.Should().BeTrue();
        result.Summary.Should().Be("Corolla is better.");
        await _openAiClient.DidNotReceive().GetResponseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
