using HotChocolate;

namespace GraphqlBff.Schema;

// Tipos de entrada (input) de las mutations del contrato oficial.
// Ademas de los campos del SDL de referencia se incluye type, que la API de Productos de la practica 8
// requiere (el PDF pide incorporar al tipo de entrada comun los campos que el REST exige).
// type es opcional, para que las consultas del PDF sigan siendo validas.

[GraphQLName("CrearProductoInput")]
public class CrearProductoInput
{
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool Status { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
}

[GraphQLName("ActualizarProductoInput")]
public class ActualizarProductoInput
{
    public string? Name { get; set; }
    public double? Price { get; set; }
    public bool? Status { get; set; }
    public string? Description { get; set; }
}

[GraphQLName("CrearUsuarioInput")]
public class CrearUsuarioInput
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
