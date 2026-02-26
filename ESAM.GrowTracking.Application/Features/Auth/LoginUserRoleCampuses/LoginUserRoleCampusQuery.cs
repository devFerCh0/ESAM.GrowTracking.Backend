using ESAM.GrowTracking.Application.Commons.Result;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses
{
    public record LoginUserRoleCampusQuery : IRequest<Result<List<LoginUserRoleCampusReadModel>>>
    {
        public int? WorkProfileId { get; init; }

        public LoginUserRoleCampusQuery(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}