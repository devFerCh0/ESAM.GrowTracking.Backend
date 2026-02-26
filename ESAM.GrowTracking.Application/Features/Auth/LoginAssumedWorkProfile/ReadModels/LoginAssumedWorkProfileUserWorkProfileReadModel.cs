using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels
{
    public record LoginAssumedWorkProfileUserWorkProfileReadModel
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public LoginAssumedWorkProfileUserWorkProfileReadModel(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}