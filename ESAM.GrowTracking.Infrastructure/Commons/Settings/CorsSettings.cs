namespace ESAM.GrowTracking.Infrastructure.Commons.Settings
{
    public class CorsSettings
    {
        public const string SectionName = "CorsSettings";

        public string PolicyName { get; set; } = "CorsPolicy";

        public List<string> AllowedOrigins { get; set; } = [];

        public List<string> AllowedOriginWildcards { get; set; } = [];

        public List<string> AllowedOriginRegex { get; set; } = [];

        public List<string> AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Requested-With", "X-XSRF-TOKEN", "X-Refresh-Token"];

        public List<string> AllowedMethods { get; set; } = ["GET", "HEAD", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];

        public List<string> ExposeHeaders { get; set; } = ["X-XSRF-TOKEN"];

        public bool AllowCredentials { get; set; } = true;

        public int PreflightMaxAgeSeconds { get; set; } = 600;

        public bool AllowHttpOnLocalhost { get; set; } = false;

        public List<int> LocalhostPorts { get; set; } = [3000, 4200, 8080];

        public bool EnforceStrictOriginsInProduction { get; set; } = true;
    }
}