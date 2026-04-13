using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public sealed record SendMessageCommand : IRequest<SendMessageResponse>
{
    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;
}
