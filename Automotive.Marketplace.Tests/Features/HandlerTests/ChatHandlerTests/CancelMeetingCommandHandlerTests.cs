using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;
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

public class CancelMeetingCommandHandlerTests(
    DatabaseFixture<CancelMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CancelMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CancelMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private CancelMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_InitiatorCancelsPendingMeeting_ShouldSetStatusToCancelled()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MeetingId.Should().Be(meeting.Id);
        result.ConversationId.Should().Be(conversation.Id);
        result.NewStatus.Should().Be(MeetingStatus.Cancelled);
        result.InitiatorId.Should().Be(buyer.Id);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_InitiatorCancelsAcceptedMeeting_ShouldSetStatusToCancelled()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Accepted)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_NonInitiatorCancelsPending_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = seller.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ResponderCancelsAcceptedMeeting_ShouldSucceed()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Accepted)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = seller.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(MeetingStatus.Cancelled);

        await context.Entry(meeting).ReloadAsync();
        meeting.Status.Should().Be(MeetingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_MeetingAlreadyDeclined_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .WithStatus(MeetingStatus.Declined)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = buyer.Id
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("MeetingId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyCancels_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var meeting = new MeetingBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(meeting);
        await context.SaveChangesAsync();

        var thirdParty = Guid.NewGuid();
        var command = new CancelMeetingCommand
        {
            MeetingId = meeting.Id,
            CancellerId = thirdParty
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
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
