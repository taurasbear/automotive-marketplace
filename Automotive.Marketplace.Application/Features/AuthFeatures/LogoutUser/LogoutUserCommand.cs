using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.LogoutUser;

public sealed record LogoutUserCommand : IRequest
{
    public string RefreshToken { get; set; } = string.Empty;
};
