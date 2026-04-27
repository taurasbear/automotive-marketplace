using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelContract;

public sealed record CancelContractResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid RecipientId { get; set; }
}
