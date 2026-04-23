using Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;
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

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetConversationsQueryHandlerTests(
    DatabaseFixture<GetConversationsQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetConversationsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetConversationsQueryHandlerTests> _fixture = fixture;

    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetConversationsQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(), _imageStorageService);

    [Fact]
    public async Task Handle_BuyerWithConversations_ShouldReturnConversationsAsBuyer()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        // Act
        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = buyer.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_SellerWithConversations_ShouldReturnConversationsAsSeller()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, _) = await SeedConversationAsync(context);

        // Act
        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = seller.Id }, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_NoConversations_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        // Act
        var result = await handler.Handle(
            new GetConversationsQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_BuyerHasLikedListing_ShouldReturnBuyerHasLikedTrue()
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

        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = seller.Id },
            CancellationToken.None)).ToList();

        result.Should().ContainSingle();
        result[0].BuyerHasEngaged.Should().BeTrue();
        result[0].BuyerId.Should().Be(buyer.Id);
        result[0].SellerId.Should().Be(seller.Id);
    }

    [Fact]
    public async Task Handle_BuyerHasNotEngaged_ShouldReturnBuyerHasEngagedFalse()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, _) = await SeedConversationAsync(context);

        var result = (await handler.Handle(
            new GetConversationsQuery { UserId = seller.Id },
            CancellationToken.None)).ToList();

        result.Should().ContainSingle();
        result[0].BuyerHasEngaged.Should().BeFalse();
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
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
