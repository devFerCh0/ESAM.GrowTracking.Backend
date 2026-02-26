namespace ESAM.GrowTracking.API.Controllers.Auth.Login.Responses
{
    public record LoginUserResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public List<LoginUserWorkProfileResponse> LoginUserWorkProfiles { get; init; }

        public LoginUserResponse(int userId, string username, string email, string fullName, List<LoginUserWorkProfileResponse> loginUserWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullName;
            LoginUserWorkProfiles = loginUserWorkProfiles;
        }
    }
}