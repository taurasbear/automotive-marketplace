using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.RequestContract;

public sealed record RequestContractResponse
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public Guid RecipientId { get; set; }
    public ContractCardData ContractCard { get; set; } = null!;

    public sealed record ContractCardData
    {
        public Guid Id { get; set; }
        public ContractCardStatus Status { get; set; }
        public Guid InitiatorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
