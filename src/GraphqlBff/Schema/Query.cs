using GraphqlBff.Clients;

namespace GraphqlBff.Schema;

/// <summary>
/// Raiz de las consultas GraphQL. Los resolvers no acceden a ninguna base de datos:
/// llaman a los microservicios REST existentes.
/// </summary>
public class Query
{
    /// <summary>Comprobacion rapida de que el BFF esta vivo.</summary>
    public string Estado() => "GraphQL BFF operativo";

    /// <summary>Lista los productos llamando a GET /api/products del microservicio de Productos.</summary>
    public async Task<IReadOnlyList<Producto>> GetProductosAsync(
        [Service] ProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var productos = await productsClient.GetAllAsync(cancellationToken);

        return productos
            .Select(p => new Producto
            {
                Id = p.Id.ToString(),
                Nombre = p.Name,
                Precio = (double)p.Price,
                Tipo = p.Type
            })
            .ToList();
    }
}
