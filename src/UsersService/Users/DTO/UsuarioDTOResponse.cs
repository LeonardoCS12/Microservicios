using apitienda.Models;

/// <summary >
/// Representa los datos del usuario transferidos entre capas de la aplicación.
/// Se utiliza para mandar respuesta corta.
/// </summary >

public class UsuarioDTOResponce
{
public Guid Id { get; set; }
public string username { get; set; }
public string? email { get; set; }
/// <summary >
/// Constructor que inicializa la respuesta del usuario a partir de un <see
/// cref="UsuarioDTO"/>.
/// </summary >
/// <param name="id">Identificador único del usuario.</param >
/// <param name="usuario">Objeto <see cref="UsuarioDTO"/> con los datos del
/// usuario.</param >
public UsuarioDTOResponce(Guid id,UsuarioDTO usuario)
{
Id = id;
username = usuario.username;
email = usuario.email;
}

}
