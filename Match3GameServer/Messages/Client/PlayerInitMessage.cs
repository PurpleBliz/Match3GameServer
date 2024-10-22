using Match3GameServer.Messages.Base;
using Newtonsoft.Json;

namespace Match3GameServer.Messages.Client;

[Serializable]
public class PlayerInitMessage : WebsocketMessage
{
    [JsonProperty("PlayerId")]
    public Guid PlayerId;
    [JsonProperty("SId")]
    public Guid SId;
}