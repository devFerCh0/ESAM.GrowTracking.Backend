namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels
{
    public record LoginAssumedRoleReadModel
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public LoginAssumedRoleUserReadModel LoginAssumedRoleUser { get; init; }

        public LoginAssumedRoleReadModel(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string tokenIdentifier, string refreshTokenPlain, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, 
            LoginAssumedRoleUserReadModel loginAssumedRoleUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = $"{tokenIdentifier}.{refreshTokenPlain}";
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            LoginAssumedRoleUser = loginAssumedRoleUser;
        }
    }
}