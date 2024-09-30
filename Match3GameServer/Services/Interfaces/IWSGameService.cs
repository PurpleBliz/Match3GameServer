namespace Match3GameServer.Services.Interfaces;

public interface IWSGameService
{
    bool IsStarted { get; protected set; }

    Task HandleWebSocketAsync(HttpContext context);
}