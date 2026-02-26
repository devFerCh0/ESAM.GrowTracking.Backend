using ESAM.GrowTracking.API.Commons.Exceptions;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.API.Middlewares
{
    public class ValidateXsrfHeaderMiddleware
    {
        private readonly ILogger<ValidateXsrfHeaderMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly CookieSettings _cookieSettings;

        public ValidateXsrfHeaderMiddleware(ILogger<ValidateXsrfHeaderMiddleware> logger, RequestDelegate next, IOptions<CookieSettings> cookieOptions)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(next, $"{nameof(next)} no puede ser nulo.");
            Guard.AgainstNull(cookieOptions, $"{nameof(cookieOptions)} no puede ser nulo.");
            Guard.AgainstNull(cookieOptions.Value, $"{nameof(cookieOptions.Value)} no puede ser nulo.");
            _logger = logger;
            _next = next;
            _cookieSettings = cookieOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            Guard.AgainstNull(context, $"{nameof(context)} no puede ser nulo.");
            if (IsStateChangingMethod(context.Request.Method))
            {
                var refreshCookieName = _cookieSettings.EffectiveRefreshCookieName();
                var xsrfCookieName = _cookieSettings.EffectiveXsrfCookieName();
                if (context.Request.Cookies.TryGetValue(refreshCookieName, out _))
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var method = context.Request.Method;
                    var path = context.Request.Path.Value ?? string.Empty;
                    var traceId = context.TraceIdentifier;
                    if (!context.Request.Headers.TryGetValue("X-XSRF-TOKEN", out var headerVal) || string.IsNullOrWhiteSpace(headerVal))
                    {
                        _logger.LogWarning("XSRF validation failed — missing X-XSRF-TOKEN header. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", method, path, clientIp, traceId);
                        await Deny(context);
                        return;
                    }
                    if (!context.Request.Cookies.TryGetValue(xsrfCookieName, out var xsrfCookieValue) || string.IsNullOrWhiteSpace(xsrfCookieValue))
                    {
                        _logger.LogWarning("XSRF validation failed — missing XSRF cookie '{CookieName}'. Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", xsrfCookieName, method, path, clientIp, traceId);
                        await Deny(context);
                        return;
                    }
                    var headerBytes = Encoding.UTF8.GetBytes(headerVal.ToString());
                    var cookieBytes = Encoding.UTF8.GetBytes(xsrfCookieValue);
                    if (headerBytes.Length != cookieBytes.Length || !CryptographicOperations.FixedTimeEquals(headerBytes, cookieBytes))
                    {
                        _logger.LogWarning("XSRF validation failed — token mismatch (possible CSRF attempt). Method={Method} Path={Path} ClientIp={ClientIp} TraceId={TraceId}", method, path, clientIp, traceId);
                        await Deny(context);
                        return;
                    }
                }
            }
            await _next(context);
        }

        private static bool IsStateChangingMethod(string method) => HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method);

        private static async Task Deny(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            const string payload = "{\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.3\",\"title\":\"Forbidden\",\"status\":403,\"detail\":\"Invalid or missing XSRF token.\"}";
            await context.Response.WriteAsync(payload);
        }
    }
}