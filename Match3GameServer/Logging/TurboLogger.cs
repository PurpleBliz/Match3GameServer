namespace Match3GameServer.Logging;

public class TurboLogger : ILogger
{
    private readonly string _categoryName;

    public TurboLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (!GetLoggingAvailability(logLevel))
        {
            return;
        }

        string timestamp = $"[{DateTime.Now:dd.MM.yy HH:mm:ss}] ";
        string level = $"[{logLevel}]: ".ToUpper();
        string category = $"[{_categoryName}] ";
        string message = $"{formatter(state, exception!)}";

        ConsoleColor originalColor = Console.ForegroundColor;
        
        Console.Write(timestamp);

        Console.ForegroundColor = GetLogLevelConsoleColor(logLevel);
        Console.Write(level);

        Console.ForegroundColor = originalColor;
        Console.WriteLine(category + message);

        if (exception != null)
        {
            Console.WriteLine($"\r\n[{exception.Source}] {exception.Message}\r\n{exception.StackTrace}");
        }
    }

    private ConsoleColor GetLogLevelConsoleColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Blue,
            LogLevel.Information => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => ConsoleColor.White,
        };
    }
    
    private bool GetLoggingAvailability(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => TagOptions.DEBUG,
            LogLevel.Information => TagOptions.INFO,
            LogLevel.Warning => TagOptions.WARN,
            LogLevel.Error => TagOptions.ERROR,
            LogLevel.Critical => TagOptions.CRITICAL,
            _ => true,
        };
    }
}