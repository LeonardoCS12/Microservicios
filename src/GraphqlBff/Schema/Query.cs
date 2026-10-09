using GraphqlBff.Clients;
using HotChocolate;
using HotChocolate.Types;

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
        return productos.Select(p => p.ToGraphQL()).ToList();
    }

    /// <summary>
    /// HU-05: busca un usuario en el microservicio de Usuarios. Sus productos se resuelven
    /// aparte (ver Usuario.GetProductosAsync) y solo si el cliente los pide.
    /// </summary>
    public async Task<Usuario?> GetUsuarioAsync(
        [GraphQLType(typeof(NonNullType<IdType>))] string id,
        [Service] UsersClient usersClient,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var userId))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("El id del usuario debe ser un GUID valido.")
                .SetCode("BAD_USER_INPUT")
                .Build());
        }

        var usuario = await usersClient.GetByIdAsync(userId, cancellationToken);
        return usuario?.ToGraphQL();
    }
}
