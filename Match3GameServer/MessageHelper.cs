using Match3GameServer.Messages.Client;
using Match3GameServer.Messages.Server;
using Match3GameServer.Services;

namespace Match3GameServer;

public static class MessageHelper
{
    /// <summary>
    /// Registers message types and their corresponding IDs in the MessageHandlers system.
    /// </summary>
    public static void RegisterMessages()
    {
        //Client messages
        MessageHandlers.RegisterMessage<PlayerInitMessage>(1000);
        MessageHandlers.RegisterMessage<SwapTileMessage>(1001);

        //Server messages
        MessageHandlers.RegisterMessage<BoardLayoutMessage>(3000);
        MessageHandlers.RegisterMessage<InitMessage>(3001);
    }
}