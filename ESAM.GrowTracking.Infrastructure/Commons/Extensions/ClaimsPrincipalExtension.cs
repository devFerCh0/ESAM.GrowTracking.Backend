using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Infrastructure.Commons.Constants;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace ESAM.GrowTracking.Infrastructure.Commons.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            return user.Identity?.IsAuthenticated ?? false;
        }

        public static AccessTokenType? GetAccessTokenType(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var accessTokenTypeClaim = user.FindFirst(CustomClaimConstant.AccessTokenType)?.Value;
            if (string.IsNullOrWhiteSpace(accessTokenTypeClaim))
                return null;
            return (AccessTokenType)byte.Parse(accessTokenTypeClaim);
        }

        public static int? GetUserId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return null;
            return int.Parse(userIdClaim);
        }

        public static string? GetSecurityStamp(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var securityStampClaim = user.FindFirst(CustomClaimConstant.SecurityStamp)?.Value;
            if (string.IsNullOrWhiteSpace(securityStampClaim))
                return null;
            return securityStampClaim;
        }

        public static int? GetTokenVersion(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var tokenVersionClaim = user.FindFirst(CustomClaimConstant.TokenVersion)?.Value;
            if (string.IsNullOrWhiteSpace(tokenVersionClaim))
                return null;
            return int.Parse(tokenVersionClaim);
        }

        public static string? GetJti(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var jtiClaim = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(jtiClaim))
                return null;
            return jtiClaim;
        }

        public static DateTime? GetAccessTokenExpiration(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var accessTokenExpirationClaim = user.FindFirst(CustomClaimConstant.AccessTokenExpiration)?.Value;
            if (string.IsNullOrWhiteSpace(accessTokenExpirationClaim))
                return null;
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(accessTokenExpirationClaim)).UtcDateTime;
        }

        public static int? GetUserDeviceId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var userDeviceIdClaim = user.FindFirst(CustomClaimConstant.UserDeviceId)?.Value;
            if (string.IsNullOrWhiteSpace(userDeviceIdClaim))
                return null;
            return int.Parse(userDeviceIdClaim);
        }

        public static int? GetUserSessionId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var userSessionIdClaim = user.FindFirst(CustomClaimConstant.UserSessionId)?.Value;
            if (string.IsNullOrWhiteSpace(userSessionIdClaim))
                return null;
            return int.Parse(userSessionIdClaim);
        }

        public static bool? GetIsPersistent(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var isPersistentClaim = user.FindFirst(CustomClaimConstant.IsPersistent)?.Value;
            if (string.IsNullOrWhiteSpace(isPersistentClaim))
                return null;
            return bool.Parse(isPersistentClaim);
        }

        public static int? GetWorkProfileId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var workProfileIdClaim = user.FindFirst(CustomClaimConstant.WorkProfileId)?.Value;
            if (string.IsNullOrWhiteSpace(workProfileIdClaim))
                return null;
            return int.Parse(workProfileIdClaim);
        }

        public static int? GetRoleId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var roleIdClaim = user.FindFirst(CustomClaimConstant.RoleId)?.Value;
            if (string.IsNullOrWhiteSpace(roleIdClaim))
                return null;
            return int.Parse(roleIdClaim);
        }

        public static int? GetCampusId(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            var campusIdClaim = user.FindFirst(CustomClaimConstant.CampusId)?.Value;
            if (string.IsNullOrWhiteSpace(campusIdClaim))
                return null;
            return int.Parse(campusIdClaim);
        }

        public static List<string> GetPermissions(this ClaimsPrincipal user)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            return [.. user.FindAll(CustomClaimConstant.Permissions).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase)];
        }

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            Guard.AgainstNull(user, $"El objeto {nameof(user)} no puede ser nulo.");
            Guard.AgainstNullOrWhiteSpace(permission, $"El {nameof(permission)} proporcionado no puede estar vacío.");
            var permissions = user.GetPermissions();
            var permissionSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
            return permissionSet.Contains(permission);
        }
    }
}