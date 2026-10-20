using GraphqlBff.Clients;
using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Las cinco queries del contrato oficial: usuarios, usuario(id), productos, producto(id) y panelGeneral.
/// Los resolvers no acceden a ninguna base de datos: llaman a los microservicios REST, que no se modifican.
/// </summary>
public class Query
{
    /// <summary>GET /api/users.</summary>
    public async Task<IReadOnlyList<Usuario>> GetUsuariosAsync(
        [Service] IUsersClient usersClient,
        CancellationToken cancellationToken)
    {
        var usuarios = await usersClient.GetAllAsync(cancellationToken);
        return usuarios.Select(u => u.ToGraphQL()).ToList();
    }

    /// <summary>GET /api/users/{id}. Devuelve null si no existe.</summary>
    public async Task<Usuario?> GetUsuarioAsync(
        [GraphQLType(typeof(NonNullType<IdType>))] string id,
        [Service] IUsersClient usersClient,
        CancellationToken cancellationToken)
    {
        var usuario = await usersClient.GetByIdAsync(Ids.Parse(id, "usuario"), cancellationToken);
        return usuario?.ToGraphQL();
    }

    /// <summary>GET /api/products.</summary>
    public async Task<IReadOnlyList<Producto>> GetProductosAsync(
        [Service] IProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var productos = await productsClient.GetAllAsync(cancellationToken);
        return productos.Select(p => p.ToGraphQL()).ToList();
    }

    /// <summary>GET /api/products/{id}. Devuelve null si no existe.</summary>
    public async Task<Producto?> GetProductoAsync(
        [GraphQLType(typeof(NonNullType<IdType>))] string id,
        [Service] IProductsClient productsClient,
        CancellationToken cancellationToken)
    {
        var producto = await productsClient.GetByIdAsync(Ids.Parse(id, "producto"), cancellationToken);
        return producto?.ToGraphQL();
    }

    /// <summary>
    /// Composicion de dos microservicios en una sola respuesta: usuarios (Usuarios) y productos (Productos),
    /// sin relacion entre ellos. Los campos los resuelve PanelGeneral.
    /// </summary>
    public PanelGeneral GetPanelGeneral() => new();
}
