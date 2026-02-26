using ESAM.GrowTracking.Application.Commons.Types;

namespace ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }

        AccessTokenType? AccessTokenType { get; }

        int? UserId { get; }

        string? SecurityStamp { get; }

        int? TokenVersion { get; }

        string? Jti { get; }

        DateTime? AccessTokenExpiration { get; }

        int? UserDeviceId { get; }

        int? UserSessionId { get; }

        bool? IsPersistent { get; }

        int? WorkProfileId { get; }

        int? RoleId { get; }

        int? CampusId { get; }

        List<string> Permissions { get; }

        bool HasPermission(string permission);
    }
}