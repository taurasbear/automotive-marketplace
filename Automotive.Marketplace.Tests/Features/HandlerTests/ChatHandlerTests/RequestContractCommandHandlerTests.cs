using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;
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

public class RequestContractCommandHandlerTests(
    DatabaseFixture<RequestContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RequestContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private RequestContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_ValidRequest_CreatesContractCardPendingAndMessage()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.ContractCard.Status.Should().Be(ContractCardStatus.Pending);
        result.ContractCard.InitiatorId.Should().Be(buyer.Id);

        var savedCard = await context.ContractCards.FindAsync(result.ContractCard.Id);
        savedCard.Should().NotBeNull();
        savedCard!.Status.Should().Be(ContractCardStatus.Pending);

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Contract);
        savedMessage.ContractCardId.Should().Be(savedCard.Id);

        var savedConversation = await context.Conversations.FindAsync(conversation.Id);
        await context.Entry(savedConversation!).ReloadAsync();
        savedConversation!.LastMessageAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_ActiveContractAlreadyExists_ThrowsConflict()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingCard = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(ContractCardStatus.Active)
            .Build();
        await context.AddAsync(existingCard);
        await context.SaveChangesAsync();

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_NonParticipant_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, _) = await SeedConversationAsync(context);
        var stranger = new UserBuilder().Build();
        await context.AddAsync(stranger);
        await context.SaveChangesAsync();

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = stranger.Id,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SellerInitiates_SetsRecipientToBuyer()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var command = new RequestContractCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.SenderId.Should().Be(seller.Id);
        result.RecipientId.Should().Be(buyer.Id);
        result.SenderUsername.Should().Be(seller.Username);
        result.ContractCard.InitiatorId.Should().Be(seller.Id);
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
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m)
            .WithMunicipality(municipality.Id).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}
