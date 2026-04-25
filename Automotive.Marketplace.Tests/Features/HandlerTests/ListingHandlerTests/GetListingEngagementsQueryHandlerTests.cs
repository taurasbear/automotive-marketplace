using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetListingEngagementsQueryHandlerTests(
    DatabaseFixture<GetListingEngagementsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetListingEngagementsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetListingEngagementsQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetListingEngagementsQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetListingEngagementsQueryHandler(repository);
    }

    private async Task<(Guid listingId, Guid sellerId)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();
        return (listing.Id, seller.Id);
    }

    [Fact]
    public async Task Handle_ListingDoesNotExist_ShouldThrowDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var query = new GetListingEngagementsQuery
        {
            ListingId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
        };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_CurrentUserIsNotSeller_ShouldThrowUnauthorizedAccessException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, _) = await SeedListingAsync(context);

        var query = new GetListingEngagementsQuery
        {
            ListingId = listingId,
            CurrentUserId = Guid.NewGuid(),
        };

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NoEngagements_ShouldReturnEmptyCollections()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().BeEmpty();
        result.Likers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithConversation_ShouldReturnConversationWithCorrectLastMessageType()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var buyer = new UserBuilder().Build();
        var conversation = new ConversationBuilder().WithListing(listingId).WithBuyer(buyer.Id).Build();
        var textMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Text)
            .With(m => m.SentAt, DateTime.UtcNow.AddMinutes(-10))
            .Build();
        var offerMessage = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Offer)
            .With(m => m.SentAt, DateTime.UtcNow)
            .Build();

        await context.AddRangeAsync(buyer, conversation, textMessage, offerMessage);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().HaveCount(1);
        var c = result.Conversations.First();
        c.BuyerId.Should().Be(buyer.Id);
        c.BuyerUsername.Should().Be(buyer.Username);
        c.LastMessageType.Should().Be("Offer");
    }

    [Fact]
    public async Task Handle_WithLikerWhoHasNoConversation_ShouldAppearInLikers()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var liker = new UserBuilder().Build();
        var like = new UserListingLikeBuilder().WithListing(listingId).WithUser(liker.Id).Build();

        await context.AddRangeAsync(liker, like);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Likers.Should().HaveCount(1);
        result.Likers.First().UserId.Should().Be(liker.Id);
        result.Likers.First().Username.Should().Be(liker.Username);
        result.Conversations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWhoLikedAndMessaged_ShouldAppearOnlyInConversations()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var (listingId, sellerId) = await SeedListingAsync(context);

        var buyer = new UserBuilder().Build();
        var conversation = new ConversationBuilder().WithListing(listingId).WithBuyer(buyer.Id).Build();
        var message = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(buyer.Id)
            .With(m => m.MessageType, MessageType.Text)
            .Build();
        var like = new UserListingLikeBuilder().WithListing(listingId).WithUser(buyer.Id).Build();

        await context.AddRangeAsync(buyer, conversation, message, like);
        await context.SaveChangesAsync();

        var query = new GetListingEngagementsQuery { ListingId = listingId, CurrentUserId = sellerId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Conversations.Should().HaveCount(1);
        result.Likers.Should().BeEmpty();
    }
}
