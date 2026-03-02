using ESAM.GrowTracking.API.Commons.Converters;
using ESAM.GrowTracking.API.Commons.Extensions;
using ESAM.GrowTracking.API.Commons.Mappers;
using ESAM.GrowTracking.API.Filters;
using ESAM.GrowTracking.API.Middlewares;
using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Application.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using ESAM.GrowTracking.Infrastructure.Commons.Validators;
using ESAM.GrowTracking.Infrastructure.Services;
using ESAM.GrowTracking.Persistence;
using ESAM.GrowTracking.Persistence.Contexts;
using ESAM.GrowTracking.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<System.Reflection.Assembly>(optional: true);
if (!builder.Environment.IsDevelopment())
    if (builder.Configuration is IConfigurationRoot configRoot)
    {
        var fileProviders = configRoot.Providers.Where(p => p.GetType().FullName is string typeName && (typeName.Contains("JsonConfigurationProvider",
            StringComparison.OrdinalIgnoreCase) || typeName.Contains("XmlConfigurationProvider", StringComparison.OrdinalIgnoreCase) || typeName.Contains("IniConfigurationProvider", StringComparison.OrdinalIgnoreCase)));
        foreach (var provider in fileProviders)
        {
            if (provider.TryGet("JwtSettings:SecretKey", out var secretFromFile) && !string.IsNullOrWhiteSpace(secretFromFile))
                throw new InvalidOperationException("Startup Security: 'JwtSettings:SecretKey' detectado en proveedor de archivo de configuración en entorno no-Development. " +
                    "Los secretos deben proveerse exclusivamente mediante variables de entorno (JWTSETTINGS__SECRETKEY) o un gestor de secretos seguro. " +
                    "Elimine el valor del archivo appsettings.json antes de desplegar en producción.");
            if (provider.TryGet("ConnectionStrings:DefaultConnection", out var connFromFile) && !string.IsNullOrWhiteSpace(connFromFile) && (connFromFile.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
                connFromFile.Contains("pwd=", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Startup Security: 'ConnectionStrings:DefaultConnection' contiene credenciales (Password/pwd) en texto plano " +
                    "en proveedor de archivo de configuración en entorno no-Development. Provea la cadena de conexión mediante variable de entorno (CONNECTIONSTRINGS__DEFAULTCONNECTION) o gestor de secretos.");
        }
    }
builder.Services.AddOptions<JwtSetting>().Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(js => !string.IsNullOrWhiteSpace(js.Issuer) && !string.IsNullOrWhiteSpace(js.Audience) && !string.IsNullOrWhiteSpace(js.SecretKey) && js.SecretKey.Length >= 32,
    "JwtSettings: Issuer, Audience y SecretKey son obligatorios; SecretKey >= 32 caracteres. Establecer JWTSETTINGS__SECRETKEY como variable de entorno.").ValidateOnStart();
builder.Services.AddOptions<TimeSecuritySetting>().Bind(builder.Configuration.GetSection("TimeSecuritySetting"))
    .Validate(tss => tss.TemporaryLifetimeMinutes > 0 && tss.LifetimeMinutes > 0 && tss.LifetimeDays > 0 && tss.AbsoluteLifetimeDays > 0 && tss.IdleWindowDays > 0,
    "La configuración de seguridad de tiempos no es válida.").ValidateOnStart();
builder.Services.AddOptions<ClientInfoSetting>().Bind(builder.Configuration.GetSection("ClientInfo"))
    .Validate(cis => cis == null || cis.IpHeaderKeys == null || cis.IpHeaderKeys.Count > 0,
    "ClientInfo: IpHeaderKeys no debe estar vacío si está configurado.").ValidateOnStart();
builder.Services.AddOptions<LoginSecuritySetting>().Bind(builder.Configuration.GetSection("LoginSecuritySetting"))
    .Validate(lss => lss.MaxFailedAttempts > 0 && lss.LockoutDuration.Minutes > 0 && lss.Duration.Hours > 0,
    "La configuración de seguridad de login no es válida.").ValidateOnStart();
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection("CookieSettings"));
builder.Services.Configure<CleanupSetting>(builder.Configuration.GetSection("CleanupSetting"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection es requerido.");
if (!builder.Environment.IsDevelopment())
{
    if (connectionString.Contains("TrustServerCertificate=True", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("ConnectionStrings: 'TrustServerCertificate=True' detectado en entorno no-Development. " +
            "Use certificados TLS válidos en SQL Server y retire este flag de la cadena de conexión de producción. " +
            "Provea la cadena de conexión sin este flag mediante variable de entorno (CONNECTIONSTRINGS__DEFAULTCONNECTION).");
    if (connectionString.Contains("Integrated Security=True", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Trusted_Connection=True", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Trusted_Connection=yes", StringComparison.OrdinalIgnoreCase))
        Console.Error.WriteLine("ConnectionStrings: 'Integrated Security / Trusted_Connection' detectado en entorno no-Development. " +
            "Asegúrese de que la cuenta de servicio disponga únicamente de los permisos mínimos requeridos (db_datareader, db_datawriter, EXECUTE) " +
            "sobre la base de datos y que no se use la cuenta sa ni cuentas con privilegios de administrador.");
}
builder.Services.AddDbContextPool<AppDbContext>(dcob =>
{
    dcob.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        sql.CommandTimeout(60);
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthCookieServices(builder.Configuration.GetSection("CookieSettings"), builder.Environment, NullLogger.Instance, builder.Configuration);
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
builder.Services.AddHostedService<PurgeExpiredTokensHostedService>();
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
builder.Services.AddMediatR(mrsc => mrsc.RegisterServicesFromAssembly(typeof(ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses.LoginUserRoleCampusQuery).Assembly))
    .AddValidatorsFromAssemblyContaining<ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses.LoginUserRoleCampusQueryValidator>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSetting>() ?? throw new InvalidOperationException("JwtSettings son obligatorios.");
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
builder.Services.AddAuthentication(ao => { ao.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; ao.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
    .AddJwtBearer(jbo =>
    {
        jbo.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        jbo.SaveToken = false;
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
        jbo.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearerEvents");
                try
                {
                    if (!string.IsNullOrWhiteSpace(context.Request.Headers.Authorization.ToString()))
                        return Task.CompletedTask;
                    var path = context.Request.Path;
                    if (path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                    {
                        var accessTokenFromQuery = context.Request.Query["access_token"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(accessTokenFromQuery))
                        {
                            context.Token = accessTokenFromQuery;
                            logger.LogDebug("Token recuperado de Query String para SignalR en: {Path}", path);
                            return Task.CompletedTask;
                        }
                    }
                    if (context.Request.Headers.TryGetValue("X-Access-Token", out var xAccess))
                    {
                        var headerToken = xAccess.FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(headerToken))
                        {
                            context.Token = headerToken;
                            logger.LogDebug("Token recuperado de header X-Access-Token.");
                            return Task.CompletedTask;
                        }
                    }
                    if (context.Request.Cookies.TryGetValue("AccessToken", out var cookieToken))
                        if (!string.IsNullOrWhiteSpace(cookieToken))
                        {
                            context.Token = cookieToken;
                            logger.LogDebug("Token recuperado desde Cookie.");
                            return Task.CompletedTask;
                        }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Excepción al intentar extraer el token en OnMessageReceived.");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                // Obtener claims del token desde el Servicio: CurrentUserService
                // Usar validaciones del Servicio
            },
            //OnTokenValidated = async ctx =>
            //{
            //    // --- Punto 13: resiliencia / timeout corto para llamadas externas ---
            //    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.HttpContext.RequestAborted);
            //    cts.CancelAfter(TimeSpan.FromSeconds(2)); // ajustar según SLA
            //    var ct = cts.Token;

            //    // Resolver servicios desde DI (no obligatorio registrar todos; comprobamos null y degradamos)
            //    var sp = ctx.HttpContext.RequestServices;
            //    var loggerFactory = sp.GetService<ILoggerFactory>() ?? throw new InvalidOperationException("ILoggerFactory no registrado");
            //    var logger = loggerFactory.CreateLogger("Auth.OnTokenValidated");

            //    var revocationService = sp.GetService<ITokenRevocationService>();     // Punto 1
            //    var userReadService = sp.GetService<IUserReadService>();         // Punto 2
            //    var deviceBindingSvc = sp.GetService<IDeviceBindingService>();    // Punto 5
            //    var anomalyService = sp.GetService<IAnomalyDetectionService>(); // Punto 10
            //    var auditLogger = sp.GetService<IAuditLogger>();             // Puntos 9 / 12
            //    var metrics = sp.GetService<IMetrics>();                 // Punto 9

            //    string? userId = null;
            //    string? jti = null;

            //    try
            //    {
            //        // Extraer principal y token (jwt puede ser null si otro validador ya lo procesó)
            //        var principal = ctx.Principal;
            //        var jwtToken = ctx.SecurityToken as JwtSecurityToken;

            //        // Obtener claims clave (sub / nameidentifier) y jti
            //        userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            //                 ?? principal?.FindFirst("sub")?.Value;

            //        jti = principal?.FindFirst("jti")?.Value;

            //        // --- Punto 1: Revocación (jti) ---
            //        if (!string.IsNullOrEmpty(jti) && revocationService != null)
            //        {
            //            bool isRevoked = false;
            //            try
            //            {
            //                isRevoked = await revocationService.IsRevokedAsync(jti, ct);
            //            }
            //            catch (OperationCanceledException) { throw; } // bubbla para manejar timeout
            //            catch (Exception ex)
            //            {
            //                // No bloquear por fallo del servicio de revocación: decisión de disponibilidad
            //                logger.LogWarning(ex, "Revocation check error for jti {Jti}", jti);
            //            }

            //            if (isRevoked)
            //            {
            //                ctx.HttpContext.Items["AuthFailureReason"] = "token_revoked"; // Punto 12
            //                ctx.Fail("token_revoked");
            //                metrics?.IncrementAuthFailure("token_revoked");
            //                if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId ?? "unknown", jti, "fail", "token_revoked", DateTime.UtcNow), ct);
            //                return;
            //            }
            //        }

            //        // --- Punto 2: Usuario existe y activo ---
            //        if (string.IsNullOrEmpty(userId) || userReadService == null)
            //        {
            //            ctx.HttpContext.Items["AuthFailureReason"] = "missing_subject_or_user_service_unavailable";
            //            ctx.Fail("missing_subject");
            //            metrics?.IncrementAuthFailure("missing_subject");
            //            return;
            //        }

            //        var user = await userReadService.GetByIdAsync(userId, ct);
            //        if (user == null || !user.IsActive)
            //        {
            //            ctx.HttpContext.Items["AuthFailureReason"] = "user_not_active";
            //            ctx.Fail("user_not_active");
            //            metrics?.IncrementAuthFailure("user_not_active");
            //            if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId, jti ?? "no-jti", "fail", "user_not_active", DateTime.UtcNow), ct);
            //            return;
            //        }

            //        // --- Punto 4: Validaciones temporales adicionales (iat/nbf/exp defensivas) ---
            //        if (jwtToken != null)
            //        {
            //            // iat: política de edad máxima del token (ej: 30 días)
            //            if (jwtToken.Payload.Iat.HasValue)
            //            {
            //                var iat = DateTimeOffset.FromUnixTimeSeconds(jwtToken.Payload.Iat.Value).UtcDateTime;
            //                var maxTokenAge = TimeSpan.FromDays(30); // configura según política
            //                if (DateTime.UtcNow - iat > maxTokenAge)
            //                {
            //                    ctx.HttpContext.Items["AuthFailureReason"] = "token_too_old";
            //                    ctx.Fail("token_too_old");
            //                    metrics?.IncrementAuthFailure("token_too_old");
            //                    if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId, jti ?? "no-jti", "fail", "token_too_old", DateTime.UtcNow), ct);
            //                    return;
            //                }
            //            }

            //            // nbf: token not before — check con pequeña tolerancia
            //            if (jwtToken.Payload.Nbf.HasValue)
            //            {
            //                var nbf = DateTimeOffset.FromUnixTimeSeconds(jwtToken.Payload.Nbf.Value).UtcDateTime;
            //                var tolerance = TimeSpan.FromMinutes(2);
            //                if (DateTime.UtcNow + tolerance < nbf)
            //                {
            //                    ctx.HttpContext.Items["AuthFailureReason"] = "token_not_yet_valid";
            //                    ctx.Fail("token_not_yet_valid");
            //                    metrics?.IncrementAuthFailure("token_not_yet_valid");
            //                    return;
            //                }
            //            }

            //            // Nota: exp y validaciones básicas son manejadas por TokenValidationParameters,
            //            // aquí solo hacemos chequeos adicionales de política cuando procede.
            //        }

            //        // --- Punto 5: Comprobación device binding si claim presente ---
            //        var deviceId = principal?.FindFirst("device_id")?.Value ?? principal?.FindFirst("cnf")?.Value;
            //        if (!string.IsNullOrEmpty(deviceId) && deviceBindingSvc != null)
            //        {
            //            try
            //            {
            //                var bound = await deviceBindingSvc.IsDeviceBoundToUserAsync(userId, deviceId, ct);
            //                if (!bound)
            //                {
            //                    ctx.HttpContext.Items["AuthFailureReason"] = "device_mismatch";
            //                    ctx.Fail("device_mismatch");
            //                    metrics?.IncrementAuthFailure("device_mismatch");
            //                    if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId, jti ?? "no-jti", "fail", "device_mismatch", DateTime.UtcNow), ct);
            //                    return;
            //                }
            //            }
            //            catch (OperationCanceledException) { throw; } // timeout -> dejar que manejo superior capture
            //            catch (Exception ex)
            //            {
            //                // No bloquear por fallo en servicio de binding; opción de seguridad: fallar cerrado si prefieres
            //                logger.LogWarning(ex, "Device binding check error for user {UserId}", userId);
            //            }
            //        }

            //        // --- Punto 10: Detección de anomalías (heurísticas) ---
            //        if (anomalyService != null)
            //        {
            //            try
            //            {
            //                var remoteIp = ctx.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            //                var anomaly = await anomalyService.EvaluateAsync(userId, remoteIp, jti, ct);
            //                if (anomaly != null && anomaly.IsHighRisk)
            //                {
            //                    ctx.HttpContext.Items["AuthFailureReason"] = $"anomaly:{anomaly.Reason}";
            //                    ctx.Fail("anomalous_activity");
            //                    metrics?.IncrementAuthFailure("anomalous_activity");
            //                    if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId, jti ?? "no-jti", "fail", $"anomaly:{anomaly.Reason}", DateTime.UtcNow), ct);
            //                    return;
            //                }
            //            }
            //            catch (OperationCanceledException) { throw; }
            //            catch (Exception ex)
            //            {
            //                // No bloquear por fallo en detección; registrar en debug.
            //                logger.LogDebug(ex, "Anomaly detection failed for user {UserId}", userId);
            //            }
            //        }

            //        // --- Punto 11: Poblar HttpContext.Items para upstream (Presentation/Application) ---
            //        ctx.HttpContext.Items["UserId"] = userId;
            //        if (!string.IsNullOrEmpty(jti)) ctx.HttpContext.Items["Jti"] = jti;
            //        // No añadimos datos sensibles; solo identificadores mínimos.

            //        // --- Punto 9: Logging y métricas (sin tokens ni PII) ---
            //        logger.LogInformation("Token validated for user {UserId}, jti {Jti}", userId, jti);
            //        metrics?.IncrementAuthSuccess();
            //        if (auditLogger != null) await auditLogger.LogAuthEventAsync(new AuthEvent(userId, jti ?? "no-jti", "success", "validated", DateTime.UtcNow), ct);

            //        // Exitosa validación: no llamar ctx.Success explícitamente; simplemente salir.
            //        return;
            //    }
            //    catch (OperationCanceledException)
            //    {
            //        // --- Punto 13: timeout/resiliencia ---
            //        logger.LogWarning("Token validation cancelled/timed out for user {UserId}", userId);
            //        ctx.HttpContext.Items["AuthFailureReason"] = "validation_timeout";
            //        ctx.Fail("validation_timeout");
            //        sp.GetService<IMetrics>()?.IncrementAuthFailure("validation_timeout");
            //        if (auditLogger != null)
            //        {
            //            // registrar sin bloquear (usar CancellationToken.None porque el original ya canceló)
            //            await auditLogger.LogAuthEventAsync(new AuthEvent(userId ?? "unknown", jti ?? "no-jti", "fail", "validation_timeout", DateTime.UtcNow), CancellationToken.None);
            //        }
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        // --- Punto 12: manejo estandarizado de errores ---
            //        logger.LogError(ex, "Unexpected error during token validation for user {UserId}", userId);
            //        ctx.HttpContext.Items["AuthFailureReason"] = "validation_error";
            //        ctx.Fail("validation_error");
            //        sp.GetService<IMetrics>()?.IncrementAuthFailure("validation_error");
            //        if (auditLogger != null)
            //        {
            //            await auditLogger.LogAuthEventAsync(new AuthEvent(userId ?? "unknown", jti ?? "no-jti", "fail", "validation_error", DateTime.UtcNow), CancellationToken.None);
            //        }
            //        return;
            //    }
            //},
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtBearerEvents") ?? NullLogger.Instance;
                try
                {
                    if (context.Exception is SecurityTokenExpiredException stex)
                    {
                        logger.LogInformation("Autenticación fallida: Token expirado.");
                        context.Response.Headers["X-Token-Expired"] = "true";
                        if (stex.Expires != DateTime.MinValue)
                            context.Response.Headers["X-Token-Expired-At"] = stex.Expires.ToString("o");
                    }
                    else
                        logger.LogWarning(context.Exception, "Autenticación fallida: {Message}", context.Exception.Message);
                }
                catch
                {
                }
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                if (context.Response.HasStarted) return;
                context.HandleResponse();
                try
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    var message = string.IsNullOrWhiteSpace(context.ErrorDescription) ? "Acceso no autorizado. El token es inválido o no fue proporcionado." : context.ErrorDescription;
                    var payload = new { success = false, errors = new[] { new { code = "unauthorized", message } } };
                    //var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var json = JsonSerializer.Serialize(payload, jsonOptions);
                    await context.Response.WriteAsync(json);
                }
                catch (Exception ex)
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtBearerEvents.OnChallenge");
                    logger?.LogError(ex, "Error crítico enviando respuesta 401.");
                }
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                    return;
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>() ?.CreateLogger("JwtBearerEvents.OnForbidden") ?? NullLogger.Instance;
                logger.LogWarning("Acceso Denegado (403): El usuario {User} intentó acceder a {Path}", context.HttpContext.User?.Identity?.Name ?? "Anónimo", context.HttpContext.Request.Path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = new { success = false, errors = new[] { new { code = "forbidden", message = "No tienes permisos para realizar esta acción." } } };
                //var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var json = JsonSerializer.Serialize(payload, jsonOptions);
                await context.Response.WriteAsync(json);
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(fho =>
{
    fho.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    fho.KnownNetworks.Clear();
    fho.KnownProxies.Clear();
    var fhSettings = builder.Configuration.GetSection("ForwardedHeadersSettings").Get<ForwardedHeadersSettings>();
    if (fhSettings?.KnownNetworks is { Count: > 0 })
        foreach (var cidr in fhSettings.KnownNetworks)
        {
            var parts = cidr.Split('/', 2);
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var ip) || !int.TryParse(parts[1], out var prefix))
                throw new InvalidOperationException($"ForwardedHeadersSettings: formato CIDR inválido '{cidr}'. Use notación 'x.x.x.x/n' (p.ej. '10.0.0.0/8').");
            fho.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(ip, prefix));
        }
    if (fhSettings?.KnownProxies is { Count: > 0 })
        foreach (var proxy in fhSettings.KnownProxies)
        {
            if (!IPAddress.TryParse(proxy, out var proxyIp))
                throw new InvalidOperationException($"ForwardedHeadersSettings: IP de proxy inválida '{proxy}'.");
            fho.KnownProxies.Add(proxyIp);
        }
});
builder.Services.AddScoped<ValidateModelStateFilter>();
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddTransient<SecurityHeadersMiddleware>();
builder.Services.AddTransient<ValidateXsrfHeaderMiddleware>();
builder.Services.AddTransient<CorrelationIdMiddleware>();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.AddControllers(options => { options.Filters.AddService<ValidateModelStateFilter>(order: 0); })
    .AddJsonOptions(jo => { jo.JsonSerializerOptions.Converters.Add(new JsonEnumConverterFactory()); jo.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddRateLimiter(rl =>
{
    rl.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var isAuthEndpoint = httpContext.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var partitionKey = isAuthEndpoint ? $"auth:{clientIp}" : $"global:{clientIp}";
        if (isAuthEndpoint)
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
            { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueProcessingOrder = QueueProcessingOrder.OldestFirst, QueueLimit = 0 });
        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
        { PermitLimit = 100, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 6, QueueProcessingOrder = QueueProcessingOrder.OldestFirst, QueueLimit = 0 });
    });
    rl.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/problem+json; charset=utf-8";
        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        var log = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimiterOptions>>();
        log.LogWarning(
            "Rate limit excedido. Path={Path} ClientIp={ClientIp} TraceId={TraceId}", ctx.HttpContext.Request.Path, ctx.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", ctx.HttpContext.TraceIdentifier);
        const string payload = "{\"type\":\"https://tools.ietf.org/html/rfc6585#section-4\",\"title\":\"Too Many Requests\",\"status\":429," +
            "\"detail\":\"Has excedido el límite de solicitudes permitidas. Intente de nuevo en un momento.\"}";
        await ctx.HttpContext.Response.WriteAsync(payload, token);
    };
});
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: ["ready"]).AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"]);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(sgo =>
{
    sgo.CustomSchemaIds(type => type.FullName);
    sgo.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Ingrese 'Bearer {token}' (sin comillas)." });
    sgo.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});
builder.Services.AddRobustCors(builder.Configuration, builder.Environment, NullLogger.Instance);
var app = builder.Build();
var cookieSettings = app.Configuration.GetSection("CookieSettings").Exists() ? app.Configuration.GetSection("CookieSettings").Get<CookieSettings>() ?? new CookieSettings() : new CookieSettings();
var corsPolicyName = app.Configuration.GetSection(CorsSettings.SectionName).Exists() ? app.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()?.PolicyName ?? "CorsPolicy" : "CorsPolicy";
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(corsPolicyName);
app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();
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
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<ValidateXsrfHeaderMiddleware>();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHealthChecks("/health/live", new HealthCheckOptions
{ Predicate = _ => false, ResponseWriter = async (context, _) => { context.Response.ContentType = "application/json"; await context.Response.WriteAsJsonAsync(new { status = "Healthy" }); } });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new { status = report.Status.ToString(), checks = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description }) };
        await context.Response.WriteAsJsonAsync(result);
    }
});
app.MapControllers();
var autoMigrate = app.Configuration.GetValue<bool>("Database:AutoMigrate", false);
if (autoMigrate)
{
    if (app.Environment.IsProduction())
        throw new InvalidOperationException(
            "Database:AutoMigrate = true está bloqueado en entorno de producción por política de seguridad. Establezca Database:AutoMigrate = false en appsettings.json de producción y ejecute " +
            "'dotnet ef database update' o el script SQL equivalente como paso controlado del pipeline CI/CD antes del despliegue, con backup previo de la base de datos.");
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var migrationLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        db.Database.Migrate();
        migrationLogger.LogInformation("Database migrations applied at startup (Database:AutoMigrate = true).");
    }
    catch (Exception ex)
    {
        var migrationLogger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
        migrationLogger?.LogError(ex, "Failed applying migrations at startup.");
        if (app.Environment.IsProduction())
            throw;
    }
}
app.Run();