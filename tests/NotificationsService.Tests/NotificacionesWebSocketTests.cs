using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace NotificationsService.Tests;

/// <summary>
/// Pruebas del servicio de Notificaciones: handshake WebSocket con JWT (secciones 6.1 y 7.2 del PDF)
/// y difusion de un evento pedido por POST /notificar (seccion 6.2). Se usa un par de llaves RSA
/// generado para la prueba, asi no depende de las llaves reales.
/// </summary>
public class NotificacionesWebSocketTests : IDisposable
{
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly string _publicKeyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pem");
    private readonly WebApplicationFactory<Program> _factory;

    public NotificacionesWebSocketTests()
    {
        File.WriteAllText(_publicKeyPath, _rsa.ExportSubjectPublicKeyInfoPem());
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:PublicKeyPath"] = _publicKeyPath,
                ["Jwt:Issuer"] = "apitienda",
                ["Jwt:Audience"] = "apitienda_users"
            })));
    }

    [Fact]
    public async Task Evento_pedido_por_POST_llega_al_cliente_WebSocket()
    {
        var wsClient = _factory.Server.CreateWebSocketClient();
        using var socket = await wsClient.ConnectAsync(
            new Uri(_factory.Server.BaseAddress, $"/ws/notificaciones?access_token={CrearToken()}"), CancellationToken.None);

        var respuesta = await _factory.CreateClient().PostAsJsonAsync("/notificar", new
        {
            evento = "producto_creado",
            datos = new { id = 101, nombre = "Teclado", precio = 450.0 }
        });
        Assert.True(respuesta.IsSuccessStatusCode);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var buffer = new byte[1024];
        var recibido = await socket.ReceiveAsync(buffer, cts.Token);
        using var mensaje = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, recibido.Count));

        Assert.Equal("producto_creado", mensaje.RootElement.GetProperty("evento").GetString());
        var datos = mensaje.RootElement.GetProperty("datos");
        Assert.Equal(101, datos.GetProperty("id").GetInt32());
        Assert.Equal("Teclado", datos.GetProperty("nombre").GetString());
        Assert.Equal(450.0, datos.GetProperty("precio").GetDouble());
    }

    [Fact]
    public async Task Handshake_sin_token_es_rechazado()
    {
        var wsClient = _factory.Server.CreateWebSocketClient();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            wsClient.ConnectAsync(new Uri(_factory.Server.BaseAddress, "/ws/notificaciones"), CancellationToken.None));
    }

    private string CrearToken()
    {
        var credenciales = new SigningCredentials(new RsaSecurityKey(_rsa), SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken("apitienda", "apitienda_users",
            expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: credenciales);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _rsa.Dispose();
        File.Delete(_publicKeyPath);
    }
}
