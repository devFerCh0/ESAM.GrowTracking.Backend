using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Commons.ValueObjects;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Entities;
using System.Threading;

namespace ESAM.GrowTracking.Application.Services
{
    public class UserSessionValidatorService : IUserSessionValidatorService
    {
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokenPermanentRepository;
        private readonly IUserRepository _userRepository;

        public UserSessionValidatorService(IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, 
            IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokenPermanentRepository, IUserRepository userRepository)
        {
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository ?? throw new ArgumentNullException(nameof(blacklistedAccessTokenTemporaryRepository));
            _blacklistedAccessTokenPermanentRepository = blacklistedAccessTokenPermanentRepository ?? throw new ArgumentNullException(nameof(blacklistedAccessTokenPermanentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        public async Task<Result> IsUserAccountValidAsync(int idUsuario, string secutiryStamp, int tokenVersion, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var validateUserStatus = await _userRepository.ValidateUserStatusAsync(id: idUsuario, utcNow: utcNow, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!validateUserStatus)
                return Result.Fail(ErrorValueObject.Unauthorized("Usuario inválido o bloqueado."));
            var validateUserSecutiry = await _userRepository.ValidateUserSecurityAsync(id: idUsuario, securityStamp: secutiryStamp, tokenVersion: tokenVersion, asTracking: asTracking, cancellationToken: cancellationToken);
            if (!validateUserSecutiry)
                return Result.Fail(ErrorValueObject.Unauthorized("Sesión invalidada por cambios en la cuenta."));
            return Result.Ok();
        }
    }
}