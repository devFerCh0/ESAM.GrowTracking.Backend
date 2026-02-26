using System.Security.Cryptography;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class AuthCookieService : IAuthCookieService
    {
        private readonly ILogger<AuthCookieService> _logger;
        private readonly CookieSettings _settings;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;
        private const string ProtectorPurpose = "ESAM.GrowTracking.Security.RefreshTokenProtector.v1";

        public AuthCookieService(ILogger<AuthCookieService> logger, IOptions<CookieSettings> options, IWebHostEnvironment env, IDataProtectionProvider dataProtectionProvider)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(options, $"{nameof(options)} no puede ser nulo.");
            Guard.AgainstNull(options.Value, $"{nameof(options.Value)} no puede ser nulo.");
            Guard.AgainstNull(env, $"{nameof(env)} no puede ser nulo.");
            Guard.AgainstNull(dataProtectionProvider,  $"{nameof(dataProtectionProvider)} no puede ser nulo.");
            _logger = logger;
            _settings = options.Value;
            _env = env;
            _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            _settings.Validate();
            ValidateSameSiteSecureCoherence();
        }

        public string EffectiveRefreshCookieName() => _settings.EffectiveRefreshCookieName();

        public void SetRefreshTokenCookie(HttpResponse response, string refreshToken, DateTimeOffset expiresAt)
        {
            Guard.AgainstNull(response, $"{nameof(response)} no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogDebug("Attempt to set empty refresh token cookie was ignored.");
                return;
            }
            try
            {
                var protectedToken = _protector.Protect(refreshToken);
                var opts = BuildCookieOptions(expiresAt, isRefreshToken: true);
                response.Cookies.Append(_settings.EffectiveRefreshCookieName(), protectedToken, opts);
            }
            catch (CryptographicException ce)
            {
                _logger.LogError(ce, "Failed protecting refresh token for cookie.");
                throw;
            }
        }

        public void DeleteRefreshTokenCookie(HttpResponse response)
        {
            Guard.AgainstNull(response, $"{nameof(response)} no puede ser nulo.");
            var opts = BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), isRefreshToken: true);
            try
            {
                response.Cookies.Delete(_settings.EffectiveRefreshCookieName(), opts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete refresh token cookie (best-effort).");
            }
        }

        public bool TryGetRefreshTokenFromRequest(HttpRequest request, out string? refreshToken)
        {
            refreshToken = null;
            if (request == null)
                return false;
            if (request.Cookies.TryGetValue(_settings.EffectiveRefreshCookieName(), out var cookieVal) && !string.IsNullOrWhiteSpace(cookieVal))
            {
                try
                {
                    refreshToken = _protector.Unprotect(cookieVal);
                    return !string.IsNullOrWhiteSpace(refreshToken);
                }
                catch (CryptographicException ce)
                {
                    _logger.LogWarning(ce, "Failed to unprotect refresh token cookie. Token will be ignored.");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error while unprotecting refresh token cookie. Token will be ignored.");
                    return false;
                }
            }
            if (_settings.AllowRefreshTokenHeader && request.Headers.TryGetValue("X-Refresh-Token", out var headerVal) && !string.IsNullOrWhiteSpace(headerVal.ToString()))
            {
                refreshToken = headerVal.ToString().Trim();
                return !string.IsNullOrWhiteSpace(refreshToken);
            }
            return false;
        }

        public void SetXsrfCookie(HttpResponse response, string xsrfToken, int? expiresInMinutes = null)
        {
            Guard.AgainstNull(response, $"{nameof(response)} no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(xsrfToken))
            {
                _logger.LogDebug("Attempt to set empty xsrf cookie was ignored.");
                return;
            }
            var expiry = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes ?? _settings.XsrfCookieExpiresMinutes);
            var opts = BuildCookieOptions(expiry, isRefreshToken: false);
            opts.HttpOnly = false;
            try
            {
                response.Cookies.Append(_settings.EffectiveXsrfCookieName(), xsrfToken, opts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set XSRF cookie (best-effort).");
            }
        }

        public void DeleteXsrfCookie(HttpResponse response)
        {
            Guard.AgainstNull(response, $"{nameof(response)} no puede ser nulo.");
            var opts = BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(-1), isRefreshToken: false);
            opts.HttpOnly = false;
            try
            {
                response.Cookies.Delete(_settings.EffectiveXsrfCookieName(), opts);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete XSRF cookie (best-effort).");
            }
        }

        private bool ShouldUseSecureFlag() => _env.IsProduction() || _settings.AlwaysSecure;

        private CookieOptions BuildCookieOptions(DateTimeOffset expiresAt, bool isRefreshToken)
        {
            var secure = ShouldUseSecureFlag();
            if (_settings.SameSite == SameSiteMode.None && !secure)
            {
                _logger.LogWarning(
                    "SameSite=None detectado con Secure=false. Forzando Secure=true en la cookie para conformidad con RFC 6265bis. En desarrollo se recomienda usar SameSite=Lax o establecer AlwaysSecure=true.");
                secure = true;
            }
            var cookieOptions = new CookieOptions { HttpOnly = isRefreshToken, Secure = secure, SameSite = _settings.SameSite, Expires = expiresAt, Path = string.IsNullOrWhiteSpace(_settings.Path) ? "/" : _settings.Path, 
                IsEssential = true };
            if (_settings.UseHostPrefix)
            {
                cookieOptions.Path = "/";
                cookieOptions.Secure = true;
                cookieOptions.Domain = null;
            }
            else if (!string.IsNullOrWhiteSpace(_settings.Domain))
                cookieOptions.Domain = _settings.Domain;
            return cookieOptions;
        }

        private void ValidateSameSiteSecureCoherence()
        {
            if (_settings.SameSite != SameSiteMode.None)
                return;
            if (!_env.IsDevelopment() && !_settings.AlwaysSecure && !_settings.UseHostPrefix)
                throw new InvalidOperationException(
                    "CookieSettings: SameSite=None requiere Secure=true en producción. Establezca AlwaysSecure=true o UseHostPrefix=true en la configuración. Los navegadores rechazarán cookies SameSite=None sin el atributo Secure.");
        }
    }
}