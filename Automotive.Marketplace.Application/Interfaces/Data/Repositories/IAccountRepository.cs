namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories;

using Automotive.Marketplace.Domain.Entities;

public interface IAccountRepository
{
    public Task<Account?> GetAccountAsync(string email, CancellationToken cancellationToken);

    public Task<Account?> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken);

    public Task<Account> AddAccountAsync(Account account, CancellationToken cancellationToken);
}
