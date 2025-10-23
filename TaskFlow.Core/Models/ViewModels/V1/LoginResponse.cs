namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class LoginResponse
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTimeOffset TokenExpiration { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTimeOffset RefreshTokenExpiration { get; set; } 
    }
}
