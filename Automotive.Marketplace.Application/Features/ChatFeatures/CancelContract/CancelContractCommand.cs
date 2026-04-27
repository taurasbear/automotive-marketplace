using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public sealed record CancelContractCommand : IRequest<CancelContractResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
