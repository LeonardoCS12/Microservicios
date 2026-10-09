using System.Net;
using System.Text.Json;
using HotChocolate;

namespace GraphqlBff.Clients;

/// <summary>
/// Cliente REST hacia el microservicio de Productos (red interna de Docker).
/// </summary>
public class ProductsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public ProductsClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>GET api/products</summary>
    public Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken) =>
        GetListAsync("api/products", cancellationToken);

    /// <summary>GET api/products?user_id={id} (HU-05): productos que creo un usuario.</summary>
    public Task<IReadOnlyList<ProductDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        GetListAsync($"api/products?user_id={userId}", cancellationToken);

    private async Task<IReadOnlyList<ProductDto>> GetListAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(url, cancellationToken);

        // El microservicio responde 404 cuando no hay productos: para GraphQL eso es una lista vacia.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Array.Empty<ProductDto>();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await ToErrorAsync(response, cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductsPage>>(JsonOptions, cancellationToken);
        return body?.Resultado?.Products ?? new List<ProductDto>();
    }

    /// <summary>POST api/products (HU-04). Las reglas de negocio (ej. precio &gt; 0) las valida Productos, no el BFF.</summary>
    public async Task<ProductDto> CreateAsync(string name, string type, decimal price, CancellationToken cancellationToken)
    {
        var request = new { type, name, price };
        using var response = await _http.PostAsJsonAsync("api/products", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await ToErrorAsync(response, cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(JsonOptions, cancellationToken);
        return body?.Resultado ?? throw Error("Productos respondio sin datos del producto creado.", "UPSTREAM_ERROR");
    }

    // ---- Manejo de errores: traduce errores HTTP del microservicio a errores GraphQL ----

    private static async Task<GraphQLException> ToErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return Error("No autorizado: envia un JWT valido en el header Authorization.", "UNAUTHENTICATED");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var detail = await TryReadMessageAsync(response, cancellationToken);
            return Error(detail ?? "Datos invalidos.", "BAD_USER_INPUT");
        }

        return Error($"El servicio de Productos respondio {(int)response.StatusCode}.", "UPSTREAM_ERROR");
    }

    // Productos devuelve { codigo, mensaje, resultado }; en los 400 el detalle suele venir en "resultado".
    private static async Task<string?> TryReadMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (root.TryGetProperty("resultado", out var resultado) && resultado.ValueKind == JsonValueKind.String)
            {
                return resultado.GetString();
            }

            if (root.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
            {
                return mensaje.GetString();
            }
        }
        catch (JsonException)
        {
            // cuerpo que no es JSON: se usa el mensaje generico
        }

        return null;
    }

    private static GraphQLException Error(string message, string code) =>
        new(ErrorBuilder.New().SetMessage(message).SetCode(code).Build());
}
