using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Models;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Shared.Common;

namespace TaskFlow.API.Controllers
{
    [ApiVersion("1.0")]    
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DevelopersController : ControllerBase
    {
        private readonly IDeveloperService _developerService;

        public DevelopersController(IDeveloperService developerService)
        {
            _developerService = developerService;
        }

        /// <summary>
        /// Retrieves a paginated list of developers from the TaskFlow system.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="pageIndex"/> and <paramref name="pageSize"/> to control pagination.  
        /// Example: <c>GET /api/v1/developers?pageIndex=1&pageSize=10</c>
        /// </remarks>
        /// <param name="pageIndex">The page index (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>A paginated list of developers.</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(PagesResult<DeveloperViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagesResult<DeveloperViewModel>>> GetDevelopers(int pageIndex = 1, int pageSize = 10)
            => Ok(await _developerService.GetAllDevelopers(pageIndex, pageSize));


        /// <summary>
        /// Retrieves a specific developer by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the developer.</param>
        /// <returns>The developer details.</returns>
        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(DeveloperViewViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeveloperViewViewModel>> GetDeveloper(Guid id)
            => Ok(await _developerService.GetById(id));


        /// <summary>
        /// Registers a new developer in the TaskFlow system.
        /// </summary>
        /// <remarks>
        /// This endpoint accepts a form with developer details.  
        /// Example: <c>POST /api/v1/developers</c>
        /// </remarks>
        /// <param name="request">The developer registration request.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>Returns <c>true</c> if the developer was successfully created.</returns>
        [HttpPost]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = ApplicationConstants.Admin)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CreateDeveloper([FromForm] CreateDeveloperRequest request, CancellationToken cancellationToken = default)
            => Ok(await _developerService.CreateDeveloper(request, cancellationToken));

        /// <summary>
        /// Updates details of a developer.
        /// </summary>
        /// <param name="request">The update request object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if update succeeded.</returns>
        [HttpPut]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Manager)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDeveloper(
            UpdateDeveloperRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _developerService.UpdateDeveloper(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a developer by Id.
        /// </summary>
        /// <param name="developerId">Developer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deletion succeeded.</returns>
        [HttpDelete("{developerId}")]
        [Authorize(Roles = ApplicationConstants.Admin + "," + ApplicationConstants.Manager)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDeveloper(Guid developerId, CancellationToken cancellationToken)
        {
            var result = await _developerService.DeleteDeveloper(developerId, cancellationToken);
            return Ok(result);
        }
    }
}
