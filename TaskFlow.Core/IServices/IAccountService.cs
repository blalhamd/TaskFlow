namespace TaskFlow.Core.IServices
{
    public interface IAccountService
    {
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword); // userId from claims
    }
}

