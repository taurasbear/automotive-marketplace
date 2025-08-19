namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using MediatR;

public sealed record RegisterAccountCommand : IRequest<RegisterAccountResponse>
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
};
