namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile
{
    public record LoginAssumedWorkProfileRequest
    {
        public int? WorkProfileId { get; init; }

        public LoginAssumedWorkProfileRequest(int? workProfileId)
        {
            WorkProfileId = workProfileId;
        }
    }
}