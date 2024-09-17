namespace Match3GameServer.Services.Interfaces;

public interface IGameService
{
    bool IsStarted { get; protected set; }

    Task HandleWebSocketAsync(HttpContext context);
}