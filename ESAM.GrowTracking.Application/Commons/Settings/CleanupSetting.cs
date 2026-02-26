namespace ESAM.GrowTracking.Application.Commons.Settings
{
    public class CleanupSetting
    {
        public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

        public int BatchSize { get; set; } = 1000;

        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMinutes(1);
    }
}