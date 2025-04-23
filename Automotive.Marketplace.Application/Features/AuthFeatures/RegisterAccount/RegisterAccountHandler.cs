namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Application.Interfaces.Services;
    using Automotive.Marketplace.Domain.Entities;

    public class RegisterAccountHandler : BaseHandler<RegisterAccountRequest, RegisterAccountResponse>
    {
        private readonly IPasswordHasher passwordHasher;

        public RegisterAccountHandler(IMapper mapper, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : base(mapper, unitOfWork)
        {
            this.passwordHasher = passwordHasher;
        }

        public override async Task<RegisterAccountResponse> Handle(RegisterAccountRequest request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Username = request.username,
                Email = request.email,
                HashedPassword = this.passwordHasher.Hash(request.password),
            };

            var addedAccount = await this.UnitOfWork.AccountRepository.AddAccountAsync(account, cancellationToken);

            await this.UnitOfWork.SaveAsync(cancellationToken);

            return this.Mapper.Map<RegisterAccountResponse>(addedAccount);
        }
    }
}
