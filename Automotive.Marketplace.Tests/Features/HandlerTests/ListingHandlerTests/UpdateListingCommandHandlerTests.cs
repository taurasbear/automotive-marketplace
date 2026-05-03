using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class UpdateListingCommandHandlerTests(
    DatabaseFixture<UpdateListingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateListingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateListingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpdateListingCommandHandler CreateHandler(IServiceScope scope)
    {
        return new UpdateListingCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>(),
            scope.ServiceProvider.GetRequiredService<IMapper>());
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesListingFields()
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

        // Create listing
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

        var command = new UpdateListingCommand
        {
            Id = listingId,
            CurrentUserId = user.Id,
            Permissions = [],
            Price = 9999m,
            Mileage = 50000,
            Description = "Updated description",
            MunicipalityId = municipality.Id,
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await context.Set<Listing>().AsNoTracking()
            .FirstAsync(l => l.Id == listingId);
        updated.Price.Should().Be(9999m);
        updated.Mileage.Should().Be(50000);
        updated.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task Handle_NonExistentListing_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(
            new UpdateListingCommand 
            { 
                Id = Guid.NewGuid(),
                CurrentUserId = Guid.NewGuid(),
                Permissions = []
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
