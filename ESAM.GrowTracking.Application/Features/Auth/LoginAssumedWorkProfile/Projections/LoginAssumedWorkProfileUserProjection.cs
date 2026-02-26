namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections
{
    public record LoginAssumedWorkProfileUserProjection
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string? PhotoURL { get; init; }

        public List<LoginAssumedWorkProfileUserWorkProfileProjection> LoginAssumedWorkProfileUserWorkProfiles { get; init; }

        public LoginAssumedWorkProfileUserSessionProjection? LoginAssumedWorkProfileUserSession { get; init; }

        public LoginAssumedWorkProfileUserProjection(int userId, string username, string email, string fullname, string? photoURL, List<LoginAssumedWorkProfileUserWorkProfileProjection> loginAssumedWorkProfileUserWorkProfiles, 
            LoginAssumedWorkProfileUserSessionProjection? loginAssumedWorkProfileUserSession)
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