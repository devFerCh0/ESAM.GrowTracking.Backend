using AutoMapper;
using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Extensions;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole
{
    public class LoginAssumedRoleCommandHandler : IRequestHandler<LoginAssumedRoleCommand, Result<LoginAssumedRoleReadModel>>
    {
        private readonly ILogger<LoginAssumedRoleCommandHandler> _logger;
        private readonly IValidator<LoginAssumedRoleCommand> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TimeSecuritySetting _timeSecuritySetting;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public LoginAssumedRoleCommandHandler(ILogger<LoginAssumedRoleCommandHandler> logger, IValidator<LoginAssumedRoleCommand> validator, ICurrentUserService currentUserService, IDateTimeService dateTimeService, 
            ICurrentUserValidatorService currentUserValidatorService, IUserSessionService userSessionService, ITokenService tokenService, IOptions<TimeSecuritySetting> timeSecuritySettingOptions, 
            IUserRepository userRepository, IMapper mapper)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(validator, $"{nameof(validator)} no puede ser nulo.");
            Guard.AgainstNull(currentUserService, $"{nameof(currentUserService)} no puede ser nulo.");
            Guard.AgainstNull(dateTimeService, $"{nameof(dateTimeService)} no puede ser nulo.");
            Guard.AgainstNull(currentUserValidatorService, $"{nameof(currentUserValidatorService)} no puede ser nulo.");
            Guard.AgainstNull(userSessionService, $"{nameof(userSessionService)} no puede ser nulo.");
            Guard.AgainstNull(tokenService, $"{nameof(tokenService)} no puede ser nulo.");
            Guard.AgainstNull(timeSecuritySettingOptions, $"{nameof(timeSecuritySettingOptions)} no puede ser nulo.");
            Guard.Against(timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.LifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.LifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.AbsoluteLifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.AbsoluteLifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.IdleWindowDays <= 0, $"{timeSecuritySettingOptions.Value.IdleWindowDays} debe ser mayor que cero.");
            Guard.AgainstNull(userRepository, $"{nameof(userRepository)} no puede ser nulo.");
            Guard.AgainstNull(mapper, $"{nameof(mapper)} no puede ser nulo.");
            _logger = logger;
            _validator = validator;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _currentUserValidatorService = currentUserValidatorService;
            _userSessionService = userSessionService;
            _tokenService = tokenService;
            _timeSecuritySetting = timeSecuritySettingOptions.Value;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<LoginAssumedRoleReadModel>> Handle(LoginAssumedRoleCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: validation failed. Errors: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<LoginAssumedRoleReadModel>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: unauthenticated access attempt.");
                return Result<LoginAssumedRoleReadModel>.Fail(ErrorValueObject.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            var userResult = await _currentUserValidatorService.GetAndValidateCurrentUserAsync(currentUserId: currentUserId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: get/validate current user failed. UserId: {UserId}", currentUserId);
                return Result<LoginAssumedRoleReadModel>.Fail(userResult.Errors);
            }
            var user = userResult.Value;
            var userDeviceResult = await _currentUserValidatorService.GetAndValidateCurrentUserDeviceAsync(currentUserId: currentUserId, currentUserDeviceId: currentUserDeviceId, utcNow: utcNow, asTracking: asTracking, 
                cancellationToken: cancellationToken);
            if (userDeviceResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: get/validate current user device failed. UserId: {UserId}, UserDeviceId: {UserDeviceId}", currentUserId, currentUserDeviceId);
                return Result<LoginAssumedRoleReadModel>.Fail(userDeviceResult.Errors);
            }
            var userDevice = userDeviceResult.Value;
            var validateUserWorkProfileAndTypeResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId: currentUserId, currentWorkProfileId: request.WorkProfileId!.Value,
                workProfileType: WorkProfileType.WithRoles, asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateUserWorkProfileAndTypeResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: work profile validation failed. UserId: {UserId}, WorkProfileId: {WorkProfileId}", currentUserId, request.WorkProfileId!.Value);
                return Result<LoginAssumedRoleReadModel>.Fail(validateUserWorkProfileAndTypeResult.Errors);
            }
            var validateUserRoleCampusAndHasPermissionsResult = await _currentUserValidatorService.ValidateUserRoleCampusAndHasPermissionsAsync(currentUserId: currentUserId, currentRoleId: request.RoleId!.Value,
                currentCampusId: request.CampusId!.Value, asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateUserRoleCampusAndHasPermissionsResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: role-campus validation or permissions check failed. UserId: {UserId}, RoleId: {RoleId}, CampusId: {CampusId}", currentUserId, request.RoleId!.Value, request.CampusId!.Value);
                return Result<LoginAssumedRoleReadModel>.Fail(validateUserRoleCampusAndHasPermissionsResult.Errors);
            }
            var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId: currentUserId, currentUserDeviceId: currentUserDeviceId, ipAddress: userDevice.LastIp, 
                userAgent: userDevice.LastUserAgent, currentWorkProfileId: request.WorkProfileId!.Value, workProfileType: WorkProfileType.WithRoles, currentRoleId: request.RoleId!.Value, 
                currentCampusId: request.CampusId!.Value, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            var accessToken = _tokenService.GenerateAccessToken(userId: currentUserId, securityStamp: user.SecurityStamp, tokenVersion: user.TokenVersion, userDeviceId: currentUserDeviceId, userSessionId: userSession.Id,
                accessTokenType: AccessTokenType.Permanent, workProfileId: request.WorkProfileId!.Value, roleId: request.RoleId!.Value, campusId: request.CampusId!.Value, utcNow: utcNow, 
                lifetimeMinutes: _timeSecuritySetting.LifetimeMinutes);
            var loginAssumedRoleUser = await _userRepository.GetLoginAssumedRoleUserByUserIdAndUserSessionIdAsync(userId: user.Id, userSessionId: userSession.Id, asTracking: asTracking, 
                cancellationToken: cancellationToken);
            if (loginAssumedRoleUser is null)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: login assumed-role user info not found. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedRoleReadModel>.Fail(ErrorValueObject.NotFound("Usuario no encontrado."));
            }
            if (loginAssumedRoleUser.LoginAssumedRoleUserWorkProfiles is null || loginAssumedRoleUser.LoginAssumedRoleUserWorkProfiles.Count == 0)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: user has no work profiles in assumed-role context. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedRoleReadModel>.Fail(ErrorValueObject.NotFound("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (loginAssumedRoleUser.LoginAssumedRoleUserRoleCampuses is null || loginAssumedRoleUser.LoginAssumedRoleUserRoleCampuses.Count == 0)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: user has no role-campus assignments in assumed-role context. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedRoleReadModel>.Fail(ErrorValueObject.NotFound("El usuario no tiene roles de sede asignados."));
            }
            if (loginAssumedRoleUser.LoginAssumedRoleUserSession is null)
            {
                _logger.LogWarning("LoginAssumedRoleCommand: session missing in returned assumed-role data. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedRoleReadModel>.Fail(ErrorValueObject.NotFound("No se encontró la sesión del usuario."));
            }
            return Result<LoginAssumedRoleReadModel>.Ok(_mapper.Map<LoginAssumedRoleReadModel>((accessToken, refreshToken, loginAssumedRoleUser)));
        }
    }
}