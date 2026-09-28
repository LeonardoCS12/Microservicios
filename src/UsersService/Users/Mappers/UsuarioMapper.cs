using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apitienda.Models;

public class UsuarioMapper
{
/// <summary >
/// Convierte una entidad de tipo <see cref="Usuario"/> en un objeto DTO (<see
/// cref="UsuarioDTO"/>).
/// </summary >
/// <param name="usuario">La entidad de usuario que se desea convertir.</param >
/// <returns >Un objeto <see cref="UsuarioDTO"/> con los valores asignados desde
/// la entidad.</returns >
public UsuarioDTO ToDTO(Usuario usuario)
{
// Mapea los valores de la entidad Usuario a un DTO.
return new UsuarioDTO
{
id = usuario.id,
username = usuario.username ,
email = usuario.email ,
date_joined = usuario.date_joined ,
first_name = usuario.first_name ,
last_name = usuario.last_name ,
nationality = usuario.nationality ,
password = usuario.password ,
last_login = usuario.last_login ,
is_superuser = usuario.is_superuser ,
is_active = usuario.is_active ,
created_at = usuario.created_at ,
modified_at = usuario.modified_at ,
is_deleted = usuario.is_deleted ,
deleted_at = usuario.deleted_at ,
profile_picture = usuario.profile_picture ,
email_verified = usuario.email_verified ,
email_verified_at = usuario.email_verified_at ,
occupation = usuario.occupation ,
date_of_birth = usuario.date_of_birth ,
contact_phone_number = usuario.contact_phone_number ,
gender = usuario.gender ,
address = usuario.address ,
address_number = usuario.address_number ,
address_interior_number = usuario.address_interior_number ,
address_complement = usuario.address_complement ,
address_neighboorhood = usuario.address_neighborhood ,
address_zip_code = usuario.address_zip_code ,
address_city = usuario.address_city ,
address_state = usuario.address_state ,
role = usuario.role
};
}


/// <summary >
/// Convierte un UsuarioDTO en una entidad Usuario.
/// </summary >
/// <param name="usuarioDTO">Datos del usuario en formato DTO.</param >
/// <returns >Entidad Usuario con los valores asignados.</returns >
public Usuario ToEntity(UsuarioDTO usuarioDTO)
{
// Mapea los valores del DTO a la entidad Usuario.
return new Usuario
{
id = usuarioDTO.id,
username = usuarioDTO.username ,
email = usuarioDTO.email ,
date_joined = usuarioDTO.date_joined ,
first_name = usuarioDTO.first_name ,
last_name = usuarioDTO.last_name ,
nationality = usuarioDTO.nationality ,
password = usuarioDTO.password ,
last_login = usuarioDTO.last_login ,
is_superuser = usuarioDTO.is_superuser ,
is_active = usuarioDTO.is_active ,
created_at = usuarioDTO.created_at ,
modified_at = usuarioDTO.modified_at ,
is_deleted = usuarioDTO.is_deleted ,
deleted_at = usuarioDTO.deleted_at ,
profile_picture = usuarioDTO.profile_picture ,
email_verified = usuarioDTO.email_verified ,
email_verified_at = usuarioDTO.email_verified_at ,
occupation = usuarioDTO.occupation ,
date_of_birth = usuarioDTO.date_of_birth ,
contact_phone_number = usuarioDTO.contact_phone_number ,
gender = usuarioDTO.gender ,
address = usuarioDTO.address ,
address_number = usuarioDTO.address_number ,
address_interior_number = usuarioDTO.address_interior_number ,
address_complement = usuarioDTO.address_complement ,
address_neighborhood = usuarioDTO.address_neighboorhood ,
address_zip_code = usuarioDTO.address_zip_code ,
address_city = usuarioDTO.address_city ,
address_state = usuarioDTO.address_state ,
role = usuarioDTO.role
};
}
}
