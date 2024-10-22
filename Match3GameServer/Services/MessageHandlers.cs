using System.Net.WebSockets;
using System.Text;
using Match3GameServer.Logging;
using Match3GameServer.Messages.Base;
using Match3GameServer.Models;

namespace Match3GameServer.Services;

public static class MessageHandlers
{
    private static readonly Dictionary<Type, List<Action<WebSocketClient, WebsocketMessage>>> Handlers = new();
    private static readonly Dictionary<int, Type> Messages = new();

    private static ILogger? _logger;

    public static void Init(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a message handler for a specific message type.
    /// </summary>
    /// <param name="action">Method to execute when the message is received.</param>
    /// <typeparam name="T">The WebsocketMessage type that the handler will process.</typeparam>
    public static void RegisterHandler<T>(Action<WebSocketClient, T> action) where T : WebsocketMessage
    {
        var type = typeof(T);

        if (!Handlers.ContainsKey(type))
        {
            Handlers[type] = new();
        }

        Action<WebSocketClient, WebsocketMessage> wrappedAction = (connection, msg) => action(connection, (T)msg);

        Handlers[type].Add(wrappedAction);
    }

    /// <summary>
    /// Registers the message type by associating it with an integer identifier.
    /// </summary>
    /// <param name="messageId">An integer representing the message type.</param>
    /// <typeparam name="T">The WebsocketMessage type being registered.</typeparam>
    public static void RegisterMessage<T>(int messageId)
    {
        var type = typeof(T);

        var result = Messages.TryAdd(messageId, type);

        if (!result)
        {
            _logger?.LogWarning(
                $"There was an error when registering the type, perhaps such a message already exists: {messageId}");
        }
    }

    /// <summary>
    /// Retrieves the response ID associated with a given type.
    /// </summary>
    /// <typeparam name="T">The type for which the response ID is needed.</typeparam>
    /// <returns>A tuple containing a success flag and the associated response ID if found, or 0 if not.</returns>
    public static (bool Success, int ResponseId) GetIdByType<T>()
    {
        var type = typeof(T);
        
        foreach (var kvp in Messages)
        {
            if (kvp.Value == type)
            {
                return (true, kvp.Key); // Возвращаем идентификатор и успех
            }
        }
        
        _logger?.LogError($"Response type not found in the registry for type: {type.Name}");
        
        return (false, 0);
    }

    /// <summary>
    /// Invokes the appropriate handler based on the received message type.
    /// </summary>
    /// <param name="client">The WebSocket client that sent the message.</param>
    /// <param name="jsonMessage">The raw JSON message received from the client.</param>
    public static void InvokeHandler(WebSocketClient client, string jsonMessage)
    {
        var baseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<WebsocketMessage>(jsonMessage);

        if (baseMessage == null || !Messages.TryGetValue(baseMessage.MessageId, out var type))
        {
            _logger?.LogError($"Invalid or unrecognized message type: {baseMessage?.MessageId}");

            return;
        }

        var message = (WebsocketMessage?)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonMessage, type);

        if (message == null || !Handlers.TryGetValue(type, out var handlers))
        {
            _logger?.LogError("Message is null or no handlers found for this message type.");

            return;
        }

        foreach (var handler in handlers)
        {
            handler.Invoke(client, message);
        }
    }

    /// <summary>
    /// Sends a message to the specified WebSocket client after assigning the appropriate message ID.
    /// This method serializes the message to JSON and sends it over the WebSocket connection.
    /// </summary>
    /// <param name="client">The WebSocket client to which the message will be sent.</param>
    /// <param name="message">The message to send, which must inherit from <see cref="WebsocketMessage"/>.</param>
    /// <typeparam name="T">The type of the message, which must inherit from <see cref="WebsocketMessage"/>.</typeparam>
    public static async Task SendMessage<T>(this WebSocketClient client, T message) where T : WebsocketMessage
    {
        var (success, messageId) = MessageHandlers.GetIdByType<T>();

        if (!success)
        {
            _logger?.LogError($"Message ID not found for type {typeof(T).Name}");

            return;
        }

        message.MessageId = messageId;

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        var responseBuffer = System.Text.Encoding.UTF8.GetBytes(json);
        
        var compressedMessage = json;

        if (LoggerOptions.ENCODE_PAYLOAD)
        {
            compressedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        _logger?.LogInformation($"Sending message to player. Payload: {compressedMessage}");

        await client.Connection.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}