using Match3GameServer.Logging;
using Match3GameServer.Messages;
using Match3GameServer.Messages.Responses;
using Match3GameServer.Services;
using Match3GameServer.Services.Implementation;
using Match3GameServer.Services.Interfaces;

using LoggingOptions = Match3GameServer.Logging.TagOptions;

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
        serviceCollection.AddSingleton<MessageHandlers>();

        serviceCollection.AddHostedService<RoomService>();
    }

    /// <summary>
    /// Initializes logging settings by loading tag options from the configuration manager.
    /// </summary>
    /// <param name="manager">Configuration manager to access app settings.</param>
    public static void InitLogging(this ConfigurationManager manager)
    {
        var loggingTagsSection = manager.GetSection("Logging:Tags");
        LoggingOptions.INFO = loggingTagsSection.GetValue<bool>("INFO");
        LoggingOptions.DEBUG = loggingTagsSection.GetValue<bool>("DEBUG");
        LoggingOptions.ERROR = loggingTagsSection.GetValue<bool>("ERROR");
        LoggingOptions.WARN = loggingTagsSection.GetValue<bool>("WARN");
        LoggingOptions.CRITICAL = loggingTagsSection.GetValue<bool>("CRITICAL");
    }

    /// <summary>
    /// Registers message types and associates them with specific message codes.
    /// </summary>
    /// <param name="serviceCollection">Service provider to access registered services.</param>
    public static void RegisterMessages(this IServiceProvider serviceCollection)
    {
        var messageHandlers = serviceCollection.GetService<MessageHandlers>();

        if (messageHandlers != null)
        {
            messageHandlers.RegisterMessage<PlayerInitMessage>(1001);
            
            messageHandlers.RegisterResponse<InitResponse>(1001);
        }
    }
}
