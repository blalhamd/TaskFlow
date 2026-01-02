using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskFlow.Core.IServices;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Business.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountService> _logger;
        public AccountService(UserManager<ApplicationUser> userManager, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Result.Failure(UserErrors.EmptyPassword);
            }

            var parsedUserId = userId.ToString();
            var user = await _userManager.FindByIdAsync(parsedUserId);
            if (user is null)
            {

                _logger.LogWarning("ChangePassword failed: User with ID {UserId} not found.", userId);
                return Result.Failure(UserErrors.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();

                if(error is not null)
                {
                    _logger.LogWarning("ChangePassword failed for user {UserId}. Code: {Code}, Error: {Description}",
                               userId, error.Code, error.Description);
                    return Result.Failure(new Error(error.Code, error.Description, ErrorType.Validation));
                }
            }

            _logger.LogInformation("Password successfully changed for user {UserId}", userId);
            return Result.Success();
        }
    }
}
