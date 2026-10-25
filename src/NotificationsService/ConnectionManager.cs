using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace NotificationsService;

/// <summary>Lista de conexiones WebSocket activas y operacion de difusion (broadcast).</summary>
public class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();

    public Guid Add(WebSocket socket)
    {
        var id = Guid.NewGuid();
        _connections[id] = socket;
        return id;
    }

    public void Remove(Guid id) => _connections.TryRemove(id, out _);

    /// <summary>Envia el mensaje a todas las conexiones abiertas. Devuelve a cuantas se envio.</summary>
    public async Task<int> BroadcastAsync(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var enviados = 0;

        foreach (var (id, socket) in _connections)
        {
            if (socket.State != WebSocketState.Open)
            {
                Remove(id);
                continue;
            }

            try
            {
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                enviados++;
            }
            catch (Exception)
            {
                Remove(id);
            }
        }

        return enviados;
    }
}
