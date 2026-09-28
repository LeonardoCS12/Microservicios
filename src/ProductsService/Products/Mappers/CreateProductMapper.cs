using productos.Models;
using productos.Data;

public class CreateProductMapper
{
    /// <summary>
    /// Convierte un DTO de creación a una entidad Product.
    /// </summary>
    /// <param name="productDTO">DTO con los datos del producto a crear.</param>
    /// <returns>Entidad `Product` lista para ser almacenada en la base de datos.</returns>
    public Product ToEntity(ProductCreateDTO productDTO)
    {
        return new Product
        {
            id = Guid.NewGuid(),
            created_at = DateTimeOffset.UtcNow,
            modified_at = DateTimeOffset.UtcNow,
            is_deleted = false,
            deleted_at = null,
            status = true,
            type = productDTO.type,
            name = productDTO.name,
            price = productDTO.price,
            text = productDTO.text,
            image_lick = productDTO.image_link,

            // Si 'ProductKey' es nulo o vacío, generar una clave única de 8 caracteres a partir de un GUID.
            Product_key = string.IsNullOrEmpty(productDTO.Product_key)
                ? Guid.NewGuid().ToString("N").Substring(0, 8)
                : productDTO.Product_key
        };
    }

    /// <summary>
    /// Convierte una entidad 'Product' a un DTO de creación (`ProductCreateDTO`).
    /// </summary>
    /// <param name="product">Instancia de `Product` que se transformará en un DTO.</param>
    /// <returns>Un objeto `ProductCreateDTO` con los datos del producto.</returns>
    public ProductDTO ToDTO(Product product)
    {
        return new ProductDTO
        {
            id = product.id,
            created_at = product.created_at,
            modified_at = product.modified_at,
            is_deleted = product.is_deleted,
            deleted_at = product.deleted_at,
            type = product.type,
            name = product.name,
            price = product.price,
            status = product.status,
            text = product.text,
            Product_key = product.Product_key,
            image_link = product.image_lick
        };
    }
}