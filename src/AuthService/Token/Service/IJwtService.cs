using System.Security.Claims;

public interface IJwtService
{
    // Generamos los tokens
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken(Guid userId);

    // Duración de los tokens validos
    int AccessTokenExpiryMinutes { get; }
    int RefreshTokenExpiryDays { get; }

    // Verifica si el token es auténtico
    ClaimsPrincipal ValidateToken(string token, bool validateLifetime = true);

    // Remueve o mete al token en la lista negra para no ejercer su uso
    Task RevokeTokenAsync(string jti, Guid userId, DateTimeOffset expiresAt, string reason);
}
