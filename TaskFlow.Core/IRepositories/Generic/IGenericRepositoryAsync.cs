using System.Linq.Expressions;
using TaskFlow.Domain.Entities.Base;

namespace TaskFlow.Core.IRepositories.Generic
{
    public interface IGenericRepositoryAsync<T> where T : BaseEntity
    {
        Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? pageNumber = null,
            int? pageSize = null,
            params Expression<Func<T, object>>[] includes
        );

        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate);
        Task CreateAsync(T entity, CancellationToken cancellation);
        Task UpdateAsync(T entity, CancellationToken cancellation);
        Task DeleteAsync(T entity, CancellationToken cancellation);
    }
}
