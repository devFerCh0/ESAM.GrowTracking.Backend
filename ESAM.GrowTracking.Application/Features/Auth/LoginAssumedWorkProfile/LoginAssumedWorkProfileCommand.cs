using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile
{
    public record LoginAssumedWorkProfileCommand : IRequest<Result<LoginAssumedWorkProfileReadModel>>
    {
        public int? WorkProfileId { get; init; }

        public LoginAssumedWorkProfileCommand(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}