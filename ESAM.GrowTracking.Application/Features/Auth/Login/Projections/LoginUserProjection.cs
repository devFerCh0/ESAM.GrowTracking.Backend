namespace ESAM.GrowTracking.Application.Features.Auth.Login.Projections
{
    public record LoginUserProjection
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public List<LoginUserWorkProfileProjection> LoginUserWorkProfiles { get; init; }

        public LoginUserProjection(int userId, string username, string email, string fullName, List<LoginUserWorkProfileProjection> loginUserWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullName;
            LoginUserWorkProfiles = loginUserWorkProfiles;
        }
    }
}