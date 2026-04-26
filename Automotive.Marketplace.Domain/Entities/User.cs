namespace Automotive.Marketplace.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? PersonalIdCode { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Listing> LikedListings { get; set; } = [];

    public virtual ICollection<Listing> Listings { get; set; } = [];

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = [];
}
