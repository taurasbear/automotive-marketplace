using Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;
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

public class DeleteListingNoteCommandHandlerTests(
    DatabaseFixture<DeleteListingNoteCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteListingNoteCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteListingNoteCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static DeleteListingNoteCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ExistingNote_ShouldDeleteNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAndNoteAsync(context);

        var command = new DeleteListingNoteCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoExistingNote_ShouldNotThrow()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var command = new DeleteListingNoteCommand { UserId = Guid.NewGuid(), ListingId = Guid.NewGuid() };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_DeleteNote_ShouldNotDeleteLike()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAndNoteAsync(context);

        var command = new DeleteListingNoteCommand { UserId = user.Id, ListingId = listing.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var likeInDb = await context.Set<UserListingLike>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == user.Id && l.ListingId == listing.Id);
        likeInDb.Should().NotBeNull();
    }

    private static async Task<(User user, Listing listing)> SeedUserWithLikeAndNoteAsync(AutomotiveContext context)
    {
        var user = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(user.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var like = new UserListingLike
        {
            Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id,
            CreatedAt = DateTime.UtcNow, CreatedBy = user.Id.ToString()
        };
        var note = new UserListingNoteBuilder()
            .WithUser(user.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like, note);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
