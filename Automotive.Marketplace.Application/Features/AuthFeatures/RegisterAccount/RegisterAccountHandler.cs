namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;

public class RegisterAccountHandler(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService
    ) : BaseHandler<RegisterAccountRequest, RegisterAccountResponse>(mapper, unitOfWork)
{
    private readonly IPasswordHasher passwordHasher = passwordHasher;

    private readonly ITokenService tokenService = tokenService;

    public override async Task<RegisterAccountResponse> Handle(RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        var accountToAdd = new Account
        {
            Username = request.username,
            Email = request.email,
            HashedPassword = this.passwordHasher.Hash(request.password),
        };

        var addedAccount = await this.UnitOfWork.AccountRepository.AddAccountAsync(accountToAdd, cancellationToken);

        var freshAccessToken = this.tokenService.GenerateAccessToken(addedAccount);
        var refreshTokenToAdd = this.tokenService.GenerateRefreshTokenEntity(addedAccount);

        await this.UnitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenToAdd, cancellationToken);

        var response = this.Mapper.Map<RegisterAccountResponse>(refreshTokenToAdd);
        response.AccessToken = freshAccessToken;

        await this.UnitOfWork.SaveAsync(cancellationToken);

        return response;
    }
}
