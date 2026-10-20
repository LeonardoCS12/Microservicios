namespace GraphqlBff.Clients;

// Datos que el BFF envia a los microservicios. Se agrupan en records para no pasar muchos parametros sueltos.

/// <summary>Datos para crear un producto (POST /api/products).</summary>
public record ProductCreate(string Type, string Name, decimal Price, string? Text, string? ProductKey, string? ImageLink);

/// <summary>Cambios opcionales de un producto (PATCH /api/products/{id}). Lo que sea null no se envia.</summary>
public record ProductChanges(string? Type, string? Name, decimal? Price, bool? Status, string? Text, string? ProductKey, string? ImageLink);

/// <summary>Datos para crear un usuario (POST /api/users). La API de Usuarios exige nombre y apellido.</summary>
public record UserCreate(string Username, string Password, string Email, string? FirstName, string? LastName);
