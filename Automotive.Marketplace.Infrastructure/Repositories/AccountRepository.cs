namespace Automotive.Marketplace.Infrastructure.Repositories
{
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
    using Automotive.Marketplace.Domain.Entities;
    using Automotive.Marketplace.Infrastructure.Data.DbContext;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository(AutomotiveContext automotiveContext) : base(automotiveContext)
        { }

        public async Task<Account> AddAccountAsync(Account account, CancellationToken cancellationToken)
        {
            var result = await this.automotiveContext.Accounts
                .AddAsync(account, cancellationToken);

            return result.Entity;
        }

        public async Task<Account?> GetAccountAsync(string email, CancellationToken cancellationToken)
        {
            return await this.automotiveContext.Accounts
                .Where(account => account.Email == email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Account?> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return await this.automotiveContext.Accounts.FindAsync(accountId, cancellationToken);
        }
    }
}
