using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using productos.Models;
using Microsoft.AspNetCore.Authorization;


[Route("api/")]
[Authorize(AuthenticationSchemes = "Bearer")]
[ApiController]
public class productController : ControllerBase
{
    private readonly IProductService _iProductService;

    public productController(IProductService productService)
    {
        _iProductService = productService;
    }

    /// <summary>
    /// Obtiene los productos de la base de datos con parametros opcionales.
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetAll2(int? page,int? limit,string? sort,string? order,bool? status,bool? is_deleted,string? type)
    {
        try{

            if(!(sort == null)){
                if(!(sort == "name" || sort =="price" || sort == "created_at" || sort == "type" || sort == "status")){
                    return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
                }

            }

            if(!(order == null)){
                if(!(order == "desc" || order == "asc")){
                    return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
                }
            }

            if(!(page==null)){
                if(page < 1){
                    return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
                }
            }

            if(!(limit==null)){
                if(limit < 1){
                    return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
                }
            }

            /// <summary>
            /// Se determina su el valor es null y si no lo es se le asigna el valor despues de '??' a las variables.
            /// </summary>
            /*int safePage = page ?? 1;
            int safeLimit = limit ?? 10;
            bool safeStatus = status ?? true;
            bool safeIsdelete = is_deleted ?? false;
            string safeOrder = order ?? "asc";*/

            var result = await _iProductService.GetAllProductsAsync(page,limit,sort,order,status,is_deleted,type);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500,new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerGetAll500")));
        }
    }

    /// <summary>
    /// Recuperar un producto por id.
    /// </summary>
    [HttpGet("products/{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            var result = await _iProductService.GetProductByIdAsync(id);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerGetProduct500")));
        }
    }

    /// <summary>
    /// Crear un nuevo producto.
    /// </summary>
    [HttpPost("products")]
    public async Task<IActionResult> Post([FromBody] ProductCreateDTO productDTO)
    {
        try{
            if (productDTO == null)
            {
                return BadRequest(new ApiResponse<string>(404,MessageService.Instance.GetMessage("controllerProductDTO400")));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(404,MessageService.Instance.GetMessage("controllerProductDTO400")));
            }
            var result = await _iProductService.CreateProductAsync(productDTO);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerPost500")));
        }
    }

    /// <summary>
    /// Actualizar un producto.
    /// </summary>
    [HttpPut("products/{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] ProductUpdateDTO productUpdateDTO)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            if (productUpdateDTO == null)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerProductDTO400")));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerProductDTO400")));
            }

            var result = await _iProductService.Put(id, productUpdateDTO);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerPut500")));
        }
    }

    /// <summary>
    /// Actualiza parcialmente un producto existente.
    /// </summary>
    [HttpPatch("products/{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] ProductPartialUpdateDTO productPartialUpdate)
    {
        try{
            if(id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
            }

            if(productPartialUpdate == null)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controller400")));
            }

            var result = await _iProductService.Patch(id, productPartialUpdate);
            return result;
        }
        catch(Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerPatch500")));
        }
    }

    /// <summary>
    /// Eliminado logico.
    /// </summary>
    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }
            var result = await _iProductService.DeleteProduct(id);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerDeleteProduct500")));
        }

    }

    /// <summary>
    /// Recuperacion de productos eliminados.
    /// </summary>
    [HttpPatch("products/{id}/restore")]
    public async Task<IActionResult> RestoreProduct(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            var result = await _iProductService.RestoreProduct(id);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerRestoreProduct500")));
        }
    }

    /// <summary>
    // buscar por nombre o descripcion.
    /// </summary>
    [HttpGet("products/search/")]
    public async Task<IActionResult> GetProductsSearch(string search)
    {
        try{

            if (string.IsNullOrEmpty(search))
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerIsNullOrEmpty400")));
            }

            var result = await _iProductService.GetProductsSearh(search);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerGetProductsSearch500")));
        }
    }

    /// <summary>
    // Listar productos por rango de precios.
    /// </summary>
    [HttpGet("products/{min_price}/{max_price}")]
    public async Task<IActionResult> GetProductsPrice(decimal min_price,decimal max_price)
    {
        try{
            if (min_price <= 0)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerProductsPrice400")));
            }

            if (max_price < min_price)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerProductsPrice400")));
            }


            var result = await _iProductService.GetProductsPrice(min_price, max_price);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerGetProductsPrice500")));
        }

    }

    /// <summary>
    // Actualizar imagen.
    /// </summary>
    [HttpPatch("products/{id}/update-image")]
    public async Task<IActionResult> UpdateImage(Guid id, [FromBody] ProductUpdateImageDTO updateImage)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            if (updateImage == null)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerProductDTO400")));
            }

            var result = await _iProductService.UpdateImage(id, updateImage);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerUpdateImage500")));
        }
    }

    /// <summary>
    // Desactivar un producto.
    /// </summary>
    [HttpPatch("products/{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            var result = await _iProductService.DeactivateProduct(id);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerDeactivateProduct500")));
        }


    }

    /// <summary>
    // Activar un producto.
    /// </summary>
    [HttpPatch("products/{id}/activate")]
    public async Task<IActionResult> ActivateProduct(Guid id)
    {
        try{
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400,MessageService.Instance.GetMessage("controllerGuidEmpty400")));
            }

            var result = await _iProductService.ActivateProduct(id);
            return result;

        }catch(Exception ex){
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500,MessageService.Instance.GetMessage("controllerActivateProduct500")));
        }
    }
}