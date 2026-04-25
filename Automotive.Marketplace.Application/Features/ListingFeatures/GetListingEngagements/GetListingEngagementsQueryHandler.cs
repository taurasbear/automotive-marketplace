using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public class GetListingEngagementsQueryHandler(IRepository repository)
    : IRequestHandler<GetListingEngagementsQuery, GetListingEngagementsResponse>
{
    public async Task<GetListingEngagementsResponse> Handle(
        GetListingEngagementsQuery request,
        CancellationToken cancellationToken)
    {
        var listing = await repository
            .AsQueryable<Listing>()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), request.ListingId);

        if (listing.SellerId != request.CurrentUserId)
            throw new UnauthorizedAccessException("You are not the seller of this listing.");

        var conversations = await repository
            .AsQueryable<Conversation>()
            .Include(c => c.Messages)
            .Include(c => c.Buyer)
            .Where(c => c.ListingId == request.ListingId)
            .ToListAsync(cancellationToken);

        var buyerIds = conversations.Select(c => c.BuyerId).ToHashSet();

        var likers = await repository
            .AsQueryable<UserListingLike>()
            .Include(l => l.User)
            .Where(l => l.ListingId == request.ListingId && !buyerIds.Contains(l.UserId))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var conversationDtos = conversations
            .Select(c =>
            {
                var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                return new GetListingEngagementsResponse.Conversation
                {
                    ConversationId = c.Id,
                    BuyerId = c.BuyerId,
                    BuyerUsername = c.Buyer.Username,
                    LastMessageType = lastMsg?.MessageType.ToString() ?? "Text",
                    LastInteractionAt = c.LastMessageAt,
                };
            })
            .OrderByDescending(c => c.LastInteractionAt)
            .ToList();

        var likerDtos = likers
            .Select(l => new GetListingEngagementsResponse.Liker
            {
                UserId = l.UserId,
                Username = l.User.Username,
                LikedAt = l.CreatedAt,
            })
            .ToList();

        return new GetListingEngagementsResponse
        {
            Conversations = conversationDtos,
            Likers = likerDtos,
        };
    }
}
