namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses
{
    public record LoginAssumedWorkProfileUserWorkProfileResponse
    {
        public int WorkProfileId { get; init; }

        public string WorkProfile { get; init; }

        public string WorkProfileType { get; init; }

        public LoginAssumedWorkProfileUserWorkProfileResponse(int workProfileId, string workProfile, string workProfileType)
        {
            WorkProfileId = workProfileId;
            WorkProfile = workProfile;
            WorkProfileType = workProfileType;
        }
    }
}