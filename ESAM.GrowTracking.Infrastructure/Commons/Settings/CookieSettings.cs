using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ESAM.GrowTracking.Infrastructure.Commons.Settings
{
    public class CookieSettings
    {
        public string CookieName { get; set; } = "rt";

        public bool UseHostPrefix { get; set; } = false;

        public bool AllowRefreshTokenHeader { get; set; } = true;

        public bool AlwaysSecure { get; set; } = false;

        public string XsrfCookieName { get; set; } = "XSRF-T";

        public int XsrfCookieExpiresMinutes { get; set; } = 30;

        public string? Domain { get; set; } = null;

        public SameSiteMode SameSite { get; set; } = SameSiteMode.None;

        public string Path { get; set; } = "/";

        public List<string> AllowedOrigins { get; set; } = [];

        public string EffectiveRefreshCookieName() => UseHostPrefix ? "__Host-" + CookieName : CookieName;

        public string EffectiveXsrfCookieName() => UseHostPrefix ? "__Host-" + XsrfCookieName : XsrfCookieName;

        public void Validate()
        {
            Guard.AgainstNullOrWhiteSpace(CookieName, "CookieName cannot be empty.");
            if (UseHostPrefix)
            {
                Guard.Against(!string.Equals(Path, "/", StringComparison.Ordinal), "When UseHostPrefix is true, Path must be '/'.");
                Guard.Against(!string.IsNullOrWhiteSpace(Domain), "When UseHostPrefix is true, Domain must be null or empty (host-prefixed cookies must not specify a domain).");
                Guard.Against(!AlwaysSecure, "When UseHostPrefix is true, AlwaysSecure must be true (host-prefixed cookies require Secure).");
            }
        }
    }
}