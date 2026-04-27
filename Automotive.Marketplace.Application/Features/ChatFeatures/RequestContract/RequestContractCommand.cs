using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public sealed record RequestContractCommand : IRequest<RequestContractResponse>
{
    public Guid ConversationId { get; set; }
    public Guid InitiatorId { get; set; }
}
