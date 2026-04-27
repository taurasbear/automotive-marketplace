using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;

public sealed record ExportContractPdfQuery : IRequest<byte[]>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
