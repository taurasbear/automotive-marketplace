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
                },
                Meeting = m.Meeting is null ? null : new GetMessagesResponse.Message.MeetingData
                {
                    Id = m.Meeting.Id,
                    ProposedAt = m.Meeting.ProposedAt,
                    DurationMinutes = m.Meeting.DurationMinutes,
                    LocationText = m.Meeting.LocationText,
                    LocationLat = m.Meeting.LocationLat,
                    LocationLng = m.Meeting.LocationLng,
                    Status = m.Meeting.Status,
                    ExpiresAt = m.Meeting.ExpiresAt,
                    InitiatorId = m.Meeting.InitiatorId,
                    ParentMeetingId = m.Meeting.ParentMeetingId
                },
                AvailabilityCard = m.AvailabilityCard is null ? null : new GetMessagesResponse.Message.AvailabilityCardData
                {
                    Id = m.AvailabilityCard.Id,
                    Status = m.AvailabilityCard.Status,
                    ExpiresAt = m.AvailabilityCard.ExpiresAt,
                    InitiatorId = m.AvailabilityCard.InitiatorId,
                    Slots = m.AvailabilityCard.Slots.Select(s => new GetMessagesResponse.Message.AvailabilityCardData.SlotData
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToList()
                },
                ContractCard = m.ContractCard is null ? null : new GetMessagesResponse.Message.ContractCardData
                {
                    Id = m.ContractCard.Id,
                    Status = m.ContractCard.Status,
                    InitiatorId = m.ContractCard.InitiatorId,
                    AcceptedAt = m.ContractCard.AcceptedAt,
                    CreatedAt = m.ContractCard.CreatedAt,
                    SellerSubmittedAt = m.ContractCard.SellerSubmission != null
                        ? m.ContractCard.SellerSubmission.SubmittedAt
                        : null,
                    BuyerSubmittedAt = m.ContractCard.BuyerSubmission != null
                        ? m.ContractCard.BuyerSubmission.SubmittedAt
                        : null,
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
