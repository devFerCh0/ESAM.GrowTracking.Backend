using ESAM.GrowTracking.Application.Commons.Result;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public record LogoutCommand : IRequest<Result>
    {
        public string? RefreshTokenRaw { get; init; }

        public string? DeviceIdentifier { get; init; }

        public LogoutCommand(string? refreshTokenRaw, string? deviceIdentifier)
        {
            RefreshTokenRaw = refreshTokenRaw;
            DeviceIdentifier = deviceIdentifier;
        }
    }
}