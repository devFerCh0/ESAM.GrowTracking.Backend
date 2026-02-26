namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleUserSessionReadModel
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel LoginAssumedRoleUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedRoleUserSessionReadModel(int userSessionId, string? ipAddress, string? userAgent,
            LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel loginAssumedRoleUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedRoleUserSessionUserWorkProfileSelected = loginAssumedRoleUserSessionUserWorkProfileSelected;
        }
    }
}