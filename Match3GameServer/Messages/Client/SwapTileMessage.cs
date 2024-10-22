using Match3GameServer.Messages.Base;

namespace Match3GameServer.Messages.Client;

public class SwapTileMessage : WebsocketMessage
{
    public int FromXPosition { get; set; }
    public int FromYPosition { get; set; }

    public int ToXPosition { get; set; }
    public int ToYPosition { get; set; }

    public SwapTileMessage
    (
        int fromX,
        int fromY,
        int toX,
        int toY
    )
    {
        FromXPosition = fromX;
        FromYPosition = fromY;
        ToXPosition = toX;
        ToYPosition = toY;
    }
}