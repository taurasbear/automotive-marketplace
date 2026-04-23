using System.Security.Claims;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;
using Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Automotive.Marketplace.Server.Hubs;

[Authorize]
public class ChatHub(IMediator mediator) : Hub
{
    private Guid UserId =>
        Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{UserId}");
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var result = await mediator.Send(new SendMessageCommand
        {
            ConversationId = conversationId,
            SenderId = UserId,
            Content = content
        });

        await Clients.Group($"user-{UserId}").SendAsync("ReceiveMessage", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("ReceiveMessage", result);
        await Clients.Group($"user-{result.RecipientId}")
            .SendAsync("UpdateUnreadCount", result.RecipientUnreadCount);
    }

    public async Task MakeOffer(Guid conversationId, decimal amount)
    {
        var result = await mediator.Send(new MakeOfferCommand
        {
            ConversationId = conversationId,
            InitiatorId = UserId,
            Amount = amount
        });

        await Clients.Group($"user-{UserId}").SendAsync("OfferMade", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("OfferMade", result);
    }

    public async Task RespondToOffer(Guid offerId, string action, decimal? counterAmount = null)
    {
        var result = await mediator.Send(new RespondToOfferCommand
        {
            OfferId = offerId,
            ResponderId = UserId,
            Action = Enum.Parse<OfferResponseAction>(action, ignoreCase: true),
            CounterAmount = counterAmount
        });

        var eventName = result.NewStatus switch
        {
            Domain.Enums.OfferStatus.Accepted => "OfferAccepted",
            Domain.Enums.OfferStatus.Declined => "OfferDeclined",
            Domain.Enums.OfferStatus.Countered => "OfferCountered",
            _ => throw new InvalidOperationException($"Unexpected offer status: {result.NewStatus}")
        };

        await Clients.Group($"user-{result.InitiatorId}").SendAsync(eventName, result);
        await Clients.Group($"user-{result.ResponderId}").SendAsync(eventName, result);
    }

    public async Task ProposeMeeting(
        Guid conversationId, DateTime proposedAt, int durationMinutes,
        string? locationText = null, decimal? locationLat = null, decimal? locationLng = null)
    {
        var result = await mediator.Send(new ProposeMeetingCommand
        {
            ConversationId = conversationId,
            InitiatorId = UserId,
            ProposedAt = proposedAt,
            DurationMinutes = durationMinutes,
            LocationText = locationText,
            LocationLat = locationLat,
            LocationLng = locationLng
        });

        await Clients.Group($"user-{UserId}").SendAsync("MeetingProposed", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("MeetingProposed", result);
    }

    public async Task RespondToMeeting(Guid meetingId, string action,
        RespondToMeetingCommand.RescheduleData? rescheduleData = null)
    {
        var result = await mediator.Send(new RespondToMeetingCommand
        {
            MeetingId = meetingId,
            ResponderId = UserId,
            Action = Enum.Parse<MeetingResponseAction>(action, ignoreCase: true),
            Reschedule = rescheduleData
        });

        var eventName = result.NewStatus switch
        {
            Domain.Enums.MeetingStatus.Accepted => "MeetingAccepted",
            Domain.Enums.MeetingStatus.Declined => "MeetingDeclined",
            Domain.Enums.MeetingStatus.Rescheduled => "MeetingRescheduled",
            _ => throw new InvalidOperationException($"Unexpected meeting status: {result.NewStatus}")
        };

        await Clients.Group($"user-{result.InitiatorId}").SendAsync(eventName, result);
        await Clients.Group($"user-{result.ResponderId}").SendAsync(eventName, result);
    }

    public async Task ShareAvailability(Guid conversationId,
        List<ShareAvailabilityCommand.SlotData> slots)
    {
        var result = await mediator.Send(new ShareAvailabilityCommand
        {
            ConversationId = conversationId,
            InitiatorId = UserId,
            Slots = slots
        });

        await Clients.Group($"user-{UserId}").SendAsync("AvailabilityShared", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("AvailabilityShared", result);
    }

    public async Task RespondToAvailability(Guid availabilityCardId, string action,
        Guid? slotId = null, List<RespondToAvailabilityCommand.ShareBackSlot>? shareBackSlots = null)
    {
        var result = await mediator.Send(new RespondToAvailabilityCommand
        {
            AvailabilityCardId = availabilityCardId,
            ResponderId = UserId,
            Action = Enum.Parse<AvailabilityResponseAction>(action, ignoreCase: true),
            SlotId = slotId,
            ShareBackSlots = shareBackSlots
        });

        var initiatorId = result.Action == AvailabilityResponseAction.PickSlot
            ? result.PickedSlotMeeting!.RecipientId
            : result.SharedBackAvailability!.RecipientId;

        await Clients.Group($"user-{UserId}").SendAsync("AvailabilityResponded", result);
        await Clients.Group($"user-{initiatorId}").SendAsync("AvailabilityResponded", result);
    }

    public async Task CancelMeeting(Guid meetingId)
    {
        var result = await mediator.Send(new CancelMeetingCommand
        {
            MeetingId = meetingId,
            CancellerId = UserId
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("MeetingCancelled", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("MeetingCancelled", result);
    }

    public async Task CancelAvailability(Guid availabilityCardId)
    {
        var result = await mediator.Send(new CancelAvailabilityCommand
        {
            AvailabilityCardId = availabilityCardId,
            CancellerId = UserId
        });

        await Clients.Group($"user-{result.InitiatorId}").SendAsync("AvailabilityCancelled", result);
        await Clients.Group($"user-{result.RecipientId}").SendAsync("AvailabilityCancelled", result);
    }
}
