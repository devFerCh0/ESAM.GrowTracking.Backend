namespace ESAM.GrowTracking.Application.Commons.DTOs
{
    public record AccessTokenDTO
    {
        public string AccessToken { get; init; }

        public int AccessTokenExpiresIn { get; init; }

        public DateTime AccessTokenExpiresAt { get; init; }

        public AccessTokenDTO(string accessToken, int accessTokenExpiresIn, DateTime accessTokenExpiresAt)
        {
            AccessToken = accessToken;
            AccessTokenExpiresIn = accessTokenExpiresIn;
            AccessTokenExpiresAt = accessTokenExpiresAt;
        }
    }
}