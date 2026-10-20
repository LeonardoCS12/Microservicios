using GraphqlBff.Clients;
using HotChocolate;

namespace GraphqlBff.Schema;

/// <summary>
/// Consulta agregada del contrato oficial. Reune en una sola respuesta la lista de usuarios
/// (microservicio de Usuarios) y la lista de productos (microservicio de Productos),
/// SIN ninguna relacion entre ellos. Cada campo es un resolver independiente: solo llama
/// a su servicio si el cliente lo pide, y GraphQL puede resolverlos en paralelo.
/// </summary>
public class PanelGeneral
{
    public async Task<IReadOnlyList<Usuario>> GetUsuariosAsync(
        [Service] IUsersClient usersClient,
        CancellationToken cancellationToken)
    {
        var usuarios = await usersClient.GetAllAsync(cancellationToken);
        return usuarios.Select(u => u.ToGraphQL()).ToList();
    }

    public async Task<IReadOnlyList<Producto>> GetProductosAsync(
        [Service] IProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var productos = await productsClient.GetAllAsync(cancellationToken);
        return productos.Select(p => p.ToGraphQL()).ToList();
    }
}
