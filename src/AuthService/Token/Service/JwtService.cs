using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Linq;
using apitienda.Data;
using apitienda.Models;
using Microsoft.IdentityModel.Tokens;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _context;

    private readonly IHttpContextAccessor _ihttpContextAccessor;
    private readonly ILogger<JwtService> _logger;

    public int AccessTokenExpiryMinutes =>
        int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);

    public int RefreshTokenExpiryDays =>
        int.Parse(_configuration["Jwt:RefreshTokenDays"]!);


    public JwtService(IHttpContextAccessor contextAccessor, IConfiguration configuration, AuthDbContext context, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _context = context;
        _ihttpContextAccessor = contextAccessor;
        _logger = logger;
    }

    // Creamos el token firmando con la llave privada RS256
    private string CreateToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(_configuration["Jwt:PrivateKeyPath"]!));
        var creds = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        try
        {
            var userEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "Desconocido";
            var userId = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "N/A";

            var log = new Auditoria
            {
                Usuario = userEmail,
                Accion = "GENERACION_JWT",
                Tabla = "Seguridad/Auth",
                RegistroId = userId,
                Detalle = $"Acceso concedido. El token expira el: {expires:yyyy-MM-dd HH:mm:ss} UTC.",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoría de token para {User}", claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
        }

        return tokenString;
    }

    // Generamos el refresh token
    public string GenerateRefreshToken(Guid userId)
    {
        var jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("type", "refresh")
        };

        return CreateToken(
            claims,
            DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenDays"]!)
            )
        );
    }

    public string GenerateAccessToken(Guid userId, string email)
    {
        var jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Email, email)
        };

        return CreateToken(
            claims,
            DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:AccessTokenMinutes"]!)
            )
        );
    }

    // Validamos el token con la llave pública RS256
    public ClaimsPrincipal ValidateToken(string token, bool validateLifetime = true)
    {
        var handler = new JwtSecurityTokenHandler();

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(_configuration["Jwt:PublicKeyPath"]!));

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? throw new SecurityTokenException("Token sin jti");

        if (_context.token_blacklist.Any(t => t.jti == jti))
            throw new SecurityTokenException("Token revocado");

        return principal;
    }

    public async Task RevokeTokenAsync(string jti, Guid userId, DateTimeOffset expiresAt, string reason)
    {
        _context.token_blacklist.Add(new token_blacklist
        {
            jti = jti,
            user_id = userId,
            revoked_at = DateTimeOffset.UtcNow,
            expires_at = expiresAt,
            reason = reason
        });

        await _context.SaveChangesAsync();
    }
}
