using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}