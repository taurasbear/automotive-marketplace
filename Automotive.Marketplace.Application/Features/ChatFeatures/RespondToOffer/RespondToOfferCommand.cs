using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public sealed record RespondToOfferCommand : IRequest<RespondToOfferResponse>
{
    public Guid OfferId { get; set; }

    public Guid ResponderId { get; set; }

    public OfferResponseAction Action { get; set; }

    public decimal? CounterAmount { get; set; }
}

public enum OfferResponseAction
{
    Accept,
    Decline,
    Counter
}
