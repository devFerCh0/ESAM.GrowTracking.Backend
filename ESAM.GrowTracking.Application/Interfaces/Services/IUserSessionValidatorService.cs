using ESAM.GrowTracking.Application.Commons.Result;

namespace ESAM.GrowTracking.Application.Interfaces.Services
{
    public interface IUserSessionValidatorService
    {
        Task<Result> IsJtiBlacklistedAccessTokenTemporaryAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> IsJtiBlacklistedAccessTokenPermanentAsync(string jti, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> IsUserAccountValidAsync(int idUsuario, string secutiryStamp, int tokenVersion, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<Result> ValidateAccessAsync(CancellationToken cancellationToken = default);
    }
}