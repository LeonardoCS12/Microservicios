using productos.Models;
using Dapper;
public interface IProductDAO
{

    /// <summary>
    /// Recupera todos los productos desde la base de datos.
    /// </summary>
    Task<List<Product>> GetProducts(string Sentencia, DynamicParameters Parametros);

    /// <summary>
    /// Recupera un producto específico por su ID.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    Task<Product?> GetByIdAsync(Guid id);

    /// <summary>
    /// Agrega un nuevo producto a la base de datos.
    /// </summary>
    /// <param name="product">El producto a agregar.</param>
    Task AddAsync(Product product);

    /// <summary>
    /// Actualiza un producto existente en la base de datos.
    /// </summary>
    /// <param name="product">El producto con los nuevos datos.</param>
    Task UpdateAsync(Product product);

    /// <summary>
    /// Elimina un producto de la base de datos por su ID.
    /// </summary>
    /// <param name="id">ID del producto a eliminar.</param>
    Task DeleteProduct(Guid id);

    /// <summary>
    /// Actualiza parcialmente un producto en la base de datos.
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="product_partial">DTO con los datos parciales a actualizar.</param>
    Task<bool> UpdatePartialAsync(Guid id, ProductPartialUpdateDTO product_partial);

    /// <summary>
    /// Realiza una búsqueda de productos por un término de búsqueda.
    /// </summary>
    /// <param name="search">Término de búsqueda.</param>
    Task<List<Product>> GetProductsSearh(string search);

    /// <summary>
    /// Recupera productos en un rango de precios especificado.
    /// </summary>
    /// <param name="min_price">Precio mínimo.</param>
    /// <param name="max_price">Precio máximo.</param>
    Task<List<Product>> GetProductsPrice(decimal min_price, decimal max_price);

}