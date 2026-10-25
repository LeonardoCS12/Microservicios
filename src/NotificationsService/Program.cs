using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotificationsService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConnectionManager>();

// Mismo JWT RS256 de la practica 6. El navegador no puede mandar cabeceras en el handshake WebSocket,
// por eso el token tambien se acepta como parametro de consulta (?access_token=...).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Path.StartsWithSegments("/ws") &&
                context.Request.Query.TryGetValue("access_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(configuration["Jwt:PublicKeyPath"]!));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

// Handshake WebSocket (Codigo 6 del PDF): GET /ws/notificaciones con Upgrade: websocket.
app.Map("/ws/notificaciones", async Task<IResult> (HttpContext context, ConnectionManager connections) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        return Results.BadRequest();
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var id = connections.Add(socket);
    try
    {
        // El cliente solo recibe; se lee hasta que cierre la conexion.
        var buffer = new byte[256];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
    }
    catch (Exception)
    {
        // conexion cerrada de forma abrupta
    }
    finally
    {
        connections.Remove(id);
    }

    return Results.Empty;
}).RequireAuthorization();

// Operacion interna de difusion (Codigo 7 del PDF). La llaman los servicios de dominio;
// el Gateway no la expone (solo enruta /ws/).
app.MapPost("/notificar", async Task<IResult> (NotificacionRequest request, ConnectionManager connections) =>
{
    if (string.IsNullOrWhiteSpace(request.Evento))
    {
        return Results.BadRequest(new { mensaje = "El campo evento es obligatorio." });
    }

    var json = JsonSerializer.Serialize(new { evento = request.Evento, datos = request.Datos });
    var enviados = await connections.BroadcastAsync(json);
    return Results.Ok(new { enviados });
});

app.Run();

public record NotificacionRequest(string? Evento, JsonElement? Datos);

public partial class Program { }
