using productos.Models;
using productos.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

public class ProductDAO : IProductDAO
{
    private readonly DataContextProduct _context;

    public ProductDAO(DataContextProduct context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los productos con sentencia personalizada del cliente (Query).
    /// </summary>
    public async Task<List<Product>> GetProducts(string sentencia, DynamicParameters parametros)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(); // Asegurar que la conexión se abre antes de usarla
        return (await connection.QueryAsync<Product>(sentencia, parametros)).ToList();
    }

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Agrega un nuevo producto a la base de datos.
    /// </summary>
    /// <param name="product">El producto a agregar.</param>
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza un producto existente en la base de datos.
    /// </summary>
    /// <param name="product">El producto con los datos actualizados.</param>
    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un producto de la base de datos por su ID.
    /// </summary>
    /// <param name="id">ID del producto a eliminar.</param>
    public async Task DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Actualiza parcialmente un producto en la base de datos.
    /// Solo actualiza los campos que no son nulos en el DTO.
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="product_partial">DTO con los datos parciales.</param>
    public async Task<bool> UpdatePartialAsync(Guid id, ProductPartialUpdateDTO product_partial)
    {
        var productExists = await _context.Products.FindAsync(id);
        if (productExists == null)
        {
            return false;
        }

        // Solo actualiza los valores no nulos del DTO
        if (!string.IsNullOrEmpty(product_partial.text))
            productExists.text = product_partial.text;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Realiza una búsqueda de productos basados en el nombre.
    /// </summary>
    /// <param name="search">Término de búsqueda que se encuentra en el nombre del producto.</param>
    public async Task<List<Product>> GetProductsSearh(string search)
    {
        return await _context.Products
            .Where(p => (p.name != null && p.name.Contains(search)) || (p.text != null && p.text.Contains(search)))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene productos dentro de un rango de precios.
    /// </summary>
    /// <param name="min_price">Precio mínimo.</param>
    /// <param name="max_price">Precio máximo.</param>
    public async Task<List<Product>> GetProductsPrice(decimal min_price, decimal max_price)
    {
        return await _context.Products
            .Where(p => p.price >= min_price && p.price <= max_price) // Filtra por rango de precios
            .ToListAsync();
    }

}