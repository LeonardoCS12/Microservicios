using System.ComponentModel.DataAnnotations;
/// <summary >
/// DTO utilizado para actualizar la información completa de un usuario.
/// Contiene todos los campos necesarios para modificar los datos de un usuario
/// existente.
/// </summary >
public class UsuarioUpdate
{
/// <summary >
/// Nombre de usuario único.
/// Longitud máxima de 20 caracteres.
/// </summary >
[StringLength(20)]
public required string username { get; set; }
public required string password { get; set; }
public required string email { get; set; }
public required string first_name { get; set; }
public required string last_name { get; set; }
public bool is_active {get; set;} = true;
public bool is_superuser {get; set;} = false;
public required string profile_picture { get; set; }
public required string nacionality { get; set; }
public required string occupation { get; set; }
public required DateTime date_of_birth { get; set; }
public required string contact_phone_number { get; set; }
public required string gender { get; set; }
public required string address { get; set; }
public required string address_number { get; set; }
public required string address_interior_number { get; set; }
public required string address_complement { get; set; }
public required string address_neighborhood { get; set; }
public required string address_zip_code { get; set; }
public required string address_city { get; set; }
public required string address_state { get; set; }
public required string role { get; set; }

}
