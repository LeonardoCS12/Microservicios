using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

/// <summary >
/// Objeto de Transferencia de Datos (DTO) para la entidad <see cref="Usuario"/>.
/// Se utiliza para la transferencia de datos entre la capa de presentación y la
/// lógica de negocio.
/// </summary >
public class UsuarioDTO
{
public Guid id { get; set; }

[Required]
public required string username { get; set; }
public string? email { get; set; }
public DateTimeOffset date_joined { get; set; }
[Required]
public required string first_name { get; set; }
[Required]
public required string last_name { get; set; }
public string? nationality { get; set; }
[Required]
public required string password { get; set; }
public DateTimeOffset? last_login { get; set; }
public bool is_superuser { get; set; }
public bool is_active { get; set; }
public DateTimeOffset created_at { get; set; }
public DateTimeOffset modified_at { get; set; }
public bool is_deleted { get; set; }
public DateTimeOffset? deleted_at { get; set; }
public string? profile_picture { get; set; }
public bool email_verified { get; set; }
public DateTimeOffset? email_verified_at { get; set; }
public string? occupation { get; set; }
public DateTime? date_of_birth { get; set; }
public string? contact_phone_number { get; set; }
public string? gender { get; set; }
public string? address { get; set; }
public string? address_number { get; set; }
public string? address_interior_number { get; set; }
public string? address_complement { get; set; }
public string? address_neighboorhood { get; set; }
public string? address_zip_code { get; set; }
public string? address_city { get; set; }
public string? address_state { get; set; }
public string? role { get; set; }
}
