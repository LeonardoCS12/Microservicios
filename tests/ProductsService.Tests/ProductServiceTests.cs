using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using productos.Models;
using productos.Methods;
using apitienda.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

public class ProductServiceTests
{
    private readonly Mock<IProductDAO> _daoMock;
    private readonly Mock<IHttpContextAccessor> _httpContextMock;
    private readonly DataContext _context; // Quitamos el Mock y usamos la clase real
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _daoMock = new Mock<IProductDAO>();
        _httpContextMock = new Mock<IHttpContextAccessor>();

        // 1. Configuramos el ServiceProvider para aislar InMemory de PostgreSQL
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider)
            .Options;

        // IMPORTANTE: Instancia REAL del contexto, no Mock
        _context = new DataContext(options);

        // 2. Simulamos el usuario dentro del HttpContext para la Auditoría
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, "UsuarioTest"),
            new Claim("id", "123")
        }, "TestAuth"));

        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns(user);
        _httpContextMock.Setup(h => h.HttpContext).Returns(mockContext.Object);

        // 3. Usamos mappers reales
        var productMapper = new ProductMapper();
        var createProductMapper = new CreateProductMapper();

        // 4. Instanciamos el servicio con el CONTEXTO REAL
        _service = new ProductService(
            _httpContextMock.Object,
            _context,     // <--- Pasamos la instancia real
            _daoMock.Object,
            productMapper,
            createProductMapper
        );
    }

    [Fact]
    public async Task CreateProductAsync_RetornaOk_Y_RegistraAuditoria()
    {
        // Arrange
        var productDto = new ProductCreateDTO
        {
            name = "Martillo",
            price = 150,
            type = "Ferretería"
        };

        // Act
        var result = await _service.CreateProductAsync(productDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificar que la auditoría se guardó en el contexto real
        Assert.True(await _context.Auditorias.AnyAsync(a => a.Tabla == "Products" || a.Tabla == "Productos"));
        _daoMock.Verify(d => d.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_RetornaOk_CuandoProductoExiste()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoExistente = new Product { id = productId, name = "Taladro", price = 1500, type = "Herramienta" };
        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoExistente);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetProductByIdAsync_RetornaNotFound_CuandoProductoNoExiste()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Configuración del Mock para devolver nulo (simulando que no existe en BD)
        _daoMock.Setup(d => d.GetByIdAsync(productId))
                .ReturnsAsync((Product)null!);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Put_RetornaOk_CuandoActualizacionEsExitosa()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoExistente = new Product { id = productId, name = "Original", price = 100, type = "Tipo A" };
        var updateDto = new ProductUpdateDTO
        {
            name = "Actualizado",
            price = 500,
            type = "Tipo B",
            status = true,
            text = "Nueva descripción"
        };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoExistente);

        // Act
        var result = await _service.Put(productId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificamos que los campos se actualizaron en el objeto antes de enviarlo al DAO
        Assert.Equal("Actualizado", productoExistente.name);
        Assert.Equal(500, productoExistente.price);
        _daoMock.Verify(d => d.UpdateAsync(productoExistente), Times.Once);
    }

    [Fact]
    public async Task Put_RetornaBadRequest_CuandoPrecioEsInvalido()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoExistente = new Product { id = productId };

        var updateDto = new ProductUpdateDTO
        {
            name = "Producto Temporal",
            type = "Categoría Test",
            price = 0 // Este es el valor que realmente estamos probando
        };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoExistente);

        // Act
        var result = await _service.Put(productId, updateDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Patch_RetornaOk_CuandoActualizaSoloUnCampo()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoOriginal = new Product
        {
            id = productId,
            name = "Nombre Original",
            price = 100,
            type = "Herramienta",
            is_deleted = false
        };

        // Solo se desea actualizar el precio, los demás campos son nulos
        var patchDto = new ProductPartialUpdateDTO { price = 999 };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoOriginal);

        // Act
        var result = await _service.Patch(productId, patchDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificación de actualización parcial
        Assert.Equal(999, productoOriginal.price); // Cambió
        Assert.Equal("Nombre Original", productoOriginal.name); // Se mantuvo igual
        _daoMock.Verify(d => d.UpdateAsync(productoOriginal), Times.Once);
    }

    [Fact]
    public async Task Patch_RetornaBadRequest_CuandoProductoEstaEliminado()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoEliminado = new Product { id = productId, is_deleted = true };
        var patchDto = new ProductPartialUpdateDTO { name = "Nuevo Nombre" };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoEliminado);

        // Act
        var result = await _service.Patch(productId, patchDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_RetornaOk_CuandoEliminacionLogicaEsExitosa()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoActivo = new Product { id = productId, is_deleted = false };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoActivo);

        // Act
        var result = await _service.DeleteProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificación de cambio de estado
        Assert.True(productoActivo.is_deleted);
        Assert.NotNull(productoActivo.deleted_at);
        _daoMock.Verify(d => d.UpdateAsync(productoActivo), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_RetornaBadRequest_CuandoYaEstaEliminado()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoYaEliminado = new Product { id = productId, is_deleted = true };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoYaEliminado);

        // Act
        var result = await _service.DeleteProduct(productId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        // No se debe llamar a Update si ya estaba borrado
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task RestoreProduct_RetornaOk_CuandoRestauracionEsExitosa()
    {
        // Arrange
        var productId = Guid.NewGuid();
        // El producto debe estar marcado como eliminado para poder restaurarse
        var productoEliminado = new Product { id = productId, is_deleted = true, deleted_at = DateTimeOffset.UtcNow };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoEliminado);

        // Act
        var result = await _service.RestoreProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificación de reversión de estado
        Assert.False(productoEliminado.is_deleted);
        Assert.Null(productoEliminado.deleted_at);
        _daoMock.Verify(d => d.UpdateAsync(productoEliminado), Times.Once);
    }

    [Fact]
    public async Task RestoreProduct_RetornaBadRequest_CuandoProductoYaEstaActivo()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoActivo = new Product { id = productId, is_deleted = false };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoActivo);

        // Act
        var result = await _service.RestoreProduct(productId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        // No se debe intentar actualizar si no hubo cambios de estado
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
    [Fact]
    public async Task GetProductsSearh_RetornaOk_CuandoExistenCoincidencias()
    {
        // Arrange
        string terminoBusqueda = "martillo";
        var productosEncontrados = new List<Product>
        {
            new Product
            {
                id = Guid.NewGuid(),
                name = "Martillo",
                price = 100,
                type = "Herramientas"
            }
        };

        _daoMock.Setup(d => d.GetProductsSearh(terminoBusqueda))
                .ReturnsAsync(productosEncontrados);

        // Act
        var result = await _service.GetProductsSearh(terminoBusqueda);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // En lugar de buscar una propiedad específica, vamos a convertir el objeto a JSON
        // y verificar que el texto contenga el nombre del producto.
        // Esto siempre funciona sin importar cómo se llamen las propiedades internas.
        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);

        Assert.Contains("Martillo", jsonResponse);
        Assert.Contains("100", jsonResponse);
    }
    [Fact]
    public async Task DeactivateProduct_RetornaOk_CuandoDesactivacionEsExitosa()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoActivo = new Product { id = productId, status = true };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoActivo);

        // Act
        var result = await _service.DeactivateProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificación del cambio de estado a falso
        Assert.False(productoActivo.status);
        _daoMock.Verify(d => d.UpdateAsync(productoActivo), Times.Once);
    }

    [Fact]
    public async Task DeactivateProduct_RetornaBadRequest_CuandoYaEstaDesactivado()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoDesactivado = new Product { id = productId, status = false };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoDesactivado);

        // Act
        var result = await _service.DeactivateProduct(productId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        // No se debe realizar la actualización si el estado ya era falso
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ActivateProduct_RetornaOk_CuandoActivacionEsExitosa()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoInactivo = new Product { id = productId, status = false };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoInactivo);

        // Act
        var result = await _service.ActivateProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        // Verificación de que el estado cambió a verdadero
        Assert.True(productoInactivo.status);
        _daoMock.Verify(d => d.UpdateAsync(productoInactivo), Times.Once);
    }

    [Fact]
    public async Task ActivateProduct_RetornaBadRequest_CuandoYaEstaActivo()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productoYaActivo = new Product { id = productId, status = true };

        _daoMock.Setup(d => d.GetByIdAsync(productId)).ReturnsAsync(productoYaActivo);

        // Act
        var result = await _service.ActivateProduct(productId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);

        // No se debe invocar la actualización si no hay cambio de estado real
        _daoMock.Verify(d => d.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

}
