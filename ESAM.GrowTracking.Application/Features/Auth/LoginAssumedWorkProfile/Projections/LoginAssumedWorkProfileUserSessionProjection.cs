namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections
{
    public record LoginAssumedWorkProfileUserSessionProjection
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection? LoginAssumedWorkProfileUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionProjection(int userSessionId, string? ipAddress, string? userAgent, 
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection? loginAssumedWorkProfileUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelected = loginAssumedWorkProfileUserSessionUserWorkProfileSelected;
        }
    }
}