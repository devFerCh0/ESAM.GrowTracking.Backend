namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels
{
    public record LoginAssumedWorkProfileReadModel
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public LoginAssumedWorkProfileUserReadModel LoginAssumedWorkProfileUser { get; init; }

        public LoginAssumedWorkProfileReadModel(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string tokenIdentifier, string refreshTokenPlain, int refreshTokenExpiresIn, 
            DateTime refreshTokenExpiresAt, LoginAssumedWorkProfileUserReadModel loginAssumedWorkProfileUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{tokenIdentifier}.{refreshTokenPlain}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            LoginAssumedWorkProfileUser = loginAssumedWorkProfileUser;
        }
    }
}