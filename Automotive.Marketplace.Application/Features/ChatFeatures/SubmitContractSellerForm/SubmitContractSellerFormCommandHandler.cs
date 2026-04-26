using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using FluentValidation.Results;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;

public class SubmitContractSellerFormCommandHandler(IRepository repository)
    : IRequestHandler<SubmitContractSellerFormCommand, SubmitContractSellerFormResponse>
{
    private static readonly ContractCardStatus[] SubmittableStatuses =
    [
        ContractCardStatus.Active,
        ContractCardStatus.BuyerSubmitted,
    ];

    public async Task<SubmitContractSellerFormResponse> Handle(
        SubmitContractSellerFormCommand request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        if (listing.SellerId != request.SellerId)
            throw new UnauthorizedAccessException(
                "Only the seller of this conversation may submit the seller form.");

        if (!SubmittableStatuses.Contains(card.Status))
            throw new RequestValidationException(
            [
                new ValidationFailure("ContractCardId",
                    "The seller form can only be submitted when the contract is Active or BuyerSubmitted.")
            ]);

        var submission = new ContractSellerSubmission
        {
            Id = Guid.NewGuid(),
            ContractCardId = card.Id,
            SdkCode = request.SdkCode,
            Make = request.Make,
            CommercialName = request.CommercialName,
            RegistrationNumber = request.RegistrationNumber,
            Mileage = request.Mileage,
            Vin = request.Vin,
            RegistrationCertificate = request.RegistrationCertificate,
            TechnicalInspectionValid = request.TechnicalInspectionValid,
            WasDamaged = request.WasDamaged,
            DamageKnown = request.WasDamaged ? request.DamageKnown : null,
            DefectBrakes = request.DefectBrakes,
            DefectSafety = request.DefectSafety,
            DefectSteering = request.DefectSteering,
            DefectExhaust = request.DefectExhaust,
            DefectLighting = request.DefectLighting,
            DefectDetails = request.DefectDetails,
            Price = request.Price,
            PersonalIdCode = request.PersonalIdCode,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Country = request.Country,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.SellerId.ToString(),
        };

        await repository.CreateAsync(submission, cancellationToken);

        card.Status = card.Status == ContractCardStatus.BuyerSubmitted
            ? ContractCardStatus.Complete
            : ContractCardStatus.SellerSubmitted;

        await repository.UpdateAsync(card, cancellationToken);

        if (request.UpdateProfile)
        {
            var seller = await repository.GetByIdAsync<User>(request.SellerId, cancellationToken);
            seller.PhoneNumber = request.Phone;
            seller.PersonalIdCode = request.PersonalIdCode;
            seller.Address = request.Address;
            try { await repository.UpdateAsync(seller, cancellationToken); }
            catch { /* profile save is non-critical */ }
        }

        return new SubmitContractSellerFormResponse
        {
            ContractCardId = card.Id,
            ConversationId = conversation.Id,
            NewStatus = card.Status,
            SellerSubmittedAt = submission.SubmittedAt,
            InitiatorId = card.InitiatorId,
            BuyerId = conversation.BuyerId,
            SellerId = listing.SellerId,
        };
    }
}
