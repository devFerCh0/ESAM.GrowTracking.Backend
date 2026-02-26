namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses
{
    public record LoginAssumedWorkProfileResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public LoginAssumedWorkProfileUserResponse LoginAssumedWorkProfileUser { get; init; }

        public LoginAssumedWorkProfileResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string refreshTokenRaw, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, 
            LoginAssumedWorkProfileUserResponse loginAssumedWorkProfileUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = refreshTokenRaw;
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            LoginAssumedWorkProfileUser = loginAssumedWorkProfileUser;
        }
    }
}