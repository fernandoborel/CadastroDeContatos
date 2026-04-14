using System.Linq.Expressions;

namespace Contatos.Domain.Interfaces.Repositories;

public interface IBaseRepository<TEntity, TKey> : IDisposable where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();

    //Where genérico
    Task<IEnumerable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);

    //Primeiro registro baseado em uma condição
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    //Contagem
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    //Paginação
    Task<(IEnumerable<TEntity> Data, int Total)> GetPageAsync(
            int page,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null);
}