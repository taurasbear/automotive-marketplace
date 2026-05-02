using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public class CancelContractCommandHandler(IRepository repository)
    : IRequestHandler<CancelContractCommand, CancelContractResponse>
{
    public async Task<CancelContractResponse> Handle(
        CancelContractCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        // Either party can cancel (not just initiator)
        var buyerId = conversation.BuyerId;
        var sellerId = listing.SellerId;
        if (request.RequesterId != buyerId && request.RequesterId != sellerId)
            throw new UnauthorizedAccessException("Only conversation participants can cancel.");

        // Can cancel if not Complete
        var cancellableStatuses = new[]
        {
            ContractCardStatus.Pending,
            ContractCardStatus.Active,
            ContractCardStatus.SellerSubmitted,
            ContractCardStatus.BuyerSubmitted,
        };
        if (!cancellableStatuses.Contains(card.Status))
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "Contract cannot be cancelled in its current state.")
            ]);

        var recipientId = card.InitiatorId == conversation.BuyerId
            ? listing.SellerId
            : conversation.BuyerId;

        card.Status = ContractCardStatus.Cancelled;
        await repository.UpdateAsync(card, cancellationToken);

        return new CancelContractResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            InitiatorId = card.InitiatorId,
            RecipientId = recipientId,
        };
    }
}
