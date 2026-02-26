namespace ESAM.GrowTracking.Application.Commons.Settings
{
    public sealed class TimeSecuritySetting
    {
        public int TemporaryLifetimeMinutes { get; init; } = 5;

        public int LifetimeMinutes { get; init; } = 15;

        public int LifetimeDays { get; init; } = 7;

        public int AbsoluteLifetimeDays { get; init; } = 30;

        public int IdleWindowDays { get; init; } = 3;
    }
}