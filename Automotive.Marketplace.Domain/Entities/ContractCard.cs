namespace Automotive.Marketplace.Domain.Entities;

using Automotive.Marketplace.Domain.Enums;

public class ContractCard : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid InitiatorId { get; set; }

    public ContractCardStatus Status { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Initiator { get; set; } = null!;

    public virtual ContractSellerSubmission? SellerSubmission { get; set; }

    public virtual ContractBuyerSubmission? BuyerSubmission { get; set; }

    public virtual Message? Message { get; set; }
}
