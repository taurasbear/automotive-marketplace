using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public class MarkMessagesReadCommandHandler(IRepository repository)
    : IRequestHandler<MarkMessagesReadCommand, MarkMessagesReadResponse>
{
    public async Task<MarkMessagesReadResponse> Handle(
        MarkMessagesReadCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var unreadMessages = conversation.Messages
            .Where(m => m.SenderId != request.UserId && !m.IsRead)
            .ToList();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await repository.SaveChangesAsync(cancellationToken);

        var totalUnreadCount = await repository.AsQueryable<Message>()
            .Where(m => !m.IsRead
                && m.SenderId != request.UserId
                && (m.Conversation.BuyerId == request.UserId
                    || m.Conversation.Listing.SellerId == request.UserId))
            .CountAsync(cancellationToken);

        return new MarkMessagesReadResponse { TotalUnreadCount = totalUnreadCount };
    }
}
