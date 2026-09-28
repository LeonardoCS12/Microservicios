using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using productos.Models;
using productos.Methods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.BearerToken;
using apitienda.Data;
using apitienda.Models;

public class ProductService : IProductService
{
    private readonly IProductDAO _iProductDAO;
    private readonly ProductMapper _productMapper;
    private readonly CreateProductMapper _createProductoMapper;
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _ihttpContextAccessor;

    public ProductService(IHttpContextAccessor contextAccessor, DataContext context, IProductDAO iProductDAO, ProductMapper productMapper, CreateProductMapper createProductoMapper)
    {
        _iProductDAO = iProductDAO;
        _productMapper = productMapper;
        _createProductoMapper = createProductoMapper;
        _context = context;
        _ihttpContextAccessor = contextAccessor;
    }

    /// <summary>
    /// Obtiene todos los productos de la base de datos.
    /// </summary>
    public async Task<IActionResult> GetAllProductsAsync(int? safePage,int? safeLimit,string? sort,string? safeOrder,bool? safeStatus,bool? safeIsdelete,string? type)
    {

        if(!safePage.HasValue && !safeLimit.HasValue && string.IsNullOrEmpty(sort) && string.IsNullOrEmpty(safeOrder) && !safeStatus.HasValue && !safeIsdelete.HasValue && string.IsNullOrEmpty(type))
        {
            safeIsdelete = false;
        }


        SentenciaProductos crearSentencia = new SentenciaProductos(safePage, safeLimit,sort,safeOrder,safeStatus,safeIsdelete,type);
        var sentencia = crearSentencia.CrearSenentiaSQLProduct();

        var products = await _iProductDAO.GetProducts(sentencia.Sentencia,sentencia.Parametros);

        if (products == null || !products.Any())
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("Productos404")));
        }



        //Contamos el numero de resultados
        var numResult = products.Count();

        // Mapear los productos a DTO antes de devolverlos
        var result = products.Select(p => _productMapper.ToDTO(p)).ToList();

        /// <summary>
        /// Lanza un ok 200 y regresa una respuesta de tipo ResponseGetProducts que a su ves regresa los productos.
        /// </summary>
        return new OkObjectResult(new ApiResponse<ResponseGetProducts<List<ProductDTO>>>(200,MessageService.Instance.GetMessage("Productos200"),
        new ResponseGetProducts<List<ProductDTO>>(numResult, sentencia.valores[6], sentencia.valores[5], result, sentencia.valores[3], sentencia.valores[4], sentencia.valores[0], sentencia.valores[1], sentencia.valores[2])));

    }

    /// <summary>
    /// Recuperar un producto por id.
    /// </summary>
    public async Task<IActionResult> GetProductByIdAsync(Guid id)
    {
        var product = await _iProductDAO.GetByIdAsync(id);
        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,"Producto no encontrado."));
        }


        // Mapear el producto a DTO antes de devolverlo
        var result = _productMapper.ToDTO(product);
        return new OkObjectResult(new ApiResponse<ProductDTO>(200,"Producto encontrado",result));
    }

    /// <summary>
    /// Crear un nuevo producto.
    /// </summary>
    public async Task<IActionResult> CreateProductAsync(ProductCreateDTO productDTO)
    {

        if (productDTO.price <= 0)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controllerProductDTO400"), "El precio debe ser mayor a 0."));
        }

        // Crear entidad usando el Mapper
        var product = _createProductoMapper.ToEntity(productDTO);

        await _iProductDAO.AddAsync(product);

        var result = _productMapper.ToDTO(product);

        var respuestaDTO = new ProductoResponseDTO{
            name = result.name,
            type = result.type,
            price = result.price,
        };

        var user = _ihttpContextAccessor.HttpContext?.User;
        var log = new Auditoria
        {
            Usuario = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("id")?.Value
                ?? user?.Identity?.Name
                ?? "ID_No_Encontrado",
            Accion = "CREAR_PRODUCTO",
            Tabla = "Products",
            RegistroId = result.id.ToString(),
            Detalle = $"Se registró el producto: {result.name}",
            Fecha = DateTime.UtcNow
        };

        _context.Auditorias.Add(log);
        await _context.SaveChangesAsync();

        return new OkObjectResult(new ApiResponse<ProductoResponseDTO>(200,MessageService.Instance.GetMessage("Productoscreate200"),respuestaDTO));
    }


    /// <summary>
    /// Actualizar un producto.
    /// </summary>
    public async Task<IActionResult> Put(Guid id, ProductUpdateDTO productUpdateDTO)
    {
        var product = await _iProductDAO.GetByIdAsync(id);
        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("ProductUpdate404")));
        }

        if(productUpdateDTO.price <= 0)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("ProductUpdate400")));
        }

        // Actualizar solo los campos proporcionados en el DTO, preservando los que no se proporcionan
        product.type = productUpdateDTO.type;
        product.name = productUpdateDTO.name;
        product.price = productUpdateDTO.price;
        product.status = productUpdateDTO.status;

        // Los campos opcionales solo se actualizan si el cliente proporciona un valor
        product.text = productUpdateDTO.text ?? product.text;
        product.image_lick = productUpdateDTO.image_link ?? product.image_lick;

        product.modified_at = DateTimeOffset.UtcNow; // Actualizamos la fecha de modificación

        // Guardar el producto actualizado
        await _iProductDAO.UpdateAsync(product);

        var user = _ihttpContextAccessor.HttpContext?.User;

        var log = new Auditoria
        {
            Usuario = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? user?.FindFirst("id")?.Value
                      ?? user?.Identity?.Name
                      ?? "Sistema/Admin",
            Accion = "ACTUALIZAR_PRODUCTO",
            Tabla = "Products",
            RegistroId = product.id.ToString(),
            Detalle = $"Se actualizó el producto: {product.name}. Precio final: {product.price}. Estado: {product.status}",
            Fecha = DateTime.UtcNow
        };

        _context.Auditorias.Add(log);
        await _context.SaveChangesAsync();

        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("ProductUpdate200"))); // Producto actualizado correctamente
    }


    /// <summary>
    /// Actualizar un producto parcialmente.
    /// </summary>
    public async Task<IActionResult> Patch(Guid id, ProductPartialUpdateDTO productPartialUpdate)
    {
        // Buscar el producto por ID en la base de datos
        var product = await _iProductDAO.GetByIdAsync(id);

        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("ProductPartialUpdate404")));
        }

        string nombreAnterior = product.name;

        if(product.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("ProductPartialUpdate400")));
        }

        if(productPartialUpdate.price <= 0){
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controllerProductDTO400")));
        }

        // Actualización parcial: solo se modifican los valores que no sean null en el DTO
        product.type = productPartialUpdate.type ?? product.type;
        product.name = productPartialUpdate.name ?? product.name;
        product.price = productPartialUpdate.price ?? product.price;
        product.status = productPartialUpdate.status ?? product.status;
        product.text = productPartialUpdate.text ?? product.text;
        product.Product_key = productPartialUpdate.Product_key ?? product.Product_key;
        product.image_lick = productPartialUpdate.image_lick ?? product.image_lick;

        // Registrar la última modificación
        product.modified_at = DateTimeOffset.UtcNow;

        // Guardar los cambios en la base de datos
        await _iProductDAO.UpdateAsync(product);

        try
        {
            var user = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? user?.FindFirst("id")?.Value
                          ?? user?.Identity?.Name
                          ?? "ID_No_Encontrado",
                Accion = "ACTUALIZACION_PARCIAL",
                Tabla = "Products",
                RegistroId = id.ToString(),
                Detalle = $"Cambio parcial en producto: {nombreAnterior}. Nuevo nombre: {product.name}, Precio: {product.price}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar auditoría en Patch: {ex.Message}");
        }

        return new OkObjectResult(new ApiResponse<string>(200, MessageService.Instance.GetMessage("ProductPartialUpdate200")));
    }


    /// <summary>
    /// Eliminado logico.
    /// </summary>
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _iProductDAO.GetByIdAsync(id);
        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("DeleteProduct404")));
        }
        //Si el producto ya se encuantra eliminado no se puede volver a eliminar.
        if(product.is_deleted){
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("DeleteProduct400")));
        }

        string nombreProducto = product.name;

        product.is_deleted = true;
        product.deleted_at = DateTimeOffset.UtcNow;

        await _iProductDAO.UpdateAsync(product);

        try
        {
            var user = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? user?.FindFirst("id")?.Value
                          ?? user?.Identity?.Name
                          ?? "ID_No_Encontrado",
                Accion = "ELIMINAR_LOGICO",
                Tabla = "Products",
                RegistroId = id.ToString(),
                Detalle = $"Se realizó el eliminado lógico del producto: {nombreProducto}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar eliminación de {id}: {ex.Message}");
        }

        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("DeleteProduct200")));
    }


    /// <summary>
    /// Recuperacion de productos eliminados.
    /// </summary>
    public async Task<IActionResult> RestoreProduct(Guid id)
    {
        var product = await _iProductDAO.GetByIdAsync(id);
        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("RestoreProduct404")));
        }
        //Si es producto no esta elimindo no se puede restaurar.
        if (!product.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("RestoreProduct400")));
        }

        product.is_deleted = false;
        product.deleted_at = null;

        await _iProductDAO.UpdateAsync(product);

        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("RestoreProduct200")));
    }

    /// <summary>
    // buscar por nombre o descripcion.
    /// </summary>
    public async Task<IActionResult> GetProductsSearh(string search){

        var products = await _iProductDAO.GetProductsSearh(search);

        if (products == null || products.Count == 0)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("ProductsSearh404")));
        }
        // Mapear los productos a DTO (si es necesario) antes de enviarlos en la respuesta
        var result = products.Select(p => _productMapper.ToDTO(p)).ToList();

        return new OkObjectResult(new ApiResponse<List<ProductDTO>>(200, MessageService.Instance.GetMessage("ProductsSearh200"),result));
    }


    /// <summary>
    // Listar productos por rango de precios.
    /// </summary>
    public async Task<IActionResult> GetProductsPrice(decimal min_price, decimal max_price)
    {
        var product = await _iProductDAO.GetProductsPrice(min_price,max_price);

        if(product == null || !product.Any()){
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("ProductsPrice404")));
        }

        var result = product.Select(p => _productMapper.ToDTO(p)).ToList();

        return new OkObjectResult(new ApiResponse<List<ProductDTO>>(200, MessageService.Instance.GetMessage("ProductsPrice200"),result));
    }

    /// <summary>
    // Actualizar imagen.
    /// </summary>
    public async Task<IActionResult> UpdateImage(Guid id, ProductUpdateImageDTO updateImage){

        var existingProduct = await _iProductDAO.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("UpdateImage404")));
        }

        string oldImage = existingProduct.image_lick ?? "Ninguna";

        existingProduct.image_lick = updateImage.image_link;

        await _iProductDAO.UpdateAsync(existingProduct);

        try
        {
            var user = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? user?.FindFirst("id")?.Value
                          ?? user?.Identity?.Name
                          ?? "ID_No_Encontrado",
                Accion = "ACTUALIZAR_IMAGEN",
                Tabla = "Products",
                RegistroId = id.ToString(),
                Detalle = $"Se cambió la imagen del producto '{existingProduct.name}'. URL anterior: {oldImage} -> Nueva URL: {existingProduct.image_lick}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar actualización de imagen: {ex.Message}");
        }

        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("UpdateImage200")));
    }

    /// <summary>
    // Desactivar un producto.
    /// </summary>
    public async Task<IActionResult> DeactivateProduct(Guid id){

        var product = await _iProductDAO.GetByIdAsync(id);

        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("DeactivateProduct404")));
        }

        if (!product.status)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("DeactivateProduct400")));
        }

        // Realizamos la desactivación lógica
        product.status = false;

        await _iProductDAO.UpdateAsync(product);
        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("DeactivateProduct200")));
    }

    /// <summary>
    // Activar un producto.
    /// </summary>
    public async Task<IActionResult> ActivateProduct(Guid id)
    {
        var product = await _iProductDAO.GetByIdAsync(id);
        if (product == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404, MessageService.Instance.GetMessage("ActivateProduct404")));
        }

        if (product.status)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400, MessageService.Instance.GetMessage("ActivateProduct400")));
        }

        // Activación lógica del producto
        product.status = true;
        await _iProductDAO.UpdateAsync(product);

        return new OkObjectResult(new ApiResponse<string>(200,MessageService.Instance.GetMessage("ActivateProduct200")));
    }

}