using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;
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

public class CancelContractCommandHandlerTests(
    DatabaseFixture<CancelContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CancelContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private CancelContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_InitiatorCancelsPending_TransitionsToCancelled()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedCardAsync(context, ContractCardStatus.Pending, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = buyer.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Cancelled);
        result.RecipientId.Should().Be(seller.Id);
        result.InitiatorId.Should().Be(buyer.Id);
        result.ConversationId.Should().Be(conversation.Id);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_CancelActiveCard_TransitionsToCancelled()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedCardAsync(context, ContractCardStatus.Active, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = buyer.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Cancelled);
        result.RecipientId.Should().Be(seller.Id);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_NonInitiatorParticipantCancels_TransitionsToCancelled()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedCardAsync(context, ContractCardStatus.Pending, initiatorIsBuyer: true);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = seller.Id, // seller is not the initiator but is a participant
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Cancelled);
        result.RecipientId.Should().Be(seller.Id);
        result.InitiatorId.Should().Be(buyer.Id);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_SellerInitiatorCancelsPending_SetsRecipientToBuyer()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedCardAsync(context, ContractCardStatus.Pending, initiatorIsBuyer: false);

        var command = new CancelContractCommand
        {
            ContractCardId = card.Id,
            RequesterId = seller.Id,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Cancelled);
        result.InitiatorId.Should().Be(seller.Id);
        result.RecipientId.Should().Be(buyer.Id);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Cancelled);
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedCardAsync(AutomotiveContext context, ContractCardStatus status, bool initiatorIsBuyer)
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
        var card = new ContractCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorIsBuyer ? buyer.Id : seller.Id)
            .WithStatus(status)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
