using GraphqlBff.Clients;

namespace GraphqlBff.Schema;

/// <summary>
/// Traduce los DTO de las APIs REST a los tipos del schema GraphQL
/// (por ejemplo: text -> description, first_name -> firstName).
/// </summary>
public static class Mapping
{
    public static Producto ToGraphQL(this ProductDto p) => new()
    {
        Id = p.Id.ToString(),
        Name = p.Name,
        Price = (double)p.Price,
        Status = p.Status,
        Description = p.Text
    };

    public static Usuario ToGraphQL(this UserDto u) => new()
    {
        Id = u.Id.ToString(),
        Username = u.Username,
        Email = u.Email ?? string.Empty,
        FirstName = u.FirstName,
        LastName = u.LastName
    };
}
