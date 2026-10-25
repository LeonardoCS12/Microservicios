using System.Text.Json;
using GraphqlBff.Clients;
using Moq;
using Xunit;

namespace GraphqlBff.Tests;

/// <summary>
/// Pruebas automatizadas del BFF (seccion 8.1 del PDF): se ejecutan las cinco queries y las tres
/// mutations contra el schema GraphQL real, con los microservicios REST sustituidos por dobles de
/// prueba, y se verifica la forma exacta de la respuesta.
/// Las operaciones de los Codigos 1, 2, 4 y 5 son las del PDF tal cual.
/// </summary>
public class GraphqlExecutionTests
{
    private readonly BffHarness _bff = new();

    // ---------- Queries ----------

    [Fact]
    public async Task Query_usuarios()
    {
        _bff.Users.Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Lista(TestData.NuevoUsuario()));

        using var respuesta = await _bff.RunAsync("query { usuarios { id username email } }");

        SinErrores(respuesta);
        var usuario = Data(respuesta).GetProperty("usuarios").EnumerateArray().Single();
        Assert.Equal(new[] { "id", "username", "email" }, Nombres(usuario));
        Assert.Equal(TestData.UsuarioId.ToString(), usuario.GetProperty("id").GetString());
        Assert.Equal("ana", usuario.GetProperty("username").GetString());
        Assert.Equal("ana@correo.com", usuario.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Query_usuario_por_id()
    {
        _bff.Users.Setup(c => c.GetByIdAsync(TestData.UsuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.NuevoUsuario());

        using var respuesta = await _bff.RunAsync(
            "query { usuario(id: \"" + TestData.UsuarioId + "\") { id username email } }");

        SinErrores(respuesta);
        var usuario = Data(respuesta).GetProperty("usuario");
        Assert.Equal(new[] { "id", "username", "email" }, Nombres(usuario));
        Assert.Equal(TestData.UsuarioId.ToString(), usuario.GetProperty("id").GetString());
        Assert.Equal("ana", usuario.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Query_productos()
    {
        _bff.Products.Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Lista(TestData.NuevoProducto()));

        using var respuesta = await _bff.RunAsync("query { productos { id name price } }");

        SinErrores(respuesta);
        var producto = Data(respuesta).GetProperty("productos").EnumerateArray().Single();
        Assert.Equal(new[] { "id", "name", "price" }, Nombres(producto));
        Assert.Equal("Teclado", producto.GetProperty("name").GetString());
        Assert.Equal(450.0, producto.GetProperty("price").GetDouble());
    }

    // Codigo 1 del PDF
    [Fact]
    public async Task Query_producto_por_id_Codigo1()
    {
        _bff.Products.Setup(c => c.GetByIdAsync(TestData.ProductoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.NuevoProducto());

        using var respuesta = await _bff.RunAsync(
            """query { producto(id: "550e8400-e29b-41d4-a716-446655440000") { id name price } }""");

        SinErrores(respuesta);
        var producto = Data(respuesta).GetProperty("producto");
        Assert.Equal(new[] { "id", "name", "price" }, Nombres(producto));
        Assert.Equal("550e8400-e29b-41d4-a716-446655440000", producto.GetProperty("id").GetString());
        Assert.Equal("Teclado", producto.GetProperty("name").GetString());
        Assert.Equal(450.0, producto.GetProperty("price").GetDouble());
    }

    // Codigo 2 del PDF
    [Fact]
    public async Task Query_panelGeneral_agrupa_usuarios_y_productos_Codigo2()
    {
        _bff.Users.Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Lista(TestData.NuevoUsuario()));
        _bff.Products.Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Lista(TestData.NuevoProducto()));

        using var respuesta = await _bff.RunAsync(
            "query { panelGeneral { usuarios { id username email } productos { id name price } } }");

        SinErrores(respuesta);
        var panel = Data(respuesta).GetProperty("panelGeneral");

        // Forma exacta: un unico objeto con las dos listas agrupadas.
        Assert.Equal(new[] { "usuarios", "productos" }, Nombres(panel));

        var usuario = panel.GetProperty("usuarios").EnumerateArray().Single();
        Assert.Equal(new[] { "id", "username", "email" }, Nombres(usuario));
        Assert.Equal("ana", usuario.GetProperty("username").GetString());

        var producto = panel.GetProperty("productos").EnumerateArray().Single();
        Assert.Equal(new[] { "id", "name", "price" }, Nombres(producto));
        Assert.Equal("Teclado", producto.GetProperty("name").GetString());
    }

    // ---------- Mutations ----------

    // Codigo 4 del PDF
    [Fact]
    public async Task Mutation_crearProducto_Codigo4()
    {
        _bff.Products.Setup(c => c.CreateAsync(It.IsAny<ProductCreate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.NuevoProducto());

        using var respuesta = await _bff.RunAsync(
            """mutation { crearProducto(input: { name: "Teclado", price: 450.0, status: true }) { id name price } }""");

        SinErrores(respuesta);
        var producto = Data(respuesta).GetProperty("crearProducto");
        Assert.Equal(new[] { "id", "name", "price" }, Nombres(producto));
        Assert.Equal("550e8400-e29b-41d4-a716-446655440000", producto.GetProperty("id").GetString());
        Assert.Equal("Teclado", producto.GetProperty("name").GetString());
        Assert.Equal(450.0, producto.GetProperty("price").GetDouble());
    }

    // Codigo 5 del PDF
    [Fact]
    public async Task Mutation_actualizarProducto_Codigo5()
    {
        _bff.Products.Setup(c => c.UpdateAsync(TestData.ProductoId, It.IsAny<ProductChanges>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.NuevoProducto(price: 420.0m));

        using var respuesta = await _bff.RunAsync(
            """mutation { actualizarProducto(id: "550e8400-e29b-41d4-a716-446655440000", input: { price: 420.0 }) { id name price } }""");

        SinErrores(respuesta);
        var producto = Data(respuesta).GetProperty("actualizarProducto");
        Assert.Equal(new[] { "id", "name", "price" }, Nombres(producto));
        Assert.Equal("550e8400-e29b-41d4-a716-446655440000", producto.GetProperty("id").GetString());
        Assert.Equal(420.0, producto.GetProperty("price").GetDouble());
    }

    [Fact]
    public async Task Mutation_crearUsuario()
    {
        _bff.Users.Setup(c => c.CreateAsync(It.IsAny<UserCreate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.NuevoUsuario());

        using var respuesta = await _bff.RunAsync(
            """mutation { crearUsuario(input: { username: "ana", password: "Secreta123!", email: "ana@correo.com" }) { id username email } }""");

        SinErrores(respuesta);
        var usuario = Data(respuesta).GetProperty("crearUsuario");
        Assert.Equal(new[] { "id", "username", "email" }, Nombres(usuario));
        Assert.Equal("ana", usuario.GetProperty("username").GetString());
        Assert.Equal("ana@correo.com", usuario.GetProperty("email").GetString());
    }

    // ---------- Ayudas ----------

    private static JsonElement Data(JsonDocument respuesta) => respuesta.RootElement.GetProperty("data");

    private static string[] Nombres(JsonElement objeto) => objeto.EnumerateObject().Select(p => p.Name).ToArray();

    private static void SinErrores(JsonDocument respuesta) =>
        Assert.False(respuesta.RootElement.TryGetProperty("errors", out _),
            "La respuesta GraphQL trae errores: " + respuesta.RootElement.GetRawText());
}
