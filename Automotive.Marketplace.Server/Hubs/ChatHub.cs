using System.Security.Claims;
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
}
