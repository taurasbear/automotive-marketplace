using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public sealed record GetMessagesQuery : IRequest<GetMessagesResponse>
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }
}
