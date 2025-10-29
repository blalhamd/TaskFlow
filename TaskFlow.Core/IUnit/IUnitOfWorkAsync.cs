using TaskFlow.Core.IRepositories.Generic;
using TaskFlow.Core.IRepositories.Non_Generic;
using TaskFlow.Domain.Entities.Base;

namespace TaskFlow.Core.IUnit
{
    public interface IUnitOfWorkAsync : IDisposable
    {
        IGenericRepositoryAsync<T> Repository<T>() where T : BaseEntity;
        ITaskRepositoryAsync taskRepositoryAsync { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
