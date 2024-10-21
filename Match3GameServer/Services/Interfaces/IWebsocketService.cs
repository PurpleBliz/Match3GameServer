using Match3GameServer.Messages.Base;
using Match3GameServer.Models;

namespace Match3GameServer.Services.Interfaces;

public interface IWebsocketService
{
    bool IsStarted { get; protected set; }

    event Action<WebSocketClient> OnClientConnected;
    event Action<WebSocketClient> OnClientDisconnected;

    Task HandleWebSocketAsync(HttpContext context);

    Task SendToPlayer<T>(WebSocketClient client, T message) where T : WebSocketResponse;
}