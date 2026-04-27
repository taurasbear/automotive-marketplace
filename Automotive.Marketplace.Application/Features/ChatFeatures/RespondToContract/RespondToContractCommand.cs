using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public sealed record RespondToContractCommand : IRequest<RespondToContractResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid ResponderId { get; set; }
    public ContractResponseAction Action { get; set; }
}
