namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

public class RegisterAccountCommandHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    public async Task<RegisterAccountResponse> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            Username = request.Username,
            Email = request.Email,
            HashedPassword = passwordHasher.Hash(request.Password),
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
