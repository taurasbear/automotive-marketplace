namespace Automotive.Marketplace.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public virtual string RoleName => "User";

    public virtual ICollection<Listing> LikedListings { get; set; } = [];

    public virtual ICollection<Listing> Listings { get; set; } = [];

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = [];
}
