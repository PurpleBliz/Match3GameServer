namespace Match3GameServer.Logging;

public class LoggerOptions
{
    public static bool INFO { get; set; }
    public static bool DEBUG { get; set; }
    public static bool ERROR { get; set; }
    public static bool WARN { get; set; }
    public static bool CRITICAL { get; set; }
    public static bool ENCODE_PAYLOAD { get; set; }
}