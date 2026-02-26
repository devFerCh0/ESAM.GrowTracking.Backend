namespace ESAM.GrowTracking.Infrastructure.Commons.Settings
{
    public class ClientInfoSetting
    {
        public List<string> IpHeaderKeys { get; set; } = ["X-Forwarded-For", "X-Real-IP"];
    }
}