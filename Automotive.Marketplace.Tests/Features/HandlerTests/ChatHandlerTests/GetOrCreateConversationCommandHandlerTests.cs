using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetOrCreateConversationCommandHandlerTests(
    DatabaseFixture<GetOrCreateConversationCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetOrCreateConversationCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetOrCreateConversationCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetOrCreateConversationCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NewConversation_ShouldCreateAndReturnConversationId()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedBuyerAndListingAsync(context);

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = buyer.Id,
            ListingId = listing.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ConversationId.Should().NotBeEmpty();
        var saved = await context.Conversations.FindAsync(result.ConversationId);
        saved.Should().NotBeNull();
        saved!.BuyerId.Should().Be(buyer.Id);
        saved.ListingId.Should().Be(listing.Id);
    }

    [Fact]
    public async Task Handle_ExistingConversation_ShouldReturnExistingConversationId()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, listing) = await SeedBuyerAndListingAsync(context);
        var existing = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();
        await context.AddAsync(existing);
        await context.SaveChangesAsync();

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = buyer.Id,
            ListingId = listing.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ConversationId.Should().Be(existing.Id);
        var count = await context.Conversations.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_BuyerIsTheSeller_ShouldThrowRequestValidationException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();

        await context.AddRangeAsync(seller, make, model, fuel, transmission, bodyType, drivetrain, variant, listing);
        await context.SaveChangesAsync();

        var command = new GetOrCreateConversationCommand
        {
            BuyerId = seller.Id,
            ListingId = listing.Id
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RequestValidationException>();
    }

    private static async Task<(User buyer, Listing listing)> SeedBuyerAndListingAsync(
        AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var buyer = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, variant, listing);
        await context.SaveChangesAsync();

        return (buyer, listing);
    }
}
