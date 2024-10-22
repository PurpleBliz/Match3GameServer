using Match3GameServer.Models;

namespace Match3GameServer.Services.Interfaces;

public interface ISessionService
{
    Task AddPlayerAsync(WebSocketClient client);
}