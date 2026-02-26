namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses
{
    public record LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedResponse
    {
        public int WorkProfileIdSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedResponse(int workProfileIdSelected)
        {
            WorkProfileIdSelected = workProfileIdSelected;
        }
    }
}