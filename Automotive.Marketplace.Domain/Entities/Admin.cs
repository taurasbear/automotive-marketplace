namespace Automotive.Marketplace.Domain.Entities
{
    using Automotive.Marketplace.Domain.Enums;

    public class Admin : Login
    {
        public Role Role { get; set; }
    }
}
