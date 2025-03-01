namespace Automotive.Marketplace.Domain.Entities
{
    public class Client : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
