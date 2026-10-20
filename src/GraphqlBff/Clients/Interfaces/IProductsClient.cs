namespace GraphqlBff.Clients;

/// <summary>
/// Contrato del cliente hacia el microservicio de Productos.
/// Los resolvers dependen de esta interfaz, no de la clase concreta, para poder
/// sustituir el cliente HTTP real por un doble (Moq) en las pruebas.
/// </summary>
public interface IProductsClient
{
    /// <summary>GET api/products (sin productos eliminados).</summary>
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>GET api/products/{id}. Devuelve null si no existe.</summary>
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>POST api/products. Devuelve el producto creado, con su id.</summary>
    Task<ProductDto> CreateAsync(ProductCreate data, CancellationToken cancellationToken);

    /// <summary>PATCH api/products/{id}. Devuelve el producto ya actualizado.</summary>
    Task<ProductDto> UpdateAsync(Guid id, ProductChanges changes, CancellationToken cancellationToken);
}
