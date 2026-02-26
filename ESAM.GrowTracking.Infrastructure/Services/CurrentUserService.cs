using System.Security.Claims;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using ESAM.GrowTracking.Infrastructure.Commons.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ILogger<CurrentUserService> _logger;
        private readonly ClaimsPrincipal? _user;
        private readonly Lazy<List<string>> _permissions;

        public CurrentUserService(ILogger<CurrentUserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(httpContextAccessor, $"{nameof(httpContextAccessor)} no puede ser nulo.");
            _logger = logger;
            _user = httpContextAccessor.HttpContext?.User;
            _permissions = new(() => _user?.GetPermissions() ?? []);
        }

        private ClaimsPrincipal? User => _user;

        public bool IsAuthenticated => User?.IsAuthenticated() ?? false;

        public AccessTokenType? AccessTokenType => User?.GetAccessTokenType();

        public int? UserId => User?.GetUserId();

        public string? SecurityStamp => User?.GetSecurityStamp();

        public int? TokenVersion => User?.GetTokenVersion();

        public string? Jti => User?.GetJti();

        public DateTime? AccessTokenExpiration => User?.GetAccessTokenExpiration();

        public int? UserDeviceId => User?.GetUserDeviceId();

        public int? UserSessionId => User?.GetUserSessionId();

        public bool? IsPersistent => User?.GetIsPersistent();

        public int? WorkProfileId => User?.GetWorkProfileId();

        public int? RoleId => User?.GetRoleId();

        public int? CampusId => User?.GetCampusId();

        public List<string> Permissions
        {
            get
            {
                var perms = _permissions.Value;
                _logger.LogInformation("Permisos cargados.");
                return perms;
            }
        }

        public bool HasPermission(string permission)
        {
            Guard.AgainstNullOrWhiteSpace(permission, $"{nameof(permission)} no puede ser vacío ni espacios en blanco.");
            return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }
    }
}