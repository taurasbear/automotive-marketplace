using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public sealed record SubmitContractBuyerFormResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public DateTime BuyerSubmittedAt { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
}
