using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Features.ChatFeatures.ShareAvailability;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToAvailability;

public class RespondToAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<RespondToAvailabilityCommand, RespondToAvailabilityResponse>
{
    public async Task<RespondToAvailabilityResponse> Handle(
        RespondToAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<AvailabilityCard>(
            request.AvailabilityCardId, cancellationToken);
        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (card.Status != AvailabilityCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId", "This availability card has already been responded to.")
            ]);

        if (card.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own availability card.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond to an availability card.");

        if (card.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId", "This availability card has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        var recipientId = card.InitiatorId;

        if (request.Action == AvailabilityResponseAction.PickSlot)
        {
            if (request.SlotId is null)
                throw new RequestValidationException(
                [
                    new ValidationFailure("SlotId", "SlotId is required when action is PickSlot.")
                ]);

            var slot = card.Slots.FirstOrDefault(s => s.Id == request.SlotId.Value)
                ?? throw new RequestValidationException(
                [
                    new ValidationFailure("SlotId", "The specified slot does not belong to this availability card.")
                ]);

            card.Status = AvailabilityCardStatus.Responded;
            await repository.UpdateAsync(card, cancellationToken);

            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                InitiatorId = request.ResponderId,
                ProposedAt = slot.StartTime,
                DurationMinutes = (int)(slot.EndTime - slot.StartTime).TotalMinutes,
                Status = MeetingStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                Content = string.Empty,
                MessageType = MessageType.Meeting,
                MeetingId = meeting.Id,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };

            conversation.LastMessageAt = message.SentAt;

            await repository.CreateAsync(meeting, cancellationToken);
            await repository.CreateAsync(message, cancellationToken);
            await repository.UpdateAsync(conversation, cancellationToken);

            return new RespondToAvailabilityResponse
            {
                AvailabilityCardId = card.Id,
                ConversationId = conversation.Id,
                Action = AvailabilityResponseAction.PickSlot,
                PickedSlotMeeting = new ProposeMeetingResponse
                {
                    MessageId = message.Id,
                    ConversationId = conversation.Id,
                    SenderId = request.ResponderId,
                    SenderUsername = responderUsername,
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
                },
                SharedBackAvailability = null
            };
        }

        // ShareBack
        if (request.ShareBackSlots is null || request.ShareBackSlots.Count == 0)
            throw new RequestValidationException(
            [
                new ValidationFailure("ShareBackSlots", "At least one time slot is required for ShareBack.")
            ]);

        foreach (var slot in request.ShareBackSlots)
        {
            if (slot.StartTime <= DateTime.UtcNow)
                throw new RequestValidationException(
                [
                    new ValidationFailure("ShareBackSlots.StartTime", "All slot start times must be in the future.")
                ]);

            if (slot.EndTime <= slot.StartTime)
                throw new RequestValidationException(
                [
                    new ValidationFailure("ShareBackSlots.EndTime", "Slot end time must be after start time.")
                ]);
        }

        card.Status = AvailabilityCardStatus.Responded;
        await repository.UpdateAsync(card, cancellationToken);

        var newCard = new AvailabilityCard
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            Status = AvailabilityCardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        await repository.CreateAsync(newCard, cancellationToken);

        var slotEntities = new List<AvailabilitySlot>();
        foreach (var slot in request.ShareBackSlots)
        {
            var slotEntity = new AvailabilitySlot
            {
                Id = Guid.NewGuid(),
                AvailabilityCardId = newCard.Id,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ResponderId.ToString()
            };
            await repository.CreateAsync(slotEntity, cancellationToken);
            slotEntities.Add(slotEntity);
        }

        var shareBackMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Availability,
            AvailabilityCardId = newCard.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = shareBackMessage.SentAt;

        await repository.CreateAsync(shareBackMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RespondToAvailabilityResponse
        {
            AvailabilityCardId = card.Id,
            ConversationId = conversation.Id,
            Action = AvailabilityResponseAction.ShareBack,
            PickedSlotMeeting = null,
            SharedBackAvailability = new ShareAvailabilityResponse
            {
                MessageId = shareBackMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = shareBackMessage.SentAt,
                RecipientId = recipientId,
                AvailabilityCard = new ShareAvailabilityResponse.AvailabilityCardData
                {
                    Id = newCard.Id,
                    Status = newCard.Status,
                    ExpiresAt = newCard.ExpiresAt,
                    InitiatorId = newCard.InitiatorId,
                    Slots = slotEntities.Select(s => new ShareAvailabilityResponse.AvailabilityCardData.SlotData
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToList()
                }
            }
        };
    }
}