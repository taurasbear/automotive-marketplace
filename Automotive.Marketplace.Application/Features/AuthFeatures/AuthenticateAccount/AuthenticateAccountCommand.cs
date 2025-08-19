using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

public sealed record AuthenticateAccountCommand : IRequest<AuthenticateAccountResponse>
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
};
