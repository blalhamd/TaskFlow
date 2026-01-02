using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Controllers.Base;
using TaskFlow.API.Extensions;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accounts")]
    public class AccountsController : BaseApiController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Change the password of the currently logged-in user.
        /// </summary>
        /// <param name="request">The change password request containing current and new passwords.</param>
        /// <returns>Returns <c>true</c> if the password was successfully changed.</returns>
        [HttpPost("change-password")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.GetUserId();

            if (userId is null)
                return Unauthorized("Unauthorized user");

            return Success(await _accountService.ChangePasswordAsync(Guid.Parse(userId), request.CurrentPassword, request.NewPassword));
        }
    }

}
