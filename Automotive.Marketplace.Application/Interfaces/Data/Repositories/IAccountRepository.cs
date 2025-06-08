namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories;

using Automotive.Marketplace.Domain.Entities;

public interface IAccountRepository : IBaseRepository<Account>
{
    public Task<Account?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken);
}
