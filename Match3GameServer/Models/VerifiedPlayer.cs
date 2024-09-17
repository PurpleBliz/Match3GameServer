using System.Net.Sockets;

namespace Match3GameServer.Models;

public sealed class VerifiedPlayer
{
    public readonly Guid SID;
    public readonly Guid PlayerId;
    public readonly TcpClient Client;

    public VerifiedPlayer(Guid sid, Guid playerId, TcpClient client)
    {
        SID = sid;
        PlayerId = playerId;
        Client = client;
    }
}