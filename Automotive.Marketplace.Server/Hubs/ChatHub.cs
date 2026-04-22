using System.Security.Claims;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;
using Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;
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
}
