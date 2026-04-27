using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;
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

public class SubmitContractSellerFormCommandHandlerTests(
    DatabaseFixture<SubmitContractSellerFormCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<SubmitContractSellerFormCommandHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private SubmitContractSellerFormCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static SubmitContractSellerFormCommand BuildSellerCommand(Guid contractCardId, Guid sellerId)
        => new()
        {
            ContractCardId = contractCardId,
            SellerId = sellerId,
            Make = "Toyota",
            CommercialName = "Corolla",
            RegistrationNumber = "ABC123",
            Mileage = 50000,
            Vin = "JT2BF22K1W0037674",
            TechnicalInspectionValid = true,
            WasDamaged = false,
            DamageKnown = null,
            DefectBrakes = false,
            DefectSafety = false,
            DefectSteering = false,
            DefectExhaust = false,
            DefectLighting = false,
            Price = 12000m,
            PersonalIdCode = "38901011234",
            FullName = "Jonas Jonaitis",
            Phone = "+37060000001",
            Email = "seller@example.com",
            Address = "Gedimino pr. 1, Vilnius",
            Country = "Lietuva",
            UpdateProfile = false,
        };

    [Fact]
    public async Task Handle_SellerSubmitsOnActiveCard_TransitionsToSellerSubmitted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);

        var command = BuildSellerCommand(card.Id, seller.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.SellerSubmitted);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(ContractCardStatus.SellerSubmitted);

        var savedSubmission = await context.ContractSellerSubmissions
            .FirstOrDefaultAsync(s => s.ContractCardId == card.Id);
        savedSubmission.Should().NotBeNull();
        savedSubmission!.Make.Should().Be("Toyota");
    }

    [Fact]
    public async Task Handle_SellerSubmitsWhenBuyerAlreadySubmitted_TransitionsToComplete()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, _, card) = await SeedActiveCardAsync(context);

        // Seed buyer submission first
        card.Status = ContractCardStatus.BuyerSubmitted;
        var buyerSub = new ContractBuyerSubmissionBuilder()
            .WithContractCard(card.Id)
            .Build();
        context.Update(card);
        await context.AddAsync(buyerSub);
        await context.SaveChangesAsync();

        var command = BuildSellerCommand(card.Id, seller.Id);
        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(ContractCardStatus.Complete);
    }

    [Fact]
    public async Task Handle_BuyerAttemptsSellerForm_ThrowsUnauthorized()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card) = await SeedActiveCardAsync(context);
        var command = BuildSellerCommand(card.Id, buyer.Id); // buyer not allowed

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CardNotActive_ThrowsValidationException()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card) = await SeedActiveCardAsync(context);
        card.Status = ContractCardStatus.Pending;
        context.Update(card);
        await context.SaveChangesAsync();

        var command = BuildSellerCommand(card.Id, seller.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ContractCardId"));
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
