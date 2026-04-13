using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUnreadCount;

public class GetUnreadCountQueryHandler(IRepository repository)
    : IRequestHandler<GetUnreadCountQuery, GetUnreadCountResponse>
{
    public async Task<GetUnreadCountResponse> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await repository.AsQueryable<Message>()
            .Where(m => !m.IsRead
                && m.SenderId != request.UserId
                && (m.Conversation.BuyerId == request.UserId
                    || m.Conversation.Listing.SellerId == request.UserId))
            .CountAsync(cancellationToken);

        return new GetUnreadCountResponse { UnreadCount = count };
    }
}
