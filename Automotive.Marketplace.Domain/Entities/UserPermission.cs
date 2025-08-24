using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Domain.Entities;

public class UserPermission : BaseEntity
{
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public Permission Permission { get; set; }
}