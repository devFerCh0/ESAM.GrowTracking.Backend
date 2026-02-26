using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ESAM.GrowTracking.API.Middlewares
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment, ProblemDetailsFactory problemDetailsFactory)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        private static readonly Regex _sensitivePattern = new(@"(password|pwd|secret|token|key|bearer|authorization|connectionstring|data\s*source|initial\s*catalog)\s*[=:]\s*\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50));

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request {TraceId} was cancelled by the client.", context.TraceIdentifier);
                return;
            }
            catch (Exception ex)
            {
                if (_environment.IsDevelopment())
                    _logger.LogError(ex, "Unhandled exception processing request {Method} {Path} (TraceId: {TraceId})", context.Request?.Method, context.Request?.Path, context.TraceIdentifier);
                else
                    _logger.LogError("Unhandled exception [{ExceptionType}] processing request {Method} {Path} (TraceId: {TraceId}). Message: {SanitizedMessage}", ex.GetType().FullName, context.Request?.Method,
                        context.Request?.Path, context.TraceIdentifier, SanitizeExceptionMessage(ex.Message));
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response has already started for TraceId {TraceId}; cannot write error response.", context.TraceIdentifier);
                    return;
                }
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                var detail = _environment.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.";
                var pd = _problemDetailsFactory.CreateProblemDetails(context, statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.1", detail: detail);
                if (!pd.Extensions.ContainsKey("traceId"))
                    pd.Extensions["traceId"] = context.TraceIdentifier;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    await context.Response.WriteAsJsonAsync(pd, options);
                }
                catch (Exception writeEx)
                {
                    _logger.LogError(writeEx, "Failed to write ProblemDetails for TraceId {TraceId}", context.TraceIdentifier);
                }
            }
        }

        private static string SanitizeExceptionMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;
            try
            {
                return _sensitivePattern.Replace(message, m => $"{m.Groups[1].Value}=[REDACTED]");
            }
            catch (RegexMatchTimeoutException)
            {
                return "[message sanitization timeout]";
            }
            catch
            {
                return "[message sanitization failed]";
            }
        }
    }
}