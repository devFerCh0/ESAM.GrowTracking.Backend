using AutoMapper;
using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Commons.Extensions;
using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Catalogs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses
{
    public class LoginUserRoleCampusQueryHandler : IRequestHandler<LoginUserRoleCampusQuery, Result<List<LoginUserRoleCampusReadModel>>>
    {
        private readonly ILogger<LoginUserRoleCampusQueryHandler> _logger;
        private readonly IValidator<LoginUserRoleCampusQuery> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IMapper _mapper;

        public LoginUserRoleCampusQueryHandler(ILogger<LoginUserRoleCampusQueryHandler> logger, IValidator<LoginUserRoleCampusQuery> validator, ICurrentUserService currentUserService, IDateTimeService dateTimeService,
            ICurrentUserValidatorService currentUserValidatorService, IUserRoleCampusRepository userRoleCampusRepository, IMapper mapper)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(validator, $"{nameof(validator)} no puede ser nulo.");
            Guard.AgainstNull(currentUserService, $"{nameof(currentUserService)} no puede ser nulo.");
            Guard.AgainstNull(dateTimeService, $"{nameof(dateTimeService)} no puede ser nulo.");
            Guard.AgainstNull(currentUserValidatorService, $"{nameof(currentUserValidatorService)} no puede ser nulo.");
            Guard.AgainstNull(userRoleCampusRepository, $"{nameof(userRoleCampusRepository)} no puede ser nulo.");
            Guard.AgainstNull(mapper, $"{nameof(mapper)} no puede ser nulo.");
            _validator = validator;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _currentUserValidatorService = currentUserValidatorService;
            _userRoleCampusRepository = userRoleCampusRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<LoginUserRoleCampusReadModel>>> Handle(LoginUserRoleCampusQuery request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: validation failed. Errors: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: unauthenticated access attempt.");
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(ErrorValueObject.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            var validateCurrentUserResult = await _currentUserValidatorService.ValidateCurrentUserAsync(currentUserId: currentUserId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateCurrentUserResult.IsFailure)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: current user validation failed. UserId: {UserId}", currentUserId);
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(validateCurrentUserResult.Errors);
            }
            var validateCurrentUserDeviceResult = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId: currentUserId, currentUserDeviceId: currentUserDeviceId, utcNow: utcNow, 
                asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateCurrentUserDeviceResult.IsFailure)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: current user device validation failed. UserId: {UserId}, UserDeviceId: {UserDeviceId}", currentUserId, currentUserDeviceId);
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(validateCurrentUserDeviceResult.Errors);
            }
            var validateUserWorkProfileAndTypeResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId: currentUserId, currentWorkProfileId: request.WorkProfileId!.Value,
                workProfileType: WorkProfileType.WithRoles, asTracking: asTracking, cancellationToken: cancellationToken);
            if (validateUserWorkProfileAndTypeResult.IsFailure)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: work profile validation failed. UserId: {UserId}, WorkProfileId: {WorkProfileId}", currentUserId, request.WorkProfileId!.Value);
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(validateUserWorkProfileAndTypeResult.Errors);
            }
            var loginUserRoleCampuses = await _userRoleCampusRepository.GetLoginUserRoleCampusesByUserIdAsync(userId: currentUserId, asTracking: asTracking, cancellationToken);
            if (loginUserRoleCampuses is null || loginUserRoleCampuses.Count == 0)
            {
                _logger.LogWarning("LoginUserRoleCampusQuery: no role-campus assignments found for user. UserId: {UserId}", currentUserId);
                return Result<List<LoginUserRoleCampusReadModel>>.Fail(ErrorValueObject.NotFound("No se encontraron roles asignados para el usuario."));
            }
            return Result<List<LoginUserRoleCampusReadModel>>.Ok(_mapper.Map<List<LoginUserRoleCampusReadModel>>(loginUserRoleCampuses));
        }
    }
}