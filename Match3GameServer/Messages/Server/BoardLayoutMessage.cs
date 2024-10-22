using Match3GameServer.GameLogic.Models;
using Match3GameServer.Messages.Base;
using Newtonsoft.Json;

namespace Match3GameServer.Messages.Server;

public class BoardLayoutMessage : WebsocketMessage
{
    [JsonProperty]
    public TileLayout[,] TileLayouts { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
}