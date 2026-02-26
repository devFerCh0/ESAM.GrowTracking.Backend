namespace ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels
{
    public record LoginReadModel
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public LoginUserReadModel LoginUser { get; init; }

        public LoginReadModel(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, LoginUserReadModel loginUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            LoginUser = loginUser;
        }
    }
}