using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;
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

public class UpsertListingNoteCommandHandlerTests(
    DatabaseFixture<UpsertListingNoteCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpsertListingNoteCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpsertListingNoteCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static UpsertListingNoteCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingNote_ShouldCreateNote()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAsync(context);

        var command = new UpsertListingNoteCommand
        {
            UserId = user.Id, ListingId = listing.Id, Content = "Nice car!"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb.Should().NotBeNull();
        noteInDb!.Content.Should().Be("Nice car!");
        noteInDb.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        noteInDb.CreatedBy.Should().Be(user.Id.ToString());
        noteInDb.ModifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExistingNote_ShouldUpdateContent()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing) = await SeedUserWithLikeAsync(context);
        var note = new UserListingNoteBuilder()
            .WithUser(user.Id)
            .WithListing(listing.Id)
            .WithContent("Old note")
            .Build();
        await context.AddAsync(note);
        await context.SaveChangesAsync();

        var command = new UpsertListingNoteCommand
        {
            UserId = user.Id, ListingId = listing.Id, Content = "Updated note"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var noteInDb = await context.Set<UserListingNote>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.UserId == user.Id && n.ListingId == listing.Id);
        noteInDb!.Content.Should().Be("Updated note");
        noteInDb.ModifiedAt.Should().NotBeNull();
        noteInDb.ModifiedBy.Should().Be(user.Id.ToString());
    }

    [Fact]
    public async Task Handle_NoLikeExists_ShouldThrowNotFoundException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var command = new UpsertListingNoteCommand
        {
            UserId = Guid.NewGuid(), ListingId = Guid.NewGuid(), Content = "Orphan note"
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    private static async Task<(User user, Listing listing)> SeedUserWithLikeAsync(AutomotiveContext context)
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
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, CreatedAt = DateTime.UtcNow, CreatedBy = user.Id.ToString() };

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like);
        await context.SaveChangesAsync();

        return (user, listing);
    }
}
