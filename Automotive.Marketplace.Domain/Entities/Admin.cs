namespace Automotive.Marketplace.Domain.Entities
{
    using Automotive.Marketplace.Domain.Enums;

    public class Admin : Account
    {
        public AdminRole AdminRole { get; set; }
    }
}
