using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels
{
    public record LoginUserWorkProfileReadModel
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public WorkProfileType WorkProfileType { get; init; }

        public LoginUserWorkProfileReadModel(int workProfileId, string workProfile, WorkProfileType workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}