using Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class SendMessageCommandHandlerTests(
    DatabaseFixture<SendMessageCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SendMessageCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<SendMessageCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private SendMessageCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistMessageAndUpdateLastMessageAt()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation) = await SeedConversationAsync(context);

        var command = new SendMessageCommand
        {
            ConversationId = conversation.Id,
            SenderId = buyer.Id,
            Content = "Is this still available?"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Content.Should().Be("Is this still available?");
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().Be(seller.Id);

        var savedMessage = await context.Messages.FindAsync(result.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.ConversationId.Should().Be(conversation.Id);

        var updatedConversation = await context.Conversations.FindAsync(conversation.Id);
        await context.Entry(updatedConversation!).ReloadAsync();
        updatedConversation!.LastMessageAt.Should().BeCloseTo(result.SentAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_UnreadMessage_ShouldIncrementRecipientUnreadCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationAsync(context);

        // Act
        var result = await handler.Handle(new SendMessageCommand
        {
            ConversationId = conversation.Id,
            SenderId = buyer.Id,
            Content = "Hello"
        }, CancellationToken.None);

        // Assert
        result.RecipientUnreadCount.Should().Be(1);
    }

    private static async Task<(User buyer, User seller, Conversation conversation)> SeedConversationAsync(
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
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id)
            .WithListing(listing.Id)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
