using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;

public class LoginUserCommandHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
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

        var response = mapper.Map<LoginUserResponse>(refreshTokenToAdd);
        response.FreshAccessToken = freshAccessToken;

        return response;
    }
}
