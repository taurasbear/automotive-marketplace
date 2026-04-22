using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;
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

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class RespondToOfferCommandHandlerTests(
    DatabaseFixture<RespondToOfferCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToOfferCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToOfferCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToOfferCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_AcceptOffer_ShouldSetStatusAcceptedAndListingOnHold()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        result.NewStatus.Should().Be(OfferStatus.Accepted);
        result.CounterOffer.Should().BeNull();

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Accepted);

        await context.Entry(listing).ReloadAsync();
        listing.Status.Should().Be(Status.OnHold);
    }

    [Fact]
    public async Task Handle_DeclineOffer_ShouldSetStatusDeclinedAndListingStaysAvailable()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Decline
        }, CancellationToken.None);

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Declined);

        await context.Entry(listing).ReloadAsync();
        listing.Status.Should().Be(Status.Available);
    }

    [Fact]
    public async Task Handle_CounterOffer_ShouldCreateNewOfferAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var counterAmount = listing.Price * 0.9m;
        var result = await handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Counter,
            CounterAmount = counterAmount
        }, CancellationToken.None);

        result.NewStatus.Should().Be(OfferStatus.Countered);
        result.CounterOffer.Should().NotBeNull();
        result.CounterOffer!.Offer.Amount.Should().Be(counterAmount);
        result.CounterOffer.Offer.ParentOfferId.Should().Be(offer.Id);

        await context.Entry(offer).ReloadAsync();
        offer.Status.Should().Be(OfferStatus.Countered);

        var newOffer = await context.Offers.FindAsync(result.CounterOffer.Offer.Id);
        newOffer.Should().NotBeNull();
        newOffer!.Status.Should().Be(OfferStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.CounterOffer.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Offer);
        newMessage.OfferId.Should().Be(newOffer.Id);
    }

    [Fact]
    public async Task Handle_OfferAlreadyResolved_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);
        offer.Status = OfferStatus.Accepted;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Decline
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnOffer_ShouldThrowUnauthorizedException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, offer, _) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = buyer.Id,  // buyer made the offer; can't respond to own offer
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_OfferExpired_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);
        offer.ExpiresAt = DateTime.UtcNow.AddHours(-1);  // already expired
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    [Fact]
    public async Task Handle_CounterAmountBelowMinimum_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, offer, listing) = await SeedPendingOfferAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToOfferCommand
        {
            OfferId = offer.Id,
            ResponderId = listing.SellerId,
            Action = OfferResponseAction.Counter,
            CounterAmount = listing.Price / 4  // below 1/3 floor
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Automotive.Marketplace.Application.Common.Exceptions.RequestValidationException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Offer offer, Listing listing)>
        SeedPendingOfferAsync(AutomotiveContext context, bool initiatedByBuyer)
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

        var initiatorId = initiatedByBuyer ? buyer.Id : seller.Id;
        var offer = new OfferBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .WithAmount(listing.Price * 0.8m)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation, offer);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, offer, listing);
    }
}
