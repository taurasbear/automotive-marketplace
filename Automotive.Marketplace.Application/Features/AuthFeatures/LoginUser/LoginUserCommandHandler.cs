using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
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
        var user = await repository
            .AsQueryable<User>()
            .Where(user => user.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new UserNotFoundException(request.Email);

        if (!passwordHasher.Verify(request.Password, user.HashedPassword))
        {
            throw new InvalidCredentialsException();
        }

        var freshAccessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenToAdd = tokenService.GenerateRefreshTokenEntity(user);

        await repository.CreateAsync(refreshTokenToAdd, cancellationToken);

        var response = mapper.Map<LoginUserResponse>(refreshTokenToAdd);
        response.Permissions = [.. user.UserPermissions.Select(userPermission => userPermission.Permission)];
        response.FreshAccessToken = freshAccessToken;

        return response;
    }
}
