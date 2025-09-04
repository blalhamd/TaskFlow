namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class JwtProviderResponse
    {
        public string Token { get; set; } = null!;
        public int ExpireIn { get; set; }
    }
}
