namespace ESAM.GrowTracking.Infrastructure.Commons.Settings
{
    public class ForwardedHeadersSettings
    {
        public List<string> KnownNetworks { get; init; } = [];

        public List<string> KnownProxies { get; init; } = [];
    }
}