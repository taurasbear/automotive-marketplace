using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public sealed record CancelOfferCommand : IRequest<CancelOfferResponse>
{
    public Guid OfferId { get; set; }
    public Guid RequesterId { get; set; }
}