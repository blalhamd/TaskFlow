namespace TaskFlow.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresOn { get; set; }
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? RevokedOn { get; set; }
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOn;
        public bool IsActive => RevokedOn is null && !IsExpired;
    }
}
