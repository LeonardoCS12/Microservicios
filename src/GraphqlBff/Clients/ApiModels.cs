namespace GraphqlBff.Clients;

// Modelos para leer las respuestas JSON del microservicio de Productos.
// Reflejan el formato que ya devuelve la API REST: { codigo, mensaje, resultado }.

public class ApiResponse<T>
{
    public int Codigo { get; set; }
    public string? Mensaje { get; set; }
    public T? Resultado { get; set; }
}

public class ProductsPage
{
    public int Total { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Status { get; set; }
}

/// <summary>
/// Subconjunto de campos del usuario que el BFF necesita. Se omiten a proposito
/// password y demas datos sensibles: lo que no se lee, no se puede filtrar por GraphQL.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string? Email { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
}
