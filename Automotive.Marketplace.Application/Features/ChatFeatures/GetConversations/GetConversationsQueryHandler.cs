using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public class GetConversationsQueryHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<GetConversationsQuery, IEnumerable<ConversationSummaryResponse>>
{
    public async Task<IEnumerable<ConversationSummaryResponse>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await repository.AsQueryable<Conversation>()
            .Where(c => c.BuyerId == request.UserId || c.Listing.SellerId == request.UserId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);

        var result = new List<ConversationSummaryResponse>();

        foreach (var conversation in conversations)
        {
            var listing = conversation.Listing;
            var variant = listing.Variant;
            var counterpart = conversation.BuyerId == request.UserId
                ? listing.Seller
                : conversation.Buyer;

            var lastMessage = conversation.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = conversation.Messages
                .Count(m => m.SenderId != request.UserId && !m.IsRead);

            string? thumbnailUrl = null;
            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage is not null)
                thumbnailUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey);

            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);

            result.Add(new ConversationSummaryResponse
            {
                Id = conversation.Id,
                ListingId = listing.Id,
                ListingTitle = $"{listing.Year} {variant.Model.Make.Name} {variant.Model.Name}",
                ListingThumbnail = thumbnailUrl is not null
                    ? new Automotive.Marketplace.Application.Models.ImageDto
                      {
                          Url = thumbnailUrl,
                          AltText = firstImage!.AltText
                      }
                    : null,
                ListingPrice = listing.Price,
                CounterpartId = counterpart.Id,
                CounterpartUsername = counterpart.Username,
                LastMessage = lastMessage?.Content,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount,
                BuyerId = conversation.BuyerId,
                SellerId = listing.SellerId,
                BuyerHasLiked = buyerHasLiked
            });
        }

        return result;
    }
}
