namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Application.Interfaces.Services;
    using Automotive.Marketplace.Domain.Entities;

    public class RefreshTokenHandler : BaseHandler<RefreshTokenRequest, RefreshTokenResponse>
    {
        private readonly ITokenService tokenService;

        public RefreshTokenHandler(IMapper mapper, IUnitOfWork unitOfWork, ITokenService tokenService) : base(mapper, unitOfWork)
        {
            this.tokenService = tokenService;
        }

        public override async Task<RefreshTokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var currentRefreshToken = await this.UnitOfWork.RefreshTokenRepository.GetRefreshTokenByTokenAsync(request.refreshToken, cancellationToken);

            if (currentRefreshToken == null || string.IsNullOrWhiteSpace(currentRefreshToken.Token) || currentRefreshToken.IsRevoked || currentRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                // TODO: replace with custom exception (like unauthorized?)
                throw new Exception();
            }

            var account = await this.UnitOfWork.AccountRepository.GetAccountByIdAsync(currentRefreshToken.AccountId, cancellationToken);

            if (account == null)
            {
                throw new Exception();
            }

            var freshAccessToken = this.tokenService.GenerateAccessToken(account);
            var freshRefreshToken = this.tokenService.GenerateRefreshToken();
            var freshExpiryDate = this.tokenService.GetRefreshTokenExpiryData();

            currentRefreshToken.IsRevoked = true;
            var newRefreshToken = new RefreshToken
            {
                Token = freshRefreshToken,
                ExpiryDate = freshExpiryDate,
                IsRevoked = false,
                IsUsed = false,
                AccountId = currentRefreshToken.AccountId
            };

            await this.UnitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
            await this.UnitOfWork.SaveAsync(cancellationToken);
            throw new Exception();
        }
    }
}
