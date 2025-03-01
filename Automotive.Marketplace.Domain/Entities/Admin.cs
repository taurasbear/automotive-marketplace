namespace Automotive.Marketplace.Domain.Entities
{
    using Automotive.Marketplace.Domain.Enums;

    public class Admin : BaseEntity
    {
        public Role Role { get; set; }
    }
}
