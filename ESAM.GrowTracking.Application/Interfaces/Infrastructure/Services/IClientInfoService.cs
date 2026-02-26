namespace ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services
{
    public interface IClientInfoService
    {
        string? GetIpAddress();

        string? GetUserAgent();
    }
}