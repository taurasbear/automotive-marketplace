using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public class SendMessageCommandHandler(IRepository repository)
    : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    public async Task<SendMessageResponse> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var recipientId = conversation.BuyerId == request.SenderId
            ? conversation.Listing.SellerId
            : conversation.BuyerId;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.SenderId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        var recipientUnreadCount = await repository.AsQueryable<Message>()
            .Where(m => !m.IsRead
                && m.SenderId != recipientId
                && (m.Conversation.BuyerId == recipientId
                    || m.Conversation.Listing.SellerId == recipientId))
            .CountAsync(cancellationToken);

        return new SendMessageResponse
        {
            Id = message.Id,
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderUsername = conversation.Buyer.Id == request.SenderId
                ? conversation.Buyer.Username
                : conversation.Listing.Seller.Username,
            Content = request.Content,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            RecipientUnreadCount = recipientUnreadCount
        };
    }
}
