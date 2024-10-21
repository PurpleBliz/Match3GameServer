using Newtonsoft.Json;

namespace Match3GameServer.Messages.Base;

[Serializable]
public class WebSocketResponse
{
    [JsonProperty("MessageId")]
    public int MessageId { get; set; }
}