using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Common.Exceptions;

namespace Automotive.Marketplace.Infrastructure.Data;

public class Repository(AutomotiveContext dbContext) : IRepository
{
    public async Task CreateAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity
    {
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken) where T : BaseEntity
    {
        return await dbContext.Set<T>().ToListAsync(cancellationToken);
    }

    public async Task<T> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken) where T : BaseEntity
    {
        return await dbContext.Set<T>().FindAsync(id, cancellationToken)
            ?? throw new DbEntityNotFoundException(typeof(T).Name, id);
    }

    public async Task DeleteAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity
    {
        dbContext.Set<T>().Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken) where T : BaseEntity
    {
        dbContext.Set<T>().Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public IQueryable<T> AsQueryable<T>() where T : BaseEntity
    {
        return dbContext.Set<T>();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}