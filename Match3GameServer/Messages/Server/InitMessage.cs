using Match3GameServer.Messages.Base;

namespace Match3GameServer.Messages.Server;

public class InitMessage : WebsocketMessage
{
    public string Text;
}