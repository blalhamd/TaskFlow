using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Extensions;
using TaskFlow.API.Models;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
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
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.GetUserId();

            if (userId is null)
                return Unauthorized("Unauthorized user");

            var result = await _accountService.ChangePasswordAsync(Guid.Parse(userId), request.CurrentPassword, request.NewPassword);
            return Ok(result);
        }
    }

}
