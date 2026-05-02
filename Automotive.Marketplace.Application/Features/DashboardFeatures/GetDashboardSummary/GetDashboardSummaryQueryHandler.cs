using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(IRepository repository)
    : IRequestHandler<GetDashboardSummaryQuery, GetDashboardSummaryResponse>
{
    public async Task<GetDashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.CurrentUserId;

        // Pending offers where user needs to respond (they are NOT the initiator)
        var pendingOffers = await repository.AsQueryable<Offer>()
            .Include(o => o.Conversation)
                .ThenInclude(c => c.Listing)
                    .ThenInclude(l => l.Variant)
                        .ThenInclude(v => v.Model)
                            .ThenInclude(m => m.Make)
            .Include(o => o.Conversation)
                .ThenInclude(c => c.Listing)
                    .ThenInclude(l => l.Seller)
            .Include(o => o.Initiator)
            .Where(o => o.Status == OfferStatus.Pending
                && o.InitiatorId != userId
                && (o.Conversation.BuyerId == userId || o.Conversation.Listing.SellerId == userId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        // Upcoming confirmed meetings
        var upcomingMeetings = await repository.AsQueryable<Meeting>()
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Listing)
                    .ThenInclude(l => l.Variant)
                        .ThenInclude(v => v.Model)
                            .ThenInclude(m => m.Make)
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Listing)
                    .ThenInclude(l => l.Seller)
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Buyer)
            .Where(m => m.Status == MeetingStatus.Accepted
                && m.ProposedAt > DateTime.UtcNow
                && (m.Conversation.BuyerId == userId || m.Conversation.Listing.SellerId == userId))
            .OrderBy(m => m.ProposedAt)
            .ToListAsync(cancellationToken);

        // Contracts needing action
        var actionContracts = await repository.AsQueryable<ContractCard>()
            .Include(c => c.Conversation)
                .ThenInclude(c => c.Listing)
                    .ThenInclude(l => l.Variant)
                        .ThenInclude(v => v.Model)
                            .ThenInclude(m => m.Make)
            .Where(c => (c.Conversation.BuyerId == userId || c.Conversation.Listing.SellerId == userId)
                && (c.Status == ContractCardStatus.Pending
                    || c.Status == ContractCardStatus.Active
                    || c.Status == ContractCardStatus.SellerSubmitted
                    || c.Status == ContractCardStatus.BuyerSubmitted))
            .ToListAsync(cancellationToken);

        // Pending availability requests where user needs to respond
        var pendingAvailability = await repository.AsQueryable<AvailabilityCard>()
            .Include(a => a.Conversation)
                .ThenInclude(c => c.Listing)
            .Where(a => a.Status == AvailabilityCardStatus.Pending
                && a.InitiatorId != userId
                && (a.Conversation.BuyerId == userId || a.Conversation.Listing.SellerId == userId))
            .CountAsync(cancellationToken);

        var newestOffer = pendingOffers.FirstOrDefault();
        var nextMeeting = upcomingMeetings.FirstOrDefault();
        var nextContract = actionContracts.FirstOrDefault();

        // Helper to get counterpart name for meetings
        string? GetMeetingCounterpart(Meeting? meeting)
        {
            if (meeting == null) return null;
            
            var conversation = meeting.Conversation;
            if (conversation.BuyerId == userId)
                return conversation.Listing.Seller.Username;
            else
                return conversation.Buyer.Username;
        }

        // Helper to format vehicle name
        string? FormatVehicleName(Listing? listing)
        {
            if (listing?.Variant?.Model?.Make == null) return null;
            return $"{listing.Variant.Model.Make.Name} {listing.Variant.Model.Name}";
        }

        return new GetDashboardSummaryResponse
        {
            Offers = new GetDashboardSummaryResponse.OfferSummary
            {
                PendingCount = pendingOffers.Count,
                NewestOfferListing = FormatVehicleName(newestOffer?.Conversation?.Listing),
                NewestOfferAmount = newestOffer?.Amount,
                NewestOfferFrom = newestOffer?.Initiator?.Username,
            },
            Meetings = new GetDashboardSummaryResponse.MeetingSummary
            {
                UpcomingCount = upcomingMeetings.Count,
                NextMeetingAt = nextMeeting?.ProposedAt,
                NextMeetingCounterpart = GetMeetingCounterpart(nextMeeting),
                NextMeetingListing = FormatVehicleName(nextMeeting?.Conversation?.Listing),
            },
            Contracts = new GetDashboardSummaryResponse.ContractSummary
            {
                ActionNeededCount = actionContracts.Count,
                NextActionListing = FormatVehicleName(nextContract?.Conversation?.Listing),
                NextActionType = nextContract?.Status.ToString(),
            },
            Availability = new GetDashboardSummaryResponse.AvailabilitySummary
            {
                PendingCount = pendingAvailability,
            },
        };
    }
}