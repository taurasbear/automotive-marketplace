using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;

public sealed record GetOrCreateConversationCommand : IRequest<GetOrCreateConversationResponse>
{
    public Guid BuyerId { get; set; }

    public Guid ListingId { get; set; }
}
