using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskFlow.Core.IServices;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ResetPasswordRequest> _resetValidator;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider, ILogger<AuthenticationService> logger, IValidator<LoginRequest> loginValidator, IValidator<ResetPasswordRequest> resetValidator)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _logger = logger;
            _loginValidator = loginValidator;
            _resetValidator = resetValidator;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Validate request
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return HandleInvalidLogin();

            // Find user
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return HandleInvalidLogin();

            // Validate password
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return HandleInvalidLogin();

            // Roles & permissions
            var (roles, permissions) = await GetRolesAndPermissions(user);

            // Generate token
            var tokenResult = _jwtProvider.GenerateToken(user, roles, permissions);

            return new LoginResponse
            {
                UserName = user.UserName ?? "Unknown",
                Email = user.Email ?? request.Email,
                Token = tokenResult.Token,
                ExpireIn = tokenResult.ExpireIn * 60
            };
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogWarning("Password reset token generation failed: User with email {Email} not found.", email);
                return string.Empty;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Password reset token generated successfully for user {Email}", email);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Validate request
            var validationResult = await _resetValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(',', validationResult.Errors.Select(x => x.ErrorMessage).ToList());
                _logger.LogWarning(errors);
                throw new BadRequestException(errors);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Password reset failed: User with email {Email} not found.", request.Email);
                throw new ItemNotFoundException("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                _logger.LogWarning("Password reset failed for user {Email}. Errors: {Errors}", request.Email, errors);
                throw new BadRequestException($"Password reset failed: {errors}");
            }

            _logger.LogInformation("Password reset successfully for user {Email}", request.Email);
            return true;
        }


        private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetRolesAndPermissions(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _userManager.GetClaimsAsync(user);
            return (roles, permissions.Select(c => c.Value));
        }

        private LoginResponse HandleInvalidLogin()
        {
            _logger.LogWarning("Invalid login attempt with provided credentials.");
            throw new BadRequestException("Invalid email or password");
        }
    }
}
