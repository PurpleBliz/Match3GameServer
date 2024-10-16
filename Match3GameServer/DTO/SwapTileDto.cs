namespace Match3GameServer.DTO;

public class SwapTileDto
{
    public int FromXPosition { get; set; }
    public int FromYPosition { get; set; }

    public int ToXPosition { get; set; }
    public int ToYPosition { get; set; }

    public SwapTileDto
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