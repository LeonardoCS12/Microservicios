using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Tipo GraphQL "Producto" del contrato oficial: id, name, price, status, description.
/// </summary>
public class Producto
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Price { get; set; }

    public bool Status { get; set; }

    public string? Description { get; set; }
}
