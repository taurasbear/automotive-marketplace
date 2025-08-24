using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

public class AuthenticateAccountCommandHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<AuthenticateAccountCommand, AuthenticateAccountResponse>
{
    public async Task<AuthenticateAccountResponse> Handle(AuthenticateAccountCommand request, CancellationToken cancellationToken)
    {
        var fetchedAccount = await repository
            .AsQueryable<User>()
            .Where(account => account.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new UserNotFoundException(request.Email);

        if (!passwordHasher.Verify(request.Password, fetchedAccount.HashedPassword))
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
