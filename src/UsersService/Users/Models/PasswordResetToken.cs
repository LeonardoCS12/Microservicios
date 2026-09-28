using System.ComponentModel.DataAnnotations;
/// <summary >
/// Representa un token de restablecimiento de contraseña asociado a un usuario.
/// Se utiliza para validar solicitudes de cambio de contraseña de manera segura.
/// </summary >

public class PasswordResetToken
{
/// <summary >
/// Token único de restablecimiento de contraseña.
/// </summary >
[Key]
public required string Token { get; set; }
/// <summary >
/// Correo electrónico del usuario asociado al token.
/// </summary >
public required string Email { get; set; }
/// <summary >
/// Fecha y hora de expiración del token.
/// El token será inválido después de este momento.
/// </summary >
public required DateTime Expiration { get; set; }
/// <summary >
/// Identificador único del usuario asociado al token.
/// </summary >
public required Guid UserId { get; set; }
}
