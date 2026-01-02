using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.IServices
{
    public interface ITaskService
    {
        Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasks(int pageNumber, int pageSize); // for admin/manager
        Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasks(Guid userId, int pageNumber, int pageSize); // user id from claims
        Task<ValueResult<TaskEntityViewModel>> GetTaskById(Guid taskId);
        Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasksByStatus(TaskProgress taskProgress, int pageNumber, int pageSize); // Filter by status
        Task<ValueResult<TaskEntityViewModel>> AssignTaskEntity(CreateTaskEntity entity, CancellationToken cancellation);
        Task<ValueResult<TaskEntityViewModel>> UpdateTaskDetails(UpdateTaskEntity taskEntity, CancellationToken cancellationToken);
        Task<Result> DeleteTaskById(Guid taskId, CancellationToken cancellationToken);
        Task<Result> ChangeTaskStatus(Guid taskId, TaskProgress progress, CancellationToken cancellationToken);
        Task<ValueResult<CommentViewModel>> AddCommentToTask(Guid userId, Guid taskId, CreateCommentRequest comment, CancellationToken cancellationToken);
        Task<ValueResult<List<CommentViewModel>>> GetCommentsForTask(Guid taskId);
    }
}
