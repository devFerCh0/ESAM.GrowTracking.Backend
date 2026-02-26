namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public record RefreshReadModel
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public RefreshReadModel(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string tokenIdentifier, string refreshTokenPlain, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{tokenIdentifier}.{refreshTokenPlain}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
        }
    }
}