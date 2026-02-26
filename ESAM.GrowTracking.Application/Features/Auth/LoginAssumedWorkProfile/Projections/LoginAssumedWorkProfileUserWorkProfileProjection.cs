using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections
{
    public record LoginAssumedWorkProfileUserWorkProfileProjection
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public LoginAssumedWorkProfileUserWorkProfileProjection(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}