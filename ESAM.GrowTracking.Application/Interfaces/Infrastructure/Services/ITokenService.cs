using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Commons.Types;

namespace ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services
{
    public interface ITokenService
    {
        AccessTokenDTO GenerateAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, AccessTokenType accessTokenType, DateTime utcNow, int lifetimeMinutes);

        AccessTokenDTO GenerateAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, AccessTokenType accessTokenType, DateTime utcNow, int lifetimeMinutes, 
            int? workProfileId = null, int? roleId = null, int? campusId = null);

        RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays);
    }
}