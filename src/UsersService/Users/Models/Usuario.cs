using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apitienda.Models
{
/// <summary >
/// Representa la entidad de usuario en el sistema.
/// Contiene información personal , de autenticación , estado y configuración
/// relacionada con el usuario.
/// </summary >
public class Usuario
{
/// <summary >
/// Obtiene o establece el identificador único del usuario.
/// </summary >
public Guid id { get; set; }

/// <summary >
/// Obtiene o establece la contraseña del usuario.
/// </summary >
public required string password { get; set; }

/// <summary >
/// Obtiene o establece la fecha y hora del último inicio de sesión del
/// usuario.
/// </summary >
public DateTimeOffset? last_login { get; set; }

/// <summary >
/// Obtiene o establece si el usuario es un superusuario (administrador).
/// </summary >
public bool is_superuser { get; set; } = false;

/// <summary >
/// Obtiene o establece el primer nombre del usuario.
/// </summary >
public string first_name { get; set; } = string.Empty;

/// <summary >
/// Obtiene o establece el apellido del usuario.
/// </summary >
public string last_name { get; set; } = string.Empty;

/// <summary >
/// Obtiene o establece si el usuario está activo.
/// </summary >
public bool is_active { get; set; } = true;

/// <summary >
/// Obtiene o establece la fecha y hora en que el usuario se unió al
/// sistema.
/// </summary >
public DateTimeOffset date_joined { get; set; } = DateTimeOffset.UtcNow;

/// <summary >
/// Obtiene o establece la fecha y hora de creación de la cuenta del
/// usuario.
/// </summary >
public DateTimeOffset created_at { get; set; } = DateTimeOffset.UtcNow;

/// <summary >
/// Obtiene o establece la fecha y hora de la última modificación del
/// perfil del usuario.
/// </summary >
public DateTimeOffset modified_at { get; set; } = DateTimeOffset.UtcNow;

/// <summary >
/// Obtiene o establece si el usuario está marcado como eliminado.
/// </summary >
public bool is_deleted { get; set; } = false;

/// <summary >
/// Obtiene o establece la fecha y hora en que el usuario fue eliminado.
/// </summary >
public DateTimeOffset? deleted_at { get; set; }

/// <summary >
/// Obtiene o establece el nombre de usuario del usuario.
/// </summary >
public string username { get; set; } = string.Empty;

/// <summary >
/// Obtiene o establece la dirección de correo electrónico del usuario.
/// </summary >
public string? email { get; set; }

/// <summary >
/// Obtiene o establece la URL de la foto de perfil del usuario.
/// </summary >
public string? profile_picture { get; set; }

/// <summary >
/// Obtiene o establece si el correo electrónico del usuario ha sido
/// verificado.
/// </summary >
public bool email_verified { get; set; } = false;

/// <summary >
/// Obtiene o establece la fecha y hora de la verificación del correo
/// electrónico del usuario.
/// </summary >
public DateTimeOffset? email_verified_at { get; set; }

/// <summary >
/// Obtiene o establece la nacionalidad del usuario.
/// </summary >
public string? nationality { get; set; }

/// <summary >
/// Obtiene o establece la ocupación del usuario.
/// </summary >
public string? occupation { get; set; }

/// <summary >
/// Obtiene o establece la fecha de nacimiento del usuario.
/// </summary >/// <summary >
/// Obtiene o establece el nombre de usuario del usuario.
/// </summary >
public DateTime? date_of_birth { get; set; }

/// <summary >
/// Obtiene o establece el número de teléfono de contacto del usuario.
/// </summary >
public string? contact_phone_number { get; set; }

/// <summary >
/// Obtiene o establece el género del usuario.
/// </summary >
public string? gender { get; set; }

/// <summary >
/// Obtiene o establece la dirección del usuario.
/// </summary >
public string? address { get; set; }

/// <summary >
/// Obtiene o establece el número de la dirección del usuario.
/// </summary >
public string? address_number { get; set; }

/// <summary >
/// Obtiene o establece el número interior de la dirección del usuario.
/// </summary >
public string? address_interior_number { get; set; }

/// <summary >
/// Obtiene o establece el complemento de la dirección del usuario.
/// </summary >
public string? address_complement { get; set; }

/// <summary >
/// Obtiene o establece el barrio o colonia del usuario.
/// </summary >
public string? address_neighborhood { get; set; }

/// <summary >
/// Obtiene o establece el código postal de la dirección del usuario.
/// </summary >
public string? address_zip_code { get; set; }

/// <summary >
/// Obtiene o establece la ciudad de la dirección del usuario.
/// </summary >
public string? address_city { get; set; }

/// <summary >
/// Obtiene o establece el estado de la dirección del usuario.
/// </summary >
public string? address_state { get; set; }

/// <summary >
/// Obtiene o establece el rol del usuario (por ejemplo , administrador ,
/// usuario regular).
/// </summary >
public string? role { get; set; }

/// <summary >
/// Obtiene o establece el token de restablecimiento de contraseña del
/// usuario.
/// </summary >
public string? password_reset_token { get; set; }

/// <summary >
/// Obtiene o establece la fecha y hora de expiración del token de
/// restablecimiento de contraseña del usuario.
/// </summary >
public DateTimeOffset? password_reset_token_expiration { get; set; }
}
}
