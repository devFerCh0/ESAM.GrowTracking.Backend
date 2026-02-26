using Microsoft.AspNetCore.Http;

namespace ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services
{
    public interface IAuthCookieService
    {
        void SetRefreshTokenCookie(HttpResponse response, string refreshToken, DateTimeOffset expiresAt);

        void DeleteRefreshTokenCookie(HttpResponse response);

        bool TryGetRefreshTokenFromRequest(HttpRequest request, out string? refreshToken);

        void SetXsrfCookie(HttpResponse response, string xsrfToken, int? expiresInMinutes = null);

        void DeleteXsrfCookie(HttpResponse response);

        string EffectiveRefreshCookieName();
    }
}