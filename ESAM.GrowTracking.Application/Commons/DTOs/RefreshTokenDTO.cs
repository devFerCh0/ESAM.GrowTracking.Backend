namespace ESAM.GrowTracking.Application.Commons.DTOs
{
    public record RefreshTokenDTO
    {
        public string TokenIdentifier { get; init; }

        public string RefreshTokenPlain { get; init; }

        public int RefreshTokenExpiresIn { get; init; }

        public DateTime RefreshTokenExpiresAt { get; init; }

        public RefreshTokenDTO(string tokenIdentifier, string refreshTokenPlain, int refreshTokenExpiresIn, DateTime refreshTokenExpiresAt)
        {
            TokenIdentifier = tokenIdentifier;
            RefreshTokenPlain = refreshTokenPlain;
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
        }
    }
}