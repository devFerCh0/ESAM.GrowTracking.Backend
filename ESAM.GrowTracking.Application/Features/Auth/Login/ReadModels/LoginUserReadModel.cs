namespace ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels
{
    public record LoginUserReadModel
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public List<LoginUserWorkProfileReadModel> LoginUserWorkProfiles { get; init; }

        public LoginUserReadModel(int userId, string username, string email, string fullName, List<LoginUserWorkProfileReadModel> loginUserWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullName;
            LoginUserWorkProfiles = loginUserWorkProfiles;
        }
    }
}