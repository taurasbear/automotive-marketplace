namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

public class RegisterAccountHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<RegisterAccountRequest, RegisterAccountResponse>
{
    public async Task<RegisterAccountResponse> Handle(RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            Username = request.username,
            Email = request.email,
            HashedPassword = passwordHasher.Hash(request.password),
        };

        await repository.CreateAsync(account, cancellationToken);

        var freshAccessToken = tokenService.GenerateAccessToken(account);
        var refreshTokenToAdd = tokenService.GenerateRefreshTokenEntity(account);

        await repository.CreateAsync(refreshTokenToAdd, cancellationToken);

        var response = mapper.Map<RegisterAccountResponse>(refreshTokenToAdd);
        response.AccessToken = freshAccessToken;

        return response;
    }
}
