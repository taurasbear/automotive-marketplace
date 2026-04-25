using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversationAsSeller;

public sealed record GetOrCreateConversationAsSellerCommand : IRequest<GetOrCreateConversationAsSellerResponse>
{
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
}
