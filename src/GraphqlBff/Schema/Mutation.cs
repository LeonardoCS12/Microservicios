using GraphqlBff.Clients;

namespace GraphqlBff.Schema;

/// <summary>
/// Raiz de las mutations GraphQL (operaciones de escritura). Igual que Query, solo delega en los microservicios.
/// </summary>
public class Mutation
{
    /// <summary>
    /// Crea un producto llamando a POST /api/products. "tipo" es obligatorio en la API REST (max. 10 caracteres)
    /// pero no esta en el schema del PDF, asi que aqui es un argumento opcional con valor por defecto.
    /// </summary>
    public async Task<Producto> CrearProductoAsync(
        string nombre,
        double precio,
        [Service] ProductsClient productsClient,
        CancellationToken cancellationToken,
        string tipo = "general")
    {
        var creado = await productsClient.CreateAsync(nombre, tipo, (decimal)precio, cancellationToken);

        return new Producto
        {
            Id = creado.Id.ToString(),
            Nombre = creado.Name,
            Precio = (double)creado.Price,
            Tipo = creado.Type
        };
    }
}
