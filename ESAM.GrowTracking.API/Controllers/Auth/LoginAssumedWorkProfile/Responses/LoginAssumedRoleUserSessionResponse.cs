namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses
{
    public record LoginAssumedWorkProfileUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedResponse LoginAssumedWorkProfileUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent, 
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedResponse loginAssumedWorkProfileUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelected = loginAssumedWorkProfileUserSessionUserWorkProfileSelected;
        }
    }
}