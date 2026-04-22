using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToOffer;

public class RespondToOfferCommandHandler(IRepository repository)
    : IRequestHandler<RespondToOfferCommand, RespondToOfferResponse>
{
    public async Task<RespondToOfferResponse> Handle(
        RespondToOfferCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await repository.GetByIdAsync<Offer>(request.OfferId, cancellationToken);
        var conversation = offer.Conversation;
        var listing = conversation.Listing;

        if (offer.Status != OfferStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("OfferId", "This offer has already been resolved.")
            ]);

        if (offer.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own offer.");

        if (offer.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("OfferId", "This offer has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        if (request.Action == OfferResponseAction.Accept)
        {
            offer.Status = OfferStatus.Accepted;
            listing.Status = Status.OnHold;

            await repository.UpdateAsync(offer, cancellationToken);
            await repository.UpdateAsync(listing, cancellationToken);

            return new RespondToOfferResponse
            {
                OfferId = offer.Id,
                ConversationId = conversation.Id,
                NewStatus = OfferStatus.Accepted,
                InitiatorId = offer.InitiatorId,
                ResponderId = request.ResponderId,
                CounterOffer = null
            };
        }

        if (request.Action == OfferResponseAction.Decline)
        {
            offer.Status = OfferStatus.Declined;
            await repository.UpdateAsync(offer, cancellationToken);

            return new RespondToOfferResponse
            {
                OfferId = offer.Id,
                ConversationId = conversation.Id,
                NewStatus = OfferStatus.Declined,
                InitiatorId = offer.InitiatorId,
                ResponderId = request.ResponderId,
                CounterOffer = null
            };
        }

        // Counter
        var counterAmount = request.CounterAmount!.Value;

        if (counterAmount < listing.Price / 3)
            throw new RequestValidationException(
            [
                new ValidationFailure("CounterAmount",
                    $"Counter offer must be at least {listing.Price / 3:C} (one third of the asking price).")
            ]);

        if (counterAmount > listing.Price)
            throw new RequestValidationException(
            [
                new ValidationFailure("CounterAmount", "Counter offer cannot exceed the listing price.")
            ]);

        offer.Status = OfferStatus.Countered;

        var counterOffer = new Offer
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            Amount = counterAmount,
            Status = OfferStatus.Pending,
            ParentOfferId = offer.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        var counterMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Offer,
            OfferId = counterOffer.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = counterMessage.SentAt;

        await repository.UpdateAsync(offer, cancellationToken);
        await repository.CreateAsync(counterOffer, cancellationToken);
        await repository.CreateAsync(counterMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var percentageOff = Math.Round(
            (listing.Price - counterOffer.Amount) / listing.Price * 100, 2);

        return new RespondToOfferResponse
        {
            OfferId = offer.Id,
            ConversationId = conversation.Id,
            NewStatus = OfferStatus.Countered,
            InitiatorId = offer.InitiatorId,
            ResponderId = request.ResponderId,
            CounterOffer = new MakeOfferResponse
            {
                MessageId = counterMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = counterMessage.SentAt,
                RecipientId = offer.InitiatorId,
                Offer = new MakeOfferResponse.OfferData
                {
                    Id = counterOffer.Id,
                    Amount = counterOffer.Amount,
                    ListingPrice = listing.Price,
                    PercentageOff = percentageOff,
                    Status = OfferStatus.Pending,
                    ExpiresAt = counterOffer.ExpiresAt,
                    InitiatorId = request.ResponderId,
                    ParentOfferId = counterOffer.ParentOfferId
                }
            }
        };
    }
}
