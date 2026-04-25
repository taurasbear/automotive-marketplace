using Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class ToggleLikeCommandHandlerTests(
    DatabaseFixture<ToggleLikeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ToggleLikeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ToggleLikeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static ToggleLikeCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingLike_ShouldCreateLikeAndReturnIsLikedTrue()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeTrue();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExistingLike_ShouldDeleteLikeAndReturnIsLikedFalse()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, CreatedAt = DateTime.UtcNow, CreatedBy = user.Id.ToString() };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingLikeWithNote_ShouldDeleteBothLikeAndNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, CreatedAt = DateTime.UtcNow, CreatedBy = user.Id.ToString() };
        var note = new UserListingNoteBuilder().WithUser(user.Id).WithListing(listing.Id).WithContent("Great car!").Build();
        await context.AddRangeAsync(like, note);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().BeNull();
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().BeNull();
    }

    private static async Task<(User user, Listing listing)> SeedUserAndListingAsync(AutomotiveContext context)
    {
        var user = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, municipality, variant, listing);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
