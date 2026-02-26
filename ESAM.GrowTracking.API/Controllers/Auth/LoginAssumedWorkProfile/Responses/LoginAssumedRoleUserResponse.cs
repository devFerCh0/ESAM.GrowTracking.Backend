namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses
{
    public record LoginAssumedWorkProfileUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedWorkProfileUserWorkProfileResponse> LoginAssumedWorkProfileUserWorkProfiles { get; init; }

        public LoginAssumedWorkProfileUserSessionResponse LoginAssumedWorkProfileUserSession { get; init; }

        public LoginAssumedWorkProfileUserResponse(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedWorkProfileUserWorkProfileResponse> loginAssumedWorkProfileUserWorkProfiles,
            LoginAssumedWorkProfileUserSessionResponse loginAssumedWorkProfileUserSession)
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