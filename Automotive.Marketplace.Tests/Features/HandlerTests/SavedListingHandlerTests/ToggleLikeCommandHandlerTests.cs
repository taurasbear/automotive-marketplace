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

        var (buyer, listing) = await SeedUserAndListingAsync(context);

        var command = new ToggleLikeCommand { UserId = buyer.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeTrue();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == buyer.Id && l.ListingId == listing.Id);
        likeInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExistingLike_ShouldDeleteLikeAndReturnIsLikedFalse()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = buyer.Id, ListingId = listing.Id, CreatedAt = DateTime.UtcNow, CreatedBy = buyer.Id.ToString() };
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = buyer.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == buyer.Id && l.ListingId == listing.Id);
        likeInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingLikeWithNote_ShouldDeleteBothLikeAndNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedUserAndListingAsync(context);
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = buyer.Id, ListingId = listing.Id, CreatedAt = DateTime.UtcNow, CreatedBy = buyer.Id.ToString() };
        var note = new UserListingNoteBuilder().WithUser(buyer.Id).WithListing(listing.Id).WithContent("Great car!").Build();
        await context.AddRangeAsync(like, note);
        await context.SaveChangesAsync();

        var command = new ToggleLikeCommand { UserId = buyer.Id, ListingId = listing.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsLiked.Should().BeFalse();
        var likeInDb = await context.Set<UserListingLike>()
            .FirstOrDefaultAsync(l => l.UserId == buyer.Id && l.ListingId == listing.Id);
        likeInDb.Should().BeNull();
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == buyer.Id && n.ListingId == listing.Id);
        noteInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SellerTriesToLikeOwnListing_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (seller, listing) = await SeedSellerWithListingAsync(context);
        // The seller tries to like their own listing
        var command = new ToggleLikeCommand { UserId = seller.Id, ListingId = listing.Id };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, Listing listing)> SeedUserAndListingAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
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
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, municipality, variant, listing);
        await context.SaveChangesAsync();

        return (buyer, listing);
    }

    private static async Task<(User seller, Listing listing)> SeedSellerWithListingAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
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
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(seller, make, model, fuel, transmission, bodyType, drivetrain, municipality, variant, listing);
        await context.SaveChangesAsync();

        return (seller, listing);
    }
}
