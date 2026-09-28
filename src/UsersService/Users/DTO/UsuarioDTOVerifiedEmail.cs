using apitienda.Models;

/// <summary >
/// DTO utilizado para transferir información relacionada con la verificación de
/// correo electrónico de un usuario.
/// Contiene el ID, correo electrónico , estado de verificación y fecha de
/// verificación.
/// </summary >

public class UsuarioDTOVerifiedEmail
{
/// <summary >
/// Identificador único del usuario.
/// </summary >
public Guid Id { get; set; }
/// <summary >
/// Correo electrónico del usuario. Opcional.
/// </summary >
public string? Email { get; set; }
/// <summary >
/// Indica si el correo electrónico del usuario ha sido verificado.
/// Por defecto es false.
/// </summary >
public bool Email_verified { get; set; } = false;
/// <summary >
/// Fecha y hora en que se verificó el correo electrónico. Opcional.
/// </summary >
public DateTimeOffset? Email_verified_at { get; set; }

/// <summary >
/// Constructor que inicializa el DTO a partir de un objeto <see cref="Usuario
/// "/>.
/// </summary >
/// <param name="id">Identificador único del usuario.</param >
/// <param name="usuarioVerificadoEmail">Objeto <see cref="Usuario"/> con la
/// información de verificación de correo electrónico.</param >

public UsuarioDTOVerifiedEmail(Guid id, Usuario usuarioVerificadoEmail)
{
Id = id;
Email = usuarioVerificadoEmail.email;
Email_verified = usuarioVerificadoEmail.email_verified;
Email_verified_at = usuarioVerificadoEmail.email_verified_at;
}


}
