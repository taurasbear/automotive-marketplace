using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public class GetConversationsQuery : IRequest<IEnumerable<ConversationSummaryResponse>>
{
    public Guid UserId { get; set; }
}
