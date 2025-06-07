namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;

public class RefreshTokenHandler(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ITokenService tokenService) : BaseHandler<RefreshTokenRequest, RefreshTokenResponse>(mapper, unitOfWork)
{
    private readonly ITokenService tokenService = tokenService;

    public override async Task<RefreshTokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var currentRefreshToken = await this.UnitOfWork.RefreshTokenRepository
            .GetRefreshTokenByTokenAsync(request.refreshToken, cancellationToken);

        if (
            currentRefreshToken == null
            || string.IsNullOrWhiteSpace(currentRefreshToken.Token)
            || currentRefreshToken.IsRevoked
            || currentRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            // TODO: replace with custom exception (like unauthorized?)
            throw new Exception();
        }

        var fetchedAccount = await this.UnitOfWork.AccountRepository.GetAccountByIdAsync(currentRefreshToken.AccountId, cancellationToken)
            ?? throw new Exception();

        currentRefreshToken.IsRevoked = true;

        var freshAccessToken = this.tokenService.GenerateAccessToken(fetchedAccount);
        var refreshTokenToAdd = this.tokenService.GenerateRefreshTokenEntity(fetchedAccount);

        await this.UnitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenToAdd, cancellationToken);

        var response = this.Mapper.Map<RefreshTokenResponse>(refreshTokenToAdd);
        response.FreshAccessToken = freshAccessToken;

        await this.UnitOfWork.SaveAsync(cancellationToken);

        return response;
    }
}
