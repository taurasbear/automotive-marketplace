using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.ReactivateListing;

public class ReactivateListingCommandHandler(IRepository repository)
    : IRequestHandler<ReactivateListingCommand, ReactivateListingResponse>
{
    public async Task<ReactivateListingResponse> Handle(
        ReactivateListingCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);

        var canManage = request.Permissions.Contains(Permission.ManageListings.ToString());
        if (listing.SellerId != request.CurrentUserId && !canManage)
            throw new UnauthorizedAccessException("You can only reactivate your own listings.");

        if (listing.Status != Status.OnHold)
            throw new InvalidOperationException("Only OnHold listings can be reactivated.");

        var acceptedOffers = await repository.AsQueryable<Offer>()
            .Include(o => o.Conversation)
            .Where(o => o.Conversation.ListingId == request.ListingId
                        && o.Status == OfferStatus.Accepted)
            .ToListAsync(cancellationToken);

        var conversationIds = acceptedOffers.Select(o => o.ConversationId).ToList();

        var hasActiveContract = await repository.AsQueryable<ContractCard>()
            .Where(c => conversationIds.Contains(c.ConversationId)
                        && c.Status != ContractCardStatus.Pending
                        && c.Status != ContractCardStatus.Declined
                        && c.Status != ContractCardStatus.Cancelled)
            .AnyAsync(cancellationToken);

        if (hasActiveContract)
            throw new InvalidOperationException(
                "Cannot reactivate: a contract has already been agreed upon for this listing.");

        foreach (var offer in acceptedOffers)
        {
            offer.Status = OfferStatus.Cancelled;
            await repository.UpdateAsync(offer, cancellationToken);
        }

        listing.Status = Status.Available;
        await repository.UpdateAsync(listing, cancellationToken);

        return new ReactivateListingResponse
        {
            ListingId = listing.Id,
            CancelledOffers = acceptedOffers
                .Select(o => new ReactivateListingResponse.CancelledOfferInfo
                {
                    OfferId = o.Id,
                    ConversationId = o.ConversationId,
                    BuyerId = o.Conversation.BuyerId,
                    SellerId = listing.SellerId,
                })
                .ToList(),
        };
    }
}
