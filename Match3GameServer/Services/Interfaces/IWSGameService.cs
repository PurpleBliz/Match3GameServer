namespace Match3GameServer.Services.Interfaces;

public interface IWebsocketService
{
    bool IsStarted { get; protected set; }

    Task HandleWebSocketAsync(HttpContext context);
}