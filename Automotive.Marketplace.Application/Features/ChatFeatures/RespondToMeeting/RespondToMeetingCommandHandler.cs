using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ChatFeatures.ProposeMeeting;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToMeeting;

public class RespondToMeetingCommandHandler(IRepository repository)
    : IRequestHandler<RespondToMeetingCommand, RespondToMeetingResponse>
{
    public async Task<RespondToMeetingResponse> Handle(
        RespondToMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync<Meeting>(request.MeetingId, cancellationToken);
        var conversation = meeting.Conversation;
        var listing = conversation.Listing;

        if (meeting.Status != MeetingStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId", "This meeting proposal has already been resolved.")
            ]);

        if (meeting.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException("You cannot respond to your own meeting proposal.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond to a meeting proposal.");

        if (meeting.ExpiresAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId", "This meeting proposal has expired.")
            ]);

        var responderUsername = request.ResponderId == conversation.BuyerId
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        if (request.Action == MeetingResponseAction.Accept)
        {
            meeting.Status = MeetingStatus.Accepted;
            await repository.UpdateAsync(meeting, cancellationToken);

            return new RespondToMeetingResponse
            {
                MeetingId = meeting.Id,
                ConversationId = conversation.Id,
                NewStatus = MeetingStatus.Accepted,
                InitiatorId = meeting.InitiatorId,
                ResponderId = request.ResponderId,
                RescheduledMeeting = null
            };
        }

        if (request.Action == MeetingResponseAction.Decline)
        {
            meeting.Status = MeetingStatus.Declined;
            await repository.UpdateAsync(meeting, cancellationToken);

            return new RespondToMeetingResponse
            {
                MeetingId = meeting.Id,
                ConversationId = conversation.Id,
                NewStatus = MeetingStatus.Declined,
                InitiatorId = meeting.InitiatorId,
                ResponderId = request.ResponderId,
                RescheduledMeeting = null
            };
        }

        // Reschedule
        if (request.Reschedule is null)
            throw new RequestValidationException(
            [
                new ValidationFailure("Reschedule", "Reschedule data is required when action is Reschedule.")
            ]);

        if (request.Reschedule.ProposedAt <= DateTime.UtcNow)
            throw new RequestValidationException(
            [
                new ValidationFailure("Reschedule.ProposedAt", "Rescheduled meeting time must be in the future.")
            ]);

        meeting.Status = MeetingStatus.Rescheduled;

        var rescheduledMeeting = new Meeting
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            InitiatorId = request.ResponderId,
            ProposedAt = request.Reschedule.ProposedAt,
            DurationMinutes = request.Reschedule.DurationMinutes,
            LocationText = request.Reschedule.LocationText,
            LocationLat = request.Reschedule.LocationLat,
            LocationLng = request.Reschedule.LocationLng,
            Status = MeetingStatus.Pending,
            ParentMeetingId = meeting.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        var rescheduledMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.ResponderId,
            Content = string.Empty,
            MessageType = MessageType.Meeting,
            MeetingId = rescheduledMeeting.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ResponderId.ToString()
        };

        conversation.LastMessageAt = rescheduledMessage.SentAt;

        await repository.UpdateAsync(meeting, cancellationToken);
        await repository.CreateAsync(rescheduledMeeting, cancellationToken);
        await repository.CreateAsync(rescheduledMessage, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RespondToMeetingResponse
        {
            MeetingId = meeting.Id,
            ConversationId = conversation.Id,
            NewStatus = MeetingStatus.Rescheduled,
            InitiatorId = meeting.InitiatorId,
            ResponderId = request.ResponderId,
            RescheduledMeeting = new ProposeMeetingResponse
            {
                MessageId = rescheduledMessage.Id,
                ConversationId = conversation.Id,
                SenderId = request.ResponderId,
                SenderUsername = responderUsername,
                SentAt = rescheduledMessage.SentAt,
                RecipientId = meeting.InitiatorId,
                Meeting = new ProposeMeetingResponse.MeetingData
                {
                    Id = rescheduledMeeting.Id,
                    ProposedAt = rescheduledMeeting.ProposedAt,
                    DurationMinutes = rescheduledMeeting.DurationMinutes,
                    LocationText = rescheduledMeeting.LocationText,
                    LocationLat = rescheduledMeeting.LocationLat,
                    LocationLng = rescheduledMeeting.LocationLng,
                    Status = MeetingStatus.Pending,
                    ExpiresAt = rescheduledMeeting.ExpiresAt,
                    InitiatorId = request.ResponderId,
                    ParentMeetingId = rescheduledMeeting.ParentMeetingId
                }
            }
        };
    }
}