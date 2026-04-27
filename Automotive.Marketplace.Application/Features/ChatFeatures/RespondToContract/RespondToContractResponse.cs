using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RespondToContract;

public sealed record RespondToContractResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public Guid ResponderId { get; set; }
    public Guid InitiatorId { get; set; }
}
