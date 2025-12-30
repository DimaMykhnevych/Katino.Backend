using Katino.Domain.Repositories;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly KatinoDbContext context;

    public Repository(KatinoDbContext context)
    {
        this.context = context;
    }

    public async Task<TEntity> Get(Guid id)
    {
        return await context.Set<TEntity>().FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetAll()
    {
        return await context.Set<TEntity>().ToListAsync();
    }

    public async Task<TEntity> Insert(TEntity entity)
    {
        await context.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public void Delete(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }

    public async Task DeleteById(Guid id)
    {
        TEntity entity = await Get(id);
        if (entity != null)
        {
            context.Set<TEntity>().Remove(entity);
        }
    }

    public async Task Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    public async Task Save()
    {
        await context.SaveChangesAsync();
    }

    public void ClearTracking() => context.ChangeTracker.Clear();
}