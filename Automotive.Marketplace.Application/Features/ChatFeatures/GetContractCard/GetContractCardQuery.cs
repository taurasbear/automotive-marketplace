using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public sealed record GetContractCardQuery : IRequest<GetContractCardResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid RequesterId { get; set; }
}
