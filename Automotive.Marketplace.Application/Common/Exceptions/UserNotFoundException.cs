namespace Automotive.Marketplace.Application.Common.Exceptions;

public class UserNotFoundException : Exception
{
    public string Email { get; } = string.Empty;

    public Guid UserId { get; }

    public UserNotFoundException()
    { }

    public UserNotFoundException(string email) : base($"User with email '{email}' was not found.")
    {
        Email = email;
    }

    public UserNotFoundException(Guid userId) : base($"User with ID '{userId}' was not found.")
    {
        UserId = userId;
    }

    public UserNotFoundException(
        string email,
        Exception? innerException) : base($"User with email '{email}' was not found.", innerException)
    {
        Email = email;
    }

    public UserNotFoundException(
        Guid userId,
        Exception? innerException) : base($"User with ID '{userId}' was not found.", innerException)
    {
        UserId = userId;
    }
}
