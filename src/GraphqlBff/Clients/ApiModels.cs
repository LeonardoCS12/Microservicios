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
