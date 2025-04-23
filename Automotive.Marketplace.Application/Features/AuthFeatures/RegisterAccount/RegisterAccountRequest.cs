namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount
{
    using MediatR;

    public sealed record RegisterAccountRequest(string username, string email, string password) : IRequest<RegisterAccountResponse>;
}
