using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public sealed record MakeOfferCommand : IRequest<MakeOfferResponse>
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public decimal Amount { get; set; }
}
