using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public class MarkMessagesReadCommand : IRequest<MarkMessagesReadResponse>
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }
}
