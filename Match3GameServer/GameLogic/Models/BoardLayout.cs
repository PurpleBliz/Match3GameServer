using Newtonsoft.Json;

namespace Match3GameServer.GameLogic.Models;

[Serializable]
public class BoardLayout
{
    [JsonProperty]
    public TileLayout[,] TileLayouts { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public BoardLayout(int width, int height)
    {
        Width = width;
        Height = height;

        TileLayouts = new TileLayout[Width, Height];
    }
}