namespace GraphqlBff.Clients;

/// <summary>
/// Contrato del cliente hacia el microservicio de Usuarios.
/// Los resolvers dependen de esta interfaz, no de la clase concreta, para poder
/// sustituir el cliente HTTP real por un doble (Moq) en las pruebas.
/// </summary>
public interface IUsersClient
{
    /// <summary>GET api/users (sin usuarios eliminados).</summary>
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>GET api/users/{id}. Devuelve null si no existe.</summary>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>POST api/users. Devuelve el usuario creado.</summary>
    Task<UserDto> CreateAsync(UserCreate data, CancellationToken cancellationToken);
}
