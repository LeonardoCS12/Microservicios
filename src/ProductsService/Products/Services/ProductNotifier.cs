using System.Net.Http.Json;

/// <summary>
/// Pide al servicio de Notificaciones que difunda un evento (POST /notificar, red interna).
/// Si Notificaciones no responde, solo se registra el aviso: no debe romper la operacion de Productos.
/// </summary>
public interface IProductNotifier
{
    Task NotifyAsync(string evento, object datos);
}

public class ProductNotifier : IProductNotifier
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductNotifier> _logger;

    public ProductNotifier(HttpClient http, ILogger<ProductNotifier> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task NotifyAsync(string evento, object datos)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("notificar", new { evento, datos });
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo notificar el evento {Evento}.", evento);
        }
    }
}
