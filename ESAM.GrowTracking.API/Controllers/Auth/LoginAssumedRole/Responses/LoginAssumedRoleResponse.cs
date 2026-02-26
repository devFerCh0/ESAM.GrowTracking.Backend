namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses
{
    public record LoginAssumedRoleResponse
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public string RefreshTokenRaw { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public LoginAssumedRoleUserResponse LoginAssumedRoleUser { get; init; }

        public LoginAssumedRoleResponse(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt, string refreshTokenRaw, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt, 
            LoginAssumedRoleUserResponse loginAssumedRoleUser)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenRaw = refreshTokenRaw;
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            LoginAssumedRoleUser = loginAssumedRoleUser;
        }
    }
}