using apitienda.DTOs;

public interface IAuthService
{
    Task<ApiResponse<TokenResponse>> LoginAsync(string email, string password);
    Task<ApiResponse<TokenResponse>> RefreshAsync(string refreshToken);
}
