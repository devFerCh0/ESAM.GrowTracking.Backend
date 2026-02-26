namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections
{
    public record LoginAssumedRoleUserSessionProjection
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection? LoginAssumedRoleUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedRoleUserSessionProjection(int userSessionId, string? ipAddress, string? userAgent,
            LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection? loginAssumedRoleUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedRoleUserSessionUserWorkProfileSelected = loginAssumedRoleUserSessionUserWorkProfileSelected;
        }
    }
}