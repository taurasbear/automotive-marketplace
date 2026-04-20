using Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.SavedListingHandlerTests;

public class GetSavedListingsQueryHandlerTests(
    DatabaseFixture<GetSavedListingsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetSavedListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetSavedListingsQueryHandlerTests> _fixture = fixture;

    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetSavedListingsQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(), _imageStorageService);

    [Fact]
    public async Task Handle_UserWithLikedListings_ShouldReturnListingsWithDetails()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing, _) = await SeedLikeWithNoteAsync(context, noteContent: null);

        // Act
        var result = (await handler.Handle(
            new GetSavedListingsQuery { UserId = user.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ListingId.Should().Be(listing.Id);
        result[0].Price.Should().Be(listing.Price);
        result[0].City.Should().Be(listing.City);
        result[0].Mileage.Should().Be(listing.Mileage);
        result[0].NoteContent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserWithLikedListingAndNote_ShouldReturnNoteContent()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (user, listing, _) = await SeedLikeWithNoteAsync(context, noteContent: "Check this one!");

        // Act
        var result = (await handler.Handle(
            new GetSavedListingsQuery { UserId = user.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].NoteContent.Should().Be("Check this one!");
    }

    [Fact]
    public async Task Handle_UserWithNoLikes_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        // Act
        var result = await handler.Handle(
            new GetSavedListingsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    private static async Task<(User user, Listing listing, UserListingLike like)> SeedLikeWithNoteAsync(
        AutomotiveContext context, string? noteContent)
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
        var like = new UserListingLike { Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id };

        await context.AddRangeAsync(user, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, like);

        if (noteContent is not null)
        {
            var note = new UserListingNote
            {
                Id = Guid.NewGuid(), UserId = user.Id, ListingId = listing.Id, Content = noteContent
            };
            await context.AddAsync(note);
        }

        await context.SaveChangesAsync();
        return (user, listing, like);
    }
}
