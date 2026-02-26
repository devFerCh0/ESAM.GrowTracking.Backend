namespace ESAM.GrowTracking.API.Controllers.Auth.Login.Responses
{
    public record LoginResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public LoginUserResponse LoginUser { get; init; }

        public LoginResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, LoginUserResponse loginUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            LoginUser = loginUser;
        }
    }
}