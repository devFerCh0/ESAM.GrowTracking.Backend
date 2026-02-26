using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections
{
    public record LoginAssumedRoleUserWorkProfileProjection
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public LoginAssumedRoleUserWorkProfileProjection(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}