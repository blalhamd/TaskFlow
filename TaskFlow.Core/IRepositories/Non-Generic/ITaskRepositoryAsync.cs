using System.Linq.Expressions;
using TaskFlow.Core.IRepositories.Generic;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Core.IRepositories.Non_Generic
{
    public interface ITaskRepositoryAsync : IGenericRepositoryAsync<TaskEntity>
    {
        Task<TaskDto?> FirstOrDefaultAsync(Expression<Func<TaskEntity, bool>> predicate);
    }
}
