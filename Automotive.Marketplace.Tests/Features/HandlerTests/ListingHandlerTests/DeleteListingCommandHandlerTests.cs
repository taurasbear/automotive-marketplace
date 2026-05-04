using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class DeleteListingCommandHandlerTests(
    DatabaseFixture<DeleteListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new DeleteListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ExistingListing_DeletesSuccessfully()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        // Create and save all required entities
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();

        await context.AddRangeAsync(fuel, transmission, bodyType);
        await context.SaveChangesAsync();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id)
            .WithFuel(fuel.Id)
            .WithTransmission(transmission.Id)
            .WithBodyType(bodyType.Id)
            .Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var user = new UserBuilder().Build();

        await context.AddRangeAsync(make, model, variant, drivetrain, municipality, user);
        await context.SaveChangesAsync();

        var listing = new ListingBuilder()
            .WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id)
            .WithMunicipality(municipality.Id)
            .WithSeller(user.Id)
            .Build();

        await context.AddAsync(listing);
        await context.SaveChangesAsync();
        var listingId = listing.Id;

        var handler = CreateHandler(scope);

        await handler.Handle(
            new DeleteListingCommand { Id = listingId, CurrentUserId = user.Id, Permissions = [] },
            CancellationToken.None);

        var deleted = await context.Set<Listing>().AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentListing_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(
            new DeleteListingCommand { Id = Guid.NewGuid(), CurrentUserId = Guid.NewGuid(), Permissions = [] },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
