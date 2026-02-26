using ESAM.GrowTracking.Application.Commons.Result;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public record RefreshCommand : IRequest<Result<RefreshReadModel>>
    {
        public string? RefreshTokenRaw { get; init; }

        public string? DeviceIdentifier { get; init; }

        public RefreshCommand(string? refreshTokenRaw, string? deviceIdentifier)
        {
            RefreshTokenRaw = refreshTokenRaw;
            DeviceIdentifier = deviceIdentifier;
        }
    }
}