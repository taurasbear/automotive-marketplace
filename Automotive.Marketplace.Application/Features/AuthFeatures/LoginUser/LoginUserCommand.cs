using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;

public sealed record LoginUserCommand : IRequest<LoginUserResponse>
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
};
