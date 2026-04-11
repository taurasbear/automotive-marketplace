using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public sealed record GetConversationsQuery : IRequest<IEnumerable<ConversationSummaryResponse>>
{
    public Guid UserId { get; set; }
}
