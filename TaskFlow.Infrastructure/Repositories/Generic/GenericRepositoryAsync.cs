using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskFlow.Core.IRepositories.Generic;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Infrastructure.Data.context;

namespace TaskFlow.Infrastructure.Repositories.Generic
{
    public class GenericRepositoryAsync<T> : IGenericRepositoryAsync<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        protected readonly DbSet<T> _repo;

        public GenericRepositoryAsync(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repo = _context.Set<T>();
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _repo.LongCountAsync();
            return await _repo.LongCountAsync(predicate);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? pageNumber = null,
            int? pageSize = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _repo.AsNoTracking();

            // filtering
            if (predicate != null)
                query = query.Where(predicate);

            // includes
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            // ordering
            if (orderBy != null)
                query = orderBy(query);

            // paging
            if (pageNumber.HasValue && pageSize.HasValue)
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value)
                             .Take(pageSize.Value);

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _repo.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _repo.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate)
            => await _repo.AnyAsync(predicate);

        public async Task CreateAsync(T entity, CancellationToken cancellationToken)
            => await _repo.AddAsync(entity);

        public Task UpdateAsync(T entity, CancellationToken cancellationToken)
        {
            _repo.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken)
        {
            _repo.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
