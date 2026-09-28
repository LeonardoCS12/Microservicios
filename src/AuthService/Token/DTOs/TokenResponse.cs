namespace apitienda.DTOs
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int AccessTokenExpiresInMinutes { get; set; }
        public int RefreshTokenExpiresInDays { get; set; }
    }
}
