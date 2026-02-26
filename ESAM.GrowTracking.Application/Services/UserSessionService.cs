using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ITokenService _tokenService;
        private readonly TimeSecuritySetting _timeSecuritySetting;
        private readonly IHashService _hashService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;

        public UserSessionService(ICurrentUserService currentUserService, ITokenService tokenService, IOptions<TimeSecuritySetting> timeSecuritySettingOptions, IHashService hashService,
            IBlacklistedTokenService blacklistedTokenService, IUnitOfWork unitOfWork, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository)
        {
            Guard.AgainstNull(currentUserService, $"{nameof(currentUserService)} no puede ser nulo.");
            Guard.AgainstNull(tokenService, $"{nameof(tokenService)} no puede ser nulo.");
            Guard.AgainstNull(timeSecuritySettingOptions, $"{nameof(timeSecuritySettingOptions)} no puede ser nulo.");
            Guard.Against(timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.LifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.LifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.AbsoluteLifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.AbsoluteLifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.IdleWindowDays <= 0, $"{timeSecuritySettingOptions.Value.IdleWindowDays} debe ser mayor que cero.");
            Guard.AgainstNull(hashService, $"{nameof(hashService)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedTokenService, $"{nameof(blacklistedTokenService)} no puede ser nulo.");
            Guard.AgainstNull(unitOfWork, $"{nameof(unitOfWork)} no puede ser nulo.");
            Guard.AgainstNull(userSessionRefreshTokenRepository, $"{nameof(userSessionRefreshTokenRepository)} no puede ser nulo.");
            _currentUserService = currentUserService;
            _tokenService = tokenService;
            _timeSecuritySetting = timeSecuritySettingOptions.Value;
            _hashService = hashService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
        }

        public async Task<(RefreshTokenDTO, UserSession)> CreateUserSessionAsync(int currentUserId, int currentUserDeviceId, string? ipAddress, string? userAgent, int currentWorkProfileId, DateTime utcNow,
            WorkProfileType workProfileType, int currentRoleId = 0, int currentCampusId = 0, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var currentJti = _currentUserService.Jti!;
            var currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration!.Value;
            var currentIsPersistent = _currentUserService.IsPersistent!.Value;
            var (refreshToken, tokenSalt, tokenHash) = GetRefreshToken(utcNow: utcNow);
            var userSession = new UserSession(userId: currentUserId, userDeviceId: currentUserDeviceId, ipAddress: ipAddress, userAgent: userAgent, expiresAt: utcNow.AddDays(_timeSecuritySetting.IdleWindowDays),
                absoluteExpiresAt: utcNow.AddDays(_timeSecuritySetting.AbsoluteLifetimeDays), isPersistent: currentIsPersistent, createdBy: currentUserId);
            var userSessionUserWorkProfileSelected = new UserSessionUserWorkProfileSelected(userId: currentUserId, workProfileId: currentWorkProfileId);
            var userSessionUserWorkProfileSelectedUserRoleCampusSelected = (workProfileType == WorkProfileType.WithRoles) ? 
                new UserSessionUserWorkProfileSelectedUserRoleCampusSelected(userId: currentUserId, roleId: currentRoleId, campusId: currentCampusId) : null;
            userSession.UpdateLastActivity(lastActivityAt: utcNow);
            var userSessionRefrehToken = new UserSessionRefreshToken(tokenIdentifier: refreshToken.TokenIdentifier, salt: tokenSalt, tokenHash: tokenHash, expiresAt: refreshToken.RefreshTokenExpiresAt, 
                createdBy: currentUserId);
            userSessionRefrehToken.UpdateLastUsedAt(lastUsedAt: utcNow);
            var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(userId: currentUserId, jti: currentJti, expirationDate: currentAccessTokenExpiration, 
                blacklistedAt: utcNow, reason: "Cierre de sesión (Autenticado): Access token temporal revocado.", createdBy: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessions.InsertAsync(userSession, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                userSessionUserWorkProfileSelected.AddUserSessionId(userSessionId: userSession.Id);
                await _unitOfWork.UserSessionUserWorkProfilesSelected.InsertAsync(userSessionUserWorkProfileSelected, ct);
                if (userSessionUserWorkProfileSelectedUserRoleCampusSelected is not null)
                {
                    userSessionUserWorkProfileSelectedUserRoleCampusSelected.AddUserSessionId(userSessionId: userSession.Id);
                    await _unitOfWork.UserSessionUserWorkProfileSelectedUserRoleCampusesSelected.InsertAsync(userSessionUserWorkProfileSelectedUserRoleCampusSelected, ct);
                }
                userSessionRefrehToken.AddUserSessionId(userSessionId: userSession.Id);
                await _unitOfWork.UserSessionRefreshTokens.InsertAsync(userSessionRefrehToken, ct);
                if (blacklistedAccessTokenTemporary is not null)
                    await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, ct);
            }, cancellationToken);
            return (refreshToken, userSession);
        }

        public async Task<RefreshTokenDTO> RotateUserSessionAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, string? jti, DateTime? accessTokenExpiration, string revokedReason, int currentUserId,
            DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var (refreshToken, tokenSalt, tokenHash) = GetRefreshToken(utcNow: utcNow);
            var newUserSessionRefreshToken = new UserSessionRefreshToken(userSessionId: userSession.Id, tokenIdentifier: refreshToken.TokenIdentifier, salt: tokenSalt, tokenHash: tokenHash,
                expiresAt: refreshToken.RefreshTokenExpiresAt, rotationCount: userSessionRefreshToken.RotationCount + 1, createdBy: userSession.UserId);
            userSession.UpdateExpiresAt(expiresAt: utcNow.AddDays(_timeSecuritySetting.IdleWindowDays));
            userSession.UpdateLastActivity(lastActivityAt: utcNow);
            userSession.UpdateAudit(updatedAt: utcNow, updatedBy: currentUserId);
            userSessionRefreshToken.Revoke(revokedAt: utcNow, revokedReason: revokedReason);
            userSessionRefreshToken.UpdateLastUsedAt(lastUsedAt: utcNow);
            userSessionRefreshToken.UpdateAudit(updatedAt: utcNow, updatedBy: userSession.UserId);
            var blacklistedRefreshToken = await _blacklistedTokenService.TryGenerateBlacklistedRefreshTokenAsync(userSessionRefreshTokenId: userSessionRefreshToken.Id, tokenIdentifier: userSessionRefreshToken.TokenIdentifier,
                expirationDate: userSessionRefreshToken.ExpiresAt, blacklistedAt: utcNow, reason: revokedReason, createdBy: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            var blacklistedAccessTokenPermanent = (jti is not null && accessTokenExpiration is not null) ? await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenPermanentAsync(userSessionId: userSession.Id,
                jti: jti, expirationDate: accessTokenExpiration.Value, blacklistedAt: utcNow, reason: revokedReason, createdBy: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken) : null;
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessionRefreshTokens.InsertAsync(newUserSessionRefreshToken, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.UserSessions.UpdateAsync(userSession, ct);
                userSessionRefreshToken.UpdateReplacedByUserSessionRefreshTokenId(replacedByUserSessionRefreshTokenId: newUserSessionRefreshToken.Id);
                await _unitOfWork.UserSessionRefreshTokens.UpdateAsync(userSessionRefreshToken, ct);
                if (blacklistedRefreshToken is not null)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertAsync(blacklistedRefreshToken, ct);
                if (blacklistedAccessTokenPermanent is not null)
                    await _unitOfWork.BlacklistedAccessTokensPermanent.InsertAsync(blacklistedAccessTokenPermanent, ct);
            }, cancellationToken);
            return refreshToken;
        }

        public async Task RevokeUserSessionAsync(UserSession userSession, string? jti, DateTime? accessTokenExpiration, string revokedReason, int currentUserId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            UserSession? userSessionRevoked = null;
            if (!userSession.IsRevoked)
            {
                userSession.Revoke(revokedAt: utcNow, revokedReason: revokedReason, closedByUserId: currentUserId);
                userSession.UpdateLastActivity(lastActivityAt: utcNow);
                userSession.UpdateAudit(updatedAt: utcNow, updatedBy: currentUserId);
                userSessionRevoked = userSession;
            }
            var userSessionRefreshTokens = await _userSessionRefreshTokenRepository.GetAllByUserSessionIdAsync(userSessionId: userSession.Id, asTracking: asTracking, cancellationToken: cancellationToken);
            var userSessionRefreshTokensToRevoke = userSessionRefreshTokens.Where(usrt => !usrt.IsRevoked).ToList();
            foreach (var userSessionRefreshTokenToRevoke in userSessionRefreshTokensToRevoke)
            {
                userSessionRefreshTokenToRevoke.Revoke(revokedAt: utcNow, revokedReason: revokedReason);
                userSessionRefreshTokenToRevoke.UpdateLastUsedAt(lastUsedAt: utcNow);
                userSessionRefreshTokenToRevoke.UpdateAudit(updatedAt: utcNow, updatedBy: currentUserId);
            }
            var blacklistedRefreshTokens = await _blacklistedTokenService.GetPendingBlacklistedRefreshTokensAsync(
                userSessionRefreshTokens: [.. userSessionRefreshTokens.Select(usrt => (usrt.Id, usrt.TokenIdentifier, usrt.ExpiresAt))], blacklistedAt: utcNow, reason: revokedReason, currentUserId: currentUserId, 
                asTracking: asTracking, cancellationToken: cancellationToken);
            var blacklistedAccessTokenPermanent = (jti is not null && accessTokenExpiration is not null) ? await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenPermanentAsync(userSessionId: userSession.Id, 
                jti: jti, expirationDate: accessTokenExpiration.Value, blacklistedAt: utcNow, reason: revokedReason, createdBy: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken) : null;
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionRevoked is not null)
                    await _unitOfWork.UserSessions.UpdateAsync(userSessionRevoked, ct);
                if (userSessionRefreshTokens.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokens, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
                if (blacklistedAccessTokenPermanent is not null)
                    await _unitOfWork.BlacklistedAccessTokensPermanent.InsertAsync(blacklistedAccessTokenPermanent, ct);
            }, cancellationToken);
        }

        private (RefreshTokenDTO, string, string) GetRefreshToken(DateTime utcNow)
        {
            var refreshToken = _tokenService.GenerateRefreshToken(utcNow: utcNow, lifetimeDays: _timeSecuritySetting.LifetimeDays);
            var tokenSalt = _hashService.GenerateSalt();
            var tokenHash = _hashService.ComputeHash(input: refreshToken.RefreshTokenPlain, salt: tokenSalt);
            return (refreshToken, tokenSalt, tokenHash);
        }
    }
}