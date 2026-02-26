namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels
{
    public record LoginAssumedWorkProfileUserReadModel
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedWorkProfileUserWorkProfileReadModel> LoginAssumedWorkProfileUserWorkProfiles { get; init; }

        public LoginAssumedWorkProfileUserSessionReadModel LoginAssumedWorkProfileUserSession { get; init; }

        public LoginAssumedWorkProfileUserReadModel(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedWorkProfileUserWorkProfileReadModel> loginAssumedWorkProfileUserWorkProfiles,
            LoginAssumedWorkProfileUserSessionReadModel loginAssumedWorkProfileUserSession)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            PhotoURL = photoURL;
            LoginAssumedWorkProfileUserWorkProfiles = loginAssumedWorkProfileUserWorkProfiles;
            LoginAssumedWorkProfileUserSession = loginAssumedWorkProfileUserSession;
        }
    }
}