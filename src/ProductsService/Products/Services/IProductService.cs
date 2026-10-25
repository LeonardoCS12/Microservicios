using System.Collections.Generic;
using System.Threading.Tasks;
using productos.Models;
using Microsoft.AspNetCore.Mvc;

public interface IProductService
{
    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    /// <returns>Lista de productos en formato DTO.</returns>
    Task<IActionResult> GetAllProductsAsync(int? page,int? limit,string? sort, string? order,bool? status,bool? is_delete,string? type);

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    /// <returns>El producto solicitado en formato DTO.</returns>
    Task<IActionResult> GetProductByIdAsync(Guid id);

    /// <summary>
    /// Crea un nuevo producto en la base de datos.
    /// </summary>
    /// <param name="productDTO">DTO del producto a crear.</param>
    /// <returns>Resultado de la creación del producto.</returns>
    Task<IActionResult> CreateProductAsync(ProductCreateDTO productCreateDTO);

    /// <summary>
    /// Actualiza completamente un producto.
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="productUpdateDTO">DTO con los datos para actualizar el producto.</param>
    /// <returns>Resultado de la actualización.</returns>
    Task<IActionResult> Put(Guid id, ProductUpdateDTO productUpdateDTO);

    /// <summary>
    /// Actualiza parcialmente un producto.
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="productPartialUpdate">DTO con los datos parciales para actualizar el producto.</param>
    /// <returns>Resultado de la actualización parcial.</returns>
    Task<IActionResult> Patch(Guid id, ProductPartialUpdateDTO productPartialUpdate);

    /// <summary>
    /// Elimina un producto de la base de datos.
    /// </summary>
    /// <param name="id">ID del producto a eliminar.</param>
    /// <returns>Resultado de la eliminación.</returns>
    Task<IActionResult> DeleteProduct(Guid id);

    /// <summary>
    /// Restaura un producto previamente eliminado (eliminación lógica).
    /// </summary>
    /// <param name="id">ID del producto a restaurar.</param>
    /// <returns>Resultado de la restauración.</returns>
    Task<IActionResult> RestoreProduct(Guid id);

    /// <summary>
    /// Busca productos por nombre u otros criterios.
    /// </summary>
    /// <param name="search">Término de búsqueda.</param>
    /// <returns>Lista de productos que coinciden con la búsqueda.</returns>
    Task<IActionResult> GetProductsSearh(string search);

    /// <summary>
    /// Obtiene productos dentro de un rango de precios.
    /// </summary>
    /// <param name="min_price">Precio mínimo.</param>
    /// <param name="max_price">Precio máximo.</param>
    /// <returns>Lista de productos dentro del rango de precios.</returns>
    Task<IActionResult> GetProductsPrice(decimal min_price, decimal max_price);

    /// <summary>
    /// Actualiza la imagen del producto.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    /// <param name="updateImage">DTO con la nueva imagen.</param>
    /// <returns>Resultado de la actualización de la imagen.</returns>
    Task<IActionResult> UpdateImage(Guid id, ProductUpdateImageDTO updateImage);

    /// <summary>
    /// Desactiva un producto (cambia su estado a inactivo).
    /// </summary>
    /// <param name="id">ID del producto a desactivar.</param>
    /// <returns>Resultado de la desactivación.</returns>
    Task<IActionResult> DeactivateProduct(Guid id);

    /// <summary>
    /// Activa un producto (cambia su estado a activo).
    /// </summary>
    /// <param name="id">ID del producto a activar.</param>
    /// <returns>Resultado de la activación.</returns>
    Task<IActionResult> ActivateProduct(Guid id);
}