using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;

public sealed record RegisterUserCommand : IRequest<RegisterUserResponse>
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
};
