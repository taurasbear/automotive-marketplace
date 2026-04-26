using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;

public class ExportContractPdfQueryHandler(IRepository repository, IContractPdfService pdfService)
    : IRequestHandler<ExportContractPdfQuery, byte[]>
{
    public async Task<byte[]> Handle(
        ExportContractPdfQuery request,
        CancellationToken cancellationToken)
    {
        var card = await repository.GetByIdAsync<ContractCard>(
            request.ContractCardId, cancellationToken);

        var conversation = card.Conversation;
        var listing = conversation.Listing;

        var isParticipant = request.RequesterId == conversation.BuyerId
            || request.RequesterId == listing.SellerId;

        if (!isParticipant)
            throw new UnauthorizedAccessException("Access denied.");

        if (card.Status != ContractCardStatus.Complete)
            throw new UnauthorizedAccessException(
                "PDF is only available after both parties have submitted.");

        var seller = card.SellerSubmission
            ?? throw new InvalidOperationException("Seller submission missing for complete contract.");
        var buyer = card.BuyerSubmission
            ?? throw new InvalidOperationException("Buyer submission missing for complete contract.");

        return pdfService.Generate(card, seller, buyer);
    }
}
