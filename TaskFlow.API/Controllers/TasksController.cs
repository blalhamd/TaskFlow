using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Controllers.Base;
using TaskFlow.API.Extensions;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Common;

namespace TaskFlow.API.Controllers
{
    /// <summary>
    /// API controller for managing tasks.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tasks")]
    public class TasksController : BaseApiController
    {
        private readonly ITaskService _taskService;
        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Assigns a new task to a user.
        /// </summary>
        /// <param name="entity">Task creation data.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Created task entity view model.</returns>
        [HttpPost]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(TaskEntityViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignTaskEntity([FromForm] CreateTaskEntity entity, CancellationToken cancellation = default)
        {
            var result = await _taskService.AssignTaskEntity(entity, cancellation);
            return Success(result);
        }

        /// <summary>
        /// Changes the status of a task.
        /// </summary>
        /// <param name="taskId">Task identifier.</param>
        /// <param name="progress">New task progress.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Status change result.</returns>
        [HttpPatch("{taskId:guid}/status")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = $"{ApplicationConstants.Admin},{ApplicationConstants.Developer}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeTaskStatus(Guid taskId, [FromQuery] TaskProgress progress, CancellationToken cancellationToken = default)
        {
            var result = await _taskService.ChangeTaskStatus(taskId, progress, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// Deletes a task by its identifier.
        /// </summary>
        /// <param name="taskId">Task identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Delete operation result.</returns>
        [HttpDelete("{taskId:guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTaskById(Guid taskId, CancellationToken cancellationToken = default)
        {
            var result = await _taskService.DeleteTaskById(taskId, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// Gets a task by its identifier.
        /// </summary>
        /// <param name="taskId">Task identifier.</param>
        /// <returns>Task entity view model.</returns>
        [HttpGet("{taskId:guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = $"{ApplicationConstants.Admin},{ApplicationConstants.Developer}")]
        [ProducesResponseType(typeof(TaskEntityViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var result = await _taskService.GetTaskById(taskId);
            return Success(result);
        }

        /// <summary>
        /// Gets paged tasks for the authenticated developer.
        /// </summary>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged result of tasks.</returns>
        [HttpGet("developer")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Developer)]
        [ProducesResponseType(typeof(PagesResult<TaskEntityViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDeveloperTasks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated");

            var result = await _taskService.GetTasks(Guid.Parse(userId), pageNumber, pageSize);
            return Success(result);
        }

        /// <summary>
        /// Gets paged tasks for admin (all tasks).
        /// </summary>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Paged result of tasks.</returns>
        [HttpGet("admin")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(PagesResult<TaskEntityViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminTasks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _taskService.GetTasks(pageNumber, pageSize);
            return Success(result);
        }

        /// <summary>
        /// Gets paged tasks filtered by status.
        /// </summary>
        /// <param name="progress">Task progress status to filter by.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size (max 10).</param>
        /// <returns>Paged result of tasks with specified status.</returns>
        [HttpGet("by-status")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Manager)]
        [ProducesResponseType(typeof(PagesResult<TaskEntityViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTasksByStatus(
            [FromQuery] TaskProgress progress,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _taskService.GetTasksByStatus(progress, pageNumber, pageSize);
            return Success(result);
        }

        /// <summary>
        /// Updates the details of an existing task.
        /// </summary>
        /// <param name="request">The updated task details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated task entity view model.</returns>
        [HttpPut]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Manager)]
        [ProducesResponseType(typeof(TaskEntityViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTaskDetails(
            [FromForm] UpdateTaskEntity request,
            CancellationToken cancellationToken = default)
        {
            var result = await _taskService.UpdateTaskDetails(request, cancellationToken);
            return Success(result);
        }

        [HttpPost("{taskId}/comment")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Developer)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddCommentToTask(Guid taskId, CreateCommentRequest request, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated");

            return Success(await _taskService.AddCommentToTask(Guid.Parse(userId), taskId, request, cancellationToken));
        }

        [HttpGet("{taskId}/comments")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Developer)]
        [ProducesResponseType(typeof(List<CommentViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCommentsForTask(Guid taskId)
            => Success(await _taskService.GetCommentsForTask(taskId));

    }
}
