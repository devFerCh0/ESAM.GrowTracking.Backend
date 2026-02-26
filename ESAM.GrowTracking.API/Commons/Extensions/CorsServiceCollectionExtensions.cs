using System.Text.RegularExpressions;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace ESAM.GrowTracking.API.Commons.Extensions
{
    public static class CorsServiceCollectionExtensions
    {
        public static IServiceCollection AddRobustCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, ILogger logger)
        {
            var corsSection = configuration.GetSection(CorsSettings.SectionName);
            var corsSettings = corsSection.Exists() ? corsSection.Get<CorsSettings>() ?? new CorsSettings() : new CorsSettings();
            var cookieSection = configuration.GetSection("CookieSettings");
            var cookieSettings = cookieSection.Exists() ? cookieSection.Get<CookieSettings>() ?? new CookieSettings() : new CookieSettings();
            var mergedOrigins = MergeOrigins(corsSettings, cookieSettings);
            var wildcardList = NormalizeWildcards(corsSettings.AllowedOriginWildcards, env.IsProduction() && corsSettings.EnforceStrictOriginsInProduction);
            var regexList = corsSettings.AllowedOriginRegex?.Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToList() ?? [];
            if (env.IsProduction() && corsSettings.EnforceStrictOriginsInProduction)
            {
                ValidateProductionOrigins(mergedOrigins);
                ValidateProductionWildcards(wildcardList);
            }
            var allowedHeaders = corsSettings.AllowedHeaders?.ToArray() ?? [];
            var allowedMethods = corsSettings.AllowedMethods?.ToArray() ?? [];
            var exposeHeaders = corsSettings.ExposeHeaders?.ToArray() ?? [];
            var policyName = corsSettings.PolicyName ?? "CorsPolicy";
            var preflightMaxAge = TimeSpan.FromSeconds(Math.Max(0, corsSettings.PreflightMaxAgeSeconds));
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    ConfigureCorsPolicy(builder, mergedOrigins, wildcardList, regexList, allowedHeaders, allowedMethods, exposeHeaders, corsSettings, env, logger, preflightMaxAge);
                });
            });
            return services;
        }

        private static List<string> MergeOrigins(CorsSettings corsSettings, CookieSettings cookieSettings)
        {
            var merged = new List<string>();
            if (corsSettings.AllowedOrigins?.Count > 0)
                merged.AddRange(corsSettings.AllowedOrigins.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (cookieSettings.AllowedOrigins?.Count > 0)
                merged.AddRange(cookieSettings.AllowedOrigins.Where(x => !string.IsNullOrWhiteSpace(x)));
            return [.. merged.Select(o => o.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)];
        }

        private static List<string> NormalizeWildcards(List<string>? wildcards, bool enforceHttps)
        {
            if (wildcards is null or { Count: 0 })
                return [];
            return [.. wildcards.Select(w => w.Trim()).Where(w => !string.IsNullOrEmpty(w)).Select(w =>
            {
                if (enforceHttps && !w.Contains("://", StringComparison.Ordinal))
                    return $"https://{w}";
                return w;
            })];
        }

        private static void ValidateProductionOrigins(List<string> mergedOrigins)
        {
            if (mergedOrigins.Any(o => o == "*" || o.Equals("null", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("CORS: wildcard origins ('*' / 'null') no están permitidos en producción.");
            var insecure = mergedOrigins.Where(o =>
            {
                if (Uri.TryCreate(o, UriKind.Absolute, out var u))
                    return !string.Equals(u.Scheme, "https", StringComparison.OrdinalIgnoreCase);
                return true;
            }).ToList();
            if (insecure.Count != 0)
                throw new InvalidOperationException(
                    $"CORS: orígenes inseguros (no-https) encontrados para producción: {string.Join(", ", insecure)}. Asegúrese de incluir el esquema 'https://' en AllowedOrigins y CookieSettings.AllowedOrigins.");
        }

        private static void ValidateProductionWildcards(List<string> wildcards)
        {
            var insecure = wildcards.Where(w => !w.StartsWith("https://", StringComparison.OrdinalIgnoreCase)).ToList();
            if (insecure.Count != 0)
                throw new InvalidOperationException(
                    $"CORS: wildcards sin esquema 'https://' encontrados para producción: {string.Join(", ", insecure)}. Añadir el esquema en AllowedOriginWildcards (p.ej. 'https://*.dominio.com').");
        }

        private static void ConfigureCorsPolicy(CorsPolicyBuilder builder, List<string> mergedOrigins, List<string> wildcardList, List<string> regexList, string[] allowedHeaders, string[] allowedMethods, 
            string[] exposeHeaders, CorsSettings corsSettings, IWebHostEnvironment env, ILogger logger, TimeSpan preflightMaxAge)
        {
            builder.SetIsOriginAllowed(origin => IsOriginAllowed(origin, mergedOrigins, wildcardList, regexList, corsSettings, env, logger));
            if (allowedMethods.Length > 0)
                builder.WithMethods(allowedMethods);
            else
                builder.AllowAnyMethod();
            if (allowedHeaders.Length > 0)
                builder.WithHeaders(allowedHeaders);
            else
                builder.AllowAnyHeader();
            if (exposeHeaders.Length > 0)
                builder.WithExposedHeaders(exposeHeaders);
            if (corsSettings.AllowCredentials)
                builder.AllowCredentials();
            else
                builder.DisallowCredentials();
            if (preflightMaxAge > TimeSpan.Zero)
                builder.SetPreflightMaxAge(preflightMaxAge);
        }

        private static bool IsOriginAllowed(string origin, List<string> mergedOrigins, List<string> wildcardList, List<string> regexList, CorsSettings corsSettings, IWebHostEnvironment env, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;
            if (mergedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                return true;
            if (!env.IsProduction() && corsSettings.AllowHttpOnLocalhost)
                if (IsLocalhost(origin, corsSettings.LocalhostPorts))
                    return true;
            foreach (var wildcard in wildcardList)
                if (TryMatchWildcard(origin, wildcard, out _))
                    return true;
            foreach (var rx in regexList)
            {
                try
                {
                    if (Regex.IsMatch(origin, rx, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
                        return true;
                }
                catch (RegexMatchTimeoutException te)
                {
                    logger.LogWarning(te, "CORS: regex '{Regex}' timed out for origin '{Origin}'.", rx, origin);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "CORS: invalid regex '{Regex}' in AllowedOriginRegex.", rx);
                }
            }
            if (env.IsProduction())
                logger.LogWarning("CORS: origen denegado '{Origin}'. No coincide con ninguna regla de AllowedOrigins, wildcards ni regex. Revise CorsSettings si este origen es legítimo.", origin);
            else
                logger.LogDebug("CORS: origen denegado '{Origin}' (entorno no-producción).", origin);
            return false;
        }

        private static bool IsLocalhost(string origin, List<int>? localhostPorts)
        {
            if (Uri.TryCreate(origin, UriKind.Absolute, out var u))
                if ((u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || u.Host.Equals("127.0.0.1")) && localhostPorts?.Contains(u.Port) == true)
                    return true;
            return false;
        }

        private static bool TryMatchWildcard(string origin, string wildcardPattern, out string? matched)
        {
            matched = null;
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(wildcardPattern))
                return false;
            var pattern = wildcardPattern.Trim();
            string? patternScheme = null;
            if (pattern.Contains("://", StringComparison.Ordinal))
            {
                var idx = pattern.IndexOf("://", StringComparison.Ordinal);
                patternScheme = pattern[..idx];
                pattern = pattern[(idx + 3)..];
            }
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                return false;
            if (!string.IsNullOrEmpty(patternScheme) && !string.Equals(patternScheme, originUri.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;
            pattern = pattern.TrimEnd('/');
            var patternHost = pattern;
            int? patternPort = null;
            if (patternHost.Contains(':', StringComparison.Ordinal))
            {
                var parts = patternHost.Split(':', 2);
                patternHost = parts[0];
                if (int.TryParse(parts[1], out var p))
                    patternPort = p;
            }
            var originHost = originUri.Host;
            var escaped = Regex.Escape(patternHost).Replace("\\*", ".*");
            var regex = $"^{escaped}$";
            try
            {
                if (!Regex.IsMatch(originHost, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
                    return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            if (patternPort.HasValue && originUri.Port != patternPort.Value)
                return false;
            matched = origin;
            return true;
        }
    }
}