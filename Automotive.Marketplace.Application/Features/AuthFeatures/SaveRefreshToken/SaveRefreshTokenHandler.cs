namespace Automotive.Marketplace.Application.Features.AuthFeatures.SaveRefreshToken;

using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

public class SaveRefreshTokenHandler(IUnitOfWork unitOfWork) : IRequestHandler<SaveRefreshTokenRequest>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task Handle(SaveRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Token = request.refreshToken,
            ExpiryDate = request.expiryDate,
            IsRevoked = false,
            IsUsed = false,
            AccountId = request.accountId
        };

        await this.unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);

        await this.unitOfWork.SaveAsync(cancellationToken);
    }
}
