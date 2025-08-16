namespace Automotive.Marketplace.Application.Interfaces.Data;

using Automotive.Marketplace.Domain.Entities;

public interface IRepository
{
    public Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken) where T : BaseEntity;

    public Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken) where T : BaseEntity;

    public Task CreateAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity;

    public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity;

    public Task UpdateAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity;

    public IQueryable<T> AsQueryable<T>() where T : BaseEntity;

    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
