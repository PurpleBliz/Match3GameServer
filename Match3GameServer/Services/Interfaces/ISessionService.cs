using System.Net.Sockets;

namespace Match3GameServer.Services.Interfaces;

public interface ISessionService
{
    void AddPlayer(TcpClient client, int playerId);
}