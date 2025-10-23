namespace TaskFlow.Core.Models.ViewModels.V1
{
    public class JwtProviderResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset TokenExpiration { get; set; }
    }
}
