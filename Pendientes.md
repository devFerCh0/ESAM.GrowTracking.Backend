* httpOnly

como se actualiza e refresh
que pasa si en el refresh hay anomalia, se cierra la sesión??

como hace la verificación de expiración (permanente o volatil), DONDE SE ALOJA EL ACCESS TOKEN,

****************************************************************************************************

Eres un experto desarrollador senior en Back-End con especialización en ASP.NET Core Web API (NET Core 8+), un analista de sistemas enfocado en seguridad, autenticación y autorización avanzada, y un arquitecto de software con dominio en Clean Architecture y Principios SOLID.
Además, eres especialista en modelado de bases de datos SQL Server, y experto en de diseño, asegurando una implementación robusta y escalable.

Actúa en base a lo mencionado anteriormente y quiero que me des concejos en base a las buenas prácticas de cómo debería manejar ciertos aspectos en mi Proyecto ASP.NET Core Web API, con Clean Architecture (Solo con capas Domain, Application, Infrastructure y Presentation), donde manejo de los Principios SOLID, y donde implemente los Patrones de Diseño Repository, UnitOfWork, MediatR (Sin Pipelines, ni Behabiors.), CQRS, Result, Dependecy Injection. 
Los concejos que quiero que me proporciones es a temas concretos y específicos de los cuales yo te hablare. ¿Te parece?, ¿me dices si podemos empezar?

****************************************************************************************************

Te proporcionare:
- Mi servicio de manejo de cookies.
- Mis appsettings tanto de producción y desarrollo.
- Mis extensiones de servicio y un middleware.
- Mi Controlador Auth con métodos EndPoint que implementan el uso del servicio.
- Mi clase Program.

En base a un análisis a detalle, exhaustivo, cuidadoso y en todos los sentidos a todo lo que te proporcionare.
Quiero que me des:
1. Implementaciones finales faltantes y/o ajustes finales faltantes.
2. Inyecciones finales faltantes y/o ajustes finales o faltantes.
3. Configuraciones finales y/o ajustes finales o faltantes.
Con el objetivo de que está mi API ya entre en producción en su primer SPRINT bajo la metodología SCRUM.

Importante:
- No menciones ni implementes nada relacionado Caché, ni nada relacionado.
- Consumirán de está API, aplicaciones WEB y Aplicaciones Móviles. Tanto las aplicaciones WEB como las Móviles podrán ya estar en Producción, en Pruebas o Desarrollo. De igual manera se harán pruebas desde PostMAN y Swagger.
- Está API estará en Producción para su primer SPRINT, pero también estará en pruebas y en desarrollo para continuar con los demás SPRINTs.
- Todo lo relacionado a los Handlers que son llamados en los EndPoints ya lo tengo desarrollado, implementado y funcional y con sus respectivas inyecciones realizadas en la clase Program que te proporcionare.

Datos generales a considerar de desarrollo de la API (Como se está desarrollando y como se seguirá desarrollando).
- ASP.Net Core Web API 8+
- Principios SOLID y Arquitectura Limpia (Capas Domain, Application, Infraestructure, Persistence y API)
- Patrones de Diseño Repository, UnitOfWork, CQRS, MediatR (Sin Pipelines, sin Behabiors), DI, Result.
- Estructura Monolítica Modular (Sin microservicios).

Como respuesta solo quiero las implementaciones completas, a detalle, sin que omitas, ni olvides nada y de manera bastante clara de:
1. Implementaciones finales faltantes y/o ajustes finales faltantes.
2. Inyecciones finales faltantes y/o ajustes finales o faltantes.
3. Configuraciones finales y/o ajustes finales o faltantes.
Solo te pido un buen análisis tomándote tu tiempo y esperando que hayas comprendido lo que necesito para obtener una buena respuesta, de esto depende si continuo o no en mi trabajo.
Y no olvides que Todo lo relacionado a los Handlers que son llamados en los EndPoints ya lo tengo desarrollado, implementado y funcional y con sus respectivas inyecciones, así que no necesitare que las menciones, ni las toques, ni las implementes para nada.

En el siguiente PROMPT te proporciono lo mencionado, mientras te lo proporciono no hagas nada.

****************************************************************************************************

/***** AuthCookieService.cs *****/
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
            Guard.AgainstNull(dataProtectionProvider, $"{nameof(dataProtectionProvider)} no puede ser nulo.");
            _logger = logger;
            _settings = options.Value;
            _env = env;
            _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            _settings.Validate();
        }

        public string EffectiveRefreshCookieName() => _settings.EffectiveRefreshCookieName();

        private bool ShouldUseSecureFlag()
        {
            return _env.IsProduction() || _settings.AlwaysSecure;
        }

        private CookieOptions BuildCookieOptions(DateTimeOffset expiresAt, bool isRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = isRefreshToken,
                Secure = ShouldUseSecureFlag(),
                SameSite = _settings.SameSite,
                Expires = expiresAt,
                Path = string.IsNullOrWhiteSpace(_settings.Path) ? "/" : _settings.Path,
                IsEssential = true
            };
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
    }
}

/****** appssetings.developmen.json *****/
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=.\\FASCILS;Initial Catalog=ESAM.GrowTracking.DB;Integrated Security=True; TrustServerCertificate=True;"
    },
    "LoginSecuritySetting": {
        "MaxFailedAttempts": 5,
        "LockoutDuration": "00:15:00",
        "Duration": "01:00:00"
    },
    "TimeSecuritySetting": {
        "TemporaryLifetimeMinutes": 5,
        "LifetimeMinutes": 15,
        "LifetimeDays": 7,
        "AbsoluteLifetimeDays": 30,
        "IdleWindowDays": 3
    },
    "JwtSettings": {
        "Issuer": "https://mi-api.company.com",
        "Audience": "https://mi-app.company.com",
        "SecretKey": "rM9fk1NusvNkF6i4Bna2NEhEgS7ZmS0c"
    },
    "CorsSettings": {
        "PolicyName": "CorsPolicy",
        "AllowedOrigins": [
            "https://app.example.com", // orígenes reales si tu front está desplegado en dev remoto
            "http://localhost:3000",
            "http://localhost:4200"
        ],
        "AllowedOriginWildcards": [
            "*.localhost"
        ],
        "AllowedHeaders": [ "Content-Type", "Authorization", "X-Requested-With", "X-XSRF-TOKEN", "X-Refresh-Token" ],
        "AllowedMethods": [ "GET", "HEAD", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" ],
        "ExposeHeaders": [ "X-XSRF-TOKEN" ],
        "AllowCredentials": true,
        "PreflightMaxAgeSeconds": 600,
        "AllowHttpOnLocalhost": true,
        "LocalhostPorts": [ 3000, 4200, 8080 ],
        "EnforceStrictOriginsInProduction": false
    },
    "CookieSettings": {
        "CookieName": "rt",
        "UseHostPrefix": true,
        "AllowRefreshTokenHeader": false,
        "AlwaysSecure": true,
        "XsrfCookieName": "XSRF-T",
        "XsrfCookieExpiresMinutes": 30,
        "SameSite": "None",
        "Path": "/",
        "AllowedOrigins": [ "http://localhost:4200" ]
    }
}

/***** appsettings.json *****/
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=.\\FASCILS;Initial Catalog=ESAM.GrowTracking.DB;Integrated Security=True; TrustServerCertificate=True;"
    },
    "LoginSecuritySetting": {
        "MaxFailedAttempts": 5,
        "LockoutDuration": "00:15:00",
        "Duration": "01:00:00"
    },
    "TimeSecuritySetting": {
        "TemporaryLifetimeMinutes": 5,
        "LifetimeMinutes": 15,
        "LifetimeDays": 7,
        "AbsoluteLifetimeDays": 30,
        "IdleWindowDays": 3
    },
    "JwtSettings": {
        "Issuer": "https://mi-api.company.com",
        "Audience": "https://mi-app.company.com",
        "SecretKey": "rM9fk1NusvNkF6i4Bna2NEhEgS7ZmS0c"
    },
    "CorsSettings": {
        "PolicyName": "CorsPolicy",
        "AllowedOrigins": [
            "https://app.example.com",
            "https://admin.example.com"
        ],
        "AllowedOriginWildcards": [
            "https://*.cdn.example.com",
            "*.static.example.com"
        ],
        "AllowedOriginRegex": [
            "^https:\\/\\/([a-z0-9-]+\\.)?example\\.com(:[0-9]{1,5})?$"
        ],
        "AllowedHeaders": [ "Content-Type", "Authorization", "X-Requested-With", "X-XSRF-TOKEN", "X-Refresh-Token" ],
        "AllowedMethods": [ "GET", "HEAD", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" ],
        "ExposeHeaders": [ "X-XSRF-TOKEN" ],
        "AllowCredentials": true,
        "PreflightMaxAgeSeconds": 600,
        "AllowHttpOnLocalhost": false,
        "LocalhostPorts": [ 3000, 4200, 8080 ],
        "EnforceStrictOriginsInProduction": true
    },
    "CookieSettings": {
        "CookieName": "rt",
        "UseHostPrefix": true,
        "AllowRefreshTokenHeader": false,
        "AlwaysSecure": true,
        "XsrfCookieName": "XSRF-T",
        "XsrfCookieExpiresMinutes": 30,
        "SameSite": "None",
        "Path": "/",
        "AllowedOrigins": [ "https://app.example.com" ]
    }
}

/***** CookieServiceCollectionExtensions.cs *****/
namespace ESAM.GrowTracking.API.Commons.Extensions
{
    public static class CookieServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthCookieServices(this IServiceCollection services, IConfigurationSection cookieSettingsSection, IWebHostEnvironment env, ILogger logger)
        {
            services.Configure<CookieSettings>(cookieSettingsSection);
            var dpBuilder = services.AddDataProtection().SetApplicationName("ESAM.GrowTracking");
            try
            {
                if (!env.IsDevelopment())
                {
                    var keysFolder = Path.Combine(env.ContentRootPath, "DataProtection-Keys");
                    Directory.CreateDirectory(keysFolder);
                    dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
                }
            }
            catch
            {
                logger.LogWarning("No se iniciaron las claves locales.");
            }
            services.AddScoped<IAuthCookieService, AuthCookieService>();
            services.AddTransient<ValidateXsrfHeaderMiddleware>();
            return services;
        }
    }
}

/***** CorsServiceCollectionExtensions.cs *****/
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
            var mergedOrigins = new List<string>();
            if (corsSettings.AllowedOrigins?.Count > 0)
                mergedOrigins.AddRange(corsSettings.AllowedOrigins.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (cookieSettings.AllowedOrigins?.Count > 0)
                mergedOrigins.AddRange(cookieSettings.AllowedOrigins.Where(x => !string.IsNullOrWhiteSpace(x)));
            mergedOrigins = [.. mergedOrigins.Select(o => o.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)];
            if (env.IsProduction() && corsSettings.EnforceStrictOriginsInProduction)
            {
                if (mergedOrigins.Any(o => o == "*" || o.Equals("null", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("CORS: wildcard origins are not allowed in production.");
                var insecureOrigins = mergedOrigins.Where(o =>
                {
                    if (Uri.TryCreate(o, UriKind.Absolute, out var u))
                        return !string.Equals(u.Scheme, "https", StringComparison.OrdinalIgnoreCase);
                    return true;
                }).ToList();
                if (insecureOrigins.Count != 0)
                    throw new InvalidOperationException($"CORS: Found insecure origins for production: {string.Join(", ", insecureOrigins)}");
            }
            var wildcardList = corsSettings.AllowedOriginWildcards?.Select(w => w.Trim()).Where(w => !string.IsNullOrEmpty(w)).ToList() ?? [];
            var regexList = corsSettings.AllowedOriginRegex?.Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToList() ?? [];
            var allowedHeaders = corsSettings.AllowedHeaders?.ToArray() ?? [];
            var allowedMethods = corsSettings.AllowedMethods?.ToArray() ?? [];
            var exposeHeaders = corsSettings.ExposeHeaders?.ToArray() ?? [];
            var policyName = corsSettings.PolicyName ?? "CorsPolicy";
            var preflightMaxAge = TimeSpan.FromSeconds(Math.Max(0, corsSettings.PreflightMaxAgeSeconds));
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin))
                            return false;
                        if (mergedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                            return true;
                        if (!env.IsProduction() && corsSettings.AllowHttpOnLocalhost)
                            if (Uri.TryCreate(origin, UriKind.Absolute, out var u))
                                if ((u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || u.Host.Equals("127.0.0.1")) && (corsSettings.LocalhostPorts?.Contains(u.Port) == true))
                                    return true;
                        foreach (var wildcard in wildcardList)
                            if (TryMatchWildcard(origin, wildcard, out var _))
                                return true;
                        foreach (var rx in regexList)
                        {
                            try
                            {
                                if (Regex.IsMatch(origin, rx, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                    return true;
                            }
                            catch (ArgumentException ex)
                            {
                                logger.LogWarning(ex, "CORS: invalid regex '{Regex}' in AllowedOriginRegex.", rx);
                            }
                        }
                        return false;
                    });
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
                });
            });
            return services;
        }

        private static bool TryMatchWildcard(string origin, string wildcardPattern, out string? matched)
        {
            matched = null;
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(wildcardPattern))
                return false;
            var pattern = wildcardPattern.Trim();
            string? patternScheme = null;
            if (pattern.Contains("://"))
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
            if (patternHost.Contains(':'))
            {
                var parts = patternHost.Split(':', 2);
                patternHost = parts[0];
                if (int.TryParse(parts[1], out var p))
                    patternPort = p;
            }
            var originHost = originUri.Host;
            var escaped = Regex.Escape(patternHost).Replace("\\*", ".*");
            var regex = $"^{escaped}$";
            if (!Regex.IsMatch(originHost, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return false;
            if (patternPort.HasValue && originUri.Port != patternPort.Value)
                return false;
            matched = origin;
            return true;
        }
    }
}

/***** ValidateXsrfHeaderMiddleware.cs *****/
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
                    if (!context.Request.Headers.TryGetValue("X-XSRF-TOKEN", out var headerVal) || string.IsNullOrWhiteSpace(headerVal))
                    {
                        _logger.LogInformation("Missing X-XSRF-TOKEN header.");
                        await Deny(context);
                        return;
                    }
                    if (!context.Request.Cookies.TryGetValue(xsrfCookieName, out var xsrfCookieValue) || string.IsNullOrWhiteSpace(xsrfCookieValue))
                    {
                        _logger.LogInformation("Missing XSRF cookie.");
                        await Deny(context);
                        return;
                    }
                    var headerBytes = Encoding.UTF8.GetBytes(headerVal.ToString());
                    var cookieBytes = Encoding.UTF8.GetBytes(xsrfCookieValue);
                    if (headerBytes.Length != cookieBytes.Length || !CryptographicOperations.FixedTimeEquals(headerBytes, cookieBytes))
                    {
                        _logger.LogWarning("XSRF token mismatch.");
                        await Deny(context);
                        return;
                    }
                }
            }
            await _next(context);
        }

        private static bool IsStateChangingMethod(string method)
        {
            return HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method);
        }

        private static async Task Deny(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            var payload = "{\"type\":\"https://tools.ietf.org/html/rfc7231#section-6.5.3\",\"title\":\"Forbidden\",\"status\":403,\"detail\":\"Invalid or missing XSRF token.\"}";
            await context.Response.WriteAsync(payload);
        }
    }
}

/***** AuthController.cs *****/
namespace ESAM.GrowTracking.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;
        private readonly IErrorToHttpMapper _errorToHttpMapper;
        private readonly IMapper _mapper;
        private readonly IAuthCookieService _authCookieService;

        public AuthController(ILogger<AuthController> logger, IMediator mediator, IErrorToHttpMapper errorToHttpMapper, IMapper mapper, IAuthCookieService authCookieService)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puedo ser nulo.");
            Guard.AgainstNull(mediator, $"{nameof(mediator)} no puedo ser nulo.");
            Guard.AgainstNull(errorToHttpMapper, $"{nameof(errorToHttpMapper)} no puedo ser nulo.");
            Guard.AgainstNull(mapper, $"{nameof(mapper)} no puede ser nulo.");
            Guard.AgainstNull(authCookieService, $"{nameof(authCookieService)} no puede ser nulo.");
            _logger = logger;
            _mediator = mediator;
            _errorToHttpMapper = errorToHttpMapper;
            _mapper = mapper;
            _authCookieService = authCookieService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginCommand>(request);
            var loginResult = await _mediator.Send(command, cancellationToken);
            if (!loginResult.IsSuccess)
                return loginResult.ToErrorActionResult(_errorToHttpMapper);
            var login = _mapper.Map<LoginResponse>(loginResult.Value);
            return Ok(new { success = true, data = login });
        }

        [AllowAnonymous]
        [HttpGet("user-workProfiles/workProfile/{workProfileId:int}/login-user-role-campuses")]
        [ProducesResponseType(typeof(List<LoginUserRoleCampusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<LoginUserRoleCampusResponse>>> LoginUserRoleCampusesAsync([FromRoute] int workProfileId, CancellationToken cancellationToken)
        {
            var query = new LoginUserRoleCampusQuery(workProfileId);
            var userRoleCampusesResult = await _mediator.Send(query, cancellationToken);
            if (!userRoleCampusesResult.IsSuccess)
                return userRoleCampusesResult.ToErrorActionResult(_errorToHttpMapper);
            var userRoleCampuses = _mapper.Map<List<LoginUserRoleCampusResponse>>(userRoleCampusesResult.Value);
            return Ok(new { success = true, data = userRoleCampuses });
        }

        [AllowAnonymous]
        [HttpPost("login-assumed-role")]
        [ProducesResponseType(typeof(LoginAssumedRoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginAssumedRoleResponse>> LoginAssumedRoleAsync([FromBody] LoginAssumedRoleRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginAssumedRoleCommand>(request);
            var loginAssumedRoleResult = await _mediator.Send(command, cancellationToken);
            if (!loginAssumedRoleResult.IsSuccess)
                return loginAssumedRoleResult.ToErrorActionResult(_errorToHttpMapper);
            var loginAssumedRole = loginAssumedRoleResult.Value;
            bool hadIncomingCookieToken = _authCookieService.TryGetRefreshTokenFromRequest(Request, out _);
            bool treatAsBrowser = IsLikelyBrowserRequest(Request) || hadIncomingCookieToken;
            if (treatAsBrowser)
            {
                if (!string.IsNullOrWhiteSpace(loginAssumedRole.RefreshTokenRaw))
                {
                    try
                    {
                        var expiresAt = new DateTimeOffset(loginAssumedRole.RefreshTokenExpiresAt.ToUniversalTime());
                        _authCookieService.SetRefreshTokenCookie(Response, loginAssumedRole.RefreshTokenRaw, expiresAt);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to set refresh token cookie during login-assumed-role.");
                    }
                }
                try
                {
                    var xsrfToken = GenerateXsrfToken();
                    _authCookieService.SetXsrfCookie(Response, xsrfToken);
                    if (!Response.Headers.ContainsKey("X-XSRF-TOKEN"))
                        Response.Headers["X-XSRF-TOKEN"] = xsrfToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set xsrf cookie/header during login-assumed-role.");
                }
            }
            return Ok(new { success = true, data = _mapper.Map<LoginAssumedRoleResponse>(loginAssumedRole, opt => opt.Items["IsBrowser"] = treatAsBrowser) });
        }

        [AllowAnonymous]
        [HttpPost("login-assumed-workProfile")]
        [ProducesResponseType(typeof(LoginAssumedWorkProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginAssumedWorkProfileResponse>> LoginAssumedWorkProfileAsync([FromBody] LoginAssumedWorkProfileRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginAssumedWorkProfileCommand>(request);
            var loginAssumedWorkProfileResult = await _mediator.Send(command, cancellationToken);
            if (!loginAssumedWorkProfileResult.IsSuccess)
                return loginAssumedWorkProfileResult.ToErrorActionResult(_errorToHttpMapper);
            var loginAssumedWorkProfile = loginAssumedWorkProfileResult.Value;
            bool hadIncomingCookieToken = _authCookieService.TryGetRefreshTokenFromRequest(Request, out _);
            bool treatAsBrowser = IsLikelyBrowserRequest(Request) || hadIncomingCookieToken;
            if (treatAsBrowser)
            {
                if (!string.IsNullOrWhiteSpace(loginAssumedWorkProfile.RefreshTokenRaw))
                {
                    try
                    {
                        var expiresAt = new DateTimeOffset(loginAssumedWorkProfile.RefreshTokenExpiresAt.ToUniversalTime());
                        _authCookieService.SetRefreshTokenCookie(Response, loginAssumedWorkProfile.RefreshTokenRaw, expiresAt);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to set refresh token cookie during login-assumed-workProfile.");
                    }
                }
                try
                {
                    var xsrfToken = GenerateXsrfToken();
                    _authCookieService.SetXsrfCookie(Response, xsrfToken);
                    if (!Response.Headers.ContainsKey("X-XSRF-TOKEN"))
                        Response.Headers["X-XSRF-TOKEN"] = xsrfToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set xsrf cookie/header during login-assumed-workProfile.");
                }
            }
            return Ok(new { success = true, data = _mapper.Map<LoginAssumedWorkProfileResponse>(loginAssumedWorkProfile, opt => opt.Items["IsBrowser"] = treatAsBrowser) });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LogoutCommand>(request);
            bool hadIncomingCookieToken = false;
            if (string.IsNullOrWhiteSpace(command.RefreshTokenRaw))
                if (_authCookieService.TryGetRefreshTokenFromRequest(Request, out var cookieToken))
                {
                    command = command with { RefreshTokenRaw = cookieToken };
                    hadIncomingCookieToken = true;
                }
            var logoutResult = await _mediator.Send(command, cancellationToken);
            if (!logoutResult.IsSuccess)
                return logoutResult.ToErrorActionResult(_errorToHttpMapper);
            if (hadIncomingCookieToken)
            {
                try
                {
                    _authCookieService.DeleteRefreshTokenCookie(Response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete refresh token cookie during logout.");
                }
                try
                {
                    _authCookieService.DeleteXsrfCookie(Response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete xsrf cookie during logout.");
                }
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<RefreshCommand>(request);
            bool hadIncomingCookieToken = false;
            if (string.IsNullOrWhiteSpace(command.RefreshTokenRaw))
                if (_authCookieService.TryGetRefreshTokenFromRequest(Request, out var cookieToken))
                {
                    command = command with { RefreshTokenRaw = cookieToken };
                    hadIncomingCookieToken = true;
                }
            var refreshResult = await _mediator.Send(command, cancellationToken);
            if (!refreshResult.IsSuccess)
                return refreshResult.ToErrorActionResult(_errorToHttpMapper);
            var refresh = refreshResult.Value;
            bool treatAsBrowser = IsLikelyBrowserRequest(Request) || hadIncomingCookieToken;
            if (treatAsBrowser)
            {
                if (!string.IsNullOrWhiteSpace(refresh.RefreshTokenRaw))
                {
                    try
                    {
                        var expiresAt = new DateTimeOffset(refresh.RefreshTokenExpiresAt.ToUniversalTime());
                        _authCookieService.SetRefreshTokenCookie(Response, refresh.RefreshTokenRaw, expiresAt);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to set refresh token cookie during refresh.");
                    }
                }
                try
                {
                    var xsrfToken = GenerateXsrfToken();
                    _authCookieService.SetXsrfCookie(Response, xsrfToken);
                    if (!Response.Headers.ContainsKey("X-XSRF-TOKEN"))
                        Response.Headers["X-XSRF-TOKEN"] = xsrfToken;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set xsrf cookie/header during refresh.");
                }
            }
            return Ok(new { success = true, data = _mapper.Map<RefreshResponse>(refresh, opt => opt.Items["IsBrowser"] = treatAsBrowser) });;
        }

        private static bool IsLikelyBrowserRequest(HttpRequest request)
        {
            if (request == null)
                return false;
            if (request.Headers.ContainsKey("Origin"))
                return true;
            if (request.Headers.ContainsKey("Referer"))
                return true;
            if (request.Headers.TryGetValue("User-Agent", out var ua))
            {
                var uaStr = ua.ToString();
                if (uaStr.Contains("Mozilla", StringComparison.OrdinalIgnoreCase) || uaStr.Contains("Chrome", StringComparison.OrdinalIgnoreCase) || uaStr.Contains("Safari", StringComparison.OrdinalIgnoreCase))
                    if (!uaStr.Contains("Postman", StringComparison.OrdinalIgnoreCase) && !uaStr.Contains("curl", StringComparison.OrdinalIgnoreCase))
                        return true;
            }
            return false;
        }

        private static string GenerateXsrfToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            base64 = base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return base64;
        }
    }
}

/***** Program.cs *****/
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<JwtSetting>().Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(js => !string.IsNullOrWhiteSpace(js.Issuer) && !string.IsNullOrWhiteSpace(js.Audience) && !string.IsNullOrWhiteSpace(js.SecretKey) && js.SecretKey.Length >= 32,
    "La configuración de jwt no es válida.");
builder.Services.AddOptions<TimeSecuritySetting>().Bind(builder.Configuration.GetSection("TimeSecuritySetting"))
    .Validate(tss => tss.TemporaryLifetimeMinutes > 0 && tss.LifetimeMinutes > 0 && tss.LifetimeDays > 0 && tss.AbsoluteLifetimeDays > 0 && tss.IdleWindowDays > 0, "La configuración de seguridad de tiempos no es válida.");
builder.Services.AddOptions<ClientInfoSetting>().Bind(builder.Configuration.GetSection("ClientInfo"))
    .Validate(cis => cis == null || cis.IpHeaderKeys == null || cis.IpHeaderKeys.Count > 0, "ClientInfo: IpHeaderKeys no debe estar vacío si está configurado.");
builder.Services.AddOptions<LoginSecuritySetting>().Bind(builder.Configuration.GetSection("LoginSecuritySetting"))
    .Validate(lss => lss.MaxFailedAttempts > 0 && lss.LockoutDuration.Minutes > 0 && lss.Duration.Hours > 0, "La configuración de securidad de login no es válida.");
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection("CookieSettings"));
builder.Services.AddDbContext<AppDbContext>(dcob => dcob.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthCookieServices(builder.Configuration.GetSection("CookieSettings"), builder.Environment, NullLogger.Instance);
builder.Services.AddScoped<IIpAddressValidator, IpAddressValidator>();
builder.Services.AddScoped<IClientInfoService, ClientInfoService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IBlacklistedAccessTokenPermanentRepository, BlacklistedAccessTokenPermanentRepository>();
builder.Services.AddScoped<IBlacklistedAccessTokenTemporaryRepository, BlacklistedAccessTokenTemporaryRepository>();
builder.Services.AddScoped<IBlacklistedRefreshTokenRepository, BlacklistedRefreshTokenRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
builder.Services.AddScoped<IUserRoleCampusRepository, UserRoleCampusRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IUserSessionRefreshTokenRepository, UserSessionRefreshTokenRepository>();
builder.Services.AddScoped<IUserSessionUserWorkProfileSelectedRepository, UserSessionUserWorkProfileSelectedRepository>();
builder.Services.AddScoped<IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository, UserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository>();
builder.Services.AddScoped<IUserWorkProfileRepository, UserWorkProfileRepository>();
builder.Services.AddScoped<IWorkProfileRepository, WorkProfileRepository>();
builder.Services.AddScoped<IWorkProfilePermissionRepository, WorkProfilePermissionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IErrorToHttpMapper, ErrorToHttpMapper>();
builder.Services.AddScoped<IBlacklistedTokenService, BlacklistedTokenService>();
builder.Services.AddScoped<ICurrentUserValidatorService, CurrentUserValidatorService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddAutoMapper(mce =>
{
    mce.AddMaps(typeof(ESAM.GrowTracking.Application.Features.Auth.Login.LoginMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.Login.LoginMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses.LoginUserRoleCampusMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.LoginUserRoleCampuses.LoginUserRoleCampusMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.LoginAssumedRoleMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.LoginAssumedRoleMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.LoginAssumedWorkProfileMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.LoginAssumedWorkProfileMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.Logout.LogoutMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.Application.Features.Auth.Refresh.RefreshMapping).Assembly);
    mce.AddMaps(typeof(ESAM.GrowTracking.API.Controllers.Auth.Refresh.RefreshMapping).Assembly);
});
builder.Services.AddMediatR(mrsc =>
{
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.Login.LoginCommand).Assembly);
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.LoginAssumedRoleCommand).Assembly);
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.LoginAssumedWorkProfileCommand).Assembly);
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.Logout.LogoutCommand).Assembly);
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.Refresh.RefreshCommand).Assembly);
})
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.Login.LoginCommandValidator>()
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.LoginAssumedRoleCommandValidator>()
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.LoginAssumedWorkProfileCommandValidator>()
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.Logout.LogoutCommandValidator>()
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.Refresh.RefreshCommandValidator>();
builder.Services.AddMediatR(mrsc =>
{
    mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses.LoginUserRoleCampusQuery).Assembly);
})
.AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses.LoginUserRoleCampusQueryValidator>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSetting>() ?? throw new InvalidOperationException("JwtSettings are required.");
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(ao =>
{
    ao.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    ao.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jbo =>
{
    jbo.RequireHttpsMetadata = true;
    jbo.SaveToken = true;
    jbo.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(fho =>
{
    fho.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    fho.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
    fho.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("fd00::"), 8));
});
builder.Services.AddControllers().AddJsonOptions(jo =>
{
    jo.JsonSerializerOptions.Converters.Add(new JsonEnumConverterFactory());
    jo.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sgo =>
{
    sgo.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddRobustCors(builder.Configuration, builder.Environment, NullLogger.Instance);
var app = builder.Build();
var cookieSettings = app.Configuration.GetSection("CookieSettings").Exists() ? app.Configuration.GetSection("CookieSettings").Get<CookieSettings>() ?? new CookieSettings() : new CookieSettings();
var corsPolicyName = app.Configuration.GetSection(CorsSettings.SectionName).Exists() ? app.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()?.PolicyName ?? "CorsPolicy" : "CorsPolicy";
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseCors(corsPolicyName);
app.UseHttpsRedirection();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = cookieSettings.SameSite,
    Secure = cookieSettings.UseHostPrefix || cookieSettings.AlwaysSecure ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest,
    HttpOnly = HttpOnlyPolicy.Always,
    OnAppendCookie = ctx =>
    {
        if (cookieSettings.UseHostPrefix && ctx.CookieName != null && ctx.CookieName.StartsWith("__Host-", StringComparison.Ordinal))
        {
            ctx.CookieOptions.Secure = true;
            ctx.CookieOptions.Path = "/";
            ctx.CookieOptions.Domain = null;
        }
    },
    OnDeleteCookie = ctx =>
    {
        if (cookieSettings.UseHostPrefix && ctx.CookieName != null && ctx.CookieName.StartsWith("__Host-", StringComparison.Ordinal))
        {
            ctx.CookieOptions.Secure = true;
            ctx.CookieOptions.Path = "/";
            ctx.CookieOptions.Domain = null;
        }
    }
});
app.UseAuthentication();
app.UseMiddleware<ValidateXsrfHeaderMiddleware>();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();