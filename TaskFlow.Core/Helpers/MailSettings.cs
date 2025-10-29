namespace TaskFlow.Core.Helpers
{
    public class MailSettings
    {
        public const string SectionName = "MailSettings";
        public string DisplayName { get; set; } = null!;
        public int Port { get; set; }
        public string Host { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
