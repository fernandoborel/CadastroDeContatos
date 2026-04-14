using Contatos.Domain.Interfaces.Repositories;
using Contatos.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Contatos.Infra.Data.Repositories;

public class BaseRepository<TEntity, TKey>(AppDbContext context) : IBaseRepository<TEntity, TKey> where TEntity : class
{
    public async Task AddAsync(TEntity entity)
      => await context.AddAsync(entity);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
      => await context.Set<TEntity>().CountAsync(predicate);

    public async Task DeleteAsync(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
        await Task.CompletedTask;
    }

    public void Dispose()
      => context.Dispose();

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
      => await context.Set<TEntity>().FirstOrDefaultAsync(predicate);

    public async Task<IEnumerable<TEntity>> GetAllAsync()
      => await context.Set<TEntity>().ToListAsync();

    public async Task<TEntity?> GetByIdAsync(TKey id)
      => await context.Set<TEntity>().FindAsync(id);


    public async Task<(IEnumerable<TEntity> Data, int Total)> GetPageAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? predicate = null)
    {
        if (pageSize > 25) pageSize = 25;

        IQueryable<TEntity> query = context.Set<TEntity>();

        if (predicate != null)
            query = query.Where(predicate);

        var total = await query.CountAsync();

        var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

        return (data, total);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        => await context.Set<TEntity>().Where(predicate).ToListAsync();
}