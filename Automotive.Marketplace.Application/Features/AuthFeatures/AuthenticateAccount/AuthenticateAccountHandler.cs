namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Application.Interfaces.Services;

    public class AuthenticateAccountHandler : BaseHandler<AuthenticateAccountRequest, AuthenticateAccountResponse>
    {
        private readonly IPasswordHasher passwordHasher;

        public AuthenticateAccountHandler(IMapper mapper, IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher) : base(mapper, unitOfWork)
        {
            this.passwordHasher = passwordHasher;
        }

        public override async Task<AuthenticateAccountResponse> Handle(AuthenticateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = await this.UnitOfWork.AccountRepository.GetAccountAsync(request.email, cancellationToken);

            if (account == null)
            {
                return new AuthenticateAccountResponse();
            }

            if (this.passwordHasher.Verify(request.password, account.HashedPassword))
            {
                return new AuthenticateAccountResponse() { Account = account };
            }
            else
            {
                return new AuthenticateAccountResponse();
            }
        }
    }
}
