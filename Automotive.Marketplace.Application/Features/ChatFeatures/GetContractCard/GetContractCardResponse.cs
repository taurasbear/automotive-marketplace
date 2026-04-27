using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;

public sealed record GetContractCardResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid InitiatorId { get; set; }
    public ContractCardStatus Status { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SellerSubmittedAt { get; set; }
    public DateTime? BuyerSubmittedAt { get; set; }
}
