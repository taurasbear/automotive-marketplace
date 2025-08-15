namespace Automotive.Marketplace.Infrastructure.Repositories;

using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class AccountRepository(AutomotiveContext automotiveContext) : BaseRepository<Account>(automotiveContext), IAccountRepository
{
    public async Task<Account?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await this.automotiveContext.Accounts
            .Where(account => account.Email == email)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
