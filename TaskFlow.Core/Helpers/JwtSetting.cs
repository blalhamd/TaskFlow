namespace TaskFlow.Core.Helpers
{
    public class JwtSetting
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int LifeTime { get; set; }
        public string Key { get; set; } = null!;
    }
}
