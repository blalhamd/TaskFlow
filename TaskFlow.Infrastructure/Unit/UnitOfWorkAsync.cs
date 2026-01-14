using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Core.IRepositories.Non_Generic;
using TaskFlow.Core.IUnit;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Infrastructure.Repositories.Non_Generic;

namespace TaskFlow.Infrastructure.Unit
{
    public class UnitOfWorkAsync : IUnitOfWorkAsync
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWorkAsync(AppDbContext context, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider;
        }
        public ITaskRepositoryAsync TaskRepositoryAsync
            => _serviceProvider.GetRequiredService<TaskRepositoryAsync>();

        public IDeveloperRepositoryAsync DeveloperRepositoryAsync
             => _serviceProvider.GetRequiredService<DeveloperRepositoryAsync>();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return; // Already in a transaction

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
