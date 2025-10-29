using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskFlow.Business.Helper.Socket;
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
        private readonly IImageService _imageService;
        private readonly ILogger<TaskService> _logger;
        private readonly IValidator<CreateTaskEntity> _createTaskValidator;
        private readonly IValidator<UpdateTaskEntity> _updateTaskValidator;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<TaskHub> _hubContext;
        private readonly string _baseUrl;
        public TaskService(IUnitOfWorkAsync unitOfWork, IImageService imageService, ILogger<TaskService> logger, IValidator<CreateTaskEntity> createTaskValidator, IValidator<UpdateTaskEntity> updateTaskValidator, IConfiguration configuration, IHubContext<TaskHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _logger = logger;
            _createTaskValidator = createTaskValidator;
            _updateTaskValidator = updateTaskValidator;
            _configuration = configuration;
            _hubContext = hubContext;
            _baseUrl = _configuration["BaseUrl"] ?? "default.pdf";
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

            string? documentPath = null;
            if(entity.Document is not null)
            {
               documentPath = await _imageService.UploadImageOnServer(entity.Document, false, null!, cancellation);
            }

            var taskEntity = new TaskEntity(entity.StartAt, entity.EndAt, entity.Content, documentPath, TaskProgress.NotStarted);
            taskEntity.AssignToDeveloper(entity.AssignedToDeveloperId);

            await _unitOfWork.Repository<TaskEntity>().CreateAsync(taskEntity, cancellation);
            await _unitOfWork.SaveChangesAsync(cancellation);

            _logger.LogInformation("Task created and assigned successfully");

            var taskVM = MapToModel(taskEntity);

            await _hubContext.Clients.All.SendAsync("assigntask", taskVM, cancellation);

            return taskVM; 
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
            await _hubContext.Clients.All.SendAsync("deletetask", task, cancellationToken);

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
            task.SetProgress(taskEntity.Progress);
            task.AssignToDeveloper(taskEntity.AssignedToDeveloperId);
            if (!string.IsNullOrEmpty(taskEntity.Content))
            {
                task.SetContent(taskEntity.Content);
            }

            string? oldPath = task.Document;
            string? newPath = null;

            if(taskEntity.Document != null)
            {
                newPath = await _imageService.UploadImageOnServer(taskEntity.Document,false,oldPath!,cancellationToken);
            }
            if (!string.IsNullOrEmpty(newPath))
            {
                task.SetDocument(newPath);
            }

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if(rowsAffected < 0)
            {
                if (!string.IsNullOrEmpty(newPath))
                {
                    await _imageService.RemoveImage(newPath);
                }
            }

            var taskVM = MapToTaskVM(task);

            await _hubContext.Clients.All.SendAsync("updatetask", taskVM, cancellationToken);

            return taskVM!;
        }

        public async Task<bool> AddCommentToTask(Guid userId, Guid taskId, CreateCommentRequest request, CancellationToken cancellationToken)
        {
            var developer = await _unitOfWork.Repository<Developer>().FirstOrDefaultAsync(x => x.UserId == userId);

            if (developer is null)
                throw new ItemNotFoundException("user not found"); // 89353308930a

            var task = await _unitOfWork.Repository<TaskEntity>().FirstOrDefaultAsync(x => x.Id == taskId);

            if (task is null)
                throw new ItemNotFoundException("task not found");

            var comment = new Comment
            {
                Content = request.Content,
                DeveloperId = developer.Id,
                TaskEntityId = taskId,
            };

            task.Comments.Add(comment);

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<List<CommentViewModel>> GetCommentsForTask(Guid taskId)
        {
            var taskDto = await _unitOfWork.taskRepositoryAsync
                             .FirstOrDefaultAsync(x => x.Id == taskId);

            if (taskDto is null)
                throw new ItemNotFoundException("task not found");

            var commentsVm = taskDto.CommentDtos.Select(MapToCommentViewModel)
                .ToList();

            return commentsVm;
        }

        private TaskEntityViewModel? MapToTaskVM(TaskEntity task)
            => new TaskEntityViewModel
            {
                Id = task.Id,
                StartAt = task.StartAt,
                EndAt = task.EndAt,
                Content = task.Content,
                Document = string.IsNullOrEmpty(task.Document) ? _baseUrl : $"{_baseUrl}{task.Document}",
                IsFinished = task.IsFinished,
                Progress = task.Progress,
                AssignedToDeveloperId = task.AssignedToDeveloperId,
            };

        private TaskEntityViewModel MapToModel(TaskEntity entity)
        {
            return new TaskEntityViewModel
            {
                Id = entity.Id,
                AssignedToDeveloperId = entity.AssignedToDeveloperId,
                Content = entity.Content?? string.Empty,
                Document = string.IsNullOrEmpty(entity.Document) ? _baseUrl : $"{_baseUrl}{entity.Document}",
                EndAt = entity.EndAt,
                IsFinished = entity.IsFinished,
                Progress = entity.Progress,
                StartAt = entity.StartAt,
            };
        }


        private CommentViewModel MapToCommentViewModel(CommentDto c) => new CommentViewModel
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            FullName = c.FullName
        };
    }
}
