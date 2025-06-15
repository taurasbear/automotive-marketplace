namespace Automotive.Marketplace.Application.Common.Exceptions;

public class AccountNotFoundException : Exception
{
    public string Email { get; } = string.Empty;

    public Guid AccountId { get; set; }

    public AccountNotFoundException()
    { }

    public AccountNotFoundException(string email) : base($"Account with email '{email}' was not found.")
    {
        this.Email = email;
    }

    public AccountNotFoundException(Guid accountId) : base($"Account with ID '{accountId}' was not found.")
    {
        this.AccountId = accountId;
    }

    public AccountNotFoundException(
        string email,
        Exception? innerException) : base($"Account with email '{email}' was not found.", innerException)
    {
        this.Email = email;
    }

    public AccountNotFoundException(
        Guid accountId,
        Exception? innerException) : base($"Account with ID '{accountId}' was not found.", innerException)
    {
        this.AccountId = accountId;
    }
}
