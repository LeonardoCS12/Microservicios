using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Tipo GraphQL "Producto". Es la forma que ve el cliente, distinta del DTO interno de la API REST
/// (por ejemplo: name -> nombre, price -> precio).
/// </summary>
public class Producto
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public double Precio { get; set; }

    public string Tipo { get; set; } = string.Empty;
}
