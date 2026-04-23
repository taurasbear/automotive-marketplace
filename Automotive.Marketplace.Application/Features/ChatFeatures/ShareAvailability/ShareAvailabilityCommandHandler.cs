using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;

public class ShareAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<ShareAvailabilityCommand, ShareAvailabilityResponse>
{
    public async Task<ShareAvailabilityResponse> Handle(
        ShareAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isSeller && conversation.BuyerId != request.InitiatorId)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may share availability.");

        if (isSeller)
        {
            var buyerHasLiked = await repository.AsQueryable<UserListingLike>()
                .AnyAsync(l => l.UserId == conversation.BuyerId
                            && l.ListingId == listing.Id, cancellationToken);
            var buyerHasSentMessage = conversation.Messages.Any(m => m.SenderId == conversation.BuyerId);
            if (!buyerHasLiked && !buyerHasSentMessage)
                throw new RequestValidationException(
                    [new("Conversation", "Seller can only share availability if the buyer has engaged.")]);
        }

        if (request.Slots.Count == 0)
            throw new RequestValidationException(
            [
                new ValidationFailure("Slots", "At least one time slot is required.")
            ]);

        foreach (var slot in request.Slots)
        {
            if (slot.StartTime <= DateTime.UtcNow)
                throw new RequestValidationException(
                [
                    new ValidationFailure("Slots.StartTime", "All slot start times must be in the future.")
                ]);

            if (slot.EndTime <= slot.StartTime)
                throw new RequestValidationException(
                [
                    new ValidationFailure("Slots.EndTime", "Slot end time must be after start time.")
                ]);
        }

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

        var card = new AvailabilityCard
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Status = AvailabilityCardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        await repository.CreateAsync(card, cancellationToken);

        var slotEntities = new List<AvailabilitySlot>();
        foreach (var slot in request.Slots)
        {
            var slotEntity = new AvailabilitySlot
            {
                Id = Guid.NewGuid(),
                AvailabilityCardId = card.Id,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.InitiatorId.ToString()
            };
            await repository.CreateAsync(slotEntity, cancellationToken);
            slotEntities.Add(slotEntity);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Availability,
            AvailabilityCardId = card.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString()
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new ShareAvailabilityResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            AvailabilityCard = new ShareAvailabilityResponse.AvailabilityCardData
            {
                Id = card.Id,
                Status = card.Status,
                ExpiresAt = card.ExpiresAt,
                InitiatorId = card.InitiatorId,
                Slots = slotEntities.Select(s => new ShareAvailabilityResponse.AvailabilityCardData.SlotData
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            }
        };
    }
}