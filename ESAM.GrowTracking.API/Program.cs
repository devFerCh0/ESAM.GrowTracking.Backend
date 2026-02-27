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
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
            OnTokenValidated = async ctx => { },
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

            //OnTokenValidated = async ctx =>
            //{
            //    var blacklistedTokenService = ctx.HttpContext.RequestServices
            //        .GetService<IBlacklistedTokenService>();
            //    if (blacklistedTokenService is null)
            //        return;
            //    var jti = ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
            //    if (string.IsNullOrWhiteSpace(jti))
            //    {
            //        ctx.Fail("Access token sin identificador jti; token rechazado.");
            //        return;
            //    }
            //    try
            //    {
            //        var isBlacklisted = await blacklistedTokenService.IsAccessTokenBlacklistedAsync(jti, ctx.HttpContext.RequestAborted);
            //        if (isBlacklisted)
            //            ctx.Fail("Access token revocado.");
            //    }
            //    catch (OperationCanceledException)
            //    {
            //    }
            //    catch (Exception ex)
            //    {
            //        var log = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            //        log.LogError(ex, "Error al verificar blacklist para jti={Jti} TraceId={TraceId}.", jti, ctx.HttpContext.TraceIdentifier);
            //        ctx.Fail("Error al verificar estado del token.");
            //    }
            //}
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