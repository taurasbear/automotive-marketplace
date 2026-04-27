namespace Automotive.Marketplace.Domain.Entities;

public class ContractBuyerSubmission : BaseEntity
{
    public Guid ContractCardId { get; set; }

    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public virtual ContractCard ContractCard { get; set; } = null!;
}
