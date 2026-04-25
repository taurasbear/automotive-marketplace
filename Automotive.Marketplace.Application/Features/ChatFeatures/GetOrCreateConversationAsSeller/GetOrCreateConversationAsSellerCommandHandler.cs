using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversationAsSeller;

public class GetOrCreateConversationAsSellerCommandHandler(IRepository repository)
    : IRequestHandler<GetOrCreateConversationAsSellerCommand, GetOrCreateConversationAsSellerResponse>
{
    public async Task<GetOrCreateConversationAsSellerResponse> Handle(
        GetOrCreateConversationAsSellerCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);

        if (listing.SellerId != request.SellerId)
            throw new UnauthorizedAccessException("You are not the seller of this listing.");

        var buyerHasRelationship = await repository
            .AsQueryable<UserListingLike>()
            .AnyAsync(l => l.UserId == request.BuyerId && l.ListingId == request.ListingId, cancellationToken);

        if (!buyerHasRelationship)
        {
            var existingConversation = await repository
                .AsQueryable<Conversation>()
                .AnyAsync(c => c.BuyerId == request.BuyerId && c.ListingId == request.ListingId, cancellationToken);
            
            if (!existingConversation)
                throw new RequestValidationException(["Buyer has no relationship with this listing."]);
        }

        var existing = await repository.AsQueryable<Conversation>()
            .FirstOrDefaultAsync(
                c => c.BuyerId == request.BuyerId && c.ListingId == request.ListingId,
                cancellationToken);

        if (existing is not null)
            return new GetOrCreateConversationAsSellerResponse { ConversationId = existing.Id };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BuyerId = request.BuyerId,
            ListingId = request.ListingId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            CreatedBy = request.SellerId.ToString()
        };

        await repository.CreateAsync(conversation, cancellationToken);
        return new GetOrCreateConversationAsSellerResponse { ConversationId = conversation.Id };
    }
}
