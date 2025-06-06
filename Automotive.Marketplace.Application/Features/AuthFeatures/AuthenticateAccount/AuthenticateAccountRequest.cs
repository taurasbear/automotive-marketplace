namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

using MediatR;

public sealed record AuthenticateAccountRequest(string email, string password) : IRequest<AuthenticateAccountResponse>;
