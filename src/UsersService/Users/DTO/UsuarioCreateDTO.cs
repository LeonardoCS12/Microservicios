using System.ComponentModel.DataAnnotations;

/// <summary >
/// DTO utilizado para crear un nuevo usuario en el sistema.
/// Contiene todos los campos necesarios y opcionales para el registro de un
/// usuario.
/// </summary >
public class UsuarioCreateDTO
{

/// <summary >
/// Nombre de usuario único.
/// Obligatorio , con longitud mínima de 1 y máxima de 20 caracteres.
/// </summary >
[Required]
[StringLength(20, MinimumLength = 1)]
public required string username { get; set; }
/// <summary >
/// Contraseña del usuario.
/// Obligatoria y debe cumplir con las políticas de seguridad definidas.
/// </summary >
[Required]
public required string password { get; set; }
/// <summary >
/// Correo electrónico del usuario.
/// Obligatorio para registro y notificaciones.
/// </summary >
public required string email { get; set; }
/// <summary >
/// Primer nombre del usuario.
/// Obligatorio.
/// </summary >
[Required]
public required string first_name { get; set; }
/// <summary >
/// Apellido del usuario.
/// Obligatorio.
/// </summary >
[Required]
public required string last_name { get; set; }
/// <summary >
/// Estado activo del usuario.
/// </summary >
public required bool is_active { get; set; }
/// <summary >
/// Indica si el usuario tiene privilegios de superusuario.
/// Por defecto es false.
/// </summary >
public bool is_superuser { get; set; } = false;
/// <summary >
/// URL de la imagen de perfil del usuario.
/// </summary >
public string? profile_picture { get; set; }
/// <summary >
/// Nacionalidad del usuario.
/// </summary >
public string? nationality { get; set; }
/// <summary >
/// Ocupación o profesión del usuario.
/// </summary >
public string? occupation { get; set; }
public DateTime? date_of_birth { get; set; }
public string? contact_phone_number { get; set; }
public string? gender { get; set; }
public string? address { get; set; }
public string? address_number { get; set; }
public string? address_interior_number { get; set; }
/// <summary >
/// Complemento de la dirección (ej. torre , departamento).
/// Opcional.
/// </summary >
public string? address_complement { get; set; }
/// <summary >
/// Barrio o colonia del usuario.
/// </summary >
public string? address_neighborhood { get; set; }
public string? address_zip_code { get; set; }
public string? address_city { get; set; }
/// <summary >
/// Estado o provincia de residencia del usuario.
/// Opcional.
/// </summary >
public string? address_state { get; set; }
public string? role { get; set; }
}
