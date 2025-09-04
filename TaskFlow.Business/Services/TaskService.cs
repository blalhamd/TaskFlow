using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Core.IServices;
using TaskFlow.Core.IUnit;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.Business.Services
{
    /// <summary>
    ///  Service for managing task entities, including assignment, status changes, deletion, retrieval and paging.
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly ILogger<TaskService> _logger;
        private readonly IValidator<CreateTaskEntity> _createTaskValidator;
        private readonly IValidator<UpdateTaskEntity> _updateTaskValidator;
        public TaskService(IUnitOfWorkAsync unitOfWork, ILogger<TaskService> logger, IValidator<CreateTaskEntity> createTaskValidator, IValidator<UpdateTaskEntity> updateTaskValidator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _createTaskValidator = createTaskValidator;
            _updateTaskValidator = updateTaskValidator;
        }

        public async Task<TaskEntityViewModel> AssignTaskEntity(CreateTaskEntity entity, CancellationToken cancellation)
        {
            _logger.LogInformation("Attempting to assign task to developer {DeveloperId}", entity.AssignedToDeveloperId);

            var validationResult = await _createTaskValidator.ValidateAsync(entity, cancellation);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid data {EndAt}, {Content} and {AssignedToDeveloperId}", entity.EndAt, entity.Content, entity.AssignedToDeveloperId);
                throw new BadRequestException("Invalid data");
            }

            var taskEntity = new TaskEntity(entity.StartAt, entity.EndAt, entity.Content, TaskProgress.NotStarted);
            taskEntity.AssignToDeveloper(entity.AssignedToDeveloperId);

            await _unitOfWork.Repository<TaskEntity>().CreateAsync(taskEntity, cancellation);
            await _unitOfWork.SaveChangesAsync(cancellation);

            _logger.LogInformation("Task created and assigned successfully");

            return MapToModel(taskEntity);
        }

        public async Task<bool> ChangeTaskStatus(Guid taskId, TaskProgress progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to change status of task {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);

            if (task is null)
            {
                _logger.LogWarning("Task with {TaskId} not found", taskId);
                throw new ItemNotFoundException("Task not found");
            }

            if (task.Progress == progress)
            {
                _logger.LogWarning("Progress of task with {TaskId} already set", taskId);
                throw new BadRequestException("Task progress already set");
            }

            task.SetProgress(progress);

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task progress changed successfully");

            return true;
        }

        public async Task<bool> DeleteTaskById(Guid taskId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete task with {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);

            if (task is null)
            {
                _logger.LogWarning("Task with {TaskId} not found", taskId);
                throw new ItemNotFoundException("Task not found");
            }

            await _unitOfWork.Repository<TaskEntity>().DeleteAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task with {TaskId} deleted successfully", taskId);

            return true;
        }

        public async Task<TaskEntityViewModel> GetTaskById(Guid taskId)
        {
            _logger.LogInformation("Attempting to get task by {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);

            if (task is null)
            {
                _logger.LogWarning("Task with {TaskId} not found", taskId);
                throw new ItemNotFoundException("Task not found");
            }

            return MapToModel(task);
        }

        public async Task<PagesResult<TaskEntityViewModel>> GetTasks(Guid userId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to get tasks for {UserId} with paging", userId);

            var developer = await _unitOfWork.Repository<Developer>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (developer is null)
            {
                _logger.LogWarning("Developer with {UserId} not found", userId);
                throw new ItemNotFoundException("Developer not found");
            }

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var tasks = await _unitOfWork.Repository<TaskEntity>()
                .GetAllAsync(x => x.AssignedToDeveloperId == developer.Id, null, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Repository<TaskEntity>().CountAsync(x => x.AssignedToDeveloperId == developer.Id);

            var tasksVM = tasks.Select(MapToModel).ToList();

            return new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount);
        }


        public async Task<PagesResult<TaskEntityViewModel>> GetTasks(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to get tasks with paging");

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var tasks = await _unitOfWork.Repository<TaskEntity>()
                .GetAllAsync(null, null, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Repository<TaskEntity>().CountAsync();

            var tasksVM = tasks.Select(MapToModel).ToList();

            return new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount);
        }

        public async Task<PagesResult<TaskEntityViewModel>> GetTasksByStatus(TaskProgress taskProgress, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to get tasks with {TaskProgress} and with paging", taskProgress);

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var taskRepo = _unitOfWork.Repository<TaskEntity>();

            var tasks = await taskRepo.GetAllAsync(x => x.Progress == taskProgress,
                                                        x => x.OrderByDescending(x => x.CreatedAt),
                                                        pageNumber, pageSize);
            
            var totalCount = await taskRepo.CountAsync();

            var tasksVM = tasks.Select(MapToModel).ToList();

            return new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount);
        }

        public async Task<TaskEntityViewModel> UpdateTaskDetails(UpdateTaskEntity taskEntity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update task with {TaskId}", taskEntity.Id);

            var validationResult = await _updateTaskValidator.ValidateAsync(taskEntity);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid data {EndAt}, {StartAt}, {Progress}, {Content} and {AssignedToDeveloperId}", taskEntity.EndAt, taskEntity.StartAt, taskEntity.Progress, taskEntity.Content, taskEntity.AssignedToDeveloperId);
                throw new BadRequestException("Invalid data");
            }

            var task = await _unitOfWork.Repository<TaskEntity>().FirstOrDefaultAsync(x => x.Id == taskEntity.Id);

            if(task is null)
            {
                _logger.LogWarning("Task with {TaskId} not found", taskEntity.Id);
                throw new ItemNotFoundException("Task not found");
            }

            task.SetStartAt(taskEntity.StartAt);
            task.SetEndAt(taskEntity.EndAt);
            task.SetContent(taskEntity.Content);
            task.SetProgress(taskEntity.Progress);
            task.AssignToDeveloper(taskEntity.AssignedToDeveloperId);

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TaskEntityViewModel
            {
                Id = task.Id,
                StartAt = taskEntity.StartAt,
                EndAt = taskEntity.EndAt,
                Content = taskEntity.Content,
                IsFinished = task.IsFinished,
                Progress = taskEntity.Progress,
                AssignedToDeveloperId = taskEntity.AssignedToDeveloperId,
            };
        }

        private static TaskEntityViewModel MapToModel(TaskEntity entity)
        {
            return new TaskEntityViewModel
            {
                Id = entity.Id,
                AssignedToDeveloperId = entity.AssignedToDeveloperId,
                Content = entity.Content,
                EndAt = entity.EndAt,
                IsFinished = entity.IsFinished,
                Progress = entity.Progress,
                StartAt = entity.StartAt,
            };
        }
    }
}
