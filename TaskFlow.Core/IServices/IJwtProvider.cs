using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Entities.Identity;

namespace TaskFlow.Core.IServices
{
    public interface IJwtProvider
    {
        JwtProviderResponse GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
        
        // to validate tokens that come with requests that ask new jwt token by refresh token
        string? ValidateToken(string token);
    }
}
