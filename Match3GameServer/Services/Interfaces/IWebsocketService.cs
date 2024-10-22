using Match3GameServer.Messages.Base;
using Match3GameServer.Models;

namespace Match3GameServer.Services.Interfaces;

public interface IWebsocketService
{
    bool IsStarted { get; protected set; }

    event Action<WebSocketClient> OnClientConnected;
    event Action<WebSocketClient> OnClientDisconnected;
    event Action<WebSocketClient> OnClientVerifered;

    Task HandleWebSocketAsync(HttpContext context);

    Task CloseServer();
}