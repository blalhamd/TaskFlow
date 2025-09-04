using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskFlow.Core.IServices;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Shared.Exceptions;

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

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                _logger.LogWarning("ChangePassword failed: User with ID {UserId} not found.", userId);
                throw new ItemNotFoundException("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                _logger.LogWarning("ChangePassword failed for user {UserId}. Errors: {Errors}", userId, errors);
                throw new BadRequestException(errors);
            }

            _logger.LogInformation("Password successfully changed for user {UserId}", userId);
            return true;
        }
    }
}
