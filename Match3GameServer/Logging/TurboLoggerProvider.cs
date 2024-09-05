namespace Match3GameServer.Logging;

public class TurboLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TurboLogger(categoryName);
    }

    public void Dispose()
    {
        
    }
}