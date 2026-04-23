using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;

public class ProposeMeetingCommandHandler(IRepository repository)
    : IRequestHandler<ProposeMeetingCommand, ProposeMeetingResponse>
{
    public async Task<ProposeMeetingResponse> Handle(
        ProposeMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isSeller && conversation.BuyerId != request.InitiatorId)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may propose a meeting.");

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);
            var buyerHasSentMessage = conversation.Messages.Any(m => m.SenderId == conversation.BuyerId);
            if (!buyerHasLiked && !buyerHasSentMessage)
                throw new RequestValidationException(
                    [new("Conversation", "Seller can only propose a meeting if the buyer has engaged.")]);
        }

        if (request.ProposedAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("ProposedAt", "Proposed meeting time must be in the future.")
            ]);

        var hasActiveNegotiation = await repository.AsQueryable<Meeting>()
            .AnyAsync(m => m.ConversationId == request.ConversationId
                        && m.Status == MeetingStatus.Pending, cancellationToken)
            || await repository.AsQueryable<AvailabilityCard>()
            .AnyAsync(a => a.ConversationId == request.ConversationId
                        && a.Status == AvailabilityCardStatus.Pending, cancellationToken);

        if (hasActiveNegotiation)
            throw new RequestValidationException(
            [
                new ValidationFailure("ConversationId",
                    "There is already an active meetup negotiation in this conversation.")
            ]);

        var recipientId = isSeller ? conversation.BuyerId : listing.SellerId;
        var senderUsername = isSeller
            ? listing.Seller.Username
            : conversation.Buyer.Username;

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            ProposedAt = request.ProposedAt,
            DurationMinutes = request.DurationMinutes,
            LocationText = request.LocationText,
            LocationLat = request.LocationLat,
            LocationLng = request.LocationLng,
            Status = MeetingStatus.Pending,
            ParentMeetingId = request.ParentMeetingId,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Meeting,
            MeetingId = meeting.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(meeting, cancellationToken);
        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new ProposeMeetingResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            Meeting = new ProposeMeetingResponse.MeetingData
            {
                Id = meeting.Id,
                ProposedAt = meeting.ProposedAt,
                DurationMinutes = meeting.DurationMinutes,
                LocationText = meeting.LocationText,
                LocationLat = meeting.LocationLat,
                LocationLng = meeting.LocationLng,
                Status = meeting.Status,
                ExpiresAt = meeting.ExpiresAt,
                InitiatorId = meeting.InitiatorId,
                ParentMeetingId = meeting.ParentMeetingId
            }
        };
    }
}