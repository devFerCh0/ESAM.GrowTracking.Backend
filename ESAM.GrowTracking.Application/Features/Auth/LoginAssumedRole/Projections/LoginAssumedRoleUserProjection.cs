namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections
{
    public record LoginAssumedRoleUserProjection
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedRoleUserWorkProfileProjection> LoginAssumedRoleUserWorkProfiles { get; init; }

        public List<LoginAssumedRoleUserRoleCampusProjection> LoginAssumedRoleUserRoleCampuses { get; init; }

        public LoginAssumedRoleUserSessionProjection? LoginAssumedRoleUserSession { get; init; }

        public LoginAssumedRoleUserProjection(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedRoleUserWorkProfileProjection> loginAssumedRoleUserWorkProfiles, 
            List<LoginAssumedRoleUserRoleCampusProjection> loginAssumedRoleUserRoleCampuses, LoginAssumedRoleUserSessionProjection? loginAssumedRoleUserSession)
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