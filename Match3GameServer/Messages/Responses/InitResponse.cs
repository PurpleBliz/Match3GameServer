using Match3GameServer.Messages.Base;

namespace Match3GameServer.Messages.Responses;

public class InitResponse : WebSocketResponse
{
    public string Text;
}