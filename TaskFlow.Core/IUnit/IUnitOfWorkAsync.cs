using TaskFlow.Core.IRepositories.Non_Generic;

namespace TaskFlow.Core.IUnit
{
    public interface IUnitOfWorkAsync : IDisposable
    {
        ITaskRepositoryAsync TaskRepositoryAsync { get; }
        IDeveloperRepositoryAsync DeveloperRepositoryAsync { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
