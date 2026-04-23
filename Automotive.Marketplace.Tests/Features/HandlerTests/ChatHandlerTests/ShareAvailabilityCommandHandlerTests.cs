using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
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

public class ShareAvailabilityCommandHandlerTests(
    DatabaseFixture<ShareAvailabilityCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ShareAvailabilityCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ShareAvailabilityCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private ShareAvailabilityCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerSharesValidSlots_ShouldPersistCardSlotsAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var start1 = DateTime.UtcNow.AddDays(2);
        var end1 = start1.AddHours(2);
        var start2 = DateTime.UtcNow.AddDays(3);
        var end2 = start2.AddHours(1);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData { StartTime = start1, EndTime = end1 },
                new ShareAvailabilityCommand.SlotData { StartTime = start2, EndTime = end2 }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.AvailabilityCard.Id.Should().NotBeEmpty();
        result.AvailabilityCard.Status.Should().Be(AvailabilityCardStatus.Pending);
        result.AvailabilityCard.Slots.Should().HaveCount(2);
        result.SenderId.Should().Be(buyer.Id);

        var savedCard = await context.AvailabilityCards.FindAsync(result.AvailabilityCard.Id);
        savedCard.Should().NotBeNull();
        savedCard!.Status.Should().Be(AvailabilityCardStatus.Pending);

        var savedSlots = await context.AvailabilitySlots
            .Where(s => s.AvailabilityCardId == savedCard.Id)
            .ToListAsync();
        savedSlots.Should().HaveCount(2);

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Availability);
        savedMessage.AvailabilityCardId.Should().Be(savedCard.Id);
    }

    [Fact]
    public async Task Handle_NoSlots_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots = []
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots"));
    }

    [Fact]
    public async Task Handle_PastSlotStartTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(1)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots.StartTime"));
    }

    [Fact]
    public async Task Handle_EndTimeBeforeStartTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var start = DateTime.UtcNow.AddDays(2);
        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = start,
                    EndTime = start.AddMinutes(-30)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("Slots.EndTime"));
    }

    [Fact]
    public async Task Handle_ActiveMeetingNegotiationExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingMeeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(existingMeeting);
        await context.SaveChangesAsync();

        var command = new ShareAvailabilityCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            Slots =
            [
                new ShareAvailabilityCommand.SlotData
                {
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(1)
                }
            ]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
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
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id)
            .WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id)
            .WithDrivetrain(drivetrain.Id).WithPrice(15000m).Build();
        var conversation = new ConversationBuilder()
            .WithBuyer(buyer.Id).WithListing(listing.Id).Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, listing);
    }
}