using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractSellerForm;

public sealed record SubmitContractSellerFormResponse
{
    public Guid ContractCardId { get; set; }
    public Guid ConversationId { get; set; }
    public ContractCardStatus NewStatus { get; set; }
    public DateTime SellerSubmittedAt { get; set; }
    public Guid InitiatorId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
}
