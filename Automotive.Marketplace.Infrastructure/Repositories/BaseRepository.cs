namespace Automotive.Marketplace.Infrastructure.Repositories;

using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

public abstract class BaseRepository<T>(AutomotiveContext automotiveContext) : IBaseRepository<T> where T : BaseEntity
{
    protected readonly AutomotiveContext automotiveContext = automotiveContext;

    protected readonly DbSet<T> dbSet = automotiveContext.Set<T>();

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        var trackedEntity = await this.dbSet.AddAsync(entity, cancellationToken);
        return trackedEntity.Entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await this.dbSet.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await this.dbSet.FindAsync(id, cancellationToken);
    }

    public void Remove(T entity)
    {
        this.dbSet.Remove(entity);
    }

    public void Update(T entity)
    {
        this.dbSet.Update(entity);
    }
}