using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MakeOffer;

public class MakeOfferCommandHandler(IRepository repository)
    : IRequestHandler<MakeOfferCommand, MakeOfferResponse>
{
    public async Task<MakeOfferResponse> Handle(
        MakeOfferCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);

            if (!buyerHasLiked)
                throw new UnauthorizedAccessException(
                    "Seller can only make an offer if the buyer has liked the listing.");
        }

        if (listing.Status != Status.Available)
            throw new RequestValidationException(
            [
                new ValidationFailure("ListingId", "Offers can only be made on available listings.")
            ]);

        var hasActiveOffer = await repository.AsQueryable<Offer>()
            .AnyAsync(o => o.ConversationId == request.ConversationId
                        && o.Status == OfferStatus.Pending, cancellationToken);

        if (hasActiveOffer)
            throw new RequestValidationException(
            [
                new ValidationFailure("ConversationId", "There is already a pending offer in this conversation.")
            ]);

        if (request.Amount < listing.Price / 3)
            throw new RequestValidationException(
            [
                new ValidationFailure("Amount",
                    $"Offer must be at least {listing.Price / 3:C} (one third of the asking price).")
            ]);

        if (request.Amount > listing.Price)
            throw new RequestValidationException(
            [
                new ValidationFailure("Amount", "Offer cannot exceed the listing price.")
            ]);

        var recipientId = isSeller ? conversation.BuyerId : listing.SellerId;
        var senderUsername = isSeller
            ? listing.Seller.Username
            : conversation.Buyer.Username;

        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Amount = request.Amount,
            Status = OfferStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Offer,
            OfferId = offer.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(offer, cancellationToken);
        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var percentageOff = Math.Round((listing.Price - offer.Amount) / listing.Price * 100, 2);

        return new MakeOfferResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            Offer = new MakeOfferResponse.OfferData
            {
                Id = offer.Id,
                Amount = offer.Amount,
                ListingPrice = listing.Price,
                PercentageOff = percentageOff,
                Status = offer.Status,
                ExpiresAt = offer.ExpiresAt,
                InitiatorId = offer.InitiatorId,
                ParentOfferId = offer.ParentOfferId
            }
        };
    }
}
