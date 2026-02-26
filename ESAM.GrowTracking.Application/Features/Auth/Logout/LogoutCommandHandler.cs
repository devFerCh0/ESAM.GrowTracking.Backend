using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Extensions;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHashService _hashService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IUserSessionUserWorkProfileSelectedRepository _userSessionUserWorkProfileSelectedRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfileRepository _workProfileRepository;
        private readonly IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository _userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IDateTimeService dateTimeService, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, 
            ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService, IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, 
            IHashService hashService, IUserRepository userRepository, IUserDeviceRepository userDeviceRepository, IUserSessionUserWorkProfileSelectedRepository userSessionUserWorkProfileSelectedRepository, 
            IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository, 
            IUserSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository, IWorkProfilePermissionRepository workProfilePermissionRepository, 
            IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(validator, $"{nameof(validator)} no puede ser nulo.");
            Guard.AgainstNull(dateTimeService, $"{nameof(dateTimeService)} no puede ser nulo.");
            Guard.AgainstNull(userSessionRefreshTokenRepository, $"{nameof(userSessionRefreshTokenRepository)} no puede ser nulo.");
            Guard.AgainstNull(currentUserService, $"{nameof(currentUserService)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedTokenService, $"{nameof(blacklistedTokenService)} no puede ser nulo.");
            Guard.AgainstNull(unitOfWork, $"{nameof(unitOfWork)} no puede ser nulo.");
            Guard.AgainstNull(userSessionRepository, $"{nameof(userSessionRepository)} no puede ser nulo.");
            Guard.AgainstNull(userSessionService, $"{nameof(userSessionService)} no puede ser nulo.");
            Guard.AgainstNull(hashService, $"{nameof(hashService)} no puede ser nulo.");
            Guard.AgainstNull(userRepository, $"{nameof(userRepository)} no puede ser nulo.");
            Guard.AgainstNull(userDeviceRepository, $"{nameof(userDeviceRepository)} no puede ser nulo.");
            Guard.AgainstNull(userSessionUserWorkProfileSelectedRepository, $"{nameof(userSessionUserWorkProfileSelectedRepository)} no puede ser nulo.");
            Guard.AgainstNull(userWorkProfileRepository, $"{nameof(userWorkProfileRepository)} no puede ser nulo.");
            Guard.AgainstNull(workProfileRepository, $"{nameof(workProfileRepository)} no puede ser nulo.");
            Guard.AgainstNull(userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository, $"{nameof(userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository)} no puede ser nulo.");
            Guard.AgainstNull(workProfilePermissionRepository, $"{nameof(workProfilePermissionRepository)} no puede ser nulo.");
            Guard.AgainstNull(userRoleCampusRepository, $"{nameof(userRoleCampusRepository)} no puede ser nulo.");
            Guard.AgainstNull(rolePermissionRepository, $"{nameof(rolePermissionRepository)} no puede ser nulo.");
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _currentUserService = currentUserService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _hashService = hashService;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _userSessionUserWorkProfileSelectedRepository = userSessionUserWorkProfileSelectedRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfileRepository = workProfileRepository;
            _userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository = userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: validation failed. Errors: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToDomainErrors());
            }
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            int currentUserId;
            string? currentJti = null;
            DateTime? currentAccessTokenExpiration = null;
            string revokedReason;
            UserSession? userSession;
            UserSessionRefreshToken? userSessionRefreshToken = null;
            string? tokenIdentifier = null;
            string? refreshTokenPlain = null;
            if (!string.IsNullOrWhiteSpace(request.RefreshTokenRaw))
            {
                var RefreshTokenRaws = request.RefreshTokenRaw.Split('.');
                if (RefreshTokenRaws.Length == 2)
                {
                    tokenIdentifier = RefreshTokenRaws[0];
                    refreshTokenPlain = RefreshTokenRaws[1];
                    userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByTokenIdentifierAsync(tokenIdentifier: tokenIdentifier, asTracking: asTracking, cancellationToken: cancellationToken);
                }
            }
            if (_currentUserService.IsAuthenticated)
            {
                currentUserId = _currentUserService.UserId!.Value;
                int currentUserSessionId = _currentUserService.UserSessionId!.Value;
                int currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
                currentJti = _currentUserService.Jti;
                currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
                revokedReason = "Cerrar Sesión (Autenticado):";
                if (userSessionRefreshToken is null)
                {
                    var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
                    if (currentAccessTokenType == AccessTokenType.Temporary)
                    {
                        revokedReason += " Access token temporal revocado.";
                        var BlacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(userId: currentUserId, jti: currentJti!, 
                            expirationDate: currentAccessTokenExpiration!.Value, blacklistedAt: utcNow, reason: revokedReason, createdBy: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
                        if (BlacklistedAccessTokenTemporary is not null)
                        {
                            await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(BlacklistedAccessTokenTemporary, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                        return Result.Ok();
                    }
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAnduserDeviceIdAsync(id: currentUserSessionId, userId: currentUserId, userDeviceId: currentUserDeviceId, asTracking: asTracking,
                        cancellationToken: cancellationToken);
                    if (targetUserSession is not null)
                    {
                        revokedReason += " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.";
                        _logger.LogWarning("LogoutCommand: refresh token not found for authenticated session, revoking session for security. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(userSession: targetUserSession, jti: currentJti, accessTokenExpiration: currentAccessTokenExpiration, revokedReason: revokedReason, 
                            currentUserId: currentUserId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
                    }
                    return Result.Ok();
                }
                userSession = await _userSessionRepository.GetByIdAsync(id: userSessionRefreshToken.UserSessionId, asTracking: asTracking, cancellationToken: cancellationToken);
                if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
                {
                    _logger.LogWarning("LogoutCommand: user session and refresh token mismatch. UserId={UserId}, ProvidedTokenIdentifier={TokenIdentifier}, TokenUserSessionId={TokenUserSessionId}, CurrentUserSessionId={CurrentUserSessionId}", 
                        currentUserId, tokenIdentifier!, userSessionRefreshToken?.UserSessionId, currentUserSessionId);
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAnduserDeviceIdAsync(id: currentUserSessionId, userId: currentUserId, userDeviceId: currentUserDeviceId, asTracking: asTracking,
                        cancellationToken: cancellationToken);
                    if (targetUserSession is not null)
                    {
                        revokedReason += " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.";
                        _logger.LogWarning("LogoutCommand: revoking current session due to mismatch. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(userSession: targetUserSession, jti: currentJti, accessTokenExpiration: currentAccessTokenExpiration, revokedReason: revokedReason,
                            currentUserId: currentUserId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
                    }
                    return Result.Ok();
                }
            }
            else
            {
                if (userSessionRefreshToken is null)
                    return Result.Ok();
                userSession = await _userSessionRepository.GetByIdAsync(id: userSessionRefreshToken.UserSessionId, asTracking: asTracking, cancellationToken: cancellationToken);
                if (userSession is null)
                    return Result.Ok();
                currentUserId = userSession.UserId;
                revokedReason = "Cerrar Sesión (No Autenticado):";
            }
            if (userSession.AbsoluteExpiresAt <= utcNow)
                revokedReason += " Sesión absoluta expirada.";
            else if (userSessionRefreshToken.ExpiresAt <= utcNow || userSession.ExpiresAt <= utcNow)
                revokedReason += " Refresh token o sesión expirada.";
            else if (userSessionRefreshToken.IsRevoked || userSession.IsRevoked)
                revokedReason += " Refresh token o sesión ya revocada.";
            else if (!_hashService.VerifyHash(input: refreshTokenPlain!, userSessionRefreshToken.Salt, expectedHash: userSessionRefreshToken.TokenHash))
            {
                revokedReason += " Refresh token inválido.";
                _logger.LogWarning("LogoutCommand: provided refresh token hash mismatch for UserSessionId={UserSessionId}, TokenIdentifier={TokenIdentifier}", userSession.Id, tokenIdentifier!);
            }
            else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
                revokedReason += " Refresh token reemplazado.";
            else
            {
                var user = await _userRepository.GetByIdAsync(id: userSession.UserId, asTracking: asTracking, cancellationToken: cancellationToken);
                if (user is null || user.IsDeleted || user.IsLocked(utcNow))
                {
                    revokedReason += " Usuario no encontrado, deshabilitado o bloqueado.";
                    _logger.LogWarning("LogoutCommand: user invalid for session revocation. UserSessionId={UserSessionId}, UserId={UserId}", userSession.Id, userSession.UserId);
                }
                else
                {
                    var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(id: userSession.UserDeviceId, userId: userSession.UserId, asTracking: asTracking, cancellationToken: cancellationToken);
                    if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
                    {
                        revokedReason += " Dispositivo no encontrado, deshabilitado o bloqueado.";
                        _logger.LogWarning("LogoutCommand: device invalid for session revocation. UserSessionId={UserSessionId}, UserDeviceId={UserDeviceId}", userSession.Id, userSession.UserDeviceId);
                    }
                    else if (!string.Equals(request.DeviceIdentifier, userDevice.DeviceIdentifier, StringComparison.Ordinal))
                    {
                        revokedReason += " DeviceIdentifier no coincide.";
                        _logger.LogWarning("LogoutCommand: device identifier mismatch. Expected={Expected}, Provided={Provided}, UserSessionId={UserSessionId}", userDevice.DeviceIdentifier, request.DeviceIdentifier, userSession.Id);
                    }
                    else
                    {
                        var userSessionUserWorkProfileSelected = await _userSessionUserWorkProfileSelectedRepository.GetByUserSessionIdAsync(userSessionId: userSession.Id, asTracking: asTracking,
                            cancellationToken: cancellationToken);
                        if (userSessionUserWorkProfileSelected is null)
                        {
                            revokedReason += " La sesión no tiene perfil de trabajo seleccionado.";
                            _logger.LogWarning("LogoutCommand: session lacks selected work profile. UserSessionId={UserSessionId}", userSession.Id);
                        }
                        else
                        {
                            var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(userId: userSessionUserWorkProfileSelected.UserId,
                                workProfileId: userSessionUserWorkProfileSelected.WorkProfileId, asTracking: asTracking, cancellationToken: cancellationToken);
                            if (userWorkProfile is null || userWorkProfile.IsDeleted)
                            {
                                revokedReason += " Perfil de trabajo no activo o no asignado.";
                                _logger.LogWarning("LogoutCommand: work profile invalid for session. UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}", userSession.Id, userSessionUserWorkProfileSelected.WorkProfileId);
                            }
                            else
                            {
                                var workProfileType = await _workProfileRepository.GetWorkProfileTypeById(id: userWorkProfile.WorkProfileId, asTracking: asTracking, cancellationToken: cancellationToken);
                                if (workProfileType != WorkProfileType.WithRoles && workProfileType != WorkProfileType.OnlyWorkProfile)
                                {
                                    revokedReason += " Tipo de perfil de trabajo no válido.";
                                    _logger.LogWarning("LogoutCommand: invalid work profile type. WorkProfileId={WorkProfileId}, Type={Type}", userWorkProfile.WorkProfileId, workProfileType);
                                }
                                else
                                {
                                    var userSessionUserWorkProfileSelectedUserRoleCampusSelected = await _userSessionUserWorkProfileSelectedUserRoleCampusSelectedRepository
                                        .GetByUserSessionIdAsync(userSessionId: userSession.Id, asTracking: asTracking, cancellationToken: cancellationToken);
                                    if (workProfileType == WorkProfileType.OnlyWorkProfile)
                                    {
                                        if (userSessionUserWorkProfileSelectedUserRoleCampusSelected is not null)
                                        {
                                            revokedReason += " Tipo OnlyWorkProfile No debe tener rol de sede seleccionado.";
                                            _logger.LogWarning("LogoutCommand: OnlyWorkProfile should not have role-campus selected. UserSessionId={UserSessionId}", userSession.Id);
                                        }
                                        else
                                        {
                                            var hasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(workProfileId: userWorkProfile.WorkProfileId, asTracking: asTracking,
                                                cancellationToken: cancellationToken);
                                            if (!hasActivePermissions)
                                            {
                                                revokedReason += " Perfil de trabajo no tiene permisos asignados.";
                                                _logger.LogWarning("LogoutCommand: work profile lacks permissions. WorkProfileId={WorkProfileId}", userWorkProfile.WorkProfileId);
                                            }
                                            else
                                            {
                                                if (_currentUserService.IsAuthenticated)
                                                {
                                                    int currentTokenVersion = _currentUserService.TokenVersion!.Value;
                                                    string currentSecurityStamp = _currentUserService.SecurityStamp!;
                                                    int currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
                                                    if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
                                                    {
                                                        revokedReason += " SecurityStamp o TokenVersion no coincidentes con el contexto actual.";
                                                        _logger.LogWarning("LogoutCommand: security context mismatch for authenticated logout. UserId={UserId}", user.Id);
                                                    }
                                                    else if (userSessionUserWorkProfileSelected.WorkProfileId != currentWorkProfileId)
                                                    {
                                                        revokedReason += " Perfil seleccionado en la sesión no coincide con el contexto autenticado.";
                                                        _logger.LogWarning("LogoutCommand: selected work profile mismatch with authenticated context. UserSessionId={UserSessionId}", userSession.Id);
                                                    }
                                                    else
                                                        revokedReason += " Correcto.";
                                                }
                                                else
                                                    revokedReason += " Correcto.";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (userSessionUserWorkProfileSelectedUserRoleCampusSelected is null)
                                        {
                                            revokedReason += " Tipo WithRoles requiere rol de sede seleccionado.";
                                            _logger.LogWarning("LogoutCommand: WithRoles session missing role-campus selection. UserSessionId={UserSessionId}", userSession.Id);
                                        }
                                        else
                                        {
                                            var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(userId: userSessionUserWorkProfileSelectedUserRoleCampusSelected.UserId,
                                                roleId: userSessionUserWorkProfileSelectedUserRoleCampusSelected.RoleId, campusId: userSessionUserWorkProfileSelectedUserRoleCampusSelected.CampusId, asTracking: asTracking,
                                                cancellationToken: cancellationToken);
                                            if (userRoleCampus is null || userRoleCampus.IsDeleted)
                                            {
                                                revokedReason += " Rol de sede no activo o no asignado.";
                                                _logger.LogWarning("LogoutCommand: role-campus invalid for session. UserSessionId={UserSessionId}, RoleId={RoleId}, CampusId={CampusId}", userSession.Id, userSessionUserWorkProfileSelectedUserRoleCampusSelected.RoleId, userSessionUserWorkProfileSelectedUserRoleCampusSelected.CampusId);
                                            }
                                            else
                                            {
                                                var hasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(roleId: userRoleCampus.RoleId, asTracking: asTracking,
                                                    cancellationToken: cancellationToken);
                                                if (!hasActivePermissions)
                                                {
                                                    revokedReason += " Rol no tiene permisos asignados.";
                                                    _logger.LogWarning("LogoutCommand: role lacks permissions. RoleId={RoleId}", userRoleCampus.RoleId);
                                                }
                                                else
                                                {
                                                    if (_currentUserService.IsAuthenticated)
                                                    {
                                                        int currentTokenVersion = _currentUserService.TokenVersion!.Value;
                                                        string currentSecurityStamp = _currentUserService.SecurityStamp!;
                                                        int currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
                                                        int currentRoleId = _currentUserService.RoleId!.Value;
                                                        int currentCampusId = _currentUserService.CampusId!.Value;
                                                        if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
                                                        {
                                                            revokedReason += " SecurityStamp o TokenVersion no coincidentes con el contexto actual.";
                                                            _logger.LogWarning("LogoutCommand: security context mismatch for authenticated WithRoles logout. UserId={UserId}", user.Id);
                                                        }
                                                        else if (userSessionUserWorkProfileSelected.WorkProfileId != currentWorkProfileId)
                                                        {
                                                            revokedReason += " Perfil seleccionado en la sesión no coincide con el contexto autenticado.";
                                                            _logger.LogWarning("LogoutCommand: selected work profile mismatch in WithRoles. UserSessionId={UserSessionId}", userSession.Id);
                                                        }
                                                        else if (userSessionUserWorkProfileSelectedUserRoleCampusSelected.RoleId != currentRoleId ||
                                                            userSessionUserWorkProfileSelectedUserRoleCampusSelected.CampusId != currentCampusId)
                                                        {
                                                            revokedReason += " Rol de sede seleccionado en la sesión no coincide con el contexto autenticado.";
                                                            _logger.LogWarning("LogoutCommand: role-campus mismatch with authenticated context. UserSessionId={UserSessionId}", userSession.Id);
                                                        }
                                                        else
                                                            revokedReason += " Correcto.";
                                                    }
                                                    else
                                                        revokedReason += " Correcto.";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            await _userSessionService.RevokeUserSessionAsync(userSession: userSession, jti: currentJti, accessTokenExpiration: currentAccessTokenExpiration, revokedReason: revokedReason, currentUserId: currentUserId, utcNow: utcNow, 
                asTracking: asTracking, cancellationToken: cancellationToken);
            return Result.Ok();
        }
    }
}