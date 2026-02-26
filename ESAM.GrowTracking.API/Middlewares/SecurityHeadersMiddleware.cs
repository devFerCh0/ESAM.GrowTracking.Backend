using Microsoft.Net.Http.Headers;

namespace ESAM.GrowTracking.API.Middlewares
{
    public class SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly IHostEnvironment _env = env ?? throw new ArgumentNullException(nameof(env));

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;
            if (!headers.ContainsKey(HeaderNames.XContentTypeOptions))
                headers[HeaderNames.XContentTypeOptions] = "nosniff";
            if (!headers.ContainsKey(HeaderNames.XFrameOptions))
                headers[HeaderNames.XFrameOptions] = "DENY";
            if (!headers.ContainsKey("Referrer-Policy"))
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            if (!headers.ContainsKey("Permissions-Policy"))
                headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), interest-cohort=()";
            if (!headers.ContainsKey("Cross-Origin-Opener-Policy"))
                headers["Cross-Origin-Opener-Policy"] = "same-origin";
            if (!headers.ContainsKey("Cross-Origin-Embedder-Policy"))
                headers["Cross-Origin-Embedder-Policy"] = _env.IsProduction() ? "require-corp" : "credentialless";
            if (!headers.ContainsKey("Cross-Origin-Resource-Policy"))
                headers["Cross-Origin-Resource-Policy"] = "cross-origin";
            if (!headers.ContainsKey("Content-Security-Policy"))
            {
                string csp;
                if (_env.IsDevelopment())
                    csp = "default-src 'none'; connect-src 'self' http: https:; img-src 'self' data: https:; style-src 'self' 'unsafe-inline' https:; font-src 'self' data:; script-src 'self' 'unsafe-inline' https:; " 
                        + "form-action 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'none';";
                else
                    csp = "default-src 'none'; connect-src 'self' https:; img-src 'self' data: https:; style-src 'self' https:; font-src 'self' data:; script-src 'self' https:; form-action 'self'; object-src 'none'; " 
                        + "frame-ancestors 'none'; base-uri 'none'; upgrade-insecure-requests;";
                headers.ContentSecurityPolicy = csp;
            }
            if (_env.IsProduction())
                if (!headers.ContainsKey(HeaderNames.StrictTransportSecurity))
                    headers[HeaderNames.StrictTransportSecurity] = "max-age=15552000; includeSubDomains; preload";
            await _next(context);
        }
    }
}