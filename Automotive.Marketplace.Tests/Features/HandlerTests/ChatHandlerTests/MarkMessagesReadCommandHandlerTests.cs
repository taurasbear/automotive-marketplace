using Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;
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

public class MarkMessagesReadCommandHandlerTests(
    DatabaseFixture<MarkMessagesReadCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<MarkMessagesReadCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<MarkMessagesReadCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private MarkMessagesReadCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_UnreadMessages_ShouldMarkThemAsRead()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation) = await SeedConversationWithUnreadMessagesAsync(context);

        // Act
        await handler.Handle(
            new MarkMessagesReadCommand { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        var unread = await context.Messages
            .Where(m => m.ConversationId == conversation.Id
                && m.SenderId == seller.Id && !m.IsRead)
            .CountAsync();
        unread.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AfterMarkingRead_ShouldReturnZeroTotalUnreadCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationWithUnreadMessagesAsync(context);

        // Act
        var result = await handler.Handle(
            new MarkMessagesReadCommand { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        result.TotalUnreadCount.Should().Be(0);
    }

    private static async Task<(User buyer, User seller, Conversation conversation)>
        SeedConversationWithUnreadMessagesAsync(AutomotiveContext context)
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

        var msg1 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();
        var msg2 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .WithIsRead(false)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, variant, listing,
            conversation, msg1, msg2);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
