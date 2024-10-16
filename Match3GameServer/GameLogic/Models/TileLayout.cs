using Newtonsoft.Json;

namespace Match3GameServer.GameLogic.Models;

[Serializable]
public class TileLayout
{
    public int BoardXPosition { get; private set; }
    public int BoardYPosition { get; private set; }

    public int ItemId { get; private set; }//TODO: Swap to GUID

    [JsonConstructor]
    public TileLayout(int boardXPosition, int boardYPosition, int itemId)
    {
        BoardXPosition = boardXPosition;
        BoardYPosition = boardYPosition;
        ItemId = itemId;
    }

    public void SetNewPosition(int x, int y)
    {
        BoardXPosition = x;
        BoardYPosition = y;
    }
    
    public void SetNewId(int id)
    {
        ItemId = id;
    }
}