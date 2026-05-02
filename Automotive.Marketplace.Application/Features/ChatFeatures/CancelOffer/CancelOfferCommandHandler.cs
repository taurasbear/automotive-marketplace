using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelOffer;

public class CancelOfferCommandHandler(IRepository repository)
    : IRequestHandler<CancelOfferCommand, CancelOfferResponse>
{
    public async Task<CancelOfferResponse> Handle(CancelOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await repository.GetByIdAsync<Offer>(request.OfferId, cancellationToken);
        var conversation = offer.Conversation;
        var listing = conversation.Listing;

        if (offer.InitiatorId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the offer initiator can cancel.");

        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("Only pending offers can be cancelled.");

        offer.Status = OfferStatus.Cancelled;
        await repository.UpdateAsync(offer, cancellationToken);

        var recipientId = offer.InitiatorId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        return new CancelOfferResponse
        {
            OfferId = offer.Id,
            InitiatorId = offer.InitiatorId,
            RecipientId = recipientId,
            ConversationId = offer.ConversationId,
        };
    }
}