namespace ESAM.GrowTracking.Application.Commons.Settings
{
    public class LoginSecuritySetting
    {
        public int MaxFailedAttempts { get; set; } = 5;

        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
    }
}