using AutoMapper;
using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Helpers;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.Settings;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IClientInfoService _clientInfoService;
        private readonly LoginSecuritySetting _loginSecuritySetting;
        private readonly IHashService _hashService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly TimeSecuritySetting _timeSecuritySetting;
        private readonly IMapper _mapper;

        public LoginService(IUserRepository userRepository, IUserDeviceRepository userDeviceRepository, IClientInfoService clientInfoService, IOptions<LoginSecuritySetting> loginSecuritySettingOptions, 
            IHashService hashService, IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<TimeSecuritySetting> timeSecuritySettingOptions, IMapper mapper)
        {
            Guard.AgainstNull(userRepository, $"{nameof(userRepository)} no puede ser nulo.");
            Guard.AgainstNull(userDeviceRepository, $"{nameof(userDeviceRepository)} no puede ser nulo.");
            Guard.AgainstNull(clientInfoService, $"{nameof(clientInfoService)} no puede ser nulo.");
            Guard.AgainstNull(loginSecuritySettingOptions, $"{nameof(loginSecuritySettingOptions)} no puede ser nulo.");
            Guard.Against(loginSecuritySettingOptions.Value.LockoutDuration.Minutes <= 0, $"{loginSecuritySettingOptions.Value.LockoutDuration} debe ser mayor que cero.");
            Guard.Against(loginSecuritySettingOptions.Value.Duration.Hours <= 0, $"{loginSecuritySettingOptions.Value.Duration} debe ser mayor que cero.");
            Guard.Against(loginSecuritySettingOptions.Value.MaxFailedAttempts <= 0, $"{loginSecuritySettingOptions.Value.MaxFailedAttempts} debe ser mayor que cero.");
            Guard.AgainstNull(hashService, $"{nameof(hashService)} no puede ser nulo.");
            Guard.AgainstNull(unitOfWork, $"{nameof(unitOfWork)} no puede ser nulo.");
            Guard.AgainstNull(tokenService, $"{nameof(tokenService)} no puede ser nulo.");
            Guard.AgainstNull(timeSecuritySettingOptions, $"{nameof(timeSecuritySettingOptions)} no puede ser nulo.");
            Guard.Against(timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.TemporaryLifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeMinutes <= 0, $"{timeSecuritySettingOptions.Value.LifetimeMinutes} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.LifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.LifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.AbsoluteLifetimeDays <= 0, $"{timeSecuritySettingOptions.Value.AbsoluteLifetimeDays} debe ser mayor que cero.");
            Guard.Against(timeSecuritySettingOptions.Value.IdleWindowDays <= 0, $"{timeSecuritySettingOptions.Value.IdleWindowDays} debe ser mayor que cero.");
            Guard.AgainstNull(mapper, $"{nameof(mapper)} no puede ser nulo.");
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _clientInfoService = clientInfoService;
            _loginSecuritySetting = loginSecuritySettingOptions.Value;
            _hashService = hashService;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _timeSecuritySetting = timeSecuritySettingOptions.Value;
            _mapper = mapper;
        }

        public async Task<Result<LoginReadModel>> AuthenticateCredentialAsync(LoginCommand request, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (!EnumHelper.TryParseFlexible<ApiClientType>(request.ApiClientType, out var apiClientType))
                return Result<LoginReadModel>.Fail(ErrorValueObject.Validation($"Tipo de cliente del API inválido: '{request.ApiClientType}'"));
            var user = await _userRepository.GetByCredentialAsync(credential: request.Credential!, asTracking: asTracking, cancellationToken: cancellationToken);
            if (user is null)
                return Result<LoginReadModel>.Fail(ErrorValueObject.Unauthorized("Credenciales inválidas."));
            if (user.IsDeleted)
                return Result<LoginReadModel>.Fail(ErrorValueObject.Forbidden("Cuenta inactiva."));
            if (user.IsLocked(utcNow: utcNow))
                return Result<LoginReadModel>.Fail(ErrorValueObject.Locked($"Cuenta bloqueada hasta {user.LockoutEndAt}."));
            var userDevice = await _userDeviceRepository.GetByUserIdAndDeviceIdentifierAsync(userId: user.Id, deviceIdentifier: request.DeviceIdentifier!, asTracking: asTracking, cancellationToken: cancellationToken);
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            if (userDevice is null)
                userDevice = new UserDevice(userId: user.Id, deviceIdentifier: request.DeviceIdentifier!, deviceName: request.DeviceName!, apiClientType: apiClientType, lastIp: ipAddress, lastUserAgent: userAgent, 
                    createdBy: user.Id);
            else
            {
                userDevice.Update(deviceName: request.DeviceName!, apiClientType: apiClientType, lastIp: ipAddress, lastUserAgent: userAgent);
                userDevice.UpdateAudit(updatedAt: utcNow, updatedBy: user.Id);
            }
            if (userDevice.IsLocked(utcNow: utcNow))
                return Result<LoginReadModel>.Fail(ErrorValueObject.Locked($"Dispositivo bloqueado hasta {userDevice.LockoutEndAt}."));
            var resetPerformed = false;
            if (userDevice.ShouldResetFailedAttempts(duration: _loginSecuritySetting.Duration, utcNow) && userDevice.FailedLoginCount > 0)
            {
                userDevice.ResetFailedLogin();
                resetPerformed = true;
            }
            user.UpdateAudit(updatedAt: utcNow, updatedBy: user.Id);
            userDevice.UpdateLastSeenAt(lastSeenAt: utcNow);
            var verifyHash = _hashService.VerifyHash(input: request.Password!, salt: user.Salt, expectedHash: user.PasswordHash);
            if (!verifyHash)
            {
                userDevice.RegisterFailedLogin(maxFailedAttempts: _loginSecuritySetting.MaxFailedAttempts, lockoutDuration: _loginSecuritySetting.LockoutDuration, lastFailedLoginAt: utcNow);
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (userDevice.Id == 0)
                        await _unitOfWork.UserDevices.InsertAsync(userDevice, ct);
                    else
                        await _unitOfWork.UserDevices.UpdateAsync(userDevice, ct);
                    await _unitOfWork.Users.UpdateAsync(user, ct);
                }, cancellationToken);
                var remaining = Math.Max(0, _loginSecuritySetting.MaxFailedAttempts - userDevice.FailedLoginCount);
                if (remaining > 0)
                    return Result<LoginReadModel>.Fail(ErrorValueObject.Unauthorized($"Contraseña incorrecta. Te quedan {remaining} intento(s)."));
                return Result<LoginReadModel>.Fail(ErrorValueObject.Locked("Has superado el número de intentos. Dispositivo bloqueado temporalmente."));
            }
            if (!resetPerformed && userDevice.FailedLoginCount > 0)
                userDevice.ResetFailedLogin();
            userDevice.UpdateLastLogin(lastLoginAt: utcNow);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userDevice.Id == 0)
                    await _unitOfWork.UserDevices.InsertAsync(userDevice, ct);
                else
                    await _unitOfWork.UserDevices.UpdateAsync(userDevice, ct);
                await _unitOfWork.Users.UpdateAsync(user, ct);
            }, cancellationToken);
            var accessToken = _tokenService.GenerateAccessToken(userId: user.Id, securityStamp: user.SecurityStamp, tokenVersion: user.TokenVersion, userDeviceId: userDevice.Id, isPersistent: request.IsPersistent!.Value,
                accessTokenType: AccessTokenType.Temporary, utcNow: utcNow, lifetimeMinutes: _timeSecuritySetting.TemporaryLifetimeMinutes);
            var loginUser = await _userRepository.GetLoginUserByIdAsync(id: user.Id, asTracking: asTracking, cancellationToken: cancellationToken);
            if (loginUser is null)
                return Result<LoginReadModel>.Fail(ErrorValueObject.NotFound("Usuario no encontrado."));
            if (loginUser.LoginUserWorkProfiles is null || loginUser.LoginUserWorkProfiles.Count == 0)
                return Result<LoginReadModel>.Fail(ErrorValueObject.NotFound("Usuario sin perfiles de trabajo."));
            return Result<LoginReadModel>.Ok(_mapper.Map<LoginReadModel>((accessToken, loginUser)));
        }
    }
}