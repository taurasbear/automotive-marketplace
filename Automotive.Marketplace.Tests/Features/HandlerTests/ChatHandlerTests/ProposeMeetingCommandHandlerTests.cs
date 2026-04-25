using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
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

public class ProposeMeetingCommandHandlerTests(
    DatabaseFixture<ProposeMeetingCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ProposeMeetingCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ProposeMeetingCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private ProposeMeetingCommandHandler CreateHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_BuyerProposesValidMeeting_ShouldPersistMeetingAndMessage()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var proposedAt = DateTime.UtcNow.AddDays(3);
        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = proposedAt,
            DurationMinutes = 60,
            LocationText = "Central Park"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.MessageId.Should().NotBeEmpty();
        result.Meeting.Id.Should().NotBeEmpty();
        result.Meeting.ProposedAt.Should().Be(proposedAt);
        result.Meeting.DurationMinutes.Should().Be(60);
        result.Meeting.LocationText.Should().Be("Central Park");
        result.Meeting.Status.Should().Be(MeetingStatus.Pending);
        result.SenderId.Should().Be(buyer.Id);
        result.RecipientId.Should().NotBe(buyer.Id);

        var savedMeeting = await context.Meetings.FindAsync(result.Meeting.Id);
        savedMeeting.Should().NotBeNull();
        savedMeeting!.Status.Should().Be(MeetingStatus.Pending);
        savedMeeting.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.MessageType.Should().Be(MessageType.Meeting);
        savedMessage.MeetingId.Should().Be(result.Meeting.Id);
    }

    [Fact]
    public async Task Handle_SellerProposesWhenBuyerLiked_ShouldSucceed()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, seller, conversation, listing) = await SeedConversationAsync(context);

        var like = new UserListingLikeBuilder()
            .WithUser(buyer.Id)
            .WithListing(listing.Id)
            .Build();
        await context.AddAsync(like);
        await context.SaveChangesAsync();

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 30
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Meeting.Id.Should().NotBeEmpty();
        result.SenderId.Should().Be(seller.Id);
        result.RecipientId.Should().Be(buyer.Id);
    }

    [Fact]
    public async Task Handle_SellerProposesWithoutBuyerEngagement_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, seller, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = seller.Id,
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task Handle_PastProposedAt_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddHours(-1),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ProposedAt"));
    }

    [Fact]
    public async Task Handle_ActiveMeetingExists_ShouldThrowValidationException()
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

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
    }

    [Fact]
    public async Task Handle_ActiveAvailabilityCardExists_ShouldThrowValidationException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (buyer, _, conversation, _) = await SeedConversationAsync(context);

        var existingCard = new AvailabilityCardBuilder()
            .WithConversation(conversation.Id)
            .WithInitiator(buyer.Id)
            .Build();
        await context.AddAsync(existingCard);
        await context.SaveChangesAsync();

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = buyer.Id,
            ProposedAt = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 60
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<RequestValidationException>()
            .Where(ex => ex.Errors.ContainsKey("ConversationId"));
    }

    [Fact]
    public async Task Handle_ThirdPartyInitiator_ShouldThrowUnauthorized()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (_, _, conversation, _) = await SeedConversationAsync(context);

        var command = new ProposeMeetingCommand
        {
            ConversationId = conversation.Id,
            InitiatorId = Guid.NewGuid(),
            ProposedAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 60
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