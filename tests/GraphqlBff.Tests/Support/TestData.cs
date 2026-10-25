using GraphqlBff.Clients;

namespace GraphqlBff.Tests;

/// <summary>Datos de ejemplo que "responden" los microservicios REST (dobles de prueba).</summary>
internal static class TestData
{
    // El mismo id de ejemplo que usa el PDF en los Codigos 1 y 5.
    public static readonly Guid ProductoId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    public static readonly Guid UsuarioId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");

    public static ProductDto NuevoProducto(decimal price = 450.0m) => new()
    {
        Id = ProductoId,
        Name = "Teclado",
        Type = "general",
        Price = price,
        Status = true,
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
    };

    public static UserDto NuevoUsuario() => new()
    {
        Id = UsuarioId,
        Username = "ana",
        Email = "ana@correo.com",
        FirstName = "Ana",
        LastName = "Perez"
    };

    public static IReadOnlyList<T> Lista<T>(params T[] items) => items;
}
