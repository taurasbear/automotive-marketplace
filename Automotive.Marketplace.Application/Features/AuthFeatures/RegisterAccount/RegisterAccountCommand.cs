using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

public sealed record RegisterAccountCommand : IRequest<RegisterAccountResponse>
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
};
