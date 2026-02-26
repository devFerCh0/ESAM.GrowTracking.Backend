using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace ESAM.GrowTracking.API.Middlewares
{
    public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public const string HeaderName = "X-Correlation-ID";
        private const int MaxCorrelationIdLength = 128;
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<CorrelationIdMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var correlationId = ResolveCorrelationId(context);
            var identifierFeature = context.Features.Get<IHttpRequestIdentifierFeature>();
            if (identifierFeature is not null)
                identifierFeature.TraceIdentifier = correlationId;
            context.Items[HeaderName] = correlationId;
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                    context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await _next(context);
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !StringValues.IsNullOrEmpty(incoming))
            {
                var value = incoming.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(value) && value.Length <= MaxCorrelationIdLength)
                    return value;
            }
            return context.TraceIdentifier;
        }
    }
}