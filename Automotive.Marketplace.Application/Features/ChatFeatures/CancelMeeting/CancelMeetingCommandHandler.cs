using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelMeeting;

public class CancelMeetingCommandHandler(IRepository repository)
    : IRequestHandler<CancelMeetingCommand, CancelMeetingResponse>
{
    public async Task<CancelMeetingResponse> Handle(
        CancelMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync<Meeting>(
            request.MeetingId, cancellationToken);
        var conversation = meeting.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.CancellerId == conversation.BuyerId
            || request.CancellerId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only conversation participants may cancel a meeting.");

        if (meeting.Status != MeetingStatus.Pending && meeting.Status != MeetingStatus.Accepted)
            throw new RequestValidationException(
            [
                new ValidationFailure("MeetingId",
                    "Only pending or accepted meetings can be cancelled.")
            ]);

        // Pending meetings can only be cancelled by the initiator
        if (meeting.Status == MeetingStatus.Pending && meeting.InitiatorId != request.CancellerId)
            throw new UnauthorizedAccessException(
                "Only the meeting initiator may cancel a pending meeting.");

        meeting.Status = MeetingStatus.Cancelled;
        await repository.UpdateAsync(meeting, cancellationToken);

        var recipientId = request.CancellerId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        return new CancelMeetingResponse
        {
            MeetingId = meeting.Id,
            ConversationId = conversation.Id,
            NewStatus = MeetingStatus.Cancelled,
            InitiatorId = meeting.InitiatorId,
            RecipientId = recipientId
        };
    }
}
