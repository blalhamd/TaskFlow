using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Cryptography;
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
        private readonly int _refreshTokenExpireIn = 15; // 15 days
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

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(_refreshTokenExpireIn);

            // save refresh token in DB
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
            });

            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                UserName = user.UserName ?? "Unknown",
                Email = user.Email ?? request.Email,
                Token = tokenResult.Token,
                TokenExpiration = tokenResult.TokenExpiration,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpiration
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

        public async Task<LoginResponse> GetRefreshTokenAsync(string token, string refreshToken)
        {
            // validate token and extract userId from claims of token
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return null!;

            // get user by extracted userId. if not exist? return null
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return null!;

            // check returned user has this refresh token. if exist? revoke it : return null
            var userRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefreshToken is null)
                return null!;

            // revoke old refresh token
            userRefreshToken.RevokedOn = DateTimeOffset.UtcNow;

            // generate new jwt token
            (var roles, var permissions) = await GetRolesAndPermissions(user);
            var jwtProviderResponse = _jwtProvider.GenerateToken(user, roles, permissions);

            // generate new referesh token
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(_refreshTokenExpireIn);

            // add new refresh token to user
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = newRefreshTokenExpiration,
            });

            // update user
            await _userManager.UpdateAsync(user);

            // add refresh token and expire it to login response
            return new LoginResponse
            {
                UserName = user.UserName ?? "UnKnown",
                Email = user.Email ?? "UnKnown",
                Token = jwtProviderResponse.Token,
                TokenExpiration = jwtProviderResponse.TokenExpiration,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiration = newRefreshTokenExpiration
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken)
        {
            // validate token and extract userId from claims of token
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return false;

            // get user by extracted userId. if not exist? return null
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            // check returned user has this refresh token. if exist? revoke it : return null
            var userRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefreshToken is null)
                return false;

            // revoke old refresh token
            userRefreshToken.RevokedOn = DateTimeOffset.UtcNow;

            // update user
            await _userManager.UpdateAsync(user);

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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
