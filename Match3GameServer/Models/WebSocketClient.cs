using System.Net.WebSockets;

namespace Match3GameServer.Models;

public class WebSocketClient
{
    public WebSocket Connection { get; private set; }
    public int InternalId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid SId { get; private set; }

    public WebSocketClient
    (
        WebSocket connection,
        int internalId
    )
    {
        Connection = connection;
        InternalId = internalId;
    }

    public void Verification(Guid playerId, Guid sid)
    {
        PlayerId = playerId;
        SId = sid;
    }
}