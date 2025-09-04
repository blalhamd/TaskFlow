namespace TaskFlow.Core.Models.Dtos.V1
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
