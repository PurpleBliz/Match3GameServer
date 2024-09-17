using System.Net.WebSockets;

namespace Match3GameServer.Services.Interfaces;

public interface ISessionService
{
    Task AddPlayerAsync(WebSocket webSocket, int playerId);
}