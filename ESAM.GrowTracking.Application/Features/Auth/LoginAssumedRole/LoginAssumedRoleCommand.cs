using ESAM.GrowTracking.Application.Commons.Result;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole
{
    public record LoginAssumedRoleCommand : IRequest<Result<LoginAssumedRoleReadModel>>
    {
        public int? WorkProfileId { get; init; }

        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public LoginAssumedRoleCommand(int? workProfileId, int? roleId, int? campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}