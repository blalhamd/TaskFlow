namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class LoginResponse
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public int ExpireIn { get; set; }
    }
}
