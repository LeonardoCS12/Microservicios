using System.Net.Http.Json;
using System.Security.Claims;
using apitienda.DTOs;
using System.IdentityModel.Tokens.Jwt;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(IJwtService jwtService, IHttpClientFactory httpClientFactory)
    {
        _jwtService = jwtService;
        _httpClientFactory = httpClientFactory;
    }

    // =========================
    // LOGIN
    // =========================
    public async Task<ApiResponse<TokenResponse>> LoginAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("UsersService");

        var response = await client.PostAsJsonAsync("/internal/users/validate-credentials", new LoginRequest(email, password));

        if (!response.IsSuccessStatusCode)
            return new ApiResponse<TokenResponse>(401, "Credenciales inválidas");

        var user = await response.Content.ReadFromJsonAsync<ValidatedUserResponse>();

        if (user is null)
            return new ApiResponse<TokenResponse>(401, "Credenciales inválidas");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        return new ApiResponse<TokenResponse>(200, "Usuario encontrado", BuildResponse(accessToken, refreshToken));
    }

    // =========================
    // REFRESH TOKEN
    // =========================
    public async Task<ApiResponse<TokenResponse>> RefreshAsync(string refreshToken)
    {
        var principal = _jwtService.ValidateToken(refreshToken);

        if (principal.FindFirst("type")?.Value != "refresh")
            return new ApiResponse<TokenResponse>(404, "No es refresh token");

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)!.Value;
        var userId = Guid.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var exp = principal.FindFirst(JwtRegisteredClaimNames.Exp)!.Value;

        await _jwtService.RevokeTokenAsync(
            jti,
            userId,
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)),
            "refresh"
        );

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? "";

        var newAccess = _jwtService.GenerateAccessToken(userId, email);
        var newRefresh = _jwtService.GenerateRefreshToken(userId);

        return new ApiResponse<TokenResponse>(200, "Token refresh generado", BuildResponse(newAccess, newRefresh));
    }

    private TokenResponse BuildResponse(string accessToken, string refreshToken)
    {
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresInMinutes = _jwtService.AccessTokenExpiryMinutes,
            RefreshTokenExpiresInDays = _jwtService.RefreshTokenExpiryDays
        };
    }
}

public record LoginRequest(string Email, string Password);
public record ValidatedUserResponse(Guid Id, string Email);
