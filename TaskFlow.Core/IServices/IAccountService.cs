using TaskFlow.Domain.Common;

namespace TaskFlow.Core.IServices
{
    public interface IAccountService
    {
        Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword); // userId from claims
    }
}

