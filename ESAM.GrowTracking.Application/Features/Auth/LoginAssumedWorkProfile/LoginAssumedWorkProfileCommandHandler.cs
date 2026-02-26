using AutoMapper;
using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Extensions;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile
{
    public class LoginAssumedWorkProfileCommandHandler : IRequestHandler<LoginAssumedWorkProfileCommand, Result<LoginAssumedWorkProfileReadModel>>
    {
        private readonly ILogger<LoginAssumedWorkProfileCommandHandler> _logger;
        private readonly IValidator<LoginAssumedWorkProfileCommand> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TimeSecuritySetting _timeSecuritySetting;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public LoginAssumedWorkProfileCommandHandler(ILogger<LoginAssumedWorkProfileCommandHandler> logger, IValidator<LoginAssumedWorkProfileCommand> validator, ICurrentUserService currentUserService, IDateTimeService dateTimeService,
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

        public async Task<Result<LoginAssumedWorkProfileReadModel>> Handle(LoginAssumedWorkProfileCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: validation failed. Errors: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<LoginAssumedWorkProfileReadModel>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: unauthenticated access attempt.");
                return Result<LoginAssumedWorkProfileReadModel>.Fail(ErrorValueObject.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            var userResult = await _currentUserValidatorService.GetAndValidateCurrentUserAsync(currentUserId: currentUserId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: get/validate current user failed. UserId: {UserId}", currentUserId);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(userResult.Errors);
            }
            var user = userResult.Value;
            var userDeviceResult = await _currentUserValidatorService.GetAndValidateCurrentUserDeviceAsync(currentUserId: currentUserId, currentUserDeviceId: currentUserDeviceId, utcNow: utcNow, asTracking: asTracking,
                cancellationToken: cancellationToken);
            if (userDeviceResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: get/validate current user device failed. UserId: {UserId}, UserDeviceId: {UserDeviceId}", currentUserId, currentUserDeviceId);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(userDeviceResult.Errors);
            }
            var userDevice = userDeviceResult.Value;
            var validateUserWorkProfileAndTypeAndHasPermissionsResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(currentUserId: currentUserId, 
                currentWorkProfileId: request.WorkProfileId!.Value, workProfileType: WorkProfileType.OnlyWorkProfile, asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateUserWorkProfileAndTypeAndHasPermissionsResult.IsFailure)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: work profile + permissions validation failed. UserId: {UserId}, WorkProfileId: {WorkProfileId}", currentUserId, request.WorkProfileId!.Value);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(validateUserWorkProfileAndTypeAndHasPermissionsResult.Errors);
            }
            var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId: currentUserId, currentUserDeviceId: currentUserDeviceId, ipAddress: userDevice.LastIp,
               userAgent: userDevice.LastUserAgent, currentWorkProfileId: request.WorkProfileId!.Value, utcNow: utcNow, workProfileType: WorkProfileType.OnlyWorkProfile, asTracking: asTracking, 
               cancellationToken: cancellationToken);
            var accessToken = _tokenService.GenerateAccessToken(userId: currentUserId, securityStamp: user.SecurityStamp, tokenVersion: user.TokenVersion, userDeviceId: currentUserDeviceId, userSessionId: userSession.Id,
               accessTokenType: AccessTokenType.Permanent, workProfileId: request.WorkProfileId!.Value, utcNow: utcNow, lifetimeMinutes: _timeSecuritySetting.LifetimeMinutes);
            var loginAssumedWorkProfileUser = await _userRepository.GetLoginAssumedWorkProfileUserByUserIdAndUserSessionIdAsync(userId: user.Id, userSessionId: userSession.Id, asTracking: asTracking,
                cancellationToken: cancellationToken);
            if (loginAssumedWorkProfileUser is null)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: login assumed-workprofile user info not found. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(ErrorValueObject.NotFound("Usuario no encontrado."));
            }
            if (loginAssumedWorkProfileUser.LoginAssumedWorkProfileUserWorkProfiles is null || loginAssumedWorkProfileUser.LoginAssumedWorkProfileUserWorkProfiles.Count == 0)
            {
                _logger.LogWarning("LoginAssumedWorkProfileCommand: user has no work profiles in assumed-workprofile context. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(ErrorValueObject.NotFound("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (loginAssumedWorkProfileUser.LoginAssumedWorkProfileUserSession is null)
            {
                _logger.LogError("LoginAssumedWorkProfileCommand: session missing in returned assumed-workprofile data. UserId: {UserId}, UserSessionId: {UserSessionId}", user.Id, userSession.Id);
                return Result<LoginAssumedWorkProfileReadModel>.Fail(ErrorValueObject.NotFound("No se encontró la sesión del usuario."));
            }
            return Result<LoginAssumedWorkProfileReadModel>.Ok(_mapper.Map<LoginAssumedWorkProfileReadModel>((accessToken, refreshToken, loginAssumedWorkProfileUser)));
        }
    }
}