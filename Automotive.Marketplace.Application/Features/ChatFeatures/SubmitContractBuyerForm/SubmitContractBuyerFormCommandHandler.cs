using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public class SubmitContractBuyerFormCommandHandler(IRepository repository)
    : IRequestHandler<SubmitContractBuyerFormCommand, SubmitContractBuyerFormResponse>
{
    private static readonly ContractCardStatus[] SubmittableStatuses =
    [
        ContractCardStatus.Active,
        ContractCardStatus.SellerSubmitted,
    ];

    public async Task<SubmitContractBuyerFormResponse> Handle(
        SubmitContractBuyerFormCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (conversation.BuyerId != request.BuyerId)
            throw new UnauthorizedAccessException(
                "Only the buyer of this conversation may submit the buyer form.");

        if (!SubmittableStatuses.Contains(card.Status))
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "The buyer form can only be submitted when the contract is Active or SellerSubmitted.")
            ]);

        card.Status = card.Status == ContractCardStatus.SellerSubmitted
            ? ContractCardStatus.Complete
            : ContractCardStatus.BuyerSubmitted;

        var submission = new ContractBuyerSubmission
        {
            Id = Guid.NewGuid(),
            ContractCardId = card.Id,
            PersonalIdCode = request.PersonalIdCode,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.BuyerId.ToString(),
        };

        await repository.CreateAsync(submission, cancellationToken);

        if (request.UpdateProfile)
        {
            var buyer = await repository.GetByIdAsync<User>(request.BuyerId, cancellationToken);
            buyer.PhoneNumber = request.Phone;
            buyer.PersonalIdCode = request.PersonalIdCode;
            buyer.Address = request.Address;
            try { await repository.UpdateAsync(buyer, cancellationToken); }
            catch { /* non-critical */ }
        }

        return new SubmitContractBuyerFormResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            BuyerSubmittedAt = submission.SubmittedAt,
            InitiatorId = card.InitiatorId,
            BuyerId = conversation.BuyerId,
            SellerId = listing.SellerId,
        };
    }
}
