using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public class GetContractCardQueryHandler(IRepository repository)
    : IRequestHandler<GetContractCardQuery, GetContractCardResponse>
{
    public async Task<GetContractCardResponse> Handle(
        GetContractCardQuery request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.RequesterId == conversation.BuyerId
            || request.RequesterId == listing.SellerId;

        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may view the contract card.");

        return new GetContractCardResponse
        {
            Id = card.Id,
            ConversationId = card.ConversationId,
            InitiatorId = card.InitiatorId,
            Status = card.Status,
            AcceptedAt = card.AcceptedAt,
            CreatedAt = card.CreatedAt,
            SellerSubmittedAt = card.SellerSubmission?.SubmittedAt,
            BuyerSubmittedAt = card.BuyerSubmission?.SubmittedAt,
        };
    }
}
