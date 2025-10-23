using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;

namespace TaskFlow.Core.IServices
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<LoginResponse> GetRefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken);
    }
}
