using Match3GameServer.Logging;
using Match3GameServer.Messages;
using Match3GameServer.Messages.Responses;
using Match3GameServer.Services;
using Match3GameServer.Services.Implementation;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer;

public static class ServicesExtensions
{
    /// <summary>
    /// Registers necessary services for the application, including gRPC, logging, and custom services.
    /// </summary>
    /// <param name="serviceCollection">The IServiceCollection to which the services will be added.</param>
    public static void AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpc();

        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new TurboLoggerProvider());
        });

        serviceCollection.AddSingleton<ISessionService, SessionService>();
        serviceCollection.AddSingleton<IWebsocketService, WebSocketService>();

        serviceCollection.AddHostedService<RoomService>();
    }

    /// <summary>
    /// Initializes logging settings by loading tag options from the configuration manager.
    /// </summary>
    /// <param name="manager">Configuration manager to access app settings.</param>
    public static void InitLogging(this ConfigurationManager manager)
    {
        var loggingTagsSection = manager.GetSection("Logging:Tags");
        LoggerOptions.INFO = loggingTagsSection.GetValue<bool>("INFO");
        LoggerOptions.DEBUG = loggingTagsSection.GetValue<bool>("DEBUG");
        LoggerOptions.ERROR = loggingTagsSection.GetValue<bool>("ERROR");
        LoggerOptions.WARN = loggingTagsSection.GetValue<bool>("WARN");
        LoggerOptions.CRITICAL = loggingTagsSection.GetValue<bool>("CRITICAL");
        LoggerOptions.ENCODE_PAYLOAD = loggingTagsSection.GetValue<bool>("ENCODE_PAYLOAD");
    }

    /// <summary>
    /// Initializes the MessageHandlers with a logger instance created from the provided ILoggerFactory.
    /// Throws InvalidOperationException if the logger service is not registered.
    /// </summary>
    /// <param name="loggerFactory">Factory used to create logger instances.</param>
    /// <exception cref="InvalidOperationException">Thrown if the logger service is not registered.</exception>
    public static void InitMessageHandlers(this ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(MessageHandlers));

        if (logger == null)
        {
            throw new InvalidOperationException("Logger service is not registered.");
        }

        MessageHandlers.Init(logger);
    }

    /// <summary>
    /// Registers message types and their corresponding IDs in the MessageHandlers system.
    /// </summary>
    public static void RegisterMessages()
    {
        MessageHandlers.RegisterMessage<PlayerInitMessage>(1001);
        MessageHandlers.RegisterMessage<SwapTileMessage>(1002);

        MessageHandlers.RegisterResponse<InitResponse>(1001);
        MessageHandlers.RegisterResponse<BoardLayoutResponse>(1002);
    }
}
