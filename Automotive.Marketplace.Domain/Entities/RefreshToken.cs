namespace Automotive.Marketplace.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsUsed { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
