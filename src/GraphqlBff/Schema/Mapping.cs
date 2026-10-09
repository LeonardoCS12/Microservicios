using GraphqlBff.Clients;

namespace GraphqlBff.Schema;

/// <summary>
/// Traduce los DTO de la API REST a los tipos del schema GraphQL
/// (por ejemplo: name -> nombre, price -> precio, first_name + last_name -> nombre).
/// </summary>
public static class Mapping
{
    public static Producto ToGraphQL(this ProductDto p) => new()
    {
        Id = p.Id.ToString(),
        Nombre = p.Name,
        Precio = (double)p.Price,
        Tipo = p.Type
    };

    public static Usuario ToGraphQL(this UserDto u) => new()
    {
        Id = u.Id.ToString(),
        Nombre = $"{u.FirstName} {u.LastName}".Trim(),
        Email = u.Email ?? string.Empty
    };
}
