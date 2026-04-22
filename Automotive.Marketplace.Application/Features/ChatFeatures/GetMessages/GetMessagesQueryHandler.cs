using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesQueryHandler(IRepository repository)
    : IRequestHandler<GetMessagesQuery, GetMessagesResponse>
{
    public async Task<GetMessagesResponse> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var isBuyer = conversation.BuyerId == request.UserId;
        var isSeller = conversation.Listing.SellerId == request.UserId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException(
                "You are not a participant in this conversation.");

        var listingPrice = conversation.Listing.Price;

        var messages = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new GetMessagesResponse.Message
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.Sender.Username,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                MessageType = m.MessageType,
                Offer = m.Offer is null ? null : new GetMessagesResponse.Message.OfferData
                {
                    Id = m.Offer.Id,
                    Amount = m.Offer.Amount,
                    ListingPrice = listingPrice,
                    PercentageOff = Math.Round(
                        (listingPrice - m.Offer.Amount) / listingPrice * 100, 2),
                    Status = m.Offer.Status,
                    ExpiresAt = m.Offer.ExpiresAt,
                    InitiatorId = m.Offer.InitiatorId,
                    ParentOfferId = m.Offer.ParentOfferId
                }
            })
            .ToList();

        return new GetMessagesResponse
        {
            ConversationId = request.ConversationId,
            Messages = messages
        };
    }
}
