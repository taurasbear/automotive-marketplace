using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;
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

public class RespondToContractCommandHandlerTests(
    DatabaseFixture<RespondToContractCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToContractCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private RespondToContractCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_RecipientAccepts_TransitionsToActive()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        // buyer initiated, seller responds
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Accept,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Active);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Active);
        card.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_RecipientDeclines_TransitionsToDeclined()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Decline,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Declined);
        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.Declined);
        card.AcceptedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InitiatorResponds_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = buyer.Id, // buyer is initiator — cannot respond
            Action = ContractResponseAction.Accept,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CardNotPending_ThrowsValidationException()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        card.Status = ContractCardStatus.Active;
        context.Update(card);
        await context.SaveChangesAsync();

        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = seller.Id,
            Action = ContractResponseAction.Accept,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ContractCardId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyResponds_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, card) = await SeedPendingCardAsync(context, initiatorId: b => b.Id);
        var stranger = new UserBuilder().Build();
        await context.AddAsync(stranger);
        await context.SaveChangesAsync();

        var command = new RespondToContractCommand
        {
            ContractCardId = card.Id,
            ResponderId = stranger.Id,
            Action = ContractResponseAction.Accept,
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedPendingCardAsync(AutomotiveContext context, Func<User, Guid> initiatorId)
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
            .WithInitiator(initiatorId(buyer))
            .WithStatus(ContractCardStatus.Pending)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
