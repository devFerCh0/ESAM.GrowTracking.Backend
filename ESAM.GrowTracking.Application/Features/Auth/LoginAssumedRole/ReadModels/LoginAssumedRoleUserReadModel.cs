namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleUserReadModel
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedRoleUserWorkProfileReadModel> LoginAssumedRoleUserWorkProfiles { get; init; }

        public List<LoginAssumedRoleUserRoleCampusReadModel> LoginAssumedRoleUserRoleCampuses { get; init; }

        public LoginAssumedRoleUserSessionReadModel LoginAssumedRoleUserSession { get; init; }

        public LoginAssumedRoleUserReadModel(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedRoleUserWorkProfileReadModel> loginAssumedRoleUserWorkProfiles,
            List<LoginAssumedRoleUserRoleCampusReadModel> loginAssumedRoleUserRoleCampuses, LoginAssumedRoleUserSessionReadModel loginAssumedRoleUserSession)
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