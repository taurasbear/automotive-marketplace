using AutoMapper;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ChatHandlerTests;

public class GetMessagesQueryHandlerTests(
    DatabaseFixture<GetMessagesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetMessagesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetMessagesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetMessagesQueryHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>(),
               scope.ServiceProvider.GetRequiredService<IMapper>());

    [Fact]
    public async Task Handle_ParticipantRequest_ShouldReturnMessagesOrderedBySentAt()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation) = await SeedConversationWithMessagesAsync(context);

        // Act
        var result = await handler.Handle(
            new GetMessagesQuery { ConversationId = conversation.Id, UserId = buyer.Id },
            CancellationToken.None);

        // Assert
        result.Messages.Should().HaveCount(2);
        result.Messages.Should().BeInAscendingOrder(m => m.SentAt);
    }

    [Fact]
    public async Task Handle_NonParticipantRequest_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation) = await SeedConversationWithMessagesAsync(context);

        // Act
        var act = async () => await handler.Handle(
            new GetMessagesQuery { ConversationId = conversation.Id, UserId = Guid.NewGuid() },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation)>
        SeedConversationWithMessagesAsync(AutomotiveContext context)
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
            .WithSender(buyer.Id)
            .With(m => m.SentAt, DateTime.UtcNow.AddMinutes(-5))
            .Build();
        var msg2 = new MessageBuilder()
            .WithConversation(conversation.Id)
            .WithSender(seller.Id)
            .With(m => m.SentAt, DateTime.UtcNow)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel, transmission, bodyType, drivetrain, variant, listing,
            conversation, msg1, msg2);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation);
    }
}
