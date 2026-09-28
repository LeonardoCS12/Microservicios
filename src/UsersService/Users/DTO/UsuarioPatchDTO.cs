/// <summary >
/// Objeto de Transferencia de Datos (DTO) para la entidad <see cref="Usuario"/>.
/// Se utiliza para la transferencia de datos entre la capa de presentación y la
/// lógica de negocio.
/// Estos campos son especificamente para el metodo Patch , donde se muestran solo
/// los campos a modificar
/// </summary >
public class UsuarioPatchDTO
{
public string? first_name { get; set; }
public string? last_name { get; set; }
public string? email { get; set; }
// Se pueden agregar otros campos opcionales
}
