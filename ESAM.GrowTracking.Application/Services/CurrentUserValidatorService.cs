using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Services
{
    public class CurrentUserValidatorService : ICurrentUserValidatorService
    {
        private readonly ILogger<CurrentUserValidatorService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfileRepository _workProfileRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;

        public CurrentUserValidatorService(ILogger<CurrentUserValidatorService> logger, ICurrentUserService currentUserService, IUserRepository userRepository, IUserDeviceRepository userDeviceRepository, 
            IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository, IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository, 
            IWorkProfilePermissionRepository workProfilePermissionRepository)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(currentUserService, $"{nameof(currentUserService)} no puede ser nulo.");
            Guard.AgainstNull(userRepository, $"{nameof(userRepository)} no puede ser nulo.");
            Guard.AgainstNull(userDeviceRepository, $"{nameof(userDeviceRepository)} no puede ser nulo.");
            Guard.AgainstNull(userWorkProfileRepository, $"{nameof(userWorkProfileRepository)} no puede ser nulo.");
            Guard.AgainstNull(workProfileRepository, $"{nameof(workProfileRepository)} no puede ser nulo.");
            Guard.AgainstNull(userRoleCampusRepository, $"{nameof(userRoleCampusRepository)} no puede ser nulo.");
            Guard.AgainstNull(rolePermissionRepository, $"{nameof(rolePermissionRepository)} no puede ser nulo.");
            Guard.AgainstNull(workProfilePermissionRepository, $"{nameof(workProfilePermissionRepository)} no puede ser nulo.");
            _logger = logger;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _userDeviceRepository = userDeviceRepository;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfileRepository = workProfileRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
        }

        public async Task<Result> ValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var currentSecurityStamp = _currentUserService.SecurityStamp!;
            var currentTokenVersion = _currentUserService.TokenVersion!.Value;
            var user = await _userRepository.GetByIdAsync(id: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (user is null || user.IsDeleted || user.IsLocked(utcNow))
            {
                _logger.LogWarning("ValidateCurrentUserAsync: invalid or locked user. UserId={UserId}", currentUserId);
                return Result.Fail(ErrorValueObject.Unauthorized("Usuario inválido o bloqueado."));
            }
            if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
            {
                _logger.LogWarning("ValidateCurrentUserAsync: session invalidated by account changes. UserId={UserId}", currentUserId);
                return Result.Fail(ErrorValueObject.Unauthorized("Sesión invalidada por cambios en la cuenta."));
            }
            return Result.Ok();
        }

        public async Task<Result<User>> GetAndValidateCurrentUserAsync(int currentUserId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var currentSecurityStamp = _currentUserService.SecurityStamp!;
            var currentTokenVersion = _currentUserService.TokenVersion!.Value;
            var user = await _userRepository.GetByIdAsync(id: currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (user is null || user.IsDeleted || user.IsLocked(utcNow))
            {
                _logger.LogWarning("GetAndValidateCurrentUserAsync: invalid or locked user. UserId={UserId}", currentUserId);
                return Result<User>.Fail(ErrorValueObject.Unauthorized("Usuario inválido o bloqueado."));
            }
            if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
            {
                _logger.LogWarning("GetAndValidateCurrentUserAsync: session invalidated by account changes. UserId={UserId}", currentUserId);
                return Result<User>.Fail(ErrorValueObject.Unauthorized("Sesión invalidada por cambios en la cuenta."));
            }
            return Result<User>.Ok(user);

        }

        public async Task<Result> ValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(id: currentUserDeviceId, currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("ValidateCurrentUserDeviceAsync: invalid or locked device. UserId={UserId}, DeviceId={DeviceId}", currentUserId, currentUserDeviceId);
                return Result.Fail(ErrorValueObject.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result.Ok();
        }

        public async Task<Result<UserDevice>> GetAndValidateCurrentUserDeviceAsync(int currentUserId, int currentUserDeviceId, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(id: currentUserDeviceId, currentUserId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
            {
                _logger.LogWarning("GetAndValidateCurrentUserDeviceAsync: invalid or locked device. UserId={UserId}, DeviceId={DeviceId}", currentUserId, currentUserDeviceId);
                return Result<UserDevice>.Fail(ErrorValueObject.Unauthorized("Dispositivo inválido o bloqueado."));
            }
            return Result<UserDevice>.Ok(userDevice);
        }

        public async Task<Result> ValidateUserWorkProfileAndTypeAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(userId: currentUserId, workProfileId: currentWorkProfileId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (userWorkProfile is null || userWorkProfile.IsDeleted)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: user work profile not found or deleted. UserId={UserId}, WorkProfileId={WorkProfileId}", currentUserId, currentWorkProfileId);
                return Result.Fail(ErrorValueObject.NotFound("No se encontró un perfil de trabajo activo asignado al usuario."));
            }
            var isValidWorkProfileType = await _workProfileRepository.IsValidWorkProfileTypeAsync(id: currentWorkProfileId, workProfileType: workProfileType, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!isValidWorkProfileType)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAsync: invalid work profile type for WorkProfileId={WorkProfileId}, ExpectedType={ExpectedType}", currentWorkProfileId, workProfileType);
                return Result.Fail(ErrorValueObject.Validation("El perfil de trabajo no corresponde al tipo esperado."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(int currentUserId, int currentWorkProfileId, WorkProfileType workProfileType, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var validateUserWorkProfileAndTypeResult = await ValidateUserWorkProfileAndTypeAsync(currentUserId: currentUserId, currentWorkProfileId: currentWorkProfileId, workProfileType: workProfileType, 
                asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateUserWorkProfileAndTypeResult.IsFailure)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAndHasPermissionsAsync: base validation failed. UserId={UserId}, WorkProfileId={WorkProfileId}", currentUserId, currentWorkProfileId);
                return Result.Fail(validateUserWorkProfileAndTypeResult.Errors);
            }
            var hasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(workProfileId: currentWorkProfileId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!hasActivePermissions)
            {
                _logger.LogWarning("ValidateUserWorkProfileAndTypeAndHasPermissionsAsync: no active permissions for WorkProfileId={WorkProfileId}", currentWorkProfileId);
                return Result.Fail(ErrorValueObject.NotFound("El perfil de trabajo no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }

        public async Task<Result> ValidateUserRoleCampusAndHasPermissionsAsync(int currentUserId, int currentRoleId, int currentCampusId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(userId: currentUserId, roleId: currentRoleId, campusId: currentCampusId, asTracking: asTracking, 
                cancellationToken: cancellationToken);
            if (userRoleCampus is null || userRoleCampus.IsDeleted)
            {
                _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: user role campus not found or deleted. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}", currentUserId, currentRoleId, currentCampusId);
                return Result.Fail(ErrorValueObject.NotFound("No se encontró un rol de sede activo asignado al usuario."));
            }
            var hasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(roleId: userRoleCampus.RoleId, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!hasActivePermissions)
            {
                _logger.LogWarning("ValidateUserRoleCampusAndHasPermissionsAsync: role has no active permissions. RoleId={RoleId}", userRoleCampus.RoleId);
                return Result.Fail(ErrorValueObject.NotFound("El rol no tiene permisos activos asignados."));
            }
            return Result.Ok();
        }
    }
}