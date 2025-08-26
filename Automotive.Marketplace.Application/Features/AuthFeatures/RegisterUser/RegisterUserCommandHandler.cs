using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;

public class RegisterUserCommandHandler(
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            HashedPassword = passwordHasher.Hash(request.Password),
        };

        await repository.CreateAsync(user, cancellationToken);

        var freshAccessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenToAdd = tokenService.GenerateRefreshTokenEntity(user);

        await repository.CreateAsync(refreshTokenToAdd, cancellationToken);

        var response = mapper.Map<RegisterUserResponse>(refreshTokenToAdd);
        response.AccessToken = freshAccessToken;

        return response;
    }
}
