using System.Security.Claims;

namespace TaskFlow.API.Extensions
{
    public static class UserClaims
    {
        public static string? GetUserId(this ClaimsPrincipal claimPrinciple) 
            => claimPrinciple.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
