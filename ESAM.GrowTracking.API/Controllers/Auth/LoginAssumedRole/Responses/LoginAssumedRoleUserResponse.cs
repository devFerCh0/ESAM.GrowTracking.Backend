namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses
{
    public record LoginAssumedRoleUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedRoleUserWorkProfileResponse> LoginAssumedRoleUserWorkProfiles { get; init; }

        public List<LoginAssumedRoleUserRoleCampusResponse> LoginAssumedRoleUserRoleCampuses { get; init; }

        public LoginAssumedRoleUserSessionResponse LoginAssumedRoleUserSession { get; init; }

        public LoginAssumedRoleUserResponse(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedRoleUserWorkProfileResponse> loginAssumedRoleUserWorkProfiles,
            List<LoginAssumedRoleUserRoleCampusResponse> loginAssumedRoleUserRoleCampuses, LoginAssumedRoleUserSessionResponse loginAssumedRoleUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            LoginAssumedRoleUserWorkProfiles = loginAssumedRoleUserWorkProfiles;
            LoginAssumedRoleUserRoleCampuses = loginAssumedRoleUserRoleCampuses;
            LoginAssumedRoleUserSession = loginAssumedRoleUserSession;
        }
    }
}