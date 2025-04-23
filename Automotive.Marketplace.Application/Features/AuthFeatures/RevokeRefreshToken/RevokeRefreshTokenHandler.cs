namespace Automotive.Marketplace.Application.Features.AuthFeatures.RevokeRefreshToken
{
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Domain.Entities;
    using MediatR;

    public sealed record RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshTokenRequest>
    {
        private readonly IUnitOfWork unitOfWork;

        public RevokeRefreshTokenHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task Handle(RevokeRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var oldRefreshToken = await this.unitOfWork.RefreshTokenRepository.GetRefreshTokenByTokenAsync(request.oldRefreshToken, cancellationToken);

            if (oldRefreshToken == null)
            {
                throw new ArgumentException("Refresh token doesn't exist.");
            }

            oldRefreshToken.IsRevoked = true;
            var newRefreshToken = new RefreshToken
            {
                Token = request.newRefreshToken,
                ExpiryDate = request.newExpiryDate,
                IsRevoked = false,
                IsUsed = false,
                AccountId = request.accountId
            };

            await this.unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);

            await this.unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
