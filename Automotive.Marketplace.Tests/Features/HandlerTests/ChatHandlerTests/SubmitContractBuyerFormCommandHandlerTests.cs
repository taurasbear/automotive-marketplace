using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;
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

public class SubmitContractBuyerFormCommandHandlerTests(
    DatabaseFixture<SubmitContractBuyerFormCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SubmitContractBuyerFormCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private SubmitContractBuyerFormCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static SubmitContractBuyerFormCommand BuildBuyerCommand(Guid contractCardId, Guid buyerId)
        => new()
        {
            ContractCardId = contractCardId,
            BuyerId = buyerId,
            PersonalIdCode = "49001011234",
            FullName = "Petras Petraitis",
            Phone = "+37060000002",
            Email = "buyer@example.com",
            Address = "Laisvės al. 5, Kaunas",
            UpdateProfile = false,
        };

    [Fact]
    public async Task Handle_BuyerSubmitsOnActiveCard_TransitionsToBuyerSubmitted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedActiveCardAsync(context);

        var command = BuildBuyerCommand(card.Id, buyer.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.BuyerSubmitted);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.BuyerSubmitted);

        var savedSub = await context.ContractBuyerSubmissions
            .FirstOrDefaultAsync(b => b.ContractCardId == card.Id);
        savedSub.Should().NotBeNull();
        savedSub!.FullName.Should().Be("Petras Petraitis");
    }

    [Fact]
    public async Task Handle_BuyerSubmitsWhenSellerAlreadySubmitted_TransitionsToComplete()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, _, card) = await SeedActiveCardAsync(context);

        card.Status = ContractCardStatus.SellerSubmitted;
        var sellerSub = new ContractSellerSubmissionBuilder()
            .WithContractCard(card.Id)
            .Build();
        context.Update(card);
        await context.AddAsync(sellerSub);
        await context.SaveChangesAsync();

        var command = BuildBuyerCommand(card.Id, buyer.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Complete);
    }

    [Fact]
    public async Task Handle_SellerAttemptsBuyerForm_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);
        var command = BuildBuyerCommand(card.Id, seller.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, ContractCard card)>
        SeedActiveCardAsync(AutomotiveContext context)
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
            .WithInitiator(buyer.Id)
            .WithStatus(ContractCardStatus.Active)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card);
    }
}
