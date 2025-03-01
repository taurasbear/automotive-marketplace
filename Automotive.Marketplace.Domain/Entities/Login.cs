namespace Automotive.Marketplace.Domain.Entities
{
    public class Login : BaseEntity
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty; 

        public string HashedPassword { get; set; } = string.Empty;
    }
}
