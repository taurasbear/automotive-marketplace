using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public class RespondToContractCommandHandler(IRepository repository)
    : IRequestHandler<RespondToContractCommand, RespondToContractResponse>
{
    public async Task<RespondToContractResponse> Handle(
        RespondToContractCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (card.Status != ContractCardStatus.Pending)
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "This contract card has already been responded to.")
            ]);

        if (card.InitiatorId == request.ResponderId)
            throw new UnauthorizedAccessException(
                "You cannot respond to your own contract request.");

        var isParticipant = request.ResponderId == conversation.BuyerId
            || request.ResponderId == listing.SellerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may respond.");

        card.Status = request.Action == ContractResponseAction.Accept
            ? ContractCardStatus.Active
            : ContractCardStatus.Declined;

        if (request.Action == ContractResponseAction.Accept)
            card.AcceptedAt = DateTime.UtcNow;

        await repository.UpdateAsync(card, cancellationToken);

        return new RespondToContractResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            ResponderId = request.ResponderId,
            InitiatorId = card.InitiatorId,
        };
    }
}
