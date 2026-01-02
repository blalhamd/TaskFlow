using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;

namespace TaskFlow.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Authenticates a user with their credentials and issues a JWT token.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <returns>
        /// A <see cref="LoginResponse"/> containing the JWT token, expiry time, and user details if authentication is successful.  
        /// Returns a <see cref="ProblemDetails"/> if authentication fails.
        /// </returns>
        /// <response code="200">Returns the generated JWT token and user information.</response>
        /// <response code="400">If the request is invalid or credentials are incorrect.</response>

        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request)
          => Ok(await _authenticationService.LoginAsync(request));


        /// <summary>
        /// Sends a password reset token to the specified email address.
        /// </summary>
        /// <param name="request">The request containing the user's email.</param>
        /// <returns>The generated password reset token (should typically be sent via email).</returns>
        [HttpPost("forgot-password")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
           => Ok(await _authenticationService.GeneratePasswordResetTokenAsync(request.Email));

        /// <summary>
        /// Resets the user's password using the provided reset token.
        /// </summary>
        /// <param name="request">The request containing email, reset token, and new password.</param>
        /// <returns><c>true</c> if reset succeeded; <c>false</c> with error response if not.</returns>
        [HttpPost("reset-password")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
           => Ok(await _authenticationService.ResetPasswordAsync(request));

        /// <summary>
        /// Generates a new JWT access token and a new refresh token using an existing, valid refresh token.
        /// </summary>
        /// <param name="request">The request body containing the expired JWT <see cref="RefreshTokenRequest.Token"/> and the current <see cref="RefreshTokenRequest.RefreshToken"/>.</param>
        /// <returns>
        /// A <see cref="Task{IActionResult}"/> containing the new JWT and Refresh Token pair if successful,
        /// or a <see cref="StatusCodes.Status400BadRequest"/> if the tokens are invalid or expired.
        /// </returns>
        [HttpPost("refresh-token")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshAsync(RefreshTokenRequest request)
        {
            var result = await _authenticationService.GetRefreshTokenAsync(request.Token, request.RefreshToken);

            return result is null ? BadRequest("Invalid token") : Ok(result);
        }

        /// <summary>
        /// Revokes a specific refresh token, immediately invalidating it and preventing its future use.
        /// </summary>
        /// <param name="request">The request body containing the access token and the <see cref="RefreshTokenRequest.RefreshToken"/> to be revoked.</param>
        /// <returns>
        /// A <see cref="Task{IActionResult}"/> containing a boolean result wrapped in an <see cref="StatusCodes.Status200OK"/>
        /// if the token was successfully revoked, or a <see cref="StatusCodes.Status400BadRequest"/> if the operation failed.
        /// </returns>
        [HttpPut("revoke-refresh-token")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenRequest request)
        {
            var result = await _authenticationService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken);

            return result is false? BadRequest("Failed operation") : Ok(result);
        }
    }
}
