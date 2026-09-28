using apitienda.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto)
    {
        var tokens = await _authService.LoginAsync(dto.Email, dto.Password);
        if (tokens.Codigo == 200)
            return Ok(tokens);

        return new UnauthorizedObjectResult(new ApiResponse<string>(401, tokens.Mensaje));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest dto)
    {
        var tokens = await _authService.RefreshAsync(dto.RefreshToken);
        if (tokens.Codigo == 200)
            return Ok(tokens);

        return new NotFoundObjectResult(new ApiResponse<string>(404, tokens.Mensaje));
    }
}
