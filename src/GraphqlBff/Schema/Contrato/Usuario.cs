using HotChocolate;
using HotChocolate.Types;

namespace GraphqlBff.Schema;

/// <summary>
/// Tipo GraphQL "Usuario" del contrato oficial: id, username, email, firstName, lastName.
/// No incluye password: nunca sale por GraphQL.
/// </summary>
public class Usuario
{
    [GraphQLType(typeof(NonNullType<IdType>))]
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
