using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;
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

public class RespondToMeetingCommandHandlerTests(
    DatabaseFixture<RespondToMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RespondToMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RespondToMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RespondToMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_AcceptMeeting_ShouldSetStatusAccepted()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = (await context.Conversations.FindAsync(meeting.ConversationId))!.Listing.SellerId,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Accepted);
        result.RescheduledMeeting.Should().BeNull();

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Accepted);
    }

    [Fact]
    public async Task Handle_DeclineMeeting_ShouldSetStatusDeclined()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Decline
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Declined);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Declined);
    }

    [Fact]
    public async Task Handle_RescheduleMeeting_ShouldCreateNewMeetingAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var newTime = DateTime.UtcNow.AddDays(5);
        var result = await handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Reschedule,
            Reschedule = new RespondToMeetingCommand.RescheduleData
            {
                ProposedAt = newTime,
                DurationMinutes = 90,
                LocationText = "New location"
            }
        }, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Rescheduled);
        result.RescheduledMeeting.Should().NotBeNull();
        result.RescheduledMeeting!.Meeting.ProposedAt.Should().Be(newTime);
        result.RescheduledMeeting.Meeting.DurationMinutes.Should().Be(90);
        result.RescheduledMeeting.Meeting.ParentMeetingId.Should().Be(meeting.Id);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Rescheduled);

        var newMeeting = await context.Meetings.FindAsync(result.RescheduledMeeting.Meeting.Id);
        newMeeting.Should().NotBeNull();
        newMeeting!.Status.Should().Be(MeetingStatus.Pending);

        var newMessage = await context.Messages.FindAsync(result.RescheduledMeeting.MessageId);
        newMessage.Should().NotBeNull();
        newMessage!.MessageType.Should().Be(MessageType.Meeting);
        newMessage.MeetingId.Should().Be(newMeeting.Id);
    }

    [Fact]
    public async Task Handle_MeetingAlreadyResolved_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        meeting.Status = MeetingStatus.Accepted;
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Decline
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_InitiatorRespondsToOwnMeeting_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = buyer.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_MeetingExpired_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        meeting.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_RescheduleWithPastTime_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = seller.Id,
            Action = MeetingResponseAction.Reschedule,
            Reschedule = new RespondToMeetingCommand.RescheduleData
            {
                ProposedAt = DateTime.UtcNow.AddHours(-1),
                DurationMinutes = 60
            }
        }, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_NonParticipantResponder_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, _, meeting, _) = await SeedPendingMeetingAsync(context, initiatedByBuyer: true);
        var outsider = new UserBuilder().Build();
        await context.AddAsync(outsider);
        await context.SaveChangesAsync();

        var act = () => handler.Handle(new RespondToMeetingCommand
        {
            MeetingId = meeting.Id,
            ResponderId = outsider.Id,
            Action = MeetingResponseAction.Accept
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static async Task<(User buyer, User seller, Conversation conversation, Meeting meeting, Listing listing)>
        SeedPendingMeetingAsync(AutomotiveContext context, bool initiatedByBuyer)
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

        var initiatorId = initiatedByBuyer ? buyer.Id : seller.Id;
        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(initiatorId)
            .Build();

        await context.AddRangeAsync(seller, buyer, make, model, fuel,
            transmission, bodyType, drivetrain, variant, listing, conversation, meeting);
        await context.SaveChangesAsync();

        return (buyer, seller, conversation, meeting, listing);
    }
}