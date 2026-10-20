using System.Text.Json.Serialization;

namespace GraphqlBff.Clients;

// Moldes para leer las respuestas JSON de los microservicios.
// Reflejan el formato que ya devuelven las APIs REST: { codigo, mensaje, resultado }.

public class ApiResponse<T>
{
    public int Codigo { get; set; }
    public string? Mensaje { get; set; }
    public T? Resultado { get; set; }
}

/// <summary>GET /api/products devuelve { total, products: [...] }.</summary>
public class ProductsPage
{
    public int Total { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}

/// <summary>GET /api/users devuelve { total, users: [...] }.</summary>
public class UsersPage
{
    public int Total { get; set; }
    public List<UserDto> Users { get; set; } = new();
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Status { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("product_key")]
    public string? ProductKey { get; set; }

    [JsonPropertyName("image_link")]
    public string? ImageLink { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Subconjunto de campos del usuario que el BFF necesita. Se omiten a proposito
/// password y demas datos sensibles: lo que no se lee, no se puede filtrar por GraphQL.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
}
