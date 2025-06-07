namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;

public class AuthenticateAccountHandler(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : BaseHandler<AuthenticateAccountRequest, AuthenticateAccountResponse>(mapper, unitOfWork)
{
    private readonly IPasswordHasher passwordHasher = passwordHasher;

    private readonly ITokenService tokenService = tokenService;

    public override async Task<AuthenticateAccountResponse> Handle(AuthenticateAccountRequest request, CancellationToken cancellationToken)
    {
        var fetchedAccount = await this.UnitOfWork.AccountRepository.GetAccountAsync(request.email, cancellationToken)
            ?? throw new Exception("Account does not exist"); // TODO: Use custom exception

        if (!this.passwordHasher.Verify(request.password, fetchedAccount.HashedPassword))
        {
            // TODO: Use custom expection
            throw new Exception("Invalid email or password");
        }

        var freshAccessToken = this.tokenService.GenerateAccessToken(fetchedAccount);
        var refreshTokenToAdd = this.tokenService.GenerateRefreshTokenEntity(fetchedAccount);

        await this.UnitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenToAdd, cancellationToken);

        var response = this.Mapper.Map<AuthenticateAccountResponse>(refreshTokenToAdd);
        response.FreshAccessToken = freshAccessToken;

        await this.UnitOfWork.SaveAsync(cancellationToken);

        return response;
    }
}
