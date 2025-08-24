namespace Automotive.Marketplace.Application.Common.Exceptions;

public class UserNotFoundException : Exception
{
    public string Email { get; } = string.Empty;

    public Guid UserId { get; }

    public UserNotFoundException()
    { }

    public UserNotFoundException(string email) : base($"User with email '{email}' was not found.")
    {
        this.Email = email;
    }

    public UserNotFoundException(Guid userId) : base($"User with ID '{userId}' was not found.")
    {
        this.UserId = userId;
    }

    public UserNotFoundException(
        string email,
        Exception? innerException) : base($"User with email '{email}' was not found.", innerException)
    {
        this.Email = email;
    }

    public UserNotFoundException(
        Guid userId,
        Exception? innerException) : base($"User with ID '{userId}' was not found.", innerException)
    {
        this.UserId = userId;
    }
}
