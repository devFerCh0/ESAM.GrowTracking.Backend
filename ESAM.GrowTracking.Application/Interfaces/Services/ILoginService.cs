using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels;

namespace ESAM.GrowTracking.Application.Interfaces.Services
{
    public interface ILoginService
    {
        Task<Result<LoginReadModel>> AuthenticateCredentialAsync(LoginCommand request, DateTime utcNow, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}