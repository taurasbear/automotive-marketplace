using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public class RequestContractCommandHandler(IRepository repository)
    : IRequestHandler<RequestContractCommand, RequestContractResponse>
{
    private static readonly ContractCardStatus[] ActiveStatuses =
    [
        ContractCardStatus.Pending,
        ContractCardStatus.Active,
        ContractCardStatus.SellerSubmitted,
        ContractCardStatus.BuyerSubmitted,
    ];

    public async Task<RequestContractResponse> Handle(
        RequestContractCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync<Conversation>(
            request.ConversationId, cancellationToken);

        var listing = conversation.Listing;
        var isBuyer = conversation.BuyerId == request.InitiatorId;
        var isSeller = listing.SellerId == request.InitiatorId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException(
                "Only the buyer or seller of this conversation may request a contract.");

        var hasActiveContract = await repository.AsQueryable<ContractCard>()
            .AnyAsync(c => c.ConversationId == request.ConversationId
                        && ActiveStatuses.Contains(c.Status), cancellationToken);

        if (hasActiveContract)
            throw new ConflictException("An active contract already exists in this conversation.");

        var recipientId = isBuyer ? listing.SellerId : conversation.BuyerId;
        var senderUsername = isBuyer
            ? conversation.Buyer.Username
            : listing.Seller.Username;

        var card = new ContractCard
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Status = ContractCardStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString(),
        };

        await repository.CreateAsync(card, cancellationToken);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.InitiatorId,
            Content = string.Empty,
            MessageType = MessageType.Contract,
            ContractCardId = card.Id,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatorId.ToString(),
        };

        conversation.LastMessageAt = message.SentAt;

        await repository.CreateAsync(message, cancellationToken);
        await repository.UpdateAsync(conversation, cancellationToken);

        return new RequestContractResponse
        {
            MessageId = message.Id,
            ConversationId = conversation.Id,
            SenderId = request.InitiatorId,
            SenderUsername = senderUsername,
            SentAt = message.SentAt,
            RecipientId = recipientId,
            ContractCard = new RequestContractResponse.ContractCardData
            {
                Id = card.Id,
                Status = card.Status,
                InitiatorId = card.InitiatorId,
                CreatedAt = card.CreatedAt,
            },
        };
    }
}
