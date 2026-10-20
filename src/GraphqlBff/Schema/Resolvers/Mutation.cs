using GraphqlBff.Clients;
using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Las tres mutations del contrato oficial: crearProducto, actualizarProducto y crearUsuario.
/// Cada una delega en el microservicio correspondiente; las validaciones y reglas de negocio
/// las aplica el microservicio, y sus errores se traducen a errores GraphQL.
/// </summary>
public class Mutation
{
    // Tipo por defecto cuando el cliente no envia "type" (la API de Productos maneja ese dato, el SDL de referencia no).
    private const string TipoPorDefecto = "general";

    /// <summary>
    /// POST /api/products. La API de Productos siempre crea el producto activo y no recibe "status":
    /// si se pide status = false, el BFF lo ajusta despues con un PATCH.
    /// </summary>
    public async Task<Producto> CrearProductoAsync(
        CrearProductoInput input,
        [Service] IProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var creado = await productsClient.CreateAsync(
            new ProductCreate(
                input.Type ?? TipoPorDefecto,
                input.Name,
                (decimal)input.Price,
                input.Description,
                null,    // Product_key: la API de Productos la genera sola
                null),   // image_link: opcional en la API, no forma parte del contrato GraphQL
            cancellationToken);

        if (!input.Status)
        {
            creado = await productsClient.UpdateAsync(
                creado.Id, new ProductChanges(null, null, null, false, null, null, null), cancellationToken);
        }

        return creado.ToGraphQL();
    }

    /// <summary>PATCH /api/products/{id}. Solo se envian los campos recibidos.</summary>
    public async Task<Producto> ActualizarProductoAsync(
        [GraphQLType(typeof(NonNullType<IdType>))] string id,
        ActualizarProductoInput input,
        [Service] IProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var cambios = new ProductChanges(
            null,
            input.Name,
            input.Price.HasValue ? (decimal)input.Price.Value : null,
            input.Status,
            input.Description,
            null,
            null);

        var actualizado = await productsClient.UpdateAsync(Ids.Parse(id, "producto"), cambios, cancellationToken);
        return actualizado.ToGraphQL();
    }

    /// <summary>POST /api/users. El usuario se crea activo.</summary>
    public async Task<Usuario> CrearUsuarioAsync(
        CrearUsuarioInput input,
        [Service] IUsersClient usersClient,
        CancellationToken cancellationToken)
    {
        var creado = await usersClient.CreateAsync(
            new UserCreate(input.Username, input.Password, input.Email, input.FirstName, input.LastName),
            cancellationToken);

        return creado.ToGraphQL();
    }
}
