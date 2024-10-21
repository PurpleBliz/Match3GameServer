using Newtonsoft.Json;

namespace Match3GameServer.Messages.Base;

[Serializable]
public class BaseMessage
{
    [JsonProperty("MessageId")]
    public int MessageId { get; set; }
}