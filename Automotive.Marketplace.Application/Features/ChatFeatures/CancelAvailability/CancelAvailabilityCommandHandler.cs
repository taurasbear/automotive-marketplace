using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public class CancelAvailabilityCommandHandler(IRepository repository)
    : IRequestHandler<CancelAvailabilityCommand, CancelAvailabilityResponse>
{
    public async Task<CancelAvailabilityResponse> Handle(
        CancelAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<AvailabilityCard>(
            request.AvailabilityCardId, cancellationToken);
        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.CancellerId == conversation.BuyerId
            || request.CancellerId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only conversation participants may cancel an availability card.");

        if (card.InitiatorId != request.CancellerId)
            throw new UnauthorizedAccessException(
                "Only the availability card initiator may cancel it.");

        if (card.Status != AvailabilityCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("AvailabilityCardId",
                    "Only pending availability cards can be cancelled.")
            ]);

        card.Status = AvailabilityCardStatus.Cancelled;
        await repository.UpdateAsync(card, cancellationToken);

        var recipientId = card.InitiatorId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        return new CancelAvailabilityResponse
        {
            AvailabilityCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = AvailabilityCardStatus.Cancelled,
            InitiatorId = card.InitiatorId,
            RecipientId = recipientId
        };
    }
}
