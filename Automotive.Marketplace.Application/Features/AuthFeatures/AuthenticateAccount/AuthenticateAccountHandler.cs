namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class AuthenticateAccountHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<AuthenticateAccountRequest, AuthenticateAccountResponse>
{
    public async Task<AuthenticateAccountResponse> Handle(AuthenticateAccountRequest request, CancellationToken cancellationToken)
    {
        var fetchedAccount = await repository
            .AsQueryable<Account>()
            .Where(account => account.Email == request.email)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new AccountNotFoundException(request.email);

        if (!passwordHasher.Verify(request.password, fetchedAccount.HashedPassword))
        {
            throw new InvalidCredentialsException();
        }

        var freshAccessToken = tokenService.GenerateAccessToken(fetchedAccount);
        var refreshTokenToAdd = tokenService.GenerateRefreshTokenEntity(fetchedAccount);

        await repository.CreateAsync(refreshTokenToAdd, cancellationToken);

        var response = mapper.Map<AuthenticateAccountResponse>(refreshTokenToAdd);
        response.FreshAccessToken = freshAccessToken;

        return response;
    }
}
