/// <summary >
/// Representa los datos del usuario transferidos entre capas de la aplicación.
/// Se utiliza para mandar respuesta larga heredando de la UsuarioDTOResponce
/// </summary >

public class UsuarioDTOResponceExtends : UsuarioDTOResponce
{
public string first_name { get; set; }
public string last_name { get; set; }
public bool is_active { get; set; }
public string? role { get; set; }
/// <summary >
/// Constructor que inicializa la información extendida del usuario
/// a partir de un <see cref="UsuarioDTO"/> y su ID.
/// </summary >
/// <param name="id">Identificador único del usuario.</param >
/// <param name="usuario">Objeto <see cref="UsuarioDTO"/> con los datos del
/// usuario.</param >

public UsuarioDTOResponceExtends(Guid id, UsuarioDTO usuario) : base(id,
usuario)
{
first_name = usuario.first_name;
last_name = usuario.last_name;
is_active = usuario.is_active;
role = usuario.role;
}
}
