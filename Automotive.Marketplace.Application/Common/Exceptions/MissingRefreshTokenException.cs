namespace Automotive.Marketplace.Application.Common.Exceptions;

public class MissingRefreshTokenException : Exception
{
    public MissingRefreshTokenException() : base("No refresh token was provided.")
    { }
}
