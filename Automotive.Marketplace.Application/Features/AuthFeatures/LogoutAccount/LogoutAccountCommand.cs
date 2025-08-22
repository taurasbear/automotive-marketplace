using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

public sealed record LogoutAccountCommand : IRequest
{
    public string RefreshToken { get; set; } = string.Empty;
};
