namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public record RefreshResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public RefreshResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string refreshTokenRaw, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = refreshTokenRaw;
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
        }
    }
}