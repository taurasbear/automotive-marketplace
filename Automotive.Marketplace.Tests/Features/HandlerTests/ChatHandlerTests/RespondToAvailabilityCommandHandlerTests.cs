using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;
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

public class RespondToAvailabilityCommandHandlerTests(
    DatabaseFixture<RespondToAvailabilityCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToAvailabilityCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToAvailabilityCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToAvailabilityCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_PickSlot_ShouldCreateMeetingAndMarkCardResponded()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        result.Action.Should().Be(AvailabilityResponseAction.PickSlot);
        result.PickedSlotMeeting.Should().NotBeNull();
        result.PickedSlotMeeting!.Meeting.ProposedAt.Should().Be(slot.StartTime);
        result.PickedSlotMeeting.Meeting.Status.Should().Be(MeetingStatus.Pending);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(AvailabilityCardStatus.Responded);

        var newMeeting = await context.Meetings.FindAsync(result.PickedSlotMeeting.Meeting.Id);
        newMeeting.Should().NotBeNull();
        newMeeting!.Status.Should().Be(MeetingStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.PickedSlotMeeting.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Meeting);
    }

    [Fact]
    public async Task Handle_ShareBack_ShouldCreateNewCardAndMarkOldCardResponded()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var newStart = DateTime.UtcNow.AddDays(4);
        var result = await handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.ShareBack,
            ShareBackSlots =
            [
                new RespondToAvailabilityCommand.ShareBackSlot
                {
                    StartTime = newStart,
                    EndTime = newStart.AddHours(2)
                }
            ]
        }, CancellationToken.None);

        result.Action.Should().Be(AvailabilityResponseAction.ShareBack);
        result.SharedBackAvailability.Should().NotBeNull();
        result.SharedBackAvailability!.AvailabilityCard.Slots.Should().HaveCount(1);

        await context.Entry(card).ReloadAsync();
        card.Status.Should().Be(AvailabilityCardStatus.Responded);

        var newCard = await context.AvailabilityCards.FindAsync(result.SharedBackAvailability.AvailabilityCard.Id);
        newCard.Should().NotBeNull();
        newCard!.Status.Should().Be(AvailabilityCardStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.SharedBackAvailability.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Availability);
    }

    [Fact]
    public async Task Handle_CardAlreadyResponded_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);
        card.Status = AvailabilityCardStatus.Responded;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnCard_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NonParticipantResponder_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, card, slot) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);
        var outsider = new UserBuilder().Build();
        await context.AddAsync(outsider);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = outsider.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = slot.Id
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PickSlotWithInvalidSlotId_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = seller.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = Guid.NewGuid()
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_PickSlotWithCustomStartTimeAndDuration_ShouldUseProvidedValues()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: false);

        var slotStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var slotEnd = slotStart.AddHours(4);
        var customSlot = new AvailabilitySlotBuilder()
            .WithCard(card.Id)
            .WithTimes(slotStart, slotEnd)
            .Build();
        await context.AddAsync(customSlot);
        await context.SaveChangesAsync();

        var customStart = slotStart.AddHours(1);
        var command = new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = customSlot.Id,
            StartTime = customStart,
            DurationMinutes = 30
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.PickedSlotMeeting.Should().NotBeNull();
        result.PickedSlotMeeting!.Meeting.ProposedAt.Should().Be(customStart);
        result.PickedSlotMeeting.Meeting.DurationMinutes.Should().Be(30);
    }

    [Fact]
    public async Task Handle_PickSlotWithStartTimeOutsideRange_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, card, _) = await SeedPendingAvailabilityAsync(context, initiatedByBuyer: false);

        var slotStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var slotEnd = slotStart.AddHours(2);
        var customSlot = new AvailabilitySlotBuilder()
            .WithCard(card.Id)
            .WithTimes(slotStart, slotEnd)
            .Build();
        await context.AddAsync(customSlot);
        await context.SaveChangesAsync();

        var command = new RespondToAvailabilityCommand
        {
            AvailabilityCardId = card.Id,
            ResponderId = buyer.Id,
            Action = AvailabilityResponseAction.PickSlot,
            SlotId = customSlot.Id,
            StartTime = slotStart.AddHours(-1),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("StartTime"));
    }

    private static async Task<(User buyer, User seller, Conversation conversation, AvailabilityCard card, AvailabilitySlot slot)>
        SeedPendingAvailabilityAsync(AutomotiveContext context, bool initiatedByBuyer)
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

        var initiatorId = initiatedByBuyer ? buyer.Id : seller.Id;
        var card = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .Build();

        var slot = new AvailabilitySlotBuilder()
            .WithCard(card.Id)
            .WithTimes(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1))
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, municipality, variant, listing, conversation, card, slot);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, card, slot);
    }
}