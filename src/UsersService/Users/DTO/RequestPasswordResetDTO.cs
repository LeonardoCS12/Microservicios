using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary >
/// DTO utilizado para solicitar el restablecimiento de la contraseña de un usuario
/// .
/// Contiene el correo electrónico asociado a la cuenta que desea recuperar.
/// </summary >
public class RequestPasswordResetDTO
{
/// <summary >
/// Correo electrónico del usuario que solicita el restablecimiento de
/// contraseña.
/// Este correo se utiliza para enviar el enlace o token de recuperación.
/// </summary >
public string Email { get; set; } = string.Empty;
}
