using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
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

public class MakeOfferCommandHandlerTests(
    DatabaseFixture<MakeOfferCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<MakeOfferCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<MakeOfferCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private MakeOfferCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerMakesValidOffer_ShouldPersistOfferAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var offerAmount = listing.Price * 0.8m;
        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = offerAmount
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.Offer.Id.Should().NotBeEmpty();
        result.Offer.Amount.Should().Be(offerAmount);
        result.Offer.Status.Should().Be(OfferStatus.Pending);
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().NotBe(buyer.Id);

        var savedOffer = await context.Offers.FindAsync(result.Offer.Id);
        savedOffer.Should().NotBeNull();
        savedOffer!.Status.Should().Be(OfferStatus.Pending);
        savedOffer.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Offer);
        savedMessage.OfferId.Should().Be(result.Offer.Id);

        var savedConversation = await context.Conversations.FindAsync(conversation.Id);
        await context.Entry(savedConversation!).ReloadAsync();
        savedConversation!.LastMessageAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        result.Offer.ListingPrice.Should().Be(listing.Price);
        result.Offer.PercentageOff.Should().Be(20.00m);  // listing.Price=15000, offerAmount=15000*0.8=12000, discount=20%
    }

    [Fact]
    public async Task Handle_SellerMakesOfferWhenBuyerHasLiked_ShouldSucceed()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var like = new UserListingLikeBuilder()
            .WithUser(buyer.Id)
            .WithListing(listing.Id)
            .Build();
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            Amount = listing.Price * 0.95m
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Offer.Id.Should().NotBeEmpty();
        result.Offer.Status.Should().Be(OfferStatus.Pending);
        result.SenderId.Should().Be(seller.Id);
        result.RecipientId.Should().Be(buyer.Id);
    }

    [Fact]
    public async Task Handle_SellerMakesOfferWithoutBuyerEngagement_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            Amount = listing.Price * 0.9m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_AmountBelowMinimum_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price / 4  // less than 1/3
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Amount"));
    }

    [Fact]
    public async Task Handle_AmountAboveListingPrice_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price + 1
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Amount"));
    }

    [Fact]
    public async Task Handle_ListingNotAvailable_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);
        listing.Status = Status.OnHold;
        await context.SaveChangesAsync();

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price * 0.8m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ListingId"));
    }

    [Fact]
    public async Task Handle_ActiveOfferAlreadyExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, listing) = await SeedConversationAsync(context);

        var existingOffer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithAmount(listing.Price * 0.8m)
            .Build();
        await context.AddAsync(existingOffer);
        await context.SaveChangesAsync();

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Amount = listing.Price * 0.7m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyInitiator_ShouldThrowUnauthorizedException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, listing) = await SeedConversationAsync(context);

        var command = new MakeOfferCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = Guid.NewGuid(), // not buyer or seller
            Amount = listing.Price * 0.8m
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
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
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
