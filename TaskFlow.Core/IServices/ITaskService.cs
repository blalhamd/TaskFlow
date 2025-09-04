using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.IServices
{
    public interface ITaskService
    {
        Task<PagesResult<TaskEntityViewModel>> GetTasks(int pageNumber, int pageSize); // for admin/manager
        Task<PagesResult<TaskEntityViewModel>> GetTasks(Guid userId, int pageNumber, int pageSize); // user id from claims
        Task<TaskEntityViewModel> GetTaskById(Guid taskId);
        Task<PagesResult<TaskEntityViewModel>> GetTasksByStatus(TaskProgress taskProgress, int pageNumber, int pageSize); // Filter by status
        Task<TaskEntityViewModel> AssignTaskEntity(CreateTaskEntity entity, CancellationToken cancellation);
        Task<TaskEntityViewModel> UpdateTaskDetails(UpdateTaskEntity taskEntity, CancellationToken cancellationToken);
        Task<bool> DeleteTaskById(Guid taskId, CancellationToken cancellationToken);
        Task<bool> ChangeTaskStatus(Guid taskId, TaskProgress progress, CancellationToken cancellationToken);
    }
}
