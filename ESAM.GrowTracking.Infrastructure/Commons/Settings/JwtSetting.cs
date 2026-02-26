namespace ESAM.GrowTracking.Infrastructure.Commons.Settings
{
    public sealed class JwtSetting
    {
        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public string SecretKey { get; init; } = string.Empty;
    }
}