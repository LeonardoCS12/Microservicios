using System.Net;
using System.Text.Json;

namespace GraphqlBff.Clients;

/// <summary>
/// Cliente REST hacia el microservicio de Usuarios (red interna de Docker).
/// </summary>
public class UsersClient : IUsersClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public UsersClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Se pide is_deleted=false de forma explicita para no recibir usuarios eliminados.
        var pagina = await GetAsync<UsersPage>("api/users?is_deleted=false", cancellationToken);
        return pagina?.Users ?? new List<UserDto>();
    }

    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        GetAsync<UserDto>($"api/users/{id}", cancellationToken);

    public async Task<UserDto> CreateAsync(UserCreate data, CancellationToken cancellationToken)
    {
        // La API exige is_active; un usuario nuevo se crea activo. Si falta first_name o last_name,
        // la API responde 400 y ese error llega tal cual al cliente.
        var request = new
        {
            username = data.Username,
            password = data.Password,
            email = data.Email,
            first_name = data.FirstName,
            last_name = data.LastName,
            is_active = true
        };

        using var response = await _http.PostAsJsonAsync("api/users", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await UpstreamErrors.FromResponseAsync(response, "Usuarios", cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions, cancellationToken);
        var creado = body?.Resultado
            ?? throw UpstreamErrors.Error("Usuarios respondio sin datos del usuario creado.", "UPSTREAM_ERROR");

        return creado;
    }

    // GET generico: 404 -> null (para GraphQL "no hay datos" no es un error); otros fallos -> error GraphQL.
    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        using var response = await _http.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await UpstreamErrors.FromResponseAsync(response, "Usuarios", cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, cancellationToken);
        return body?.Resultado;
    }
}
