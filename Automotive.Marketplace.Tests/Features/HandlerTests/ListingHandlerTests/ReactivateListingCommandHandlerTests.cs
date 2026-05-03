using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.ReactivateListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class ReactivateListingCommandHandlerTests(
    DatabaseFixture<ReactivateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ReactivateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ReactivateListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static ReactivateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new ReactivateListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_OnHoldListing_ReactivatesToAvailable()
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
            .With(l => l.Status, Status.OnHold)
            .Build();

        await context.AddAsync(listing);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        var response = await handler.Handle(
            new ReactivateListingCommand
            {
                ListingId = listing.Id,
                CurrentUserId = user.Id,
                Permissions = []
            },
            CancellationToken.None);

        response.ListingId.Should().Be(listing.Id);
        response.CancelledOffers.Should().BeEmpty();

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listing.Id);
        updated.Status.Should().Be(Status.Available);
    }

    [Fact]
    public async Task Handle_NonExistentListing_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var userId = Guid.NewGuid();

        var act = () => handler.Handle(
            new ReactivateListingCommand
            {
                ListingId = Guid.NewGuid(),
                CurrentUserId = userId,
                Permissions = []
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_NotOnHoldListing_ThrowsInvalidOperationException()
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
            .Build(); // Status.Available by default

        await context.AddAsync(listing);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        var act = () => handler.Handle(
            new ReactivateListingCommand
            {
                ListingId = listing.Id,
                CurrentUserId = user.Id,
                Permissions = []
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only OnHold listings can be reactivated.");
    }
}
