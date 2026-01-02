using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskFlow.Business.Helper.Socket;
using TaskFlow.Core.IServices;
using TaskFlow.Core.IUnit;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;
using TaskFlow.Shared.Common;
using Error = TaskFlow.Domain.Common.Error;

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

        public TaskService(
            IUnitOfWorkAsync unitOfWork,
            IImageService imageService,
            ILogger<TaskService> logger,
            IValidator<CreateTaskEntity> createTaskValidator,
            IValidator<UpdateTaskEntity> updateTaskValidator,
            IConfiguration configuration,
            IHubContext<TaskHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _logger = logger;
            _createTaskValidator = createTaskValidator;
            _updateTaskValidator = updateTaskValidator;
            _configuration = configuration;
            _hubContext = hubContext;
            _baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5001/images/"; 
        }

        public async Task<ValueResult<TaskEntityViewModel>> AssignTaskEntity(CreateTaskEntity entity, CancellationToken cancellation)
        {
            _logger.LogInformation("Creating task for developer {DeveloperId}", entity.AssignedToDeveloperId);

            // Validation
            var validationResult = await _createTaskValidator.ValidateAsync(entity, cancellation);
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.First();
                return ValueResult<TaskEntityViewModel>.Failure(new Error(error.ErrorCode, error.ErrorMessage, ErrorType.Validation));
            }

            // Upload Image
            string? documentPath = null;
            if(entity.Document is not null)
            {
               documentPath = await _imageService.UploadImageOnServer(entity.Document, false, null!, cancellation);
            }

            // create task 
            var result = TaskEntity.Create(entity.StartAt, entity.EndAt, entity.Content, documentPath);
            if (!result.IsSuccess)
            {
                if (documentPath != null) _imageService.RemoveImage(documentPath);
                return ValueResult<TaskEntityViewModel>.Failure(result.Error);
            }

            result.Value.AssignToDeveloper(entity.AssignedToDeveloperId);

            try
            {
                await _unitOfWork.Repository<TaskEntity>().CreateAsync(result.Value, cancellation);
                await _unitOfWork.SaveChangesAsync(cancellation);

                var taskVM = MapToModel(result.Value);

                await _hubContext.Clients.All.SendAsync("assigntask", taskVM, cancellation);

                _logger.LogInformation("Task created and assigned successfully");
                return ValueResult<TaskEntityViewModel>.Success(taskVM);
            }
            catch (Exception)
            {
                if (documentPath != null) _imageService.RemoveImage(documentPath);
                throw; // will handle by global handling middlerware
            }
        }

        public async Task<Result> ChangeTaskStatus(Guid taskId, TaskProgress progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to change status of task {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);
            if (task is null)
                return Result.Failure(TaskErrors.NotFound);

            if (task.Progress == progress)
                return Result.Success();

            var result = task.UpdateProgress(progress);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task progress changed successfully");

            return Result.Success();
        }

        public async Task<Result> DeleteTaskById(Guid taskId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to delete task with {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);
            if (task is null)
                return Result.Failure(TaskErrors.NotFound);

            await _unitOfWork.Repository<TaskEntity>().DeleteAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task with {TaskId} deleted successfully", taskId);
            await _hubContext.Clients.All.SendAsync("deletetask", task, cancellationToken);

            return Result.Success();
        }

        public async Task<ValueResult<TaskEntityViewModel>> GetTaskById(Guid taskId)
        {
            _logger.LogInformation("Attempting to get task by {TaskId}", taskId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetByIdAsync(taskId);
            if (task is null)
                return ValueResult<TaskEntityViewModel>.Failure(TaskErrors.NotFound);

            return ValueResult<TaskEntityViewModel>.Success(MapToModel(task));
        }

        public async Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasks(Guid userId, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to get tasks for {UserId} with paging", userId);

            var developer = await _unitOfWork.Repository<Developer>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (developer is null)
                return ValueResult<PagesResult<TaskEntityViewModel>>.Failure(DeveloperErrors.NotFound);

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var tasks = await _unitOfWork.Repository<TaskEntity>()
                .GetAllAsync(x => x.AssignedToDeveloperId == developer.Id, null, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Repository<TaskEntity>().CountAsync(x => x.AssignedToDeveloperId == developer.Id);

            var tasksVM = tasks.Select(MapToModel).ToList();

            return ValueResult<PagesResult<TaskEntityViewModel>>.Success(new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount));
        }


        public async Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasks(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to get tasks with paging");

            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 10);

            var tasks = await _unitOfWork.Repository<TaskEntity>()
                .GetAllAsync(null, null, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Repository<TaskEntity>().CountAsync();

            var tasksVM = tasks.Select(MapToModel).ToList();

            return ValueResult<PagesResult<TaskEntityViewModel>>.Success(new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount));
        }

        public async Task<ValueResult<PagesResult<TaskEntityViewModel>>> GetTasksByStatus(TaskProgress taskProgress, int pageNumber, int pageSize)
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

            return ValueResult<PagesResult<TaskEntityViewModel>>.Success(new PagesResult<TaskEntityViewModel>(tasksVM, pageNumber, pageSize, (int)totalCount));
        }

        public async Task<ValueResult<TaskEntityViewModel>> UpdateTaskDetails(UpdateTaskEntity taskEntity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update task with {TaskId}", taskEntity.Id);

            var validationResult = await _updateTaskValidator.ValidateAsync(taskEntity);
            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.First();
                return ValueResult<TaskEntityViewModel>.Failure(new Error(error.ErrorCode, error.ErrorMessage, ErrorType.Validation));
            }

            var task = await _unitOfWork.Repository<TaskEntity>().FirstOrDefaultAsync(x => x.Id == taskEntity.Id);
            if(task is null)
                return ValueResult<TaskEntityViewModel>.Failure(TaskErrors.NotFound);

            string? oldPath = task.Document;
            string? newPath = null;

            if (taskEntity.Document != null)
            {
                newPath = await _imageService.UploadImageOnServer(taskEntity.Document, false, oldPath!, cancellationToken);
            }

            var updateResult = task.Update(taskEntity.StartAt, taskEntity.EndAt, taskEntity.Content, newPath);
            if (!updateResult.IsSuccess)
            {
                if (newPath != null) _imageService.RemoveImage(newPath); // Cleanup new file
                return ValueResult<TaskEntityViewModel>.Failure(updateResult.Error);
            }
            task.AssignToDeveloper(taskEntity.AssignedToDeveloperId);

            try
            {
                await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var taskVM = MapToTaskVM(task);

                await _hubContext.Clients.All.SendAsync("updatetask", taskVM, cancellationToken);

                if (newPath != null && !string.IsNullOrEmpty(oldPath))
                {
                    _imageService.RemoveImage(oldPath);
                }

                return ValueResult<TaskEntityViewModel>.Success(taskVM!);
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(newPath))
                    _imageService.RemoveImage(newPath);
                throw;
            }
        }

        public async Task<ValueResult<CommentViewModel>> AddCommentToTask(Guid userId, Guid taskId, CreateCommentRequest request, CancellationToken cancellationToken)
        {
            var developer = await _unitOfWork.Repository<Developer>().FirstOrDefaultAsync(x => x.UserId == userId);
            if (developer is null)
                return ValueResult<CommentViewModel>.Failure(UserErrors.NotFound);

            var task = await _unitOfWork.Repository<TaskEntity>().FirstOrDefaultAsync(x => x.Id == taskId, x => x.AssignedToDeveloper);
            if (task is null)
                return ValueResult<CommentViewModel>.Failure(TaskErrors.NotFound);

            var result = Comment.Create(request.Content, taskId, developer.Id);
            if (!result.IsSuccess)
                return ValueResult<CommentViewModel>.Failure(result.Error);


            task.Comments.Add(result.Value);

            await _unitOfWork.Repository<TaskEntity>().UpdateAsync(task, cancellationToken);
            var success = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

            if (success)
            {
                // notify the assigned developer that someone commented on his task
                var assignedUserId = task.AssignedToDeveloper.UserId.ToString();
                await _hubContext.Clients.User(assignedUserId)
                    .SendAsync("notifycomment",
                    new
                    {
                        From = developer.FullName,
                        Content = result.Value.Content,
                        TaskTitle = task.Content
                    },
                    cancellationToken);
            }

            return ValueResult<CommentViewModel>.Success(MapToCommentVM(comment: result.Value, developer.FullName));
        }

        public async Task<ValueResult<List<CommentViewModel>>> GetCommentsForTask(Guid taskId)
        {
            var taskDto = await _unitOfWork.taskRepositoryAsync.FirstOrDefaultAsync(x => x.Id == taskId);
            if (taskDto is null)
                return ValueResult<List<CommentViewModel>>.Failure(TaskErrors.NotFound);

            var commentsVm = taskDto.CommentDtos.Select(MapToCommentViewModel).ToList();

            return ValueResult<List<CommentViewModel>>.Success(commentsVm);
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

        private CommentViewModel MapToCommentVM(Comment comment, string fullName)
            => new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                FullName = fullName,
                CreatedAt = comment.CreatedAt,
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
