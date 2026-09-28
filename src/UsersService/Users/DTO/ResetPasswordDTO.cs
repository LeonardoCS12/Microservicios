/// <summary >
/// DTO utilizado para restablecer la contraseña de un usuario.
/// Contiene el token de recuperación y la nueva contraseña que se desea establecer
/// .
/// </summary >
public class ResetPasswordDTO
{

/// <summary >
/// Token único de recuperación de contraseña enviado al correo del usuario.
/// Se utiliza para validar la autenticidad de la solicitud de restablecimiento
/// .
/// </summary >
public required string Token { get; set; }
/// <summary >
/// Nueva contraseña que el usuario desea establecer.
/// Debe cumplir con las políticas de seguridad definidas en la aplicación.
/// </summary >
public required string NewPassword { get; set; }
}
