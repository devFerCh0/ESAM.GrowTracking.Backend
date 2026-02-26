using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using ESAM.GrowTracking.Infrastructure.Commons.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class ClientInfoService : IClientInfoService
    {
        private readonly ILogger<ClientInfoService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HashSet<string> _hashSet;
        private readonly IIpAddressValidator _ipAddressValidator;

        public ClientInfoService(ILogger<ClientInfoService> logger, IHttpContextAccessor httpContextAccessor, IOptions<ClientInfoSetting> options, IIpAddressValidator validator)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(httpContextAccessor, $"{nameof(httpContextAccessor)} no puede ser nulo.");
            Guard.AgainstNull(options, $"{nameof(options)} no puede ser nulo.");
            Guard.AgainstNull(options.Value, $"{nameof(options.Value)} no puede ser nulo.");
            Guard.Against(options.Value.IpHeaderKeys is null || options.Value.IpHeaderKeys.Count == 0, $"{nameof(options.Value.IpHeaderKeys)} debe contener al menos una clave de encabezado.");
            Guard.AgainstNull(validator, $"{nameof(validator)} no puede ser nulo.");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _hashSet = new HashSet<string>(options.Value.IpHeaderKeys!, StringComparer.OrdinalIgnoreCase);
            _ipAddressValidator = validator;
        }

        public string? GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                _logger.LogWarning("HttpContext no disponible; IP no resuelta.");
                return null;
            }
            foreach (var header in _hashSet)
                if (context.Request.Headers.TryGetValue(header, out var values))
                {
                    var raw = values.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        var candidate = raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).FirstOrDefault();
                        if (candidate != null && _ipAddressValidator.TryValidate(candidate, out var ip) && ip != null)
                        {
                            var ipString = ip.ToString();
                            _logger.LogInformation("IP resuelta desde header {Header}: {Ip}", header, ipString);
                            return ipString;
                        }
                        _logger.LogWarning("Formato IP inválido en header {Header}: {Value}", header, raw);
                    }
                }
            var remote = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remote) && _ipAddressValidator.TryValidate(remote, out var addr) && addr != null)
            {
                var remoteString = addr.ToString();
                _logger.LogInformation("IP resuelta desde conexión: {Ip}", remoteString);
                return remoteString;
            }
            _logger.LogWarning("No se pudo determinar la IP del cliente.");
            return null;
        }

        public string? GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                _logger.LogWarning("HttpContext no disponible; User-Agent no resuelto.");
                return null;
            }
            const string uaHeader = "User-Agent";
            if (context.Request.Headers.TryGetValue(uaHeader, out var uaValues))
            {
                var ua = uaValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ua))
                {
                    _logger.LogInformation("User-Agent resuelto: {Ua}", ua);
                    return ua;
                }
            }
            _logger.LogWarning("Header User-Agent faltante o vacío.");
            return null;
        }
    }
}