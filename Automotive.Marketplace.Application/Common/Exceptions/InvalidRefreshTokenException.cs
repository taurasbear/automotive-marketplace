namespace Automotive.Marketplace.Application.Common.Exceptions;

public class InvalidRefreshTokenException : Exception
{
    public string Token { get; } = string.Empty;

    public InvalidRefreshTokenException()
    { }

    public InvalidRefreshTokenException(string token) : base("The provided refresh token is invalid or expired.")
    {
        this.Token = token;
    }

    public InvalidRefreshTokenException(
        string token,
        Exception? innerException) : base("The provided refresh token is invalid or expired.", innerException)
    {
        this.Token = token;
    }
}
