using GraphqlBff.Clients;
using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Tipo GraphQL "Usuario". Los campos id, nombre y email vienen del microservicio de Usuarios.
/// El campo "productos" NO existe en ese microservicio: lo compone el BFF llamando a Productos.
/// </summary>
public class Usuario
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Resolver del campo "productos". GraphQL solo lo ejecuta si el cliente lo pide:
    /// si la query no incluye "productos", no se hace la llamada a Productos.
    /// </summary>
    public async Task<IReadOnlyList<Producto>> GetProductosAsync(
        [Service] ProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var productos = await productsClient.GetByUserAsync(Guid.Parse(Id), cancellationToken);
        return productos.Select(p => p.ToGraphQL()).ToList();
    }
}
