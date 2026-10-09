using System.Net;
using System.Text.Json;
using HotChocolate;

namespace GraphqlBff.Clients;

/// <summary>
/// Cliente REST hacia el microservicio de Usuarios (red interna de Docker).
/// </summary>
public class UsersClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public UsersClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>GET api/users/{id}. Devuelve null si el usuario no existe.</summary>
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync($"api/users/{id}", cancellationToken);

        // Para GraphQL, "no existe" es null (el schema declara usuario como nullable), no un error.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw Error("No autorizado: envia un JWT valido en el header Authorization.", "UNAUTHENTICATED");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw Error($"El servicio de Usuarios respondio {(int)response.StatusCode}.", "UPSTREAM_ERROR");
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions, cancellationToken);
        return body?.Resultado;
    }

    private static GraphQLException Error(string message, string code) =>
        new(ErrorBuilder.New().SetMessage(message).SetCode(code).Build());
}
