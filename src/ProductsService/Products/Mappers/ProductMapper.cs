using productos.Models;

public class ProductMapper
{
    /// <summary>
    /// Convierte un objeto de tipo Product a ProductDTO.
    /// </summary>
    /// <param name="product">El objeto Product que se desea convertir.</param>
    /// <returns>Un objeto ProductDTO con los datos del Product.</returns>
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

    /// <summary>
    /// Convierte un objeto de tipo ProductDTO a Product (entidad).
    /// </summary>
    /// <param name="product">El objeto ProductDTO que se desea convertir.</param>
    /// <returns>Un objeto Product con los datos del ProductDTO.</returns>
    public virtual Product ToEntity(ProductDTO product)
    {
        return new Product
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
            // description = product.text,
            // product_key = product.Product_key,
            //image_link = product.image_link
        };
    }


}