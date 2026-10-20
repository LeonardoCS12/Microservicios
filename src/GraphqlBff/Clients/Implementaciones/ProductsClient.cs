using System.Net;
using System.Text.Json;

namespace GraphqlBff.Clients;

/// <summary>
/// Cliente REST hacia el microservicio de Productos (red interna de Docker).
/// </summary>
public class ProductsClient : IProductsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public ProductsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Se pide is_deleted=false de forma explicita para no recibir productos eliminados.
        var pagina = await GetAsync<ProductsPage>("api/products?is_deleted=false", cancellationToken);
        return pagina?.Products ?? new List<ProductDto>();
    }

    public Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        GetAsync<ProductDto>($"api/products/{id}", cancellationToken);

    public async Task<ProductDto> CreateAsync(ProductCreate data, CancellationToken cancellationToken)
    {
        // La API usa los nombres text / image_link / Product_key. Si Product_key viene vacia, ella genera una.
        var request = new
        {
            type = data.Type,
            name = data.Name,
            price = data.Price,
            text = data.Text,
            image_link = data.ImageLink,
            Product_key = data.ProductKey
        };

        using var response = await _http.PostAsJsonAsync("api/products", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await UpstreamErrors.FromResponseAsync(response, "Productos", cancellationToken);
        }

        // La API NO devuelve el id al crear (solo name, type y price) y no se modifica.
        // El BFF lo recupera buscando el producto recien creado: mismo nombre, tipo y precio, el mas reciente.
        var precioGuardado = Math.Round(data.Price, 2, MidpointRounding.AwayFromZero);   // la BD guarda 2 decimales
        var candidatos = await SearchAsync(data.Name, cancellationToken);

        var creado = candidatos
            .Where(p => p.Name == data.Name
                        && p.Type == data.Type
                        && p.Price == precioGuardado
                        && (string.IsNullOrEmpty(data.ProductKey) || p.ProductKey == data.ProductKey))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        return creado ?? throw UpstreamErrors.Error(
            "El producto se creo, pero no se pudo recuperar su id.", "UPSTREAM_ERROR");
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductChanges changes, CancellationToken cancellationToken)
    {
        // Solo se envian los campos con valor. OJO: el PATCH de Productos nombra la imagen "image_lick" (asi esta en esa API).
        var cuerpo = new Dictionary<string, object?>();
        if (changes.Type != null) cuerpo["type"] = changes.Type;
        if (changes.Name != null) cuerpo["name"] = changes.Name;
        if (changes.Price != null) cuerpo["price"] = changes.Price;
        if (changes.Status != null) cuerpo["status"] = changes.Status;
        if (changes.Text != null) cuerpo["text"] = changes.Text;
        if (changes.ProductKey != null) cuerpo["Product_key"] = changes.ProductKey;
        if (changes.ImageLink != null) cuerpo["image_lick"] = changes.ImageLink;

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/products/{id}")
        {
            Content = JsonContent.Create(cuerpo, options: JsonOptions)
        };
        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await UpstreamErrors.FromResponseAsync(response, "Productos", cancellationToken);
        }

        // El PATCH solo responde un mensaje; se vuelve a leer el producto para devolverlo completo.
        return await GetByIdAsync(id, cancellationToken)
            ?? throw UpstreamErrors.Error("El producto se actualizo, pero no se pudo leer de nuevo.", "UPSTREAM_ERROR");
    }

    // GET /api/products/search/?search=... (busca en nombre y descripcion). Solo lo usa CreateAsync.
    private async Task<IReadOnlyList<ProductDto>> SearchAsync(string text, CancellationToken cancellationToken)
    {
        var lista = await GetAsync<List<ProductDto>>(
            $"api/products/search/?search={Uri.EscapeDataString(text)}", cancellationToken);
        return lista ?? new List<ProductDto>();
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
            throw await UpstreamErrors.FromResponseAsync(response, "Productos", cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, cancellationToken);
        return body?.Resultado;
    }
}
