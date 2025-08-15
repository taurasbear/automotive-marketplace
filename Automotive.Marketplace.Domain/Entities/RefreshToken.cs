namespace Automotive.Marketplace.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsUsed { get; set; }

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;
}
