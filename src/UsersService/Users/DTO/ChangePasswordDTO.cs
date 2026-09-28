using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary >
/// DTO utilizado para cambiar la contraseña de un usuario.
/// Contiene la contraseña actual y la nueva contraseña que se desea establecer.
/// </summary >
public class ChangePasswordDTO
{

/// <summary >
/// Contraseña actual del usuario.
/// Se utiliza para verificar que el usuario es quien solicita el cambio.
/// </summary >
public string CurrentPassword { get; set; } = string.Empty;

/// <summary >
/// Nueva contraseña que el usuario desea establecer.
/// </summary >
public string NewPassword { get; set; } = string.Empty;

/// <summary >
/// Confirmación de la nueva contraseña.
/// Debe coincidir con la propiedad NewPassword para validar el cambio.
/// </summary >
public string ConfirmPassword { get; set; } = string.Empty;
}
