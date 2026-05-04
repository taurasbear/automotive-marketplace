using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListingStatus;
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

public class UpdateListingStatusCommandHandlerTests(
    DatabaseFixture<UpdateListingStatusCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateListingStatusCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateListingStatusCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateListingStatusCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateListingStatusCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_AvailableToSold_UpdatesStatus()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        // Create and save lookup entities
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();

        await context.AddRangeAsync(fuel, transmission, bodyType);
        await context.SaveChangesAsync();

        // Create variant with relationships to fuel, transmission, bodyType
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

        // Create listing with Available status
        var listing = new ListingBuilder()
            .WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id)
            .WithMunicipality(municipality.Id)
            .WithSeller(user.Id)
            .Build();

        await context.AddAsync(listing);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        await handler.Handle(
            new UpdateListingStatusCommand
            {
                Id = listing.Id,
                Status = "Sold",
                CurrentUserId = user.Id,
                Permissions = [],
            },
            CancellationToken.None);

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listing.Id);
        updated.Status.Should().Be(Status.Sold);
    }

    [Fact]
    public async Task Handle_NonExistentListing_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var userId = Guid.NewGuid();

        var act = () => handler.Handle(
            new UpdateListingStatusCommand
            {
                Id = Guid.NewGuid(),
                Status = "Sold",
                CurrentUserId = userId,
                Permissions = [],
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
