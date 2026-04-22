using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingComparisonQueryHandlerTests(
    DatabaseFixture<GetListingComparisonQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingComparisonQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingComparisonQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingComparisonQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingComparisonQueryHandler(mapper, repository, _imageStorageService);
    }

    private async Task<(Guid listingId, Guid variantId)> SeedListingAsync(AutomotiveContext context)
    {
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
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();
        return (listing.Id, variant.Id);
    }

    [Fact]
    public async Task Handle_BothListingsExist_ReturnsBothInResponse()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idA, _) = await SeedListingAsync(context);
        var (idB, _) = await SeedListingAsync(context);

        var query = new GetListingComparisonQuery { ListingAId = idA, ListingBId = idB };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ListingA.Should().NotBeNull();
        result.ListingB.Should().NotBeNull();
        result.ListingA.Id.Should().Be(idA);
        result.ListingB.Id.Should().Be(idB);
    }

    [Fact]
    public async Task Handle_ListingAMissing_Throws()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idB, _) = await SeedListingAsync(context);
        var missingId = Guid.NewGuid();

        var query = new GetListingComparisonQuery { ListingAId = missingId, ListingBId = idB };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ListingBMissing_Throws()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (idA, _) = await SeedListingAsync(context);
        var missingId = Guid.NewGuid();

        var query = new GetListingComparisonQuery { ListingAId = idA, ListingBId = missingId };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_SameIdForBoth_ReturnsSameListingTwice()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (id, _) = await SeedListingAsync(context);

        var query = new GetListingComparisonQuery { ListingAId = id, ListingBId = id };

        var result = await handler.Handle(query, CancellationToken.None);

        result.ListingA.Id.Should().Be(id);
        result.ListingB.Id.Should().Be(id);
    }
}
