namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories;

using Automotive.Marketplace.Domain.Entities;

public interface IBaseRepository<T> where T : BaseEntity
{
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken);

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken);

    public void Remove(T entity);

    public void Update(T entity);
}