using System.Net;
using System.Text.Json;
using HotChocolate;

namespace GraphqlBff.Clients;

/// <summary>
/// Traduce los errores HTTP de los microservicios a errores GraphQL.
/// Un solo lugar para los dos clientes (Usuarios y Productos).
/// </summary>
internal static class UpstreamErrors
{
    public static GraphQLException Error(string message, string code) =>
        new(ErrorBuilder.New().SetMessage(message).SetCode(code).Build());

    public static async Task<GraphQLException> FromResponseAsync(
        HttpResponseMessage response, string servicio, CancellationToken cancellationToken)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return Error("No autorizado: envia un JWT valido en el header Authorization.", "UNAUTHENTICATED");
        }

        // En estos casos el microservicio explica el motivo en su mensaje; se reutiliza tal cual.
        var detail = await TryReadMessageAsync(response, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error(detail ?? "Datos invalidos.", "BAD_USER_INPUT"),
            HttpStatusCode.NotFound => Error(detail ?? "No se encontro el recurso.", "NOT_FOUND"),
            HttpStatusCode.Conflict => Error(detail ?? "Conflicto con un registro existente.", "CONFLICT"),
            _ => Error($"El servicio de {servicio} respondio {(int)response.StatusCode}.", "UPSTREAM_ERROR")
        };
    }

    // Los microservicios devuelven { codigo, mensaje, resultado }; el detalle puede venir en cualquiera de los dos.
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
            // el cuerpo no era JSON: se usa el mensaje generico
        }

        return null;
    }
}
