using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserSessionValidatorService : IUserSessionValidatorService
    {
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokenPermanentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;

        public UserSessionValidatorService(IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
            IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokenPermanentRepository, IUserRepository userRepository, IDateTimeService dateTimeService, ICurrentUserService currentUserService)
        {
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository ?? throw new ArgumentNullException(nameof(blacklistedAccessTokenTemporaryRepository));
            _blacklistedAccessTokenPermanentRepository = blacklistedAccessTokenPermanentRepository ?? throw new ArgumentNullException(nameof(blacklistedAccessTokenPermanentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<Result> IsJtiBlacklistedAccessTokenTemporaryAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var exists = await _blacklistedAccessTokenTemporaryRepository.ExistsAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
            if (exists)
                return Result.Fail(ErrorValueObject.Unauthorized("El token temporal ha sido revocado."));
            return Result.Ok();
        }

        public async Task<Result> IsJtiBlacklistedAccessTokenPermanentAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var exists = await _blacklistedAccessTokenPermanentRepository.ExistsAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
            if (exists)
                return Result.Fail(ErrorValueObject.Unauthorized("El token permanente ha sido revocado."));
            return Result.Ok();
        }

        public async Task<Result> IsUserAccountValidAsync(int userId, string secutiryStamp, int tokenVersion, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var validateUserStatus = await _userRepository.ValidateUserStatusAsync(id: userId, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!validateUserStatus)
                return Result.Fail(ErrorValueObject.Unauthorized("Usuario inválido o bloqueado."));
            var validateUserSecutiry = await _userRepository.ValidateUserSecurityAsync(id: userId, securityStamp: secutiryStamp, tokenVersion: tokenVersion, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!validateUserSecutiry)
                return Result.Fail(ErrorValueObject.Unauthorized("Sesión invalidada por cambios en la cuenta."));
            return Result.Ok();
        }

        public async Task<Result> ValidateAccessAsync(CancellationToken cancellationToken = default)
        {
            var accessTokenType = _currentUserService.AccessTokenType!.Value;
            var jti = _currentUserService.Jti!;
            var userId = _currentUserService.UserId!.Value;
            var securityStamp = _currentUserService.SecurityStamp!;
            var tokenVersion = _currentUserService.TokenVersion!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var asTracking = false;
            if (accessTokenType == AccessTokenType.Temporary)
            {
                var isJtiBlacklistedAccessTokenTemporaryResult = await IsJtiBlacklistedAccessTokenTemporaryAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
                if (!isJtiBlacklistedAccessTokenTemporaryResult.IsSuccess)
                    return isJtiBlacklistedAccessTokenTemporaryResult;
            }
            else
            {
                var isJtiBlacklistedAccessTokenPermanentResult = await IsJtiBlacklistedAccessTokenPermanentAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
                if (!isJtiBlacklistedAccessTokenPermanentResult.IsSuccess)
                    return isJtiBlacklistedAccessTokenPermanentResult;
            }
            var isUserAccountValidResult = await IsUserAccountValidAsync(userId: userId, secutiryStamp: securityStamp, tokenVersion: tokenVersion, utcNow: utcNow, asTracking: asTracking, 
                cancellationToken: cancellationToken);
            if (!isUserAccountValidResult.IsSuccess)
                return isUserAccountValidResult;
            return Result.Ok();
        }
    }
}