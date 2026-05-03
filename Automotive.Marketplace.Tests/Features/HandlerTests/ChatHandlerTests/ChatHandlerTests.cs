using Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetUnreadCount;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class ChatHandlerTests(
    DatabaseFixture<ChatHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ChatHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ChatHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetUnreadCountQueryHandler CreateGetUnreadCountHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static CancelOfferCommandHandler CreateCancelOfferHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task GetUnreadCount_WithUnreadMessages_ReturnsCorrectCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateGetUnreadCountHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        // Create unread messages from seller to buyer
        var unreadMessage1 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();
        var unreadMessage2 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();
        // Create read message (should not be counted)
        var readMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(true)
            .Build();
        // Create message from buyer to themselves (should not be counted)
        var ownMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .WithIsRead(false)
            .Build();

        await context.AddRangeAsync(unreadMessage1, unreadMessage2, readMessage, ownMessage);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetUnreadCountQuery { UserId = buyer.Id }, CancellationToken.None);

        // Assert
        result.UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUnreadCount_NoMessages_ReturnsZero()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateGetUnreadCountHandler(scope);

        // Act
        var result = await handler.Handle(new GetUnreadCountQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelOffer_ValidPendingOffer_CancelsSuccessfully()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateCancelOfferHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var offer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(OfferStatus.Pending)
            .Build();

        await context.AddAsync(offer);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(
            new CancelOfferCommand { OfferId = offer.Id, RequesterId = buyer.Id },
            CancellationToken.None);

        // Assert
        result.OfferId.Should().Be(offer.Id);
        result.InitiatorId.Should().Be(buyer.Id);
        result.RecipientId.Should().Be(seller.Id);
        result.ConversationId.Should().Be(conversation.Id);

        // Verify offer status was updated
        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOffer_NonInitiator_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateCancelOfferHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var offer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(OfferStatus.Pending)
            .Build();

        await context.AddAsync(offer);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new CancelOfferCommand { OfferId = offer.Id, RequesterId = seller.Id },
                CancellationToken.None));
    }

    [Fact]
    public async Task CancelOffer_NonPendingOffer_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateCancelOfferHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var offer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(OfferStatus.Accepted)
            .Build();

        await context.AddAsync(offer);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new CancelOfferCommand { OfferId = offer.Id, RequesterId = buyer.Id },
                CancellationToken.None));
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Listing listing)>
        SeedConversationAsync(AutomotiveContext context)
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
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, municipality, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}