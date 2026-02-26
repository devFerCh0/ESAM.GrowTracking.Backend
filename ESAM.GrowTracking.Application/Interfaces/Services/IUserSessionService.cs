using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Interfaces.Services
{
    public interface IUserSessionService
    {
        Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, int currentWorkProfileId, DateTime utcNow,
            WorkProfileType workProfileType, int currentRoleId = 0, int currentCampusId = 0, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? jti, DateTime? accessTokenExpiration, string revokedReason, int currentUserId,
            DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task RevokeUserSessionAsync(UserSession userSession, string? jti, DateTime? accessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}