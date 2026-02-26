namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses
{
    public record LoginAssumedRoleUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedResponse LoginAssumedRoleUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedRoleUserSessionResponse(int userSessionId, string? ipAddress, string? userAgent,
            LoginAssumedRoleUserSessionUserWorkProfileSelectedResponse loginAssumedRoleUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedRoleUserSessionUserWorkProfileSelected = loginAssumedRoleUserSessionUserWorkProfileSelected;
        }
    }
}