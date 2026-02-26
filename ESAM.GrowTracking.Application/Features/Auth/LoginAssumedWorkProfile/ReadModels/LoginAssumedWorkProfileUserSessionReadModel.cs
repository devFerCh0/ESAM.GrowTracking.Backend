namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels
{
    public record LoginAssumedWorkProfileUserSessionReadModel
    {
        public int UserSessionId { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel LoginAssumedWorkProfileUserSessionUserWorkProfileSelected { get; init; }

        public LoginAssumedWorkProfileUserSessionReadModel(int userSessionId, string? ipAddress, string? userAgent, 
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel loginAssumedWorkProfileUserSessionUserWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            LoginAssumedWorkProfileUserSessionUserWorkProfileSelected = loginAssumedWorkProfileUserSessionUserWorkProfileSelected;
        }
    }
}